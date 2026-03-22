using System.ComponentModel.DataAnnotations;
using Logbook.Models;

namespace Logbook.ViewModels;

public class AddEditViewModel
{
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

    // Determines whether the form renders in Create or Edit mode
    public bool IsEdit => Id > 0;
}
