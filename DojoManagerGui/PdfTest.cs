 
using Gehtsoft.PDFFlow.Builder;
using Gehtsoft.PDFFlow.Models.Shared;
using Gehtsoft.PDFFlow.Models.Enumerations;
using Gehtsoft.PDFFlow.Utils;
using System.Diagnostics;

namespace DojoManagerGui
{
    class PdfTest
    {
        public static void Test()
        {
            Color colorDarkBlue = Color.FromRgba(0, 125.0 / 255.0, 181.0 / 255.0);
            Color colorLightBlue = Color.FromRgba(0, 175.0 / 255.0, 245.0 / 255.0);

            // Create a document builder:

            DocumentBuilder builder = DocumentBuilder.New();

            // Create a section builder and customize the section:

            var sectionBuilder =
                builder
                    .AddSection()
                        // Customize settings:
                        .SetMargins(horizontal: 30, vertical: 10)
                        .SetSize(PaperSize.A4)
                         .SetOrientation(PageOrientation.Portrait)
                        .SetNumerationStyle(NumerationStyle.Arabic);

            // Add a TOC item: 
            sectionBuilder
                .AddParagraph("Adding Paragraph")
                    .SetMarginTop(15)
                    .SetFontColor(Color.Gray)
                    .SetAlignment(HorizontalAlignment.Center)
                    .SetOutline();

            // Create a paragraph builder to customize a paragraph and add content to it:

            sectionBuilder
                .AddParagraph("Lorem ipsum dolor sit amet, consectetur adipiscing elit. ")
                    .SetBackColor(colorLightBlue)
                    .SetFirstLineIndent(20)
                    .SetJustifyAlignment(true)
                    .SetFont(Fonts.Courier(16)).SetFontColor(colorDarkBlue).SetBold()
                    //.SetBorderStroke(Stroke.Dotted).SetBorderWidth(3).SetBorderColor(colorDarkBlue)
                    .AddText("Lorem ipsum dolor sit amet, consectetur adipiscing elit")
                        //.SetBackColor(colorDarkBlue)
                        .SetFontColor(Color.White)
                        .SetStrikethrough(Stroke.Solid, Color.Gray)
                .ToParagraph()
                    .AddText(". Lorem ipsum dolor sit amet, consectetur adipiscing elit");
                
            builder.Build("pdftest.pdf");
 
        }
    }
}
