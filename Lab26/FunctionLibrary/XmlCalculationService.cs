using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace FunctionLibrary;

public class XmlCalculationService
{
    public List<CalculationInput> ReadInputs(string path)
    {
        var inputs = new List<CalculationInput>();

        if (!File.Exists(path))
        {
            return inputs;
        }

        var doc = new XmlDocument();
        doc.Load(path);

        var calcNodes = doc.GetElementsByTagName("calc");
        foreach (XmlNode node in calcNodes)
        {
            if (node.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            var element = (XmlElement)node;
            var input = new CalculationInput
            {
                P = ReadDouble(element, "p"),
                X = ReadDouble(element, "x"),
                Y = ReadDouble(element, "y"),
            };
            inputs.Add(input);
        }

        return inputs;
    }

    public void WriteResults(string path, IReadOnlyList<CalculationResult> results)
    {
        var doc = new XmlDocument();
        var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.AppendChild(declaration);

        var root = doc.CreateElement("results");
        root.SetAttribute("count", results.Count.ToString(CultureInfo.InvariantCulture));
        doc.AppendChild(root);

        foreach (var result in results)
        {
            var resultNode = doc.CreateElement("result");
            AppendChildWithValue(doc, resultNode, "p", result.P);
            AppendChildWithValue(doc, resultNode, "x", result.X);
            AppendChildWithValue(doc, resultNode, "y", result.Y);
            AppendChildWithValue(doc, resultNode, "F", result.F);
            root.AppendChild(resultNode);
        }

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    ",
            Encoding = new UTF8Encoding(false),
        };

        using var writer = XmlWriter.Create(path, settings);
        doc.Save(writer);
    }

    private static double ReadDouble(XmlElement parent, string tagName)
    {
        var nodes = parent.GetElementsByTagName(tagName);
        if (nodes.Count == 0)
        {
            return 0;
        }

        var text = nodes[0]?.InnerText.Trim() ?? string.Empty;
        return double.Parse(text, CultureInfo.InvariantCulture);
    }

    private static void AppendChildWithValue(XmlDocument doc, XmlElement parent, string tagName, double value)
    {
        var child = doc.CreateElement(tagName);
        child.InnerText = value.ToString(CultureInfo.InvariantCulture);
        parent.AppendChild(child);
    }
}
