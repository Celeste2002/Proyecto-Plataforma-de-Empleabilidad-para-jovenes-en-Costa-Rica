using System.Security.Cryptography;
using domain.constants;
using domain.entities;
using services.dtos;
using services.exceptions;
using services.interfaces;

namespace services;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IPasswordResetSender passwordResetSender) : IAuthService
{
    private const int PasswordResetTokenByteLength = 32;
    private static readonly TimeSpan PasswordResetTokenLifetime = TimeSpan.FromHours(1);

    public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(loginRequest.Email) || string.IsNullOrWhiteSpace(loginRequest.Password))
        {
            throw new AuthenticationException("Credenciales invalidas.");
        }

        string normalizedEmail = loginRequest.Email.Trim().ToLowerInvariant();
        User? user = await userRepository.FindByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive)
        {
            throw new AuthenticationException("Credenciales invalidas.");
        }

        if (user.PasswordHash is null)
        {
            throw new AuthenticationException(
                "Esta cuenta no tiene una contrasena configurada. " +
                "Usa 'Olvide mi contrasena' para establecerla.");
        }

        bool passwordValid = passwordHasher.Verify(loginRequest.Password, user.PasswordHash);

        if (!passwordValid)
        {
            throw new AuthenticationException("Credenciales invalidas.");
        }

        string token = tokenService.GenerateJwtToken(user);

        return new LoginResponse(token, user.Role, user.Id, user.Email);
    }

    public async Task RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return;
        }

        string normalizedEmail = request.Email.Trim().ToLowerInvariant();
        User? user = await userRepository.FindByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive)
        {
            // No revelar si el correo existe o no
            return;
        }

        string resetToken = GeneratePasswordResetToken();
        DateTime expiresAt = DateTime.UtcNow.Add(PasswordResetTokenLifetime);

        await userRepository.SavePasswordResetTokenAsync(user.Id, resetToken, expiresAt, cancellationToken);
        await passwordResetSender.SendPasswordResetAsync(user.Email, resetToken, cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        List<string> validationErrors = [];

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            validationErrors.Add("El token de recuperacion es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        {
            validationErrors.Add("La contrasena debe tener al menos 8 caracteres.");
        }

        if (validationErrors.Count > 0)
        {
            throw new RequestValidationException(validationErrors);
        }

        User? user = await userRepository.FindByPasswordResetTokenAsync(request.Token.Trim(), cancellationToken);

        if (user is null ||
            user.PasswordResetTokenExpiresAtUtc is null ||
            user.PasswordResetTokenExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new RequestValidationException(["El enlace de recuperacion es invalido o ha expirado."]);
        }

        string passwordHash = passwordHasher.Hash(request.NewPassword);

        await userRepository.UpdatePasswordAsync(user.Id, passwordHash, cancellationToken);
        await userRepository.ClearPasswordResetTokenAsync(user.Id, cancellationToken);
    }

    private static string GeneratePasswordResetToken()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(PasswordResetTokenByteLength);
        return Convert.ToHexString(randomBytes).ToLowerInvariant();
    }
}
