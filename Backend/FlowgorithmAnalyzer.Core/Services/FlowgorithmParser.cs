using System.Xml;
using System.Xml.XPath;
using System.Text;
using System.Text.RegularExpressions;

namespace FlowgorithmAnalyzer.Core.Services;

public interface IFlowgorithmParser
{
    FlowgorithmData? ParseFlowgorithmFile(byte[] fileContent, string fileName);
    FlowgorithmData? ParseFlowgorithmFile(string filePath);
}

public class FlowgorithmParser : IFlowgorithmParser
{
    public FlowgorithmData? ParseFlowgorithmFile(byte[] fileContent, string fileName)
    {
        try
        {
            using var ms = new MemoryStream(fileContent);
            return ParseXmlContent(ms, fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing Flowgorithm file: {ex.Message}");
            return null;
        }
    }

    public FlowgorithmData? ParseFlowgorithmFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return null;

            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return ParseXmlContent(fs, Path.GetFileName(filePath));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing Flowgorithm file: {ex.Message}");
            return null;
        }
    }

    private FlowgorithmData ParseXmlContent(Stream content, string fileName)
    {
        var doc = new XmlDocument();
        doc.Load(content);

        var data = new FlowgorithmData { FileName = fileName };

        // Parse metadata
        var infoNode = doc.SelectSingleNode("//information");
        if (infoNode != null)
        {
            data.Author = infoNode.SelectSingleNode("author")?.InnerText;
            data.Title = infoNode.SelectSingleNode("title")?.InnerText;
            data.Description = infoNode.SelectSingleNode("description")?.InnerText;
            
            if (TryParseFlowgorithmDate(infoNode.SelectSingleNode("created")?.InnerText, out var created))
                data.CreatedDate = created;
            
            if (TryParseFlowgorithmDate(infoNode.SelectSingleNode("modified")?.InnerText, out var modified))
                data.ModifiedDate = modified;
        }
        else
        {
            ParseFlowgorithmAttributes(doc, data);
        }

        // Parse symbols (variables/data)
        var symbolNodes = doc.SelectNodes("//symbols/symbol");
        if (symbolNodes != null)
        {
            foreach (XmlNode node in symbolNodes)
            {
                data.Variables.Add(new Variable
                {
                    Name = node.SelectSingleNode("name")?.InnerText ?? "",
                    Type = node.SelectSingleNode("type")?.InnerText ?? "unknown",
                    Value = node.SelectSingleNode("value")?.InnerText
                });
            }
        }

        var declareNodes = doc.SelectNodes("//declare");
        if (declareNodes != null)
        {
            foreach (XmlNode node in declareNodes)
            {
                data.Variables.Add(new Variable
                {
                    Name = node.Attributes?["name"]?.Value ?? "",
                    Type = node.Attributes?["type"]?.Value ?? "unknown",
                    Value = node.Attributes?["value"]?.Value
                });
            }
        }

        // Parse flowchart elements
        var elementNodes = doc.SelectNodes("//flowchart/*[self::start or self::end or self::process or self::decision or self::loop or self::terminal or self::data]");
        var elementCount = 0;
        var instructions = new List<string>();

        if (elementNodes != null)
        {
            foreach (XmlNode node in elementNodes)
            {
                elementCount++;
                var element = new FlowchartElement
                {
                    Type = node.Name,
                    Content = node.SelectSingleNode("text")?.InnerText ?? ""
                };
            
                data.Elements.Add(element);
            
                if (!string.IsNullOrEmpty(element.Content))
                    instructions.Add(element.Content);
            }
        }

        if (data.Elements.Count == 0)
        {
            var bodyNodes = doc.SelectNodes("//function/body/*");
            if (bodyNodes != null)
            {
                foreach (XmlNode node in bodyNodes)
                {
                    AddFlowgorithmElements(node, data.Elements, instructions);
                }
            }
        }

        data.ElementCount = data.Elements.Count;
        data.ComplexityScore = CalculateComplexity(data.ElementCount, data.Variables.Count);
        data.InstructionHashes = HashInstructions(instructions);

        return data;
    }

    private static void ParseFlowgorithmAttributes(XmlDocument doc, FlowgorithmData data)
    {
        var attributeNodes = doc.SelectNodes("//attributes/attribute");
        if (attributeNodes == null)
            return;

        foreach (XmlNode node in attributeNodes)
        {
            var name = node.Attributes?["name"]?.Value;
            var value = node.Attributes?["value"]?.Value;

            switch (name?.ToLowerInvariant())
            {
                case "authors":
                case "author":
                    data.Author = value;
                    break;
                case "name":
                    data.Title = value;
                    break;
                case "about":
                    data.Description = value;
                    break;
                case "created":
                    if (TryParseFlowgorithmDate(value, out var created))
                        data.CreatedDate = created;
                    break;
                case "edited":
                    if (TryParseFlowgorithmDate(value, out var edited))
                        data.ModifiedDate = edited;
                    break;
                case "saved":
                case "modified":
                    if (TryParseFlowgorithmDate(value, out var modified))
                        data.ModifiedDate = modified;
                    break;
            }
        }
    }

    private static bool TryParseFlowgorithmDate(string? value, out DateTime date)
    {
        date = default;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var candidates = new List<string> { value.Trim() };

        if (TryDecodeBase64(value, out var decoded))
            candidates.Insert(0, decoded);

        foreach (var candidate in candidates)
        {
            if (DateTime.TryParse(candidate, out date))
                return true;

            var timestampMatch = Regex.Match(candidate, @"\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}");
            if (timestampMatch.Success && DateTime.TryParse(timestampMatch.Value, out date))
                return true;

            var parts = candidate.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (var i = 0; i < parts.Length - 1; i++)
            {
                var combined = $"{parts[i]} {parts[i + 1]}";
                if (Regex.IsMatch(combined, @"^\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}") &&
                    DateTime.TryParse(combined, out date))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryDecodeBase64(string value, out string decoded)
    {
        decoded = "";

        try
        {
            var bytes = Convert.FromBase64String(value.Trim());
            decoded = Encoding.UTF8.GetString(bytes);
            return !string.IsNullOrWhiteSpace(decoded);
        }
        catch
        {
            return false;
        }
    }

    private static void AddFlowgorithmElements(XmlNode node, List<FlowchartElement> elements, List<string> instructions)
    {
        if (node.NodeType != XmlNodeType.Element)
            return;

        if (IsExecutableNode(node.Name))
        {
            var content = GetNodeContent(node);
            elements.Add(new FlowchartElement
            {
                Type = node.Name,
                Content = content
            });

            if (!string.IsNullOrWhiteSpace(content))
                instructions.Add(content);
        }

        foreach (XmlNode child in node.ChildNodes)
        {
            AddFlowgorithmElements(child, elements, instructions);
        }
    }

    private static bool IsExecutableNode(string nodeName)
    {
        return nodeName is "declare" or "assign" or "input" or "output" or "if" or "while"
            or "for" or "do" or "call" or "return" or "break" or "continue";
    }

    private static string GetNodeContent(XmlNode node)
    {
        var attributes = new[] { "expression", "variable", "name", "type" }
            .Select(name => node.Attributes?[name]?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value));

        return string.Join(" ", attributes);
    }

    private int CalculateComplexity(int elementCount, int variableCount)
    {
        // Simple complexity scoring based on elements and variables
        return (elementCount * 2) + variableCount;
    }

    private List<string> HashInstructions(List<string> instructions)
    {
        var hashes = new List<string>();
        foreach (var instruction in instructions)
        {
            hashes.Add(GetInstructionHash(instruction));
        }
        return hashes;
    }

    private string GetInstructionHash(string instruction)
    {
        // Normalize instruction for hashing
        var normalized = instruction.ToLowerInvariant().Trim();
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(normalized);
        var hashBytes = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}

public class FlowgorithmData
{
    public required string FileName { get; set; }
    public string? Author { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public List<Variable> Variables { get; set; } = [];
    public List<FlowchartElement> Elements { get; set; } = [];
    public int ElementCount { get; set; }
    public int ComplexityScore { get; set; }
    public List<string> InstructionHashes { get; set; } = [];
}

public class Variable
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public string? Value { get; set; }
}

public class FlowchartElement
{
    public required string Type { get; set; }
    public required string Content { get; set; }
}
