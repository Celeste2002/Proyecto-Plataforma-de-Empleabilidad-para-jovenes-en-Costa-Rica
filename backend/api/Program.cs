using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using api.configuration;
using api.middleware;
using domain.constants;
using infrastructure.auth;
using infrastructure.email;
using infrastructure.repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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
            .WithOrigins(
                "http://localhost:5170", "http://127.0.0.1:5170",
                "http://localhost:5171", "http://127.0.0.1:5171",
                "http://localhost:5172", "http://127.0.0.1:5172"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// -- JWT auth --

JwtSettings jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>() ?? new JwtSettings();

if (string.IsNullOrWhiteSpace(jwtSettings.Key))
{
    throw new InvalidOperationException(
        "Jwt__Key es obligatorio. Configura la clave secreta JWT en el archivo .env.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(UserRoles.Administrator));
});

// -- Repositorios y servicios --

string defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "ConnectionStrings__DefaultConnection es obligatorio. No se permite guardar candidatos localmente.");

builder.Services.AddSingleton<ICandidateRepository>(_ =>
    new SqlCandidateRepository(defaultConnectionString));

builder.Services.AddSingleton<IEmployerRepository>(_ =>
    new SqlEmployerRepository(defaultConnectionString));

builder.Services.AddSingleton<IUserRepository>(_ =>
    new SqlUserRepository(defaultConnectionString));

builder.Services.AddSingleton<IVacanteRepository>(_ =>
    new SqlVacanteRepository(defaultConnectionString));

builder.Services.AddSingleton<IPostulacionRepository>(_ =>
    new SqlPostulacionRepository(defaultConnectionString));

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

builder.Services.AddSingleton<IEmployerActivationSender>(_ =>
{
    EmailSettings emailSettings = builder.Configuration
        .GetSection("Email")
        .Get<EmailSettings>() ?? new EmailSettings();

    EmailSettings smtpSettings = builder.Configuration
        .GetSection("Smtp")
        .Get<EmailSettings>() ?? new EmailSettings();

    bool smtpSectionIsConfigured = !string.IsNullOrWhiteSpace(smtpSettings.Host) ||
        !string.IsNullOrWhiteSpace(smtpSettings.SmtpHost);

    return new SmtpEmployerActivationSender(smtpSectionIsConfigured ? smtpSettings : emailSettings);
});

builder.Services.AddSingleton<IPasswordResetSender>(_ =>
{
    EmailSettings emailSettings = builder.Configuration
        .GetSection("Email")
        .Get<EmailSettings>() ?? new EmailSettings();

    EmailSettings smtpSettings = builder.Configuration
        .GetSection("Smtp")
        .Get<EmailSettings>() ?? new EmailSettings();

    bool smtpSectionIsConfigured = !string.IsNullOrWhiteSpace(smtpSettings.Host) ||
        !string.IsNullOrWhiteSpace(smtpSettings.SmtpHost);

    EmailSettings passwordResetEmailSettings = smtpSectionIsConfigured ? smtpSettings : emailSettings;

    string frontendUrl = builder.Configuration["App:FrontendUrl"]
        ?? builder.Configuration["App__FrontendUrl"]
        ?? "http://localhost:5170";

    return new SmtpPasswordResetSender(passwordResetEmailSettings, frontendUrl);
});

builder.Services.AddSingleton<ITokenService>(_ => new JwtTokenService(jwtSettings));
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

builder.Services.AddScoped<ICandidateRegistrationService, CandidateRegistrationService>();
builder.Services.AddScoped<IEmployerRegistrationService, EmployerRegistrationService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IVacanteService>(sp =>
    new VacanteService(
        sp.GetRequiredService<IVacanteRepository>(),
        sp.GetRequiredService<IPostulacionRepository>(),
        sp.GetRequiredService<ICandidateRepository>(),
        sp.GetRequiredService<IEmployerRepository>()));

builder.Services.AddSingleton<IMensajeRepository>(_ =>
    new SqlMensajeRepository(defaultConnectionString));

builder.Services.AddScoped<IPostulacionManagementService>(sp =>
    new PostulacionManagementService(
        sp.GetRequiredService<IVacanteRepository>(),
        sp.GetRequiredService<IPostulacionRepository>(),
        sp.GetRequiredService<IEmployerRepository>()));

builder.Services.AddScoped<IMensajeService>(sp =>
    new MensajeService(
        sp.GetRequiredService<IMensajeRepository>(),
        sp.GetRequiredService<IPostulacionRepository>(),
        sp.GetRequiredService<IVacanteRepository>(),
        sp.GetRequiredService<IEmployerRepository>(),
        sp.GetRequiredService<ICandidateRepository>()));

WebApplication app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors(frontendCorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

// -- Rutas de candidatos --

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

candidateRoutes.MapGet("/me", async (
    ClaimsPrincipal user,
    ICandidateRegistrationService candidateRegistrationService,
    CancellationToken cancellationToken) =>
{
    CandidateProfileResponse candidateProfileResponse =
        await candidateRegistrationService.GetProfileByUserIdAsync(GetAuthenticatedUserId(user), cancellationToken);

    return Results.Ok(candidateProfileResponse);
}).RequireAuthorization();

candidateRoutes.MapPut("/me", async (
    ClaimsPrincipal user,
    UpdateCandidateProfileRequest updateCandidateProfileRequest,
    ICandidateRegistrationService candidateRegistrationService,
    CancellationToken cancellationToken) =>
{
    CandidateProfileResponse candidateProfileResponse =
        await candidateRegistrationService.UpdateProfileAsync(
            GetAuthenticatedUserId(user),
            updateCandidateProfileRequest,
            cancellationToken);

    return Results.Ok(candidateProfileResponse);
}).RequireAuthorization();

candidateRoutes.MapPut("/me/password", async (
    ClaimsPrincipal user,
    UpdateCandidatePasswordRequest updateCandidatePasswordRequest,
    ICandidateRegistrationService candidateRegistrationService,
    CancellationToken cancellationToken) =>
{
    await candidateRegistrationService.UpdatePasswordAsync(
        GetAuthenticatedUserId(user),
        updateCandidatePasswordRequest,
        cancellationToken);

    return Results.Ok(new { message = "Contraseña actualizada correctamente." });
}).RequireAuthorization();

// -- Rutas de empleadores --

RouteGroupBuilder employerRoutes = app.MapGroup("/api/employers");

employerRoutes.MapPost("/register", async (
    RegisterEmployerRequest registerEmployerRequest,
    IEmployerRegistrationService employerRegistrationService,
    CancellationToken cancellationToken) =>
{
    EmployerRegistrationResponse response =
        await employerRegistrationService.RegisterAsync(registerEmployerRequest, cancellationToken);

    return Results.Created(
        $"/api/employers/{response.EmployerProfile.Id}",
        response);
});

employerRoutes.MapGet("/me", async (
    ClaimsPrincipal user,
    IEmployerRegistrationService employerRegistrationService,
    CancellationToken cancellationToken) =>
{
    EmployerProfileResponse profile =
        await employerRegistrationService.GetProfileByUserIdAsync(GetAuthenticatedUserId(user), cancellationToken);

    return Results.Ok(profile);
}).RequireAuthorization();

employerRoutes.MapGet("/candidates", async (
    ICandidateRegistrationService candidateRegistrationService,
    CancellationToken cancellationToken) =>
{
    IReadOnlyCollection<CandidateProfileResponse> visibleCandidateProfiles =
        await candidateRegistrationService.GetProfilesVisibleToPartnerEmployersAsync(cancellationToken);

    return Results.Ok(visibleCandidateProfiles);
}).RequireAuthorization();

employerRoutes.MapGet("/me/vacantes", async (
    ClaimsPrincipal user,
    IVacanteService vacanteService,
    CancellationToken cancellationToken) =>
{
    IReadOnlyCollection<VacanteResponse> vacantes =
        await vacanteService.GetMyVacantesAsync(GetAuthenticatedUserId(user), cancellationToken);

    return Results.Ok(vacantes);
}).RequireAuthorization();

employerRoutes.MapPost("/me/vacantes", async (
    ClaimsPrincipal user,
    CreateVacanteRequest createVacanteRequest,
    IVacanteService vacanteService,
    CancellationToken cancellationToken) =>
{
    VacanteResponse vacante =
        await vacanteService.CreateVacanteAsync(GetAuthenticatedUserId(user), createVacanteRequest, cancellationToken);

    return Results.Created($"/api/employers/me/vacantes/{vacante.Id}", vacante);
}).RequireAuthorization();

// HU7 — Panel de gestión de candidatos
employerRoutes.MapGet("/me/candidatos", async (
    ClaimsPrincipal user,
    IPostulacionManagementService postulacionManagementService,
    CancellationToken cancellationToken) =>
{
    IReadOnlyCollection<VacanteConPostulantesResponse> resultado =
        await postulacionManagementService.GetPostulantesAgrupadosByVacanteAsync(
            GetAuthenticatedUserId(user), cancellationToken);

    return Results.Ok(resultado);
}).RequireAuthorization();

employerRoutes.MapPut("/me/postulaciones/{id:guid}/status", async (
    Guid id,
    ClaimsPrincipal user,
    UpdatePostulacionStatusRequest request,
    IPostulacionManagementService postulacionManagementService,
    CancellationToken cancellationToken) =>
{
    UpdatePostulacionStatusResponse response =
        await postulacionManagementService.UpdatePostulacionStatusAsync(
            GetAuthenticatedUserId(user), id, request, cancellationToken);

    return Results.Ok(response);
}).RequireAuthorization();

// -- Rutas de autenticacion --

RouteGroupBuilder authRoutes = app.MapGroup("/api/auth");

authRoutes.MapPost("/login", async (
    LoginRequest loginRequest,
    IAuthService authService,
    CancellationToken cancellationToken) =>
{
    LoginResponse loginResponse = await authService.LoginAsync(loginRequest, cancellationToken);
    return Results.Ok(loginResponse);
});

authRoutes.MapPost("/forgot-password", async (
    ForgotPasswordRequest forgotPasswordRequest,
    IAuthService authService,
    CancellationToken cancellationToken) =>
{
    await authService.RequestPasswordResetAsync(forgotPasswordRequest, cancellationToken);
    // Siempre responder 200 para no revelar si el correo existe
    return Results.Ok(new { message = "Si tu correo está registrado, recibirás un enlace seguro para restablecer tu contraseña." });
});

authRoutes.MapPost("/reset-password", async (
    ResetPasswordRequest resetPasswordRequest,
    IAuthService authService,
    CancellationToken cancellationToken) =>
{
    await authService.ResetPasswordAsync(resetPasswordRequest, cancellationToken);
    return Results.Ok(new { message = "Tu contraseña fue restablecida correctamente." });
});

// -- Rutas de administrador --

RouteGroupBuilder adminRoutes = app.MapGroup("/api/admin").RequireAuthorization("AdminOnly");

adminRoutes.MapGet("/users", async (
    IAdminService adminService,
    CancellationToken cancellationToken) =>
{
    IReadOnlyCollection<UserSummaryResponse> users =
        await adminService.GetAllUsersAsync(cancellationToken);

    return Results.Ok(users);
});

adminRoutes.MapPut("/users/{id:guid}/role", async (
    Guid id,
    UpdateUserRoleRequest updateUserRoleRequest,
    IAdminService adminService,
    CancellationToken cancellationToken) =>
{
    await adminService.UpdateUserRoleAsync(id, updateUserRoleRequest.NewRole, cancellationToken);
    return Results.NoContent();
});

adminRoutes.MapPost("/employers/{id:guid}/activate", async (
    Guid id,
    IEmployerRegistrationService employerRegistrationService,
    CancellationToken cancellationToken) =>
{
    await employerRegistrationService.ActivateAsync(id, cancellationToken);
    return Results.Ok(new { message = "Empleador activado correctamente." });
});

// -- Rutas de vacantes y postulaciones --

RouteGroupBuilder vacanteRoutes = app.MapGroup("/api/vacantes");

vacanteRoutes.MapGet("/", async (
    IVacanteService vacanteService,
    CancellationToken cancellationToken) =>
{
    IReadOnlyCollection<VacanteResponse> vacantes =
        await vacanteService.GetActiveVacantesAsync(cancellationToken);

    return Results.Ok(vacantes);
}).RequireAuthorization();

RouteGroupBuilder postulacionRoutes = app.MapGroup("/api/postulaciones");

postulacionRoutes.MapPost("/", async (
    ClaimsPrincipal user,
    ApplyToVacanteRequest applyToVacanteRequest,
    IVacanteService vacanteService,
    CancellationToken cancellationToken) =>
{
    await vacanteService.ApplyAsync(GetAuthenticatedUserId(user), applyToVacanteRequest, cancellationToken);
    return Results.Created("/api/postulaciones", new { message = "Postulación enviada correctamente." });
}).RequireAuthorization();

candidateRoutes.MapGet("/me/postulaciones", async (
    ClaimsPrincipal user,
    IVacanteService vacanteService,
    CancellationToken cancellationToken) =>
{
    IReadOnlyCollection<PostulacionResponse> postulaciones =
        await vacanteService.GetMyPostulacionesAsync(GetAuthenticatedUserId(user), cancellationToken);

    return Results.Ok(postulaciones);
}).RequireAuthorization();

// HU7 — Bandeja de entrada de mensajes del candidato
candidateRoutes.MapGet("/me/mensajes", async (
    ClaimsPrincipal user,
    IMensajeService mensajeService,
    CancellationToken cancellationToken) =>
{
    IReadOnlyCollection<MensajeResponse> mensajes =
        await mensajeService.GetMisBandejaEntradaAsync(GetAuthenticatedUserId(user), cancellationToken);

    return Results.Ok(mensajes);
}).RequireAuthorization();

// HU7 — Envío de mensajes del empleador al candidato
app.MapPost("/api/mensajes", async (
    ClaimsPrincipal user,
    SendMensajeRequest request,
    IMensajeService mensajeService,
    CancellationToken cancellationToken) =>
{
    MensajeResponse response =
        await mensajeService.SendMensajeAsync(GetAuthenticatedUserId(user), request, cancellationToken);

    return Results.Created($"/api/mensajes/{response.Id}", response);
}).RequireAuthorization();

// -- Health check --

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "Employability Platform API"
}));

// -- Dev utilities (solo disponibles en Development) --

if (app.Environment.IsDevelopment())
{
    app.MapGet("/api/dev/hash", (string password, IPasswordHasher passwordHasher) =>
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return Results.BadRequest(new { error = "El parametro 'password' es obligatorio." });
        }

        string hash = passwordHasher.Hash(password);
        return Results.Ok(new { password, hash });
    });
}

app.Run();

static Guid GetAuthenticatedUserId(ClaimsPrincipal user)
{
    string? userId = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
        user.FindFirstValue(ClaimTypes.NameIdentifier);

    if (!Guid.TryParse(userId, out Guid parsedUserId))
    {
        throw new UnauthorizedAccessException("Token de usuario invalido.");
    }

    return parsedUserId;
}
