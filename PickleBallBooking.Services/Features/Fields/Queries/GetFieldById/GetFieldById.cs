using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Services.Models.Responses.Fields;

namespace PickleBallBooking.Services.Features.Fields.Queries.GetFieldById;

public record GetFieldByIdQuery : IRequest<DataServiceResponse<FieldResponse>>
{
    public Guid Id { get; init; }
}

public class GetFieldByIdQueryValidator : AbstractValidator<GetFieldByIdQuery>
{
    public GetFieldByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Field ID is required!");

    }
}

public class GetFieldByIdQueryHandler : IRequestHandler<GetFieldByIdQuery, DataServiceResponse<FieldResponse>>
{
    private readonly IFieldService _fieldService;
    private readonly ILogger<GetFieldByIdQueryHandler> _logger;

    public GetFieldByIdQueryHandler(IFieldService fieldService, ILogger<GetFieldByIdQueryHandler> logger)
    {
        _fieldService = fieldService;
        _logger = logger;
    }

    public async Task<DataServiceResponse<FieldResponse>> Handle(GetFieldByIdQuery request, CancellationToken cancellationToken)
    {
        var response = await _fieldService.GetFieldByIdAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to retrieve field: {Message}", response.Message);
            return response;
        }
        _logger.LogInformation("Field retrieved successfully with ID: {FieldId}", request.Id);
        return response;
    }
}
