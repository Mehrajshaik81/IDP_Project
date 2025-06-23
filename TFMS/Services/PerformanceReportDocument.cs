// TFMS.Services/PerformanceReportDocument.cs
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System; // Required for Enum.Parse
using System.Linq; // For Any()
using TFMS.Models; // For TripStatus enum and EnumExtensions (since it's in TFMS.Models namespace)
using TFMS.ViewModels; // For PerformanceReportData

namespace TFMS.Services
{
    public class PerformanceReportDocument : IDocument
    {
        public PerformanceReportData Model { get; }

        public PerformanceReportDocument(PerformanceReportData model)
        {
            Model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            // Set the QuestPDF license
          

            container
                .Page(page =>
                {
                    page.Margin(50); // Set margins for the page

                    page.Header().Column(header =>
                    {
                        header.Item().Text("Fleet Performance Report")
                            .FontSize(24)
                            .SemiBold()
                            .FontColor(Colors.Blue.Darken2)
                            .AlignCenter();

                        header.Item().PaddingTop(10).Text($"Generated Date: {Model.GeneratedDate:yyyy-MM-dd HH:mm:ss}")
                            .FontSize(10)
                            .AlignRight()
                            .FontColor(Colors.Grey.Medium);
                    });

                    page.Content().PaddingVertical(20).Column(content =>
                    {
                        // Vehicle Overview Section
                        content.Item().Section("Vehicle Overview")
                            .Text("Vehicle Overview").FontSize(18).SemiBold().FontColor(Colors.Blue.Medium);
                        content.Item().PaddingTop(5).Column(col =>
                        {
                            col.Item().Text($"Total Vehicles: {Model.TotalVehicles}").FontSize(12);
                            col.Item().Text($"Available: {Model.AvailableVehicles}").FontSize(12).FontColor(Colors.Green.Darken1);
                            col.Item().Text($"In Maintenance: {Model.InMaintenanceVehicles}").FontSize(12).FontColor(Colors.Orange.Darken1);
                            col.Item().Text($"Unavailable: {Model.UnavailableVehicles}").FontSize(12).FontColor(Colors.Red.Darken1);
                        });

                        // Trip Overview Section
                        content.Item().PaddingTop(20).Section("Trip Overview")
                            .Text("Trip Overview").FontSize(18).SemiBold().FontColor(Colors.Blue.Medium);
                        content.Item().PaddingTop(5).Column(col =>
                        {
                            col.Item().Text($"Total Trips: {Model.TotalTrips}").FontSize(12);
                            col.Item().Text($"Completed: {Model.CompletedTrips}").FontSize(12).FontColor(Colors.Green.Darken1);
                            col.Item().Text($"In Progress: {Model.InProgressTrips}").FontSize(12).FontColor(Colors.Blue.Lighten1);
                            col.Item().Text($"Pending: {Model.PendingTrips}").FontSize(12).FontColor(Colors.Yellow.Darken1);
                            col.Item().Text($"Cancelled: {Model.CancelledTrips}").FontSize(12).FontColor(Colors.Red.Darken1);
                        });

                        // Recent Trips Table
                        if (Model.RecentTrips != null && Model.RecentTrips.Any())
                        {
                            content.Item().PaddingTop(20).Section("Recent Trips")
                                .Text("Recent Trips (Last 10)").FontSize(18).SemiBold().FontColor(Colors.Blue.Medium);
                            content.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(1.5f);
                                    columns.RelativeColumn(1f);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("Start Location").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("End Location").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("Driver").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("Vehicle").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("Scheduled Start").SemiBold().FontSize(10);
                                    header.Cell().BorderBottom(1).PaddingBottom(5).Text("Status").SemiBold().FontSize(10);
                                });

                                foreach (var trip in Model.RecentTrips)
                                {
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).Text(trip.StartLocation ?? "N/A").FontSize(9);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).Text(trip.EndLocation ?? "N/A").FontSize(9);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).Text(trip.Driver?.Email ?? "N/A").FontSize(9);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).Text(trip.Vehicle?.RegistrationNumber ?? "N/A").FontSize(9);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).Text(trip.ScheduledStartTime?.ToString("yyyy-MM-dd HH:mm") ?? "N/A").FontSize(9);

                                    // CRITICAL FIX: Parse trip.Status (string) to TripStatus enum before calling GetDescription()
                                    // This assumes your Trip.Status property is a string representation of the TripStatus enum.
                                    var tripStatusEnum = (TripStatus)Enum.Parse(typeof(TripStatus), trip.Status);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).Text(tripStatusEnum.GetDescription()).FontSize(9);
                                }
                            });
                        }
                    });

                    page.Footer().AlignRight().Text(text =>
                    {
                        text.Span("Page ").FontSize(10);
                        text.CurrentPageNumber().FontSize(10);
                        text.Span(" of ").FontSize(10);
                        text.TotalPages().FontSize(10);
                    });

                });
        }
    }
}
