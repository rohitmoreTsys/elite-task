using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.Extensions.Configuration;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System;
using System.Text;

namespace Elite.ListToPDF
{
    public class ConvertHtmlToPDF
    {
        private IConverter _converter;

        public ConvertHtmlToPDF(IConverter converter)
        {
            _converter = converter;
        }

        public string StartConvert(string htmlDataNew, string name, IConfiguration _configuration, int footerLength = 0, bool isBilingual = false)
        {
            if (htmlDataNew != null)
            {
                string filePathPdf = _configuration.GetSection("ConnectionConfiguration:filePDFPathTemplate").Value;
                string fileName = filePathPdf + name;
                var marginSettings = new MarginSettings(0.2, 0.15, footerLength > 0 ? footerLength * 0.5 : 0.3, 0.15)
                {
                    Unit = Unit.Inches
                };

                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    PaperSize = PaperKind.A4Extra,
                    Outline = false,
                    Margins = marginSettings,
                    DocumentTitle = name,
                    Out = fileName
                };

                //orientSettings
                var orientSettings = _configuration.GetSection("PdfDownloadSettings:IsLandScaped").Value;
                if (orientSettings.ToUpper() == "Y") globalSettings.Orientation = Orientation.Landscape; else globalSettings.Orientation = Orientation.Portrait;

                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = htmlDataNew,
                    //FooterSettings = { FontName = "MB Corpo S Text Office", FontSize = 7, Right = "[page]/[toPage]", Spacing = 30 },
                    WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = System.IO.Path.Combine(_configuration.GetSection("ConnectionConfiguration:filePDFPathTemplate").Value, @"assets\css", "pdf-styles.css") },
                    LoadSettings = { BlockLocalFileAccess = false }
                };

               

                var pdf = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }


                };
                _converter.Convert(pdf);
                return fileName;
            }
            return "";
        }

        public string GeneratePdfFromNewTemplate(string htmlDataNew, string name, IConfiguration _configuration, int footerLength = 0, bool isBilingual = false)
        {
            if (htmlDataNew != null)
            {
                string filePathPdf = _configuration.GetSection("ConnectionConfiguration:filePDFPathTemplate").Value;
                string fileName = filePathPdf + name;
                var marginSettings = new MarginSettings
                {
                    Top = 0.2,
                    Bottom = 1.2,
                    Left = 0.2,
                    Right = 0.2,
                    Unit = Unit.Inches
                };

                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    PaperSize = PaperKind.A4Extra,
                    Outline = false,
                    Margins = marginSettings,
                    DocumentTitle = name,
                    Out = fileName
                };

                var orientSettings = _configuration.GetSection("PdfDownloadSettings:IsLandScaped").Value;
                globalSettings.Orientation = orientSettings.ToUpper() == "Y" ? Orientation.Landscape : Orientation.Portrait;

                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = htmlDataNew,
                    WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = System.IO.Path.Combine(_configuration.GetSection("ConnectionConfiguration:filePDFPathTemplate").Value, @"assets\font\linearicons", "MBCorpoSText.css") },
                    LoadSettings = { BlockLocalFileAccess = false }
                };

                var pdf = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }

                };
                _converter.Convert(pdf);
                return fileName;
            }
            return "";
        }

        public string AddWaterMarkPDF(string sourceFileName, string text, string User, string dtNow)
        {
            try
            {
                PdfDocument doc = PdfReader.Open(sourceFileName, PdfDocumentOpenMode.Modify);

                StringBuilder watermark = new StringBuilder();
                if (!string.IsNullOrEmpty(User))
                {
                    watermark.Append(text + " " + User.ToUpper().Trim() + dtNow.Trim());
                }
                float emSize = 20;

                while (watermark.Length < 200)
                {
                    watermark.Append(watermark.ToString());
                    watermark.Append("   ");
                }

                XFont font = new XFont("MB Corpo S Text Office", emSize, XFontStyle.BoldItalic);
                foreach (PdfPage page in doc.Pages)
                {
                    // Get an XGraphics object for drawing beneath the existing content.
                    var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
                    XGraphicsState state = gfx.Save();

                    // Get the size (in points) of the text.
                    var size = gfx.MeasureString(watermark.ToString(), font);

                    // Define a rotation transformation at the center of the page.
                    gfx.TranslateTransform(page.Width / 2, page.Height / 2);
                    gfx.RotateTransform(-Math.Atan(page.Height / page.Width) * 180 / Math.PI);
                    gfx.TranslateTransform(-page.Width / 2, -page.Height / 2);

                    // Create a string format.
                    var format = new XStringFormat();
                    format.Alignment = XStringAlignment.Near;
                    format.LineAlignment = XLineAlignment.Near;

                    // Create a dimmed red brush.
                    XBrush brush = new XSolidBrush(XColor.FromArgb(100, 211, 211, 211));

                    gfx.DrawString(watermark.ToString(), font, brush,
                       new XPoint(-200.0, (page.Height - size.Height) - 410.0),
                       format);
                    gfx.Dispose();
                }

                doc.Save(sourceFileName);
                return sourceFileName;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void AddFooter(string sourceFileName, string[] footerText, string imagePath)
        {
            PdfDocument doc = PdfReader.Open(sourceFileName, PdfDocumentOpenMode.Modify);

            float emSize = 9;
            float pageNumberFontSize = 10;
            XFont font = new XFont("MB Corpo S Text Office", pageNumberFontSize, XFontStyle.Regular);
            XImage footerImage = XImage.FromFile(imagePath);

            double lineSpacing = 18;
            int lineCount = footerText.Length;
            double footerTextHeight = lineSpacing * lineCount;

            double imageHeight = footerImage.PixelHeight * 20.0 / footerImage.VerticalResolution;
            double imageWidth = footerImage.PixelWidth * 20.0 / footerImage.HorizontalResolution;

            double maxImageWidth = 100;
            double maxImageHeight = 25;
            double scale = Math.Min(maxImageWidth / imageWidth, maxImageHeight / imageHeight);
            imageWidth *= scale;
            imageHeight *= scale;

            double footerHeight = footerTextHeight + imageHeight + 20;

            // Define custom brushes with colors
            XBrush footerTextBrush = new XSolidBrush(XColor.FromArgb(0x5C, 0x5C, 0x5C));   // #5C5C5C
            XBrush pageNumberBrush = new XSolidBrush(XColor.FromArgb(0x84, 0x84, 0x84));    // #848484

            for (int pageIndex = 0; pageIndex < doc.Pages.Count; pageIndex++)
            {
                PdfPage page = doc.Pages[pageIndex];
                using (var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append))
                {
                    double pageHeight = page.Height.Point;
                    double pageWidth = page.Width.Point;

                    gfx.DrawRectangle(new XSolidBrush(XColors.White), 0, pageHeight - footerHeight, pageWidth, footerHeight);

                    double footerTextStartY = pageHeight - footerHeight + 10;
                    for (int i = 0; i < lineCount; i++)
                    {
                        string s = footerText[i];
                        if (string.IsNullOrEmpty(s)) continue;
                        var size = gfx.MeasureString(s, font);
                        gfx.DrawString(s, font, footerTextBrush, new XPoint((pageWidth - size.Width) / 2, footerTextStartY + (lineSpacing * i)));
                    }

                    string pageNumberText = $"{pageIndex + 1}/{doc.Pages.Count}";
                    var pageSize = gfx.MeasureString(pageNumberText, font);
                    gfx.DrawString(pageNumberText, font, pageNumberBrush, new XPoint(pageWidth - pageSize.Width - 20, pageHeight - 10));

                    double x = (pageWidth - imageWidth) / 2;
                    double yImage = pageHeight - imageHeight - 10;
                    gfx.DrawImage(footerImage, x, yImage, imageWidth, imageHeight);
                }
            }

            doc.Save(sourceFileName);
        }
        public void AddFooterWithoutLines(string sourceFileName)
        {
            PdfDocument doc = PdfReader.Open(sourceFileName, PdfDocumentOpenMode.Modify);

            float emSize = 9;
            float pageNumberFontSize = 10;
            XFont font = new XFont("MB Corpo S Text Office", pageNumberFontSize, XFontStyle.Regular);
           
           
            // Define custom brushes with colors
            //XBrush footerTextBrush = new XSolidBrush(XColor.FromArgb(0x5C, 0x5C, 0x5C));   // #5C5C5C
            XBrush pageNumberBrush = new XSolidBrush(XColor.FromArgb(0x84, 0x84, 0x84));    // #848484

            for (int pageIndex = 0; pageIndex < doc.Pages.Count; pageIndex++)
            {
                PdfPage page = doc.Pages[pageIndex];
                using (var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append))
                {
                    double pageHeight = page.Height.Point;
                    double pageWidth = page.Width.Point;
                    string pageNumberText = $"{pageIndex + 1}/{doc.Pages.Count}";
                    var pageSize = gfx.MeasureString(pageNumberText, font);
                    gfx.DrawString(pageNumberText, font, pageNumberBrush, new XPoint(pageWidth - pageSize.Width - 20, pageHeight - 10));
                }
            }

            doc.Save(sourceFileName);
        }
    }
}
