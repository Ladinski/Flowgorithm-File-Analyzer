namespace FlowgorithmAnalyzer.Core.DTOs;
using Microsoft.AspNetCore.Http;
public class UploadFileDto
{
    public required IFormFile File { get; set; }
    public required string StudentId { get; set; }
    public required string StudentName { get; set; }
    public required string AssignmentName { get; set; }
    public DateTime SubmissionDate { get; set; }
}

public class BulkUploadDto
{
    public required IFormFile ZipFile { get; set; }
    public required string AssignmentName { get; set; }
    public string? Semester { get; set; }
}
