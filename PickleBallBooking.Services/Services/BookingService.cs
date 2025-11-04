using Microsoft.EntityFrameworkCore;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Domain.Enums;
using PickleBallBooking.Repositories.Interfaces.Repositories;
using PickleBallBooking.Services.Features.Bookings.Commands.CreateBooking;
using PickleBallBooking.Services.Features.Bookings.Commands.UpdateBooking;
using PickleBallBooking.Services.Features.Bookings.Queries.GetBookingById;
using PickleBallBooking.Services.Features.Bookings.Queries.GetBookingByUser;
using PickleBallBooking.Services.Features.Bookings.Queries.GetBookings;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public BookingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    public async Task<DataServiceResponse<Guid>> CreateBookingAsync(
        CreateBookingCommand command, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fieldRepo = _unitOfWork.GetRepository<Field>();
            var field = await fieldRepo.Query()
                .FirstOrDefaultAsync(f => f.Id == command.FieldId, cancellationToken);
            
            if (field == null)
                return new DataServiceResponse<Guid>
                {
                    Success = false,
                    Data = Guid.Empty,
                    Message = $"Field with ID {command.FieldId} does not exist."
                };


            var timeSlotIds = command.TimeSlotIds;
            var timeSlotRepo = _unitOfWork.GetRepository<TimeSlot>();
            var existingTimeSlots = await timeSlotRepo.Query()
                .Where(t => timeSlotIds.Contains(t.Id))
                .ToListAsync(cancellationToken);
            
            if (existingTimeSlots.Count != timeSlotIds.Count)
            {
                var missingIds = timeSlotIds.Except(existingTimeSlots.Select(t => t.Id)).ToList();
                return new DataServiceResponse<Guid>
                {
                    Success = false,
                    Data = Guid.Empty,
                    Message = $"Time slots with IDs {string.Join(", ", missingIds)} do not exist."
                };
            }
            // If booking date is today, ensure no past time slots are being booked
            var today = DateOnly.FromDateTime(DateTime.Now);
            if (command.Date == today)
            {
                var currentTime = TimeOnly.FromDateTime(DateTime.Now);
                var pastTimeSlots = existingTimeSlots
                    .Where(ts => ts.StartTime < currentTime)
                    .Select(ts => ts.Id)
                    .ToList();
    
                if (pastTimeSlots.Any())
                {
                    return new DataServiceResponse<Guid>
                    {
                        Success = false,
                        Data = Guid.Empty,
                        Message = $"Cannot book past time slots for today."
                    };
                }
            }
            // Check if TimeSlots are already booked for this field and date
            var bookingTimeSlotRepo = _unitOfWork.GetRepository<BookingTimeSlot>();
            var conflictingBookings = await bookingTimeSlotRepo.Query()
                .Include(bts => bts.Booking)
                .Where(bts => timeSlotIds.Contains(bts.TimeSlotId) 
                    && bts.Booking.FieldId == command.FieldId
                    && bts.Booking.Date == command.Date
                    && bts.Booking.Status != BookingStatus.Cancelled)
                .ToListAsync(cancellationToken);

            if (conflictingBookings.Any())
            {
                var conflictingTimeSlotIds = conflictingBookings
                    .Select(cb => cb.TimeSlotId).Distinct();
                return new DataServiceResponse<Guid>
                {
                    Success = false,
                    Data = Guid.Empty,
                    Message = $"Time slots with IDs {string.Join(", ", conflictingTimeSlotIds)} are already booked for this field on {command.Date}."
                };
            }
            
            // Get DayOfWeek from DateTime (0=Sunday, 6=Saturday)
            var dayOfWeek = (int)command.Date.DayOfWeek;
            var pricingQuery = _unitOfWork.GetRepository<Pricing>().Query()
                .Where(pr => pr.FieldId == command.FieldId
                             && command.TimeSlotIds.Contains(pr.TimeSlotId)
                             && (int)pr.DayOfWeek == dayOfWeek);

            if (!await pricingQuery.AnyAsync(cancellationToken))
            {
                return new DataServiceResponse<Guid>
                {
                    Success = false,
                    Data = Guid.Empty,
                    Message = $"No pricing found for field with ID {command.FieldId}."
                };
            }

            var totalPrice = await pricingQuery.SumAsync(pr => pr.Price, cancellationToken);
            
            var id = Guid.NewGuid();
            var booking = new Booking
            {
                Id = id,
                UserId = command.UserId,
                FieldId = command.FieldId,
                PaymentId = command.PaymentId,
                Date = command.Date,
                Status = BookingStatus.Pending,
                TotalPrice = totalPrice,
                CreatedAt = DateTime.UtcNow
            };
            
            foreach(var timeSlotId in command.TimeSlotIds)
            {
                booking.BookingTimeSlots.Add(new BookingTimeSlot
                {
                    BookingId = id,
                    TimeSlotId = timeSlotId,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            await _unitOfWork.GetRepository<Booking>().InsertAsync(booking, cancellationToken);
            await _unitOfWork.SaveChangesAsync();
            
            return new DataServiceResponse<Guid>
            {
                Success = true,
                Data = id,
                Message = "Booking created successfully."
            };
        }
        catch (Exception ex)
        {
            return new DataServiceResponse<Guid>
            {
                Success = false,
                Data = Guid.Empty,
                Message = $"Failed to create booking: {ex.Message}"
            };
        }
    }

    public async Task<PaginatedServiceResponse<BookingResponse>> GetBookingByUserAsync(
        GetBookingByUserQuery query,
        CancellationToken cancellationToken = default)
    {
        var bookingRepo = _unitOfWork.GetRepository<Booking>(); 
        var bookingsQuery = bookingRepo.Query()
            .Include(b => b.Field)
            .Include(b => b.BookingTimeSlots)
                .ThenInclude(bts => bts.TimeSlot)
            .Where(b => b.UserId == query.UserId) 
            .AsQueryable();
        
        if (!string.IsNullOrEmpty(query.FieldName)) 
        { 
            bookingsQuery = bookingsQuery
                .Where(b => b.Field.Name.Contains(query.FieldName));
        }
        
        if (query.MinPrice.HasValue) 
        { 
            bookingsQuery = bookingsQuery
                .Where(b => b.TotalPrice >= query.MinPrice.Value);
        }
        
        if (query.MaxPrice.HasValue) 
        { 
            bookingsQuery = bookingsQuery.Where(b => b.TotalPrice <= query.MaxPrice.Value);
        }
        
        if (query.IsActive.HasValue) 
        { 
            bookingsQuery = bookingsQuery.Where(b => b.IsActive == query.IsActive.Value);
        }

        if (query.Status.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.Status == query.Status.Value);
        }

        var totalCounts = await bookingsQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCounts / (double)query.PageSize);
        var bookings = await bookingsQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var bookingResponses = bookings.Select(b => new BookingResponse
        {
            Id = b.Id,
            FieldId = b.FieldId,
            FieldName = b.Field.Name,
            Date = b.Date,
            TotalPrice = b.TotalPrice,
            Status = b.Status.ToString(), 
            TimeSlots = b.BookingTimeSlots.Select(bts => new TimeSlotResponse
            { 
                Id = bts.TimeSlot.Id, 
                StartTime = bts.TimeSlot.StartTime, 
                EndTime = bts.TimeSlot.EndTime
            }).ToList(),
        }).ToList();

        return new PaginatedServiceResponse<BookingResponse>
        {
            Success = true,
            Message = "Bookings retrieved successfully",
            Data = bookingResponses,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCounts,
            TotalPages = totalPages
        };
    }
    
    public async Task<PaginatedServiceResponse<BookingResponse>> GetBookingsAsync(
        GetBookingsQuery query,
        CancellationToken cancellationToken = default)
    {
        var bookingRepo = _unitOfWork.GetRepository<Booking>(); 
        var bookingsQuery = bookingRepo.Query()
            .Include(b => b.Field)
            .Include(b => b.BookingTimeSlots)
                .ThenInclude(bts => bts.TimeSlot)
            .AsQueryable();
        
        if (!string.IsNullOrEmpty(query.FieldName)) 
        { 
            bookingsQuery = bookingsQuery
                .Where(b => b.Field.Name.Contains(query.FieldName));
        }
        
        if (query.MinPrice.HasValue) 
        { 
            bookingsQuery = bookingsQuery
                .Where(b => b.TotalPrice >= query.MinPrice.Value);
        }
        
        if (query.MaxPrice.HasValue) 
        { 
            bookingsQuery = bookingsQuery.Where(b => b.TotalPrice <= query.MaxPrice.Value);
        }
        
        if (query.IsActive.HasValue) 
        { 
            bookingsQuery = bookingsQuery.Where(b => b.IsActive == query.IsActive.Value);
        }

        if (query.Status.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.Status == query.Status.Value);
        }

        var totalCounts = await bookingsQuery.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCounts / (double)query.PageSize);
        var bookings = await bookingsQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var bookingResponses = bookings.Select(b => new BookingResponse
        {
            Id = b.Id,
            FieldId = b.FieldId,
            FieldName = b.Field.Name,
            Date = b.Date,
            TotalPrice = b.TotalPrice,
            Status = b.Status.ToString(), 
            TimeSlots = b.BookingTimeSlots.Select(bts => new TimeSlotResponse
            { 
                Id = bts.TimeSlot.Id, 
                StartTime = bts.TimeSlot.StartTime, 
                EndTime = bts.TimeSlot.EndTime
            }).ToList(),
        }).ToList();

        return new PaginatedServiceResponse<BookingResponse>
        {
            Success = true,
            Message = "Bookings retrieved successfully",
            Data = bookingResponses,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCounts,
            TotalPages = totalPages
        };
    }

    public async Task<DataServiceResponse<BookingResponse>> GetBookingByIdAsync(
        GetBookingByIdQuery query, 
        CancellationToken cancellationToken = default)
    {
        var bookingRepo = _unitOfWork.GetRepository<Booking>();
        var booking = await bookingRepo.Query()
            .Include(b => b.Field)
            .Include(b => b.BookingTimeSlots)
                .ThenInclude(bts => bts.TimeSlot)
            .FirstOrDefaultAsync(b => b.Id == query.Id, cancellationToken);
        
        if (booking == null)
        {
            return new DataServiceResponse<BookingResponse>
            {
                Success = false,
                Data = null,
                Message = $"Booking with ID {query.Id} does not exist."
            };
        }

        var bookingResponse = new BookingResponse
        {
            Id = booking.Id,
            FieldId = booking.FieldId,
            FieldName = booking.Field.Name,
            Date = booking.Date,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status.ToString(),
            TimeSlots = booking.BookingTimeSlots.Select(bts => new TimeSlotResponse
            {
                Id = bts.TimeSlot.Id,
                StartTime = bts.TimeSlot.StartTime,
                EndTime = bts.TimeSlot.EndTime
            }).ToList(),
        };

        return new DataServiceResponse<BookingResponse>
        {
            Success = true,
            Data = bookingResponse,
            Message = "Booking retrieved successfully."
        };
    }

    public async Task<BaseServiceResponse> CancelBookingAsync(CancelBookingCommand command, CancellationToken cancellationToken = default)
    {
        var bookingRepo = _unitOfWork.GetRepository<Booking>();
        var booking = await bookingRepo.Query()
            .FirstOrDefaultAsync(b => b.Id == command.Id, cancellationToken);
        
        if (booking == null)
        {
            return new BaseServiceResponse
            {
                Success = false,
                Message = $"Booking with ID {command.Id} does not exist."
            };
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return new BaseServiceResponse
            {
                Success = false,
                Message = $"Booking with ID {command.Id} is already cancelled."
            };
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        await bookingRepo.UpdateAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse
        {
            Success = true,
            Message = "Booking cancelled successfully.",
        };
    }
}
