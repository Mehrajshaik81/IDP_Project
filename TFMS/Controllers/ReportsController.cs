// Controllers/ReportsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using TFMS.Services;
using TFMS.ViewModels;
using TFMS.Models;
using System;
using System.Linq;

namespace TFMS.Controllers
{
    [Authorize(Roles = "Fleet Administrator,Fleet Operator")]
    public class ReportsController : Controller
    {
        private readonly IVehicleService _vehicleService;
        private readonly ITripService _tripService;
        private readonly IFuelService _fuelService; // Changed from IFuelRecordService based on your snippet

        public ReportsController(IVehicleService vehicleService, ITripService tripService, IFuelService fuelService)
        {
            _vehicleService = vehicleService;
            _tripService = tripService;
            _fuelService = fuelService;
        }

        // Action for the PDF Performance Report
        public async Task<IActionResult> PerformanceReportPdf()
        {
            // Gather data for the report
            var reportData = new PerformanceReportData
            {
                GeneratedDate = DateTime.Now,

                TotalVehicles = await _vehicleService.GetTotalVehiclesAsync(),
                AvailableVehicles = await _vehicleService.GetAvailableVehiclesCountAsync(),
                InMaintenanceVehicles = await _vehicleService.GetVehiclesInMaintenanceCountAsync(),
                UnavailableVehicles = await _vehicleService.GetUnavailableVehiclesCountAsync(),

                TotalTrips = await _tripService.GetTotalTripsAsync(),
                CompletedTrips = await _tripService.GetTripsCountByStatusAsync(TripStatus.Completed),
                InProgressTrips = await _tripService.GetTripsCountByStatusAsync(TripStatus.InProgress),
                PendingTrips = await _tripService.GetTripsCountByStatusAsync(TripStatus.Pending),
               

                RecentTrips = await _tripService.GetRecentTripsAsync(10)
            };

            // Generate the PDF document
            var document = new PerformanceReportDocument(reportData);
            byte[] pdfBytes = document.GeneratePdf();

            // Return the PDF as a file download
            return File(pdfBytes, "application/pdf", "FleetPerformanceReport.pdf");
        }

        // --- THE ACTION FOR VEHICLE UTILIZATION REPORT ---
        public async Task<IActionResult> VehicleUtilizationReport()
        {
            var utilizationData = await _tripService.GetVehicleUtilizationDataAsync();

            // Pass data to the view. If no data, the view will display a message.
            return View(utilizationData);
        }
    }
}
