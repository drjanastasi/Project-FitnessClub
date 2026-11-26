using FitnessClub.Api.Data;
using FitnessClub.Api.DTOs.Bookings;
using FitnessClub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Api.Repositories
{
    public interface IBookingRepository
    {
        Task<List<Booking>> GetAllAsync(BookingQueryParameters query);
        Task<Booking?> GetByIdAsync(int id);
        Task<int> CountConfirmedAsync(int workoutId, DateOnly date);
        Task<bool> ActiveBookingExistsAsync(int memberId, int workoutId, DateOnly date);
        Task AddAsync(Booking booking);
        Task SaveChangesAsync();
        Task DeleteAsync(Booking booking);
    }

    /// <summary>
    /// Робота із записами на тренування через Entity Framework Core.
    /// </summary>
    public class BookingRepository : IBookingRepository
    {
        private readonly FitnessClubDbContext _db;

        public BookingRepository(FitnessClubDbContext db)
        {
            _db = db;
        }

        public async Task<List<Booking>> GetAllAsync(BookingQueryParameters q)
        {
            // Include потрібен, бо BookingDto містить назву й розклад тренування.
            IQueryable<Booking> query = _db.Bookings
                .AsNoTracking()
                .Include(b => b.Workout);

            // ----- Фільтрація -----
            if (q.MemberId.HasValue)
                query = query.Where(b => b.MemberId == q.MemberId.Value);

            if (q.WorkoutId.HasValue)
                query = query.Where(b => b.WorkoutId == q.WorkoutId.Value);

            if (q.ClubId.HasValue)
                query = query.Where(b => b.Workout.ClubId == q.ClubId.Value);

            if (q.Status.HasValue)
                query = query.Where(b => b.Status == q.Status.Value);

            if (q.DateFrom.HasValue)
                query = query.Where(b => b.Date >= q.DateFrom.Value);

            if (q.DateTo.HasValue)
                query = query.Where(b => b.Date <= q.DateTo.Value);

            // ----- Сортування -----
            query = q.SortBy?.ToLowerInvariant() switch
            {
                "date" => q.Desc
                    ? query.OrderByDescending(b => b.Date).ThenByDescending(b => b.Id)
                    : query.OrderBy(b => b.Date).ThenBy(b => b.Id),
                "createdat" => q.Desc
                    ? query.OrderByDescending(b => b.CreatedAt)
                    : query.OrderBy(b => b.CreatedAt),
                _ => query.OrderBy(b => b.Id)
            };

            return await query.ToListAsync();
        }

        /// <summary>Запис разом із тренуванням (EF відстежує його, SaveChangesAsync збереже зміни).</summary>
        public Task<Booking?> GetByIdAsync(int id) =>
            _db.Bookings
                .Include(b => b.Workout)
                .FirstOrDefaultAsync(b => b.Id == id);

        /// <summary>Скільки місць уже зайнято (лише підтверджені записи).</summary>
        public Task<int> CountConfirmedAsync(int workoutId, DateOnly date) =>
            _db.Bookings.CountAsync(b =>
                b.WorkoutId == workoutId &&
                b.Date == date &&
                b.Status == BookingStatus.Confirmed);

        /// <summary>Чи вже є в клієнта підтверджений запис на це тренування в цю дату.</summary>
        public Task<bool> ActiveBookingExistsAsync(int memberId, int workoutId, DateOnly date) =>
            _db.Bookings.AnyAsync(b =>
                b.MemberId == memberId &&
                b.WorkoutId == workoutId &&
                b.Date == date &&
                b.Status == BookingStatus.Confirmed);

        public async Task AddAsync(Booking booking)
        {
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();

        public async Task DeleteAsync(Booking booking)
        {
            _db.Bookings.Remove(booking);
            await _db.SaveChangesAsync();
        }
    }
}
