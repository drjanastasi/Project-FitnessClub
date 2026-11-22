namespace FitnessClub.Api.Models;
public class Workout
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TrainerId { get; set; }
    public int ClubId { get; set; }
    public Club Club { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public int DurationMinutes { get; set; }
    public DifficultyLevel Level { get; set; }
    public int MaxParticipants { get; set; }
    public ICollection<MembershipWorkout> MembershipWorkouts { get; set; } = new List<MembershipWorkout>();
}
public enum DifficultyLevel
{
    Beginner,
    Intermediate,
    Advanced
}
