using FitnessClub.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace FitnessClub.Api.DTOs.Bookings
{
    /// <summary>Запис на тренування у відповіді API.</summary>
    public record BookingDto(
        int Id,
        int MemverId,
        int WorkoutId,
        string WorkotName,
        DayOfWeek DayOfWeek,
        TimeOnly StartTime,
        DateOnly Date,
        BookingStatus Status,
        DateTime CreatedAt);

    /// <summary>Тіло запиту POST /api/bookings.</summary>
    public class CreateBookingDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Вкажіть коректний id клієнта.")]
        public int MemberId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Вкажіть коректний id тренування.")]
        public int WorkoutId { get; set; }

        /// <summary>Дата заняття у форматі yyyy-MM-dd.</summary>
        [Required(ErrorMessage = "Дата заняття обов'язкова.")]
        public DateOnly? Date { get; set; }
    }

    /// <summary>
    /// Query-параметри для GET /api/bookings: фільтрація та сортування.
    /// </summary>
    public class BookingQueryParameters : IValidatableObject
    {
        public int? MemberId { get; set; }

        public int? WorkoutId { get; set; }

        /// <summary>Записи на тренування конкретного клубу (філії).</summary>
        public int? ClubId { get; set; }

        /// <summary>Статус запису: Confirmed або Cancelled.</summary>
        public BookingStatus? Status { get; set; }

        /// <summary>Записи з цієї дати включно.</summary>
        public DateOnly? DateFrom { get; set; }

        /// <summary>Записи до цієї дати включно.</summary>
        public DateOnly? DateTo { get; set; }

        /// <summary>Поле сортування: date, createdAt.</summary>
        [RegularExpression("^(?i)(date|createdAt)$", ErrorMessage = "sortBy може бути лише: date, createdAt.")]
        public string? SortBy { get; set; }

        /// <summary>Сортувати за спаданням.</summary>
        public bool Desc { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DateFrom.HasValue && DateTo.HasValue && DateFrom > DateTo)
            {
                yield return new ValidationResult(
                    "dateFrom не може бути пізніше за dateTo.",
                    new[] { nameof(DateFrom), nameof(DateTo) });
            }
        }
    }

    /// <summary>Кількість вільних місць на тренування в конкретну дату.</summary>
    public record WorkoutAvailabilityDto(
        int WorkoutId,
        DateOnly Date,
        int MaxParticipants,
        int Booked,
        int FreePlaces);

}
