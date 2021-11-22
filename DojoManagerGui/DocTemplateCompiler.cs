using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI;
using NPOI.XWPF.UserModel;

namespace DojoManagerGui
{
    internal class DocTemplateCompiler
    {
        public string TemplatePath { get; set; }
        public Dictionary<string, string> Macros { get; set; } = new Dictionary<string, string>();
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
                foreach (var run in par.Runs) 
                    foreach (var kv in Macros)
                        run.SetText(run.Text.Replace(kv.Key, kv.Value));
        }

        public static void Compile(string templatePath, string outputFilePath, Dictionary<string, string> macros)
        {
            DocTemplateCompiler compiler = new DocTemplateCompiler(templatePath, outputFilePath);
            compiler.Macros = macros;
            compiler.Compile();
        }
    }
}
