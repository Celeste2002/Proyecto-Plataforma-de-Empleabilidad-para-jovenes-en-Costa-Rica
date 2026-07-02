# Plataforma de Empleabilidad para Jovenes en Costa Rica

## Estructura

- `backend/api`: endpoints HTTP, CORS, autenticacion y middleware.
- `backend/domain`: entidades y catalogos del negocio.
- `backend/services`: casos de uso, contratos, DTOs y validaciones.
- `backend/infrastructure`: repositorios SQL Server, JWT, hash de contrasenas y correo SMTP.
- `frontend/shared`: contexto de autenticacion, componentes, estilos y constantes compartidas.
- `frontend/frontend-Candidato`: app React para jovenes candidatos, puerto `5170`.
- `frontend/frontend-Empleador`: app React para empleadores aliados, puerto `5171`.
- `frontend/frontend-Admin`: app React para administradores, puerto `5172`.
- `database`: script principal de base de datos y scripts incrementales de apoyo.

## Requisitos

- Node.js con npm.
- .NET SDK 9.
- SQL Server local.

## Instalacion inicial

Desde la raiz del repositorio:

```bash
npm install
npm run setup
```

`npm install` instala las herramientas del orquestador raiz. `npm run setup` instala dependencias de las tres apps frontend y restaura el backend .NET.

## Variables de entorno

Crear un archivo `.env` en la raiz del repositorio con la conexion a SQL Server y la configuracion SMTP:

```env
ConnectionStrings__DefaultConnection=Server=.;Database=Plataforma_Empleabilidad_BD;Integrated Security=True;TrustServerCertificate=True;Encrypt=False
Jwt__Key=CAMBIA_ESTA_CLAVE_LARGA_Y_SEGURA
Jwt__Issuer=Sinergia
Jwt__Audience=Sinergia
Smtp__Host=smtp-relay.sendinblue.com
Smtp__Port=587
Smtp__Username=TU_USUARIO_SMTP
Smtp__Password=TU_SMTP_KEY
Smtp__FromName=Sinergia
Smtp__FromAddress=tu-correo-verificado@example.com
```

Cada frontend espera la API en `http://localhost:5000`. Si cambia, crear un `.env` dentro de la app correspondiente:

```env
VITE_API_BASE_URL=http://localhost:5000
```

## Base de datos

El script principal esta en:

```text
database/Plataforma_Empleabilidad_BD.sql
```

Ese archivo es la fuente de verdad del esquema y contiene tablas, columnas, vistas, indices y datos iniciales. Los scripts en `database/scripts` son apoyos incrementales o semillas especificas.

## Comandos

Desde la raiz del repositorio:

```bash
npm run dev:backend
npm run dev:frontends
npm run dev:all
npm run build:backend
npm run build:frontends
npm run build:all
```

URLs locales:

- Backend API: `http://localhost:5000`
- Candidatos: `http://127.0.0.1:5170`
- Empleadores: `http://127.0.0.1:5171`
- Administracion: `http://127.0.0.1:5172`

## Apps individuales

```bash
npm --prefix frontend/frontend-Candidato run dev
npm --prefix frontend/frontend-Empleador run dev
npm --prefix frontend/frontend-Admin run dev
```

## Endpoints principales

- `POST /api/candidates/register`
- `POST /api/employers/register`
- `POST /api/auth/login`
- `GET /api/vacantes`
- `GET /api/health`

## Archivos generados

No se versionan dependencias ni salidas de build:

- `node_modules`
- `frontend/**/dist`
- `backend/**/bin`
- `backend/**/obj`
- `backend/**/obj-buildcheck`

