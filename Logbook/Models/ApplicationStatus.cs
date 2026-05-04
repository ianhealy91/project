namespace Logbook.Models;

/// Defines the fixed set of stages a job application can progress through.
/// Values are stored as strings in the database rather than integers
/// to improve readability of raw records.
public enum ApplicationStatus
{
    /// Application has been submitted but no response received.
    Applied,
    /// An interview has been scheduled with the company.
    InterviewScheduled,
    /// An offer has been received from the company.
    OfferReceived,
    /// The application has been rejected by the company.
    Rejected,
    /// The application has been withdrawn by the applicant.
    Withdrawn
}
