namespace FlowgorithmAnalyzer.Core.Models;

public class AnalysisResult
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public int StudentId { get; set; }
    public required string AssignmentName { get; set; }
    public int TotalElements { get; set; }
    public int ComplexityScore { get; set; }
    public double PlagiarismScore { get; set; }
    public string RiskLevel { get; set; } = "Low";
    public DateTime AnalyzedAt { get; set; }
    public bool IsFlagged { get; set; }
    
    public Submission? Submission { get; set; }
    public Student? Student { get; set; }
    public List<RiskIndicator> RiskIndicators { get; set; } = [];
    public List<SimilarityMatch> SimilarityMatches { get; set; } = [];
    public ForensicAnalysis? ForensicAnalysis { get; set; }
}

public class RiskIndicator
{
    public int Id { get; set; }
    public int AnalysisResultId { get; set; }
    public required string Type { get; set; }
    public required string Description { get; set; }
    public int SeverityLevel { get; set; }
    public string? Evidence { get; set; }
    
    public AnalysisResult? AnalysisResult { get; set; }
}

public class SimilarityMatch
{
    public int Id { get; set; }
    public int AnalysisResultId { get; set; }
    public int MatchedAnalysisResultId { get; set; }
    public double SimilarityPercentage { get; set; }
    public int MatchedElementsCount { get; set; }
    public string? MatchedPatterns { get; set; }
    
    public AnalysisResult? AnalysisResult { get; set; }
    public AnalysisResult? MatchedAnalysisResult { get; set; }
}

public class ForensicAnalysis
{
    public int Id { get; set; }
    public int AnalysisResultId { get; set; }
    public DateTime FileCreatedDate { get; set; }
    public DateTime FileModifiedDate { get; set; }
    public int MetadataAnomalies { get; set; }
    public string? AnomalyDetails { get; set; }
    public bool SuspiciousMetadata { get; set; }
    public string? Author { get; set; }
    public int VersionCount { get; set; }
    
    public AnalysisResult? AnalysisResult { get; set; }
}
