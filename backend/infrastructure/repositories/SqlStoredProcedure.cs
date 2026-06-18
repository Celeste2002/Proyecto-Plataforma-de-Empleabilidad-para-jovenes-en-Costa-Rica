using System.Data;
using Microsoft.Data.SqlClient;

namespace infrastructure.repositories;

internal static class SqlStoredProcedure
{
    public static async Task<SqlConnection> OpenConnectionAsync(
        string connectionString,
        CancellationToken cancellationToken)
    {
        SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    public static SqlCommand CreateStoredProcedureCommand(
        this SqlConnection connection,
        string procedureName)
    {
        SqlCommand command = connection.CreateCommand();
        command.CommandText = procedureName;
        command.CommandType = CommandType.StoredProcedure;
        return command;
    }

    public static SqlParameter AddNullableWithValue(
        this SqlParameterCollection parameters,
        string parameterName,
        object? value)
    {
        return parameters.AddWithValue(parameterName, value ?? DBNull.Value);
    }
}

internal static class StoredProcedures
{
    internal static class Users
    {
        public const string FindByEmail = "dbo.usp_User_FindByEmail";
        public const string FindById = "dbo.usp_User_FindById";
        public const string FindByPasswordResetToken = "dbo.usp_User_FindByPasswordResetToken";
        public const string GetAll = "dbo.usp_User_GetAll";
        public const string Save = "dbo.usp_User_Save";
        public const string UpdatePassword = "dbo.usp_User_UpdatePassword";
        public const string UpdateRole = "dbo.usp_User_UpdateRole";
        public const string SavePasswordResetToken = "dbo.usp_User_SavePasswordResetToken";
        public const string ClearPasswordResetToken = "dbo.usp_User_ClearPasswordResetToken";
        public const string SetActive = "dbo.usp_User_SetActive";
    }

    internal static class Candidates
    {
        public const string FindByEmail = "dbo.usp_Candidate_FindByEmail";
        public const string FindByUserId = "dbo.usp_Candidate_FindByUserId";
        public const string GetVisibleToPartnerEmployers = "dbo.usp_Candidate_GetVisibleToPartnerEmployers";
        public const string Save = "dbo.usp_Candidate_Save";
        public const string Update = "dbo.usp_Candidate_Update";
        public const string UpdateAvailability = "dbo.usp_Candidate_UpdateAvailability";
        public const string MarkEmailConfirmationSent = "dbo.usp_Candidate_MarkEmailConfirmationSent";
        public const string GetExperiencias = "dbo.usp_Candidate_GetExperiencias";
        public const string SaveExperiencia = "dbo.usp_Candidate_SaveExperiencia";
        public const string DeleteExperiencia = "dbo.usp_Candidate_DeleteExperiencia";
        public const string GetHabilidades = "dbo.usp_Candidate_GetHabilidades";
        public const string SaveHabilidad = "dbo.usp_Candidate_SaveHabilidad";
        public const string DeleteHabilidad = "dbo.usp_Candidate_DeleteHabilidad";
        public const string GetCursos = "dbo.usp_Candidate_GetCursos";
        public const string SaveCurso = "dbo.usp_Candidate_SaveCurso";
        public const string DeleteCurso = "dbo.usp_Candidate_DeleteCurso";
    }

    internal static class Employers
    {
        public const string FindByEmail = "dbo.usp_Employer_FindByEmail";
        public const string FindByUserId = "dbo.usp_Employer_FindByUserId";
        public const string FindById = "dbo.usp_Employer_FindById";
        public const string Save = "dbo.usp_Employer_Save";
        public const string UpdateStatus = "dbo.usp_Employer_UpdateStatus";
        public const string MarkActivationEmailSent = "dbo.usp_Employer_MarkActivationEmailSent";
    }

    internal static class Vacantes
    {
        public const string GetActive = "dbo.usp_Vacante_GetActive";
        public const string GetByEmployerProfileId = "dbo.usp_Vacante_GetByEmployerProfileId";
        public const string FindById = "dbo.usp_Vacante_FindById";
        public const string Save = "dbo.usp_Vacante_Save";
        public const string UpdateEditableFields = "dbo.usp_Vacante_UpdateEditableFields";
    }

    internal static class Postulaciones
    {
        public const string Save = "dbo.usp_Postulacion_Save";
        public const string ExistsByVacanteAndCandidate = "dbo.usp_Postulacion_ExistsByVacanteAndCandidate";
        public const string GetByCandidateProfileId = "dbo.usp_Postulacion_GetByCandidateProfileId";
        public const string GetByVacanteForEmployer = "dbo.usp_Postulacion_GetByVacanteForEmployer";
        public const string FindByIdForEmployer = "dbo.usp_Postulacion_FindByIdForEmployer";
        public const string UpdateStatusForEmployer = "dbo.usp_Postulacion_UpdateStatusForEmployer";
    }

    internal static class Notificaciones
    {
        public const string Save = "dbo.usp_Notificacion_Save";
        public const string GetByEmployerProfileId = "dbo.usp_Notificacion_GetByEmployerProfileId";
        public const string MarkAsRead = "dbo.usp_Notificacion_MarkAsRead";
        public const string GetUnreadCount = "dbo.usp_Notificacion_GetUnreadCount";
    }

    internal static class AdminReports
    {
        public const string GetReportData = "dbo.usp_AdminReport_GetReportData";
    }
}
