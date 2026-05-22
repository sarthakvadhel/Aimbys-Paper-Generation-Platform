namespace Aimbys.Web.ViewModels.Projects;

public class ProjectDetailsViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>Number of documents under this project (set by controller).</summary>
    public int DocumentCount { get; set; }
}
