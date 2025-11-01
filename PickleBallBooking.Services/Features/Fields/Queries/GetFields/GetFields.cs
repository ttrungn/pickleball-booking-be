using System.ComponentModel;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Services.Models.Responses.Fields;

namespace PickleBallBooking.Services.Features.Fields.Queries.GetFields;

public record GetFieldsQuery : IRequest<PaginatedServiceResponse<FieldResponse>>
{
    public string? Name { get; init; } = string.Empty;
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool? IsActive { get; init; } = true;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 8;
}

public class GetFieldsQueryValidator : AbstractValidator<GetFieldsQuery>
{
    public GetFieldsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than zero!");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than zero!");

        RuleFor(x => x.MinPrice)
            .LessThanOrEqualTo(x => x.MaxPrice).WithMessage("Min price must be less than or equal to max price!");
    }
}

public class GetFieldsQueryHandler : IRequestHandler<GetFieldsQuery, PaginatedServiceResponse<FieldResponse>>
{
    private readonly IFieldService _fieldService;
    private readonly ILogger<GetFieldsQueryHandler> _logger;

    public GetFieldsQueryHandler(IFieldService fieldService, ILogger<GetFieldsQueryHandler> logger)
    {
        _fieldService = fieldService;
        _logger = logger;
    }

    public async Task<PaginatedServiceResponse<FieldResponse>> Handle(GetFieldsQuery request, CancellationToken cancellationToken)
    {
        var response = await _fieldService.GetFieldsAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to retrieve fields: {Message}", response.Message);
            return response;
        }
        _logger.LogInformation("Fields retrieved successfully. Count: {Count}", response.Data.Count);
        return response;
    }
}
