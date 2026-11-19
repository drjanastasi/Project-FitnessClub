using FitnessClub.Api.Models;

namespace FitnessClub.Api.Repositories;

public interface ITrainerRepository
{
    IReadOnlyList<Trainer> GetAll();
    Trainer? GetById(int id);
    bool EmailExists(string email, int? exceptId = null);
    Trainer Add(Trainer trainer);
    bool Update(Trainer trainer);
    bool Delete(int id);
}

/// <summary>
/// Зберігання тренерів у пам'яті.
/// </summary>
public class InMemoryTrainerRepository : ITrainerRepository
{
    private readonly List<Trainer> _items = new();
    private readonly object _lock = new();
    private int _nextId = 1;

    public InMemoryTrainerRepository()
    {
        Add(new Trainer
        {
            FirstName = "Олексій",
            LastName = "Мельник",
            Email = "melnyk@gmail.com",
            Phone = "+380501234567",
            Specialization = "Силові тренування",
            Experience = 5
        });

        Add(new Trainer
        {
            FirstName = "Ірина",
            LastName = "Шевченко",
            Email = "shevchenko@gmail.com",
            Phone = "+380671234567",
            Specialization = "Йога",
            Experience = 3
        });

        Add(new Trainer
        {
            FirstName = "Максим",
            LastName = "Бондаренко",
            Email = "bondarenko@gmail.com",
            Phone = "+380931234567",
            Specialization = "Кардіотренування",
            Experience = 7
        });
    }

    public IReadOnlyList<Trainer> GetAll()
    {
        lock (_lock)
            return _items.ToList();
    }

    public Trainer? GetById(int id)
    {
        lock (_lock)
            return _items.FirstOrDefault(t => t.Id == id);
    }

    public bool EmailExists(string email, int? exceptId = null)
    {
        lock (_lock)
            return _items.Any(t =>
                t.Id != exceptId &&
                string.Equals(
                    t.Email,
                    email.Trim(),
                    StringComparison.OrdinalIgnoreCase));
    }

    public Trainer Add(Trainer trainer)
    {
        lock (_lock)
        {
            trainer.Id = _nextId++;
            _items.Add(trainer);
            return trainer;
        }
    }

    public bool Update(Trainer trainer)
    {
        lock (_lock)
        {
            var index = _items.FindIndex(t => t.Id == trainer.Id);

            if (index < 0)
                return false;

            _items[index] = trainer;
            return true;
        }
    }

    public bool Delete(int id)
    {
        lock (_lock)
            return _items.RemoveAll(t => t.Id == id) > 0;
    }
}
