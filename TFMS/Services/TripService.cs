// Services/TripService.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TFMS.Data; // Ensure correct namespace
using TFMS.Models; // Ensure correct namespace
using TFMS.Services;
using System; // For DateTime
using TFMS.ViewModels;
namespace TFMS.Services
{
    public class TripService :ITripService
    {
        private readonly ApplicationDbContext _context;

        public TripService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Trip>> GetAllTripsAsync(string? searchString = null, string? statusFilter = null, int? vehicleIdFilter = null, string? driverIdFilter = null)
        {
            var trips = _context.Trips
                                .Include(t => t.Vehicle)
                                .Include(t => t.Driver)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                trips = trips.Where(t => t.StartLocation.Contains(searchString) ||
                                        t.EndLocation.Contains(searchString) ||
                                        (t.Driver != null && t.Driver.Email != null && t.Driver.Email.Contains(searchString)) ||
                                        (t.Vehicle != null && t.Vehicle.RegistrationNumber.Contains(searchString)));
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                trips = trips.Where(t => t.Status == statusFilter);
            }

            if (vehicleIdFilter.HasValue && vehicleIdFilter.Value > 0)
            {
                trips = trips.Where(t => t.VehicleId == vehicleIdFilter.Value);
            }

            if (!string.IsNullOrEmpty(driverIdFilter) && driverIdFilter != "0")
            {
                trips = trips.Where(t => t.DriverId == driverIdFilter);
            }

            return await trips.ToListAsync();
        }

        public async Task<Trip?> GetTripByIdAsync(int id)
        {
            return await _context.Trips
                                 .Include(t => t.Vehicle)
                                 .Include(t => t.Driver)
                                 .FirstOrDefaultAsync(m => m.TripId == id);
        }

        public async Task AddTripAsync(Trip trip)
        {
            _context.Add(trip);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTripAsync(Trip trip)
        {
            _context.Update(trip);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTripAsync(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip != null)
            {
                _context.Trips.Remove(trip);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> TripExistsAsync(int id)
        {
            return await _context.Trips.AnyAsync(e => e.TripId == id);
        }

        public async Task<int> GetTotalTripsAsync()
        {
            return await _context.Trips.CountAsync();
        }

        public async Task<int> GetUpcomingTripsCountAsync()
        {
            var today = DateTime.Today;
            return await _context.Trips.CountAsync(t => t.ScheduledStartTime.HasValue && t.ScheduledStartTime.Value.Date >= today && t.Status != "Completed" && t.Status != "Canceled");
        }

        public async Task<int> GetTripsInProgressCountAsync()
        {
            return await _context.Trips.CountAsync(t => t.Status == "In Progress");
        }

        public async Task<int> GetCompletedTripsCountAsync()
        {
            return await _context.Trips.CountAsync(t => t.Status == "Completed");
        }

        // NEW: Implementation for GetTripsByDriverIdAsync
        public async Task<IEnumerable<Trip>> GetTripsByDriverIdAsync(string driverId)
        {
            return await _context.Trips
                                 .Include(t => t.Vehicle)
                                 .Include(t => t.Driver)
                                 .Where(t => t.DriverId == driverId)
                                 .OrderByDescending(t => t.ScheduledStartTime) // Order by latest trips first
                                 .ToListAsync();
        }




        public async Task<int> GetTripsCountByStatusAsync(TripStatus status)
        {
            return await _context.Trips.CountAsync(t => t.Status == status.ToString());
        }

        public async Task<List<Trip>> GetRecentTripsAsync(int count)
        {
            return await _context.Trips
                                .Include(t => t.Vehicle)
                                .Include(t => t.Driver)
                                .OrderByDescending(t => t.ScheduledStartTime)
                                .Take(count)
                                .ToListAsync();
        }


        public async Task<List<VehicleUtilizationDto>> GetVehicleUtilizationDataAsync()
        {
            // Assuming Trip.Distance is in kilometers
            // Group trips by vehicle and sum their distances
            var utilizationData = await _context.Trips
                .Where(t => t.Vehicle != null && t.ActualDistanceKm.HasValue) // Only include trips with a vehicle and distance
                .GroupBy(t => t.Vehicle) // Group by the Vehicle entity
                .Select(g => new VehicleUtilizationDto
                {
                    VehicleIdentifier = g.Key.RegistrationNumber, // Use registration number as identifier
                    TotalDistanceKm = (double)g.Sum(t => t.ActualDistanceKm.GetValueOrDefault())
                    // Sum up distances
                })
                .OrderByDescending(x => x.TotalDistanceKm) // Order by highest utilization
                .ToListAsync();

            return utilizationData;
        }

    }
}


// Services/TripService.cs

