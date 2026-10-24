namespace MyGardenPlanner2026.Tests.E2E.Internal_Tests;

using ClosedXML.Excel;
using FluentAssertions;
using System.Text;

public sealed class AuditLogExportTests
{
    [Fact]
    public void ExportResponse_FileName_LaesFoersteFilenameUdenStjerneVarianten()
    {
        var response = new ExportResponse(
            200, "text/csv",
            "attachment; filename=audit-log-export-20261023-101500.csv; filename*=UTF-8''audit-log-export-20261023-101500.csv",
            []);

        response.FileName.Should().Be("audit-log-export-20261023-101500.csv");
    }

    [Fact]
    public void ExportResponse_UdenContentDisposition_HarIngenFileName()
    {
        new ExportResponse(403, null, null, []).FileName.Should().BeNull();
    }

    [Fact]
    public void ExportResponse_Text_FjernerBom()
    {
        var body = Encoding.UTF8.GetPreamble().Concat("Id,UserId"u8.ToArray()).ToArray();

        new ExportResponse(200, "text/csv", null, body).Text.Should().Be("Id,UserId");
    }

    [Fact]
    public void ParseCsv_HaandtererAnfoerselstegnKommaOgIndlejretJson()
    {
        const string csv = "Id,NewValues\r\n1,\"{\"\"a\"\":1,\"\"b\"\":2}\"\r\n2,\r\n";

        var rows = AuditLogExport.ParseCsv(csv);

        rows.Should().HaveCount(3);
        rows[0].Should().Equal("Id", "NewValues");
        rows[1].Should().Equal("1", "{\"a\":1,\"b\":2}");
        rows[2].Should().Equal("2", string.Empty);
    }

    [Fact]
    public void ParseCsv_UdenAfsluttendeLinjeskift_MedtagerSidsteRaekke()
    {
        AuditLogExport.ParseCsv("a,b\n1,2").Should().HaveCount(2);
    }

    [Fact]
    public void JsonRowCount_GiverAntalElementer()
    {
        AuditLogExport.JsonRowCount("[{\"Id\":1},{\"Id\":2},{\"Id\":3}]"u8.ToArray()).Should().Be(3);
    }

    [Fact]
    public void XlsxRowCount_TaellerDatarakkerUdenOverskrift_OgIsZipGenkenderFilen()
    {
        using var stream = new MemoryStream();
        using (var workbook = new XLWorkbook())
        {
            var sheet = workbook.Worksheets.Add("AuditLog");
            sheet.Cell(1, 1).Value = "Id";
            sheet.Cell(2, 1).Value = 1;
            sheet.Cell(3, 1).Value = 2;
            workbook.SaveAs(stream);
        }

        var bytes = stream.ToArray();

        AuditLogExport.IsZip(bytes).Should().BeTrue();
        AuditLogExport.XlsxRowCount(bytes).Should().Be(2);
    }

    [Fact]
    public void IsZip_TekstFil_ErFalsk()
    {
        AuditLogExport.IsZip("Id,UserId"u8.ToArray()).Should().BeFalse();
    }
}