using QuestPDF.Infrastructure;
using Vetcare.Application;
using Vetcare.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();
QuestPDF.Settings.License = LicenseType.Community;

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}
app.UseDefaultFiles();
app.UseStaticFiles();
await app.Services.InitializeDatabaseAsync();

app.MapControllers();
app.Run();

public partial class Program { }
