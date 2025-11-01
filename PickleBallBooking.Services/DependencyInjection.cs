using System.Reflection;
using Azure.Storage.Blobs;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PickleBallBooking.Services.Behaviours;
using PickleBallBooking.Services.Interfaces.Services;
using PickleBallBooking.Services.Models.Configurations;
using PickleBallBooking.Services.Services;

namespace PickleBallBooking.Services;

public static class DependencyInjection
{
    public static void AddBusinessServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });

        // jwt settings
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        builder.Services.Configure<JwtSettings>(jwtSettings);

        // blob settings
        var blobSettings = builder.Configuration.GetSection("Azure:BlobStorageSettings");
        var blobConnectionString = blobSettings.GetValue<string>("ConnectionString");
        builder.Services.Configure<BlobSettings>(blobSettings);
        builder.Services.AddSingleton(new BlobServiceClient(blobConnectionString));

        // register services
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IAzureBlobService, AzureBlobService>();
        builder.Services.AddScoped<IFieldService, FieldService>();
        builder.Services.AddScoped<IPricingService, PricingService>();
        builder.Services.AddScoped<ITimeSlotService, TimeSlotService>();
    }
}
