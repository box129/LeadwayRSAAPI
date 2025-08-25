using Leadway_RSA_API.Data;
using Leadway_RSA_API.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization; // <--- MAKE SURE THIS IS PRESENT

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => // <--- ADD THESE LINES
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        // You can optionally add other options for cleaner JSON output, e.g.:
        // options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        // options.JsonSerializerOptions.WriteIndented = true; // For pretty-printed JSON in development
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Add your new services here ---
builder.Services.AddScoped<IApplicantService, ApplicantService>();
builder.Services.AddScoped<IIdentificationService, IdentificationService>();
builder.Services.AddScoped<IBeneficiaryService, BeneficiaryService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IAssetAllocationService, AssetAllocationService>();
builder.Services.AddScoped<IExecutorService, ExecutorService>();
builder.Services.AddScoped<IGuardianService, GuardianService>();
builder.Services.AddScoped<IPaymentTransactionService, PaymentTransactionService>();
// ------------------------------------

// Configure the DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
