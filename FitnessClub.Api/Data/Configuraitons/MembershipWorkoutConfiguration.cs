using FitnessClub.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitnessClub.Api.Data.Configurations;

/// <summary>
/// Проміжна таблиця MembershipWorkouts — зв'язок багато-до-багатьох
/// між абонементами та тренуваннями.
/// </summary>
public class MembershipWorkoutConfiguration : IEntityTypeConfiguration<MembershipWorkout>
{
    public void Configure(EntityTypeBuilder<MembershipWorkout> builder)
    {
        builder.ToTable("MembershipWorkouts");

        // Складений первинний ключ: одна пара (абонемент, тренування) — лише один раз.
        builder.HasKey(mw => new { mw.MembershipId, mw.WorkoutId });

        // Абонемент 1 —— * MembershipWorkout.
        // Cascade: видалили абонемент — зникають і його зв'язки з тренуваннями
        // (самі тренування залишаються).
        builder.HasOne(mw => mw.Membership)
            .WithMany(m => m.MembershipWorkouts)
            .HasForeignKey(mw => mw.MembershipId)
            .OnDelete(DeleteBehavior.Cascade);

        // Тренування 1 —— * MembershipWorkout. Те саме правило.
        builder.HasOne(mw => mw.Workout)
            .WithMany(w => w.MembershipWorkouts)
            .HasForeignKey(mw => mw.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        // Які тренування входять в які абонементи.
        builder.HasData(
            // Місячний безліміт — усі тренування.
            new MembershipWorkout { MembershipId = 2, WorkoutId = 1 },
            new MembershipWorkout { MembershipId = 2, WorkoutId = 2 },
            new MembershipWorkout { MembershipId = 2, WorkoutId = 3 },
            new MembershipWorkout { MembershipId = 2, WorkoutId = 4 },
            // Ранковий — лише ранковий пілатес.
            new MembershipWorkout { MembershipId = 3, WorkoutId = 3 },
            // Річний преміум — усі тренування.
            new MembershipWorkout { MembershipId = 4, WorkoutId = 1 },
            new MembershipWorkout { MembershipId = 4, WorkoutId = 2 },
            new MembershipWorkout { MembershipId = 4, WorkoutId = 3 },
            new MembershipWorkout { MembershipId = 4, WorkoutId = 4 });
    }
}
