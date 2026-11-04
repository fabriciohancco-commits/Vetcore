using Microsoft.EntityFrameworkCore;
using Vetcare.API;
using Vetcare.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<VetCareDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("VetCare") ?? "Data Source=vetcare.db"));
builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VetCareDbContext>();
    db.Database.EnsureCreated();
}

ApiEndpoints.Map(app);
app.Run();
