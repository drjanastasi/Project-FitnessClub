using FitnessClub.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessClub.Api.Data.Configuraitons
{
    /// <summary>Fluent API налаштування таблиці Members.</summary>
    public class MemberConfiguration : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
            builder.ToTable("Members");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(m => m.LastName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(m => m.Email)
                .IsRequired()
                .HasMaxLength(100);

            // Унікальний email: за ним акаунт (AppUser) пов'язується з клієнтом.
            builder.HasIndex(m => m.Email).IsUnique();

            builder.Property(m => m.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(m => m.BirthDate).IsRequired();

            builder.Property(m => m.IsActive).IsRequired();

            // Клієнт має абонемент. Абонемент, який хтось використовує, видалити не можна.
            builder.HasOne<Membership>()
                .WithMany()
                .HasForeignKey(m => m.MembershipId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ті самі три клієнти, що були в пам'яті (id 1–3 зберігаються).
            builder.HasData(
                new Member { Id = 1, FirstName = "Олександр", LastName = "Іваненко", Email = "ivanenko@gmail.com", Phone = "+380501112233", BirthDate = new DateOnly(1995, 4, 12), IsActive = true, MembershipId = 1 },
                new Member { Id = 2, FirstName = "Марія", LastName = "Петренко", Email = "petrenko@gmail.com", Phone = "+380671234567", BirthDate = new DateOnly(2000, 8, 25), IsActive = true, MembershipId = 2 },
                new Member { Id = 3, FirstName = "Андрій", LastName = "Коваленко", Email = "kovalenko@gmail.com", Phone = "+380931234567", BirthDate = new DateOnly(1988, 2, 5), IsActive = false, MembershipId = 3 });
        }
    }
}
