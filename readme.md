# 🏋️ Anotador Gym API 
[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/) 
[![Azure](https://img.shields.io/badge/Azure-Cloud-blue)](https://azure.microsoft.com/) 
![Azure SQL](https://img.shields.io/badge/Azure%20SQL-Database-0078D4) 
![Blob Storage](https://img.shields.io/badge/Azure-Blob%20Storage-0089D6) 
![CI/CD](https://img.shields.io/badge/GitHub%20Actions-CI/CD-black)

API backend cloud-native para gestión de rutinas de entrenamiento, diseñada con arquitectura escalable y desplegada en Azure.

## 🚀 Overview
Backend para una aplicación de fitness que gestiona ejercicios, rutinas y usuarios con control de acceso por roles.

Diseñada para alto volumen de lectura, escalabilidad horizontal y despliegue en cloud.

## 🧠 Arquitectura 
- **Pattern**: Vertical Slice (Features-based)
- **Estilo**: API stateless (escalado horizontal)
- **Separación**: Domain / Infrastructure / Features
- **Enfoque**: Cloud-native + 12-factor app

## ☁️ Infraestructura
- **Azure App Service** → Hosting de la API 
- **Azure SQL Database** → Persistencia principal
- **Azure Blob Storage** → Cache distribuido
- **GitHub Actions** → CI/CD automático (build + deploy)

## ⚡ Features clave 
- 🔐 **Autenticación JWT** con roles (Admin / Invitado)
- ⚡ **Cache distribuido** con **Blob Storage**
- - Mejora de performance en endpoints de alta carga
- - Headers X-Cache: HIT/MISS
- - Invalidación automática en escrituras
🔄 **CI/CD completo**
- - Build automático
- - Deploy directo a Azure
- 📊 +900 ejercicios precargados optimizados para lectura

### 🔄 Flujo de datos

Cliente → API → Cache (Blob Storage) → Base de datos (fallback) 
✔ Reduce carga en DB 
✔ Mejora tiempos de respuesta 
✔ Mantiene consistencia mediante invalidación de cache 

## 📱 Ecosistema 

API utilizada por: 
🌐 Web App (React + TypeScript) → panel administrativo
📱 Mobile App (.NET MAUI) → modo offline-first con SQLite

## 🔐 Autenticación

JWT Bearer Token con dos modos: 
- **Admin**: acceso completo (CRUD)
- **Invitado**: solo lectura

```Header:
Authorization: Bearer <token>
```

**📚 Swagger disponible en:** anotadorgym-api.azurewebsites.net 
💡 Puedes probar la API directamente desde Swagger utilizando el endpoint /api/auth/login/invitado. 

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

## 🧱 Tecnologías 

- .NET 9 / ASP.NET Core
- Entity Framework Core
- Azure SQL Database
- Azure Blob Storage
- JWT Authentication
- GitHub Actions

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

## 🌍 Deploy 
Deploy automático a Azure App Service en cada push a main mediante GitHub Actions. 

## ✉️ Contacto 
- **📫 Contacto profesional**: [LinkedIn](https://www.linkedin.com/in/lisandro-semperez-24b1782b8/)
- **🔗 GitHub**: [GitHub](https://github.com/lisandrosemperez-collab)

⭐ **Si este proyecto te resulta útil o interesante, ¡considera darle una estrella en GitHub!**
