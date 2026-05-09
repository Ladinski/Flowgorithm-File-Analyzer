# Flowgorithm Analyzer Pro - Backend Setup

## Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB or Express)
- Visual Studio 2022 or VS Code

## Installation

### 1. Restore NuGet Packages

```bash
cd Backend/FlowgorithmAnalyzer.API
dotnet restore
```

### 2. Database Setup

The project uses Entity Framework Core with SQL Server.

**Create database (LocalDB):**

```bash
cd Backend/FlowgorithmAnalyzer.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**Connection String:**

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FlowgorithmAnalyzerDb;Trusted_Connection=true;"
  }
}
```

### 3. Run the API

```bash
cd Backend/FlowgorithmAnalyzer.API
dotnet run
```

Server runs at `https://localhost:5001` or `http://localhost:5000`

## Project Structure

```
Backend/
├── FlowgorithmAnalyzer.API/         # ASP.NET Core Web API
│   ├── Controllers/                 # API endpoints
│   ├── Program.cs                   # Configuration
│   ├── appsettings.json             # Configuration
│   └── FlowgorithmAnalyzer.API.csproj
├── FlowgorithmAnalyzer.Core/        # Business Logic
│   ├── DTOs/                        # Data Transfer Objects
│   ├── Models/                      # Database models
│   ├── Services/                    # Analysis services
│   └── FlowgorithmAnalyzer.Core.csproj
└── FlowgorithmAnalyzer.Infrastructure/  # Data Access
    ├── Data/                        # DbContext
    └── FlowgorithmAnalyzer.Infrastructure.csproj
```

## API Endpoints

### Upload Files

- `POST /api/analysis/upload-single` - Upload single Flowgorithm file
- `POST /api/analysis/upload-bulk` - Upload ZIP with multiple files

### Analysis

- `POST /api/analysis/compare` - Compare two solutions
- `GET /api/analysis/student/{studentId}` - Get student performance

### Dashboard

- `GET /api/analysis/dashboard-summary` - Get overall statistics

## Services

### FlowgorithmParser
Parses .fgl XML files and extracts:
- Metadata (author, dates, titles)
- Variables and data types
- Flowchart elements
- Instruction hashes

### PlagiarismDetectionService
Detects:
- GPT-generated code
- Copy-paste indicators
- Metadata anomalies
- Solution similarities

### FileHandlingService
Handles:
- ZIP extraction
- File type detection
- File reading

## Features

✅ Single and bulk file uploads
✅ Automatic analysis and plagiarism detection
✅ GPT generation detection
✅ Metadata forensic analysis
✅ Student performance tracking
✅ Cross-submission comparison
✅ Real-time dashboard

## Debugging

Enable detailed logging in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

View Swagger UI: `https://localhost:5001/swagger`
