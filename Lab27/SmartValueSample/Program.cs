using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using FunctionLibrary;

internal static class Program
{
    private const string InputFileName = "input.xml";
    private const string OutputFileName = "output.xml";
    private const string ReportFileName = "report.txt";

    private static readonly Regex NumberPattern = new(@"^-?\d+([\.,]\d+)?$", RegexOptions.Compiled);

    private static void Main()
    {
        var baseDir = AppContext.BaseDirectory;
        var inputPath = Resolve(baseDir, InputFileName);
        var outputPath = Path.Combine(Path.GetDirectoryName(inputPath)!, OutputFileName);
        var reportPath = Path.Combine(Path.GetDirectoryName(inputPath)!, ReportFileName);

        try
        {
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException("Файл не найден", inputPath);
            }

            var sax = new SaxCalculationReader();
            var trace = new List<string>();
            sax.DocumentStarted += () => trace.Add("Начало разбора документа!");
            sax.DocumentEnded += () => trace.Add("Разбор документа завершён!");
            sax.ElementStarted += name => trace.Add($"<{name}>");
            sax.ElementEnded += name => trace.Add($"</{name}>");
            sax.Characters += (field, value) =>
            {
                var ok = NumberPattern.IsMatch(value) ? "" : "  [не число!]";
                trace.Add($"  {field} = {value}{ok}");
            };

            var inputs = sax.Read(inputPath);

            Console.WriteLine("--- SAX-трассировка чтения ---");
            foreach (var line in trace)
            {
                Console.WriteLine(line);
            }

            Console.WriteLine();
            Console.WriteLine($"Прочитано наборов: {inputs.Count}");

            var results = new List<CalculationResult>(inputs.Count);
            foreach (var input in inputs)
            {
                var f = FunctionCalculator.CalculateA(input.P, input.X, input.Y);
                results.Add(new CalculationResult
                {
                    P = input.P,
                    X = input.X,
                    Y = input.Y,
                    F = f,
                });

                Console.WriteLine(
                    $"  p = {Fmt(input.P)}, x = {Fmt(input.X)}, y = {Fmt(input.Y)}  =>  F = {Fmt(f)}");
            }

            WriteResultsStreaming(outputPath, results);
            Console.WriteLine($"Создан XML-результат: {outputPath}");

            WriteReport(reportPath, results, trace);
            Console.WriteLine($"Создан отчёт: {reportPath}");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Ошибка: файл {Path.GetFileName(ex.FileName ?? InputFileName)} не найден");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static void WriteResultsStreaming(string path, IReadOnlyList<CalculationResult> results)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "    ",
            Encoding = new UTF8Encoding(false),
        };

        using var writer = XmlWriter.Create(path, settings);
        writer.WriteStartDocument();
        writer.WriteStartElement("results");
        writer.WriteAttributeString("count", results.Count.ToString(CultureInfo.InvariantCulture));

        foreach (var r in results)
        {
            writer.WriteStartElement("result");
            writer.WriteElementString("p", Fmt(r.P));
            writer.WriteElementString("x", Fmt(r.X));
            writer.WriteElementString("y", Fmt(r.Y));
            writer.WriteElementString("F", Fmt(r.F));
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();
    }

    private static string Resolve(string baseDir, string fileName)
    {
        var nearExe = Path.Combine(baseDir, fileName);
        if (File.Exists(nearExe))
        {
            return nearExe;
        }

        var nearCwd = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        return File.Exists(nearCwd) ? nearCwd : nearExe;
    }

    private static string Fmt(double value)
    {
        return value.ToString("0.######", CultureInfo.InvariantCulture);
    }

    private static void WriteReport(
        string path,
        IReadOnlyList<CalculationResult> results,
        IReadOnlyList<string> trace)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Итоговый отчёт по обработке XML (SAX)");
        sb.AppendLine(new string('-', 60));
        sb.AppendLine($"Формула F = tan(p/9) * (x + y), всего вычислений: {results.Count}");
        sb.AppendLine();
        sb.AppendLine($"{"p",-8}{"x",-8}{"y",-8}F");

        foreach (var r in results)
        {
            sb.AppendLine($"{Fmt(r.P),-8}{Fmt(r.X),-8}{Fmt(r.Y),-8}{Fmt(r.F)}");
        }

        sb.AppendLine();
        sb.AppendLine($"Минимум F: {Fmt(results.Min(r => r.F))}");
        sb.AppendLine($"Максимум F: {Fmt(results.Max(r => r.F))}");
        sb.AppendLine($"Среднее F:  {Fmt(results.Average(r => r.F))}");

        sb.AppendLine();
        sb.AppendLine("Трассировка SAX-парсера:");
        foreach (var line in trace)
        {
            sb.AppendLine($"  {line}");
        }

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }
}
