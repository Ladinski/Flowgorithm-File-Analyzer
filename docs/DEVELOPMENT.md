# Development Guide

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    React Frontend (Port 3000)               │
│              Tailwind CSS + TypeScript + Vite               │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTP/REST
                         ↓
┌─────────────────────────────────────────────────────────────┐
│              ASP.NET Core API (Port 5000)                  │
│                  FlowgorithmAnalyzer.API                    │
│  Controllers → Services → Models → Database                 │
└────────────────────────┬────────────────────────────────────┘
                         │ Entity Framework Core
                         ↓
┌─────────────────────────────────────────────────────────────┐
│            SQL Server LocalDB / Express                      │
│              FlowgorithmAnalyzerDb                           │
└─────────────────────────────────────────────────────────────┘
```

## Project Structure

### Backend

#### FlowgorithmAnalyzer.API
- **Controllers/AnalysisController.cs** - API endpoints
- **Program.cs** - Startup and DI configuration
- **appsettings.json** - Database and app configuration

#### FlowgorithmAnalyzer.Core
- **DTOs/** - Data transfer objects for API requests/responses
- **Models/** - Entity Framework models (Student, Submission, etc.)
- **Services/** - Business logic (FlowgorithmParser, PlagiarismDetectionService)

#### FlowgorithmAnalyzer.Infrastructure
- **Data/ApplicationDbContext.cs** - Entity Framework DbContext

### Frontend

- **src/pages/** - React page components
  - Dashboard.tsx - Overview and statistics
  - UploadPage.tsx - File upload interface
  - AnalysisResults.tsx - Results display
  - StudentTracking.tsx - Student performance
- **src/services/** - API client (Axios)
- **src/App.tsx** - Main component and routing
- **index.html** - HTML entry point

## Key Algorithms

### Plagiarism Detection

1. **Hash-based comparison** (40%)
   - Normalize each instruction
   - Generate SHA256 hashes
   - Compare hash sets

2. **Structure analysis** (35%)
   - Extract element sequence
   - Calculate Levenshtein distance
   - Compare variable patterns

3. **Flow similarity** (25%)
   - Element-by-element type matching
   - Process/decision/loop counting

### Risk Scoring

```
Risk Score = Sum(Risk Indicator Severity × 20) / 100
```

Risk Levels:
- **Low**: 0-29%
- **Medium**: 30-59%
- **High**: 60-79%
- **Critical**: 80-100%

## Adding New Features

### Adding a New Service

1. Create interface in Core/Services
2. Implement in same folder
3. Register in Program.cs: `builder.Services.AddScoped<IMyService, MyService>();`
4. Inject in controller

### Adding a New API Endpoint

1. Add method to AnalysisController
2. Use `[HttpPost]` or `[HttpGet]` attributes
3. Return `ApiResponse<T>` wrapper
4. Document in API.md

### Adding a New Page

1. Create React component in `src/pages/`
2. Import in App.tsx
3. Add route in Routes section
4. Update Navigation component

## Testing Locally

### Backend
```bash
cd Backend/FlowgorithmAnalyzer.API
dotnet run
# Visit http://localhost:5000/swagger
```

### Frontend
```bash
cd Frontend
npm run dev
# Visit http://localhost:3000
```

### Test Upload
1. Create a .fgl file (XML)
2. Go to /upload
3. Fill form and upload
4. Check Dashboard

## Debugging

### Backend
- Use Visual Studio debugger
- Check Application Insights logs
- Review appsettings.Development.json

### Frontend
- Use React DevTools
- Check browser console
- Use VS Code debugger

## Code Style

### C#
- PascalCase for classes, methods, properties
- camelCase for parameters and locals
- Async methods end with Async

### TypeScript/React
- PascalCase for components and types
- camelCase for functions and variables
- Use interfaces over types when possible

## Performance Optimization

### Backend
- Use async/await
- Add database indexes
- Implement pagination for large datasets

### Frontend
- Lazy load pages
- Memoize expensive computations
- Optimize images and assets

## Security Considerations

- Validate all file uploads
- Implement authentication
- Use HTTPS in production
- Sanitize user input
- Implement rate limiting
- Use prepared statements (EF Core does this)

## Database Maintenance

### Backup
```bash
dotnet ef database drop
dotnet ef database update
```

### Create Migration
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Seed Sample Data
Add to Program.cs or create seed method in DbContext

## CI/CD

### Pre-commit
- Run tests
- Lint code
- Check formatting

### Build
- Restore packages
- Build project
- Run tests

### Deploy
- Publish backend
- Build frontend
- Deploy containers

## Documentation

- Update README.md for major changes
- Document complex algorithms
- Add code comments for unclear logic
- Keep API.md and DATABASE.md current
