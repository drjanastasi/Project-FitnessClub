using FitnessClub.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessClub.Api.Data.Configuraitons
{
    /// <summary>Fluent API налаштування таблиці Clubs.</summary>
    public class ClubConfiguration : IEntityTypeConfiguration<Club>
    {
        public void Configure(EntityTypeBuilder<Club> builder) 
        {
            builder.ToTable("Clubs");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Дві філії з однаковою назвою не допускаються.
            builder.HasIndex(c => c.Name).IsUnique();

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.Address)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Phone)
                .IsRequired()
                .HasMaxLength(13);

            builder.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.WorkHours)
                .HasMaxLength(100);

            // Початкові дані — потрапляють у базу через міграцію.
            builder.HasData(
                new Club { Id = 1, Name = "FitnessClub Центр", Description = "Головна філія з тренажерним залом і залами для групових занять", Address = "м. Кривий Ріг, вул. Центральна, 1", Phone = "+380501234567", Email = "center@fitnessclub.ua", WorkHours = "Пн–Нд 07:00–22:00" },
                new Club { Id = 2, Name = "FitnessClub Північ", Description = "Філія в північному районі", Address = "м. Кривий Ріг, вул. Північна, 10", Phone = "+380507654321", Email = "north@fitnessclub.ua", WorkHours = "Пн–Пт 08:00–21:00, Сб–Нд 09:00–18:00" });
        }
    }
}
