using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Entry.Auth.Models;

namespace Entry.Auth.Data
{
  public class AppDbContext : IdentityDbContext<AppUser, IdentityRole, string>
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      builder.Entity<AppUser>(entity =>
      {
        entity.Property(x => x.CreatedAt).IsRequired();

        // Optional profile fields
        entity.Property(x => x.DisplayName).HasMaxLength(64);
        entity.Property(x => x.Avatar).HasMaxLength(256);
        entity.Property(x => x.Premium).HasDefaultValue(false);
      });

      builder.Entity<RefreshToken>(entity =>
      {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Token).IsRequired();
        entity.Property(x => x.UserId).IsRequired();

        entity.HasIndex(x => x.Token).IsUnique();
        entity.HasIndex(x => x.UserId);

        entity.HasOne(x => x.User)
              .WithMany(u => u.RefreshTokens)
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Cascade);
      });

      base.OnModelCreating(builder);
    }
  }
}
