// Services/FuelService.cs
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TFMS.Data;
using TFMS.Models;

namespace TFMS.Services
{
    public class FuelService : IFuelService
    {
        private readonly ApplicationDbContext _context;

        public FuelService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FuelRecord>> GetAllFuelRecordsAsync(
            string? searchString = null,
            int? vehicleIdFilter = null,
            string? driverIdFilter = null,
            DateTime? dateFilter = null,
            string? sortOrder = null)
        {
            var fuelRecords = _context.FuelRecords
                                      .Include(f => f.Vehicle)
                                      .Include(f => f.Driver)
                                      .AsQueryable();

            // --- Filtering Logic ---

            // Search String (by Vehicle Reg No, Driver Email)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                fuelRecords = fuelRecords.Where(f =>
                    (f.Vehicle != null && f.Vehicle.RegistrationNumber.Contains(searchString)) ||
                    (f.Driver != null && f.Driver.Email != null && f.Driver.Email.Contains(searchString)));
            }

            // Vehicle Filter
            if (vehicleIdFilter.HasValue && vehicleIdFilter.Value > 0)
            {
                fuelRecords = fuelRecords.Where(f => f.VehicleId == vehicleIdFilter.Value);
            }

            // Driver Filter
            if (!string.IsNullOrWhiteSpace(driverIdFilter) && driverIdFilter != "0")
            {
                fuelRecords = fuelRecords.Where(f => f.DriverId == driverIdFilter);
            }

            // Single Date Filter (using FuelRecord.Date)
            if (dateFilter.HasValue)
            {
                // Filter for records on the exact date (ignoring time component)
                // Ensure f.Date.HasValue before accessing .Value.Date
                fuelRecords = fuelRecords.Where(f => f.Date.HasValue && f.Date.Value.Date == dateFilter.Value.Date);
            }

            // --- Sorting Logic ---
            switch (sortOrder)
            {
                case "date_desc":
                    fuelRecords = fuelRecords.OrderByDescending(f => f.Date); // Changed from FuelingDate
                    break;
                case "Vehicle":
                    fuelRecords = fuelRecords.OrderBy(f => f.Vehicle!.RegistrationNumber);
                    break;
                case "vehicle_desc":
                    fuelRecords = fuelRecords.OrderByDescending(f => f.Vehicle!.RegistrationNumber);
                    break;
                case "Driver":
                    fuelRecords = fuelRecords.OrderBy(f => f.Driver!.Email);
                    break;
                case "driver_desc":
                    fuelRecords = fuelRecords.OrderByDescending(f => f.Driver!.Email);
                    break;
                case "Amount":
                    fuelRecords = fuelRecords.OrderBy(f => f.FuelQuantity); // Changed from FuelAmountLiters
                    break;
                case "amount_desc":
                    fuelRecords = fuelRecords.OrderByDescending(f => f.FuelQuantity); // Changed from FuelAmountLiters
                    break;
                case "Cost":
                    fuelRecords = fuelRecords.OrderBy(f => f.Cost);
                    break;
                case "cost_desc":
                    fuelRecords = fuelRecords.OrderByDescending(f => f.Cost);
                    break;
                default: // Default sort by Date ascending
                    fuelRecords = fuelRecords.OrderBy(f => f.Date); // Changed from FuelingDate
                    break;
            }

            return await fuelRecords.ToListAsync();
        }

        public async Task<FuelRecord?> GetFuelRecordByIdAsync(int id)
        {
            return await _context.FuelRecords
                                .Include(f => f.Vehicle)
                                .Include(f => f.Driver)
                                .FirstOrDefaultAsync(m => m.FuelId == id); // Changed from FuelRecordId
        }

        public async Task AddFuelRecordAsync(FuelRecord fuelRecord)
        {
            _context.Add(fuelRecord);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateFuelRecordAsync(FuelRecord fuelRecord)
        {
            _context.Update(fuelRecord);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFuelRecordAsync(int id)
        {
            var fuelRecord = await _context.FuelRecords.FindAsync(id); // Changed from FuelRecordId
            if (fuelRecord != null)
            {
                _context.FuelRecords.Remove(fuelRecord);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> FuelRecordExistsAsync(int id)
        {
            return await _context.FuelRecords.AnyAsync(e => e.FuelId == id); // Changed from FuelRecordId
        }

        public async Task<decimal> GetTotalFuelCostLastDaysAsync(int days)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            // Using FuelRecord.Date
            return await _context.FuelRecords
                .Where(f => f.Date.HasValue && f.Date.Value.Date >= cutoffDate.Date && f.Cost.HasValue)
                .SumAsync(f => f.Cost ?? 0);
        }

        public async Task<List<DailyFuelConsumptionDto>> GetFuelConsumptionLastDaysAsync(int days)
        {
            var cutoffDate = DateTime.Today.AddDays(-days);
            // Using FuelRecord.Date and FuelRecord.FuelQuantity
            return await _context.FuelRecords
                .Where(f => f.Date.HasValue && f.Date.Value.Date >= cutoffDate.Date && f.FuelQuantity.HasValue)
                .GroupBy(f => f.Date!.Value.Date) // Group by FuelRecord.Date
                .Select(g => new DailyFuelConsumptionDto
                {
                    Date = g.Key,
                    Amount = g.Sum(f => f.FuelQuantity ?? 0) // Sum FuelRecord.FuelQuantity
                })
                .OrderBy(g => g.Date)
                .ToListAsync();
        }
    }
}

