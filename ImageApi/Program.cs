using Alcatraz.ImageApi.Shared.Services;
using ImageApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddOptions<ImageApiOptions>()
    .BindConfiguration(ImageApiOptions.CONFIG_NAME)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddGrpc();

builder.Services.AddDbContext<ImageDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
});

var app = builder.Build();

var apiOptions = app.Services.GetRequiredService<IOptions<ImageApiOptions>>().Value;
var saveDirectory = new DirectoryInfo($"{apiOptions.SaveLocation!}/image-api");
if (!saveDirectory.Exists)
{
    app.Logger.LogInformation("Creating save directory");
    saveDirectory.Create();
}
app.Logger.LogInformation("Save directory is present at {Path}", saveDirectory.FullName);
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ImageDbContext>();
await db.Database.MigrateAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseAuthorization();
app.MapGrpcService<ImageUploadService>();
app.MapControllers();

app.Run();