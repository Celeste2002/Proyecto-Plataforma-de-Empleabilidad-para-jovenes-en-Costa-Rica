using domain.entities;

namespace services.interfaces;

public interface IInterviewRequestSender
{
    Task SendInterviewRequestAsync(
        EmployerProfile employerProfile,
        Postulacion postulacion,
        CancellationToken cancellationToken);
}
