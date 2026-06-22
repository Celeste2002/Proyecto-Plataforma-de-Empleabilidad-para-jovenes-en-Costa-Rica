using domain.entities;

namespace services.interfaces;

public interface IContactoAccesoRepository
{
    Task SaveAsync(ContactoAcceso contactoAcceso, CancellationToken cancellationToken);
}
