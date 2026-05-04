using Logbook.Models;

namespace Logbook.ViewModels
{
    /// Carries dashboard summary data for the selected date range.
    /// Populated by DashboardController from GetByDateRangeAsync results.
    public class DashboardViewModel
    {

        /// Date range for the reporting period

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        /// Total applications within the selected range

        public int TotalApplications { get; set; }

        /// Count per status. Only statuses with results will be present

        public Dictionary<ApplicationStatus, int> StatusBreakdown { get; set; }
            = new Dictionary<ApplicationStatus, int>();


        /// True when no applications exist for the selected range

        public bool HasNoResults => TotalApplications == 0;


        /// Formatted label e.g. "31 Mar 2026 – 06 Apr 2026"

        public string DateRangeLabel =>
            $"{StartDate:dd MMM yyyy} \u2013 {EndDate:dd MMM yyyy}";
    }
}