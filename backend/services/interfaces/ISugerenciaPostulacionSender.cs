using domain.entities;

namespace services.interfaces;

public interface ISugerenciaPostulacionSender
{
    Task SendSugerenciaAsync(
        EmployerProfile employerProfile,
        CandidateProfile candidateProfile,
        Vacante vacante,
        string? message,
        CancellationToken cancellationToken);
}
