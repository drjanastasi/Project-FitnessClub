using FitnessClub.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace FitnessClub.Api.Data;

public class FitnessClubDbContext : IdentityDbContext<AppUser>
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
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FitnessClubDbContext).Assembly);
        modelBuilder.Entity<AppUser>(user =>
        {
            user.Property(u => u.FirstName).HasMaxLength(50);
            user.Property(u => u.LastName).HasMaxLength(50);
        });
    }
}
