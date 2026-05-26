# Plataforma de Empleabilidad para Jovenes en Costa Rica

## Estructura

- `backend/api`: endpoints HTTP, CORS y middleware.
- `backend/domain`: entidades y catalogos del negocio.
- `backend/services`: casos de uso, contratos, DTOs y validaciones.
- `backend/infrastructure`: repositorio SQL Server y envio de correo SMTP.
- `frontend`: aplicacion React modular para registro y vista de empleadores aliados.

## Correr backend

```comand prompt
\GitHub\Proyecto-Plataforma-de-Empleabilidad-para-jovenes-en-Costa-Rica\backend\api>dotnet run
```

Endpoints principales:

- `POST /api/candidates/register`
- `GET /api/employers/candidates`
- `GET /api/health`

El backend lee variables desde el `.env` más cercano al proyecto.

Para correo real, crear el `.env` y copiar de whatsapp:

```env
Email__Provider=Smtp
Email__SenderAddress=tu-correo@gmail.com
Email__SmtpUsername=tu-correo@gmail.com
Email__SmtpPassword=TU_APP_PASSWORD
```

La aplicacion no guarda correos localmente. SMTP debe estar configurado para completar el registro.

La conexion local a SQL Server queda en:

```env
ConnectionStrings__DefaultConnection=Server=.;Database=Plataforma_Empleabilidad_BD;Integrated Security=True;TrustServerCertificate=True;Encrypt=False
```

El script inicial de SQL Server esta en `database/scripts/001_initial_candidate_registration.sql`.


## Correr frontend

```comand prompt
\GitHub\Proyecto-Plataforma-de-Empleabilidad-para-jovenes-en-Costa-Rica\frontend>npm run dev
```

La app espera la API en `http://localhost:5000`. Si cambia, definir:

```powershell
$env:VITE_API_BASE_URL="http://localhost:5000"
```

