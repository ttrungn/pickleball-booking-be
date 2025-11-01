using PickleBallBooking.Services.Features.FieldTypes.Queries.GetFieldTypes;
using PickleBallBooking.Services.Models.Responses;

namespace PickleBallBooking.Services.Interfaces.Services;

public interface IFieldTypeService
{
    Task<PaginatedServiceResponse<FieldTypeResponse>> GetFieldTypesAsync(
        GetFieldTypesQuery query,
        CancellationToken cancellationToken = default);
}
