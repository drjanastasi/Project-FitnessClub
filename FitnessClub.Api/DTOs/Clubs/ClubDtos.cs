using System.ComponentModel.DataAnnotations;

namespace FitnessClub.Api.DTOs.Clubs
{
    /// <summary>Клуб (філія) у відповіді API.</summary>
    public record ClubDto(
        int Id,
        string Name,
        string? Description,
        string Address,
        string Phone,
        string Email,
        string? WorkHous);

    /// <summary>Тіло запиту POST /api/clubs.</summary>
    public class CreateClubDto
    {
        [Required(ErrorMessage = "Назва клубу обов'язкова.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Назва має містити від 2 до 100 символів.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Опис не може перевищувати 500 символів.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Адреса обов'язкова.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Адреса має містити від 5 до 200 символів.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Телефон обов'язковий.")]
        [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Телефон має бути у форматі +380XXXXXXXXX.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обов'язковий.")]
        [EmailAddress(ErrorMessage = "Некоректний формат email.")]
        [StringLength(100, ErrorMessage = "Email не може перевищувати 100 символів.")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Години роботи не можуть перевищувати 100 символів.")]
        public string? WorkHours { get; set; }
    }

    /// <summary>Тіло запиту PUT /api/clubs/{id}: повне оновлення.</summary>
    public class UpdateClubDto : CreateClubDto
    {
    }

    /// <summary>
    /// Query-параметри для GET /api/clubs: пошук і сортування.
    /// </summary>
    public class ClubQueryParameters
    {
        /// <summary>Пошук за назвою, адресою або описом.</summary>
        [StringLength(100)]
        public string? Search { get; set; }

        /// <summary>Поле сортування: name, address.</summary>
        [RegularExpression("^(?i)(name|address)$", ErrorMessage = "sortBy може бути лише: name, address.")]
        public string? SortBy { get; set; }

        /// <summary>Сортувати за спаданням.</summary>
        public bool Desc { get; set; }
    }
}
