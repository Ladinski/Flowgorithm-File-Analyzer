# API Documentation

## Endpoints

### File Upload

#### Upload Single File
**POST** `/api/analysis/upload-single`

Request:
```
Content-Type: multipart/form-data

- file: File (.fgl)
- studentId: string
- studentName: string
- assignmentName: string
- submissionDate: ISO8601 datetime
```

Response:
```json
{
  "success": true,
  "message": "File analyzed successfully",
  "data": {
    "id": 1,
    "studentId": "STU001",
    "studentName": "John Doe",
    "plagiarismScore": 25.5,
    "riskIndicators": []
  }
}
```

#### Upload Bulk (ZIP)
**POST** `/api/analysis/upload-bulk`

Request:
```
Content-Type: multipart/form-data

- zipFile: File (.zip)
- assignmentName: string
- semester: string (optional)
```

The ZIP may contain nested folders. Each Flowgorithm file is discovered recursively from the archive entries. Student data is parsed from file names in this format:

```
ID1234_John_Doe_solution.fprg
```

This creates or reuses student ID `ID1234` with student name `John Doe`.

Response: Array of analysis results

### Analysis

#### Compare Two Solutions
**POST** `/api/analysis/compare`

Request:
```json
{
  "firstAnalysisId": 1,
  "secondAnalysisId": 2
}
```

Response:
```json
{
  "success": true,
  "data": {
    "firstStudentId": "STU001",
    "secondStudentId": "STU002",
    "overallSimilarity": 85.5,
    "identicalBlocksCount": 12,
    "similarBlocksCount": 8
  }
}
```

### Student Management

#### Get Student Performance
**GET** `/api/analysis/student/{studentId}`

Response:
```json
{
  "success": true,
  "data": {
    "studentId": "STU001",
    "studentName": "John Doe",
    "totalAssignments": 5,
    "flaggedAssignments": 1,
    "averagePlagiarismScore": 32.4,
    "riskLevel": "Low",
    "assignments": []
  }
}
```

### Dashboard

#### Get Dashboard Summary
**GET** `/api/analysis/dashboard-summary`

Response:
```json
{
  "success": true,
  "data": {
    "totalSubmissions": 45,
    "analyzedSubmissions": 45,
    "flaggedSubmissions": 5,
    "averagePlagiarismScore": 28.6,
    "riskLevelDistribution": {
      "Low": 32,
      "Medium": 8,
      "High": 4,
      "Critical": 1
    },
    "highRiskCases": []
  }
}
```

## Error Responses

### 400 Bad Request
```json
{
  "success": false,
  "message": "Invalid input",
  "errors": ["File is empty"]
}
```

### 404 Not Found
```json
{
  "success": false,
  "message": "Resource not found"
}
```

### 500 Internal Server Error
```json
{
  "success": false,
  "message": "Internal server error",
  "errors": ["Detailed error message"]
}
```

## Status Codes

- `200` - Success
- `201` - Created
- `400` - Bad Request
- `404` - Not Found
- `500` - Internal Server Error

## Authentication

Currently, the API doesn't require authentication. For production, implement JWT or OAuth2.

## Rate Limiting

No rate limiting currently implemented. Consider adding for production.

## CORS

Configured to allow requests from `http://localhost:3000` and `http://localhost:5173`.
