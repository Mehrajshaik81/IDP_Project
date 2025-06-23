// Services/IFuelService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TFMS.Data;
using TFMS.Models;

namespace TFMS.Services
{
    public class DailyFuelConsumptionDto
    {
        public DateTime Date { get; set; } // Matches FuelRecord.Date
        public decimal Amount { get; set; }
    }

    public interface IFuelService
    {
        Task<IEnumerable<FuelRecord>> GetAllFuelRecordsAsync(
            string? searchString = null,
            int? vehicleIdFilter = null,
            string? driverIdFilter = null,
            DateTime? dateFilter = null,   // Matches FuelRecord.Date
            string? sortOrder = null);

        Task<FuelRecord?> GetFuelRecordByIdAsync(int id);
        Task AddFuelRecordAsync(FuelRecord fuelRecord);
        Task UpdateFuelRecordAsync(FuelRecord fuelRecord);
        Task DeleteFuelRecordAsync(int id);
        Task<bool> FuelRecordExistsAsync(int id);

        Task<decimal> GetTotalFuelCostLastDaysAsync(int days);
        Task<List<DailyFuelConsumptionDto>> GetFuelConsumptionLastDaysAsync(int days);
    }
}


