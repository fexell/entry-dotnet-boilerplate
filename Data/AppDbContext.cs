using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Entry.Auth.Models;

namespace Entry.Auth.Data
{
  public class AppDbContext : IdentityDbContext<AppUser>
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  public DbSet<RefreshToken> RefreshTokens { get; set; }

  protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      builder.Entity<RefreshToken>(entity =>
      {
        entity.HasKey(x => x.Id);

        entity.Property(x => x.Token).IsRequired();

        entity.Property(x => x.UserId).IsRequired();

        entity.HasOne<AppUser>()
              .WithMany()
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Cascade);
      });
    }
  }
}