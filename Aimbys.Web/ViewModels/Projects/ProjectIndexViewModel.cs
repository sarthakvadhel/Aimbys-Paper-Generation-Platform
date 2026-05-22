namespace Aimbys.Web.ViewModels.Projects;

public class ProjectIndexViewModel
{
    /// <summary>Search term as submitted by the user; echoed back in the form.</summary>
    public string? Query { get; set; }

    /// <summary>Whether archived projects are included in the listing.</summary>
    public bool IncludeArchived { get; set; }

    public IReadOnlyList<ProjectListItemViewModel> Projects { get; set; }
        = Array.Empty<ProjectListItemViewModel>();

    public int TotalCount { get; set; }
}
