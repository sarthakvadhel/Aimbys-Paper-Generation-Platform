using Aimbys.Domain.Entities;
using Aimbys.Domain.Entities.Audit;
using Aimbys.Domain.Entities.Configuration;
using Aimbys.Domain.Entities.Multilingual;
using Aimbys.Domain.Entities.Notifications;
using Aimbys.Domain.Entities.Retention;
using Aimbys.Domain.Entities.Scheduling;
using Aimbys.Domain.Entities.Workflow;
using Aimbys.Domain.SoftDelete;
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
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<FileAsset> FileAssets => Set<FileAsset>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // ----- Workflow engine (Chunk 11) -----------------------------------
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    public DbSet<ApprovalQueue> ApprovalQueues => Set<ApprovalQueue>();
    public DbSet<TaskAssignment> TaskAssignments => Set<TaskAssignment>();
    public DbSet<ReviewerAssignment> ReviewerAssignments => Set<ReviewerAssignment>();
    public DbSet<ModerationQueue> ModerationQueues => Set<ModerationQueue>();
    public DbSet<WorkflowEscalationRule> WorkflowEscalationRules => Set<WorkflowEscalationRule>();
    public DbSet<WorkflowDeadline> WorkflowDeadlines => Set<WorkflowDeadline>();
    public DbSet<WorkflowReminder> WorkflowReminders => Set<WorkflowReminder>();

    // ----- Enterprise infrastructure (Chunk 12) -------------------------
    public DbSet<ScheduledJob> ScheduledJobs => Set<ScheduledJob>();
    public DbSet<PlatformSetting> PlatformSettings => Set<PlatformSetting>();
    public DbSet<InstituteSetting> InstituteSettings => Set<InstituteSetting>();
    public DbSet<FeatureToggle> FeatureToggles => Set<FeatureToggle>();
    public DbSet<RetentionPolicy> RetentionPolicies => Set<RetentionPolicy>();
    public DbSet<ArchivePolicy> ArchivePolicies => Set<ArchivePolicy>();

    // ----- Org tree (Chunk 18) ---------------------------------------------
    public DbSet<Domain.Entities.Stream> Streams => Set<Domain.Entities.Stream>();
    public DbSet<Major> Majors => Set<Major>();
    public DbSet<Chapter> Chapters => Set<Chapter>();

    // ----- Notification hardening + audit visibility (Chunk 13) ----------
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<NotificationTemplateTranslation> NotificationTemplateTranslations
        => Set<NotificationTemplateTranslation>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<NotificationChannelConfig> NotificationChannelConfigs
        => Set<NotificationChannelConfig>();
    public DbSet<AuditVisibilityRule> AuditVisibilityRules => Set<AuditVisibilityRule>();

    // ----- Multilingual (Chunk 32) ------------------------------------------
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<QuestionTranslation> QuestionTranslations => Set<QuestionTranslation>();
    public DbSet<PaperLanguageSet> PaperLanguageSets => Set<PaperLanguageSet>();

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

        // ---------- Notification --------------------------------------------
        modelBuilder.Entity<Notification>(b =>
        {
            b.ToTable("Notifications");
            b.HasKey(x => x.Id);

            b.Property(x => x.RecipientUserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Body).HasMaxLength(2000);
            b.Property(x => x.Severity).HasConversion<int>().IsRequired();
            b.Property(x => x.RouteUrl).HasMaxLength(500);
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasIndex(x => new { x.RecipientUserId, x.IsRead, x.CreatedAtUtc })
             .HasDatabaseName("IX_Notifications_RecipientUserId_IsRead_CreatedAtUtc");
            b.HasIndex(x => x.InstituteId)
             .HasDatabaseName("IX_Notifications_InstituteId");
        });

        // ---------- WorkflowDefinition (Chunk 11) ---------------------------
        modelBuilder.Entity<WorkflowDefinition>(b =>
        {
            b.ToTable("WorkflowDefinitions");
            b.HasKey(x => x.Id);

            b.Property(x => x.Key).IsRequired().HasMaxLength(100);
            b.Property(x => x.Version).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.StatesJson).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.TransitionsJson).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.EscalationRulesJson).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasIndex(x => new { x.Key, x.Version })
             .IsUnique()
             .HasDatabaseName("UX_WorkflowDefinitions_Key_Version");
        });

        // ---------- WorkflowInstance ----------------------------------------
        modelBuilder.Entity<WorkflowInstance>(b =>
        {
            b.ToTable("WorkflowInstances");
            b.HasKey(x => x.Id);

            b.Property(x => x.DefinitionKey).IsRequired().HasMaxLength(100);
            b.Property(x => x.DefinitionVersion).IsRequired();
            b.Property(x => x.SubjectType).IsRequired().HasMaxLength(100);
            b.Property(x => x.CurrentState).IsRequired().HasMaxLength(100);
            b.Property(x => x.StartedByUserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.StartedAtUtc).IsRequired();

            b.HasMany(x => x.Transitions)
             .WithOne(t => t.Instance)
             .HasForeignKey(t => t.InstanceId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.DefinitionKey, x.SubjectType, x.SubjectId })
             .HasDatabaseName("IX_WorkflowInstances_DefinitionKey_SubjectType_SubjectId");
            b.HasIndex(x => new { x.SubjectType, x.SubjectId })
             .HasDatabaseName("IX_WorkflowInstances_SubjectType_SubjectId");
            b.HasIndex(x => new { x.IsCompleted, x.CurrentState })
             .HasDatabaseName("IX_WorkflowInstances_IsCompleted_CurrentState");
            b.HasIndex(x => x.InstituteId)
             .HasDatabaseName("IX_WorkflowInstances_InstituteId");
        });

        // ---------- WorkflowTransition (append-only history) ----------------
        modelBuilder.Entity<WorkflowTransition>(b =>
        {
            b.ToTable("WorkflowTransitions");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedOnAdd();

            b.Property(x => x.InstanceId).IsRequired();
            b.Property(x => x.FromState).IsRequired().HasMaxLength(100);
            b.Property(x => x.ToState).IsRequired().HasMaxLength(100);
            b.Property(x => x.ActorUserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.Comment).HasMaxLength(2000);
            b.Property(x => x.TransitionedAtUtc).IsRequired();

            b.HasIndex(x => new { x.InstanceId, x.TransitionedAtUtc })
             .HasDatabaseName("IX_WorkflowTransitions_InstanceId_TransitionedAtUtc");
        });

        // ---------- ApprovalQueue -------------------------------------------
        modelBuilder.Entity<ApprovalQueue>(b =>
        {
            b.ToTable("ApprovalQueues");
            b.HasKey(x => x.Id);

            b.Property(x => x.InstanceId).IsRequired();
            b.Property(x => x.DefinitionKey).IsRequired().HasMaxLength(100);
            b.Property(x => x.QueueName).IsRequired().HasMaxLength(100);
            b.Property(x => x.AssignedToUserId).HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.Priority).HasConversion<int>().IsRequired();
            b.Property(x => x.EnqueuedAtUtc).IsRequired();

            b.HasOne(x => x.Instance)
             .WithMany()
             .HasForeignKey(x => x.InstanceId)
             .OnDelete(DeleteBehavior.Cascade);

            // Inbox query: open items by (definition, queue, assignee, oldest first).
            b.HasIndex(x => new { x.DefinitionKey, x.QueueName, x.AssignedToUserId, x.EnqueuedAtUtc })
             .HasDatabaseName("IX_ApprovalQueues_DefinitionKey_QueueName_AssignedToUserId_EnqueuedAtUtc");
            b.HasIndex(x => new { x.IsResolved, x.InstituteId })
             .HasDatabaseName("IX_ApprovalQueues_IsResolved_InstituteId");
            b.HasIndex(x => x.InstanceId)
             .HasDatabaseName("IX_ApprovalQueues_InstanceId");
        });

        // ---------- TaskAssignment ------------------------------------------
        modelBuilder.Entity<TaskAssignment>(b =>
        {
            b.ToTable("TaskAssignments");
            b.HasKey(x => x.Id);

            b.Property(x => x.QueueItemId).IsRequired();
            b.Property(x => x.AssignedToUserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.AssignedByUserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.AssignedAtUtc).IsRequired();

            b.HasOne(x => x.QueueItem)
             .WithMany()
             .HasForeignKey(x => x.QueueItemId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.QueueItemId, x.IsActive })
             .HasDatabaseName("IX_TaskAssignments_QueueItemId_IsActive");
            b.HasIndex(x => new { x.AssignedToUserId, x.IsActive })
             .HasDatabaseName("IX_TaskAssignments_AssignedToUserId_IsActive");
        });

        // ---------- ReviewerAssignment --------------------------------------
        modelBuilder.Entity<ReviewerAssignment>(b =>
        {
            b.ToTable("ReviewerAssignments");
            b.HasKey(x => x.Id);

            b.Property(x => x.SubjectType).IsRequired().HasMaxLength(100);
            b.Property(x => x.ReviewerUserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.AssignedByUserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.AssignedAtUtc).IsRequired();

            b.HasIndex(x => new { x.SubjectType, x.SubjectId })
             .HasDatabaseName("IX_ReviewerAssignments_SubjectType_SubjectId");
            b.HasIndex(x => new { x.ReviewerUserId, x.IsActive })
             .HasDatabaseName("IX_ReviewerAssignments_ReviewerUserId_IsActive");
            b.HasIndex(x => x.InstituteId)
             .HasDatabaseName("IX_ReviewerAssignments_InstituteId");
        });

        // ---------- ModerationQueue -----------------------------------------
        modelBuilder.Entity<ModerationQueue>(b =>
        {
            b.ToTable("ModerationQueues");
            b.HasKey(x => x.Id);

            b.Property(x => x.EvaluationId).IsRequired();
            b.Property(x => x.ModeratorUserId).HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.Priority).HasConversion<int>().IsRequired();
            b.Property(x => x.EnqueuedAtUtc).IsRequired();

            b.HasIndex(x => new { x.IsResolved, x.ModeratorUserId, x.EnqueuedAtUtc })
             .HasDatabaseName("IX_ModerationQueues_IsResolved_ModeratorUserId_EnqueuedAtUtc");
            b.HasIndex(x => x.InstituteId)
             .HasDatabaseName("IX_ModerationQueues_InstituteId");
            b.HasIndex(x => x.EvaluationId)
             .HasDatabaseName("IX_ModerationQueues_EvaluationId");
        });

        // ---------- WorkflowEscalationRule ----------------------------------
        modelBuilder.Entity<WorkflowEscalationRule>(b =>
        {
            b.ToTable("WorkflowEscalationRules");
            b.HasKey(x => x.Id);

            b.Property(x => x.DefinitionKey).IsRequired().HasMaxLength(100);
            b.Property(x => x.State).IsRequired().HasMaxLength(100);
            b.Property(x => x.MaxDurationMinutes).IsRequired();
            b.Property(x => x.ReminderAtPercent).IsRequired();
            b.Property(x => x.EscalateToRole).HasMaxLength(100);
            b.Property(x => x.EscalateToPermission).HasMaxLength(100);

            b.HasIndex(x => new { x.DefinitionKey, x.State })
             .IsUnique()
             .HasDatabaseName("UX_WorkflowEscalationRules_DefinitionKey_State");
        });

        // ---------- WorkflowDeadline ----------------------------------------
        modelBuilder.Entity<WorkflowDeadline>(b =>
        {
            b.ToTable("WorkflowDeadlines");
            b.HasKey(x => x.Id);

            b.Property(x => x.InstanceId).IsRequired();
            b.Property(x => x.State).IsRequired().HasMaxLength(100);
            b.Property(x => x.DueAtUtc).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();

            // The escalation job's hot path: scan rows that are
            // still active and might be due.
            b.HasIndex(x => new { x.IsResolved, x.IsOverdue, x.DueAtUtc })
             .HasDatabaseName("IX_WorkflowDeadlines_IsResolved_IsOverdue_DueAtUtc");
            b.HasIndex(x => x.InstanceId)
             .HasDatabaseName("IX_WorkflowDeadlines_InstanceId");
            b.HasIndex(x => x.InstituteId)
             .HasDatabaseName("IX_WorkflowDeadlines_InstituteId");
        });

        // ---------- WorkflowReminder (audit row per dispatch) ---------------
        modelBuilder.Entity<WorkflowReminder>(b =>
        {
            b.ToTable("WorkflowReminders");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedOnAdd();

            b.Property(x => x.DeadlineId).IsRequired();
            b.Property(x => x.RecipientUserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.SentAtUtc).IsRequired();
            b.Property(x => x.Channel).HasConversion<int>().IsRequired();

            b.HasIndex(x => new { x.DeadlineId, x.SentAtUtc })
             .HasDatabaseName("IX_WorkflowReminders_DeadlineId_SentAtUtc");
            b.HasIndex(x => x.RecipientUserId)
             .HasDatabaseName("IX_WorkflowReminders_RecipientUserId");
        });

        // ===== Chunk 12 — enterprise infrastructure ==========================

        // ---------- ScheduledJob --------------------------------------------
        modelBuilder.Entity<ScheduledJob>(b =>
        {
            b.ToTable("ScheduledJobs");
            b.HasKey(x => x.Id);

            b.Property(x => x.JobKey).IsRequired().HasMaxLength(120);
            b.Property(x => x.CronExpression).HasMaxLength(120);
            b.Property(x => x.Payload).HasColumnType("nvarchar(max)");
            b.Property(x => x.Status).HasConversion<int>().IsRequired();
            b.Property(x => x.NextRunAtUtc).IsRequired();
            b.Property(x => x.MaxRetries).IsRequired();
            b.Property(x => x.RetryCount).IsRequired();
            b.Property(x => x.LastError).HasMaxLength(500);

            // Hot path: dispatcher polls (Status, NextRunAtUtc).
            b.HasIndex(x => new { x.Status, x.NextRunAtUtc })
             .HasDatabaseName("IX_ScheduledJobs_Status_NextRunAtUtc");
            b.HasIndex(x => x.JobKey)
             .HasDatabaseName("IX_ScheduledJobs_JobKey");
        });

        // ---------- PlatformSetting -----------------------------------------
        modelBuilder.Entity<PlatformSetting>(b =>
        {
            b.ToTable("PlatformSettings");
            b.HasKey(x => x.Id);

            b.Property(x => x.Key).IsRequired().HasMaxLength(200);
            b.Property(x => x.ValueJson).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.UpdatedByUserId).HasMaxLength(IdentityUserIdLength);

            b.HasIndex(x => x.Key)
             .IsUnique()
             .HasDatabaseName("UX_PlatformSettings_Key");
        });

        // ---------- InstituteSetting ----------------------------------------
        modelBuilder.Entity<InstituteSetting>(b =>
        {
            b.ToTable("InstituteSettings");
            b.HasKey(x => x.Id);

            b.Property(x => x.Key).IsRequired().HasMaxLength(200);
            b.Property(x => x.ValueJson).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.UpdatedByUserId).HasMaxLength(IdentityUserIdLength);

            b.HasOne(x => x.Institute)
             .WithMany()
             .HasForeignKey(x => x.InstituteId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.InstituteId, x.Key })
             .IsUnique()
             .HasDatabaseName("UX_InstituteSettings_InstituteId_Key");
        });

        // ---------- FeatureToggle -------------------------------------------
        modelBuilder.Entity<FeatureToggle>(b =>
        {
            b.ToTable("FeatureToggles");
            b.HasKey(x => x.Id);

            b.Property(x => x.Key).IsRequired().HasMaxLength(200);
            b.Property(x => x.InstituteOverridesJson).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.UpdatedByUserId).HasMaxLength(IdentityUserIdLength);

            b.HasIndex(x => x.Key)
             .IsUnique()
             .HasDatabaseName("UX_FeatureToggles_Key");
        });

        // ---------- RetentionPolicy -----------------------------------------
        modelBuilder.Entity<RetentionPolicy>(b =>
        {
            b.ToTable("RetentionPolicies");
            b.HasKey(x => x.Id);

            b.Property(x => x.EntityType).IsRequired().HasMaxLength(120);
            b.Property(x => x.RetentionDays).IsRequired();
            b.Property(x => x.ArchiveAfterDays).IsRequired();
            b.Property(x => x.LegalHold).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000);

            b.HasIndex(x => x.EntityType)
             .IsUnique()
             .HasDatabaseName("UX_RetentionPolicies_EntityType");
        });

        // ---------- ArchivePolicy -------------------------------------------
        modelBuilder.Entity<ArchivePolicy>(b =>
        {
            b.ToTable("ArchivePolicies");
            b.HasKey(x => x.Id);

            b.Property(x => x.EntityType).IsRequired().HasMaxLength(120);
            b.Property(x => x.Strategy).HasConversion<int>().IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000);

            b.HasIndex(x => x.EntityType)
             .IsUnique()
             .HasDatabaseName("UX_ArchivePolicies_EntityType");
        });

        // ---------- Institute (Chunk 12 columns) ----------------------------
        // The base Institute mapping above (Chunk 1) already configures the
        // table; here we add the subscription-lifecycle columns shipped by
        // Chunk 12. Splitting the calls keeps the diff localised.
        modelBuilder.Entity<Institute>(b =>
        {
            b.Property(x => x.SubscriptionStatus).HasConversion<int>().IsRequired();
            b.Property(x => x.DeletedByUserId).HasMaxLength(IdentityUserIdLength);

            b.HasIndex(x => x.SubscriptionStatus)
             .HasDatabaseName("IX_Institutes_SubscriptionStatus");
        });

        // ---------- FileAsset (Chunk 12 column) -----------------------------
        modelBuilder.Entity<FileAsset>(b =>
        {
            b.Property(x => x.DeletedByUserId).HasMaxLength(IdentityUserIdLength);
        });

        // ===== Chunk 13 — notification hardening + audit visibility =========

        // ---------- NotificationTemplate ------------------------------------
        modelBuilder.Entity<NotificationTemplate>(b =>
        {
            b.ToTable("NotificationTemplates");
            b.HasKey(x => x.Id);

            b.Property(x => x.Key).IsRequired().HasMaxLength(120);
            b.Property(x => x.TitleTemplate).IsRequired().HasMaxLength(500);
            b.Property(x => x.BodyTemplate).HasColumnType("nvarchar(max)");
            b.Property(x => x.DefaultRoutePattern).HasMaxLength(500);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.DefaultSeverity).HasConversion<int>().IsRequired();
            b.Property(x => x.IsActive).IsRequired();

            b.HasMany(x => x.Translations)
             .WithOne(t => t.Template)
             .HasForeignKey(t => t.TemplateId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.Key)
             .IsUnique()
             .HasDatabaseName("UX_NotificationTemplates_Key");
        });

        // ---------- NotificationTemplateTranslation -------------------------
        modelBuilder.Entity<NotificationTemplateTranslation>(b =>
        {
            b.ToTable("NotificationTemplateTranslations");
            b.HasKey(x => x.Id);

            b.Property(x => x.LanguageCode).IsRequired().HasMaxLength(16);
            b.Property(x => x.TitleTemplate).IsRequired().HasMaxLength(500);
            b.Property(x => x.BodyTemplate).HasColumnType("nvarchar(max)");

            b.HasIndex(x => new { x.TemplateId, x.LanguageCode })
             .IsUnique()
             .HasDatabaseName("UX_NotificationTemplateTranslations_TemplateId_LanguageCode");
        });

        // ---------- NotificationPreference ----------------------------------
        modelBuilder.Entity<NotificationPreference>(b =>
        {
            b.ToTable("NotificationPreferences");
            b.HasKey(x => x.Id);

            b.Property(x => x.UserId).IsRequired().HasMaxLength(IdentityUserIdLength);
            b.Property(x => x.ChannelKey).IsRequired().HasMaxLength(40);
            b.Property(x => x.MinimumSeverity).HasConversion<int>().IsRequired();
            b.Property(x => x.IsEnabled).IsRequired();

            // Hot path for the dispatcher: lookup by (UserId, ChannelKey).
            b.HasIndex(x => new { x.UserId, x.ChannelKey })
             .IsUnique()
             .HasDatabaseName("UX_NotificationPreferences_UserId_ChannelKey");
        });

        // ---------- NotificationChannelConfig -------------------------------
        modelBuilder.Entity<NotificationChannelConfig>(b =>
        {
            b.ToTable("NotificationChannelConfigs");
            b.HasKey(x => x.Id);

            b.Property(x => x.ChannelKey).IsRequired().HasMaxLength(40);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(120);
            b.Property(x => x.ConfigJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.IsEnabled).IsRequired();

            b.HasIndex(x => x.ChannelKey)
             .IsUnique()
             .HasDatabaseName("UX_NotificationChannelConfigs_ChannelKey");
        });

        // ---------- AuditVisibilityRule -------------------------------------
        modelBuilder.Entity<AuditVisibilityRule>(b =>
        {
            b.ToTable("AuditVisibilityRules");
            b.HasKey(x => x.Id);

            b.Property(x => x.ActionPattern).IsRequired().HasMaxLength(120);
            b.Property(x => x.VisibleToRolesJson).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.VisibleToPermissionsJson).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.MaskFieldsJson).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.RequiresComplianceMode).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000);

            b.HasIndex(x => x.ActionPattern)
             .IsUnique()
             .HasDatabaseName("UX_AuditVisibilityRules_ActionPattern");
        });

        // ===== Chunk 18 — institute org tree ===================================

        // ---------- Stream (Chunk 18) -------------------------------------------
        modelBuilder.Entity<Domain.Entities.Stream>(b =>
        {
            b.ToTable("Streams");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(120);
            b.Property(x => x.Description).HasMaxLength(500);
            b.HasOne(x => x.Institute).WithMany(i => i.Streams).HasForeignKey(x => x.InstituteId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Majors).WithOne(m => m.Stream).HasForeignKey(m => m.StreamId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.InstituteId, x.Name }).IsUnique().HasFilter("[IsDeleted] = 0").HasDatabaseName("UX_Streams_InstituteId_Name");
        });

        // ---------- Major (Chunk 18) -------------------------------------------
        modelBuilder.Entity<Major>(b =>
        {
            b.ToTable("Majors");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(500);
            b.HasOne(x => x.Institute).WithMany(i => i.Majors).HasForeignKey(x => x.InstituteId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => new { x.InstituteId, x.StreamId, x.Name }).IsUnique().HasFilter("[IsDeleted] = 0").HasDatabaseName("UX_Majors_InstituteId_StreamId_Name");
        });

        // ---------- Chapter (Chunk 18) ------------------------------------------
        modelBuilder.Entity<Chapter>(b =>
        {
            b.ToTable("Chapters");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.SortOrder).IsRequired();
            b.HasOne(x => x.Subject).WithMany(s => s.Chapters).HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.SubjectId, x.SortOrder }).IsUnique().HasFilter("[IsDeleted] = 0").HasDatabaseName("UX_Chapters_SubjectId_SortOrder");
        });

        // ===== Chunk 32 — multilingual content ==================================

        // ---------- Language (Chunk 32) -----------------------------------------
        modelBuilder.Entity<Language>(b =>
        {
            b.ToTable("Languages");
            b.HasKey(x => x.Id);
            b.Property(x => x.Code).IsRequired().HasMaxLength(16);
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.HasIndex(x => new { x.InstituteId, x.Code }).IsUnique().HasDatabaseName("UX_Languages_InstituteId_Code");
        });

        // ---------- QuestionTranslation (Chunk 32) ------------------------------
        modelBuilder.Entity<QuestionTranslation>(b =>
        {
            b.ToTable("QuestionTranslations");
            b.HasKey(x => x.Id);
            b.Property(x => x.BodyHtml).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(x => x.InstructionsHtml).HasColumnType("nvarchar(max)");
            b.Property(x => x.OptionsJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.Status).HasConversion<int>().IsRequired();
            b.Property(x => x.TranslatorUserId).IsRequired().HasMaxLength(450);
            b.HasIndex(x => new { x.QuestionVersionId, x.LanguageId }).IsUnique().HasDatabaseName("UX_QuestionTranslations_QuestionVersionId_LanguageId");
        });

        // ---------- PaperLanguageSet (Chunk 32) ---------------------------------
        modelBuilder.Entity<PaperLanguageSet>(b =>
        {
            b.ToTable("PaperLanguageSets");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.PaperVersionId, x.LanguageId }).IsUnique().HasDatabaseName("UX_PaperLanguageSets_PaperVersionId_LanguageId");
        });

        // ===== Soft-delete global query filter convention ==================
        //
        // Every entity that implements ISoftDelete gets the same filter:
        // queries return only live rows by default, and code that needs to
        // see soft-deleted rows opts in with .IgnoreQueryFilters(). Doing
        // it here (after individual configs) means new soft-deletable
        // entities pick up the filter for free — no per-entity
        // boilerplate to remember.
        ApplySoftDeleteQueryFilters(modelBuilder);
    }

    /// <summary>
    /// Applies <c>e =&gt; !e.IsDeleted</c> as a global query filter to
    /// every entity type registered with the model that implements
    /// <see cref="ISoftDelete"/>. Uses <c>EF.Property&lt;bool&gt;</c> so
    /// the filter expression doesn't depend on a specific generic
    /// closure for each entity.
    /// </summary>
    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            // Build: x => !EF.Property<bool>(x, "IsDeleted")
            var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "x");
            var efProperty = System.Linq.Expressions.Expression.Call(
                method: typeof(Microsoft.EntityFrameworkCore.EF)
                    .GetMethod(nameof(Microsoft.EntityFrameworkCore.EF.Property))!
                    .MakeGenericMethod(typeof(bool)),
                arg0: parameter,
                arg1: System.Linq.Expressions.Expression.Constant("IsDeleted"));
            var notDeleted = System.Linq.Expressions.Expression.Not(efProperty);
            var filter = System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }
}
