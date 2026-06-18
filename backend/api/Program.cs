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
    options.AddPolicy("CandidateOnly", policy =>
        policy.RequireRole(UserRoles.Candidate));

    options.AddPolicy("EmployerOnly", policy =>
        policy.RequireRole(UserRoles.Employer));

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

builder.Services.AddSingleton<INotificacionRepository>(_ =>
    new SqlNotificacionRepository(defaultConnectionString));

builder.Services.AddSingleton<IMicroCursoRepository>(_ =>
    new SqlMicroCursoRepository(defaultConnectionString));

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

builder.Services.AddSingleton<IInterviewRequestSender>(_ =>
{
    EmailSettings emailSettings = builder.Configuration
        .GetSection("Email")
        .Get<EmailSettings>() ?? new EmailSettings();

    EmailSettings smtpSettings = builder.Configuration
        .GetSection("Smtp")
        .Get<EmailSettings>() ?? new EmailSettings();

    bool smtpSectionIsConfigured = !string.IsNullOrWhiteSpace(smtpSettings.Host) ||
        !string.IsNullOrWhiteSpace(smtpSettings.SmtpHost);

    return new SmtpInterviewRequestSender(smtpSectionIsConfigured ? smtpSettings : emailSettings);
});

builder.Services.AddSingleton<ITokenService>(_ => new JwtTokenService(jwtSettings));
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

builder.Services.AddScoped<ICandidateRegistrationService, CandidateRegistrationService>();
builder.Services.AddScoped<IEmployerRegistrationService, EmployerRegistrationService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IAdminReportRepository>(_ =>
    new SqlAdminReportRepository(defaultConnectionString));

builder.Services.AddScoped<IAdminService>(sp =>
    new AdminService(
        sp.GetRequiredService<IUserRepository>(),
        sp.GetRequiredService<IAdminReportRepository>()));
builder.Services.AddScoped<IVacanteService>(sp =>
    new VacanteService(
        sp.GetRequiredService<IVacanteRepository>(),
        sp.GetRequiredService<IPostulacionRepository>(),
        sp.GetRequiredService<ICandidateRepository>(),
        sp.GetRequiredService<IEmployerRepository>(),
        sp.GetRequiredService<INotificacionRepository>(),
        sp.GetRequiredService<IInterviewRequestSender>()));

builder.Services.AddScoped<IEmployerPostulacionService>(sp =>
    new EmployerPostulacionService(
        sp.GetRequiredService<IEmployerRepository>(),
        sp.GetRequiredService<IVacanteRepository>(),
        sp.GetRequiredService<IPostulacionRepository>(),
        sp.GetRequiredService<INotificacionRepository>()));

builder.Services.AddScoped<IMicroCursoService>(sp =>
    new MicroCursoService(
        sp.GetRequiredService<IMicroCursoRepository>(),
        sp.GetRequiredService<ICandidateRepository>()));

WebApplication app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors(frontendCorsPolicyName);

// Impide que el navegador almacene en caché las respuestas de la API
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, private";
    context.Response.Headers["Pragma"] = "no-cache";
    await next();
});

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
}).RequireAuthorization("CandidateOnly");

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
}).RequireAuthorization("CandidateOnly");

candidateRoutes.MapGet("/me/perfil", async (
    ClaimsPrincipal user,
    ICandidateRegistrationService candidateRegistrationService,
    CancellationToken cancellationToken) =>
{
    CandidatoPerfilCompletoResponse perfil =
        await candidateRegistrationService.GetFullProfileAsync(GetAuthenticatedUserId(user), cancellationToken);

    return Results.Ok(perfil);
}).RequireAuthorization("CandidateOnly");

candidateRoutes.MapPatch("/me/disponibilidad", async (
    ClaimsPrincipal user,
    UpdateAvailabilityRequest request,
    ICandidateRegistrationService candidateRegistrationService,
    CancellationToken cancellationToken) =>
{
    await candidateRegistrationService.UpdateAvailabilityAsync(
        GetAuthenticatedUserId(user), request.IsAvailableForContact, cancellationToken);

    return Results.Ok(new { message = "Disponibilidad actualizada correctamente." });
}).RequireAuthorization("CandidateOnly");

candidateRoutes.MapPost("/me/experiencias", async (
    ClaimsPrincipal user,
    AddExperienciaLaboralRequest request,
    ICandidateRegistrationService candidateRegistrationService,
    CancellationToken cancellationToken) =>
{
    ExperienciaLaboralResponse response =
        await candidateRegistrationService.AddExperienciaAsync(GetAuthenticatedUserId(user), request, cancellationToken);

    return Results.Created($"/api/candidates/me/experiencias/{response.Id}", response);
}).RequireAuthorization("CandidateOnly");

candidateRoutes.MapDelete("/me/experiencias/{id:guid}", async (
    Guid id,
    ClaimsPrincipal user,
    ICandidateRegistrationService candidateRegistrationService,
    CancellationToken cancellationToken) =>
{
    await candidateRegistrationService.DeleteExperienciaAsync(GetAuthenticatedUserId(user), id, cancellationToken);
    return Results.NoContent();
}).RequireAuthorization("CandidateOnly");

candidateRoutes.MapPost("/me/habilidades", async (
    ClaimsPrincipal user,
    AddHabilidadRequest request,
    ICandidateRegistrationService candidateRegistrationService,
    CancellationToken cancellationToken) =>
{
    HabilidadResponse response =
        await candidateRegistrationService.AddHabilidadAsync(GetAuthenticatedUserId(user), request, cancellationToken);

    return Results.Created($"/api/candidates/me/habilidades/{response.Id}", response);
}).RequireAuthorization("CandidateOnly");

candidateRoutes.MapDelete("/me/habilidades/{id:guid}", async (
    Guid id,
    ClaimsPrincipal user,
    ICandidateRegistrationService candidateRegistrationService,
    CancellationToken cancellationToken) =>
{
    await candidateRegistrationService.DeleteHabilidadAsync(GetAuthenticatedUserId(user), id, cancellationToken);
    return Results.NoContent();
}).RequireAuthorization("CandidateOnly");

candidateRoutes.MapPost("/me/cursos", async (
    ClaimsPrincipal user,
    AddCursoCompletadoRequest request,
    ICandidateRegistrationService candidateRegistrationService,
    CancellationToken cancellationToken) =>
{
    CursoCompletadoResponse response =
        await candidateRegistrationService.AddCursoAsync(GetAuthenticatedUserId(user), request, cancellationToken);

    return Results.Created($"/api/candidates/me/cursos/{response.Id}", response);
}).RequireAuthorization("CandidateOnly");

candidateRoutes.MapDelete("/me/cursos/{id:guid}", async (
    Guid id,
    ClaimsPrincipal user,
    ICandidateRegistrationService candidateRegistrationService,
    CancellationToken cancellationToken) =>
{
    await candidateRegistrationService.DeleteCursoAsync(GetAuthenticatedUserId(user), id, cancellationToken);
    return Results.NoContent();
}).RequireAuthorization("CandidateOnly");

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
}).RequireAuthorization("CandidateOnly");

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
}).RequireAuthorization("EmployerOnly");

employerRoutes.MapGet("/candidates", async (
    ICandidateRegistrationService candidateRegistrationService,
    CancellationToken cancellationToken) =>
{
    IReadOnlyCollection<CandidateProfileResponse> visibleCandidateProfiles =
        await candidateRegistrationService.GetProfilesVisibleToPartnerEmployersAsync(cancellationToken);

    return Results.Ok(visibleCandidateProfiles);
}).RequireAuthorization("EmployerOnly");

employerRoutes.MapGet("/me/vacantes", async (
    ClaimsPrincipal user,
    IVacanteService vacanteService,
    CancellationToken cancellationToken) =>
{
    IReadOnlyCollection<VacanteResponse> vacantes =
        await vacanteService.GetMyVacantesAsync(GetAuthenticatedUserId(user), cancellationToken);

    return Results.Ok(vacantes);
}).RequireAuthorization("EmployerOnly");

employerRoutes.MapPut("/me/vacantes/{id:guid}/status", async (
    ClaimsPrincipal user,
    Guid id,
    UpdateVacanteStatusRequest updateVacanteStatusRequest,
    IVacanteService vacanteService,
    CancellationToken cancellationToken) =>
{
    VacanteResponse vacante =
        await vacanteService.UpdateMyVacanteStatusAsync(
            GetAuthenticatedUserId(user),
            id,
            updateVacanteStatusRequest,
            cancellationToken);

    return Results.Ok(vacante);
}).RequireAuthorization("EmployerOnly");

employerRoutes.MapPost("/me/vacantes", async (
    ClaimsPrincipal user,
    CreateVacanteRequest createVacanteRequest,
    IVacanteService vacanteService,
    CancellationToken cancellationToken) =>
{
    VacanteResponse vacante =
        await vacanteService.CreateVacanteAsync(GetAuthenticatedUserId(user), createVacanteRequest, cancellationToken);

    return Results.Created($"/api/employers/me/vacantes/{vacante.Id}", vacante);
}).RequireAuthorization("EmployerOnly");

employerRoutes.MapPatch("/me/vacantes/{vacanteId:guid}/estado", async (
    Guid vacanteId,
    ClaimsPrincipal user,
    UpdateVacanteStatusRequest request,
    IVacanteService vacanteService,
    CancellationToken cancellationToken) =>
{
    VacanteResponse vacante =
        await vacanteService.UpdateVacanteStatusAsync(
            GetAuthenticatedUserId(user),
            vacanteId,
            request.IsActive,
            cancellationToken);

    return Results.Ok(vacante);
}).RequireAuthorization("EmployerOnly");

employerRoutes.MapPut("/me/vacantes/{id:guid}", async (
    Guid id,
    ClaimsPrincipal user,
    UpdateVacanteRequest updateVacanteRequest,
    IVacanteService vacanteService,
    CancellationToken cancellationToken) =>
{
    VacanteResponse vacante =
        await vacanteService.UpdateVacanteAsync(
            GetAuthenticatedUserId(user),
            id,
            updateVacanteRequest,
            cancellationToken);

    return Results.Ok(vacante);
}).RequireAuthorization("EmployerOnly");

employerRoutes.MapGet("/me/vacantes/{id:guid}/postulaciones", async (
    Guid id,
    ClaimsPrincipal user,
    IVacanteService vacanteService,
    CancellationToken cancellationToken) =>
{
    IReadOnlyCollection<EmployerPostulacionResponse> postulaciones =
        await vacanteService.GetPostulacionesByVacanteAsync(
            GetAuthenticatedUserId(user),
            id,
            cancellationToken);

    return Results.Ok(postulaciones);
}).RequireAuthorization("EmployerOnly");

employerRoutes.MapGet("/me/postulaciones/{postulacionId:guid}", async (
    Guid postulacionId,
    ClaimsPrincipal user,
    IEmployerPostulacionService employerPostulacionService,
    CancellationToken cancellationToken) =>
{
    PostulacionDetailResponse detail =
        await employerPostulacionService.GetPostulacionDetailAsync(
            GetAuthenticatedUserId(user), postulacionId, cancellationToken);

    return Results.Ok(detail);
}).RequireAuthorization("EmployerOnly");

employerRoutes.MapPost("/me/postulaciones/{postulacionId:guid}/solicitar-entrevista", async (
    Guid postulacionId,
    ClaimsPrincipal user,
    IVacanteService vacanteService,
    CancellationToken cancellationToken) =>
{
    EmployerPostulacionResponse postulacion =
        await vacanteService.RequestInterviewAsync(
            GetAuthenticatedUserId(user), postulacionId, cancellationToken);

    return Results.Ok(postulacion);
}).RequireAuthorization("EmployerOnly");

employerRoutes.MapPut("/me/postulaciones/{postulacionId:guid}/status", async (
    Guid postulacionId,
    UpdatePostulacionStatusRequest request,
    ClaimsPrincipal user,
    IEmployerPostulacionService employerPostulacionService,
    CancellationToken cancellationToken) =>
{
    await employerPostulacionService.UpdatePostulacionStatusAsync(
        GetAuthenticatedUserId(user), postulacionId, request.Status, cancellationToken);

    return Results.Ok(new { message = $"Estado actualizado a '{request.Status}'." });
}).RequireAuthorization("EmployerOnly");

employerRoutes.MapGet("/me/notificaciones/unread-count", async (
    ClaimsPrincipal user,
    IEmployerPostulacionService employerPostulacionService,
    CancellationToken cancellationToken) =>
{
    int count = await employerPostulacionService.GetUnreadNotificacionCountAsync(
        GetAuthenticatedUserId(user), cancellationToken);

    return Results.Ok(new { count });
}).RequireAuthorization("EmployerOnly");

employerRoutes.MapGet("/me/notificaciones", async (
    Guid? vacanteId,
    ClaimsPrincipal user,
    IEmployerPostulacionService employerPostulacionService,
    CancellationToken cancellationToken) =>
{
    IReadOnlyCollection<NotificacionResponse> notificaciones =
        await employerPostulacionService.GetNotificacionesAsync(
            GetAuthenticatedUserId(user), vacanteId, cancellationToken);

    return Results.Ok(notificaciones);
}).RequireAuthorization("EmployerOnly");

employerRoutes.MapPut("/me/notificaciones/{id:guid}/read", async (
    Guid id,
    ClaimsPrincipal user,
    IEmployerPostulacionService employerPostulacionService,
    CancellationToken cancellationToken) =>
{
    await employerPostulacionService.MarkNotificacionReadAsync(
        GetAuthenticatedUserId(user), id, cancellationToken);

    return Results.Ok(new { message = "Notificación marcada como leída." });
}).RequireAuthorization("EmployerOnly");

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

adminRoutes.MapGet("/report/data", async (
    IAdminService adminService,
    CancellationToken cancellationToken) =>
{
    AdminReportResponse report = await adminService.GetReportDataAsync(cancellationToken);
    return Results.Ok(report);
});

adminRoutes.MapPost("/employers/{id:guid}/activate", async (
    Guid id,
    IEmployerRegistrationService employerRegistrationService,
    CancellationToken cancellationToken) =>
{
    await employerRegistrationService.ActivateAsync(id, cancellationToken);
    return Results.Ok(new { message = "Empleador activado correctamente." });
});

adminRoutes.MapGet("/vacantes", async (
    IVacanteService vacanteService,
    CancellationToken cancellationToken) =>
{
    IReadOnlyCollection<VacanteResponse> vacantes =
        await vacanteService.GetAllVacantesAsync(cancellationToken);

    return Results.Ok(vacantes);
});

adminRoutes.MapPut("/vacantes/{id:guid}/status", async (
    Guid id,
    UpdateVacanteStatusRequest updateVacanteStatusRequest,
    IVacanteService vacanteService,
    CancellationToken cancellationToken) =>
{
    VacanteResponse vacante =
        await vacanteService.UpdateVacanteStatusAsAdminAsync(id, updateVacanteStatusRequest, cancellationToken);

    return Results.Ok(vacante);
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
}).RequireAuthorization("CandidateOnly");

RouteGroupBuilder postulacionRoutes = app.MapGroup("/api/postulaciones");

postulacionRoutes.MapPost("/", async (
    ClaimsPrincipal user,
    ApplyToVacanteRequest applyToVacanteRequest,
    IVacanteService vacanteService,
    CancellationToken cancellationToken) =>
{
    await vacanteService.ApplyAsync(GetAuthenticatedUserId(user), applyToVacanteRequest, cancellationToken);
    return Results.Created("/api/postulaciones", new { message = "Postulación enviada correctamente." });
}).RequireAuthorization("CandidateOnly");

candidateRoutes.MapGet("/me/postulaciones", async (
    ClaimsPrincipal user,
    IVacanteService vacanteService,
    CancellationToken cancellationToken) =>
{
    IReadOnlyCollection<PostulacionResponse> postulaciones =
        await vacanteService.GetMyPostulacionesAsync(GetAuthenticatedUserId(user), cancellationToken);

    return Results.Ok(postulaciones);
}).RequireAuthorization("CandidateOnly");

// -- Rutas de microcursos --

RouteGroupBuilder microCursoRoutes = app.MapGroup("/api/microcursos").RequireAuthorization();

microCursoRoutes.MapGet("/", async (
    string? area,
    IMicroCursoService microCursoService,
    CancellationToken cancellationToken) =>
{
    IReadOnlyCollection<MicroCursoResponse> microCursos =
        await microCursoService.GetCatalogoAsync(area, cancellationToken);

    return Results.Ok(microCursos);
});

microCursoRoutes.MapGet("/recomendados", async (
    ClaimsPrincipal user,
    IMicroCursoService microCursoService,
    CancellationToken cancellationToken) =>
{
    IReadOnlyCollection<MicroCursoResponse> microCursos =
        await microCursoService.GetRecomendadosAsync(GetAuthenticatedUserId(user), cancellationToken);

    return Results.Ok(microCursos);
});

microCursoRoutes.MapGet("/{id:guid}", async (
    Guid id,
    IMicroCursoService microCursoService,
    CancellationToken cancellationToken) =>
{
    MicroCursoResponse microCurso =
        await microCursoService.GetDetalleAsync(id, cancellationToken);

    return Results.Ok(microCurso);
});

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
