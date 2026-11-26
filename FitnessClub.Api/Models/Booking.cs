namespace FitnessClub.Api.Models
{
    /// <summary>
    /// Запис клієнта на тренування в конкретну дату.
    /// Workout — це розклад на тиждень (день тижня + час), тому дата заняття
    /// зберігається в самому записі. Клуб визначається через тренування (Workout.ClubId).
    /// Таблиця: Bookings.
    /// </summary>
    public class Booking
    {
        /// <summary>Первинний ключ.</summary>
        public int Id { get; set; }

        /// <summary>
        /// Клієнт (FK → Members)
        /// </summary>
        public int MemberId { get; set; }

        /// <summary>Тренування, на яке записався клієнт (FK → Workouts).</summary>
        public int WorkoutId { get; set; }

        public Workout Workout { get; set; } = null!;

        /// <summary>Дата заняття (її день тижня має збігатися з Workout.DayOfWeek).</summary>
        public DateOnly Date { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

        /// <summary>Коли запис створено (UTC).</summary>
        public DateTime CreatedAt { get; set; }
    }

    public enum BookingStatus
    {
        Confirmed,
        Cancelled
    }
}
