# Flowgorithm Analyzer Pro

> **AI-Powered Academic Integrity Detection System for Flowgorithm Files**

A comprehensive C# .NET web application designed to analyze Flowgorithm files, detect plagiarism, and track student performance over time. Built with cutting-edge technology for educators and academic administrators.

## 🎯 Project Overview

**Flowgorithm Analyzer Pro** is a batch analysis and student performance tracking system that:

- 📤 Accepts single Flowgorithm files (.fgl) or ZIP archives
- 🔍 Automatically analyzes multiple student solutions
- 🕵️ Performs forensic analysis on student submissions
- 🔀 Compares solutions within a session
- 📊 Tracks student performance over time
- 🎨 Displays results through an intuitive, beautiful UI

### Core Features

- ✅ **Plagiarism Detection** - Detect copied, modified, or suspicious solutions
- ✅ **GPT Analysis** - Identify AI-generated content patterns
- ✅ **Metadata Forensics** - Analyze file creation/modification patterns
- ✅ **Performance Tracking** - Monitor student progress across assignments
- ✅ **Bulk Processing** - Analyze entire classes at once
- ✅ **Beautiful Dashboard** - Real-time analytics and visualizations
- ✅ **Risk Scoring** - Automatic flagging of high-risk submissions

## 🏗️ Architecture

### Backend Stack
- **Framework**: ASP.NET Core 8.0
- **Database**: Entity Framework Core with SQL Server
- **Analysis**: Custom plagiarism detection engine
- **API**: RESTful API with Swagger documentation

### Frontend Stack
- **Framework**: React 18 with TypeScript
- **Styling**: Tailwind CSS
- **Charting**: Recharts for visualizations
- **State**: Zustand for state management
- **Build**: Vite

## 📁 Project Structure

```
FlowgorithmAnalyzerPro/
├── Backend/
│   ├── FlowgorithmAnalyzer.API/          # Web API
│   ├── FlowgorithmAnalyzer.Core/         # Business logic
│   └── FlowgorithmAnalyzer.Infrastructure/ # Data access
├── Frontend/                              # React application
├── docs/                                 # Documentation
└── README.md
```

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Node.js 16+
- SQL Server (LocalDB or Express)
- Visual Studio Code or Visual Studio 2022

### Backend Setup

```bash
cd Backend/FlowgorithmAnalyzer.API
dotnet restore
dotnet ef database update
dotnet run
```

Backend runs at: `http://localhost:5000`

### Frontend Setup

```bash
cd Frontend
npm install
npm run dev
```

Frontend runs at: `http://localhost:3000`

## 📊 Dashboard Features

### Real-Time Analytics
- Total submissions and analysis count
- Flagged cases monitoring
- Average plagiarism scores
- Risk level distribution (Low/Medium/High/Critical)

### Student Tracking
- Individual performance profiles
- Assignment history with scores
- Risk level classification
- Trend analysis

### Analysis Results
- Plagiarism score with detailed breakdown
- Risk indicators and severity levels
- Forensic analysis reports
- Evidence and pattern matching

## 🔍 Detection Algorithms

### Plagiarism Detection
1. **Hash Comparison** (40% weight)
   - Instruction-level hash matching
   - Identical code block detection

2. **Structure Analysis** (35% weight)
   - Element sequence comparison
   - Flow pattern matching
   - Variable usage patterns

3. **Flow Similarity** (25% weight)
   - Decision tree comparison
   - Loop structure analysis
   - Element flow patterns

### GPT Detection
- Perfect structure indicators
- Generic variable naming patterns
- Overly sophisticated documentation
- Abnormal creation timelines

### Metadata Forensics
- File creation vs modification gaps
- Suspiciously short completion times
- Missing author information
- Version count anomalies

## 📡 API Endpoints

### File Upload
- `POST /api/analysis/upload-single` - Single file analysis
- `POST /api/analysis/upload-bulk` - Bulk ZIP processing

### Analysis & Comparison
- `POST /api/analysis/compare` - Compare two solutions
- `GET /api/analysis/dashboard-summary` - Dashboard statistics

### Student Management
- `GET /api/analysis/student/{studentId}` - Student profile
- `GET /api/analysis/student/{studentId}/assignments` - Assignment history

## 💾 Database Schema

### Core Models
- **Student** - Student profiles
- **Submission** - File uploads
- **AnalysisResult** - Analysis reports
- **RiskIndicator** - Detected risks
- **SimilarityMatch** - Cross-submission matches
- **ForensicAnalysis** - Metadata analysis

## 🎨 UI/UX Features

### Beautiful Design
- Modern gradient interface
- Responsive grid layouts
- Interactive charts and visualizations
- Color-coded risk levels
- Smooth animations and transitions

### User-Friendly Navigation
- Intuitive dashboard
- Quick file upload
- Student search and filtering
- Real-time feedback
- Export capabilities

## 🛡️ Security Features

- Entity Framework Core protection
- Input validation on all endpoints
- CORS configuration for frontend
- Secure file handling
- SQL injection prevention

## 📈 Risk Assessment

### Risk Levels
- **Low** (0-29%) - Likely original work
- **Medium** (30-59%) - Minor similarities
- **High** (60-79%) - Significant concerns
- **Critical** (80-100%) - Likely plagiarism

### Risk Indicators
- `COPIED` - Direct copy detection
- `GPT_GENERATED` - AI content indicators
- `MODIFIED` - Slight modifications
- `SUSPICIOUS` - Unusual patterns
- `METADATA_ANOMALY` - Forensic concerns

## 🔧 Configuration

### Backend (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FlowgorithmAnalyzerDb;Trusted_Connection=true;"
  }
}
```

### Frontend (.env)
```
REACT_APP_API_URL=http://localhost:5000
```

## 📚 File Format Support

### Input Files
- `.fgl` - Flowgorithm files (XML format)
- `.zip` - Archives containing multiple .fgl files

### Naming Convention for Bulk Upload
```
StudentID_StudentName_filename.fgl
Example: STU001_JohnDoe_assignment1.fgl
```

## 🧪 Testing

### Test Upload File
Create a simple Flowgorithm file with:
- Start/End terminals
- Process boxes
- Decision diamonds
- Loop structures
- Variables

## 📝 Database Migrations

Create new migration:
```bash
dotnet ef migrations add MigrationName
```

Apply migrations:
```bash
dotnet ef database update
```

## 🐛 Troubleshooting

### Database Connection Issues
- Verify LocalDB is running
- Check connection string in appsettings.json
- Run `dotnet ef database update`

### API Not Responding
- Check CORS configuration
- Verify API is running on correct port
- Check firewall settings

### Frontend Build Issues
- Clear node_modules: `rm -r node_modules`
- Reinstall: `npm install`
- Clear cache: `npm cache clean --force`

## 📖 Documentation

- [Backend Setup Guide](Backend/README.md)
- [Frontend Setup Guide](Frontend/README.md)
- [API Documentation](Backend/README.md#-api-endpoints)
- [Database Schema](docs/DATABASE.md)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 License

This project is licensed under the MIT License.

## 👨‍💼 Author

**Flowgorithm Analyzer Pro**
- Built with ❤️ for Academic Integrity

## 🎓 Educational Use

This tool is designed for educators and academic administrators to:
- Monitor student progress
- Detect academic dishonesty
- Provide fair assessment
- Maintain academic integrity

## 📞 Support

For issues, questions, or feature requests, please open an issue in the repository.

---

**Version**: 1.0.0  
**Last Updated**: May 2026  
**Status**: Production Ready ✅
