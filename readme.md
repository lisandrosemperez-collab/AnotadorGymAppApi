# 🏋️ Anotador Gym API
[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-blue)](https://learn.microsoft.com/aspnet/core)
![EF Core](https://img.shields.io/badge/EF%20Core-9.0-orange)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-%E2%9C%94-2496ED?logo=docker)
![Render](https://img.shields.io/badge/Render-deployed-46E3B7?logo=render)
[![Swagger](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?logo=swagger)](https://anotadorgymappapi.onrender.com)

API RESTful profesional para la gestión de ejercicios y entrenamientos. Este servidor centraliza la lógica de negocio, seguridad y persistencia de datos para un ecosistema integral de fitness.

## 🚀 Características
- **Arquitectura Backend:** Implementación de **Vertical Slice Architecture** (Features) en .NET 9.
- **Seguridad Avanzada:** Autenticación **JWT** con control de acceso basado en roles (**Admin/Invitado**) y hashing de contraseñas con **BCrypt**.
- **Base de Datos:** PostgreSQL alojado en [Neon](https://neon.tech) con más de 900 ejercicios precargados.
- **Optimización de Rendimiento:** Endpoint principal optimizado para servir grandes volúmenes de datos con tiempos de respuesta mínimos.
- **Documentación Dinámica:** Swagger/OpenAPI enriquecido con comentarios XML para una integración fluida con clientes.
- **Despliegue Profesional:** Dockerizado y desplegado en la nube mediante **Render**.


## 📦 Tecnologías
- **Core:** .NET 9 & ASP.NET Core.
- **Persistencia:** Entity Framework Core (PostgreSQL).
- **Seguridad:** Microsoft.AspNetCore.Authentication.JwtBearer.
- **Infraestructura:** Docker & Render.

## 📁 Estructura del Proyecto (Vertical Slicing)
La lógica está organizada por **Features**, permitiendo que cada funcionalidad sea independiente y escalable:
```text
src/
├── Domain/           # Entidades puras (Ejercicio, Rutina, Usuario)
├── Infrastructure/   # Persistencia (DbContext), Migraciones y Seguridad (JwtProvider)
└── Features/         # Rebanadas verticales (Slice)
    ├── Usuarios/     # Login, Roles, AuthController, UsuarioService
    ├── Ejercicios/   # Listado y CRUD de ejercicios
    └── Rutinas/      # Gestión de planes de entrenamiento
```

## 🌐 API en producción
La API ya se encuentra desplegada y funcionando en Render. Puedes probarla directamente sin necesidad de clonar el repositorio:

**📚 Swagger UI:** https://anotadorgymappapi.onrender.com/

Nota: Los métodos de consulta (GET) son públicos. Los métodos de modificación (POST, PUT, DELETE) requieren autorización mediante encabezado Authorization: Bearer <token>.

## 🔧 Configuración local

### Prerrequisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (opcional)
- Git

### Clonar y ejecutar
```bash
git clone https://github.com/lisandrosemperez-collab/AnotadorGymAppApi.git
cd AnotadorGymAppApi
dotnet restore
dotnet run
```
La API estará disponible en http://localhost:5000 (por defecto).
Swagger UI: http://localhost:5000/swagger

## ⚙️ Configuración de variables de entorno

La API requiere las siguientes variables para funcionar correctamente:

| Variable                          | Descripción                                                                               | Ejemplo (desarrollo)                                                                 |
|-----------------------------------|-------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------|
| `ConnectionStrings__DefaultConnection` | Cadena de conexión a PostgreSQL en Neon. **Debes solicitarla al administrador** (ver contacto abajo) o crear tu propia base de datos. | `Host=ep-mute-darkness-xxx.neon.tech;Database=neondb;Username=alice;Password=xxxx;SSL Mode=Require` |
| `Jwt__Issuer`                     | Emisor del token JWT. Puede ser cualquier nombre, por ejemplo `"Admin"`.                 | `Admin`                                                                              |
| `Jwt__Secret`                     | Clave secreta para firmar tokens (mínimo 16 caracteres). Usa una diferente en producción. | `MiClaveSuperSecretaParaDesarrollo123`                                               |

**Importante:**  
- No subas estos valores al repositorio.  
- En **desarrollo**, puedes definirlas en `appsettings.json` (este archivo debe estar en `.gitignore`).
- En **producción** (Render), configúralas desde el dashboard del servicio

#### 🔹 Ejemplo de `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=AnotadorGym;Username=postgres;Password=tu_password"
  },
  "Jwt": {
    "Issuer": "Admin",
    "Secret": "MiClaveSuperSecretaParaDesarrollo123"
  }
}
```
Nota: Si no deseas usar tu propia base de datos, escribe a lisandrosemperez@gmail.com para obtener la cadena de conexión a la base de datos precargada con 900 ejercicios.

## 🐳 Ejecutar con Docker
```bash
docker build -t anotador-api .
docker run -d -p 5000:8080 -e PORT=8080 --name anotador-api anotador-api
```
Accede a http://localhost:5000/swagger

## 🌍 Despliegue en Render

Este repositorio está configurado para desplegarse automáticamente en Render mediante Docker:

- Conecta tu repositorio de GitHub a Render.
- Render detecta automáticamente el `Dockerfile`.
- Configura las variables de entorno desde el dashboard.
- El servicio se expone mediante una URL pública.

Swagger disponible en:
https://anotadorgymappapi.onrender.com

## 🔐 Autenticación
Para acceder a endpoints protegidos (como importación de datos), debés obtener un token a través del login:

Endpoint: POST /api/auth/login

Payload: { "userName": "admin", "password": password }

Uso: Incluir el string devuelto en el header: Authorization: Bearer <tu-token>

Desde Swagger UI, haz clic en el botón Authorize y pega el token en el formato Bearer <token>.

## 📘 Endpoints principales
### 🔹GET /api/ejercicios
Devuelve la lista completa de ejercicios.

- **Respuesta exitosa (200 OK):**
```json
[
  {
    "nombre": "Curl de Biceps con Barra Recta",
    "grupoMuscular": {
      "nombre": "Brazos"
    },
    "musculoPrimario": {
      "nombre": "Biceps Braquial"
    },
    "musculosSecundarios": [
      { "nombre": "Braquial" },
      { "nombre": "Braquiorradial" },
      { "nombre": "Flexores de la muñeca" }
    ]
  }
]
```

### 🔹POST /api/imports/validate
multipart/form-data
Valida la estructura de un archivo JSON sin guardarlo.

Parámetros:

| Nombre | Tipo | Descripción
|-----------|-------------|-------------|
| **archivo**	| **IFormFile**	| **Archivo .json con ejercicios** |

- **Respuesta (200 OK):**
```json
{
  "esValido": true,
  "cantidadEjercicios": 5,
  "mensaje": "Formato válido. 5 ejercicios detectados"
}
```

### 🔹POST /api/imports
multipart/form-data
Importa y guarda ejercicios desde un archivo JSON.

Parámetros:

| Nombre | Tipo |	Descripción |
|-----------|-------------|-------------|
| **archivo**	| **IFormFile**	| **Archivo .json (máx 10 MB)** |

- **Respuesta exitosa (201 Created):**
```json
{
  "ejerciciosImportados": 5,
  "ejerciciosOmitidos": 0,
  "errores": [],
  "mensaje": "Importación completada exitosamente"
}
```

## 📁 Estructura del archivo JSON esperado
```json
 [
  {
    "nombre": "Curl de Biceps con Barra Recta",
    "grupoMuscular": {
      "nombre": "Brazos"
    },
    "musculoPrimario": {
      "nombre": "Biceps Braquial"
    },
    "musculosSecundarios": [
      { "nombre": "Braquial" },
      { "nombre": "Braquiorradial" },
      { "nombre": "Flexores de la muñeca" }
    ],
    "descripcion": "Opcional"
  }
]
```
### Reglas:

**Nombre:** obligatorio, único.

**Descripcion:** opcional.

## 🗄️ Base de datos
La API utiliza **PostgreSQL** alojado en [Neon](https://neon.tech/). Actualmente la base de datos contiene más de **900 ejercicios** precargados para facilitar el desarrollo y las pruebas.

Si deseas utilizar esta base de datos precargada, solicita la **cadena de conexión** al administrador (ver contacto abajo).
De lo contrario, puedes crear tu propia base de datos en Neon (gratuito) y usar migraciones de Entity Framework para generar el esquema.

📱 Ecosistema de Aplicaciones
Esta API sirve como el motor principal para los siguientes clientes:

WebApp Administrativa: Desarrollada en React + TypeScript, permite la gestión visual de los datos con soporte para temas (Dark/Light Mode) y rutas protegidas por roles.

App Móvil Nativa: Desarrollada en .NET MAUI, utiliza esta API para poblar una base de datos local SQLite bajo una arquitectura Offline-first.
[AnotadorGymApp](https://github.com/lisandrosemperez-collab/AnotadorGymApp)

## ✉️ Soporte y Contacto

Este proyecto es mantenido activamente por **Lisandro Semperez**.
- **📫 Contacto profesional**: [LinkedIn](https://www.linkedin.com/in/lisandro-semperez-24b1782b8/) | [Email](mailto:lisandrosemperez@gmail.com)
- **🔗 GitHub**: [GitHub](https://github.com/lisandrosemperez-collab)

⭐ **Si este proyecto te resulta útil o interesante, ¡considera darle una estrella en GitHub!**
