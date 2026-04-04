using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using Logbook.Models;

namespace Logbook.Services;

public class PdfExportService
{
    // Generates a weekly activity report PDF and returns it as a byte array
    public byte[] GenerateWeeklyReport(
        IEnumerable<JobApplication> applications,
        DateTime startDate,
        DateTime endDate,
        string jobseekerName)
    {
        var document = new PdfDocument();
        document.Info.Title = "Logbook Weekly Activity Report";
        document.Info.Author = jobseekerName;

        var page = document.AddPage();
        page.Size = PdfSharpCore.PageSize.A4;
        var gfx = XGraphics.FromPdfPage(page);

        // Fonts
        var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
        var fontHeading = new XFont("Arial", 11, XFontStyle.Bold);
        var fontBody = new XFont("Arial", 10, XFontStyle.Regular);
        var fontSmall = new XFont("Arial", 9, XFontStyle.Regular);

        // Colours
        var colourBlack = XBrushes.Black;
        var colourGrey = new XSolidBrush(XColor.FromArgb(100, 100, 100));
        var colourHeaderBg = new XSolidBrush(XColor.FromArgb(46, 64, 87));
        var colourRowAlt = new XSolidBrush(XColor.FromArgb(245, 245, 245));
        var colourWhite = XBrushes.White;

        double margin = 50;
        double y = margin;
        double pageWidth = page.Width - margin * 2;

        // ── Title ──────────────────────────────────────────────────────
        gfx.DrawString("Logbook — Job Search Activity Report",
            fontTitle, colourBlack,
            new XRect(margin, y, pageWidth, 24),
            XStringFormats.TopLeft);
        y += 30;

        // ── Metadata ───────────────────────────────────────────────────
        gfx.DrawString($"Name: {jobseekerName}",
            fontBody, colourBlack,
            new XRect(margin, y, pageWidth, 16),
            XStringFormats.TopLeft);
        y += 18;

        gfx.DrawString(
            $"Period: {startDate:dd MMM yyyy} – {endDate:dd MMM yyyy}",
            fontBody, colourBlack,
            new XRect(margin, y, pageWidth, 16),
            XStringFormats.TopLeft);
        y += 18;

        var appList = applications.ToList();
        gfx.DrawString($"Total applications: {appList.Count}",
            fontBody, colourBlack,
            new XRect(margin, y, pageWidth, 16),
            XStringFormats.TopLeft);
        y += 26;

        // ── Divider ────────────────────────────────────────────────────
        gfx.DrawLine(XPens.DarkGray, margin, y, margin + pageWidth, y);
        y += 12;

        // ── Table header ───────────────────────────────────────────────
        double[] colWidths = { 160, 140, 80, 80, 85 };
        string[] headers = { "Company", "Role Title", "Date Applied", "Source", "Status" };

        gfx.DrawRectangle(colourHeaderBg,
            margin, y, pageWidth, 18);

        double x = margin;
        for (int i = 0; i < headers.Length; i++)
        {
            gfx.DrawString(headers[i], fontHeading, colourWhite,
                new XRect(x + 4, y + 3, colWidths[i] - 8, 14),
                XStringFormats.TopLeft);
            x += colWidths[i];
        }
        y += 20;

        // ── Table rows ─────────────────────────────────────────────────
        if (!appList.Any())
        {
            gfx.DrawString("No applications recorded for this period.",
                fontBody, colourGrey,
                new XRect(margin, y + 8, pageWidth, 16),
                XStringFormats.TopLeft);
        }
        else
        {
            for (int row = 0; row < appList.Count; row++)
            {
                // Add a new page if we're running out of space
                if (y + 20 > page.Height - margin)
                {
                    page = document.AddPage();
                    page.Size = PdfSharpCore.PageSize.A4;
                    gfx = XGraphics.FromPdfPage(page);
                    y = margin;
                }

                var app = appList[row];
                bool isAlt = row % 2 == 0;

                if (isAlt)
                    gfx.DrawRectangle(colourRowAlt, margin, y, pageWidth, 18);

                x = margin;
                string[] cells =
                {
                    Truncate(app.CompanyName, 28),
                    Truncate(app.RoleTitle, 24),
                    app.DateApplied.ToString("dd MMM yyyy"),
                    Truncate(app.Source ?? "—", 14),
                    app.Status.ToString()
                };

                for (int col = 0; col < cells.Length; col++)
                {
                    gfx.DrawString(cells[col], fontSmall, colourBlack,
                        new XRect(x + 4, y + 4, colWidths[col] - 8, 14),
                        XStringFormats.TopLeft);
                    x += colWidths[col];
                }

                y += 18;
            }
        }

        // ── Footer ─────────────────────────────────────────────────────
        double footerY = page.Height - margin + 10;
        gfx.DrawString(
            $"Generated by Logbook on {DateTime.Now:dd MMM yyyy HH:mm}",
            fontSmall, colourGrey,
            new XRect(margin, footerY, pageWidth, 14),
            XStringFormats.TopLeft);

        // ── Write to byte array ────────────────────────────────────────
        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= maxLength ? value : value[..maxLength] + "…";
    }
}