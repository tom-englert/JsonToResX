using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace JsonToResX;

internal static partial class Infrastructure
{
    public static int ProcessFileConversion([Argument] string input,  [Argument] string output)
    {
        try
        {
            input = Path.GetFullPath(input);
            output = Path.GetFullPath(output);

            Console.WriteLine($"Input: {input}, Output: {output}");

            if (!File.Exists(input))
                throw new FileNotFoundException("Input file not found.", input);

            var inputExtension = Path.GetExtension(input);
            var outputExtension = Path.GetExtension(output).ToLowerInvariant();

            if (inputExtension.Equals(".json", StringComparison.OrdinalIgnoreCase) && outputExtension.Equals(".resx", StringComparison.OrdinalIgnoreCase))
            {
                ConvertToResX(input, output);
                return 0;
            }

            if (inputExtension.Equals(".resx", StringComparison.OrdinalIgnoreCase) && outputExtension.Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                ConvertToJson(input, output);
                return 0;
            }

            throw new InvalidOperationException($"Unsupported conversion from '{inputExtension}' to '{outputExtension}'. Supported conversions are: .json to .resx and .resx to .json.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    [GeneratedRegex(@"{{\s*(\S+)\s*}}", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex JsonPlaceholderExpression();

    [GeneratedRegex(@"\${\s*(\S+)\s*}", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex ResxPlaceholderExpression();

    private static string ConvertJsonToResxPlaceholder(this string value)
    {
        return JsonPlaceholderExpression().Replace(value, "${$1}");
    }

    private static string ConvertResxToJsonPlaceholder(this string value)
    {
        return ResxPlaceholderExpression().Replace(value, "{{$1}}");
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private static void ConvertToResX(string input, string output)
    {
        var jsonContent = File.ReadAllText(input);
        var jsonObject = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent) ?? throw new InvalidOperationException("Failed to deserialize JSON content.");

        var resourceFile = new ResourceFile();

        foreach (var node in jsonObject)
        {
            resourceFile.SetValue(node.Key.Replace('.', '_'), node.Value.ConvertJsonToResxPlaceholder());
        }

        resourceFile.Save(output);

        Console.WriteLine($"Successfully converted JSON to ResX: {output}");
    }

    private static void ConvertToJson(string input, string output)
    {
        var resourceFile = new ResourceFile(input);

        var jsonObject = resourceFile.GetNodes().ToDictionary(item => item.Key.Replace('_', '.'), item => item.Text?.ConvertResxToJsonPlaceholder());

        var jsonContent = JsonSerializer.Serialize(jsonObject, JsonSerializerOptions);

        File.WriteAllText(output, jsonContent);

        Console.WriteLine($"Successfully converted ResX to JSON: {output}");
    }
}
