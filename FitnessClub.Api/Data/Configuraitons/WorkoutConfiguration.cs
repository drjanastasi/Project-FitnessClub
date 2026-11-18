using FitnessClub.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessClub.Api.Data.Configurations;

/// <summary>Fluent API налаштування таблиці Workouts.</summary>
public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> builder)
    {
        builder.ToTable("Workouts");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.Description)
            .HasMaxLength(500);

        builder.Property(w => w.TrainerId).IsRequired();

        // День тижня зберігається числом (0 = неділя ... 6 = субота),
        // щоб розклад можна було сортувати на рівні бази.
        builder.Property(w => w.DayOfWeek).IsRequired();

        builder.Property(w => w.StartTime).IsRequired();

        builder.Property(w => w.DurationMinutes).IsRequired();

        // Рівень складності зберігається текстом ("Beginner"), так легше читати таблицю.
        builder.Property(w => w.Level)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(w => w.MaxParticipants).IsRequired();

        // Індекс для швидкого пошуку тренувань за днем тижня.
        builder.HasIndex(w => w.DayOfWeek);

        // ----- Зв'язок з тренером (увімкнути, коли буде сутність Trainer) -----
        // Один тренер веде багато тренувань (один-до-багатьох).
        // Restrict: не можна видалити тренера, поки в нього є тренування.
        //
        // builder.HasOne(w => w.Trainer)
        //     .WithMany(t => t.Workouts)
        //     .HasForeignKey(w => w.TrainerId)
        //     .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Workout { Id = 1, Name = "Йога", Description = "Хатха-йога для розтяжки та розслаблення", TrainerId = 1, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(18, 0), DurationMinutes = 60, Level = DifficultyLevel.Beginner, MaxParticipants = 15 },
            new Workout { Id = 2, Name = "Функціональний тренінг", Description = "Кругове тренування з власною вагою", TrainerId = 2, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeOnly(19, 0), DurationMinutes = 45, Level = DifficultyLevel.Intermediate, MaxParticipants = 12 },
            new Workout { Id = 3, Name = "Пілатес", Description = "Зміцнення м'язів кору", TrainerId = 1, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeOnly(9, 0), DurationMinutes = 50, Level = DifficultyLevel.Beginner, MaxParticipants = 10 },
            new Workout { Id = 4, Name = "Кросфіт", Description = "Високоінтенсивне тренування", TrainerId = 3, DayOfWeek = DayOfWeek.Friday, StartTime = new TimeOnly(20, 0), DurationMinutes = 60, Level = DifficultyLevel.Advanced, MaxParticipants = 8 });
    }
}
