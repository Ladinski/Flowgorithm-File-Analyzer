# Flowgorithm Analyzer Pro - Setup Instructions

## 🚀 Complete Setup Guide

### Step 1: Backend Setup

#### Prerequisites
- Install [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- Install [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-express) or use LocalDB

#### Configure Database

1. Navigate to Backend folder:
```bash
cd Backend/FlowgorithmAnalyzer.API
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Apply migrations:
```bash
dotnet ef database update
```

4. Run the server:
```bash
dotnet run
```

The API will start at `https://localhost:5001` or `http://localhost:5000`

### Step 2: Frontend Setup

#### Prerequisites
- Install [Node.js 16+](https://nodejs.org/)

#### Installation

1. Navigate to Frontend folder:
```bash
cd Frontend
```

2. Install dependencies:
```bash
npm install
```

3. Create `.env` file (copy from `.env.example`):
```bash
cp .env.example .env
```

4. Run development server:
```bash
npm run dev
```

The frontend will start at `http://localhost:3000`

### Step 3: Access the Application

1. Open browser
2. Go to `http://localhost:3000`
3. Explore the dashboard

## 📁 File Structure

```
FlowgorithmAnalyzerPro/
├── Backend/
│   ├── FlowgorithmAnalyzer.API/
│   │   ├── Controllers/          # API endpoints
│   │   ├── Program.cs            # Startup configuration
│   │   ├── appsettings.json      # Database connection
│   │   └── .csproj               # Project file
│   ├── FlowgorithmAnalyzer.Core/
│   │   ├── DTOs/                 # Data transfer objects
│   │   ├── Models/               # Entity models
│   │   ├── Services/             # Business logic
│   │   └── .csproj
│   └── FlowgorithmAnalyzer.Infrastructure/
│       ├── Data/
│       │   └── ApplicationDbContext.cs
│       └── .csproj
├── Frontend/
│   ├── src/
│   │   ├── pages/                # React pages
│   │   ├── services/             # API client
│   │   ├── App.tsx               # Main component
│   │   ├── main.tsx              # Entry point
│   │   └── index.css             # Tailwind styles
│   ├── package.json
│   ├── vite.config.ts            # Build configuration
│   ├── tsconfig.json             # TypeScript config
│   └── index.html                # HTML template
├── README.md                     # Main documentation
└── SETUP.md                      # This file
```

## 🎯 Features to Try

### 1. Upload a Flowgorithm File
- Go to `/upload` page
- Fill in student information
- Upload a `.fgl` file
- View analysis results instantly

### 2. Check Dashboard
- View overall statistics
- See high-risk cases
- Monitor trends

### 3. Track Students
- Search for specific students
- View their performance history
- Analyze individual submissions

### 4. Compare Solutions
- Select two submissions
- See similarity scores
- Identify matching patterns

## 📊 Database

Default connection string uses LocalDB:
```
Server=(localdb)\mssqllocaldb;Database=FlowgorithmAnalyzerDb;Trusted_Connection=true;
```

To change database:
1. Edit `Backend/FlowgorithmAnalyzer.API/appsettings.json`
2. Update connection string
3. Run `dotnet ef database update`

## 🔍 Testing

### Create Test Flowgorithm File

Create a simple `.fgl` file (XML format):

```xml
<?xml version="1.0" encoding="utf-8"?>
<flowgorithm fileversion="3.0">
  <information>
    <name>TestProgram</name>
    <author>Test Student</author>
    <date>2024-01-15</date>
    <description>Test Algorithm</description>
    <version>1.0</version>
    <created>2024-01-15T10:00:00</created>
    <modified>2024-01-15T10:30:00</modified>
  </information>
  <symbols>
    <symbol>
      <name>x</name>
      <type>Integer</type>
      <value>0</value>
    </symbol>
  </symbols>
  <flowchart>
    <start x="10" y="10">
      <text>Start</text>
    </start>
    <process x="10" y="60">
      <text>x = 5</text>
    </process>
    <end x="10" y="110">
      <text>End</text>
    </end>
  </flowchart>
</flowgorithm>
```

## ⚙️ Configuration

### Backend (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FlowgorithmAnalyzerDb;Trusted_Connection=true;"
  },
  "AllowedHosts": "*"
}
```

### Frontend (.env)

```
REACT_APP_API_URL=http://localhost:5000
```

## 🐛 Troubleshooting

### Port Already in Use

**Backend (Port 5000)**:
```bash
# Windows
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# macOS/Linux
lsof -i :5000
kill -9 <PID>
```

**Frontend (Port 3000)**:
```bash
# Windows
netstat -ano | findstr :3000
taskkill /PID <PID> /F

# macOS/Linux
lsof -i :3000
kill -9 <PID>
```

### Database Issues

```bash
# Reset database
dotnet ef database drop
dotnet ef database update

# View current migrations
dotnet ef migrations list
```

### CORS Errors

Ensure backend is running and configure CORS in `Program.cs`:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
```

### Module Not Found

```bash
# Clear and reinstall
cd Frontend
rm -rf node_modules
npm install
npm run dev
```

## 📚 API Documentation

Access Swagger documentation at:
```
http://localhost:5000/swagger
```

## 🚀 Production Deployment

### Backend

```bash
# Build
dotnet publish -c Release -o ./publish

# Deploy to IIS or Azure
```

### Frontend

```bash
# Build
npm run build

# Output in dist/ folder
```

## 📝 Development Tips

1. **Hot Reload**: Both frontend and backend support hot reload
2. **Debug**: Use VS Code or Visual Studio debugger
3. **Logging**: Check console for detailed logs
4. **API Testing**: Use Swagger UI or Postman

## 🔐 Security Notes

- Change default connection string for production
- Implement authentication/authorization
- Secure API endpoints with tokens
- Use HTTPS in production
- Validate all file uploads

## 📞 Support

For detailed information:
- Backend: See [Backend/README.md](Backend/README.md)
- Frontend: See [Frontend/README.md](Frontend/README.md)

---

**Happy Analyzing!** 🎓
