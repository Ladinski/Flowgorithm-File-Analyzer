# Database Schema

## Tables

### Students
```sql
CREATE TABLE Students (
    Id INT PRIMARY KEY IDENTITY,
    StudentId NVARCHAR(50) NOT NULL UNIQUE,
    Name NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255),
    EnrollmentDate DATETIME2 NOT NULL
);
```

### Submissions
```sql
CREATE TABLE Submissions (
    Id INT PRIMARY KEY IDENTITY,
    StudentId INT NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    AssignmentName NVARCHAR(255) NOT NULL,
    SubmissionDate DATETIME2 NOT NULL,
    UploadedAt DATETIME2 NOT NULL,
    FileContent VARBINARY(MAX),
    FOREIGN KEY (StudentId) REFERENCES Students(Id)
);
```

### AnalysisResults
```sql
CREATE TABLE AnalysisResults (
    Id INT PRIMARY KEY IDENTITY,
    SubmissionId INT NOT NULL,
    StudentId INT NOT NULL,
    AssignmentName NVARCHAR(255) NOT NULL,
    TotalElements INT NOT NULL,
    ComplexityScore INT NOT NULL,
    PlagiarismScore FLOAT NOT NULL,
    RiskLevel NVARCHAR(50) NOT NULL,
    AnalyzedAt DATETIME2 NOT NULL,
    IsFlagged BIT NOT NULL,
    FOREIGN KEY (SubmissionId) REFERENCES Submissions(Id),
    FOREIGN KEY (StudentId) REFERENCES Students(Id)
);
```

### RiskIndicators
```sql
CREATE TABLE RiskIndicators (
    Id INT PRIMARY KEY IDENTITY,
    AnalysisResultId INT NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    SeverityLevel INT NOT NULL,
    Evidence NVARCHAR(MAX),
    FOREIGN KEY (AnalysisResultId) REFERENCES AnalysisResults(Id) ON DELETE CASCADE
);
```

### SimilarityMatches
```sql
CREATE TABLE SimilarityMatches (
    Id INT PRIMARY KEY IDENTITY,
    AnalysisResultId INT NOT NULL,
    MatchedAnalysisResultId INT NOT NULL,
    SimilarityPercentage FLOAT NOT NULL,
    MatchedElementsCount INT NOT NULL,
    MatchedPatterns NVARCHAR(MAX),
    FOREIGN KEY (AnalysisResultId) REFERENCES AnalysisResults(Id) ON DELETE CASCADE,
    FOREIGN KEY (MatchedAnalysisResultId) REFERENCES AnalysisResults(Id)
);
```

### ForensicAnalyses
```sql
CREATE TABLE ForensicAnalyses (
    Id INT PRIMARY KEY IDENTITY,
    AnalysisResultId INT NOT NULL,
    FileCreatedDate DATETIME2 NOT NULL,
    FileModifiedDate DATETIME2 NOT NULL,
    MetadataAnomalies INT NOT NULL,
    AnomalyDetails NVARCHAR(MAX),
    SuspiciousMetadata BIT NOT NULL,
    Author NVARCHAR(255),
    VersionCount INT NOT NULL,
    FOREIGN KEY (AnalysisResultId) REFERENCES AnalysisResults(Id) ON DELETE CASCADE
);
```

## Indexes

```sql
CREATE INDEX idx_Student_Id ON Students(StudentId);
CREATE INDEX idx_Submission_StudentId ON Submissions(StudentId);
CREATE INDEX idx_Analysis_StudentId ON AnalysisResults(StudentId);
CREATE INDEX idx_Analysis_IsFlagged ON AnalysisResults(IsFlagged);
CREATE INDEX idx_Analysis_PlagiarismScore ON AnalysisResults(PlagiarismScore);
```

## Relationships

- **Students** (1) → (∞) **Submissions**
- **Students** (1) → (∞) **AnalysisResults**
- **Submissions** (1) → (∞) **AnalysisResults**
- **AnalysisResults** (1) → (∞) **RiskIndicators**
- **AnalysisResults** (1) → (∞) **SimilarityMatches**
- **AnalysisResults** (1) → (1) **ForensicAnalysis**
