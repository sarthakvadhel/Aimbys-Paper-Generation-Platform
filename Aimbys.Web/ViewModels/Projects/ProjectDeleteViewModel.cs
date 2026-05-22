namespace Aimbys.Web.ViewModels.Projects;

public class ProjectDeleteViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public int DocumentCount { get; set; }
}
