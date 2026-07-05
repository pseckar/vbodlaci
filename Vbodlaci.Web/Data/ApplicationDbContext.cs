using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vbodlaci.Web.Domain.Contacts;
using Vbodlaci.Web.Domain.Courses;
using Vbodlaci.Web.Domain.Emails;
using Vbodlaci.Web.Domain.Newsletter;
using Vbodlaci.Web.Domain.Registrations;

namespace Vbodlaci.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<IdentityUser, IdentityRole, string>(options)
{
    public DbSet<Course> Courses => Set<Course>();

    public DbSet<CourseTextDefault> CourseTextDefaults => Set<CourseTextDefault>();

    public DbSet<CourseRegistration> CourseRegistrations => Set<CourseRegistration>();

    public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();

    public DbSet<NewsletterDispatchLog> NewsletterDispatchLogs => Set<NewsletterDispatchLog>();

    public DbSet<ContactInquiry> ContactInquiries => Set<ContactInquiry>();

    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Course>(entity =>
        {
            entity.ToTable("Courses");
            entity.HasKey(course => course.Id);

            entity.Property(course => course.Type)
                .HasConversion<string>()
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(course => course.Status)
                .HasConversion<string>()
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(course => course.Title)
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(course => course.Slug)
                .HasMaxLength(180)
                .IsRequired();

            entity.Property(course => course.TimeText)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(course => course.CityOrArea)
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(course => course.PriceText)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(course => course.ShortDescription)
                .HasMaxLength(400)
                .IsRequired();

            entity.Property(course => course.FullDescription)
                .HasMaxLength(8000)
                .IsRequired();

            entity.Property(course => course.WhatToExpect)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(course => course.ImagePath)
                .HasMaxLength(260)
                .IsRequired();

            entity.Property(course => course.ThumbnailPath)
                .HasMaxLength(260)
                .IsRequired();

            entity.Property(course => course.CreatedAt)
                .IsRequired();

            entity.Property(course => course.UpdatedAt)
                .IsRequired();

            entity.HasIndex(course => course.Slug)
                .IsUnique();

            entity.HasIndex(course => new { course.Status, course.CourseDate });
        });

        builder.Entity<CourseTextDefault>(entity =>
        {
            entity.ToTable("CourseTextDefaults");
            entity.HasKey(defaultText => defaultText.Id);

            entity.Property(defaultText => defaultText.Type)
                .HasConversion<string>()
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(defaultText => defaultText.Field)
                .HasConversion<string>()
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(defaultText => defaultText.Text)
                .HasMaxLength(8000)
                .IsRequired();

            entity.HasIndex(defaultText => new { defaultText.Type, defaultText.Field })
                .IsUnique();

            var seededAt = new DateTimeOffset(2026, 5, 10, 0, 0, 0, TimeSpan.Zero);
            entity.HasData(
                new CourseTextDefault { Id = Guid.Parse("2ef8d70f-f5c0-49a0-8dbf-fc89040d1931"), Type = CourseType.Breathwork, Field = CourseTextField.ShortDescription, Text = "This is placeholder for default text", UpdatedAt = seededAt },
                new CourseTextDefault { Id = Guid.Parse("5665bfd0-4e84-4699-9174-68eba17f8d41"), Type = CourseType.Breathwork, Field = CourseTextField.FullDescription, Text = "This is placeholder for default text", UpdatedAt = seededAt },
                new CourseTextDefault { Id = Guid.Parse("f5a4d334-301e-4015-9443-425649bc74c8"), Type = CourseType.Breathwork, Field = CourseTextField.WhatToExpect, Text = "This is placeholder for default text", UpdatedAt = seededAt },
                new CourseTextDefault { Id = Guid.Parse("49bdd9a5-b692-4d46-9797-5be87405e914"), Type = CourseType.Horses, Field = CourseTextField.ShortDescription, Text = "This is placeholder for default text", UpdatedAt = seededAt },
                new CourseTextDefault { Id = Guid.Parse("c3addbf4-e5a7-45fd-8e1d-3d079223a967"), Type = CourseType.Horses, Field = CourseTextField.FullDescription, Text = "This is placeholder for default text", UpdatedAt = seededAt },
                new CourseTextDefault { Id = Guid.Parse("fc589548-f8b9-4e50-a7de-4d618d9857d4"), Type = CourseType.Horses, Field = CourseTextField.WhatToExpect, Text = "This is placeholder for default text", UpdatedAt = seededAt });
        });

        builder.Entity<CourseRegistration>(entity =>
        {
            entity.ToTable("CourseRegistrations");
            entity.HasKey(registration => registration.Id);

            entity.Property(registration => registration.FullName)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(registration => registration.Email)
                .HasMaxLength(180)
                .IsRequired();

            entity.Property(registration => registration.Note)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(registration => registration.ClientIp)
                .HasMaxLength(64)
                .IsRequired();

            entity.HasOne(registration => registration.Course)
                .WithMany(course => course.Registrations)
                .HasForeignKey(registration => registration.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(registration => registration.CourseId);
        });

        builder.Entity<NewsletterSubscriber>(entity =>
        {
            entity.ToTable("NewsletterSubscribers");
            entity.HasKey(subscriber => subscriber.Id);

            entity.Property(subscriber => subscriber.Email)
                .HasMaxLength(180)
                .IsRequired();

            entity.Property(subscriber => subscriber.UnsubscribeToken)
                .HasMaxLength(64)
                .IsRequired();

            entity.HasIndex(subscriber => subscriber.Email)
                .IsUnique();

            entity.HasIndex(subscriber => subscriber.UnsubscribeToken)
                .IsUnique();
        });

        builder.Entity<NewsletterDispatchLog>(entity =>
        {
            entity.ToTable("NewsletterDispatchLogs");
            entity.HasKey(dispatch => dispatch.Id);

            entity.HasIndex(dispatch => new { dispatch.CourseId, dispatch.SubscriberId })
                .IsUnique();
        });

        builder.Entity<ContactInquiry>(entity =>
        {
            entity.ToTable("ContactInquiries");
            entity.HasKey(inquiry => inquiry.Id);

            entity.Property(inquiry => inquiry.FullName)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(inquiry => inquiry.Email)
                .HasMaxLength(180)
                .IsRequired();

            entity.Property(inquiry => inquiry.Message)
                .HasMaxLength(2500)
                .IsRequired();

            entity.Property(inquiry => inquiry.SourcePage)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(inquiry => inquiry.ClientIp)
                .HasMaxLength(64)
                .IsRequired();
        });

        builder.Entity<EmailLog>(entity =>
        {
            entity.ToTable("EmailLogs");
            entity.HasKey(log => log.Id);

            entity.Property(log => log.Kind)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(log => log.Recipient)
                .HasMaxLength(180)
                .IsRequired();

            entity.Property(log => log.Subject)
                .HasMaxLength(220)
                .IsRequired();

            entity.Property(log => log.Status)
                .HasMaxLength(40)
                .IsRequired();

            entity.Property(log => log.ErrorMessage)
                .HasMaxLength(1200)
                .IsRequired();
        });
    }
}
