﻿namespace PickleBallBooking.Services.Models.Configurations;

public class BlobSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DefaultContainer { get; set; } = null!;
}
