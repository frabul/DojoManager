using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using NPOI;
using NPOI.XWPF.UserModel;

namespace DojoManagerGui
{
    internal class DocTemplateCompiler
    {
        public string TemplatePath { get; set; }
        public Dictionary<string, object> Macros { get; set; } = new Dictionary<string, object>();
        public string OutputFilePath { get; set; }


        public DocTemplateCompiler(string templatePath, string outputFilePath)
        {
            TemplatePath = templatePath;
            OutputFilePath = outputFilePath;
        }

        public void Compile()
        {
            using FileStream fs = new FileStream(TemplatePath, FileMode.Open, FileAccess.Read);
            XWPFDocument doc = new XWPFDocument(fs);
            foreach (var elem in doc.BodyElements)
            {
                if (elem is XWPFTable tab)
                {

                    foreach (var cell in tab.Rows.SelectMany(r => r.GetTableCells()))
                        IterateParagraphs(cell);
                }
            }
            IterateParagraphs(doc);


            using (FileStream outfile = File.Create(OutputFilePath))
            {
                doc.Write(outfile);
            }

        }

        private void IterateParagraphs(IBody body)
        {
            foreach (var par in body.Paragraphs)
            {
                foreach (var kv in Macros)
                {
                    if (par.Text.Contains(kv.Key))
                    {
                        if (kv.Value is BitmapImage img)
                        {
                            foreach (var run in par.Runs)
                                if (run.Text.Contains(kv.Key))
                                {
                                    run.ReplaceText(kv.Key, "");
                                    InsertImage(run, img, 4, 6);
                                }
                        }
                        else
                        {
                            par.ReplaceText(kv.Key, kv.Value.ToString());
                        } 
                    } 
                }
            }

        }

        const int emusPerInch = 914400;
        const int emusPerCm = 360000;

        private static void InsertImage(XWPFRun run, BitmapImage img, double maxWidthCm, double maxHeightCm)
        {
            double widthPx = img.PixelWidth;
            double heightPx = img.PixelHeight;
            double horzRezDpi = img.DpiX;
            double vertRezDpi = img.DpiY;

            double widthEmus = (widthPx / horzRezDpi * emusPerInch);
            double heightEmus = (heightPx / vertRezDpi * emusPerInch);
            double maxWidthEmus = (maxWidthCm * emusPerCm);
            double maxHeightEmus = (maxHeightCm * emusPerCm);
            double reductionFactor = Math.Min(maxWidthEmus / widthEmus, maxHeightEmus / heightEmus);

            widthEmus = reductionFactor * widthEmus;
            heightEmus = reductionFactor * heightEmus;
            //save image as png
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(img));
            using MemoryStream ms = new();
            encoder.Save(ms);
            ms.Seek(0, SeekOrigin.Begin);
            //add image 
            run.AddPicture(ms, (int)PictureType.PNG, "image1", (int)widthEmus, (int)heightEmus);
        }

        public static void Compile(string templatePath, string outputFilePath, Dictionary<string, object> macros)
        {
            DocTemplateCompiler compiler = new DocTemplateCompiler(templatePath, outputFilePath);
            compiler.Macros = macros;
            compiler.Compile();
        }
    }
}
