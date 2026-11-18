using FitnessClub.Api.Data;
using FitnessClub.Api.DTOs.Memberships;
using FitnessClub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Api.Repositories;

public interface IMembershipRepository
{
    Task<List<Membership>> GetAllAsync(MembershipQueryParameters query);
    Task<Membership?> GetByIdAsync(int id);
    Task<Membership?> GetWithWorkoutsAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> NameExistsAsync(string name, int? exceptId = null);
    Task AddAsync(Membership membership);
    Task SaveChangesAsync();
    Task DeleteAsync(Membership membership);

    Task<bool> HasWorkoutAsync(int membershipId, int workoutId);
    Task AddWorkoutAsync(int membershipId, int workoutId);
    Task<bool> RemoveWorkoutAsync(int membershipId, int workoutId);
}

/// <summary>
/// Робота з абонементами через Entity Framework Core.
/// Усі LINQ-запити перетворюються EF Core на SQL і виконуються в базі.
/// </summary>
public class MembershipRepository : IMembershipRepository
{
    private readonly FitnessClubDbContext _db;

    public MembershipRepository(FitnessClubDbContext db)
    {
        _db = db;
    }

    public async Task<List<Membership>> GetAllAsync(MembershipQueryParameters q)
    {
        // AsNoTracking — лише читаємо, EF не потрібно відстежувати зміни.
        // Include — підтягуємо зв'язки, щоб порахувати кількість тренувань.
        IQueryable<Membership> query = _db.Memberships
            .AsNoTracking()
            .Include(m => m.MembershipWorkouts);

        // ----- Фільтрація -----
        if (q.IsActive is bool isActive)
            query = query.Where(m => m.IsActive == isActive);

        if (q.MinPrice is decimal minPrice)
            query = query.Where(m => m.Price >= minPrice);

        if (q.MaxPrice is decimal maxPrice)
            query = query.Where(m => m.Price <= maxPrice);

        if (q.WorkoutId is int workoutId)
            query = query.Where(m => m.MembershipWorkouts.Any(mw => mw.WorkoutId == workoutId));

        // ----- Пошук -----
        // Увага: у SQLite пошук чутливий до регістру для кирилиці ("Йога" ≠ "йога").
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var search = q.Search.Trim();
            query = query.Where(m =>
                m.Name.Contains(search) ||
                (m.Description != null && m.Description.Contains(search)));
        }

        // ----- Сортування -----
        query = q.SortBy?.ToLowerInvariant() switch
        {
            "name" => q.Desc ? query.OrderByDescending(m => m.Name) : query.OrderBy(m => m.Name),
            "price" => q.Desc ? query.OrderByDescending(m => m.Price) : query.OrderBy(m => m.Price),
            "duration" => q.Desc ? query.OrderByDescending(m => m.DurationDays) : query.OrderBy(m => m.DurationDays),
            _ => query.OrderBy(m => m.Id)
        };

        return await query.ToListAsync();
    }

    /// <summary>Абонемент для зміни (EF відстежує його, SaveChangesAsync збереже зміни).</summary>
    public Task<Membership?> GetByIdAsync(int id) =>
        _db.Memberships
            .Include(m => m.MembershipWorkouts)
            .FirstOrDefaultAsync(m => m.Id == id);

    /// <summary>Абонемент разом із тренуваннями (отримання пов'язаних даних).</summary>
    public Task<Membership?> GetWithWorkoutsAsync(int id) =>
        _db.Memberships
            .AsNoTracking()
            .Include(m => m.MembershipWorkouts)
                .ThenInclude(mw => mw.Workout)
            .FirstOrDefaultAsync(m => m.Id == id);

    public Task<bool> ExistsAsync(int id) =>
        _db.Memberships.AnyAsync(m => m.Id == id);

    public Task<bool> NameExistsAsync(string name, int? exceptId = null)
    {
        var trimmed = name.Trim();
        return _db.Memberships.AnyAsync(m => m.Name == trimmed && m.Id != exceptId);
    }

    public async Task AddAsync(Membership membership)
    {
        _db.Memberships.Add(membership);
        await _db.SaveChangesAsync();
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();

    public async Task DeleteAsync(Membership membership)
    {
        // Зв'язки в MembershipWorkouts видаляться каскадно.
        _db.Memberships.Remove(membership);
        await _db.SaveChangesAsync();
    }

    public Task<bool> HasWorkoutAsync(int membershipId, int workoutId) =>
        _db.MembershipWorkouts.AnyAsync(mw => mw.MembershipId == membershipId && mw.WorkoutId == workoutId);

    public async Task AddWorkoutAsync(int membershipId, int workoutId)
    {
        _db.MembershipWorkouts.Add(new MembershipWorkout { MembershipId = membershipId, WorkoutId = workoutId });
        await _db.SaveChangesAsync();
    }

    public async Task<bool> RemoveWorkoutAsync(int membershipId, int workoutId)
    {
        var deleted = await _db.MembershipWorkouts
            .Where(mw => mw.MembershipId == membershipId && mw.WorkoutId == workoutId)
            .ExecuteDeleteAsync();
        return deleted > 0;
    }
}
