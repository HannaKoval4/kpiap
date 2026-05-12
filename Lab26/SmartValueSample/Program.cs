using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            ValidateInputXml(inputPath);

            var service = new XmlCalculationService();
            var inputs = service.ReadInputs(inputPath);

            Console.WriteLine($"Прочитано наборов из {InputFileName}: {inputs.Count}");

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

            service.WriteResults(outputPath, results);
            Console.WriteLine($"Создан XML-результат: {outputPath}");

            WriteReport(reportPath, results);
            Console.WriteLine($"Создан отчёт: {reportPath}");
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Ошибка: файл {Path.GetFileName(ex.FileName ?? InputFileName)} не найден");
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Ошибка формата данных: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static void ValidateInputXml(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Файл не найден", path);
        }

        var raw = File.ReadAllText(path);
        var valuePattern = new Regex(@"<(?:p|x|y)>\s*(?<v>[^<\s][^<]*?)\s*</(?:p|x|y)>");
        var matches = valuePattern.Matches(raw);

        var bad = matches
            .Select(m => m.Groups["v"].Value)
            .Where(v => !NumberPattern.IsMatch(v))
            .ToList();

        if (bad.Count > 0)
        {
            throw new FormatException(
                $"в input.xml встречены не-числовые значения: {string.Join(", ", bad)}");
        }
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

    private static void WriteReport(string path, IReadOnlyList<CalculationResult> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Итоговый отчёт по обработке XML (DOM)");
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

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }
}
