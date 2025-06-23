// TFMS.ViewModels/VehicleUtilizationDto.cs
namespace TFMS.ViewModels
{
    public class VehicleUtilizationDto
    {
        public string VehicleIdentifier { get; set; } // e.g., Registration Number
        public double TotalDistanceKm { get; set; }  // Total distance traveled by this vehicle
    }
}
