using Data.Authentication;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Persistence;

internal sealed class RentalDbContext(DbContextOptions<RentalDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<UserSession>(entity =>
        {
            entity.HasKey(session => session.Id);
            entity.Property(session => session.Id).HasMaxLength(32);
            entity.Property(session => session.UserId).IsRequired();
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(session => session.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(session => session.ExpiresAt);
            entity.ToTable("UserSessions", table => table.HasCheckConstraint("CK_UserSessions_Period", "\"ExpiresAt\" > \"CreatedAt\""));
        });
    }
}
