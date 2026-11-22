using FitnessClub.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessClub.Api.Data.Configuraitons
{
    /// <summary>Fluent API налаштування таблиці Bookings.</summary>
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("Bookings");

            builder.HasKey(b => b.Id);

            // Зовнішнього ключа на Members поки немає: клієнти зберігаються в пам'яті.
            // Коли Members перейдуть на EF Core, тут додати:
            // builder.HasOne<Member>().WithMany().HasForeignKey(b => b.MemberId)
            //        .OnDelete(DeleteBehavior.Restrict);
            builder.Property(b => b.MemberId).IsRequired();

            builder.Property(b => b.WorkoutId).IsRequired();

            // Багато записів на одне тренування. Тренування з записами видалити не можна (Restrict).
            builder.HasOne(b => b.Workout)
                .WithMany()
                .HasForeignKey(b => b.WorkoutId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(b => b.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(b => b.CreatedAt).IsRequired();

            // Швидкий підрахунок зайнятих місць на тренування в певну дату.
            builder.HasIndex(b => new { b.WorkoutId, b.Date });

            // Клієнт не може мати два активні записи на те саме тренування в ту саму дату.
            // Скасовані записи не враховуються, тож після скасування можна записатися знову.
            builder.HasIndex(b => new { b.MemberId, b.WorkoutId, b.Date })
                .IsUnique()
                .HasFilter("\"Status\" = 'Confirmed'");
        }
    }
}
