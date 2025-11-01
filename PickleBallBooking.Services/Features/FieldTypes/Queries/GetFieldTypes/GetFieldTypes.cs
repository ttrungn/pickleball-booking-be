using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.FieldTypes.Queries.GetFieldTypes;

public record GetFieldTypesQuery : IRequest<PaginatedServiceResponse<FieldTypeResponse>>
{
    public string? Name { get; init; } = string.Empty;
    public bool? IsActive { get; init; } = true;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 8;
}

public class GetFieldTypesQueryValidator : AbstractValidator<GetFieldTypesQuery>
{
    public GetFieldTypesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than zero!");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than zero!");
    }
}

public class GetFieldTypesQueryHandler : IRequestHandler<GetFieldTypesQuery, PaginatedServiceResponse<FieldTypeResponse>>
{
    private readonly IFieldTypeService _fieldTypeService;
    private readonly ILogger<GetFieldTypesQueryHandler> _logger;

    public GetFieldTypesQueryHandler(IFieldTypeService fieldTypeService, ILogger<GetFieldTypesQueryHandler> logger)
    {
        _fieldTypeService = fieldTypeService;
        _logger = logger;
    }

    public async Task<PaginatedServiceResponse<FieldTypeResponse>> Handle(GetFieldTypesQuery request, CancellationToken cancellationToken)
    {
        var response = await _fieldTypeService.GetFieldTypesAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to retrieve field types: {Message}", response.Message);
            return response;
        }
        _logger.LogInformation("Field types retrieved successfully. Count: {Count}", response.Data.Count);
        return response;
    }
}
