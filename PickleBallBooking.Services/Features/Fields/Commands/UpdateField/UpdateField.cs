using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.Fields.Commands.UpdateField;

public record UpdateFieldCommand : IRequest<BaseServiceResponse>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string Address { get; init; } = null!;
    public decimal PricePerHour { get; init; }
    public IFormFile? ImageUrl { get; init; }
    public decimal? Area { get; init; }
    public IFormFile? BluePrintImageUrl { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string? MapUrl { get; init; }
    public string? City { get; init; }
    public string? District { get; init; }
    public Guid FieldTypeId { get; init; }
}

public class UpdateFieldCommandValidator : AbstractValidator<UpdateFieldCommand>
{
    public UpdateFieldCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Field ID is required!");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Field name is required!");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required!");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required!");

        RuleFor(x => x.PricePerHour)
            .GreaterThan(0).WithMessage("Price per hour must be greater than zero!");

        RuleFor(x => x.FieldTypeId)
            .NotEmpty().WithMessage("Field type is required!");
    }
}

public class UpdateFieldCommandHandler : IRequestHandler<UpdateFieldCommand, BaseServiceResponse>
{
    private readonly IFieldService _fieldService;
    private readonly ILogger<UpdateFieldCommandHandler> _logger;

    public UpdateFieldCommandHandler(IFieldService fieldService, ILogger<UpdateFieldCommandHandler> logger)
    {
        _fieldService = fieldService;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(UpdateFieldCommand request, CancellationToken cancellationToken)
    {
        var response = await _fieldService.UpdateFieldAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to update field: {Message}", response.Message);
            return response;
        }
        _logger.LogInformation("Field update successfully with ID: {FieldId}", request.Id);
        return response;
    }
}
