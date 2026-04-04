using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<IdentityUser, IdentityRole, string>(options)
{
    public DbSet<Course> Courses => Set<Course>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Course>(entity =>
        {
            entity.ToTable("Courses");
            entity.HasKey(course => course.Id);

            entity.Property(course => course.Title)
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(course => course.Slug)
                .HasMaxLength(180)
                .IsRequired();

            entity.Property(course => course.ShortDescription)
                .HasMaxLength(400)
                .IsRequired();

            entity.Property(course => course.Description)
                .HasMaxLength(6000)
                .IsRequired();

            entity.Property(course => course.StartDate)
                .IsRequired();

            entity.Property(course => course.Capacity)
                .IsRequired();

            entity.Property(course => course.IsPublished)
                .HasDefaultValue(false);

            entity.Property(course => course.CreatedAt)
                .IsRequired();

            entity.Property(course => course.UpdatedAt)
                .IsRequired();

            entity.HasIndex(course => course.Slug)
                .IsUnique();
        });
    }
}
