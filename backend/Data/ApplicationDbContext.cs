using Microsoft.EntityFrameworkCore;
using SecureDocumentAPI.Models;

namespace SecureDocumentAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentPermission> DocumentPermissions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Folder> Folders { get; set; }
    public DbSet<DocumentComment> DocumentComments { get; set; }
    public DbSet<DocumentTag> DocumentTags { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<SecurityEvent> SecurityEvents { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
        });
        
        // Configure Document entity
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.FolderId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.SecurityLevel);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EncryptionKey).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Checksum).IsRequired().HasMaxLength(100);
        });
        
        // Configure DocumentPermission entity
        modelBuilder.Entity<DocumentPermission>(entity =>
        {
            entity.HasIndex(e => new { e.DocumentId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsActive);
        });
        
        // Configure AuditLog entity
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Action);
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.IsSecurityEvent);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
        });
        
        // Configure Folder entity
        modelBuilder.Entity<Folder>(entity =>
        {
            entity.HasIndex(e => e.ParentId);
            entity.HasIndex(e => e.OwnerId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
        });
        
        // Configure DocumentComment entity
        modelBuilder.Entity<DocumentComment>(entity =>
        {
            entity.HasIndex(e => e.DocumentId);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
        });
        
        // Configure Tag entity
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        });
        
        // Configure UserSession entity
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.SessionToken).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);
            entity.Property(e => e.SessionToken).IsRequired().HasMaxLength(255);
        });
        
        // Configure SecurityEvent entity
        modelBuilder.Entity<SecurityEvent>(entity =>
        {
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Severity);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Severity).IsRequired().HasMaxLength(50);
        });
        
        // Configure relationships
        modelBuilder.Entity<Document>()
            .HasOne(d => d.Owner)
            .WithMany(u => u.OwnedDocuments)
            .HasForeignKey(d => d.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<Document>()
            .HasOne(d => d.PreviousVersion)
            .WithMany(d => d.NextVersions)
            .HasForeignKey(d => d.PreviousVersionId)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<DocumentPermission>()
            .HasOne(dp => dp.Document)
            .WithMany(d => d.Permissions)
            .HasForeignKey(dp => dp.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<DocumentPermission>()
            .HasOne(dp => dp.User)
            .WithMany(u => u.DocumentPermissions)
            .HasForeignKey(dp => dp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        modelBuilder.Entity<AuditLog>()
            .HasOne(al => al.Document)
            .WithMany(d => d.AuditLogs)
            .HasForeignKey(al => al.DocumentId)
            .OnDelete(DeleteBehavior.SetNull);
            
        modelBuilder.Entity<AuditLog>()
            .HasOne(al => al.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(al => al.UserAuditId)
            .OnDelete(DeleteBehavior.SetNull);
    }
    
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is User || e.Entity is Document || e.Entity is Folder)
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
            
        foreach (var entry in entries)
        {
            var entity = entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                if (entity is User user)
                {
                    user.CreatedAt = DateTime.UtcNow;
                    user.UpdatedAt = DateTime.UtcNow;
                }
                else if (entity is Document document)
                {
                    document.CreatedAt = DateTime.UtcNow;
                    document.UpdatedAt = DateTime.UtcNow;
                }
                else if (entity is Folder folder)
                {
                    folder.CreatedAt = DateTime.UtcNow;
                    folder.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entity is User user)
                {
                    user.UpdatedAt = DateTime.UtcNow;
                }
                else if (entity is Document document)
                {
                    document.UpdatedAt = DateTime.UtcNow;
                }
                else if (entity is Folder folder)
                {
                    folder.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
