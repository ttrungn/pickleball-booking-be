using System.Linq;
using Microsoft.EntityFrameworkCore;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Domain.Enums;
using PickleBallBooking.Repositories.Interfaces.Repositories;
using PickleBallBooking.Services.Features.TimeSlots.Commands.CreateTimeSlot;
using PickleBallBooking.Services.Features.TimeSlots.Commands.DeleteTimeSlot;
using PickleBallBooking.Services.Features.TimeSlots.Commands.UpdateTimeSlot;
using PickleBallBooking.Services.Features.TimeSlots.Queries.GetTimeSlotById;
using PickleBallBooking.Services.Features.TimeSlots.Queries.GetTimeSlots;
using PickleBallBooking.Services.Features.TimeSlots.Queries.GetTimeSlotsByFieldAndDate;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Services;

public class TimeSlotService : ITimeSlotService
{
    private readonly IUnitOfWork _unitOfWork;

    public TimeSlotService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DataServiceResponse<Guid>> CreateTimeSlotAsync(CreateTimeSlotCommand command, CancellationToken cancellationToken = default)
    {
        var exists = await _unitOfWork.GetRepository<TimeSlot>().Query()
            .AnyAsync(t => t.StartTime == command.StartTime && t.EndTime == command.EndTime && t.IsActive, cancellationToken);
        if (exists)
            return new DataServiceResponse<Guid> { Success = false, Message = "Time slot already exists", Data = Guid.Empty };

        var id = Guid.NewGuid();
        var timeslot = new TimeSlot
        {
            Id = id,
            StartTime = command.StartTime,
            EndTime = command.EndTime
        };

        await _unitOfWork.GetRepository<TimeSlot>().InsertAsync(timeslot, cancellationToken);
        await _unitOfWork.SaveChangesAsync();

        return new DataServiceResponse<Guid> { Success = true, Message = "Time slot created successfully", Data = id };
    }

    public async Task<BaseServiceResponse> UpdateTimeSlotAsync(UpdateTimeSlotCommand command, CancellationToken cancellationToken = default)
    {
        var timeslot = await _unitOfWork.GetRepository<TimeSlot>().Query()
            .FirstOrDefaultAsync(t => t.Id == command.Id && t.IsActive, cancellationToken);
        if (timeslot == null)
            return new BaseServiceResponse { Success = false, Message = "Time slot not found!" };

        var duplicate = await _unitOfWork.GetRepository<TimeSlot>().Query()
            .AnyAsync(t => t.Id != command.Id && t.StartTime == command.StartTime && t.EndTime == command.EndTime && t.IsActive, cancellationToken);
        if (duplicate)
            return new BaseServiceResponse { Success = false, Message = "Another time slot with same start/end already exists!" };

        timeslot.StartTime = command.StartTime;
        timeslot.EndTime = command.EndTime;

        await _unitOfWork.GetRepository<TimeSlot>().UpdateAsync(timeslot);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse { Success = true, Message = "Time slot updated successfully!" };
    }

    public async Task<BaseServiceResponse> DeleteTimeSlotAsync(DeleteTimeSlotCommand command, CancellationToken cancellationToken = default)
    {
        var timeslot = await _unitOfWork.GetRepository<TimeSlot>().Query()
            .FirstOrDefaultAsync(t => t.Id == command.Id && t.IsActive, cancellationToken);
        if (timeslot == null)
            return new BaseServiceResponse { Success = false, Message = "Time slot not found!" };

        timeslot.IsActive = false;
        await _unitOfWork.GetRepository<TimeSlot>().UpdateAsync(timeslot);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse { Success = true, Message = "Time slot deleted successfully!" };
    }

    public async Task<DataServiceResponse<TimeSlotResponse>> GetTimeSlotByIdAsync(GetTimeSlotByIdQuery query, CancellationToken cancellationToken = default)
    {
        var timeslot = await _unitOfWork.GetRepository<TimeSlot>().Query()
            .FirstOrDefaultAsync(t => t.Id == query.Id && t.IsActive, cancellationToken);
        if (timeslot == null)
            return new DataServiceResponse<TimeSlotResponse> { Success = false, Message = "Time slot not found!", Data = null! };

        var response = new TimeSlotResponse { Id = timeslot.Id, StartTime = timeslot.StartTime, EndTime = timeslot.EndTime };
        return new DataServiceResponse<TimeSlotResponse> { Success = true, Message = "Time slot retrieved successfully", Data = response };
    }

    public async Task<PaginatedServiceResponse<TimeSlotResponse>> GetTimeSlotsAsync(GetTimeSlotsQuery query, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<TimeSlot>();

        // build predicate with optional start/end time filters
        var predicateQuery = repository.Query().AsNoTracking().Where(t => t.IsActive == query.IsActive);

        if (query.StartTime.HasValue)
        {
            var st = query.StartTime.Value;
            predicateQuery = predicateQuery.Where(t => t.StartTime >= st);
        }

        if (query.EndTime.HasValue)
        {
            var et = query.EndTime.Value;
            predicateQuery = predicateQuery.Where(t => t.EndTime <= et);
        }

        var totalCounts = await predicateQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCounts / (double)query.PageSize);
        var timeslots = await predicateQuery.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToListAsync(cancellationToken);

        var data = timeslots.Select(t => new TimeSlotResponse { Id = t.Id, StartTime = t.StartTime, EndTime = t.EndTime }).ToList();

        return new PaginatedServiceResponse<TimeSlotResponse>
        {
            Success = true,
            Message = "Time slots retrieved successfully",
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCounts,
            TotalPages = totalPages,
            Data = data
        };
    }

    public async Task<DataServiceResponse<List<TimeSlotWithAvailabilityResponse>>> GetTimeSlotsByFieldAndDateAsync(
        GetTimeSlotsByFieldAndDateQuery query,
        CancellationToken cancellationToken = default)
    {
        // Check if field exists
        var field = await _unitOfWork.GetRepository<Field>().Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == query.FieldId && f.IsActive, cancellationToken);

        if (field == null)
            return new DataServiceResponse<List<TimeSlotWithAvailabilityResponse>>
            {
                Success = false,
                Message = "Field not found!",
                Data = new List<TimeSlotWithAvailabilityResponse>()
            };

        // Get day of week from the date
        var dayOfWeek = query.Date.DayOfWeek switch
        {
            System.DayOfWeek.Sunday => Domain.Enums.DayOfWeek.Sunday,
            System.DayOfWeek.Monday => Domain.Enums.DayOfWeek.Monday,
            System.DayOfWeek.Tuesday => Domain.Enums.DayOfWeek.Tuesday,
            System.DayOfWeek.Wednesday => Domain.Enums.DayOfWeek.Wednesday,
            System.DayOfWeek.Thursday => Domain.Enums.DayOfWeek.Thursday,
            System.DayOfWeek.Friday => Domain.Enums.DayOfWeek.Friday,
            System.DayOfWeek.Saturday => Domain.Enums.DayOfWeek.Saturday,
            _ => Domain.Enums.DayOfWeek.Sunday
        };

        // Get all active timeslots with pricing for this field and day
        var pricingData = await _unitOfWork.GetRepository<Pricing>().Query()
            .AsNoTracking()
            .Where(p => p.FieldId == query.FieldId && p.DayOfWeek == dayOfWeek && p.IsActive)
            .Include(p => p.TimeSlot)
            .Where(p => p.TimeSlot.IsActive)
            .OrderBy(p => p.TimeSlot.StartTime)
            .ToListAsync(cancellationToken);

        if (!pricingData.Any())
            return new DataServiceResponse<List<TimeSlotWithAvailabilityResponse>>
            {
                Success = true,
                Message = "No time slots available for this field on this day",
                Data = new List<TimeSlotWithAvailabilityResponse>()
            };

        // Get booked timeslots for this field and date
        var bookedTimeSlotIds = await _unitOfWork.GetRepository<BookingTimeSlot>().Query()
            .AsNoTracking()
            .Where(bts => bts.Booking.FieldId == query.FieldId &&
                          bts.Booking.Date == query.Date &&
                          bts.Booking.IsActive &&
                          (bts.Booking.Status == BookingStatus.Pending || 
                           bts.Booking.Status == BookingStatus.Confirmed ||
                           bts.Booking.Status == BookingStatus.Completed))
            .Select(bts => bts.TimeSlotId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Build response
        var result = pricingData.Select(p => new TimeSlotWithAvailabilityResponse
        {
            Id = p.TimeSlot.Id,
            StartTime = p.TimeSlot.StartTime,
            EndTime = p.TimeSlot.EndTime,
            Price = p.Price,
            IsAvailable = !bookedTimeSlotIds.Contains(p.TimeSlot.Id)
        }).ToList();

        return new DataServiceResponse<List<TimeSlotWithAvailabilityResponse>>
        {
            Success = true,
            Message = "Time slots retrieved successfully",
            Data = result
        };
    }
}
