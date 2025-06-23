// TFMS.ViewModels/DashboardSummaryViewModel.cs
using System.Collections.Generic;
using TFMS.Services; // For DailyFuelConsumptionDto if defined there

namespace TFMS.ViewModels
{
    public class DashboardSummaryViewModel
    {
        // Vehicle Stats
        public int TotalVehicles { get; set; }
        public int AvailableVehicles { get; set; }
        public int InMaintenanceVehicles { get; set; }
        public int UnavailableVehicles { get; set; }

        // Trip Stats
        public int TotalTrips { get; set; }
        public int CompletedTrips { get; set; }
        public int InProgressTrips { get; set; }
        public int PendingTrips { get; set; }
        public int CancelledTrips { get; set; }

        // Fuel Stats
        public decimal TotalFuelCostLast30Days { get; set; }
        public List<DailyFuelConsumptionDto> DailyFuelConsumptionLast7Days { get; set; } = new List<DailyFuelConsumptionDto>();
    }
}
