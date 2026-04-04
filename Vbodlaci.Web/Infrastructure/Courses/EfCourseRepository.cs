using Microsoft.EntityFrameworkCore;
using Vbodlaci.Web.Application.Courses;
using Vbodlaci.Web.Data;
using Vbodlaci.Web.Domain.Courses;

namespace Vbodlaci.Web.Infrastructure.Courses;

public sealed class EfCourseRepository(ApplicationDbContext dbContext) : ICourseRepository
{
    public async Task<IReadOnlyList<Course>> GetPublishedAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Courses
            .AsNoTracking()
            .Where(course => course.IsPublished)
            .OrderBy(course => course.StartDate)
            .ThenBy(course => course.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Courses
            .AsNoTracking()
            .OrderByDescending(course => course.StartDate)
            .ThenBy(course => course.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<Course?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Courses.FirstOrDefaultAsync(course => course.Id == id, cancellationToken);
    }

    public Task<bool> SlugExistsAsync(string slug, Guid? excludingCourseId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Courses
            .AsNoTracking()
            .Where(course => course.Slug == slug);

        if (excludingCourseId.HasValue)
        {
            query = query.Where(course => course.Id != excludingCourseId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Course course, CancellationToken cancellationToken = default)
    {
        dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Course course, CancellationToken cancellationToken = default)
    {
        dbContext.Courses.Update(course);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Course course, CancellationToken cancellationToken = default)
    {
        dbContext.Courses.Remove(course);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
