using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace FunctionLibrary;

public class SaxCalculationReader
{
    public event Action? DocumentStarted;
    public event Action? DocumentEnded;
    public event Action<string>? ElementStarted;
    public event Action<string>? ElementEnded;
    public event Action<string, string>? Characters;

    public List<CalculationInput> Read(string path)
    {
        var inputs = new List<CalculationInput>();

        if (!File.Exists(path))
        {
            return inputs;
        }

        var settings = new XmlReaderSettings
        {
            IgnoreWhitespace = true,
            IgnoreComments = true,
        };

        using var reader = XmlReader.Create(path, settings);

        DocumentStarted?.Invoke();

        CalculationInput? current = null;
        string? currentField = null;

        while (reader.Read())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    ElementStarted?.Invoke(reader.Name);
                    if (reader.Name.Equals("calc", StringComparison.OrdinalIgnoreCase))
                    {
                        current = new CalculationInput();
                    }
                    else if (current != null)
                    {
                        currentField = reader.Name;
                    }
                    break;

                case XmlNodeType.Text:
                    var value = reader.Value.Trim();
                    if (currentField != null && current != null)
                    {
                        Characters?.Invoke(currentField, value);
                        AssignField(current, currentField, value);
                    }
                    break;

                case XmlNodeType.EndElement:
                    ElementEnded?.Invoke(reader.Name);
                    if (reader.Name.Equals("calc", StringComparison.OrdinalIgnoreCase) && current != null)
                    {
                        inputs.Add(current);
                        current = null;
                    }
                    else
                    {
                        currentField = null;
                    }
                    break;
            }
        }

        DocumentEnded?.Invoke();
        return inputs;
    }

    private static void AssignField(CalculationInput input, string field, string value)
    {
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            return;
        }

        switch (field.ToLowerInvariant())
        {
            case "p":
                input.P = parsed;
                break;
            case "x":
                input.X = parsed;
                break;
            case "y":
                input.Y = parsed;
                break;
        }
    }
}
