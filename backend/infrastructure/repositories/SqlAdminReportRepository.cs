using Microsoft.Data.SqlClient;
using services.dtos;
using services.interfaces;

namespace infrastructure.repositories;

public sealed class SqlAdminReportRepository(string connectionString) : IAdminReportRepository
{
    public async Task<AdminReportResponse> GetReportDataAsync(CancellationToken cancellationToken)
    {
        await using SqlConnection connection =
            await SqlStoredProcedure.OpenConnectionAsync(connectionString, cancellationToken);

        await using SqlCommand command =
            connection.CreateStoredProcedureCommand(StoredProcedures.AdminReports.GetReportData);
        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        // Result 1: user counts
        await reader.ReadAsync(cancellationToken);
        int totalUsers = reader.GetInt32(reader.GetOrdinal("TotalUsers"));
        int totalCandidates = reader.GetInt32(reader.GetOrdinal("TotalCandidates"));
        int totalEmployers = reader.GetInt32(reader.GetOrdinal("TotalEmployers"));
        int totalAdministrators = reader.GetInt32(reader.GetOrdinal("TotalAdministrators"));

        // Result 2: vacante counts
        await reader.NextResultAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        int totalVacantes = reader.GetInt32(reader.GetOrdinal("TotalVacantes"));
        int activeVacantes = reader.GetInt32(reader.GetOrdinal("ActiveVacantes"));
        int closedVacantes = reader.GetInt32(reader.GetOrdinal("ClosedVacantes"));

        // Result 3: postulacion aggregates
        await reader.NextResultAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        int totalPostulaciones = reader.GetInt32(reader.GetOrdinal("TotalPostulaciones"));
        int candidatesWithPostulaciones = reader.GetInt32(reader.GetOrdinal("CandidatesWithPostulaciones"));
        int vacantesWithPostulaciones = reader.GetInt32(reader.GetOrdinal("VacantesWithPostulaciones"));

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
            TotalUsers: totalUsers,
            TotalCandidates: totalCandidates,
            TotalEmployers: totalEmployers,
            TotalAdministrators: totalAdministrators,
            TotalVacantes: totalVacantes,
            ActiveVacantes: activeVacantes,
            ClosedVacantes: closedVacantes,
            TotalPostulaciones: totalPostulaciones,
            CandidatesWithPostulaciones: candidatesWithPostulaciones,
            VacantesWithPostulaciones: vacantesWithPostulaciones,
            PostulacionesByStatus: byStatus,
            CandidatesByProvince: candidatesByProvince,
            VacantesByProvince: vacantesByProvince,
            TotalMicrocursos: totalMicrocursos,
            GeneratedAt: DateTime.UtcNow);
    }
}
