namespace services.interfaces;

public interface IPasswordResetSender
{
    Task SendPasswordResetAsync(string email, string resetToken, CancellationToken cancellationToken);
}
