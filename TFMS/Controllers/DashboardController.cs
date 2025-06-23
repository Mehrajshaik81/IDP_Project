// Controllers/DashboardController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TFMS.Services; // Assuming you have these services for fetching data
using TFMS.ViewModels; // Assuming you might have a DashboardViewModel later
using TFMS.Models; // For TripStatus and VehicleStatus enums

namespace TFMS.Controllers
{
    // You might want to authorize who can see the dashboard
    [Authorize] // Example: Allow authenticated users. Adjust roles as needed (e.g., [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")] )
    public class DashboardController : Controller
    {
        private readonly IVehicleService _vehicleService;
        private readonly ITripService _tripService;
        private readonly IFuelService _fuelService; // Changed from IFuelRecordService based on your snippet

        public DashboardController(IVehicleService vehicleService, ITripService tripService, IFuelService fuelService)
        {
            _vehicleService = vehicleService;
            _tripService = tripService;
            _fuelService = fuelService;
        }

        // GET: Dashboard/Index
        public async Task<IActionResult> Index()
        {
            // Populate dashboard summary data
            var dashboardSummary = new DashboardSummaryViewModel
            {
                TotalVehicles = await _vehicleService.GetTotalVehiclesAsync(),
                AvailableVehicles = await _vehicleService.GetAvailableVehiclesCountAsync(),
                InMaintenanceVehicles = await _vehicleService.GetVehiclesInMaintenanceCountAsync(),
                UnavailableVehicles = await _vehicleService.GetUnavailableVehiclesCountAsync(),

                TotalTrips = await _tripService.GetTotalTripsAsync(),
                CompletedTrips = await _tripService.GetTripsCountByStatusAsync(TripStatus.Completed),
                InProgressTrips = await _tripService.GetTripsCountByStatusAsync(TripStatus.InProgress),
                PendingTrips = await _tripService.GetTripsCountByStatusAsync(TripStatus.Pending),
                

                TotalFuelCostLast30Days = await _fuelService.GetTotalFuelCostLastDaysAsync(30),
                DailyFuelConsumptionLast7Days = await _fuelService.GetFuelConsumptionLastDaysAsync(7)
            };

            return View(dashboardSummary);
        }
    }
}
