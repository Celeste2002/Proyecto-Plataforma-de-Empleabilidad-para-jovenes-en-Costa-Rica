using System.Globalization;
using System.Text;
using domain.entities;
using services.dtos;
using services.exceptions;
using services.interfaces;

namespace services;

public sealed class MicroCursoService(
    IMicroCursoRepository microCursoRepository,
    ICandidateRepository candidateRepository) : IMicroCursoService
{
    public async Task<IReadOnlyCollection<MicroCursoResponse>> GetCatalogoAsync(
        string? area,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<MicroCurso> microCursos =
            await microCursoRepository.GetValidatedAsync(NormalizeArea(area), cancellationToken);

        return microCursos
            .Select(curso => MapResponse(curso, []))
            .ToArray();
    }

    public async Task<MicroCursoResponse> GetDetalleAsync(Guid id, CancellationToken cancellationToken)
    {
        MicroCurso? microCurso = await microCursoRepository.FindValidatedByIdAsync(id, cancellationToken);

        if (microCurso is null)
        {
            throw new NotFoundException("El microcurso no existe o no esta disponible.");
        }

        return MapResponse(microCurso, []);
    }

    public async Task<IReadOnlyCollection<MicroCursoResponse>> GetRecomendadosAsync(
        Guid candidateUserId,
        CancellationToken cancellationToken)
    {
        CandidateProfile? candidate =
            await candidateRepository.FindByUserIdAsync(candidateUserId, cancellationToken);

        if (candidate is null)
        {
            throw new NotFoundException("No se encontro el perfil del candidato.");
        }

        IReadOnlyCollection<Habilidad> habilidades =
            await candidateRepository.GetHabilidadesAsync(candidate.Id, cancellationToken);

        string[] habilidadesNormalizadas = habilidades
            .Select(h => NormalizeText(h.Nombre))
            .Where(h => h.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (habilidadesNormalizadas.Length == 0)
        {
            return [];
        }

        IReadOnlyCollection<MicroCurso> microCursos =
            await microCursoRepository.GetValidatedAsync(null, cancellationToken);

        return microCursos
            .Select(curso => new
            {
                Curso = curso,
                Coincidentes = GetMatchingSkills(curso, habilidadesNormalizadas)
            })
            .Where(item => item.Coincidentes.Count > 0)
            .OrderByDescending(item => item.Coincidentes.Count)
            .ThenBy(item => item.Curso.Area)
            .ThenBy(item => item.Curso.Titulo)
            .Select(item => MapResponse(item.Curso, item.Coincidentes))
            .ToArray();
    }

    private static string? NormalizeArea(string? area) =>
        string.IsNullOrWhiteSpace(area) ? null : area.Trim();

    private static IReadOnlyCollection<string> GetMatchingSkills(
        MicroCurso curso,
        IReadOnlyCollection<string> candidateSkills)
    {
        return curso.Habilidades
            .Where(habilidad =>
            {
                string cursoSkill = NormalizeText(habilidad);
                return candidateSkills.Any(candidateSkill =>
                    cursoSkill.Contains(candidateSkill, StringComparison.OrdinalIgnoreCase) ||
                    candidateSkill.Contains(cursoSkill, StringComparison.OrdinalIgnoreCase));
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string NormalizeText(string value)
    {
        string normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        StringBuilder builder = new();

        foreach (char character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static MicroCursoResponse MapResponse(
        MicroCurso curso,
        IReadOnlyCollection<string> habilidadesCoincidentes) =>
        new(
            curso.Id,
            curso.Titulo,
            curso.Descripcion,
            curso.Area,
            curso.DuracionHoras,
            curso.EntidadProveedora,
            curso.TipoProveedor,
            curso.OtorgaCertificacion,
            curso.CantidadValidaciones,
            curso.Habilidades,
            habilidadesCoincidentes.Count,
            habilidadesCoincidentes);
}

