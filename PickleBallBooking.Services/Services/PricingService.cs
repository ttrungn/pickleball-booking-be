using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Repositories.Interfaces.Repositories;
using PickleBallBooking.Services.Features.Pricings.Commands.CreatePricing;
using PickleBallBooking.Services.Features.Pricings.Commands.DeletePricing;
using PickleBallBooking.Services.Features.Pricings.Commands.UpdatePricing;
using PickleBallBooking.Services.Features.Pricings.Queries.GetPricingById;
using PickleBallBooking.Services.Features.Pricings.Queries.GetPricings;
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

    public async Task<DataServiceResponse<Guid>> CreatePricingAsync(CreatePricingCommand command, CancellationToken cancellationToken = default)
    {
        var field = await _unitOfWork.GetRepository<Field>().Query().FirstOrDefaultAsync(f => f.Id == command.FieldId && f.IsActive, cancellationToken);
        if (field == null)
            return new DataServiceResponse<Guid> { Success = false, Message = "Field not found!", Data = Guid.Empty };

        var timeslot = await _unitOfWork.GetRepository<TimeSlot>().Query().FirstOrDefaultAsync(t => t.Id == command.TimeSlotId && t.IsActive, cancellationToken);
        if (timeslot == null)
            return new DataServiceResponse<Guid> { Success = false, Message = "Time slot not found!", Data = Guid.Empty };

        var exists = await _unitOfWork.GetRepository<Pricing>().Query()
            .AnyAsync(p => p.FieldId == command.FieldId && p.TimeSlotId == command.TimeSlotId && p.DayOfWeek == command.DayOfWeek && p.IsActive, cancellationToken);
        if (exists)
            return new DataServiceResponse<Guid> { Success = false, Message = "Pricing for this field/time/day already exists!", Data = Guid.Empty };

        var id = Guid.NewGuid();
        var pricing = new Pricing
        {
            Id = id,
            FieldId = command.FieldId,
            TimeSlotId = command.TimeSlotId,
            DayOfWeek = command.DayOfWeek,
            //Price = command.Price
        };

        await _unitOfWork.GetRepository<Pricing>().InsertAsync(pricing, cancellationToken);
        await _unitOfWork.SaveChangesAsync();

        return new DataServiceResponse<Guid> { Success = true, Message = "Pricing created successfully", Data = id };
    }

    public async Task<BaseServiceResponse> UpdatePricingAsync(UpdatePricingCommand command, CancellationToken cancellationToken = default)
    {
        var pricing = await _unitOfWork.GetRepository<Pricing>().Query().FirstOrDefaultAsync(p => p.Id == command.Id && p.IsActive, cancellationToken);
        if (pricing == null)
            return new BaseServiceResponse { Success = false, Message = "Pricing not found!" };

        var field = await _unitOfWork.GetRepository<Field>().Query().FirstOrDefaultAsync(f => f.Id == command.FieldId && f.IsActive, cancellationToken);
        if (field == null)
            return new BaseServiceResponse { Success = false, Message = "Field not found!" };

        var timeslot = await _unitOfWork.GetRepository<TimeSlot>().Query().FirstOrDefaultAsync(t => t.Id == command.TimeSlotId && t.IsActive, cancellationToken);
        if (timeslot == null)
            return new BaseServiceResponse { Success = false, Message = "Time slot not found!" };

        var duplicate = await _unitOfWork.GetRepository<Pricing>().Query()
            .AnyAsync(p => p.Id != command.Id && p.FieldId == command.FieldId && p.TimeSlotId == command.TimeSlotId && p.DayOfWeek == command.DayOfWeek && p.IsActive, cancellationToken);
        if (duplicate)
            return new BaseServiceResponse { Success = false, Message = "Another pricing with the same field/time/day already exists!" };

        pricing.FieldId = command.FieldId;
        pricing.TimeSlotId = command.TimeSlotId;
        pricing.DayOfWeek = command.DayOfWeek;
        //pricing.Price = command.Price;

        await _unitOfWork.GetRepository<Pricing>().UpdateAsync(pricing);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse { Success = true, Message = "Pricing updated successfully!" };
    }

    public async Task<BaseServiceResponse> DeletePricingAsync(DeletePricingCommand command, CancellationToken cancellationToken = default)
    {
        var pricing = await _unitOfWork.GetRepository<Pricing>().Query().FirstOrDefaultAsync(p => p.Id == command.Id && p.IsActive, cancellationToken);
        if (pricing == null)
            return new BaseServiceResponse { Success = false, Message = "Pricing not found!" };

        pricing.IsActive = false;
        await _unitOfWork.GetRepository<Pricing>().UpdateAsync(pricing);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse { Success = true, Message = "Pricing deleted successfully!" };
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
            Price = pricing.Price
        };

        return new DataServiceResponse<PricingResponse> { Success = true, Message = "Pricing retrieved successfully", Data = response };
    }

    public async Task<PaginatedServiceResponse<PricingResponse>> GetPricingsAsync(GetPricingsQuery query, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<Pricing>();

        var predicate = repository.Query().AsNoTracking()
            .Where(p => p.IsActive == query.IsActive &&
                        (!query.FieldId.HasValue || p.FieldId == query.FieldId.Value) &&
                        (!query.TimeSlotId.HasValue || p.TimeSlotId == query.TimeSlotId.Value) &&
                        (!query.DayOfWeek.HasValue || p.DayOfWeek == query.DayOfWeek.Value));

        var totalCounts = await predicate.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCounts / (double)query.PageSize);
        var pricings = await predicate.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToListAsync(cancellationToken);

        var response = pricings.Select(p => new PricingResponse
        {
            Id = p.Id,
            FieldId = p.FieldId,
            TimeSlotId = p.TimeSlotId,
            DayOfWeek = p.DayOfWeek,
            Price = p.Price
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
}
