# 🏋️ Anotador Gym API
API RESTful para la gestión de ejercicios y entrenamientos. Permite importar, validar y consultar ejercicios, con autenticación mediante JWT y despliegue automatizado en Railway.

## 🚀 Características
- **.NET 9.0 con Web API** (controladores y minimal APIs).
- **Autenticación JWT** solo para endpoints de importación y validación.
- **Documentación interactiva** con Swagger/OpenAPI.
- **Importación masiva** de ejercicios desde archivos JSON.
- **Validación de formato** sin persistencia.
- **Base de datos PostgreSQL** en [Neon](https://neon.tech) con más de 900 ejercicios precargados.
- **Dockerizado** – listo para desarrollo y producción.
- **Despliegue continuo desde GitHub a Railway.**

## 📦 Tecnologías
- [.NET 9](https://dotnet.microsoft.com/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/) (con PostgreSQL)
- [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [Docker](https://www.docker.com/)
- [Railway](https://railway.app/)
- [Neon PostgreSQL](https://neon.tech)

## 🌐 API en producción
La API ya se encuentra desplegada y funcionando en Railway. Puedes probarla directamente sin necesidad de clonar el repositorio:

**📚 Swagger UI:** https://anotadorgymappapi-production.up.railway.app/index.html

**Desde Swagger** puedes explorar todos los endpoints, ver los modelos de datos y probar las peticiones en tiempo real.

Los endpoints **GET /api/ejercicios** son públicos y no requieren autenticación.

Los endpoints **POST /api/imports y POST /api/imports/validate** requieren un **token JWT**. Para obtenerlo, contacta al administrador (ver sección **Soporte y Contacto**).

**Nota:** La base de datos de producción contiene más de 900 ejercicios precargados, por lo que las respuestas del GET serán inmediatas y enriquecidas.

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
- En **producción** (Railway), configúralas desde el dashboard del proyecto (sección Variables).

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

## 🌍 Despliegue en Railway
**Este repositorio está configurado para desplegarse automáticamente en Railway:**

- **Haz fork o conecta** tu repositorio a Railway.
- **Railway detecta el Dockerfile** y establece la variable PORT=8080 automáticamente.
- **Obtendrás una URL pública** como [https://anotador-api.up.railway.app](https://anotadorgymappapi-production.up.railway.app/).

## 🔐 Autenticación
Los siguientes endpoints requieren un token JWT en el encabezado Authorization:

- **POST /api/imports (importación)**
- **POST /api/imports/validate**
- **El resto de endpoints** (como GET /api/ejercicios) son públicos y no necesitan autenticación.

### Obtener un token
Contacta al administrador en lisandrosemperez@gmail.com para solicitar un token de acceso.

### Usar el token
Incluye el token en el encabezado Authorization:

```text
Authorization: Bearer <tu-token-jwt>
```
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

## 📱 Aplicación móvil MAUI
Este API es consumida por la aplicación móvil [AnotadorGymApp](https://github.com/lisandrosemperez-collab/AnotadorGymApp) desarrollada en .NET MAUI. La app sincroniza los ejercicios desde la API y los almacena en una base de datos local SQLite para su uso offline.

## ✉️ Soporte y Contacto

Este proyecto es mantenido activamente por **Lisandro Semperez**.
- **📫 Contacto profesional**: [LinkedIn](https://www.linkedin.com/in/lisandro-semperez-24b1782b8/) | [Email](mailto:lisandrosemperez@gmail.com)
- **🔗 GitHub**: [GitHub](https://github.com/lisandrosemperez-collab)

⭐ **Si este proyecto te resulta útil o interesante, ¡considera darle una estrella en GitHub!**
