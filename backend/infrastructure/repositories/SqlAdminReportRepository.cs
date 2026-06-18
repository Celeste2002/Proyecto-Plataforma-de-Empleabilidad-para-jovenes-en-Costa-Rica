using Microsoft.Data.SqlClient;
using services.dtos;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlAdminReportRepository(string connectionString) : IAdminReportRepository
{
    public async Task<AdminReportResponse> GetReportDataAsync(CancellationToken cancellationToken)
    {
        // All 6 aggregate queries in a single round-trip using multiple result sets.
        const string sql = """
            SELECT
                COUNT(*)                                                              AS TotalUsers,
                SUM(CASE WHEN Role = 'CANDIDATE'      THEN 1 ELSE 0 END)             AS TotalCandidates,
                SUM(CASE WHEN Role = 'EMPLOYER'       THEN 1 ELSE 0 END)             AS TotalEmployers,
                SUM(CASE WHEN Role = 'ADMINISTRATOR'  THEN 1 ELSE 0 END)             AS TotalAdministrators
            FROM dbo.Users;

            SELECT
                COUNT(*)                                                              AS TotalVacantes,
                SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END)                        AS ActiveVacantes,
                SUM(CASE WHEN IsActive = 0 THEN 1 ELSE 0 END)                        AS ClosedVacantes
            FROM dbo.Vacantes;

            SELECT
                COUNT(*)                                                              AS TotalPostulaciones,
                COUNT(DISTINCT CandidateProfileId)                                    AS CandidatesWithPostulaciones,
                COUNT(DISTINCT VacanteId)                                             AS VacantesWithPostulaciones
            FROM dbo.Postulaciones;

            SELECT Status, COUNT(*) AS Count
            FROM dbo.Postulaciones
            GROUP BY Status
            ORDER BY COUNT(*) DESC;

            SELECT Province, COUNT(*) AS Count
            FROM dbo.CandidateProfiles
            GROUP BY Province
            ORDER BY COUNT(*) DESC;

            SELECT Province, COUNT(*) AS Count
            FROM dbo.Vacantes
            GROUP BY Province
            ORDER BY COUNT(*) DESC;

            SELECT COUNT(*) AS TotalMicrocursos
            FROM (
                SELECT mc.Id
                FROM dbo.MicroCursos mc
                LEFT JOIN dbo.MicroCursoValidacionesEmpleador mcv
                    ON mc.Id = mcv.MicroCursoId
                LEFT JOIN dbo.EmployerProfiles ep
                    ON mcv.EmployerProfileId = ep.Id
                    AND ep.Status = N'Active'
                WHERE mc.IsActive = 1
                GROUP BY mc.Id
                HAVING COUNT(DISTINCT ep.Id) >= 3
            ) validatedMicroCursos;
            """;

        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using SqlCommand command = new(sql, connection);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        // Result 1: user counts
        await reader.ReadAsync(cancellationToken);
        int totalUsers          = reader.GetInt32(reader.GetOrdinal("TotalUsers"));
        int totalCandidates     = reader.GetInt32(reader.GetOrdinal("TotalCandidates"));
        int totalEmployers      = reader.GetInt32(reader.GetOrdinal("TotalEmployers"));
        int totalAdministrators = reader.GetInt32(reader.GetOrdinal("TotalAdministrators"));

        // Result 2: vacante counts
        await reader.NextResultAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        int totalVacantes  = reader.GetInt32(reader.GetOrdinal("TotalVacantes"));
        int activeVacantes = reader.GetInt32(reader.GetOrdinal("ActiveVacantes"));
        int closedVacantes = reader.GetInt32(reader.GetOrdinal("ClosedVacantes"));

        // Result 3: postulacion aggregates
        await reader.NextResultAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        int totalPostulaciones          = reader.GetInt32(reader.GetOrdinal("TotalPostulaciones"));
        int candidatesWithPostulaciones = reader.GetInt32(reader.GetOrdinal("CandidatesWithPostulaciones"));
        int vacantesWithPostulaciones   = reader.GetInt32(reader.GetOrdinal("VacantesWithPostulaciones"));

        // Result 4: postulaciones by status
        await reader.NextResultAsync(cancellationToken);
        List<PostulacionStatusStat> byStatus = [];
        while (await reader.ReadAsync(cancellationToken))
        {
            byStatus.Add(new PostulacionStatusStat(
                reader.GetString(reader.GetOrdinal("Status")),
                reader.GetInt32(reader.GetOrdinal("Count"))));
        }

        // Result 5: candidates by province
        await reader.NextResultAsync(cancellationToken);
        List<ProvinceStat> candidatesByProvince = [];
        while (await reader.ReadAsync(cancellationToken))
        {
            candidatesByProvince.Add(new ProvinceStat(
                reader.GetString(reader.GetOrdinal("Province")),
                reader.GetInt32(reader.GetOrdinal("Count"))));
        }

        // Result 6: vacantes by province
        await reader.NextResultAsync(cancellationToken);
        List<ProvinceStat> vacantesByProvince = [];
        while (await reader.ReadAsync(cancellationToken))
        {
            vacantesByProvince.Add(new ProvinceStat(
                reader.GetString(reader.GetOrdinal("Province")),
                reader.GetInt32(reader.GetOrdinal("Count"))));
        }

        // Result 7: validated micro-course count
        await reader.NextResultAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        int totalMicrocursos = reader.GetInt32(reader.GetOrdinal("TotalMicrocursos"));

        return new AdminReportResponse(
            TotalUsers:                  totalUsers,
            TotalCandidates:             totalCandidates,
            TotalEmployers:              totalEmployers,
            TotalAdministrators:         totalAdministrators,
            TotalVacantes:               totalVacantes,
            ActiveVacantes:              activeVacantes,
            ClosedVacantes:              closedVacantes,
            TotalPostulaciones:          totalPostulaciones,
            CandidatesWithPostulaciones: candidatesWithPostulaciones,
            VacantesWithPostulaciones:   vacantesWithPostulaciones,
            PostulacionesByStatus:       byStatus,
            CandidatesByProvince:        candidatesByProvince,
            VacantesByProvince:          vacantesByProvince,
            TotalMicrocursos:            totalMicrocursos,
            GeneratedAt:                 DateTime.UtcNow);
    }
}
