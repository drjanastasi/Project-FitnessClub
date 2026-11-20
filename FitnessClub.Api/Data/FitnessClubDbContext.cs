using FitnessClub.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessClub.Api.Data;

/// <summary>
/// Єдиний DbContext усього проєкту. Кожен DbSet — це таблиця в базі даних.
/// Учасники команди додають сюди свої DbSet (Members, Trainers, Club, Bookings),
/// а налаштування таблиці — окремим класом у папці Data/Configurations.
/// </summary>
public class FitnessClubDbContext : DbContext
{
    public FitnessClubDbContext(DbContextOptions<FitnessClubDbContext> options)
        : base(options)
    {
    }

    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<MembershipWorkout> MembershipWorkouts => Set<MembershipWorkout>();
    public DbSet<Club> Clubs => Set<Club>();

    // TODO (інші учасники):
    // public DbSet<Member> Members => Set<Member>();
    // public DbSet<Trainer> Trainers => Set<Trainer>();
    // public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Підхоплює всі класи IEntityTypeConfiguration<T> з папки Data/Configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FitnessClubDbContext).Assembly);
    }
}
