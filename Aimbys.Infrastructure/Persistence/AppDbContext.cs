using Aimbys.Domain.Entities;
using Aimbys.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Aimbys.Infrastructure.Persistence;

/// <summary>
/// Application <see cref="DbContext"/> for the Aimbys MVP schema.
///
/// All EF mapping lives here in <see cref="OnModelCreating"/> so domain
/// entities (under <c>Aimbys.Domain</c>) stay free of EF Core attributes.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Storage column length used for ASP.NET Identity user ids. Matches the
    /// <c>IdentityUser.Id</c> default of <c>nvarchar(450)</c>.
    /// </summary>
    public const int IdentityUserIdLength = 450;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<TemplateVersion> TemplateVersions => Set<TemplateVersion>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<ExportArtifact> ExportArtifacts => Set<ExportArtifact>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- Project --------------------------------------------------
        modelBuilder.Entity<Project>(b =>
        {
            b.ToTable("Projects");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(2000);
            b.Property(x => x.OwnerUserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.UpdatedAtUtc).IsRequired();

            b.HasIndex(x => x.OwnerUserId).HasDatabaseName("IX_Projects_OwnerUserId");
            b.HasIndex(x => new { x.OwnerUserId, x.IsArchived })
             .HasDatabaseName("IX_Projects_OwnerUserId_IsArchived");
        });

        // ---------- Document -------------------------------------------------
        modelBuilder.Entity<Document>(b =>
        {
            b.ToTable("Documents");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(300);
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.UpdatedAtUtc).IsRequired();

            b.HasOne(x => x.Project)
             .WithMany(p => p.Documents)
             .HasForeignKey(x => x.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.CurrentVersion)
             .WithMany()
             .HasForeignKey(x => x.CurrentVersionId)
             // Avoid multiple cascade paths: parent->Versions cascades, this
             // pointer must not.
             .OnDelete(DeleteBehavior.NoAction);

            b.HasIndex(x => x.ProjectId).HasDatabaseName("IX_Documents_ProjectId");
        });

        // ---------- DocumentVersion ------------------------------------------
        modelBuilder.Entity<DocumentVersion>(b =>
        {
            b.ToTable("DocumentVersions");
            b.HasKey(x => x.Id);
            b.Property(x => x.VersionNumber).IsRequired();
            b.Property(x => x.ContentHtml).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.AuthorUserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasOne(x => x.Document)
             .WithMany(d => d.Versions)
             .HasForeignKey(x => x.DocumentId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.DocumentId, x.VersionNumber })
             .IsUnique()
             .HasDatabaseName("UX_DocumentVersions_DocumentId_VersionNumber");
        });

        // ---------- Template -------------------------------------------------
        modelBuilder.Entity<Template>(b =>
        {
            b.ToTable("Templates");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(2000);
            b.Property(x => x.OwnerUserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.UpdatedAtUtc).IsRequired();

            b.HasOne(x => x.CurrentVersion)
             .WithMany()
             .HasForeignKey(x => x.CurrentVersionId)
             .OnDelete(DeleteBehavior.NoAction);

            b.HasIndex(x => x.OwnerUserId).HasDatabaseName("IX_Templates_OwnerUserId");
            b.HasIndex(x => x.IsPublic).HasDatabaseName("IX_Templates_IsPublic");
        });

        // ---------- TemplateVersion ------------------------------------------
        modelBuilder.Entity<TemplateVersion>(b =>
        {
            b.ToTable("TemplateVersions");
            b.HasKey(x => x.Id);
            b.Property(x => x.VersionNumber).IsRequired();
            b.Property(x => x.ContentHtml).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.AuthorUserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasOne(x => x.Template)
             .WithMany(t => t.Versions)
             .HasForeignKey(x => x.TemplateId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TemplateId, x.VersionNumber })
             .IsUnique()
             .HasDatabaseName("UX_TemplateVersions_TemplateId_VersionNumber");
        });

        // ---------- Job ------------------------------------------------------
        modelBuilder.Entity<Job>(b =>
        {
            b.ToTable("Jobs");
            b.HasKey(x => x.Id);
            b.Property(x => x.Provider).IsRequired().HasMaxLength(100);
            b.Property(x => x.RequestPayload).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.ResponsePayload).HasColumnType("nvarchar(max)");
            b.Property(x => x.ErrorMessage).HasMaxLength(4000);
            b.Property(x => x.RequestedByUserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.Status).HasConversion<int>().IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasOne(x => x.Project)
             .WithMany(p => p.Jobs)
             .HasForeignKey(x => x.ProjectId)
             // SetNull keeps audit history when a project is deleted.
             .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(x => x.Template)
             .WithMany(t => t.Jobs)
             .HasForeignKey(x => x.TemplateId)
             .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => new { x.Status, x.CreatedAtUtc })
             .HasDatabaseName("IX_Jobs_Status_CreatedAtUtc");
            b.HasIndex(x => x.RequestedByUserId).HasDatabaseName("IX_Jobs_RequestedByUserId");
        });

        // ---------- ExportArtifact -------------------------------------------
        modelBuilder.Entity<ExportArtifact>(b =>
        {
            b.ToTable("ExportArtifacts");
            b.HasKey(x => x.Id);
            b.Property(x => x.Format).HasConversion<int>().IsRequired();
            b.Property(x => x.StorageUri).IsRequired().HasMaxLength(1000);
            b.Property(x => x.CreatedByUserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasOne(x => x.Document)
             .WithMany(d => d.Exports)
             .HasForeignKey(x => x.DocumentId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.DocumentId).HasDatabaseName("IX_ExportArtifacts_DocumentId");
        });

        // ---------- AuditLog -------------------------------------------------
        modelBuilder.Entity<AuditLog>(b =>
        {
            b.ToTable("AuditLogs");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedOnAdd();
            b.Property(x => x.OccurredAtUtc).IsRequired();
            b.Property(x => x.ActorUserId).HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.Action).IsRequired().HasMaxLength(100);
            b.Property(x => x.EntityType).IsRequired().HasMaxLength(100);
            b.Property(x => x.EntityId).IsRequired().HasMaxLength(100);
            b.Property(x => x.DetailsJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.IpAddress).HasMaxLength(45);
            b.Property(x => x.Severity).HasConversion<int>().IsRequired();

            b.HasIndex(x => x.OccurredAtUtc).HasDatabaseName("IX_AuditLogs_OccurredAtUtc");
            b.HasIndex(x => new { x.EntityType, x.EntityId })
             .HasDatabaseName("IX_AuditLogs_EntityType_EntityId");
            b.HasIndex(x => x.ActorUserId).HasDatabaseName("IX_AuditLogs_ActorUserId");
        });
    }
}
