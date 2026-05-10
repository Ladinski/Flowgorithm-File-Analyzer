namespace FlowgorithmAnalyzer.Core.Services;

public interface IPlagiarismDetectionService
{
    PlagiarismAnalysis AnalyzeSolution(FlowgorithmData solution);
    double CompareTwoSolutions(FlowgorithmData solution1, FlowgorithmData solution2);
    List<string> ExtractPatterns(FlowgorithmData solution);
}

public class PlagiarismDetectionService : IPlagiarismDetectionService
{
    public PlagiarismAnalysis AnalyzeSolution(FlowgorithmData solution)
    {
        var analysis = new PlagiarismAnalysis
        {
            FileName = solution.FileName,
            ElementCount = solution.ElementCount,
            ComplexityScore = solution.ComplexityScore,
            RiskIndicators = []
        };

        // Check for GPT-like patterns
        CheckForGPTPatterns(solution, analysis);

        // Check for copy-paste indicators
        CheckForCopyPasteIndicators(solution, analysis);

        // Check for metadata anomalies
        CheckMetadataAnomalies(solution, analysis);

        // Calculate overall plagiarism score
        analysis.PlagiarismScore = CalculatePlagiarismScore(analysis.RiskIndicators);

        return analysis;
    }

    public double CompareTwoSolutions(FlowgorithmData solution1, FlowgorithmData solution2)
    {
        // Hash comparison
        var hashSimilarity = CompareHashes(solution1.InstructionHashes, solution2.InstructionHashes);

        // Structure comparison
        var structureSimilarity = CompareStructure(solution1, solution2);

        // Element flow similarity
        var flowSimilarity = CompareElementFlow(solution1.Elements, solution2.Elements);

        // Weighted average
        var overallSimilarity = (hashSimilarity * 0.4) + (structureSimilarity * 0.35) + (flowSimilarity * 0.25);

        return overallSimilarity * 100; // Return as percentage
    }

    public List<string> ExtractPatterns(FlowgorithmData solution)
    {
        var patterns = new List<string>();

        // Extract element sequence pattern
        var elementSequence = string.Join("->", solution.Elements.Select(e => e.Type));
        patterns.Add($"ELEMENT_SEQ:{elementSequence}");

        // Extract variable usage pattern
        var varPattern = string.Join(",", solution.Variables.Select(v => v.Type));
        patterns.Add($"VAR_PATTERN:{varPattern}");

        // Extract logic pattern
        var logicPattern = ExtractLogicPattern(solution.Elements);
        patterns.Add($"LOGIC_PATTERN:{logicPattern}");

        return patterns;
    }

    private void CheckForGPTPatterns(FlowgorithmData solution, PlagiarismAnalysis analysis)
    {
        // Check for overly generic variable names only when there are enough variables
        // for the pattern to be meaningful. Small beginner programs often use names
        // like i, n, sum, count, or num, and that should not be treated as GPT evidence.
        var genericVarCount = solution.Variables.Count(v => IsSuspiciouslyGenericVariableName(v.Name));
        
        if (solution.Variables.Count >= 4 && genericVarCount > solution.Variables.Count * 0.6)
        {
            analysis.RiskIndicators.Add(new RiskIndicator
            {
                Type = "GPT_GENERATED",
                Severity = 2,
                Description = "Excessive use of generic variable names",
                Evidence = [$"Generic variables: {genericVarCount}/{solution.Variables.Count}"]
            });
        }

        // Check for overly complex comments/documentation
        var complexComments = solution.Elements.Count(e => 
            e.Content.Length > 80 && (e.Content.Contains("however") || e.Content.Contains("therefore") || e.Content.Contains("algorithm")));
        
        if (complexComments > solution.Elements.Count * 0.3)
        {
            analysis.RiskIndicators.Add(new RiskIndicator
            {
                Type = "GPT_GENERATED",
                Severity = 2,
                Description = "Unusually sophisticated documentation style",
                Evidence = [$"Complex comments found: {complexComments}"]
            });
        }
    }

    private void CheckForCopyPasteIndicators(FlowgorithmData solution, PlagiarismAnalysis analysis)
    {
        // Check for identical variable declarations patterns
        var identicalVars = solution.Variables
            .GroupBy(v => v.Name)
            .Where(g => g.Count() > 1)
            .ToList();

        if (identicalVars.Count > 0)
        {
            analysis.RiskIndicators.Add(new RiskIndicator
            {
                Type = "DUPLICATE_DECLARATION",
                Severity = 2,
                Description = "Same variable declared more than once",
                Evidence = identicalVars.Select(g => $"{g.Key} (x{g.Count()})").ToList()
            });
        }

        var repeatedPatterns = FindRepeatedElementPatterns(solution.Elements);
        var reviewablePatterns = repeatedPatterns
            .Where(p => !p.IsInputOnly)
            .ToList();
        var repeatedElementCount = reviewablePatterns.Sum(p => p.WindowSize * (p.Count - 1));

        if (reviewablePatterns.Count > 0 &&
            (reviewablePatterns.Count >= 2 || repeatedElementCount >= solution.Elements.Count * 0.35))
        {
            analysis.RiskIndicators.Add(new RiskIndicator
            {
                Type = "REPETITION_PATTERN",
                Severity = 2,
                Description = "Repeated identical non-input flowchart blocks",
                Evidence = reviewablePatterns
                    .Take(5)
                    .Select(p => $"{p.Sequence} repeated {p.Count} times")
                    .ToList()
            });
        }
    }

    private void CheckMetadataAnomalies(FlowgorithmData solution, PlagiarismAnalysis analysis)
    {
        var indicators = new List<RiskIndicator>();
        var hasCreatedDate = solution.CreatedDate != default;
        var hasModifiedDate = solution.ModifiedDate != default;
        var hasBothDates = hasCreatedDate && hasModifiedDate;

        if (hasBothDates && solution.ModifiedDate < solution.CreatedDate)
        {
            indicators.Add(new RiskIndicator
            {
                Type = "METADATA_ANOMALY",
                Severity = 3,
                Description = "File modification time is before creation time",
                Evidence = [$"Created: {FormatDate(solution.CreatedDate)}; modified: {FormatDate(solution.ModifiedDate)}"]
            });
        }

        if (hasBothDates && solution.ModifiedDate >= solution.CreatedDate)
        {
            var editingMinutes = (solution.ModifiedDate - solution.CreatedDate).TotalMinutes;
            var editingEvidence = $"{solution.ElementCount} elements, complexity {solution.ComplexityScore}, created {FormatDate(solution.CreatedDate)}, last saved {FormatDate(solution.ModifiedDate)}";

            if (editingMinutes < 1 && solution.ElementCount > 10)
            {
                indicators.Add(new RiskIndicator
                {
                    Type = "METADATA_ANOMALY",
                    Severity = 4,
                    Description = "Non-trivial file has almost no recorded editing time",
                    Evidence = [$"{editingEvidence}; recorded editing time: {FormatDuration(editingMinutes)}"]
                });
            }
            else if (editingMinutes < 10 && solution.ComplexityScore >= 60)
            {
                indicators.Add(new RiskIndicator
                {
                    Type = "METADATA_REVIEW",
                    Severity = 3,
                    Description = "Complex solution was created and last saved very quickly",
                    Evidence = [$"{editingEvidence}; recorded editing time: {FormatDuration(editingMinutes)}"]
                });
            }
            else if (editingMinutes < 15 && solution.ComplexityScore >= 40)
            {
                indicators.Add(new RiskIndicator
                {
                    Type = "METADATA_REVIEW",
                    Severity = 2,
                    Description = "Moderately complex solution has a short recorded editing window",
                    Evidence = [$"{editingEvidence}; recorded editing time: {FormatDuration(editingMinutes)}"]
                });
            }
            else if (editingMinutes < 20 && solution.ComplexityScore >= 90)
            {
                indicators.Add(new RiskIndicator
                {
                    Type = "METADATA_REVIEW",
                    Severity = 2,
                    Description = "Very complex solution has a short recorded editing window",
                    Evidence = [$"{editingEvidence}; recorded editing time: {FormatDuration(editingMinutes)}"]
                });
            }

            if (solution.ModifiedDate == solution.CreatedDate && solution.ElementCount > 5)
            {
                indicators.Add(new RiskIndicator
                {
                    Type = "METADATA_REVIEW",
                    Severity = 2,
                    Description = "File shows no time gap between creation and last save",
                    Evidence = [$"{editingEvidence}; this can happen when a finished file is copied or exported"]
                });
            }
        }

        if (string.IsNullOrEmpty(solution.Author) && solution.ElementCount > 10)
        {
            indicators.Add(new RiskIndicator
            {
                Type = "METADATA_REVIEW",
                Severity = 1,
                Description = "Missing author information in file metadata",
                Evidence = ["The Flowgorithm metadata does not identify the file author"]
            });
        }

        if (!hasCreatedDate && !hasModifiedDate)
        {
            indicators.Add(new RiskIndicator
            {
                Type = "METADATA_REVIEW",
                Severity = 1,
                Description = "File is missing timestamp metadata",
                Evidence = ["The file does not include created or modified timestamp metadata"]
            });
        }

        analysis.RiskIndicators.AddRange(indicators);
    }

    private double CompareHashes(List<string> hashes1, List<string> hashes2)
    {
        if (hashes1.Count == 0 || hashes2.Count == 0)
            return 0;

        var matches = hashes1.Intersect(hashes2).Count();
        return (double)matches / Math.Max(hashes1.Count, hashes2.Count);
    }

    private double CompareStructure(FlowgorithmData solution1, FlowgorithmData solution2)
    {
        var seq1 = string.Join("->", solution1.Elements.Select(e => e.Type));
        var seq2 = string.Join("->", solution2.Elements.Select(e => e.Type));

        return CalculateSimilarity(seq1, seq2);
    }

    private double CompareElementFlow(List<FlowchartElement> elements1, List<FlowchartElement> elements2)
    {
        if (elements1.Count == 0 || elements2.Count == 0)
            return 0;

        var matches = 0;
        var minLength = Math.Min(elements1.Count, elements2.Count);

        for (int i = 0; i < minLength; i++)
        {
            if (elements1[i].Type == elements2[i].Type)
                matches++;
        }

        return (double)matches / Math.Max(elements1.Count, elements2.Count);
    }

    private double CalculateSimilarity(string str1, string str2)
    {
        if (str1 == str2)
            return 1.0;

        var distance = LevenshteinDistance(str1, str2);
        var maxLength = Math.Max(str1.Length, str2.Length);

        return 1.0 - ((double)distance / maxLength);
    }

    private int LevenshteinDistance(string s1, string s2)
    {
        var length1 = s1.Length;
        var length2 = s2.Length;
        var d = new int[length1 + 1, length2 + 1];

        for (int i = 0; i <= length1; i++)
            d[i, 0] = i;

        for (int j = 0; j <= length2; j++)
            d[0, j] = j;

        for (int i = 1; i <= length1; i++)
        {
            for (int j = 1; j <= length2; j++)
            {
                var cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }

        return d[length1, length2];
    }

    private double CalculatePlagiarismScore(List<RiskIndicator> indicators)
    {
        if (indicators.Count == 0)
            return 0;

        var totalScore = indicators.Sum(r => r.Severity * 20);
        return Math.Min(totalScore, 100);
    }

    private static bool IsSuspiciouslyGenericVariableName(string variableName)
    {
        var name = variableName.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var commonBeginnerNames = new HashSet<string>
        {
            "i", "j", "k", "n", "x", "y", "a", "b",
            "num", "number", "sum", "total", "count", "counter",
            "min", "max", "avg", "average", "result"
        };

        if (commonBeginnerNames.Contains(name))
            return false;

        return name.StartsWith("var")
            || name.StartsWith("temp")
            || name.StartsWith("tmp")
            || name.StartsWith("data")
            || name.StartsWith("value");
    }

    private List<RepeatedElementPattern> FindRepeatedElementPatterns(List<FlowchartElement> elements)
    {
        var repeatedPatterns = new List<RepeatedElementPattern>();

        if (elements.Count < 6)
            return repeatedPatterns;

        for (var windowSize = 3; windowSize <= Math.Min(5, elements.Count / 2); windowSize++)
        {
            var windows = new Dictionary<string, RepeatedElementPattern>();

            for (var i = 0; i <= elements.Count - windowSize; i++)
            {
                var window = elements.Skip(i).Take(windowSize).ToList();
                var key = string.Join("||", window.Select(GetComparableElementText));

                if (!windows.TryGetValue(key, out var pattern))
                {
                    pattern = new RepeatedElementPattern
                    {
                        ComparableSequence = key,
                        Sequence = string.Join(" -> ", window.Select(GetReadableElementText)),
                        IsInputOnly = window.All(e => e.Type.Equals("input", StringComparison.OrdinalIgnoreCase)),
                        WindowSize = windowSize,
                        Count = 0
                    };
                    windows[key] = pattern;
                }

                pattern.Count++;
            }

            repeatedPatterns.AddRange(windows.Values.Where(p => p.Count > 1));
        }

        return repeatedPatterns
            .Where(pattern => !repeatedPatterns.Any(other =>
                other.WindowSize > pattern.WindowSize &&
                other.Count >= pattern.Count &&
                other.ComparableSequence.Contains(pattern.ComparableSequence, StringComparison.Ordinal)))
            .OrderByDescending(p => p.WindowSize * p.Count)
            .ThenByDescending(p => p.Count)
            .ToList();
    }

    private static string GetComparableElementText(FlowchartElement element)
    {
        return $"{element.Type}:{NormalizeElementContent(element.Content)}";
    }

    private static string GetReadableElementText(FlowchartElement element)
    {
        var content = NormalizeElementContent(element.Content);
        if (string.IsNullOrWhiteSpace(content))
            return element.Type;

        return $"{element.Type} `{TrimForEvidence(content)}`";
    }

    private static string NormalizeElementContent(string content)
    {
        return string.Join(" ", content.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();
    }

    private static string TrimForEvidence(string content)
    {
        const int maxLength = 45;
        return content.Length <= maxLength ? content : $"{content[..maxLength]}...";
    }

    private static string FormatDate(DateTime date)
    {
        return date.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private static string FormatDuration(double minutes)
    {
        if (minutes < 1)
            return "less than 1 minute";

        return $"{minutes:F1} minutes";
    }

    private string ExtractLogicPattern(List<FlowchartElement> elements)
    {
        var decisions = elements.Count(e => e.Type == "decision");
        var loops = elements.Count(e => e.Type == "loop");
        var processes = elements.Count(e => e.Type == "process");

        return $"D{decisions}L{loops}P{processes}";
    }
}

public class PlagiarismAnalysis
{
    public required string FileName { get; set; }
    public int ElementCount { get; set; }
    public int ComplexityScore { get; set; }
    public List<RiskIndicator> RiskIndicators { get; set; } = [];
    public double PlagiarismScore { get; set; }
}

public class RiskIndicator
{
    public required string Type { get; set; }
    public int Severity { get; set; } // 1-5
    public required string Description { get; set; }
    public List<string> Evidence { get; set; } = [];
}

internal class RepeatedElementPattern
{
    public required string ComparableSequence { get; set; }
    public required string Sequence { get; set; }
    public bool IsInputOnly { get; set; }
    public int WindowSize { get; set; }
    public int Count { get; set; }
}
