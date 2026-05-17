using System.Text;

namespace CateringWebsite.Services;

public static class SimplePdfWriter
{
    private const int MaxLinesPerPage = 48;
    private const int MaxCharactersPerLine = 92;

    public static void Write(string path, IReadOnlyList<string> lines)
    {
        var pages = WrapLines(lines)
            .Chunk(MaxLinesPerPage)
            .Select(chunk => chunk.ToList())
            .ToList();

        if (pages.Count == 0)
        {
            pages.Add([]);
        }

        File.WriteAllBytes(path, BuildPdf(pages));
    }

    private static byte[] BuildPdf(IReadOnlyList<IReadOnlyList<string>> pages)
    {
        var objectCount = 2 + pages.Count * 2;
        var objects = new string[objectCount + 1];
        var pageObjectIds = new List<int>();

        objects[1] = "<< /Type /Catalog /Pages 2 0 R >>";
        var nextObjectId = 3;

        foreach (var page in pages)
        {
            var pageObjectId = nextObjectId++;
            var contentObjectId = nextObjectId++;
            pageObjectIds.Add(pageObjectId);

            objects[pageObjectId] = $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> >> >> /Contents {contentObjectId} 0 R >>";
            objects[contentObjectId] = BuildContentObject(page);
        }

        objects[2] = $"<< /Type /Pages /Kids [{string.Join(" ", pageObjectIds.Select(id => $"{id} 0 R"))}] /Count {pages.Count} >>";
        return WriteObjects(objects);
    }

    private static string BuildContentObject(IReadOnlyList<string> page)
    {
        var stream = new StringBuilder();
        stream.AppendLine("BT");
        stream.AppendLine("/F1 11 Tf");
        stream.AppendLine("50 750 Td");

        foreach (var line in page)
        {
            stream.Append(PdfLiteralText(line));
            stream.AppendLine(" Tj");
            stream.AppendLine("0 -14 Td");
        }

        stream.AppendLine("ET");
        var content = stream.ToString();
        var byteLength = Encoding.ASCII.GetByteCount(content);
        return $"<< /Length {byteLength} >>\nstream\n{content}endstream";
    }

    private static byte[] WriteObjects(IReadOnlyList<string?> objects)
    {
        using var output = new MemoryStream();
        WriteAscii(output, "%PDF-1.4\n% SofraLink generated PDF\n");
        var offsets = new List<long> { 0 };

        for (var index = 1; index < objects.Count; index++)
        {
            offsets.Add(output.Position);
            WriteAscii(output, $"{index} 0 obj\n{objects[index]}\nendobj\n");
        }

        var xrefPosition = output.Position;
        WriteAscii(output, $"xref\n0 {objects.Count}\n");
        WriteAscii(output, "0000000000 65535 f \n");

        foreach (var offset in offsets.Skip(1))
        {
            WriteAscii(output, $"{offset:0000000000} 00000 n \n");
        }

        WriteAscii(output, $"trailer\n<< /Size {objects.Count} /Root 1 0 R >>\nstartxref\n{xrefPosition}\n%%EOF");
        return output.ToArray();
    }

    private static List<string> WrapLines(IEnumerable<string> lines)
    {
        var wrapped = new List<string>();
        foreach (var line in lines)
        {
            if (line.Length <= MaxCharactersPerLine)
            {
                wrapped.Add(line);
                continue;
            }

            for (var start = 0; start < line.Length; start += MaxCharactersPerLine)
            {
                wrapped.Add(line.Substring(start, Math.Min(MaxCharactersPerLine, line.Length - start)));
            }
        }

        return wrapped;
    }

    private static string PdfLiteralText(string value)
    {
        return $"({EscapePdfText(value)})";
    }

    private static string EscapePdfText(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");
    }

    private static void WriteAscii(Stream stream, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        stream.Write(bytes);
    }
}
