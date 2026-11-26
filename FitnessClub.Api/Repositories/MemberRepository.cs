using FitnessClub.Api.Data;
using FitnessClub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Api.Repositories;

public interface IMemberRepository
{
    Task<List<Member>> GetAllAsync(bool? isActive, string? search);
    Task<Member?> GetByIdAsync(int id);
    Task<Member?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email, int? exceptId = null);
    Task AddAsync(Member member);
    Task SaveChangesAsync();
    Task DeleteAsync(Member member);
}

/// <summary>
/// Робота з клієнтами через Entity Framework Core.
/// Email зберігається в нижньому регістрі (це робить сервіс), тому порівняння прямі.
/// </summary>
public class MemberRepository : IMemberRepository
{
    private readonly FitnessClubDbContext _db;

    public MemberRepository(FitnessClubDbContext db)
    {
        _db = db;
    }

    public async Task<List<Member>> GetAllAsync(bool? isActive, string? search)
    {
        IQueryable<Member> query = _db.Members.AsNoTracking();

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        // Увага: у SQLite пошук чутливий до регістру для кирилиці.
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(m =>
                m.FirstName.Contains(s) ||
                m.LastName.Contains(s) ||
                m.Email.Contains(s));
        }

        return await query.OrderBy(m => m.Id).ToListAsync();
    }

    /// <summary>Клієнт для зміни (EF відстежує його, SaveChangesAsync збереже зміни).</summary>
    public Task<Member?> GetByIdAsync(int id) =>
        _db.Members.FirstOrDefaultAsync(m => m.Id == id);

    public Task<Member?> GetByEmailAsync(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _db.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Email == normalized);
    }

    public Task<bool> EmailExistsAsync(string email, int? exceptId = null)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _db.Members.AnyAsync(m => m.Email == normalized && m.Id != exceptId);
    }

    public async Task AddAsync(Member member)
    {
        _db.Members.Add(member);
        await _db.SaveChangesAsync();
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();

    public async Task DeleteAsync(Member member)
    {
        _db.Members.Remove(member);
        await _db.SaveChangesAsync();
    }
}