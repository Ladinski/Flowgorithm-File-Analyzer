namespace FlowgorithmAnalyzer.Core.DTOs;

public class AnalysisResultDto
{
    public int Id { get; set; }
    public required string StudentId { get; set; }
    public required string StudentName { get; set; }
    public required string AssignmentName { get; set; }
    public required string FileName { get; set; }
    public DateTime SubmissionDate { get; set; }
    public int TotalFlowchartElements { get; set; }
    public int ComplexityScore { get; set; }
    public List<RiskIndicatorDto> RiskIndicators { get; set; } = [];
    public double PlagiarismScore { get; set; } // 0-100
    public List<SimilarityMatchDto> SimilarityMatches { get; set; } = [];
    public ForensicAnalysisDto? ForensicAnalysis { get; set; }
    public DateTime AnalyzedAt { get; set; }
}

public class RiskIndicatorDto
{
    public required string Type { get; set; } // "Copied", "GPTGenerated", "Modified", "Suspicious"
    public required string Description { get; set; }
    public int SeverityLevel { get; set; } // 1-5
    public List<string> Evidence { get; set; } = [];
}

public class SimilarityMatchDto
{
    public required string MatchedStudentId { get; set; }
    public required string MatchedStudentName { get; set; }
    public required string MatchedFileName { get; set; }
    public double SimilarityPercentage { get; set; }
    public int MatchedElementsCount { get; set; }
    public List<string> MatchedPatterns { get; set; } = [];
}

public class ForensicAnalysisDto
{
    public DateTime FileCreatedDate { get; set; }
    public DateTime FileModifiedDate { get; set; }
    public int MetadataAnomalies { get; set; }
    public List<string> AnomalyDetails { get; set; } = [];
    public bool SuspiciousMetadata { get; set; }
    public string? Author { get; set; }
    public int VersionCount { get; set; }
}

public class ComparisonResultDto
{
    public required string FirstStudentId { get; set; }
    public required string SecondStudentId { get; set; }
    public required string FirstFileName { get; set; }
    public required string SecondFileName { get; set; }
    public double OverallSimilarity { get; set; }
    public List<CodeBlockComparisonDto> BlockComparisons { get; set; } = [];
    public int IdenticalBlocksCount { get; set; }
    public int SimilarBlocksCount { get; set; }
}

public class CodeBlockComparisonDto
{
    public required string BlockType { get; set; }
    public double SimilarityScore { get; set; }
    public required string FirstBlockContent { get; set; }
    public required string SecondBlockContent { get; set; }
}

public class StudentPerformanceDto
{
    public required string StudentId { get; set; }
    public required string StudentName { get; set; }
    public List<AssignmentPerformanceDto> Assignments { get; set; } = [];
    public double AveragePlagiarismScore { get; set; }
    public int TotalAssignments { get; set; }
    public int FlaggedAssignments { get; set; }
    public string RiskLevel { get; set; } = "Low"; // Low, Medium, High, Critical
}

public class AssignmentPerformanceDto
{
    public required string AssignmentName { get; set; }
    public DateTime SubmissionDate { get; set; }
    public double PlagiarismScore { get; set; }
    public int ComplexityScore { get; set; }
    public bool IsFlagged { get; set; }
    public string? FlagReason { get; set; }
}

public class DashboardSummaryDto
{
    public int TotalSubmissions { get; set; }
    public int AnalyzedSubmissions { get; set; }
    public int FlaggedSubmissions { get; set; }
    public double AveragePlagiarismScore { get; set; }
    public List<HighRiskCaseDto> HighRiskCases { get; set; } = [];
    public Dictionary<string, int> RiskLevelDistribution { get; set; } = [];
}

public class HighRiskCaseDto
{
    public required string StudentId { get; set; }
    public required string StudentName { get; set; }
    public required string AssignmentName { get; set; }
    public double PlagiarismScore { get; set; }
    public required string PrimaryRiskFactor { get; set; }
}
