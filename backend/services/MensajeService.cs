using domain.entities;
using services.dtos;
using services.exceptions;
using services.interfaces;

namespace services;

public sealed class MensajeService(
    IMensajeRepository mensajeRepository,
    IPostulacionRepository postulacionRepository,
    IVacanteRepository vacanteRepository,
    IEmployerRepository employerRepository,
    ICandidateRepository candidateRepository) : IMensajeService
{
    public async Task<MensajeResponse> SendMensajeAsync(
        Guid employerUserId,
        SendMensajeRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Body) || request.Body.Length > 2000)
        {
            throw new RequestValidationException(
                ["El mensaje es obligatorio y no puede superar los 2000 caracteres."]);
        }

        EmployerProfile? employer =
            await employerRepository.FindByUserIdAsync(employerUserId, cancellationToken);

        if (employer is null)
            throw new NotFoundException("No se encontró el perfil del empleador.");

        Postulacion? postulacion =
            await postulacionRepository.FindByIdAsync(request.PostulacionId, cancellationToken);

        if (postulacion is null)
            throw new NotFoundException("La postulación no existe.");

        Vacante? vacante =
            await vacanteRepository.FindByIdAsync(postulacion.VacanteId, cancellationToken);

        if (vacante is null || vacante.EmployerProfileId != employer.Id)
            throw new NotFoundException("No tiene permiso para enviar mensajes en esta postulación.");

        Mensaje mensaje = new()
        {
            Id = Guid.NewGuid(),
            PostulacionId = request.PostulacionId,
            SenderEmployerProfileId = employer.Id,
            RecipientCandidateProfileId = postulacion.CandidateProfileId,
            Body = request.Body.Trim(),
            SentAtUtc = DateTime.UtcNow,
            IsReadByCandidate = false
        };

        await mensajeRepository.SaveAsync(mensaje, cancellationToken);

        return new MensajeResponse(
            Id: mensaje.Id,
            PostulacionId: mensaje.PostulacionId,
            SenderEmployerProfileId: mensaje.SenderEmployerProfileId,
            SenderCompanyName: employer.CompanyName,
            JobTitle: vacante.JobTitle,
            Body: mensaje.Body,
            SentAtUtc: mensaje.SentAtUtc,
            IsReadByCandidate: false);
    }

    public async Task<IReadOnlyCollection<MensajeResponse>> GetMisBandejaEntradaAsync(
        Guid candidateUserId,
        CancellationToken cancellationToken)
    {
        CandidateProfile? candidateProfile =
            await candidateRepository.FindByUserIdAsync(candidateUserId, cancellationToken);

        if (candidateProfile is null)
            throw new NotFoundException("No se encontró el perfil del candidato.");

        IReadOnlyCollection<Mensaje> mensajes =
            await mensajeRepository.GetByRecipientCandidateProfileIdAsync(
                candidateProfile.Id, cancellationToken);

        return mensajes
            .Select(m => new MensajeResponse(
                Id: m.Id,
                PostulacionId: m.PostulacionId,
                SenderEmployerProfileId: m.SenderEmployerProfileId,
                SenderCompanyName: m.SenderCompanyName,
                JobTitle: m.JobTitle,
                Body: m.Body,
                SentAtUtc: m.SentAtUtc,
                IsReadByCandidate: m.IsReadByCandidate))
            .ToArray();
    }
}
