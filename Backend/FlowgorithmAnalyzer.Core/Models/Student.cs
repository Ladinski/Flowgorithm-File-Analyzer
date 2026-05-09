namespace FlowgorithmAnalyzer.Core.Models;

public class Student
{
    public int Id { get; set; }
    public required string StudentId { get; set; }
    public required string Name { get; set; }
    public string? Email { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public List<Submission> Submissions { get; set; } = [];
    public List<AnalysisResult> AnalysisResults { get; set; } = [];
}
