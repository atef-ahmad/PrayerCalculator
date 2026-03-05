using Microsoft.AspNetCore.Localization;
using System.Globalization;
using PrayerTimes.Calculation;
using PrayerTimes.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPrayerTimesCalculator, PrayerTimesCalculator>();

var supportedCultures = new[] { "en", "ar", "fr", "de" }
    .Select(x => new CultureInfo(x))
    .ToList();

var app = builder.Build();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/prayertimes", (
    double lat,
    double lon,
    DateOnly date,
    string? timeZoneId,
    string? utcOffset,
    CalculationMethod method,
    Madhab madhab,
    HighLatitudeRule highLatRule,
    IPrayerTimesCalculator calc) =>
{
    var req = new PrayerTimesRequestDto(
        Latitude: lat,
        Longitude: lon,
        Date: date,
        TimeZoneId: timeZoneId,
        UtcOffset: utcOffset,
        Method: method,
        Madhab: madhab,
        HighLatitudeRule: highLatRule);

    var times = calc.Calculate(req);

    var tzResolved = !string.IsNullOrWhiteSpace(timeZoneId) ? timeZoneId! : $"UTC{utcOffset}";
    return Results.Ok(new PrayerTimesResponseDto(date, tzResolved, method, madhab, highLatRule, times));
})
.WithName("GetPrayerTimes")
.WithOpenApi();

app.MapRazorComponents<PrayerTimes.Host.App>()
    .AddInteractiveServerRenderMode();

app.Run();
