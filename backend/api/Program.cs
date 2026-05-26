using api.configuration;
using api.middleware;
using infrastructure.email;
using infrastructure.repositories;
using services;
using services.dtos;
using services.interfaces;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

DotEnvLoader.LoadFromNearestEnvironmentFile(
    builder.Environment.ContentRootPath,
    Directory.GetCurrentDirectory(),
    AppContext.BaseDirectory);

builder.Configuration.AddEnvironmentVariables();

const string frontendCorsPolicyName = "frontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicyName, policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<ICandidateRepository>(_ =>
{
    string? defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(defaultConnectionString))
    {
        throw new InvalidOperationException(
            "ConnectionStrings__DefaultConnection es obligatorio. No se permite guardar candidatos localmente.");
    }

    return new SqlCandidateRepository(defaultConnectionString);
});

builder.Services.AddSingleton<IEmailConfirmationSender>(_ =>
{
    EmailSettings emailSettings = builder.Configuration
        .GetSection("Email")
        .Get<EmailSettings>() ?? new EmailSettings();

    EmailSettings smtpSettings = builder.Configuration
        .GetSection("Smtp")
        .Get<EmailSettings>() ?? new EmailSettings();

    bool smtpSectionIsConfigured = !string.IsNullOrWhiteSpace(smtpSettings.Host) ||
        !string.IsNullOrWhiteSpace(smtpSettings.SmtpHost);

    if (smtpSectionIsConfigured)
    {
        return new SmtpEmailConfirmationSender(smtpSettings);
    }

    if (string.Equals(emailSettings.Provider, "Smtp", StringComparison.OrdinalIgnoreCase))
    {
        return new SmtpEmailConfirmationSender(emailSettings);
    }

    throw new InvalidOperationException(
        "La configuracion SMTP es obligatoria. No se permite guardar correos localmente.");
});

builder.Services.AddScoped<ICandidateRegistrationService, CandidateRegistrationService>();

WebApplication app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors(frontendCorsPolicyName);

RouteGroupBuilder candidateRoutes = app.MapGroup("/api/candidates");

candidateRoutes.MapPost("/register", async (
    RegisterCandidateRequest registerCandidateRequest,
    ICandidateRegistrationService candidateRegistrationService,
    CancellationToken cancellationToken) =>
{
    CandidateRegistrationResponse candidateRegistrationResponse =
        await candidateRegistrationService.RegisterAsync(registerCandidateRequest, cancellationToken);

    return Results.Created(
        $"/api/candidates/{candidateRegistrationResponse.CandidateProfile.Id}",
        candidateRegistrationResponse);
});

RouteGroupBuilder employerRoutes = app.MapGroup("/api/employers");

employerRoutes.MapGet("/candidates", async (
    ICandidateRegistrationService candidateRegistrationService,
    CancellationToken cancellationToken) =>
{
    IReadOnlyCollection<CandidateProfileResponse> visibleCandidateProfiles =
        await candidateRegistrationService.GetProfilesVisibleToPartnerEmployersAsync(cancellationToken);

    return Results.Ok(visibleCandidateProfiles);
});

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "Employability Platform API"
}));

app.Run();
