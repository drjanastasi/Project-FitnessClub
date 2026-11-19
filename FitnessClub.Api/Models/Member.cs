namespace FitnessClub.Api.Models;


public class Member
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }

    public bool IsActive { get; set; }

    public int MembershipId { get; set; }
}
