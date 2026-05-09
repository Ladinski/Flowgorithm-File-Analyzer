namespace FlowgorithmAnalyzer.Core.Services;

public interface IPlagiarismDetectionService
{
    PlagiarismAnalysis AnalyzeSolution(FlowgorithmData solution);
    double CompareTwoSolutions(FlowgorithmData solution1, FlowgorithmData solution2);
    List<string> ExtractPatterns(FlowgorithmData solution);
}

public class PlagiarismDetectionService : IPlagiarismDetectionService
{
    private const double HashMatchThreshold = 0.7;
    private const double StructureSimilarityThreshold = 0.75;

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
                Type = "COPY_PASTE",
                Severity = 2,
                Description = "Duplicate variable declarations found",
                Evidence = identicalVars.Select(g => $"{g.Key} (x{g.Count()})").ToList()
            });
        }

        // Check for repeated element patterns
        var elementSequence = string.Join("->", solution.Elements.Select(e => e.Type));
        var repeatedPatterns = FindRepeatedPatterns(elementSequence, 3);

        if (repeatedPatterns.Count > solution.Elements.Count * 0.5)
        {
            analysis.RiskIndicators.Add(new RiskIndicator
            {
                Type = "COPY_PASTE",
                Severity = 3,
                Description = "Excessive repetition of identical code blocks",
                Evidence = repeatedPatterns.Take(5).ToList()
            });
        }
    }

    private void CheckMetadataAnomalies(FlowgorithmData solution, PlagiarismAnalysis analysis)
    {
        var anomalies = new List<string>();

        // Check creation vs modification time gap
        if (solution.ModifiedDate != default &&
            solution.CreatedDate != default &&
            solution.ModifiedDate == solution.CreatedDate)
        {
            anomalies.Add("No modification history - file never edited after creation");
        }

        // Check for suspiciously short creation time
        if (solution.ModifiedDate != default &&
            solution.CreatedDate != default &&
            (solution.ModifiedDate - solution.CreatedDate).TotalMinutes < 5 &&
            solution.ElementCount > 15)
        {
            anomalies.Add("Complex solution created and completed in under 5 minutes");
        }

        // Check author field
        if (string.IsNullOrEmpty(solution.Author) && solution.ElementCount > 10)
        {
            anomalies.Add("Missing author information in metadata");
        }

        if (anomalies.Count > 0)
        {
            analysis.RiskIndicators.Add(new RiskIndicator
            {
                Type = "METADATA_ANOMALY",
                Severity = 2,
                Description = "Suspicious metadata patterns detected",
                Evidence = anomalies
            });
        }
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

    private List<string> FindRepeatedPatterns(string sequence, int minLength)
    {
        var patterns = new List<string>();
        for (int i = 0; i < sequence.Length - minLength; i++)
        {
            var pattern = sequence.Substring(i, minLength);
            if (sequence.IndexOf(pattern) != sequence.LastIndexOf(pattern))
            {
                if (!patterns.Contains(pattern))
                    patterns.Add(pattern);
            }
        }
        return patterns;
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
