using System.Linq;
using Microsoft.EntityFrameworkCore;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Domain.Enums; // for DayOfWeek enum reference
using DomainDayOfWeek = PickleBallBooking.Domain.Enums.DayOfWeek; // alias to avoid System.DayOfWeek ambiguity
using PickleBallBooking.Repositories.Interfaces.Repositories;
using PickleBallBooking.Services.Features.Pricings.Commands.CreatePricing;
using PickleBallBooking.Services.Features.Pricings.Commands.UpdatePricingRange;
using PickleBallBooking.Services.Features.Pricings.Commands.DeletePricingRange;
using PickleBallBooking.Services.Features.Pricings.Queries.GetPricingById;
using PickleBallBooking.Services.Features.Pricings.Queries.GetPricings;
using PickleBallBooking.Services.Features.Pricings.Queries.GetPricingsByField;

using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Services;

public class PricingService : IPricingService
{
    private readonly IUnitOfWork _unitOfWork;

    public PricingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DataServiceResponse<PricingResponse>> GetPricingByIdAsync(GetPricingByIdQuery query, CancellationToken cancellationToken = default)
    {
        var pricing = await _unitOfWork.GetRepository<Pricing>().Query()
            .Include(p => p.Field)
            .Include(p => p.TimeSlot)
            .FirstOrDefaultAsync(p => p.Id == query.Id && p.IsActive, cancellationToken);

        if (pricing == null)
            return new DataServiceResponse<PricingResponse> { Success = false, Message = "Pricing not found!", Data = null! };

        var response = new PricingResponse
        {
            Id = pricing.Id,
            FieldId = pricing.FieldId,
            TimeSlotId = pricing.TimeSlotId,
            DayOfWeek = pricing.DayOfWeek,
            Price = pricing.Price,
            TimeSlotStartTime = pricing.TimeSlot.StartTime,
            TimeSlotEndTime = pricing.TimeSlot.EndTime,
            RangeStartTime = pricing.StartTime,
            RangeEndTime = pricing.EndTime
        };

        return new DataServiceResponse<PricingResponse> { Success = true, Message = "Pricing retrieved successfully", Data = response };
    }

    public async Task<PaginatedServiceResponse<PricingResponse>> GetPricingsAsync(GetPricingsQuery query, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<Pricing>();

        var baseQuery = repository.Query().AsNoTracking()
            .Include(p => p.TimeSlot)
            .Where(p => p.IsActive == query.IsActive &&
                        (!query.FieldId.HasValue || p.FieldId == query.FieldId.Value) &&
                        (!query.TimeSlotId.HasValue || p.TimeSlotId == query.TimeSlotId.Value) &&
                        (!query.DayOfWeek.HasValue || p.DayOfWeek == query.DayOfWeek.Value));

        // Ordering by explicit numeric mapping to avoid NVARCHAR -> INT conversion issues if enum stored as string
        var orderedQuery = baseQuery
            .OrderBy(p => p.DayOfWeek == DomainDayOfWeek.Monday ? 0
                             : p.DayOfWeek == DomainDayOfWeek.Tuesday ? 1
                             : p.DayOfWeek == DomainDayOfWeek.Wednesday ? 2
                             : p.DayOfWeek == DomainDayOfWeek.Thursday ? 3
                             : p.DayOfWeek == DomainDayOfWeek.Friday ? 4
                             : p.DayOfWeek == DomainDayOfWeek.Saturday ? 5
                             : 6) // Sunday
            .ThenBy(p => p.TimeSlot.StartTime)
            .ThenBy(p => p.TimeSlot.EndTime)
            .ThenBy(p => p.StartTime)
            .ThenBy(p => p.EndTime);

        var totalCounts = await orderedQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCounts / (double)query.PageSize);
        var pricings = await orderedQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var response = pricings.Select(p => new PricingResponse
        {
            Id = p.Id,
            FieldId = p.FieldId,
            TimeSlotId = p.TimeSlotId,
            DayOfWeek = p.DayOfWeek,
            Price = p.Price,
            TimeSlotStartTime = p.TimeSlot.StartTime,
            TimeSlotEndTime = p.TimeSlot.EndTime,
            RangeStartTime = p.StartTime,
            RangeEndTime = p.EndTime
        }).ToList();

        return new PaginatedServiceResponse<PricingResponse>
        {
            Success = true,
            Message = "Pricings retrieved successfully",
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCounts,
            TotalPages = totalPages,
            Data = response
        };
    }

    public async Task<DataServiceResponse<List<PricingResponse>>> GetPricingsByFieldAsync(GetPricingsByFieldQuery query, CancellationToken cancellationToken = default)
    {
        var field = await _unitOfWork.GetRepository<Field>().Query()
            .FirstOrDefaultAsync(f => f.Id == query.FieldId && f.IsActive, cancellationToken);

        if (field == null)
            return new DataServiceResponse<List<PricingResponse>>
            {
                Success = false,
                Message = "Field not found!",
                Data = new List<PricingResponse>()
            };

        var pricings = await _unitOfWork.GetRepository<Pricing>().Query()
            .AsNoTracking()
            .Include(p => p.TimeSlot)
            .Where(p => p.FieldId == query.FieldId && p.IsActive)
            .OrderBy(p => p.DayOfWeek == DomainDayOfWeek.Monday ? 0
                             : p.DayOfWeek == DomainDayOfWeek.Tuesday ? 1
                             : p.DayOfWeek == DomainDayOfWeek.Wednesday ? 2
                             : p.DayOfWeek == DomainDayOfWeek.Thursday ? 3
                             : p.DayOfWeek == DomainDayOfWeek.Friday ? 4
                             : p.DayOfWeek == DomainDayOfWeek.Saturday ? 5
                             : 6)
            .ThenBy(p => p.TimeSlot.StartTime)
            .ToListAsync(cancellationToken);

        var response = pricings.Select(p => new PricingResponse
        {
            Id = p.Id,
            FieldId = p.FieldId,
            TimeSlotId = p.TimeSlotId,
            DayOfWeek = p.DayOfWeek,
            Price = p.Price,
            TimeSlotStartTime = p.TimeSlot.StartTime,
            TimeSlotEndTime = p.TimeSlot.EndTime,
            RangeStartTime = p.StartTime,
            RangeEndTime = p.EndTime
        }).ToList();

        return new DataServiceResponse<List<PricingResponse>>
        {
            Success = true,
            Message = "Pricings retrieved successfully",
            Data = response
        };
    }

    public async Task<DataServiceResponse<List<Guid>>> CreatePricingRangeAsync(CreatePricingCommand command, CancellationToken cancellationToken = default)
    {
        var fieldExists = await _unitOfWork.GetRepository<Field>().Query()
            .AnyAsync(f => f.Id == command.FieldId && f.IsActive, cancellationToken);
        if (!fieldExists)
            return new DataServiceResponse<List<Guid>> { Success = false, Message = "Field not found!", Data = new List<Guid>() };

        var timeSlotRepo = _unitOfWork.GetRepository<TimeSlot>();
        var pricingRepo = _unitOfWork.GetRepository<Pricing>();

        // Build 30-minute intervals for requested range
        var intervals = new List<(TimeOnly s, TimeOnly e)>();
        var cursor = command.StartTime;
        while (cursor < command.EndTime)
        {
            var next = cursor.AddMinutes(30);
            intervals.Add((cursor, next));
            cursor = next;
        }

        // Fetch existing timeslots (active or inactive) matching those intervals
        var starts = intervals.Select(i => i.s).ToList();
        var ends = intervals.Select(i => i.e).ToList();
        var existingSlots = await timeSlotRepo.Query()
            .Where(ts => starts.Contains(ts.StartTime) && ends.Contains(ts.EndTime))
            .ToListAsync(cancellationToken);

        // Map (start,end) -> slotId, create missing slots
        var slotMap = new Dictionary<(TimeOnly s, TimeOnly e), Guid>();
        foreach (var (s, e) in intervals)
        {
            var found = existingSlots.FirstOrDefault(x => x.StartTime == s && x.EndTime == e);
            if (found != null) slotMap[(s, e)] = found.Id;
        }
        foreach (var (s, e) in intervals)
        {
            if (!slotMap.ContainsKey((s, e)))
            {
                var slotId = Guid.NewGuid();
                await timeSlotRepo.InsertAsync(new TimeSlot
                {
                    Id = slotId,
                    StartTime = s,
                    EndTime = e,
                    IsActive = true
                }, cancellationToken);
                slotMap[(s, e)] = slotId;
            }
        }

        // Gather slotIds for duplicate check
        var slotIds = slotMap.Values.ToList();
        // Query ANY existing pricing rows (active or inactive) for those slotIds with same Field + DayOfWeek
        var existingPricings = await pricingRepo.Query()
            .Where(p => p.FieldId == command.FieldId && p.DayOfWeek == command.DayOfWeek && slotIds.Contains(p.TimeSlotId))
            .Include(p => p.TimeSlot)
            .ToListAsync(cancellationToken);

        if (existingPricings.Any())
        {
            // Build list of duplicated 30-min slot times for message
            var duplicatedSlots = existingPricings
                .Select(p => $"{p.TimeSlot.StartTime:HH:mm}-{p.TimeSlot.EndTime:HH:mm}")
                .Distinct()
                .OrderBy(t => t)
                .ToList();
            return new DataServiceResponse<List<Guid>>
            {
                Success = false,
                Message = $"Pricing already exists for slots: {string.Join(", ", duplicatedSlots)}",
                Data = new List<Guid>()
            };
        }

        // Safe to insert new pricing rows
        var createdIds = new List<Guid>();
        foreach (var (s, e) in intervals)
        {
            var pricingId = Guid.NewGuid();
            await pricingRepo.InsertAsync(new Pricing
            {
                Id = pricingId,
                FieldId = command.FieldId,
                TimeSlotId = slotMap[(s, e)],
                DayOfWeek = command.DayOfWeek,
                StartTime = command.StartTime,
                EndTime = command.EndTime,
                Price = command.Price,
                IsActive = true
            }, cancellationToken);
            createdIds.Add(pricingId);
        }

        await _unitOfWork.SaveChangesAsync();
        return new DataServiceResponse<List<Guid>> { Success = true, Message = "Pricing range created successfully", Data = createdIds };
    }

    public async Task<BaseServiceResponse> UpdatePricingRangeAsync(UpdatePricingRangeCommand command, CancellationToken cancellationToken = default)
    {
        var timeSlotRepo = _unitOfWork.GetRepository<TimeSlot>();
        var pricingRepo = _unitOfWork.GetRepository<Pricing>();

        var intervals = new List<(TimeOnly s, TimeOnly e)>();
        var cursor = command.StartTime;
        while (cursor < command.EndTime)
        {
            var next = cursor.AddMinutes(30);
            intervals.Add((cursor, next));
            cursor = next;
        }
        var starts = intervals.Select(i => i.s).ToList();
        var ends = intervals.Select(i => i.e).ToList();
        var existingSlots = await timeSlotRepo.Query()
            .Where(ts => starts.Contains(ts.StartTime) && ends.Contains(ts.EndTime))
            .ToListAsync(cancellationToken);

        var slotMap = new Dictionary<(TimeOnly s, TimeOnly e), Guid>();
        foreach (var (s, e) in intervals)
        {
            var found = existingSlots.FirstOrDefault(x => x.StartTime == s && x.EndTime == e);
            if (found != null) slotMap[(s, e)] = found.Id;
        }
        foreach (var (s, e) in intervals)
        {
            if (!slotMap.ContainsKey((s, e)))
            {
                var slotId = Guid.NewGuid();
                await timeSlotRepo.InsertAsync(new TimeSlot
                {
                    Id = slotId,
                    StartTime = s,
                    EndTime = e,
                    IsActive = true
                }, cancellationToken);
                slotMap[(s, e)] = slotId;
            }
        }

        var slotIds = slotMap.Values.ToList();
        var existingPricings = await pricingRepo.Query()
            .Where(p => p.FieldId == command.FieldId && p.DayOfWeek == command.DayOfWeek && slotIds.Contains(p.TimeSlotId))
            .ToListAsync(cancellationToken);
        var pricingBySlot = existingPricings.ToDictionary(p => p.TimeSlotId, p => p);

        var updated = 0;
        var inserted = 0;
        foreach (var (s, e) in intervals)
        {
            var slotId = slotMap[(s, e)];
            if (pricingBySlot.TryGetValue(slotId, out var existingPricing))
            {
                existingPricing.Price = command.Price;
                existingPricing.StartTime = command.StartTime;
                existingPricing.EndTime = command.EndTime;
                existingPricing.IsActive = true; // ensure reactivated
                await pricingRepo.UpdateAsync(existingPricing);
                updated++;
            }
            else
            {
                var pricingId = Guid.NewGuid();
                await pricingRepo.InsertAsync(new Pricing
                {
                    Id = pricingId,
                    FieldId = command.FieldId,
                    TimeSlotId = slotId,
                    DayOfWeek = command.DayOfWeek,
                    StartTime = command.StartTime,
                    EndTime = command.EndTime,
                    Price = command.Price,
                    IsActive = true
                }, cancellationToken);
                inserted++;
            }
        }

        await _unitOfWork.SaveChangesAsync();
        var msg = inserted == 0 ? "Pricing range updated (all slots existed)" : updated == 0 ? "Pricing range created (all slots were new)" : $"Pricing range upserted: {updated} updated, {inserted} inserted";
        return new BaseServiceResponse { Success = true, Message = msg };
    }

    public async Task<BaseServiceResponse> DeletePricingRangeAsync(DeletePricingRangeCommand command, CancellationToken cancellationToken = default)
    {
        var pricingRepo = _unitOfWork.GetRepository<Pricing>();
        var pricings = await pricingRepo.Query()
            .Where(p => p.IsActive && p.FieldId == command.FieldId && p.DayOfWeek == command.DayOfWeek && p.StartTime == command.StartTime && p.EndTime == command.EndTime)
            .ToListAsync(cancellationToken);
        if (pricings.Count == 0)
            return new BaseServiceResponse { Success = true, Message = "No pricing records found for range" };
        foreach (var p in pricings)
        {
            p.IsActive = false;
            await pricingRepo.UpdateAsync(p);
        }
        await _unitOfWork.SaveChangesAsync();
        return new BaseServiceResponse { Success = true, Message = "Pricing range soft deleted" };
    }


}
