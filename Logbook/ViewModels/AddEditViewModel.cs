using System.ComponentModel.DataAnnotations;
using Logbook.Models;

namespace Logbook.ViewModels;
/// Presentation-layer model for the Create and Edit forms.
/// Decoupled from the JobApplication domain entity to keep
/// validation attributes out of the domain model.

public class AddEditViewModel
{
    /// Zero for a new entry; the existing record ID when editing.
    public int Id { get; set; }

    [Required(ErrorMessage = "Company name is required.")]
    [MaxLength(200, ErrorMessage = "Company name cannot exceed 200 characters.")]
    [Display(Name = "Company Name")]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role title is required.")]
    [MaxLength(200, ErrorMessage = "Role title cannot exceed 200 characters.")]
    [Display(Name = "Role Title")]
    public string RoleTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date applied is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Date Applied")]
    public DateTime DateApplied { get; set; } = DateTime.Today;

    [MaxLength(100, ErrorMessage = "Source cannot exceed 100 characters.")]
    [Display(Name = "Source")]
    public string? Source { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [Display(Name = "Status")]
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Follow-up Date")]
    public DateTime? FollowUpDate { get; set; }

    /// True when editing an existing record; false for a new entry.
    /// Used by the shared form partial to render context-appropriate labels and button text.
    public bool IsEdit => Id > 0;
}
