# Plataforma de Empleabilidad para Jovenes en Costa Rica

## Estructura

- `backend/api`: endpoints HTTP, CORS y middleware.
- `backend/domain`: entidades y catalogos del negocio.
- `backend/services`: casos de uso, contratos, DTOs y validaciones.
- `backend/infrastructure`: repositorio SQL Server y envio de correo SMTP.
- `frontend/shared`: contexto de autenticacion, componentes y estilos compartidos entre las 3 apps.
- `frontend/frontend-Candidato`: app React para jovenes candidatos (puerto 5170).
- `frontend/frontend-Empleador`: app React para empleadores aliados (puerto 5171).
- `frontend/frontend-Admin`: app React para administradores de la plataforma (puerto 5172).

## Requisitos

- Bun.
- .NET SDK 9.
- SQL Server local con la base de datos creada desde `database/Plataforma_Empleabilidad_BD.sql`.

## Instalacion inicial

Desde la raiz del repositorio:

```bash
bun install
bun run setup
```

`bun install` instala las herramientas del orquestador de la raiz. `bun run setup` instala las dependencias de las 3 apps frontend y restaura el backend .NET.

## Comandos unificados

Desde la raiz del repositorio:

```bash
# Solo backend
bun run dev:backend

# Solo los 3 frontends
bun run dev:frontends

# Backend + los 3 frontends
bun run dev:all
```

URLs locales:

- Backend API: `http://localhost:5000`
- Candidatos: `http://127.0.0.1:5170`
- Empleadores: `http://127.0.0.1:5171`
- Administracion: `http://127.0.0.1:5172`

Tambien se pueden correr apps individuales:

```bash
bun run dev:candidato
bun run dev:empleador
bun run dev:admin
```

## Correr backend manualmente

```comand prompt
\GitHub\Proyecto-Plataforma-de-Empleabilidad-para-jovenes-en-Costa-Rica\backend\api>dotnet run
```

Endpoints principales:

- `POST /api/candidates/register`
- `GET /api/employers/candidates`
- `GET /api/health`

El backend lee variables desde el `.env` más cercano al proyecto.

Para correo real, crear el `.env` con la configuracion SMTP:

```env
Smtp__Host=smtp-relay.sendinblue.com
Smtp__Port=587
Smtp__Username=TU_USUARIO_SMTP
Smtp__Password=TU_SMTP_KEY
Smtp__FromName=Sinergia
Smtp__FromAddress=tu-correo-verificado@example.com
```

La aplicacion no guarda correos localmente. SMTP debe estar configurado para completar el registro.

La conexion local a SQL Server queda en:

```env
ConnectionStrings__DefaultConnection=Server=.;Database=Plataforma_Empleabilidad_BD;Integrated Security=True;TrustServerCertificate=True;Encrypt=False
```

El script unificado para crear o completar la base de datos SQL Server esta en
`database/Plataforma_Empleabilidad_BD.sql`. Ese archivo es la unica fuente de
verdad del esquema y contiene todas las tablas, columnas, vistas, indices y datos
iniciales necesarios.


## Correr frontend manualmente

El frontend esta dividido en 3 aplicaciones independientes segun el rol. Cada una debe levantarse en su propia terminal.

**Terminal 1 — Candidatos** → `http://127.0.0.1:5170`
```
cd frontend\frontend-Candidato
bun install
bun run dev
```

**Terminal 2 — Empleadores** → `http://127.0.0.1:5171`
```
cd frontend\frontend-Empleador
bun install
bun run dev
```

**Terminal 3 — Administracion** → `http://127.0.0.1:5172`
```
cd frontend\frontend-Admin
bun install
bun run dev
```

Cada app espera la API en `http://localhost:5000`. Si cambia, editar el archivo `.env` dentro de la carpeta correspondiente:

```env
VITE_API_BASE_URL=http://localhost:5000
```

