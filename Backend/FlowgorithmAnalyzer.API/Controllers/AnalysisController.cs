using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlowgorithmAnalyzer.Core.DTOs;
using FlowgorithmAnalyzer.Core.Models;
using FlowgorithmAnalyzer.Core.Services;
using FlowgorithmAnalyzer.Infrastructure.Data;

namespace FlowgorithmAnalyzer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController : ControllerBase
{
    private readonly IFlowgorithmParser _parser;
    private readonly IPlagiarismDetectionService _plagiarismService;
    private readonly IFileHandlingService _fileService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(
        IFlowgorithmParser parser,
        IPlagiarismDetectionService plagiarismService,
        IFileHandlingService fileService,
        ApplicationDbContext context,
        ILogger<AnalysisController> logger)
    {
        _parser = parser;
        _plagiarismService = plagiarismService;
        _fileService = fileService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("upload-single")]
    public async Task<IActionResult> UploadSingleFile([FromForm] UploadFileDto dto)
    {
        try
        {
            if (dto.File.Length == 0)
                return BadRequest(new ApiResponse { Success = false, Message = "File is empty" });

            // Read file content
            using var ms = new MemoryStream();
            await dto.File.CopyToAsync(ms);
            var fileContent = ms.ToArray();

            // Get or create student
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == dto.StudentId);
            if (student == null)
            {
                student = new Student
                {
                    StudentId = dto.StudentId,
                    Name = dto.StudentName,
                    EnrollmentDate = DateTime.UtcNow
                };
                _context.Students.Add(student);
                await _context.SaveChangesAsync();
            }

            // Create submission
            var submission = new Submission
            {
                StudentId = student.Id,
                FileName = dto.File.FileName,
                FilePath = $"uploads/{dto.StudentId}/{dto.File.FileName}",
                AssignmentName = dto.AssignmentName,
                SubmissionDate = dto.SubmissionDate,
                UploadedAt = DateTime.UtcNow,
                FileContent = fileContent
            };
            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            // Parse and analyze
            var flowgData = _parser.ParseFlowgorithmFile(fileContent, dto.File.FileName);
            if (flowgData == null)
                return BadRequest(new ApiResponse { Success = false, Message = "Failed to parse Flowgorithm file" });

            var plagiarismAnalysis = _plagiarismService.AnalyzeSolution(flowgData);

            // Create analysis result
            var analysisResult = new AnalysisResult
            {
                SubmissionId = submission.Id,
                StudentId = student.Id,
                AssignmentName = dto.AssignmentName,
                TotalElements = flowgData.ElementCount,
                ComplexityScore = flowgData.ComplexityScore,
                PlagiarismScore = plagiarismAnalysis.PlagiarismScore,
                AnalyzedAt = DateTime.UtcNow,
                IsFlagged = plagiarismAnalysis.PlagiarismScore > 60,
                RiskLevel = DeterminRiskLevel(plagiarismAnalysis.PlagiarismScore),
                Submission = submission,
                Student = student
            };

            // Add risk indicators
            foreach (var risk in plagiarismAnalysis.RiskIndicators)
            {
                var riskIndicator = new FlowgorithmAnalyzer.Core.Models.RiskIndicator
                {
                    Type = risk.Type,
                    Description = risk.Description,
                    SeverityLevel = risk.Severity,
                    Evidence = string.Join("; ", risk.Evidence)
                };
                analysisResult.RiskIndicators.Add(riskIndicator);
            }

            analysisResult.ForensicAnalysis = CreateForensicAnalysis(flowgData);

            await AddCrossSubmissionFindingsAsync(analysisResult, flowgData, dto.AssignmentName, student.Id);

            _context.AnalysisResults.Add(analysisResult);
            await _context.SaveChangesAsync();

            var resultDto = MapToAnalysisResultDto(analysisResult, student);
            return Ok(new ApiResponse<AnalysisResultDto>
            {
                Success = true,
                Message = "File analyzed successfully",
                Data = resultDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading single file");
            return StatusCode(500, new ApiResponse { Success = false, Message = "Internal server error", Errors = [ex.Message] });
        }
    }

    [HttpPost("upload-bulk")]
    public async Task<IActionResult> UploadBulk([FromForm] BulkUploadDto dto)
    {
        try
        {
            var files = await _fileService.ExtractZipContentsAsync(
                await ReadFileContent(dto.ZipFile));

            if (files.Count == 0)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "ZIP file does not contain any Flowgorithm files"
                });
            }

            var results = new List<AnalysisResultDto>();

            foreach (var (fileName, content) in files)
            {
                var originalFileName = Path.GetFileName(fileName);
                var (studentId, studentName) = ParseStudentInfoFromFileName(originalFileName);

                // Similar logic to single upload
                var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
                if (student == null)
                {
                    student = new Student
                    {
                        StudentId = studentId,
                        Name = studentName,
                        EnrollmentDate = DateTime.UtcNow
                    };
                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();
                }

                var submission = new Submission
                {
                    StudentId = student.Id,
                    FileName = originalFileName,
                    FilePath = $"uploads/{studentId}/{fileName}",
                    AssignmentName = dto.AssignmentName,
                    SubmissionDate = DateTime.UtcNow,
                    UploadedAt = DateTime.UtcNow,
                    FileContent = content
                };
                _context.Submissions.Add(submission);
                await _context.SaveChangesAsync();

                var flowgData = _parser.ParseFlowgorithmFile(content, originalFileName);
                if (flowgData == null) continue;

                var plagiarismAnalysis = _plagiarismService.AnalyzeSolution(flowgData);

                var analysisResult = new AnalysisResult
                {
                    SubmissionId = submission.Id,
                    StudentId = student.Id,
                    AssignmentName = dto.AssignmentName,
                    TotalElements = flowgData.ElementCount,
                    ComplexityScore = flowgData.ComplexityScore,
                    PlagiarismScore = plagiarismAnalysis.PlagiarismScore,
                    AnalyzedAt = DateTime.UtcNow,
                    IsFlagged = plagiarismAnalysis.PlagiarismScore > 60,
                    RiskLevel = DeterminRiskLevel(plagiarismAnalysis.PlagiarismScore)
                };
                analysisResult.ForensicAnalysis = CreateForensicAnalysis(flowgData);

                foreach (var risk in plagiarismAnalysis.RiskIndicators)
                {
                    analysisResult.RiskIndicators.Add(new FlowgorithmAnalyzer.Core.Models.RiskIndicator
                    {
                        Type = risk.Type,
                        Description = risk.Description,
                        SeverityLevel = risk.Severity,
                        Evidence = string.Join("; ", risk.Evidence)
                    });
                }

                await AddCrossSubmissionFindingsAsync(analysisResult, flowgData, dto.AssignmentName, student.Id);

                _context.AnalysisResults.Add(analysisResult);
                await _context.SaveChangesAsync();

                results.Add(MapToAnalysisResultDto(analysisResult, student));
            }

            return Ok(new ApiResponse<List<AnalysisResultDto>>
            {
                Success = true,
                Message = $"Bulk upload completed: {results.Count} files analyzed",
                Data = results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading bulk files");
            return StatusCode(500, new ApiResponse { Success = false, Message = "Internal server error", Errors = [ex.Message] });
        }
    }

    [HttpPost("compare")]
    public IActionResult CompareSolutions(CompareSolutionsDto dto)
    {
        try
        {
            var result1 = _context.AnalysisResults
                .Include(ar => ar.Student)
                .Include(ar => ar.Submission)
                .FirstOrDefault(ar => ar.Id == dto.FirstAnalysisId);
            
            var result2 = _context.AnalysisResults
                .Include(ar => ar.Student)
                .Include(ar => ar.Submission)
                .FirstOrDefault(ar => ar.Id == dto.SecondAnalysisId);

            if (result1?.Submission?.FileContent == null || result2?.Submission?.FileContent == null)
                return NotFound(new ApiResponse { Success = false, Message = "Analysis results not found" });

            var data1 = _parser.ParseFlowgorithmFile(result1.Submission.FileContent, result1.Submission.FileName);
            var data2 = _parser.ParseFlowgorithmFile(result2.Submission.FileContent, result2.Submission.FileName);

            if (data1 == null || data2 == null)
                return BadRequest(new ApiResponse { Success = false, Message = "Failed to parse files" });

            var similarity = _plagiarismService.CompareTwoSolutions(data1, data2);

            var comparisonResult = new ComparisonResultDto
            {
                FirstStudentId = result1.Student?.StudentId ?? "Unknown",
                SecondStudentId = result2.Student?.StudentId ?? "Unknown",
                FirstFileName = result1.Submission.FileName,
                SecondFileName = result2.Submission.FileName,
                OverallSimilarity = similarity,
                IdenticalBlocksCount = CountIdenticalBlocks(data1, data2),
                SimilarBlocksCount = CountSimilarBlocks(data1, data2)
            };

            return Ok(new ApiResponse<ComparisonResultDto>
            {
                Success = true,
                Message = "Comparison completed",
                Data = comparisonResult
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing solutions");
            return StatusCode(500, new ApiResponse { Success = false, Message = "Internal server error", Errors = [ex.Message] });
        }
    }

    [HttpGet("results")]
    public async Task<IActionResult> GetAnalysisResults()
    {
        try
        {
            var results = await _context.AnalysisResults
                .Include(ar => ar.Student)
                .Include(ar => ar.Submission)
                .Include(ar => ar.RiskIndicators)
                .Include(ar => ar.SimilarityMatches)
                    .ThenInclude(sm => sm.MatchedAnalysisResult)
                        .ThenInclude(ar => ar!.Student)
                .Include(ar => ar.SimilarityMatches)
                    .ThenInclude(sm => sm.MatchedAnalysisResult)
                        .ThenInclude(ar => ar!.Submission)
                .Include(ar => ar.ForensicAnalysis)
                .OrderByDescending(ar => ar.AnalyzedAt)
                .ToListAsync();

            var resultDtos = results
                .Where(ar => ar.Student != null)
                .Select(ar => MapToAnalysisResultDto(ar, ar.Student!))
                .ToList();

            return Ok(new ApiResponse<List<AnalysisResultDto>>
            {
                Success = true,
                Message = "Analysis results retrieved",
                Data = resultDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching analysis results");
            return StatusCode(500, new ApiResponse { Success = false, Message = "Internal server error", Errors = [ex.Message] });
        }
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents()
    {
        try
        {
            var students = await _context.Students
                .Include(s => s.AnalysisResults)
                    .ThenInclude(ar => ar.RiskIndicators)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var studentDtos = students.Select(student =>
            {
                var averageScore = student.AnalysisResults.Any()
                    ? student.AnalysisResults.Average(ar => ar.PlagiarismScore)
                    : 0;

                return new StudentPerformanceDto
                {
                    StudentId = student.StudentId,
                    StudentName = student.Name,
                    TotalAssignments = student.AnalysisResults.Count,
                    FlaggedAssignments = student.AnalysisResults.Count(ar => ar.IsFlagged),
                    AveragePlagiarismScore = averageScore,
                    RiskLevel = DeterminRiskLevel(averageScore),
                    Assignments = student.AnalysisResults
                        .OrderByDescending(ar => ar.AnalyzedAt)
                        .Select(ar => new AssignmentPerformanceDto
                        {
                            AssignmentName = ar.AssignmentName,
                            SubmissionDate = ar.AnalyzedAt,
                            PlagiarismScore = ar.PlagiarismScore,
                            ComplexityScore = ar.ComplexityScore,
                            IsFlagged = ar.IsFlagged,
                            FlagReason = ar.RiskIndicators.FirstOrDefault()?.Description
                        })
                        .ToList()
                };
            }).ToList();

            return Ok(new ApiResponse<List<StudentPerformanceDto>>
            {
                Success = true,
                Message = "Students retrieved",
                Data = studentDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching students");
            return StatusCode(500, new ApiResponse { Success = false, Message = "Internal server error", Errors = [ex.Message] });
        }
    }

    [HttpGet("student/{studentId}")]
    public async Task<IActionResult> GetStudentPerformance(string studentId)
    {
        try
        {
            var student = await _context.Students
                .Include(s => s.AnalysisResults)
                    .ThenInclude(ar => ar.RiskIndicators)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
                return NotFound();

            var performanceDto = new StudentPerformanceDto
            {
                StudentId = student.StudentId,
                StudentName = student.Name,
                TotalAssignments = student.AnalysisResults.Count,
                FlaggedAssignments = student.AnalysisResults.Count(ar => ar.IsFlagged),
                AveragePlagiarismScore = student.AnalysisResults.Any() ? student.AnalysisResults.Average(ar => ar.PlagiarismScore) : 0,
                Assignments = student.AnalysisResults.Select(ar => new AssignmentPerformanceDto
                {
                    AssignmentName = ar.AssignmentName,
                    SubmissionDate = ar.AnalyzedAt,
                    PlagiarismScore = ar.PlagiarismScore,
                    ComplexityScore = ar.ComplexityScore,
                    IsFlagged = ar.IsFlagged,
                    FlagReason = ar.RiskIndicators.FirstOrDefault()?.Description
                }).ToList()
            };

            // Determine risk level
            performanceDto.RiskLevel = DeterminRiskLevel(performanceDto.AveragePlagiarismScore);

            return Ok(new ApiResponse<StudentPerformanceDto>
            {
                Success = true,
                Message = "Student performance retrieved",
                Data = performanceDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching student performance");
            return StatusCode(500, new ApiResponse { Success = false, Message = "Internal server error", Errors = [ex.Message] });
        }
    }

    [HttpGet("dashboard-summary")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        try
        {
            var submissions = await _context.AnalysisResults
                .Include(ar => ar.Student)
                .Include(ar => ar.RiskIndicators)
                .ToListAsync();
            var flagged = submissions.Where(s => s.IsFlagged).ToList();

            var summary = new DashboardSummaryDto
            {
                TotalSubmissions = submissions.Count,
                AnalyzedSubmissions = submissions.Count,
                FlaggedSubmissions = flagged.Count,
                AveragePlagiarismScore = submissions.Any() ? submissions.Average(s => s.PlagiarismScore) : 0,
                HighRiskCases = flagged
                    .OrderByDescending(s => s.PlagiarismScore)
                    .Take(5)
                    .Select(s => new HighRiskCaseDto
                    {
                        StudentId = s.Student?.StudentId ?? "Unknown",
                        StudentName = s.Student?.Name ?? "Unknown",
                        AssignmentName = s.AssignmentName,
                        PlagiarismScore = s.PlagiarismScore,
                        PrimaryRiskFactor = s.RiskIndicators.FirstOrDefault()?.Type ?? "Unknown"
                    }).ToList()
            };

            // Risk distribution
            summary.RiskLevelDistribution = new Dictionary<string, int>
            {
                { "Low", submissions.Count(s => s.PlagiarismScore < 30) },
                { "Medium", submissions.Count(s => s.PlagiarismScore >= 30 && s.PlagiarismScore < 60) },
                { "High", submissions.Count(s => s.PlagiarismScore >= 60 && s.PlagiarismScore < 80) },
                { "Critical", submissions.Count(s => s.PlagiarismScore >= 80) }
            };

            return Ok(new ApiResponse<DashboardSummaryDto>
            {
                Success = true,
                Message = "Dashboard summary retrieved",
                Data = summary
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard summary");
            return StatusCode(500, new ApiResponse { Success = false, Message = "Internal server error", Errors = [ex.Message] });
        }
    }

    private AnalysisResultDto MapToAnalysisResultDto(AnalysisResult result, Student student)
    {
        var dto = new AnalysisResultDto
        {
            Id = result.Id,
            StudentId = student.StudentId,
            StudentName = student.Name,
            AssignmentName = result.AssignmentName,
            FileName = result.Submission?.FileName ?? "Unknown",
            SubmissionDate = result.Submission?.SubmissionDate ?? DateTime.UtcNow,
            TotalFlowchartElements = result.TotalElements,
            ComplexityScore = result.ComplexityScore,
            PlagiarismScore = result.PlagiarismScore,
            AnalyzedAt = result.AnalyzedAt,
            RiskIndicators = result.RiskIndicators.Select(ri => new RiskIndicatorDto
            {
                Type = ri.Type,
                Description = ri.Description,
                SeverityLevel = ri.SeverityLevel,
                Evidence = ri.Evidence?.Split(';').Select(e => e.Trim()).ToList() ?? []
            }).ToList(),
            SimilarityMatches = result.SimilarityMatches.Select(sm => new SimilarityMatchDto
            {
                MatchedStudentId = sm.MatchedAnalysisResult?.Student?.StudentId ?? "Unknown",
                MatchedStudentName = sm.MatchedAnalysisResult?.Student?.Name ?? "Unknown",
                MatchedFileName = sm.MatchedAnalysisResult?.Submission?.FileName ?? "Unknown",
                SimilarityPercentage = sm.SimilarityPercentage,
                MatchedElementsCount = sm.MatchedElementsCount,
                MatchedPatterns = sm.MatchedPatterns?.Split(';').Select(p => p.Trim()).ToList() ?? []
            }).ToList()
        };

        if (result.ForensicAnalysis != null)
        {
            dto.ForensicAnalysis = new ForensicAnalysisDto
            {
                FileCreatedDate = result.ForensicAnalysis.FileCreatedDate,
                FileModifiedDate = result.ForensicAnalysis.FileModifiedDate,
                Author = result.ForensicAnalysis.Author,
                SuspiciousMetadata = result.ForensicAnalysis.SuspiciousMetadata,
                MetadataAnomalies = result.ForensicAnalysis.MetadataAnomalies,
                AnomalyDetails = result.ForensicAnalysis.AnomalyDetails?
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList() ?? []
            };
        }

        return dto;
    }

    private string DeterminRiskLevel(double plagiarismScore)
    {
        return plagiarismScore switch
        {
            < 30 => "Low",
            < 60 => "Medium",
            < 80 => "High",
            _ => "Critical"
        };
    }

    private static (string studentId, string studentName) ParseStudentInfoFromFileName(string fileName)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var parts = nameWithoutExtension
            .Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var studentId = parts.Length > 0 ? NormalizeStudentId(parts[0]) : "Unknown";
        var studentName = parts.Length switch
        {
            >= 3 => $"{parts[1]} {parts[2]}",
            2 => parts[1],
            _ => "Unknown"
        };

        return (studentId, studentName);
    }

    private static string NormalizeStudentId(string rawStudentId)
    {
        var trimmed = rawStudentId.Trim();
        if (trimmed.StartsWith("ID", StringComparison.OrdinalIgnoreCase) && trimmed.Length > 2)
        {
            var idWithoutPrefix = trimmed[2..];
            if (idWithoutPrefix.All(char.IsDigit))
                return idWithoutPrefix;
        }

        return trimmed;
    }

    private static ForensicAnalysis? CreateForensicAnalysis(FlowgorithmData flowgData)
    {
        var hasCreatedDate = flowgData.CreatedDate != default;
        var hasModifiedDate = flowgData.ModifiedDate != default;

        if (!hasCreatedDate && !hasModifiedDate && string.IsNullOrWhiteSpace(flowgData.Author))
            return null;

        var findings = GetMetadataFindings(flowgData);
        return new ForensicAnalysis
        {
            FileCreatedDate = flowgData.CreatedDate,
            FileModifiedDate = flowgData.ModifiedDate,
            Author = flowgData.Author,
            SuspiciousMetadata = findings.Count > 0,
            MetadataAnomalies = findings.Count,
            AnomalyDetails = string.Join("; ", findings)
        };
    }

    private static List<string> GetMetadataFindings(FlowgorithmData flowgData)
    {
        var findings = new List<string>();
        var hasCreatedDate = flowgData.CreatedDate != default;
        var hasModifiedDate = flowgData.ModifiedDate != default;

        if (!hasCreatedDate && !hasModifiedDate)
        {
            findings.Add("Missing created and modified timestamp metadata");
        }

        if (hasCreatedDate && hasModifiedDate)
        {
            if (flowgData.ModifiedDate < flowgData.CreatedDate)
            {
                findings.Add("Modified timestamp is before created timestamp");
            }
            else
            {
                var editingMinutes = (flowgData.ModifiedDate - flowgData.CreatedDate).TotalMinutes;
                if (editingMinutes < 1 && flowgData.ElementCount > 10)
                {
                    findings.Add($"Almost no recorded editing time for {flowgData.ElementCount} elements");
                }
                else if (editingMinutes < 10 && flowgData.ComplexityScore >= 60)
                {
                    findings.Add($"{flowgData.ComplexityScore} complexity recorded in {editingMinutes:F1} minutes");
                }
                else if (editingMinutes < 15 && flowgData.ComplexityScore >= 40)
                {
                    findings.Add($"{flowgData.ComplexityScore} complexity has a short {editingMinutes:F1} minute editing window");
                }
                else if (editingMinutes < 20 && flowgData.ComplexityScore >= 90)
                {
                    findings.Add($"{flowgData.ComplexityScore} complexity has a short {editingMinutes:F1} minute editing window");
                }
            }
        }

        if (string.IsNullOrWhiteSpace(flowgData.Author) && flowgData.ElementCount > 10)
        {
            findings.Add("Missing author metadata");
        }

        return findings;
    }

    private async Task AddCrossSubmissionFindingsAsync(
        AnalysisResult analysisResult,
        FlowgorithmData flowgData,
        string assignmentName,
        int currentStudentId)
    {
        var existingResults = await _context.AnalysisResults
            .Include(ar => ar.Student)
            .Include(ar => ar.Submission)
            .Include(ar => ar.RiskIndicators)
            .Include(ar => ar.SimilarityMatches)
            .Where(ar => ar.AssignmentName == assignmentName && ar.StudentId != currentStudentId)
            .ToListAsync();

        foreach (var existingResult in existingResults)
        {
            if (existingResult.Submission?.FileContent == null)
                continue;

            var existingData = _parser.ParseFlowgorithmFile(
                existingResult.Submission.FileContent,
                existingResult.Submission.FileName);

            if (existingData == null)
                continue;

            var similarity = _plagiarismService.CompareTwoSolutions(flowgData, existingData);
            if (similarity < 70)
                continue;

            var matchedElements = CountIdenticalBlocks(flowgData, existingData);
            var patterns = _plagiarismService.ExtractPatterns(flowgData)
                .Intersect(_plagiarismService.ExtractPatterns(existingData))
                .ToList();

            analysisResult.SimilarityMatches.Add(new SimilarityMatch
            {
                MatchedAnalysisResultId = existingResult.Id,
                SimilarityPercentage = similarity,
                MatchedElementsCount = matchedElements,
                MatchedPatterns = string.Join("; ", patterns)
            });

            existingResult.SimilarityMatches.Add(new SimilarityMatch
            {
                MatchedAnalysisResult = analysisResult,
                SimilarityPercentage = similarity,
                MatchedElementsCount = matchedElements,
                MatchedPatterns = string.Join("; ", patterns)
            });

            var severity = similarity >= 90 ? 5 : 3;
            var riskType = similarity >= 90 ? "HIGH_SIMILARITY_MATCH" : "SIMILARITY_MATCH";
            var matchedStudent = existingResult.Student?.Name ?? existingResult.Submission.FileName;
            var description = similarity >= 90
                ? "Submission is highly similar to another student's file"
                : "Submission has notable similarity to another student's file";
            var evidence = $"{similarity:F1}% similar to {matchedStudent}; Identical block positions: {matchedElements}";

            AddOrUpdateSimilarityRisk(analysisResult, riskType, severity, description, evidence);
            AddOrUpdateSimilarityRisk(existingResult, riskType, severity, description, $"{similarity:F1}% similar to current upload; Identical block positions: {matchedElements}");

            analysisResult.PlagiarismScore = Math.Max(analysisResult.PlagiarismScore, similarity);
            existingResult.PlagiarismScore = Math.Max(existingResult.PlagiarismScore, similarity);
            analysisResult.IsFlagged = analysisResult.PlagiarismScore >= 60;
            existingResult.IsFlagged = existingResult.PlagiarismScore >= 60;
            analysisResult.RiskLevel = DeterminRiskLevel(analysisResult.PlagiarismScore);
            existingResult.RiskLevel = DeterminRiskLevel(existingResult.PlagiarismScore);
        }
    }

    private static void AddOrUpdateSimilarityRisk(
        AnalysisResult result,
        string type,
        int severity,
        string description,
        string evidence)
    {
        var existingRisk = result.RiskIndicators.FirstOrDefault(ri => ri.Type == type && ri.Description == description);
        if (existingRisk == null)
        {
            result.RiskIndicators.Add(new FlowgorithmAnalyzer.Core.Models.RiskIndicator
            {
                Type = type,
                SeverityLevel = severity,
                Description = description,
                Evidence = evidence
            });
            return;
        }

        existingRisk.SeverityLevel = Math.Max(existingRisk.SeverityLevel, severity);
        var existingEvidence = existingRisk.Evidence ?? "";
        if (!existingEvidence.Contains(evidence, StringComparison.OrdinalIgnoreCase))
        {
            existingRisk.Evidence = string.IsNullOrWhiteSpace(existingEvidence)
                ? evidence
                : $"{existingEvidence}; {evidence}";
        }
    }

    private int CountIdenticalBlocks(FlowgorithmData data1, FlowgorithmData data2)
    {
        return data1.Elements.Where((e, i) => i < data2.Elements.Count && e.Type == data2.Elements[i].Type).Count();
    }

    private int CountSimilarBlocks(FlowgorithmData data1, FlowgorithmData data2)
    {
        var count = 0;
        foreach (var e1 in data1.Elements)
        {
            if (data2.Elements.Any(e2 => e2.Type == e1.Type && e2.Content.Length > 0))
                count++;
        }
        return count;
    }

    private async Task<byte[]> ReadFileContent(IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ms.ToArray();
    }
}

public class CompareSolutionsDto
{
    public int FirstAnalysisId { get; set; }
    public int SecondAnalysisId { get; set; }
}
