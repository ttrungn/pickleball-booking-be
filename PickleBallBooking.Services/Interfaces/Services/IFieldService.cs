using PickleBallBooking.Services.Features.Fields.Commands.CreateField;
using PickleBallBooking.Services.Features.Fields.Commands.DeleteField;
using PickleBallBooking.Services.Features.Fields.Commands.UpdateField;
using PickleBallBooking.Services.Features.Fields.Queries.GetFieldById;
using PickleBallBooking.Services.Features.Fields.Queries.GetFields;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Interfaces.Services;

public interface IFieldService
{
    Task<DataServiceResponse<Guid>> CreateFieldAsync(
        CreateFieldCommand command,
        CancellationToken cancellationToken = default);

    Task<BaseServiceResponse> UpdateFieldAsync(
        UpdateFieldCommand command,
        CancellationToken cancellationToken = default);

    Task<BaseServiceResponse> DeleteFieldAsync(
        DeleteFieldCommand command,
        CancellationToken cancellationToken = default);

    Task<DataServiceResponse<FieldResponse>> GetFieldByIdAsync(
        GetFieldByIdQuery query,
        CancellationToken cancellationToken = default);

    Task<PaginatedServiceResponse<FieldResponse>> GetFieldsAsync(
        GetFieldsQuery query,
        CancellationToken cancellationToken = default);
}
