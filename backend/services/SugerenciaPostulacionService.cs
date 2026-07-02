using domain.entities;
using Microsoft.Extensions.Logging;
using services.dtos;
using services.exceptions;
using services.interfaces;

namespace services;

public sealed class SugerenciaPostulacionService(
    ISugerenciaPostulacionRepository sugerenciaRepository,
    IVacanteRepository vacanteRepository,
    ICandidateRepository candidateRepository,
    IEmployerRepository employerRepository,
    IPostulacionRepository postulacionRepository,
    ISugerenciaPostulacionSender sugerenciaSender,
    ILogger<SugerenciaPostulacionService> logger) : ISugerenciaPostulacionService
{
    public async Task<SugerenciaPostulacionResponse> CreateAsync(
        Guid employerUserId,
        CreateSugerenciaPostulacionRequest request,
        CancellationToken cancellationToken)
    {
        string? message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim();

        if (message is { Length: > 500 })
        {
            throw new RequestValidationException(["El mensaje no puede superar los 500 caracteres."]);
        }

        EmployerProfile? employer = await employerRepository.FindByUserIdAsync(employerUserId, cancellationToken);

        if (employer is null)
        {
            throw new NotFoundException("No se encontro el perfil del empleador.");
        }

        Vacante? vacante = await vacanteRepository.FindByIdAsync(request.VacanteId, cancellationToken);

        if (vacante is null || vacante.EmployerProfileId != employer.Id)
        {
            throw new NotFoundException("La vacante no existe o no pertenece a este empleador.");
        }

        if (!vacante.IsActive)
        {
            throw new RequestValidationException(["Solo se pueden enviar sugerencias para vacantes activas."]);
        }

        CandidateProfile? candidate =
            await candidateRepository.FindVisibleByIdAsync(request.CandidateProfileId, cancellationToken);

        if (candidate is null)
        {
            throw new NotFoundException("El candidato no existe o no esta disponible.");
        }

        bool alreadySuggested = await sugerenciaRepository.ExistsByVacanteAndCandidateAsync(
            request.VacanteId, request.CandidateProfileId, cancellationToken);

        if (alreadySuggested)
        {
            throw new RequestValidationException(["Ya se envio una sugerencia de esta vacante a este candidato."]);
        }

        SugerenciaPostulacion sugerencia = new()
        {
            Id = Guid.NewGuid(),
            VacanteId = vacante.Id,
            CandidateProfileId = candidate.Id,
            Message = message,
            CreatedAtUtc = DateTime.UtcNow
        };

        // Se guarda primero: el constraint UNIQUE (VacanteId, CandidateProfileId) actua como
        // guardia contra condiciones de carrera (dos envios casi simultaneos para el mismo par),
        // y evita que el candidato reciba un correo de una sugerencia que no llego a persistirse.
        await sugerenciaRepository.SaveAsync(sugerencia, cancellationToken);

        try
        {
            await sugerenciaSender.SendSugerenciaAsync(employer, candidate, vacante, message, cancellationToken);
        }
        catch (EmailDeliveryException emailDeliveryException)
        {
            logger.LogWarning(
                emailDeliveryException,
                "La sugerencia {SugerenciaId} se guardo correctamente pero el correo no pudo enviarse.",
                sugerencia.Id);
        }

        return new SugerenciaPostulacionResponse(
            sugerencia.Id,
            vacante.Id,
            vacante.JobTitle,
            employer.CompanyName,
            candidate.Id,
            sugerencia.Message,
            sugerencia.CreatedAtUtc);
    }

    public async Task<IReadOnlyCollection<SugerenciaRecibidaResponse>> GetRecibidasByCandidateAsync(
        Guid candidateUserId,
        CancellationToken cancellationToken)
    {
        CandidateProfile? candidate = await candidateRepository.FindByUserIdAsync(candidateUserId, cancellationToken);

        if (candidate is null)
        {
            throw new NotFoundException("No se encontro el perfil del candidato.");
        }

        IReadOnlyCollection<SugerenciaPostulacion> sugerencias =
            await sugerenciaRepository.GetByCandidateProfileIdAsync(candidate.Id, cancellationToken);

        return sugerencias
            .Select(s => new SugerenciaRecibidaResponse(
                s.Id,
                s.VacanteId,
                s.JobTitle,
                s.CompanyName,
                s.Province,
                s.Message,
                s.CreatedAtUtc,
                s.VacanteIsActive,
                s.AlreadyApplied))
            .ToArray();
    }

    public async Task<IReadOnlyCollection<Guid>> GetAppliedVacanteIdsAsync(
        Guid employerUserId,
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        EmployerProfile? employer = await employerRepository.FindByUserIdAsync(employerUserId, cancellationToken);

        if (employer is null)
        {
            throw new NotFoundException("No se encontro el perfil del empleador.");
        }

        return await postulacionRepository.GetAppliedVacanteIdsForEmployerAsync(
            candidateProfileId, employer.Id, cancellationToken);
    }
}
