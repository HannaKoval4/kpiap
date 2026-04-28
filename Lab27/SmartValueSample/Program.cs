using System;
using System.Globalization;
using System.IO;
using FunctionLibrary;

class Program
{
    static void Main()
    {
        string baseDir = AppContext.BaseDirectory;
        string inputFile = Path.Combine(baseDir, "input.txt");
        string outputFile = Path.Combine(baseDir, "output.txt");

        try
        {
            if (!File.Exists(inputFile) && File.Exists("input.txt"))
            {
                inputFile = "input.txt";
                outputFile = "output.txt";
            }

            string[] data = File.ReadAllLines(inputFile);

            if (data.Length >= 3)
            {
                double p = double.Parse(data[0], CultureInfo.InvariantCulture);
                double x = double.Parse(data[1], CultureInfo.InvariantCulture);
                double y = double.Parse(data[2], CultureInfo.InvariantCulture);

                double result = FunctionCalculator.CalculateA(p, x, y);

                string output = $"p = {p.ToString(CultureInfo.InvariantCulture)}, x = {x.ToString(CultureInfo.InvariantCulture)}, y = {y.ToString(CultureInfo.InvariantCulture)}, F = {result.ToString(CultureInfo.InvariantCulture)}";
                File.WriteAllText(outputFile, output);

                Console.WriteLine("Результат успешно записан в файл output.txt");
                Console.WriteLine(output);
            }
            else
            {
                Console.WriteLine("Ошибка: в файле недостаточно данных");
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Ошибка: файл {Path.GetFileName(inputFile)} не найден");
        }
        catch (FormatException)
        {
            Console.WriteLine("Ошибка: неверный формат данных в файле");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}
