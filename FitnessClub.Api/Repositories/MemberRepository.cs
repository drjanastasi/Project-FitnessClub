using FitnessClub.Api.Models;

namespace FitnessClub.Api.Repositories;

public interface IMemberRepository
{
    IReadOnlyList<Member> GetAll();
    Member? GetById(int id);
    bool EmailExists(string email, int? exceptId = null);
    Member Add(Member member);
    bool Update(Member member);
    bool Delete(int id);
}


public class InMemoryMemberRepository : IMemberRepository
{
    private readonly List<Member> _items = new();
    private readonly object _lock = new();
    private int _nextId = 1;

    public InMemoryMemberRepository()
    {
        Add(new Member
        {
            FirstName = "Олександр",
            LastName = "Іваненко",
            Email = "ivanenko@gmail.com",
            Phone = "+380501112233",
            BirthDate = new DateOnly(1995, 4, 12),
            IsActive = true,
            MembershipId = 1
        });

        Add(new Member
        {
            FirstName = "Марія",
            LastName = "Петренко",
            Email = "petrenko@gmail.com",
            Phone = "+380671234567",
            BirthDate = new DateOnly(2000, 8, 25),
            IsActive = true,
            MembershipId = 2
        });

        Add(new Member
        {
            FirstName = "Андрій",
            LastName = "Коваленко",
            Email = "kovalenko@gmail.com",
            Phone = "+380931234567",
            BirthDate = new DateOnly(1988, 2, 5),
            IsActive = false,
            MembershipId = 3
        });
    }

    public IReadOnlyList<Member> GetAll()
    {
        lock (_lock)
            return _items.ToList();
    }

    public Member? GetById(int id)
    {
        lock (_lock)
            return _items.FirstOrDefault(m => m.Id == id);
    }

    public bool EmailExists(string email, int? exceptId = null)
    {
        lock (_lock)
            return _items.Any(m =>
                m.Id != exceptId &&
                string.Equals(m.Email, email.Trim(),
                    StringComparison.OrdinalIgnoreCase));
    }

    public Member Add(Member member)
    {
        lock (_lock)
        {
            member.Id = _nextId++;
            _items.Add(member);
            return member;
        }
    }

    public bool Update(Member member)
    {
        lock (_lock)
        {
            var index = _items.FindIndex(m => m.Id == member.Id);

            if (index < 0)
                return false;

            _items[index] = member;
            return true;
        }
    }

    public bool Delete(int id)
    {
        lock (_lock)
            return _items.RemoveAll(m => m.Id == id) > 0;
    }
}
