using FitnessClub.Api.DTOs.Memberships;
using FitnessClub.Api.Exceptions;
using FitnessClub.Api.Mapping;
using FitnessClub.Api.Repositories;
namespace FitnessClub.Api.Services;
public interface IMembershipService
{
    Task<IReadOnlyList<MembershipDto>> GetAllAsync(MembershipQueryParameters query);
    Task<MembershipDetailsDto> GetByIdAsync(int id);
    Task<MembershipDto> CreateAsync(CreateMembershipDto dto);
    Task<MembershipDto> UpdateAsync(int id, UpdateMembershipDto dto);
    Task<MembershipDto> UpdateStatusAsync(int id, bool isActive);
    Task DeleteAsync(int id);
    Task<MembershipDetailsDto> AddWorkoutAsync(int id, int workoutId);
    Task RemoveWorkoutAsync(int id, int workoutId);
}
public class MembershipService : IMembershipService
{
    private readonly IMembershipRepository _memberships;
    private readonly IWorkoutRepository _workouts;
    public MembershipService(IMembershipRepository memberships, IWorkoutRepository workouts)
    {
        _memberships = memberships;
        _workouts = workouts;
    }
    public async Task<IReadOnlyList<MembershipDto>> GetAllAsync(MembershipQueryParameters query)
    {
        var memberships = await _memberships.GetAllAsync(query);
        return memberships.Select(m => m.ToDto()).ToList();
    }
    public async Task<MembershipDetailsDto> GetByIdAsync(int id)
    {
        var membership = await _memberships.GetWithWorkoutsAsync(id)
            ?? throw NotFound(id);
        return membership.ToDetailsDto();
    }
    public async Task<MembershipDto> CreateAsync(CreateMembershipDto dto)
    {
        await EnsureNameIsUniqueAsync(dto.Name);
        var membership = dto.ToEntity();
        await _memberships.AddAsync(membership);
        return membership.ToDto();
    }
    public async Task<MembershipDto> UpdateAsync(int id, UpdateMembershipDto dto)
    {
        var membership = await _memberships.GetByIdAsync(id)
            ?? throw NotFound(id);
        await EnsureNameIsUniqueAsync(dto.Name, exceptId: id);
        dto.ApplyTo(membership);
        await _memberships.SaveChangesAsync();
        return membership.ToDto();
    }
    public async Task<MembershipDto> UpdateStatusAsync(int id, bool isActive)
    {
        var membership = await _memberships.GetByIdAsync(id)
            ?? throw NotFound(id);
        membership.IsActive = isActive;
        await _memberships.SaveChangesAsync();
        return membership.ToDto();
    }
    public async Task DeleteAsync(int id)
    {
        var membership = await _memberships.GetByIdAsync(id)
            ?? throw NotFound(id);

        await _memberships.DeleteAsync(membership);
    }
    public async Task<MembershipDetailsDto> AddWorkoutAsync(int id, int workoutId)
    {
        if (!await _memberships.ExistsAsync(id))
            throw NotFound(id);
        if (!await _workouts.ExistsAsync(workoutId))
            throw new NotFoundException($"Тренування з id={workoutId} не знайдено.");
        if (await _memberships.HasWorkoutAsync(id, workoutId))
            throw new ConflictException("Це тренування вже входить в абонемент.");
        await _memberships.AddWorkoutAsync(id, workoutId);
        return await GetByIdAsync(id);
    }
    public async Task RemoveWorkoutAsync(int id, int workoutId)
    {
        if (!await _memberships.RemoveWorkoutAsync(id, workoutId))
            throw new NotFoundException("Таке тренування не входить в цей абонемент.");
    }
    private async Task EnsureNameIsUniqueAsync(string name, int? exceptId = null)
    {
        if (await _memberships.NameExistsAsync(name, exceptId))
            throw new ConflictException($"Абонемент з назвою \"{name.Trim()}\" вже існує.");
    }
    private static NotFoundException NotFound(int id) =>
        new($"Абонемент з id={id} не знайдено.");
}
