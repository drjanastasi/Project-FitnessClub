using FitnessClub.Api.Models;

namespace FitnessClub.Api.Repositories;

public interface IWorkoutRepository
{
    IReadOnlyList<Workout> GetAll();
    Workout? GetById(int id);
    Workout Add(Workout workout);
    bool Update(Workout workout);
    bool Delete(int id);
}

/// <summary>
/// Зберігання тренувань у пам'яті (етап 1).
/// </summary>
public class InMemoryWorkoutRepository : IWorkoutRepository
{
    private readonly List<Workout> _items = new();
    private readonly object _lock = new();
    private int _nextId = 1;

    public InMemoryWorkoutRepository()
    {
        Add(new Workout { Name = "Йога", Description = "Хатха-йога для розтяжки та розслаблення", TrainerId = 1, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(18, 0), DurationMinutes = 60, Level = DifficultyLevel.Beginner, MaxParticipants = 15 });
        Add(new Workout { Name = "Функціональний тренінг", Description = "Кругове тренування з власною вагою", TrainerId = 2, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeOnly(19, 0), DurationMinutes = 45, Level = DifficultyLevel.Intermediate, MaxParticipants = 12 });
        Add(new Workout { Name = "Пілатес", Description = "Зміцнення м'язів кору", TrainerId = 1, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeOnly(9, 0), DurationMinutes = 50, Level = DifficultyLevel.Beginner, MaxParticipants = 10 });
        Add(new Workout { Name = "Кросфіт", Description = "Високоінтенсивне тренування", TrainerId = 3, DayOfWeek = DayOfWeek.Friday, StartTime = new TimeOnly(20, 0), DurationMinutes = 60, Level = DifficultyLevel.Advanced, MaxParticipants = 8 });
    }

    public IReadOnlyList<Workout> GetAll()
    {
        lock (_lock) return _items.ToList();
    }

    public Workout? GetById(int id)
    {
        lock (_lock) return _items.FirstOrDefault(w => w.Id == id);
    }

    public Workout Add(Workout workout)
    {
        lock (_lock)
        {
            workout.Id = _nextId++;
            _items.Add(workout);
            return workout;
        }
    }

    public bool Update(Workout workout)
    {
        lock (_lock)
        {
            var index = _items.FindIndex(w => w.Id == workout.Id);
            if (index < 0) return false;
            _items[index] = workout;
            return true;
        }
    }

    public bool Delete(int id)
    {
        lock (_lock) return _items.RemoveAll(w => w.Id == id) > 0;
    }
}
