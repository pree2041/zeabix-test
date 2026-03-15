
using Pricing.API.Infrastructure;
using Pricing.API.Mappings;
using Pricing.API.Service.Contracts;
using Pricing.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(typeof(RuleProfile));
builder.Services.AddAutoMapper(typeof(PriceProfile));
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient();
builder.Services.AddScoped<IPriceService, PriceService>();
builder.Services.AddScoped<IPriceRepository, PriceRepository>();
builder.Services.AddMessagingInfrastructure();


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await MessagingRegistration.RegisterSubscriptionsWithRetry(scope.ServiceProvider, builder.Configuration);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();
