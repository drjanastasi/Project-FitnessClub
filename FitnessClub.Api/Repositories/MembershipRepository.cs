using FitnessClub.Api.Models;

namespace FitnessClub.Api.Repositories;

public interface IMembershipRepository
{
    IReadOnlyList<Membership> GetAll();
    Membership? GetById(int id);
    bool NameExists(string name, int? exceptId = null);
    Membership Add(Membership membership);
    bool Update(Membership membership);
    bool Delete(int id);
}

/// <summary>
/// Зберігання абонементів у пам'яті (етап 1).
/// На етапі 2 буде замінено реалізацією на Entity Framework Core,
/// контролер при цьому не зміниться — він залежить лише від інтерфейсу.
/// </summary>
public class InMemoryMembershipRepository : IMembershipRepository
{
    private readonly List<Membership> _items = new();
    private readonly object _lock = new();
    private int _nextId = 1;

    public InMemoryMembershipRepository()
    {
        Add(new Membership { Name = "Разовий візит", Description = "Одне відвідування тренажерного залу", Price = 200, DurationDays = 1, IsActive = true });
        Add(new Membership { Name = "Місячний безліміт", Description = "Необмежене відвідування залу та групових тренувань", Price = 1200, DurationDays = 30, IsActive = true });
        Add(new Membership { Name = "Ранковий", Description = "Відвідування з 7:00 до 12:00", Price = 800, DurationDays = 30, IsActive = true });
        Add(new Membership { Name = "Річний преміум", Description = "Безліміт + 4 персональні тренування на місяць", Price = 12000, DurationDays = 365, IsActive = false });
    }

    public IReadOnlyList<Membership> GetAll()
    {
        lock (_lock) return _items.ToList();
    }

    public Membership? GetById(int id)
    {
        lock (_lock) return _items.FirstOrDefault(m => m.Id == id);
    }

    public bool NameExists(string name, int? exceptId = null)
    {
        lock (_lock)
            return _items.Any(m => m.Id != exceptId &&
                                   string.Equals(m.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public Membership Add(Membership membership)
    {
        lock (_lock)
        {
            membership.Id = _nextId++;
            _items.Add(membership);
            return membership;
        }
    }

    public bool Update(Membership membership)
    {
        lock (_lock)
        {
            var index = _items.FindIndex(m => m.Id == membership.Id);
            if (index < 0) return false;
            _items[index] = membership;
            return true;
        }
    }

    public bool Delete(int id)
    {
        lock (_lock) return _items.RemoveAll(m => m.Id == id) > 0;
    }
}
