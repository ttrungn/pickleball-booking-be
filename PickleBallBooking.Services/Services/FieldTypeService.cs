using Microsoft.EntityFrameworkCore;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Repositories.Interfaces.Repositories;
using PickleBallBooking.Services.Features.FieldTypes.Queries.GetFieldTypes;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Services.Models.Responses.Fields;

namespace PickleBallBooking.Services.Services;

public class FieldTypeService : IFieldTypeService
{
    private readonly IUnitOfWork _unitOfWork;

    public FieldTypeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedServiceResponse<FieldTypeResponse>> GetFieldTypesAsync(GetFieldTypesQuery query, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<FieldType>();

        var predicate = repository
            .Query()
            .AsNoTracking()
            .Where(ft => ft.IsActive == query.IsActive &&
                         (string.IsNullOrEmpty(query.Name) || ft.Name.Contains(query.Name)));

        var totalCounts = await predicate.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCounts / (double)(query.PageSize));
        var fieldTypes = await predicate
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var response = fieldTypes.Select(ft => new FieldTypeResponse
        {
            Id = ft.Id,
            Name = ft.Name,
            Description = ft.Description,
            IsActive = ft.IsActive
        }).ToList();

        return new PaginatedServiceResponse<FieldTypeResponse>
        {
            Success = true,
            Message = "Field types retrieved successfully!",
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCounts,
            TotalPages = totalPages,
            Data = response
        };
    }
}
