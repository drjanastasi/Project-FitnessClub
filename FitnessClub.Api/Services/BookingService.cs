using FitnessClub.Api.Data;
using FitnessClub.Api.DTOs.Bookings;
using FitnessClub.Api.Exceptions;
using FitnessClub.Api.Mapping;
using FitnessClub.Api.Models;
using FitnessClub.Api.Repositories;

namespace FitnessClub.Api.Services
{
    public interface IBookingService
    {
        Task<IReadOnlyList<BookingDto>> GetAllAsync(BookingQueryParameters query);
        Task<BookingDto> GetByIdAsync(int id);
        Task<BookingDto> CreateAsync(CreateBookingDto dto);
        Task<BookingDto> CancelAsync(int id);
        Task DeleteAsync(int id);
        Task<WorkoutAvailabilityDto> GetAvailabilityAsync(int workoutId, DateOnly date);
    }

    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookings;
        private readonly IWorkoutRepository _workouts;
        private readonly IMemberRepository _members;
        private readonly IMembershipRepository _memberships;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _clock;

        public BookingService(
        IBookingRepository bookings,
        IWorkoutRepository workouts,
        IMemberRepository members,
        IMembershipRepository memberships,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock)
        {
            _bookings = bookings;
            _workouts = workouts;
            _members = members;
            _memberships = memberships;
            _unitOfWork = unitOfWork;
            _clock = clock;
        }

        public async Task<IReadOnlyList<BookingDto>> GetAllAsync(BookingQueryParameters query)
        {
            var items = await _bookings.GetAllAsync(query);
            return items.Select(b => b.ToDto()).ToList();
        }

        public async Task<BookingDto> GetByIdAsync(int id)
        {
            var booking = await _bookings.GetByIdAsync(id)
                ?? throw NotFound(id);
            return booking.ToDto();
        }

        public async Task<BookingDto> CreateAsync(CreateBookingDto dto)
        {
            var date = dto.Date!.Value;

            var workout = await _workouts.GetByIdAsync(dto.WorkoutId)
                ?? throw new BadRequestException($"Тренування з id={dto.WorkoutId} не існує.");

            EnsureDateIsValid(date, workout);

            var member = _members.GetById(dto.MemberId)
                ?? throw new BadRequestException($"Клієнта з id={dto.MemberId} не існує.");

            if (!member.IsActive)
                throw new BadRequestException("Клієнт неактивний, запис неможливий.");

            await EnsureMembershipAllowsWorkoutAsync(member, workout);

            // Перевірки місць і дубля та сам запис виконуються в одній транзакції.
            var bookingId = await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (await _bookings.ActiveBookingExistsAsync(member.Id, workout.Id, date))
                    throw new ConflictException(
                        $"Клієнт уже записаний на \"{workout.Name}\" на {date:yyyy-MM-dd}.");

                var booked = await _bookings.CountConfirmedAsync(workout.Id, date);
                if (booked >= workout.MaxParticipants)
                    throw new ConflictException(
                        $"На \"{workout.Name}\" {date:yyyy-MM-dd} немає вільних місць ({workout.MaxParticipants} з {workout.MaxParticipants} зайнято).");

                var booking = dto.ToEntity(_clock.UtcNow);
                await _bookings.AddAsync(booking);
                return booking.Id;
            });

            // Перечитуємо запис разом із тренуванням, щоб заповнити DTO.
            var created = await _bookings.GetByIdAsync(bookingId)
                ?? throw NotFound(bookingId);
            return created.ToDto();
        }

        public async Task<BookingDto> CancelAsync(int id)
        {
            var booking = await _bookings.GetByIdAsync(id)
                ?? throw NotFound(id);

            if (booking.Status == BookingStatus.Cancelled)
                throw new ConflictException("Запис уже скасовано.");

            if (booking.Date < Today())
                throw new BadRequestException("Заняття вже минуло, скасувати запис неможливо.");

            booking.Status = BookingStatus.Cancelled;
            await _bookings.SaveChangesAsync();
            return booking.ToDto();
        }

        public async Task DeleteAsync(int id)
        {
            var booking = await _bookings.GetByIdAsync(id)
                ?? throw NotFound(id);
            await _bookings.DeleteAsync(booking);
        }

        public async Task<WorkoutAvailabilityDto> GetAvailabilityAsync(int workoutId, DateOnly date)
        {
            var workout = await _workouts.GetByIdAsync(workoutId)
                ?? throw new NotFoundException($"Тренування з id={workoutId} не знайдено.");

            if (date.DayOfWeek != workout.DayOfWeek)
                throw new BadRequestException(
                    $"Тренування \"{workout.Name}\" проходить у {workout.DayOfWeek}, а {date:yyyy-MM-dd} — це {date.DayOfWeek}.");

            var booked = await _bookings.CountConfirmedAsync(workoutId, date);
            var free = Math.Max(0, workout.MaxParticipants - booked);
            return new WorkoutAvailabilityDto(workoutId, date, workout.MaxParticipants, booked, free);
        }

        // ---------- допоміжні методи ----------

        private DateOnly Today() => DateOnly.FromDateTime(_clock.UtcNow);

        private void EnsureDateIsValid(DateOnly date, Workout workout)
        {
            if (date < Today())
                throw new BadRequestException("Не можна записатися на дату, що вже минула.");

            if (date.DayOfWeek != workout.DayOfWeek)
                throw new BadRequestException(
                    $"Тренування \"{workout.Name}\" проходить у {workout.DayOfWeek}, а {date:yyyy-MM-dd} — це {date.DayOfWeek}.");
        }

        private async Task EnsureMembershipAllowsWorkoutAsync(Member member, Workout workout)
        {
            var membership = await _memberships.GetByIdAsync(member.MembershipId)
                ?? throw new BadRequestException("Абонемент клієнта не знайдено.");

            if (!membership.IsActive)
                throw new BadRequestException($"Абонемент \"{membership.Name}\" неактивний.");

            if (!await _memberships.HasWorkoutAsync(membership.Id, workout.Id))
                throw new BadRequestException(
                    $"Абонемент \"{membership.Name}\" не включає тренування \"{workout.Name}\".");
        }

        private static NotFoundException NotFound(int id) =>
            new($"Запис з id={id} не знайдено.");
    }
}
