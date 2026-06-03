# 🏋️ Anotador Gym API 

Backend cloud-native para gestión de entrenamiento físico, construido con ASP.NET Core, Docker y Microsoft Azure.

[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/) 
[![Azure](https://img.shields.io/badge/Azure-Cloud-blue)](https://azure.microsoft.com/) 
![Docker](https://img.shields.io/badge/Docker-Containerized-blue)
![CI/CD](https://github.com/lisandrosemperez-collab/AnotadorGymAppApi/actions/workflows/.github/workflows/main_anotadorgym-api.yaml/badge.svg)
![Azure SQL](https://img.shields.io/badge/Azure%20SQL-Database-0078D4) 
![Blob Storage](https://img.shields.io/badge/Azure-Blob%20Storage-0089D6) 

## 🚀 Overview
API Backend para una aplicación de fitness que gestiona ejercicios, rutinas y usuarios con control de acceso por roles.

Desplegada en Microsoft Azure usando App Service for Containers, Azure Container Registry y GitHub Actions CI/CD.

## 🛠️ Stack Tecnologico

### Backend
- ASP.NET Core 9
- Entity Framework Core
- SQL Server
- JWT Authentication

### Cloud & DevOps
- Microsoft Azure
- Azure App Service
- Azure Container Registry
- Azure Blob Storage
- GitHub Actions
- Docker

---

## 🧠 Arquitectura 
- **Pattern**: Vertical Slice (Features-based)
- **Estilo**: API stateless (escalado horizontal)
- **Separación**: Domain / Infrastructure / Features
- **Enfoque**: Cloud-native + 12-factor app

---

## ☁️ Infraestructura
- **Azure App Service (Linux Containers)** → Hosting de la API
- **Azure Container Registry (ACR)** → Registro privado de imágenes Docker
- **Azure SQL Database** → Persistencia principal
- **Azure Blob Storage** → Cache distribuido
- **GitHub Actions** → Pipeline CI/CD automatizado

---

## 🔄 CI/CD

Cada push a la rama `main` ejecuta automáticamente:

1. Checkout del repositorio
2. Login federado contra Azure (OIDC)
3. Build de la API
4. Build de imagen Docker
5. Push a Azure Container Registry
6. Deploy automático a Azure App Service

---

## 🐳 Docker & Containers

La aplicación se ejecuta completamente containerizada mediante Docker.

El flujo de despliegue incluye:

1. Build de imagen Docker
2. Push automático a Azure Container Registry
3. Deploy automático a Azure App Service

---

## ⚡ Features Principales 

- 🔐 Autenticación JWT con roles (`Admin` / `Invitado`)
- ⚡ Cache distribuido utilizando Azure Blob Storage
- 📦 API completamente containerizada con Docker
- 🔄 CI/CD automatizado con GitHub Actions
- ☁️ Infraestructura desplegada en Microsoft Azure
- 📊 Más de 900 ejercicios optimizados para lectura
- 🚀 Optimización de endpoints de alta demanda
- 🧠 Invalidación automática de cache
- 📈 Headers `X-Cache: HIT/MISS` para monitoreo

### 🔄 Flujo de datos

```txt
Cliente
   ↓
API ASP.NET Core
   ↓
Cache Distribuido (Blob Storage)
   ↓
Azure SQL Database (fallback)
```

---

## 📱 Ecosistema 

API utilizada por: 

- 🌐 Web App (React + TypeScript)
  - Panel administrativo

- 📱 Mobile App (.NET MAUI + SQLite)
  - Estrategia offline-first

---

## 🔐 Autenticación

JWT Bearer Token con dos modos: 
- **Admin**: acceso completo (CRUD)
- **Invitado**: solo lectura

```http:
Authorization: Bearer <token>
```

**📚 Swagger disponible en:** [anotadorgym-api.azurewebsites.net](https://anotadorgym-api.azurewebsites.net)

💡 Puedes probar la API directamente desde Swagger utilizando el endpoint /api/auth/login/invitado. 

---

## 📘 Ejemplo de endpoint 

### 🔹GET /api/ejercicios
Devuelve catálogo completo de ejercicios optimizado con cache.

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

---

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

---

## ✉️ Contacto 
- **📫 Contacto profesional**: [LinkedIn](https://www.linkedin.com/in/lisandro-semperez-24b1782b8/)
- **🔗 GitHub**: [GitHub](https://github.com/lisandrosemperez-collab)

⭐ **Si este proyecto te resulta útil o interesante, ¡considera darle una estrella en GitHub!**