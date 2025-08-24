# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is the Zenon Wallet API for .NET - a cross-platform REST API solution designed to interface with the Zenon Alphanet blockchain. It provides wallet management, blockchain interaction, and transaction submission capabilities built on ASP.NET Core 8.0.

## Development Commands

### Build the project
```bash
cd src
dotnet restore
dotnet build --configuration Release
```

### Run the API locally (Development)
```bash
cd src/ZenonWalletApi
dotnet run --environment Development
```

### Run the API locally (Production)
```bash
cd src/ZenonWalletApi
dotnet run --environment Production --urls https://localhost:443
```

### Run CI build workflow locally
```bash
cd src
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --no-restore --verbosity normal
```

## Architecture Overview

### Core Structure
- **API Framework**: ASP.NET Core 8.0 with minimal API endpoints pattern
- **Authentication**: JWT Bearer tokens with role-based authorization (Admin, User)
- **Configuration**: Uses ASP.NET Core configuration system with appsettings.json
- **Logging**: Serilog for structured logging
- **Documentation**: Swagger/OpenAPI for API documentation and testing

### Key Directories
- `src/ZenonWalletApi/Features/` - Contains all API endpoints organized by feature (one endpoint per folder)
- `src/ZenonWalletApi/Infrastructure/` - Cross-cutting concerns (auth, filters, exception handlers, configurations)
- `src/ZenonWalletApi/Services/` - Business logic and service implementations
- `src/ZenonWalletApi/Options/` - Configuration option classes for strongly-typed settings

### Endpoint Pattern
Each feature follows a consistent structure:
- `Features/{FeatureName}/Endpoint.cs` - Contains the endpoint definition, mapping, and handler
- Endpoints use minimal API pattern with extension methods for registration
- All endpoints are mapped in `Infrastructure/Registrations/EndpointRegistration.cs`

### Configuration System
The API uses a hierarchical configuration under the `Api:` section:
- `Api:Jwt` - JWT authentication settings
- `Api:Node` - Zenon node connection settings
- `Api:Wallet` - Wallet storage configuration
- `Api:AutoReceiver` - Automatic transaction receiver settings
- `Api:AutoLocker` - Wallet auto-lock settings
- `Api:Utilities:PlasmaBot` - Plasma bot integration

### Security Model
- JWT tokens for authentication (configured via `Api:Jwt:Secret`)
- Two authorization policies: "Admin" and "User"
- Users configured in `Api:Users` with BCrypt password hashing
- Wallet auto-lock for security after inactivity

### External Dependencies
- **Zenon.Sdk** - Official Zenon SDK for .NET for blockchain interaction
- **FluentValidation** - Input validation for API requests
- **Swashbuckle** - Swagger/OpenAPI documentation generation
- **BCrypt.Net-Next** - Password hashing
- **NetLah.Extensions.EventAggregator** - Event-driven architecture support

### Zenon Network Integration
- Connects to Zenon nodes via WebSocket (default: `ws://127.0.0.1:35998`)
- Supports Proof-of-Work (PoW) plasma generation
- Plasma bot integration for QSR fusion
- Auto-receiver for processing incoming transactions