// TFMS.ViewModels/PerformanceReportData.cs
using System;
using System.Collections.Generic;
using TFMS.Models; // For Trip and Vehicle models if you want to include lists

namespace TFMS.ViewModels
{
    public class PerformanceReportData
    {
        public DateTime GeneratedDate { get; set; } = DateTime.Now;

        // Vehicle Metrics
        public int TotalVehicles { get; set; }
        public int AvailableVehicles { get; set; }
        public int InMaintenanceVehicles { get; set; }
        public int UnavailableVehicles { get; set; } // Can be calculated or directly retrieved

        // Trip Metrics
        public int TotalTrips { get; set; }
        public int CompletedTrips { get; set; }
        public int InProgressTrips { get; set; }
        public int PendingTrips { get; set; }
        public int CancelledTrips { get; set; }

        // Optional: List of recent trips for details
        public List<Trip> RecentTrips { get; set; } = new List<Trip>();
    }
}
