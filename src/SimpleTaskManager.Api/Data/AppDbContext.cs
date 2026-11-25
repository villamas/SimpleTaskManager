using Microsoft.EntityFrameworkCore;
using SimpleTaskManager.Api.Models;

namespace SimpleTaskManager.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks", table =>
            {
                table.HasCheckConstraint("CK_Tasks_Title_Length", "length(Title) BETWEEN 1 AND 255");
                table.HasCheckConstraint("CK_Tasks_Status", "Status IN ('Pending', 'Completed')");
            });
            entity.HasKey(task => task.Id);
            entity.Property(task => task.Id).ValueGeneratedOnAdd();
            entity.Property(task => task.Title).IsRequired();
            entity.Property(task => task.Status).IsRequired().HasDefaultValue("Pending");
        });
    }
}
