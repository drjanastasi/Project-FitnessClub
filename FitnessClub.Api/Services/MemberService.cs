using FitnessClub.Api.DTOs.Members;
using FitnessClub.Api.Exceptions;
using FitnessClub.Api.Mapping;
using FitnessClub.Api.Repositories;

namespace FitnessClub.Api.Services
{
    public interface IMemberService
    {
        Task<IReadOnlyList<MemberDto>> GetAllAsync(bool? isActive, string? search);
        Task<MemberDto> GetByIdAsync(int id);
        Task<MemberDto> CreateAsync(CreateMemberDto dto);
        Task<MemberDto> UpdateAsync(int id, UpdateMemberDto dto);
        Task DeleteAsync(int id);
    }

    public class MemberService : IMemberService
    {
        private readonly IMemberRepository _members;
        private readonly IMembershipRepository _memberships;
        private readonly IBookingRepository _bookings;

        public MemberService(
            IMemberRepository members,
            IMembershipRepository memberships,
            IBookingRepository bookings)
        {
            _members = members;
            _memberships = memberships;
            _bookings = bookings;
        }

        public async Task<IReadOnlyList<MemberDto>> GetAllAsync(bool? isActive, string? search)
        {
            var items = await _members.GetAllAsync(isActive, search);
            return items.Select(m => m.ToDto()).ToList();
        }

        public async Task<MemberDto> GetByIdAsync(int id)
        {
            var member = await _members.GetByIdAsync(id)
                ?? throw NotFound(id);
            return member.ToDto();
        }

        public async Task<MemberDto> CreateAsync(CreateMemberDto dto)
        {
            var email = NormalizeEmail(dto.Email);
            await EnsureEmailIsUniqueAsync(email);
            await EnsureMembershipExistsAsync(dto.MembershipId);

            var member = dto.ToModel();
            member.Email = email;
            await _members.AddAsync(member);
            return member.ToDto();
        }

        public async Task<MemberDto> UpdateAsync(int id, UpdateMemberDto dto)
        {
            var member = await _members.GetByIdAsync(id)
                ?? throw NotFound(id);

            var email = NormalizeEmail(dto.Email);
            await EnsureEmailIsUniqueAsync(email, exceptId: id);
            await EnsureMembershipExistsAsync(dto.MembershipId);

            member.FirstName = dto.FirstName.Trim();
            member.LastName = dto.LastName.Trim();
            member.Email = email;
            member.Phone = dto.Phone.Trim();
            member.BirthDate = dto.BirthDate!.Value;
            member.IsActive = dto.IsActive;
            member.MembershipId = dto.MembershipId;

            await _members.SaveChangesAsync();
            return member.ToDto();
        }

        public async Task DeleteAsync(int id)
        {
            var member = await _members.GetByIdAsync(id)
                ?? throw NotFound(id);

            if (await _bookings.HasBookingsForMemberAsync(id))
                throw new ConflictException(
                    "Клієнта неможливо видалити: у нього є записи на тренування. Спочатку видаліть записи або деактивуйте клієнта.");

            await _members.DeleteAsync(member);
        }

        // ---------- допоміжні методи ----------

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

        private async Task EnsureEmailIsUniqueAsync(string email, int? exceptId = null)
        {
            if (await _members.EmailExistsAsync(email, exceptId))
                throw new ConflictException("Клієнт з таким Email вже існує.");
        }

        private async Task EnsureMembershipExistsAsync(int membershipId)
        {
            if (!await _memberships.ExistsAsync(membershipId))
                throw new BadRequestException("Вказаний абонемент не знайдено.");
        }

        private static NotFoundException NotFound(int id) =>
            new($"Клієнта з id={id} не знайдено.");
    }
}
