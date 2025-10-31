using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Features.Fields.Commands.DeleteField;

public record DeleteFieldCommand : IRequest<BaseServiceResponse>
{
    public Guid Id { get; init; }
}

public class DeleteFieldCommandValidator : AbstractValidator<DeleteFieldCommand>
{
    public DeleteFieldCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Field ID is required!");
    }
}

public class DeleteFieldCommandHandler : IRequestHandler<DeleteFieldCommand, BaseServiceResponse>
{
    private readonly IFieldService _fieldService;
    private readonly ILogger<DeleteFieldCommandHandler> _logger;

    public DeleteFieldCommandHandler(IFieldService fieldService, ILogger<DeleteFieldCommandHandler> logger)
    {
        _fieldService = fieldService;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(DeleteFieldCommand request, CancellationToken cancellationToken)
    {
        var response = await _fieldService.DeleteFieldAsync(request, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to delete field: {Message}", response.Message);
            return response;
        }
        _logger.LogInformation("Field deleted successfully with ID: {FieldId}", request.Id);
        return response;
    }
}
