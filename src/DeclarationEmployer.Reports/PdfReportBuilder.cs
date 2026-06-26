using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace DeclarationEmployer.Reports;

public static class PdfReportBuilder
{
    static PdfReportBuilder()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] BuildSimpleReport(
        string title,
        IReadOnlyList<(string Label, string Value)> facts,
        IReadOnlyList<string> lines)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(35);
                page.Header().Text(title).FontSize(20).Bold();
                page.Content().Column(column =>
                {
                    column.Spacing(8);
                    column.Item().Text("Rapport interne foundation - non officiel").FontSize(10).Italic();

                    foreach (var fact in facts)
                    {
                        column.Item().Text($"{fact.Label} : {fact.Value}").FontSize(11);
                    }

                    column.Item().PaddingTop(10).LineHorizontal(1);

                    foreach (var line in lines)
                    {
                        column.Item().Text(line).FontSize(10);
                    }
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Declaration Employeur Tunisie - ");
                    text.Span(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm"));
                });
            });
        }).GeneratePdf();
    }
}
