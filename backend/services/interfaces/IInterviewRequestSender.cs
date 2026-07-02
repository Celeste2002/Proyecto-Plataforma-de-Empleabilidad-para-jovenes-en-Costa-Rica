using domain.entities;

namespace services.interfaces;

public interface IInterviewRequestSender
{
    Task SendInterviewRequestAsync(
        EmployerProfile employerProfile,
        Postulacion postulacion,
        CancellationToken cancellationToken);

    Task SendPostulacionDeclinedAsync(
        EmployerProfile employerProfile,
        Postulacion postulacion,
        CancellationToken cancellationToken);

    Task SendVacanteFilledAsync(
        EmployerProfile employerProfile,
        Postulacion postulacion,
        CancellationToken cancellationToken);
}
