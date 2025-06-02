using CrmSystem.Api.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CrmSystem.Api.Services;

public class PdfService
{
    public byte[] GenerateClientPdf(List<Client> clients)
    {
        // ✅ Указание лицензии для QuestPDF
        QuestPDF.Settings.License = LicenseType.Community;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.Header().Text("Clients Report").FontSize(20).Bold().AlignCenter();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Full Name");
                        header.Cell().Element(CellStyle).Text("Email");
                        header.Cell().Element(CellStyle).Text("Phone");
                        header.Cell().Element(CellStyle).Text("Company");
                        header.Cell().Element(CellStyle).Text("Active");
                    });

                    foreach (var c in clients)
                    {
                        table.Cell().Element(CellStyle).Text(c.FullName);
                        table.Cell().Element(CellStyle).Text(c.Email);
                        table.Cell().Element(CellStyle).Text(c.Phone);
                        table.Cell().Element(CellStyle).Text(c.Company);
                        table.Cell().Element(CellStyle).Text(c.IsActive ? "Yes" : "No");
                    }

                    static IContainer CellStyle(IContainer container) =>
                        container.Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                });
            });
        });

        return doc.GeneratePdf();
    }
}