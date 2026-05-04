using System.ComponentModel.DataAnnotations;

namespace Logbook.Models;
/// Domain entity representing a single job application entry.
public class JobApplication
{
    /// Auto-incremented primary key.
    public int Id { get; set; }

    /// Name of the hiring company. Required, max 200 characters.
    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;


    /// Job title of the position applied for. Required, max 200 characters.
    [Required]
    [MaxLength(200)]
    public string RoleTitle { get; set; } = string.Empty;

    /// Date the application was submitted.
    [Required]
    public DateTime DateApplied { get; set; }

    /// Platform or channel through which the listing was discovered e.g. LinkedIn, Indeed.
    /// Optional, max 100 characters.
    [MaxLength(100)]
    public string? Source { get; set; }

    /// Current stage of the application in the hiring process.
    /// Stored as a human-readable string in the database via HasConversion.
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

    /// Free-text annotations recorded by the jobseeker. Optional.
    public string? Notes { get; set; }

    /// Optional reminder date to follow up on this application.
    /// Displayed with Overdue and Due Today indicators in the list and detail views.
    [DataType(DataType.Date)]
    [Display(Name = "Follow-up Date")]
    public DateTime? FollowUpDate { get; set; }

    /// UTC timestamp set by the service layer when the entry is first created.
    /// Not set by the database engine to ensure consistent timezone handling.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// UTC timestamp updated by the service layer on every edit.
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
