namespace FlowgorithmAnalyzer.Core.Models;

public class Submission
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public required string FileName { get; set; }
    public required string FilePath { get; set; }
    public required string AssignmentName { get; set; }
    public DateTime SubmissionDate { get; set; }
    public DateTime UploadedAt { get; set; }
    public byte[]? FileContent { get; set; }
    
    public Student? Student { get; set; }
    public List<AnalysisResult> AnalysisResults { get; set; } = [];
}
