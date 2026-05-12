using System.Data;
using System.IO;
using System.Threading;
using System.Windows;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Layout;
using iText.Layout.Element;
using SWF = System.Windows.Forms;

namespace Lab34Wpf;

public partial class ReportWindow : Window
{
    private static readonly DateTime ReportDate = new(2024, 2, 14);

    public ReportWindow()
    {
        InitializeComponent();
    }

    private void ReportWindow_Loaded(object sender, RoutedEventArgs e)
    {
        new Thread(GenerateReportBackground)
        {
            IsBackground = true,
            Name = "OrdersReportDb"
        }.Start();
    }

    private void GenerateReportBackground()
    {
        try
        {
            var dataTable = OrdersReportRepository.LoadOrdersForReportDate(ReportDate);
            Dispatcher.Invoke(() => BuildPdfAndSave(dataTable));
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() =>
                System.Windows.MessageBox.Show(this, ex.Message, "Ошибка отчета", MessageBoxButton.OK, MessageBoxImage.Error));
        }
    }

    private void BuildPdfAndSave(DataTable dataTable)
    {
        using var stream = new MemoryStream();
        var pdfWriter = new PdfWriter(stream);
        pdfWriter.SetSmartMode(true);

        var pdfDocument = new PdfDocument(pdfWriter);
        var document = new Document(pdfDocument);

        document.Add(new Paragraph("Отчет о заявках на печать (14.02.2024)"));

        var table = new Table(dataTable.Columns.Count);
        foreach (DataColumn column in dataTable.Columns)
            table.AddHeaderCell(new Cell().Add(new Paragraph(column.ColumnName)));

        foreach (DataRow row in dataTable.Rows)
        {
            foreach (var item in row.ItemArray)
                table.AddCell(new Cell().Add(new Paragraph(item?.ToString() ?? string.Empty)));
        }

        document.Add(table);
        document.Close();

        var saveFileDialog = new Microsoft.Win32.SaveFileDialog { Filter = "PDF files (*.pdf)|*.pdf" };
        if (saveFileDialog.ShowDialog() == true)
        {
            File.WriteAllBytes(saveFileDialog.FileName, stream.ToArray());
            System.Windows.MessageBox.Show(this, "PDF-файл успешно создан!", "Отчет", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void OpenPdf_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog { Filter = "PDF files (*.pdf)|*.pdf" };
        if (openFileDialog.ShowDialog() != true)
            return;

        try
        {
            var dataTable = ExtractPdfData(openFileDialog.FileName);
            ReportGrid.ItemsSource = dataTable.DefaultView;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(this, ex.Message, "Ошибка чтения PDF", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static DataTable ExtractPdfData(string pdfFilePath)
    {
        var dataTable = new DataTable();
        dataTable.Columns.Add("Номер");
        dataTable.Columns.Add("Код изделия");
        dataTable.Columns.Add("Количество");
        dataTable.Columns.Add("Сумма");

        using var reader = new PdfReader(pdfFilePath);
        using var pdfDoc = new PdfDocument(reader);

        for (var i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
        {
            var strategy = new SimpleTextExtractionStrategy();
            var pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), strategy);
            var lines = pageText.Split('\n');

            for (var j = 1; j < lines.Length; j++)
            {
                var cells = lines[j].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var row = dataTable.NewRow();
                for (var k = 0; k < cells.Length; k++)
                {
                    if (k < dataTable.Columns.Count)
                        row[k] = cells[k].Trim();
                    else
                        break;
                }

                dataTable.Rows.Add(row);
            }
        }

        return dataTable;
    }

    private void Print_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new SWF.OpenFileDialog { Filter = "PDF files (*.pdf)|*.pdf" };
        if (openFileDialog.ShowDialog() != SWF.DialogResult.OK)
            return;

        var printDialog = new SWF.PrintDialog
        {
            Document = new System.Drawing.Printing.PrintDocument(),
            UseEXDialog = true
        };

        if (printDialog.ShowDialog() != SWF.DialogResult.OK)
            return;

        var printDocument = printDialog.Document;
        printDocument.PrintController = new System.Drawing.Printing.StandardPrintController();
        printDocument.DefaultPageSettings.PaperSize = new System.Drawing.Printing.PaperSize("A4", 826, 595);
        printDocument.Print();
    }
}
