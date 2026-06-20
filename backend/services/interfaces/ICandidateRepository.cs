using domain.entities;

namespace services.interfaces;

public interface ICandidateRepository
{
    Task<CandidateProfile?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<CandidateProfile?> FindByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CandidateProfile>> GetVisibleToPartnerEmployersAsync(CancellationToken cancellationToken);

    Task SaveAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken);

    Task UpdateAsync(CandidateProfile candidateProfile, CancellationToken cancellationToken);

    Task UpdateAvailabilityAsync(Guid profileId, bool isAvailableForContact, CancellationToken cancellationToken);

    Task MarkEmailConfirmationSentAsync(Guid candidateProfileId, CancellationToken cancellationToken);

    // Experiencias laborales
    Task<IReadOnlyCollection<ExperienciaLaboral>> GetExperienciasAsync(Guid candidateProfileId, CancellationToken cancellationToken);

    Task SaveExperienciaAsync(ExperienciaLaboral experiencia, CancellationToken cancellationToken);

    Task DeleteExperienciaAsync(Guid id, Guid candidateProfileId, CancellationToken cancellationToken);

    // Habilidades
    Task<IReadOnlyCollection<Habilidad>> GetHabilidadesAsync(Guid candidateProfileId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<string>> GetHabilidadesBlandasSugeridasAsync(CancellationToken cancellationToken);

    Task SaveHabilidadAsync(Habilidad habilidad, CancellationToken cancellationToken);

    Task DeleteHabilidadAsync(Guid id, Guid candidateProfileId, CancellationToken cancellationToken);

    // Cursos completados
    Task<IReadOnlyCollection<CursoCompletado>> GetCursosAsync(Guid candidateProfileId, CancellationToken cancellationToken);

    Task SaveCursoAsync(CursoCompletado curso, CancellationToken cancellationToken);

    Task DeleteCursoAsync(Guid id, Guid candidateProfileId, CancellationToken cancellationToken);
}
