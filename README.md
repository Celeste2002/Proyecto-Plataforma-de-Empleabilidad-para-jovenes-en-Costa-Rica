# Plataforma de Empleabilidad para Jovenes en Costa Rica

## Estructura

- `backend/api`: endpoints HTTP, CORS y middleware.
- `backend/domain`: entidades y catalogos del negocio.
- `backend/services`: casos de uso, contratos, DTOs y validaciones.
- `backend/infrastructure`: repositorio SQL Server y envio de correo SMTP.
- `frontend/shared`: contexto de autenticacion, componentes y estilos compartidos entre las 3 apps.
- `frontend/frontend-Candidato`: app React para jovenes candidatos (puerto 5173).
- `frontend/frontend-Empleador`: app React para empleadores aliados (puerto 5174).
- `frontend/frontend-Admin`: app React para administradores de la plataforma (puerto 5175).

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

El frontend esta dividido en 3 aplicaciones independientes segun el rol. Cada una debe levantarse en su propia terminal.

**Terminal 1 — Candidatos** → `http://127.0.0.1:5173`
```
cd frontend\frontend-Candidato
npm install
npm run dev
```

**Terminal 2 — Empleadores** → `http://127.0.0.1:5174`
```
cd frontend\frontend-Empleador
npm install
npm run dev
```

**Terminal 3 — Administracion** → `http://127.0.0.1:5175`
```
cd frontend\frontend-Admin
npm install
npm run dev
```

Cada app espera la API en `http://localhost:5000`. Si cambia, editar el archivo `.env` dentro de la carpeta correspondiente:

```env
VITE_API_BASE_URL=http://localhost:5000
```

