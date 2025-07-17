# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview
NGO-User-Case is a front-end system for an NGO platform built with ASP.NET Core MVC. It serves both general users and case clients, providing functionality for activity registration, supply procurement, and user management.

**For detailed project structure analysis, see [PROJECT_STRUCTURE.md](./PROJECT_STRUCTURE.md)**
**For database ER diagram, see [NGOPlatformDb - NGOPlatformDb - dbo.png](./NGOPlatformDb%20-%20NGOPlatformDb%20-%20dbo.png)**

## Development Commands

### Docker Development Environment
```bash
# Start development environment
docker-compose up -d

# Stop development environment
docker-compose down

# View SQL Server logs
docker logs ngo-sqlserver

# Copy database backup to container
docker cp /path/to/backup.bak ngo-sqlserver:/tmp/

# Access SQL Server container
docker exec -it ngo-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'P@sswords715981432'
```

### Local Development (in NGOPlatformWeb directory)
```bash
# Restore packages
dotnet restore

# Build project
dotnet build

# Run application
dotnet run

# Run on specific ports
dotnet run --urls="http://localhost:5000;https://localhost:5001"
```

## Architecture Overview

### Core Technology Stack
- **Framework**: ASP.NET Core 8.0 with MVC pattern
- **Database**: SQL Server 2022 with Entity Framework Core 9.0.6
- **Authentication**: Cookie-based authentication (2-hour expiration)
- **Frontend**: Bootstrap, jQuery, vanilla CSS/JS
- **Containerization**: Docker with Docker Compose

### Controller Structure
- **HomeController**: Homepage, organization info, and navigation
- **UserController**: General user functionality (registration, login, profile editing)
- **CaseController**: Case client operations (activity viewing, supply procurement)
- **AuthController**: Shared authentication handling
- **ActivityController**: Activity-related operations
- **EventController**: Event management
- **PurchaseController**: Supply procurement operations

### Key Database Entities
- **User**: General users of the platform
- **Case**: Case clients requiring assistance
- **Activity**: Events and activities
- **Supply**: Available supplies and resources
- **SupplyCategory**: Supply categorization
- **RegularSupplyNeeds**: Regular supply requirements
- **EmergencySupplyNeeds**: Emergency supply requirements

### Default Routing
The application defaults to `{controller=Case}/{action=CasePurchaseList}/{id?}`, indicating a case-centric system design.

## Database Configuration

### Connection Settings
- **Server**: `ngo-sqlserver` (Docker container)
- **Database**: `NGOPlatformDb`
- **Authentication**: SQL Server authentication with sa account
- **Connection String**: `Server=ngo-sqlserver;Database=NGOPlatformDb;User Id=sa;Password=P@sswords715981432;TrustServerCertificate=True;`

### Database Restoration
```sql
-- Restore database from backup
RESTORE DATABASE NGOPlatformDb 
FROM DISK = '/tmp/backup.bak'
WITH REPLACE,
MOVE 'LogicalName' TO '/var/opt/mssql/data/NGOPlatformDb.mdf',
MOVE 'LogicalName_Log' TO '/var/opt/mssql/data/NGOPlatformDb_Log.ldf'

-- Check database tables
USE NGOPlatformDb;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;
```

## Development Workflow

### Git Branching Strategy
- **main**: Production-ready code
- **develop**: Development branch for integration
- **feature/***: Feature development branches

### Common Development Tasks
- Use `docker-compose up -d` for consistent development environment
- Database changes require backup restoration or EF migrations
- The application uses session-based authentication with 2-hour expiration
- All database operations go through Entity Framework Core DbContext

### Docker Services
- **devcontainer**: ASP.NET Core development environment (ports 5000, 5001)
- **sqlserver**: SQL Server 2022 database (port 1433)
- **Volume**: `sqlserver_ngo_data` for database persistence

## Recent Major Updates (2025-07-15)

### Activity Registration Records System
- **UserActivityRegistration** and **CaseActivityRegistration** entities added
- Complete activity participation tracking for both user types
- Activity registration overview integrated into profile pages
- Detailed activity records pages with filtering functionality

### CSS Architecture Optimization
- **Major refactoring**: 3000+ lines of inline CSS converted to modular architecture
- **4 CSS modules created**:
  - `profile-shared.css` - Common profile page styles
  - `profile-components.css` - Profile-specific components
  - `activity-records.css` - Activity-related styles
  - `purchase-records.css` - Purchase-related styles
- **Theme differentiation**: User (blue theme) vs Case (teal theme)
- **Layout fix**: Added missing `@RenderSection("Styles")` to `_Layout.cshtml`
- **Purchase records page**: Fixed layout issues and improved responsive design

### Updated File Structure
```
Views/
├── User/
│   ├── UserProfile.cshtml (CSS optimized)
│   ├── Registrations.cshtml (new - activity records)
│   └── PurchaseRecords.cshtml (CSS optimized)
├── Case/
│   ├── CaseProfile.cshtml (CSS optimized)
│   └── Registrations.cshtml (new - activity records)
└── Shared/
    └── _Layout.cshtml (fixed CSS section)

wwwroot/css/
├── profile-shared.css (new)
├── profile-components.css (new)
├── activity-records.css (new)
├── purchase-records.css (new)
├── login.css (Auth pages)
├── payment.css (Payment pages)
├── purchase.css (Purchase index)
└── site.css (ASP.NET Core default)
```

### Database Schema Additions
```sql
-- New entities for activity registration tracking
UserActivityRegistrations (RegistrationId, UserId, ActivityId, Status, RegisterTime)
CaseActivityRegistrations (RegistrationId, CaseId, ActivityId, Status, RegisterTime)
```

## Project Context
This is a Taiwanese NGO platform focused on case management, supply procurement, and activity coordination. The codebase contains Chinese comments and is designed to work alongside a separate backend administration system sharing the same database.

**For detailed development progress, see [DEVELOPMENT_PROGRESS.md](./DEVELOPMENT_PROGRESS.md)**
**For CSS optimization details, see [CSS_OPTIMIZATION_RECORD.md](./CSS_OPTIMIZATION_RECORD.md)**