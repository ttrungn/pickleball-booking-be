using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PickleBallBooking.Services.Dashboards.Queries.GetSummary;
using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Services.Models.Responses.Dashboards;

namespace PickleBallBooking.Services.Interfaces.Services;
public interface IDashboardService
{
    Task<DataServiceResponse<DashboardSummary>> GetSummaryAsync(DateTime? startDate, DateTime? endDate);
    Task<DataServiceResponse<GetTopFieldsResponse>> GetTopFieldsAsync(int topCount = 5);
    Task<DataServiceResponse<GetRevenueResponse>> GetRevenueAsync(string view, DateTime? startDate, DateTime? endDate);
}
