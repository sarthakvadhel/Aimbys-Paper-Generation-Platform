using System.ComponentModel.DataAnnotations;

namespace Aimbys.Web.ViewModels.Projects;

/// <summary>
/// Used by both Create and Edit views. <see cref="Id"/> is empty for create
/// and bound for edit.
/// </summary>
public class ProjectFormViewModel
{
    public Guid Id { get; set; }

    [Required, StringLength(200, MinimumLength = 1, ErrorMessage = "{0} must be 1-200 characters.")]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "{0} must be 2000 characters or fewer.")]
    [Display(Name = "Description")]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    [Display(Name = "Archived")]
    public bool IsArchived { get; set; }

    /// <summary>
    /// True when rendering the Edit form, false for Create. Drives the
    /// "Archived" checkbox visibility and the submit button label.
    /// </summary>
    public bool IsEdit { get; set; }
}
