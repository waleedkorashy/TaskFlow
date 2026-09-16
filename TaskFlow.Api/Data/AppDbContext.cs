using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Entities;

namespace TaskFlow.Api.Data;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<BoardColumn> BoardColumns => Set<BoardColumn>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<EmailOtp> EmailOtps => Set<EmailOtp>();
    public DbSet<ProjectInvitation> ProjectInvitations => Set<ProjectInvitation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TaskItem>()
            .HasMany(t => t.Labels)
            .WithMany(l => l.Tasks);

        builder.Entity<Project>()
            .HasOne(p => p.Owner)
            .WithMany()
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Project>()
            .Property(p => p.Name)
            .HasMaxLength(100);

        builder.Entity<Board>()
            .Property(b => b.Name)
            .HasMaxLength(100);

        builder.Entity<BoardColumn>()
            .Property(c => c.Name)
            .HasMaxLength(50);

        builder.Entity<TaskItem>()
            .Property(t => t.Title)
            .HasMaxLength(200);

        builder.Entity<Comment>()
            .Property(c => c.Content)
            .HasMaxLength(5000);

        builder.Entity<Label>()
            .Property(l => l.Name)
            .HasMaxLength(50);

        builder.Entity<Label>()
            .Property(l => l.ColorHex)
            .HasMaxLength(7);

        builder.Entity<BoardColumn>()
            .HasIndex(c => new { c.BoardId, c.SortOrder });

        builder.Entity<TaskItem>()
            .HasIndex(t => new { t.BoardColumnId, t.SortOrder });

        builder.Entity<TaskItem>()
            .HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Board>()
            .HasOne(b => b.Project)
            .WithMany(p => p.Boards)
            .HasForeignKey(b => b.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<BoardColumn>()
            .HasOne(c => c.Board)
            .WithMany(b => b.BoardColumns)
            .HasForeignKey(c => c.BoardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TaskItem>()
            .HasOne(t => t.BoardColumn)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.BoardColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Comment>()
            .HasOne(c => c.TaskItem)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProjectMember>()
            .HasOne(m => m.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProjectMember>()
            .HasIndex(m => new { m.ProjectId, m.UserId })
            .IsUnique();

        builder.Entity<Label>()
            .HasOne(l => l.Project)
            .WithMany(p => p.Labels)
            .HasForeignKey(l => l.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<EmailOtp>()
            .Property(o => o.Code)
            .HasMaxLength(64);

        builder.Entity<EmailOtp>()
            .HasIndex(o => new { o.UserId, o.Purpose });

        builder.Entity<EmailOtp>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProjectInvitation>()
            .HasOne(i => i.Project)
            .WithMany()
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProjectInvitation>()
            .Property(i => i.Email)
            .HasMaxLength(256);

        builder.Entity<ProjectInvitation>()
            .Property(i => i.Token)
            .HasMaxLength(32);

        builder.Entity<ProjectInvitation>()
            .HasIndex(i => i.Token)
            .IsUnique();

        builder.Entity<ProjectInvitation>()
            .HasOne(i => i.InvitedBy)
            .WithMany()
            .HasForeignKey(i => i.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
