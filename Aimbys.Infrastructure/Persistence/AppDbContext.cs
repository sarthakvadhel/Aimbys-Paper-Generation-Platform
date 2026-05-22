using Aimbys.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aimbys.Infrastructure.Persistence;

/// <summary>
/// Application <see cref="DbContext"/> for the Aimbys platform.
///
/// Schema scope at this stage: the <b>organisational foundation</b>
/// (Institute, Department, AcademicYear, Subject, ClassBatch,
/// TeacherProfile, StudentProfile) plus the existing <see cref="AuditLog"/>
/// and ASP.NET Identity tables. Workflow entities &mdash; Blueprint,
/// AssessmentDesign, Question, Paper, Exam, Evaluation, Moderation,
/// Analytics &mdash; arrive in subsequent chunks (NEW CHUNK 5+) so the
/// data model and the controllers / views ship together.
///
/// All EF mapping lives in <see cref="OnModelCreating"/> so domain
/// entities (under <c>Aimbys.Domain</c>) stay free of EF Core attributes.
/// </summary>
public class AppDbContext : IdentityDbContext<IdentityUser>
{
    /// <summary>
    /// Storage column length used for ASP.NET Identity user ids. Matches the
    /// <c>IdentityUser.Id</c> default of <c>nvarchar(450)</c>.
    /// </summary>
    public const int IdentityUserIdLength = 450;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Institute> Institutes => Set<Institute>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<ClassBatch> ClassBatches => Set<ClassBatch>();
    public DbSet<TeacherProfile> TeacherProfiles => Set<TeacherProfile>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<FileAsset> FileAssets => Set<FileAsset>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configures the AspNet* Identity tables first.
        base.OnModelCreating(modelBuilder);

        // ---------- Institute -----------------------------------------------
        modelBuilder.Entity<Institute>(b =>
        {
            b.ToTable("Institutes");
            b.HasKey(x => x.Id);

            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Code).IsRequired().HasMaxLength(50);
            b.Property(x => x.City).IsRequired().HasMaxLength(120);
            b.Property(x => x.State).IsRequired().HasMaxLength(120);
            b.Property(x => x.Country).IsRequired().HasMaxLength(2);
            b.Property(x => x.ContactEmail).IsRequired().HasMaxLength(256);
            b.Property(x => x.ContactPhone).HasMaxLength(32);
            b.Property(x => x.LogoUrl).HasMaxLength(1000);
            b.Property(x => x.PrimaryColorHex).HasMaxLength(7);
            b.Property(x => x.ApprovedByUserId).HasMaxLength(IdentityUserIdLength);

            b.Property(x => x.Type).HasConversion<int>().IsRequired();
            b.Property(x => x.Status).HasConversion<int>().IsRequired();
            b.Property(x => x.LicenseTier).HasConversion<int>().IsRequired();

            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.UpdatedAtUtc).IsRequired();

            // Globally unique short code. Filtered to non-deleted rows so a
            // rejected onboarding attempt's code can be reused.
            b.HasIndex(x => x.Code)
             .IsUnique()
             .HasFilter("[IsDeleted] = 0")
             .HasDatabaseName("UX_Institutes_Code");

            b.HasIndex(x => x.Status).HasDatabaseName("IX_Institutes_Status");
            b.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_Institutes_IsDeleted");
        });

        // ---------- Department ----------------------------------------------
        modelBuilder.Entity<Department>(b =>
        {
            b.ToTable("Departments");
            b.HasKey(x => x.Id);

            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Code).HasMaxLength(50);
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.UpdatedAtUtc).IsRequired();

            b.HasOne(x => x.Institute)
             .WithMany(i => i.Departments)
             .HasForeignKey(x => x.InstituteId)
             // Restrict so a tenant can never be deleted accidentally and
             // also so the multi-cascade-path warning (Department->Subject and
             // Institute->Subject both cascade) doesn't apply.
             .OnDelete(DeleteBehavior.Restrict);

            // The HoD pointer is NoAction to avoid a cycle between
            // Department, TeacherProfile and Institute.
            b.HasOne(x => x.HeadTeacher)
             .WithMany()
             .HasForeignKey(x => x.HeadTeacherProfileId)
             .OnDelete(DeleteBehavior.NoAction);

            b.HasIndex(x => x.InstituteId).HasDatabaseName("IX_Departments_InstituteId");
            b.HasIndex(x => new { x.InstituteId, x.Code })
             .IsUnique()
             .HasFilter("[Code] IS NOT NULL")
             .HasDatabaseName("UX_Departments_InstituteId_Code");
        });

        // ---------- AcademicYear --------------------------------------------
        modelBuilder.Entity<AcademicYear>(b =>
        {
            b.ToTable("AcademicYears");
            b.HasKey(x => x.Id);

            b.Property(x => x.Name).IsRequired().HasMaxLength(50);
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.UpdatedAtUtc).IsRequired();

            b.HasOne(x => x.Institute)
             .WithMany(i => i.AcademicYears)
             .HasForeignKey(x => x.InstituteId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.InstituteId).HasDatabaseName("IX_AcademicYears_InstituteId");
            b.HasIndex(x => new { x.InstituteId, x.Name })
             .IsUnique()
             .HasDatabaseName("UX_AcademicYears_InstituteId_Name");
            b.HasIndex(x => new { x.InstituteId, x.IsCurrent })
             .HasDatabaseName("IX_AcademicYears_InstituteId_IsCurrent");
        });

        // ---------- Subject -------------------------------------------------
        modelBuilder.Entity<Subject>(b =>
        {
            b.ToTable("Subjects");
            b.HasKey(x => x.Id);

            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Code).HasMaxLength(50);
            b.Property(x => x.Description).HasMaxLength(2000);
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.UpdatedAtUtc).IsRequired();

            b.HasOne(x => x.Institute)
             .WithMany(i => i.Subjects)
             .HasForeignKey(x => x.InstituteId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Department)
             .WithMany(d => d.Subjects)
             .HasForeignKey(x => x.DepartmentId)
             .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => x.InstituteId).HasDatabaseName("IX_Subjects_InstituteId");
            b.HasIndex(x => new { x.InstituteId, x.Code })
             .IsUnique()
             .HasFilter("[Code] IS NOT NULL")
             .HasDatabaseName("UX_Subjects_InstituteId_Code");
        });

        // ---------- ClassBatch ---------------------------------------------
        modelBuilder.Entity<ClassBatch>(b =>
        {
            b.ToTable("ClassBatches");
            b.HasKey(x => x.Id);

            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.GradeLevel).HasMaxLength(40);
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.UpdatedAtUtc).IsRequired();

            b.HasOne(x => x.Institute)
             .WithMany(i => i.ClassBatches)
             .HasForeignKey(x => x.InstituteId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.AcademicYear)
             .WithMany(y => y.ClassBatches)
             .HasForeignKey(x => x.AcademicYearId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Department)
             .WithMany(d => d.ClassBatches)
             .HasForeignKey(x => x.DepartmentId)
             .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(x => x.ClassTeacher)
             .WithMany()
             .HasForeignKey(x => x.ClassTeacherProfileId)
             .OnDelete(DeleteBehavior.NoAction);

            b.HasIndex(x => x.InstituteId).HasDatabaseName("IX_ClassBatches_InstituteId");
            b.HasIndex(x => new { x.InstituteId, x.AcademicYearId, x.Name })
             .IsUnique()
             .HasDatabaseName("UX_ClassBatches_InstituteId_AcademicYearId_Name");
        });

        // ---------- TeacherProfile -----------------------------------------
        modelBuilder.Entity<TeacherProfile>(b =>
        {
            b.ToTable("TeacherProfiles");
            b.HasKey(x => x.Id);

            b.Property(x => x.UserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
            b.Property(x => x.EmployeeCode).HasMaxLength(50);
            b.Property(x => x.Designation).HasMaxLength(100);
            b.Property(x => x.Status).HasConversion<int>().IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.UpdatedAtUtc).IsRequired();

            b.HasOne(x => x.Institute)
             .WithMany(i => i.Teachers)
             .HasForeignKey(x => x.InstituteId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Department)
             .WithMany(d => d.Teachers)
             .HasForeignKey(x => x.DepartmentId)
             .OnDelete(DeleteBehavior.SetNull);

            // FK to AspNetUsers via UserId. Restrict so deleting an Identity
            // user can't silently orphan institute records.
            b.HasOne<IdentityUser>()
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.UserId)
             .IsUnique()
             .HasDatabaseName("UX_TeacherProfiles_UserId");
            b.HasIndex(x => x.InstituteId).HasDatabaseName("IX_TeacherProfiles_InstituteId");
            b.HasIndex(x => new { x.InstituteId, x.EmployeeCode })
             .IsUnique()
             .HasFilter("[EmployeeCode] IS NOT NULL")
             .HasDatabaseName("UX_TeacherProfiles_InstituteId_EmployeeCode");
        });

        // ---------- StudentProfile -----------------------------------------
        modelBuilder.Entity<StudentProfile>(b =>
        {
            b.ToTable("StudentProfiles");
            b.HasKey(x => x.Id);

            b.Property(x => x.UserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
            b.Property(x => x.AdmissionNumber).HasMaxLength(50);
            b.Property(x => x.RollNumber).HasMaxLength(50);
            b.Property(x => x.Status).HasConversion<int>().IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.UpdatedAtUtc).IsRequired();

            b.HasOne(x => x.Institute)
             .WithMany(i => i.Students)
             .HasForeignKey(x => x.InstituteId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.ClassBatch)
             .WithMany(c => c.Students)
             .HasForeignKey(x => x.ClassBatchId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<IdentityUser>()
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.UserId)
             .IsUnique()
             .HasDatabaseName("UX_StudentProfiles_UserId");
            b.HasIndex(x => new { x.InstituteId, x.ClassBatchId })
             .HasDatabaseName("IX_StudentProfiles_InstituteId_ClassBatchId");
            b.HasIndex(x => new { x.InstituteId, x.AdmissionNumber })
             .IsUnique()
             .HasFilter("[AdmissionNumber] IS NOT NULL")
             .HasDatabaseName("UX_StudentProfiles_InstituteId_AdmissionNumber");
        });

        // ---------- FileAsset ----------------------------------------------
        modelBuilder.Entity<FileAsset>(b =>
        {
            b.ToTable("FileAssets");
            b.HasKey(x => x.Id);

            b.Property(x => x.Token).IsRequired();
            b.Property(x => x.Area).HasConversion<int>().IsRequired();
            b.Property(x => x.OwnerKey).IsRequired().HasMaxLength(200);
            b.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(255);
            b.Property(x => x.StoredFileName).IsRequired().HasMaxLength(120);
            b.Property(x => x.ContentType).IsRequired().HasMaxLength(200);
            b.Property(x => x.SizeBytes).IsRequired();
            b.Property(x => x.Sha256).IsRequired().HasMaxLength(64);
            b.Property(x => x.UploadedByUserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.CreatedAtUtc).IsRequired();

            // FK to AspNetUsers via UploadedByUserId. Restrict so deleting
            // an Identity user can't silently orphan upload history.
            b.HasOne<IdentityUser>()
             .WithMany()
             .HasForeignKey(x => x.UploadedByUserId)
             .OnDelete(DeleteBehavior.Restrict);

            // Token must be unique among non-deleted rows; soft-deleted
            // rows are excluded so a token can be re-used after deletion.
            b.HasIndex(x => x.Token)
             .IsUnique()
             .HasFilter("[IsDeleted] = 0")
             .HasDatabaseName("UX_FileAssets_Token");

            b.HasIndex(x => new { x.Area, x.OwnerKey })
             .HasDatabaseName("IX_FileAssets_Area_OwnerKey");
            b.HasIndex(x => x.UploadedByUserId)
             .HasDatabaseName("IX_FileAssets_UploadedByUserId");
            b.HasIndex(x => x.InstituteId)
             .HasDatabaseName("IX_FileAssets_InstituteId");
        });

        // ---------- AuditLog ------------------------------------------------
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
