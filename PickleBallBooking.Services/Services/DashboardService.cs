using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Domain.Enums;
using PickleBallBooking.Repositories.Interfaces.Repositories;
using PickleBallBooking.Repositories.Repositories.Contexts;
using PickleBallBooking.Services.Dashboards.Queries.GetSummary;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Services.Models.Responses.Dashboards;

namespace PickleBallBooking.Services.Services;
public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, ILogger<DashboardService> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<DataServiceResponse<DashboardSummary>> GetSummaryAsync(DateTime? startDate, DateTime? endDate)
    {
        try
        {
            _logger.LogInformation("Starting dashboard summary retrieval with StartDate={StartDate}, EndDate={EndDate}", startDate, endDate);

            var bookingRepo = _unitOfWork.GetRepository<Booking>();
            var fieldRepo = _unitOfWork.GetRepository<Field>();

            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            // Set default date range
            var start = startDate ?? DateTime.MinValue;
            var end = endDate ?? now;

            // Get all bookings within date range
            var allBookings = await bookingRepo.Query()
                .Where(b => b.CreatedAt >= start && b.CreatedAt <= end)
                .ToListAsync();

            // Get completed bookings with payments
            var completedBookings = allBookings.Where(b => b.Status == BookingStatus.Completed).ToList();

            // Calculate statistics
            var totalRevenue = completedBookings.Sum(b => b.TotalPrice);
            var monthRevenue = completedBookings
                .Where(b => b.CreatedAt >= startOfMonth)
                .Sum(b => b.TotalPrice);
            var todayRevenue = completedBookings
                .Where(b => b.Date == today)
                .Sum(b => b.TotalPrice);

            // Get customer count
            var customers = await _userManager.GetUsersInRoleAsync("Customer");

            var summary = new DashboardSummary
            {
                TotalBookings = allBookings.Count,
                TotalCustomers = customers.Count,
                TotalRevenue = totalRevenue,
                CompletedBookings = completedBookings.Count,
            };

            _logger.LogInformation("Dashboard summary retrieved: TotalBookings={TotalBookings}, TotalRevenue={TotalRevenue}", 
                summary.TotalBookings, summary.TotalRevenue);

            return new DataServiceResponse<DashboardSummary>
            {
                Success = true,
                Message = "Dashboard summary retrieved successfully",
                Data = summary
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving dashboard summary");
            return new DataServiceResponse<DashboardSummary>
            {
                Success = false,
                Message = "Failed to retrieve dashboard summary",
                Data = null!
            };
        }
    }

    public async Task<DataServiceResponse<GetTopFieldsResponse>> GetTopFieldsAsync(int topCount = 5)
    {
        try
        {
            _logger.LogInformation("Retrieving top {TopCount} fields", topCount);

            var bookingRepo = _unitOfWork.GetRepository<Booking>();

            // Get top fields by booking count and revenue
            var topFields = await bookingRepo.Query()
                .Include(b => b.Field)
                .Where(b => b.Status == BookingStatus.Completed)
                .GroupBy(b => new
                {
                    b.FieldId,
                    b.Field.Name,
                    b.Field.Address,
                })
                .Select(g => new TopFieldItem
                {
                    FieldId = g.Key.FieldId,
                    FieldName = g.Key.Name,
                    BookingCount = g.Count(),
                    TotalRevenue = g.Sum(b => b.TotalPrice)
                })
                .OrderByDescending(f => f.BookingCount)
                .ThenByDescending(f => f.TotalRevenue)
                .Take(topCount)
                .ToListAsync();

            var response = new GetTopFieldsResponse
            {
                TopFields = topFields
            };

            _logger.LogInformation("Retrieved {Count} top fields successfully", topFields.Count);

            return new DataServiceResponse<GetTopFieldsResponse>
            {
                Success = true,
                Message = $"Top {topCount} fields retrieved successfully",
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving top {TopCount} fields", topCount);
            return new DataServiceResponse<GetTopFieldsResponse>
            {
                Success = false,
                Message = "Failed to retrieve top fields",
                Data = null!
            };
        }
    }

    public async Task<DataServiceResponse<GetRevenueResponse>> GetRevenueAsync(string view, DateTime? startDate, DateTime? endDate)
    {
        try
        {
            _logger.LogInformation("Retrieving revenue data with View={View}, StartDate={StartDate}, EndDate={EndDate}", 
                view, startDate, endDate);

            var bookingRepo = _unitOfWork.GetRepository<Booking>();

            // Set default date range if not provided
            var now = DateTime.UtcNow;
            var start = startDate ?? now.AddMonths(-12); // Default: last 12 months
            var end = endDate ?? now;

            // Get completed bookings in the date range
            var bookings = await bookingRepo.Query()
                .Where(b => b.Status == BookingStatus.Completed)
                .Where(b => b.Date >= DateOnly.FromDateTime(start) && b.Date <= DateOnly.FromDateTime(end))
                .Select(b => new
                {
                    b.Date,
                    b.TotalPrice,
                    b.CreatedAt
                })
                .ToListAsync();

            _logger.LogInformation("Retrieved {BookingCount} bookings for revenue calculation", bookings.Count);

            var dataPoints = GroupRevenueData(bookings, view.ToLower());

            var response = new GetRevenueResponse
            {
                View = view,
                Data = dataPoints,
                TotalRevenue = dataPoints.Sum(d => d.Revenue),
                StartDate = start,
                EndDate = end
            };

            _logger.LogInformation("Revenue data calculated: {DataPointCount} data points, TotalRevenue={TotalRevenue}", 
                dataPoints.Count, response.TotalRevenue);

            return new DataServiceResponse<GetRevenueResponse>
            {
                Success = true,
                Message = $"Revenue data ({view} view) retrieved successfully",
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving revenue data with View={View}", view);
            return new DataServiceResponse<GetRevenueResponse>
            {
                Success = false,
                Message = "Failed to retrieve revenue data",
                Data = null!
            };
        }
    }

    private List<RevenueDataPoint> GroupRevenueData(IEnumerable<dynamic> bookings, string view)
    {
        return view switch
        {
            "day" => bookings
                .GroupBy(b => b.Date)
                .Select(g => new RevenueDataPoint
                {
                    Date = g.Key.ToDateTime(TimeOnly.MinValue),
                    Label = g.Key.ToString("MMM dd, yyyy"),
                    Revenue = g.Sum(b => (decimal)b.TotalPrice),
                    BookingCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList(),

            "year" => bookings
                .GroupBy(b => b.Date.Year)
                .Select(g => new RevenueDataPoint
                {
                    Date = new DateTime(g.Key, 1, 1),
                    Label = g.Key.ToString(),
                    Revenue = g.Sum(b => (decimal)b.TotalPrice),
                    BookingCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList(),

            _ => bookings // Default: month view
                .GroupBy(b => new { Year = b.Date.Year, Month = b.Date.Month })
                .Select(g => new RevenueDataPoint
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Revenue = g.Sum(b => (decimal)b.TotalPrice),
                    BookingCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList()
        };
    }
}
