# CineCore 🎬

Sistema de administración de cine desarrollado con ASP.NET Core MVC y Entity Framework Core.

## Tecnologías

- C# / .NET 10
- ASP.NET Core MVC
- Entity Framework Core (Code First)
- SQL Server (LocalDB)
- ASP.NET Identity
- Bootstrap 5

## Funcionalidades

### Empleados
- Gestión de Películas, Géneros, Salas, Tipos de Sala y Funciones
- CRUD completo para cada entidad
- Panel de administración protegido por rol

### Clientes
- Registro e inicio de sesión
- Visualización de cartelera de funciones
- Reserva de butacas con selección visual
- Cancelación de reservas desde su panel

## Arquitectura

- Patrón MVC con arquitectura en capas
- Code First con migraciones de Entity Framework
- Sistema de roles con ASP.NET Identity (Empleado / Cliente)
- Relaciones: uno a muchos, muchos a muchos (Película-Género)
- Butacas generadas automáticamente al crear una sala

## Instalación

1. Clonar el repositorio git clone https://github.com/AgustinPagliuca/CineCore.git
2. Abrir `CineCore.sln` en Visual Studio 2022
3. Ejecutar migraciones en Package Manager Console: Update-Database
4. Correr el proyecto con `F5`

## Usuario de prueba

**Empleado admin:**
- Email: `admin@cinecore.com`
- Password: `Admin123!`