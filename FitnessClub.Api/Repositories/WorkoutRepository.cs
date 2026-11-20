using FitnessClub.Api.Data;
using FitnessClub.Api.DTOs.Workouts;
using FitnessClub.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace FitnessClub.Api.Repositories;

public interface IWorkoutRepository
{
    Task<List<Workout>> GetAllAsync(WorkoutQueryParameters query);
    Task<Workout?> GetByIdAsync(int id);
    Task<Workout?> GetWithMembershipsAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<List<Workout>> GetTrainerWorkoutsOnDayAsync(int trainerId, DayOfWeek day, int? exceptWorkoutId = null);
    Task AddAsync(Workout workout);
    Task SaveChangesAsync();
    Task DeleteAsync(Workout workout);
}
public class WorkoutRepository : IWorkoutRepository
{
    private readonly FitnessClubDbContext _db;

    public WorkoutRepository(FitnessClubDbContext db)
    {
        _db = db;
    }
    public async Task<List<Workout>> GetAllAsync(WorkoutQueryParameters q)
    {
        IQueryable<Workout> query = _db.Workouts.AsNoTracking();

        if (q.Day is DayOfWeek day)
            query = query.Where(w => w.DayOfWeek == day);

        if (q.Level is DifficultyLevel level)
            query = query.Where(w => w.Level == level);

        if (q.TrainerId is int trainerId)
            query = query.Where(w => w.TrainerId == trainerId);

        if (q.MembershipId is int membershipId)
            query = query.Where(w => w.MembershipWorkouts.Any(mw => mw.MembershipId == membershipId));

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var search = q.Search.Trim();
            query = query.Where(w =>
                w.Name.Contains(search) ||
                (w.Description != null && w.Description.Contains(search)));
        }
        query = q.SortBy?.ToLowerInvariant() switch
        {
            "name" => q.Desc ? query.OrderByDescending(w => w.Name) : query.OrderBy(w => w.Name),
            "duration" => q.Desc ? query.OrderByDescending(w => w.DurationMinutes) : query.OrderBy(w => w.DurationMinutes),
            _ => q.Desc
                ? query.OrderByDescending(w => ((int)w.DayOfWeek + 6) % 7).ThenByDescending(w => w.StartTime)
                : query.OrderBy(w => ((int)w.DayOfWeek + 6) % 7).ThenBy(w => w.StartTime)
        };
        return await query.ToListAsync();
    }
    public Task<Workout?> GetByIdAsync(int id) =>
        _db.Workouts.FirstOrDefaultAsync(w => w.Id == id);
    public Task<Workout?> GetWithMembershipsAsync(int id) =>
        _db.Workouts
            .AsNoTracking()
            .Include(w => w.MembershipWorkouts)
                .ThenInclude(mw => mw.Membership)
            .FirstOrDefaultAsync(w => w.Id == id);
    public Task<bool> ExistsAsync(int id) =>
        _db.Workouts.AnyAsync(w => w.Id == id);
    public Task<List<Workout>> GetTrainerWorkoutsOnDayAsync(int trainerId, DayOfWeek day, int? exceptWorkoutId = null) =>
        _db.Workouts
            .AsNoTracking()
            .Where(w => w.TrainerId == trainerId && w.DayOfWeek == day && w.Id != exceptWorkoutId)
            .ToListAsync();
    public async Task AddAsync(Workout workout)
    {
        _db.Workouts.Add(workout);
        await _db.SaveChangesAsync();
    }
    public Task SaveChangesAsync() => _db.SaveChangesAsync();
    public async Task DeleteAsync(Workout workout)
    {
        _db.Workouts.Remove(workout);
        await _db.SaveChangesAsync();
    }
}
