using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Vetcare.Application.UseCases.Reportes.Queries;

namespace Vetcare.API.Controllers;

[Authorize(Roles = "Administrador,Veterinario")]
[ApiController]
[Route("api/reportes")]
public sealed class ReportesController(ISender sender) : ControllerBase
{
    [HttpGet("atenciones")]
    public async Task<IActionResult> Atenciones(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? veterinario,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetReporteAtencionesQuery(desde, hasta, veterinario), cancellationToken);
        return result.StatusCode == 200
            ? Ok(result.Value)
            : this.ToActionResult(result);
    }

    [HttpGet("atenciones/pdf")]
    [Produces("application/pdf")]
    public async Task<IActionResult> AtencionesPdf(
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? veterinario,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetReporteAtencionesQuery(desde, hasta, veterinario), cancellationToken);
        if (result.StatusCode != 200) return this.ToActionResult(result);

        var rows = result.Value ?? [];
        var total = rows.Sum(row => row.TotalAtenciones);
        var pdf = Document.Create(document => document.Page(page =>
        {
            page.Margin(36);
            page.DefaultTextStyle(style => style.FontSize(10));
            page.Header().Column(header =>
            {
                header.Item().Text("VetCare · Reporte de atenciones").FontSize(20).Bold().FontColor(Colors.Teal.Darken3);
                header.Item().Text($"Período: {desde?.ToString("yyyy-MM-dd") ?? "inicio"} — {hasta?.ToString("yyyy-MM-dd") ?? "actualidad"}");
                if (!string.IsNullOrWhiteSpace(veterinario)) header.Item().Text($"Veterinario: {veterinario}");
            });
            page.Content().PaddingVertical(18).Table(table =>
            {
                table.ColumnsDefinition(columns => { columns.RelativeColumn(3); columns.RelativeColumn(1); });
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Teal.Darken3).Padding(7).Text("Veterinario").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Teal.Darken3).Padding(7).AlignRight().Text("Atenciones").FontColor(Colors.White).Bold();
                });
                foreach (var row in rows)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(7).Text(row.Veterinario);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(7).AlignRight().Text(row.TotalAtenciones.ToString());
                }
                table.Cell().Background(Colors.Grey.Lighten4).Padding(7).Text("Total").Bold();
                table.Cell().Background(Colors.Grey.Lighten4).Padding(7).AlignRight().Text(total.ToString()).Bold();
            });
            page.Footer().AlignRight().Text(text =>
            {
                text.Span("Generado ").FontColor(Colors.Grey.Darken1);
                text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm")).FontColor(Colors.Grey.Darken1);
            });
        })).GeneratePdf();

        return File(pdf, "application/pdf", $"vetcare-atenciones-{DateTime.UtcNow:yyyyMMdd-HHmm}.pdf");
    }
}
