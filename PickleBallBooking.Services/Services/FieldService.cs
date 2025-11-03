using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PickleBallBooking.Domain.Entities;
using PickleBallBooking.Repositories.Interfaces.Repositories;
using PickleBallBooking.Services.Features.Fields.Commands.CreateField;
using PickleBallBooking.Services.Features.Fields.Commands.DeleteField;
using PickleBallBooking.Services.Features.Fields.Commands.UpdateField;
using PickleBallBooking.Services.Features.Fields.Queries.GetFieldById;
using PickleBallBooking.Services.Features.Fields.Queries.GetFields;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Configurations;
using PickleBallBooking.Services.Models.Responses;
using PickleBallBooking.Services.Models.Responses.Fields;

namespace PickleBallBooking.Services.Services;

public class FieldService : IFieldService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAzureBlobService _blobService;
    private readonly BlobSettings _blobSettings;

    public FieldService(IUnitOfWork unitOfWork, IAzureBlobService blobService, IOptions<BlobSettings> blobSettings)
    {
        _unitOfWork = unitOfWork;
        _blobService = blobService;
        _blobSettings = blobSettings.Value;
    }

    public async Task<DataServiceResponse<Guid>> CreateFieldAsync(CreateFieldCommand command,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        string? imageUrl = null;
        string? bluePrintImageUrl = null;

        if (command.ImageUrl != null)
        {
            var stream = command.ImageUrl.OpenReadStream();
            var imageName = id + Path.GetExtension(command.ImageUrl.FileName);
            imageUrl = await _blobService.UploadAsync(stream, imageName, _blobSettings.DefaultContainer);
        }

        if (command.BluePrintImageUrl != null)
        {
            var stream = command.BluePrintImageUrl.OpenReadStream();
            var imageName = "blueprint-" + id + Path.GetExtension(command.BluePrintImageUrl.FileName);
            bluePrintImageUrl = await _blobService.UploadAsync(stream, imageName, _blobSettings.DefaultContainer);
        }

        var type = await _unitOfWork.GetRepository<FieldType>()
            .Query()
            .FirstOrDefaultAsync(t => t.Id == command.FieldTypeId, cancellationToken);
        if (type == null)
        {
            return new DataServiceResponse<Guid>()
            {
                Success = false,
                Message = "Field type not found!",
                Data = Guid.Empty
            };
        }

        var field = new Field
        {
            Id = id,
            Name = command.Name,
            Description = command.Description,
            Address = command.Address,
            PricePerHour = command.PricePerHour,
            ImageUrl = imageUrl,
            Area = command.Area,
            BluePrintImageUrl = bluePrintImageUrl,
            Latitude = command.Latitude,
            Longitude = command.Longitude,
            MapUrl = command.MapUrl,
            City = command.City,
            District = command.District,
            FieldTypeId = command.FieldTypeId
        };

        await _unitOfWork.GetRepository<Field>().InsertAsync(field, cancellationToken);
        await _unitOfWork.SaveChangesAsync();

        return new DataServiceResponse<Guid>()
        {
            Success = true, Message = "Field created successfully", Data = id
        };
    }

    public async Task<BaseServiceResponse> UpdateFieldAsync(UpdateFieldCommand command, CancellationToken cancellationToken = default)
    {
        var field = await _unitOfWork.GetRepository<Field>()
            .Query()
            .FirstOrDefaultAsync(f => f.Id == command.Id, cancellationToken);
        if (field == null)
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = "Field not found!"
            };
        }

        var type = await _unitOfWork.GetRepository<FieldType>()
            .Query()
            .FirstOrDefaultAsync(t => t.Id == command.FieldTypeId, cancellationToken);
        if (type == null)
        {
            return new BaseServiceResponse()
            {
                Success = false,
                Message = "Field type not found!",
            };
        }

        string? imageUrl = null;
        string? bluePrintImageUrl = null;

        if (command.ImageUrl != null)
        {
            var stream = command.ImageUrl.OpenReadStream();
            var imageName = command.Id + Path.GetExtension(command.ImageUrl.FileName);
            imageUrl = await _blobService.UploadAsync(stream, imageName, _blobSettings.DefaultContainer);
        }

        if (command.BluePrintImageUrl != null)
        {
            var stream = command.BluePrintImageUrl.OpenReadStream();
            var imageName = "blueprint-" + command.Id + Path.GetExtension(command.BluePrintImageUrl.FileName);
            bluePrintImageUrl = await _blobService.UploadAsync(stream, imageName, _blobSettings.DefaultContainer);
        }

        field.Name = command.Name;
        field.Description = command.Description;
        field.Address = command.Address;
        field.PricePerHour = command.PricePerHour;
        field.ImageUrl = imageUrl ?? field.ImageUrl;
        field.Area = command.Area;
        field.BluePrintImageUrl = bluePrintImageUrl ?? field.BluePrintImageUrl;
        field.Latitude = command.Latitude;
        field.Longitude = command.Longitude;
        field.MapUrl = command.MapUrl;
        field.City = command.City;
        field.District = command.District;
        field.FieldTypeId = command.FieldTypeId;

        await _unitOfWork.GetRepository<Field>().UpdateAsync(field);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse()
        {
            Success = true,
            Message = "Field updated successfully!"
        };
    }

    public async Task<BaseServiceResponse> DeleteFieldAsync(DeleteFieldCommand command, CancellationToken cancellationToken = default)
    {
        var field = await _unitOfWork.GetRepository<Field>()
            .Query()
            .FirstOrDefaultAsync(f => f.Id == command.Id && f.IsActive, cancellationToken);
        if (field == null)
        {
            return new BaseServiceResponse() { Success = false, Message = "Field not found!" };
        }

        field.IsActive = false;
        await _unitOfWork.GetRepository<Field>().UpdateAsync(field);
        await _unitOfWork.SaveChangesAsync();

        return new BaseServiceResponse() { Success = true, Message = "Field deleted successfully!" };
    }

    public async Task<DataServiceResponse<FieldResponse>> GetFieldByIdAsync(GetFieldByIdQuery query, CancellationToken cancellationToken = default)
    {
        var field = await _unitOfWork.GetRepository<Field>()
            .Query()
            .Include(f => f.FieldType)
            .FirstOrDefaultAsync(f => f.Id == query.Id && f.IsActive, cancellationToken);
        if (field == null)
        {
            return new DataServiceResponse<FieldResponse>()
            {
                Success = false, Message = "Field not found!", Data = null!
            };
        }

        var response = new FieldResponse
        {
            Id = field.Id,
            Name = field.Name,
            Description = field.Description,
            Address = field.Address,
            PricePerHour = field.PricePerHour,
            ImageUrl = field.ImageUrl,
            Area = field.Area,
            BluePrintImageUrl = field.BluePrintImageUrl,
            Latitude = field.Latitude,
            Longitude = field.Longitude,
            MapUrl = field.MapUrl,
            City = field.City,
            District = field.District,
            FieldType = new FieldTypeResponse
            {
                Id = field.FieldType.Id,
                Name = field.FieldType.Name,
                Description = field.FieldType.Description,
                IsActive = field.FieldType.IsActive
            }
        };

        return new DataServiceResponse<FieldResponse>()
        {
            Success = true, Message = "Field retrieved successfully!", Data = response
        };
    }

    public async Task<PaginatedServiceResponse<FieldResponse>> GetFieldsAsync(GetFieldsQuery query, CancellationToken cancellationToken = default)
    {
        var repository = _unitOfWork.GetRepository<Field>();

        var predicate = repository
            .Query()
            .AsNoTracking()
            .Where(f => f.IsActive == query.IsActive &&
                        (!query.MinPrice.HasValue || f.PricePerHour >= query.MinPrice.Value) &&
                        (!query.MaxPrice.HasValue || f.PricePerHour <= query.MaxPrice.Value) &&
                        (string.IsNullOrEmpty(query.Name) || f.Name.Contains(query.Name)))
            .OrderByDescending(f => f.CreatedAt);
        var totalCounts = await predicate.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCounts / (double)query.PageSize);
        var fields = await predicate
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var response = fields.Select(
            f => new FieldResponse
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                Address = f.Address,
                FieldTypeId = f.FieldTypeId,
                PricePerHour = f.PricePerHour,
                ImageUrl = f.ImageUrl,
                Area = f.Area,
                BluePrintImageUrl = f.BluePrintImageUrl,
                Latitude = f.Latitude,
                Longitude = f.Longitude,
                MapUrl = f.MapUrl,
                City = f.City,
                District = f.District,
            }).ToList();

        return new PaginatedServiceResponse<FieldResponse>()
        {
            Success = true,
            Message = "Fields retrieved successfully!",
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCounts,
            TotalPages = totalPages,
            Data = response,
        };
    }
}
