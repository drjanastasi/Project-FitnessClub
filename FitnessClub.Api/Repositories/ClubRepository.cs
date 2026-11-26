using FitnessClub.Api.Data;
using FitnessClub.Api.DTOs.Clubs;
using FitnessClub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Api.Repositories
{
    public interface IClubRepository
    {
        Task<List<Club>> GetAllAsync(ClubQueryParameters query);
        Task<Club?> GetByIdAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> NameExistsAsync(string name, int? exceptId = null);
        Task<bool> HasWorkoutsAsync(int id);
        Task AddAsync(Club club);
        Task SaveChangesAsync();
        Task DeleteAsync(Club club);
    }

    /// <summary>
    /// Робота з клубами (філіями) через Entity Framework Core.
    /// </summary>
    public class ClubRepository : IClubRepository
    {
        private readonly FitnessClubDbContext _db;

        public ClubRepository(FitnessClubDbContext db)
        {
            _db = db;
        }

        public async Task<List<Club>> GetAllAsync(ClubQueryParameters q)
        {
            IQueryable<Club> query = _db.Clubs.AsNoTracking();

            // ----- Пошук -----
            if (!string.IsNullOrWhiteSpace(q.Search))
            {
                var search = q.Search.Trim();
                query = query.Where(c =>
                    c.Name.Contains(search) ||
                    c.Address.Contains(search) ||
                    (c.Description != null && c.Description.Contains(search)));
            }

            // ----- Сортування -----
            query = q.SortBy?.ToLowerInvariant() switch
            {
                "name" => q.Desc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                "address" => q.Desc ? query.OrderByDescending(c => c.Address) : query.OrderBy(c => c.Address),
                _ => query.OrderBy(c => c.Id)
            };

            return await query.ToListAsync();
        }

        /// <summary>Клуб для зміни (EF відстежує його, SaveChangesAsync збереже зміни).</summary>
        public Task<Club?> GetByIdAsync(int id) =>
            _db.Clubs.FirstOrDefaultAsync(c => c.Id == id);

        public Task<bool> ExistsAsync(int id) =>
            _db.Clubs.AnyAsync(c => c.Id == id);

        public Task<bool> NameExistsAsync(string name, int? exceptId = null)
        {
            var trimmed = name.Trim();
            return _db.Clubs.AnyAsync(c => c.Name == trimmed && c.Id != exceptId);
        }

        public Task<bool> HasWorkoutsAsync(int id) =>
                _db.Workouts.AnyAsync(w => w.ClubId == id);

        public async Task AddAsync(Club club)
        {
            _db.Clubs.Add(club);
            await _db.SaveChangesAsync();
        }

        public Task SaveChangesAsync() => _db.SaveChangesAsync();

        public async Task DeleteAsync(Club club)
        {
            _db.Clubs.Remove(club);
            await _db.SaveChangesAsync();
        }
    }
}
