// Controllers/FuelRecordsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using TFMS.Models;
using TFMS.Services;
using System.Collections.Generic;

namespace TFMS.Controllers
{
    [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
    public class FuelRecordsController : Controller
    {
        private readonly IFuelService _fuelService;
        private readonly IVehicleService _vehicleService;
        private readonly UserManager<ApplicationUser> _userManager;

        public FuelRecordsController(IFuelService fuelService, IVehicleService vehicleService, UserManager<ApplicationUser> userManager)
        {
            _fuelService = fuelService;
            _vehicleService = vehicleService;
            _userManager = userManager;
        }

        // GET: FuelRecords
        [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
        public async Task<IActionResult> Index(
            string? searchString,
            int? vehicleIdFilter,
            string? driverIdFilter,
            DateTime? dateFilter,
            string? sortOrder)
        {
            // --- For Sorting ---
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["VehicleSortParm"] = sortOrder == "Vehicle" ? "vehicle_desc" : "Vehicle";
            ViewData["DriverSortParm"] = sortOrder == "Driver" ? "driver_desc" : "Driver";
            ViewData["AmountSortParm"] = sortOrder == "Amount" ? "amount_desc" : "Amount";
            ViewData["CostSortParm"] = sortOrder == "Cost" ? "cost_desc" : "Cost";

            // --- For Filtering ---
            ViewData["CurrentSearchString"] = searchString;
            ViewData["CurrentVehicleFilter"] = vehicleIdFilter;
            ViewData["CurrentDriverFilter"] = driverIdFilter;
            ViewData["CurrentDateFilter"] = dateFilter?.ToString("yyyy-MM-dd");

            // Populate Vehicle filter dropdown
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            var vehicleListItems = vehicles.Select(v => new SelectListItem
            {
                Value = v.VehicleId.ToString(),
                Text = v.RegistrationNumber
            }).ToList();
            vehicleListItems.Insert(0, new SelectListItem { Value = "0", Text = "All Vehicles" });
            ViewBag.VehicleFilterList = new SelectList(vehicleListItems, "Value", "Text", vehicleIdFilter);

            // Populate Driver filter dropdown (only active drivers)
            var drivers = await _userManager.GetUsersInRoleAsync("Driver");
            var driverListItems = drivers
                                    .Where(d => d.IsActiveDriver)
                                    .Select(d => new SelectListItem
                                    {
                                        Value = d.Id,
                                        Text = d.Email
                                    }).ToList();
            driverListItems.Insert(0, new SelectListItem { Value = "0", Text = "All Drivers" });
            ViewBag.DriverFilterList = new SelectList(driverListItems, "Value", "Text", driverIdFilter);

            var fuelRecords = await _fuelService.GetAllFuelRecordsAsync(
                searchString, vehicleIdFilter, driverIdFilter, dateFilter, sortOrder);

            return View(fuelRecords);
        }

        // GET: FuelRecords/Details/5
        [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fuelRecord = await _fuelService.GetFuelRecordByIdAsync(id.Value);
            if (fuelRecord == null)
            {
                return NotFound();
            }

            return View(fuelRecord);
        }

        // GET: FuelRecords/Create
        [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
        public async Task<IActionResult> Create()
        {
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber");
            ViewBag.DriverId = new SelectList(await _userManager.GetUsersInRoleAsync("Driver"), "Id", "Email");
            return View();
        }

        // POST: FuelRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
        public async Task<IActionResult> Create([Bind("FuelId,VehicleId,DriverId,Date,FuelQuantity,Cost,OdometerReadingKm,Location")] FuelRecord fuelRecord) // Updated property names
        {
            if (ModelState.IsValid)
            {
                await _fuelService.AddFuelRecordAsync(fuelRecord);
                TempData["SuccessMessage"] = "Fuel record added successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", fuelRecord.VehicleId);
            ViewBag.DriverId = new SelectList(await _userManager.GetUsersInRoleAsync("Driver"), "Id", "Email", fuelRecord.DriverId);
            return View(fuelRecord);
        }

        // GET: FuelRecords/Edit/5
        [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fuelRecord = await _fuelService.GetFuelRecordByIdAsync(id.Value);
            if (fuelRecord == null)
            {
                return NotFound();
            }
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", fuelRecord.VehicleId);
            ViewBag.DriverId = new SelectList(await _userManager.GetUsersInRoleAsync("Driver"), "Id", "Email", fuelRecord.DriverId);
            return View(fuelRecord);
        }

        // POST: FuelRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Fleet Administrator,Fleet Operator,Driver")]
        public async Task<IActionResult> Edit(int id, [Bind("FuelId,VehicleId,DriverId,Date,FuelQuantity,Cost,OdometerReadingKm,Location")] FuelRecord fuelRecord) // Updated property names
        {
            if (id != fuelRecord.FuelId) // Changed from FuelRecordId
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _fuelService.UpdateFuelRecordAsync(fuelRecord);
                    TempData["SuccessMessage"] = "Fuel record updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _fuelService.FuelRecordExistsAsync(fuelRecord.FuelId)) // Changed from FuelRecordId
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.VehicleId = new SelectList(await _vehicleService.GetAllVehiclesAsync(), "VehicleId", "RegistrationNumber", fuelRecord.VehicleId);
            ViewBag.DriverId = new SelectList(await _userManager.GetUsersInRoleAsync("Driver"), "Id", "Email", fuelRecord.DriverId);
            return View(fuelRecord);
        }

        // GET: FuelRecords/Delete/5
        [Authorize(Roles = "Fleet Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var fuelRecord = await _fuelService.GetFuelRecordByIdAsync(id.Value);
            if (fuelRecord == null)
            {
                return NotFound();
            }

            return View(fuelRecord);
        }

        // POST: FuelRecords/Delete/5
        [Authorize(Roles = "Fleet Administrator")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _fuelService.DeleteFuelRecordAsync(id);
            TempData["SuccessMessage"] = "Fuel record deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
