using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.Fields.Commands.CreateField;

public record CreateFieldCommand : IRequest<DataServiceResponse<Guid>>
{
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

public class CreateFieldCommandValidator : AbstractValidator<CreateFieldCommand>
{
    public CreateFieldCommandValidator()
    {
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

public class CreateFieldCommandHandler : IRequestHandler<CreateFieldCommand, DataServiceResponse<Guid>>
{
    private readonly IFieldService _fieldService;
    private readonly ILogger<CreateFieldCommandHandler> _logger;

    public CreateFieldCommandHandler(IFieldService fieldService, ILogger<CreateFieldCommandHandler> logger)
    {
        _fieldService = fieldService;
        _logger = logger;
    }

    public async Task<DataServiceResponse<Guid>> Handle(CreateFieldCommand request, CancellationToken cancellationToken)
    {
        var response = await _fieldService.CreateFieldAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to create field: {Message}", response.Message);
            return response;
        }
        _logger.LogInformation("Field created successfully with ID: {FieldId}", response.Data);
        return response;
    }
}
