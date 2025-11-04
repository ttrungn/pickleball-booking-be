using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Domain.Enums;
using PickleBallBooking.Repositories.Interfaces.Repositories;

namespace PickleBallBooking.Services.BackgroundServices;

public class BookingAutoCancelBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingAutoCancelBackgroundService> _logger;

    public BookingAutoCancelBackgroundService(IServiceProvider serviceProvider, ILogger<BookingAutoCancelBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Booking auto-cancel background service starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var bookingRepo = unitOfWork.GetRepository<Booking>();

                var cutoff = DateTime.UtcNow.AddMinutes(-2);

                var staleBookings = await bookingRepo.Query()
                    .Where(b => b.Status == BookingStatus.Pending && b.CreatedAt <= cutoff)
                    .ToListAsync(stoppingToken);

                if (staleBookings.Any())
                {
                    _logger.LogInformation("Found {Count} stale booking(s) to cancel.", staleBookings.Count);

                    foreach (var b in staleBookings)
                    {
                        b.Status = BookingStatus.Cancelled;
                        b.UpdatedAt = DateTime.UtcNow;
                        await bookingRepo.UpdateAsync(b);
                    }

                    await unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Cancelled {Count} booking(s).", staleBookings.Count);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while auto-cancelling bookings.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // ignore and allow loop to exit
            }
        }

        _logger.LogInformation("Booking auto-cancel background service stopping.");
    }
}

