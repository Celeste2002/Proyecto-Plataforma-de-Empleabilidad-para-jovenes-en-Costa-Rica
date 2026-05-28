using domain.entities;

namespace services.interfaces;

public interface ITokenService
{
    string GenerateJwtToken(User user);
}
