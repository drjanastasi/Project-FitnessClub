using FitnessClub.Api.DTOs.Clubs;
using FitnessClub.Api.Exceptions;
using FitnessClub.Api.Mapping;
using FitnessClub.Api.Repositories;

namespace FitnessClub.Api.Services
{
    public interface IClubService
    {
        Task<IReadOnlyList<ClubDto>> GetAllAsync(ClubQueryParameters query);
        Task<ClubDto> GetByIdAsync(int id);
        Task<ClubDto> CreateAsync(CreateClubDto dto);
        Task<ClubDto> UpdateAsync(int id, UpdateClubDto dto);
        Task DeleteAsync(int id);
    }

    public class ClubService : IClubService
    {
        private readonly IClubRepository _clubs;

        public ClubService(IClubRepository clubs)
        {
            _clubs = clubs;
        }

        public async Task<IReadOnlyList<ClubDto>> GetAllAsync(ClubQueryParameters query)
        {
            var clubs = await _clubs.GetAllAsync(query);
            return clubs.Select(c => c.ToDto()).ToList();
        }

        public async Task<ClubDto> GetByIdAsync(int id)
        {
            var club = await _clubs.GetByIdAsync(id)
                ?? throw NotFound(id);
            return club.ToDto();
        }

        public async Task<ClubDto> CreateAsync(CreateClubDto dto)
        {
            await EnsureNameIsUniqueAsync(dto.Name);

            var club = dto.ToEntity();
            await _clubs.AddAsync(club);
            return club.ToDto();
        }

        public async Task<ClubDto> UpdateAsync(int id, UpdateClubDto dto)
        {
            var club = await _clubs.GetByIdAsync(id)
                ?? throw NotFound(id);

            await EnsureNameIsUniqueAsync(dto.Name, exceptId: id);

            dto.ApplyTo(club);
            await _clubs.SaveChangesAsync();
            return club.ToDto();
        }

        public async Task DeleteAsync(int id)
        {
            var club = await _clubs.GetByIdAsync(id)
                ?? throw NotFound(id);

            // TODO: коли в Workout з'явиться ClubId, тут перевіряти,
            // чи є в клубі тренування, і кидати ConflictException (409).
            await _clubs.DeleteAsync(club);
        }

        private async Task EnsureNameIsUniqueAsync(string name, int? exceptId = null)
        {
            if (await _clubs.NameExistsAsync(name, exceptId))
                throw new ConflictException($"Клуб з назвою \"{name.Trim()}\" вже існує.");
        }

        private static NotFoundException NotFound(int id) =>
            new($"Клуб з id={id} не знайдено.");
    }
}
