namespace FitnessClub.Api.Models
{
    /// <summary>
    /// Філія фітнес-клубу. Таблиця: Clubs.
    /// </summary>
    public class Club
    {
        public int Id {  get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Address { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email {  get; set; } = string.Empty;

        public string? WorkHours { get; set; }

    }
}
