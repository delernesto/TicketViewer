using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TicketViewer.Data;
using TicketViewer.Services;




var builder = WebApplication.CreateBuilder(args);


//builder.Services.AddScoped<ITicketStatsService, TicketStatsService>();

// Controllers
builder.Services.AddControllers();

//  EF Core + MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 0))
    )
);

//  Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TicketViewer API",
        Version = "v1"
    });
});

//  CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// HTTPS
app.UseHttpsRedirection();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();


//Working version before changes 
//var builder = WebApplication.CreateBuilder(args);

//// Controllers
//builder.Services.AddControllers();


//// Swagger
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "TicketViewer API",
//        Version = "v1"
//    });
//});

//// CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin()
//              .AllowAnyHeader()
//              .AllowAnyMethod();
//    });
//});

//var app = builder.Build();

//app.UseCors("AllowAll");

//// HTTPS redirect
//app.UseHttpsRedirection();

//// Swagger (має бути ДО authorization!)
//app.UseSwagger();
//app.UseSwaggerUI();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
