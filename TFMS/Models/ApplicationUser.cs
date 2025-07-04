﻿// Models/ApplicationUser.cs or Data/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations; // For annotations like [Required]
namespace TFMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add any driver-specific attributes here
        [Display(Name = "First Name")]
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        [MaxLength(100)]
        public string? LastName { get; set; }

        [Display(Name = "EID")]
        [MaxLength(50)]
        public string? EmployeeId { get; set; } // For linking to your internal employee system

        [Display(Name = "License No")]
        [MaxLength(50)]
        public string? DrivingLicenseNumber { get; set; }

        [Display(Name = "License Expiry")]
        [DataType(DataType.Date)]
        public DateTime? LicenseExpiryDate { get; set; }

        [Display(Name = "Active")]
        public bool IsActiveDriver { get; set; } = true;

        // Navigation property for trips associated with this driver
        public ICollection<Trip>? Trips { get; set; }
    }
}