using FitnessClub.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessClub.Api.Data.Configurations;

/// <summary>Fluent API налаштування таблиці Memberships.</summary>
public class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("Memberships");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Дві однакові назви абонемента не допускаються.
        builder.HasIndex(m => m.Name).IsUnique();

        builder.Property(m => m.Description)
            .HasMaxLength(500);

        // SQLite не вміє порівнювати й сортувати decimal на рівні SQL,
        // тому в базі ціна зберігається як число з плаваючою комою (REAL).
        builder.Property(m => m.Price)
            .IsRequired()
            .HasConversion<double>();

        builder.Property(m => m.DurationDays).IsRequired();

        builder.Property(m => m.IsActive).IsRequired();

        // Початкові дані — потрапляють у базу через міграцію.
        builder.HasData(
            new Membership { Id = 1, Name = "Разовий візит", Description = "Одне відвідування тренажерного залу", Price = 200, DurationDays = 1, IsActive = true },
            new Membership { Id = 2, Name = "Місячний безліміт", Description = "Необмежене відвідування залу та всіх групових тренувань", Price = 1200, DurationDays = 30, IsActive = true },
            new Membership { Id = 3, Name = "Ранковий", Description = "Відвідування з 7:00 до 12:00 і ранкові групові тренування", Price = 800, DurationDays = 30, IsActive = true },
            new Membership { Id = 4, Name = "Річний преміум", Description = "Безліміт + 4 персональні тренування на місяць", Price = 12000, DurationDays = 365, IsActive = false });
    }
}
