# Plataforma de Empleabilidad para Jovenes en Costa Rica

Implementacion inicial de la HU1 del Sprint 1:

> Como joven de 18 a 30 anos quiero registrarme en la plataforma con mis datos personales, nivel educativo y zona geografica para crear un perfil que me permita postularme a empleos formales.

## Estructura

- `backend/api`: endpoints HTTP, CORS y middleware.
- `backend/domain`: entidades y catalogos del negocio.
- `backend/services`: casos de uso, contratos, DTOs y validaciones.
- `backend/infrastructure`: repositorio SQL Server y envio de correo SMTP.
- `frontend`: aplicacion React modular para registro y vista de empleadores aliados.

## Correr backend

```powershell
dotnet run --project backend\api\api.csproj --urls http://localhost:5000
```

Endpoints principales:

- `POST /api/candidates/register`
- `GET /api/employers/candidates`
- `GET /api/health`

El backend lee variables desde el `.env` mas cercano al proyecto. Puedes dejarlo en la raiz del repositorio o en `backend/api`. Usa `.env.example` como guia.

Para correo real, cambia en `.env`:

```env
Email__Provider=Smtp
Email__SenderAddress=tu-correo@gmail.com
Email__SmtpUsername=tu-correo@gmail.com
Email__SmtpPassword=TU_APP_PASSWORD
```

La aplicacion no guarda correos localmente. SMTP debe estar configurado para completar el registro.

La conexion local a SQL Server queda en:

```env
ConnectionStrings__DefaultConnection=Server=localhost;Database=EmployabilityPlatform;Trusted_Connection=True;TrustServerCertificate=True;
```

El script inicial de SQL Server esta en `database/scripts/001_initial_candidate_registration.sql`.

La HU1 persiste en SQL Server usando `dbo.CandidateProfiles`. Si `ConnectionStrings__DefaultConnection` no existe, el backend falla de forma explicita porque no se permite guardar datos localmente.

## Correr frontend

```powershell
cd frontend
npm install
npm run dev
```

La app espera la API en `http://localhost:5000`. Si cambia, definir:

```powershell
$env:VITE_API_BASE_URL="http://localhost:5000"
```

## Idea recomendada para base de datos en la nube

Para evitar pelear con scripts locales, usaria **Supabase Postgres** como base cloud durante el curso:

- Tiene plan gratuito y consola web para ver datos.
- Es PostgreSQL real, entonces despues se puede usar con Entity Framework Core y migraciones.
- Permite compartir una sola base entre integrantes sin instalar SQL Server local.
- Para DevOps, el backend solo necesitaria una variable `ConnectionStrings__DefaultConnection`.

Camino sugerido:

1. Mantener la interfaz `ICandidateRepository` como contrato.
2. Reemplazar `SqlCandidateRepository` por un `PostgresCandidateRepository` o por EF Core cuando definan la DB final.
3. Ejecutar migraciones desde CI/CD, no desde maquinas personales.
4. Usar ambientes separados: `Development`, `Staging` y `Production`.

Si el equipo prefiere Microsoft, la alternativa natural es **Azure SQL Database**, pero para un proyecto academico Supabase suele ser mas rapido y menos pesado de administrar.
