using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using iTextSharp.text;
using iTextSharp.text.pdf;
using WkHtmlToXSharp;

namespace Html2Pdf.Core
{
    public class Converter
    {
        static Converter()
        {
            WkHtmlToXLibrariesManager.Register(new Win64NativeBundle());
            WkHtmlToXLibrariesManager.Register(new Win32NativeBundle());
        }

        public static string Run(string pdfPath, string htmlFullPath)
        {
            //var pdfName = htmlFullPath.Substring(htmlFullPath.LastIndexOf("\\")+1).Replace(".html", ".pdf");
            var pdfUrl = pdfPath;
            try
            {
                #region USING WkHtmlToXSharp.dll

                // IHtmlToPdfConverter converter = new WkHtmlToPdfConverter();
                IHtmlToPdfConverter converter = new MultiplexingConverter();

                converter.GlobalSettings.Margin.Top = "0cm";
                converter.GlobalSettings.Margin.Bottom = "0cm";
                converter.GlobalSettings.Margin.Left = "0cm";
                converter.GlobalSettings.Margin.Right = "0cm";

                converter.ObjectSettings.Page = htmlFullPath;
                converter.ObjectSettings.Web.EnablePlugins = true;
                converter.ObjectSettings.Web.EnableJavascript = true;
                converter.ObjectSettings.Web.Background = true;
                converter.ObjectSettings.Web.LoadImages = true;
                converter.ObjectSettings.Load.LoadErrorHandling = LoadErrorHandlingType.abort;
                converter.ObjectSettings.Web.MinimumFontSize = 22;
                Byte[] bufferPDF = converter.Convert();
                System.IO.File.WriteAllBytes(pdfUrl, bufferPDF);
                converter.Dispose();

                #endregion
            }
            catch (Exception ex)
            {
                return "error." + ex.Message;
            }
            return "ok";
        }

        public static string Combine(string[] files, string pdfFileName)
        {
            if (!files.Any())
            {
                return "empty";
            }
            PdfReader reader = null;
            Document sourceDocument = null;
            PdfCopy pdfCopyProvider = null;
            PdfImportedPage importedPage;
            string outputPdfPath = Path.Combine(new DirectoryInfo(files[0]).Parent.Parent.FullName, pdfFileName);


            sourceDocument = new Document();
            pdfCopyProvider = new PdfCopy(sourceDocument,
                new System.IO.FileStream(outputPdfPath, System.IO.FileMode.Create));

            //Open the output file
            sourceDocument.Open();
           
            try
            {
                //Loop through the files list
                for (int f = 0; f < files.Length - 1; f++)
                {
                    int pages = GetPageCount(files[f]);

                    reader = new PdfReader(files[f]);
                    //Add pages of current file
                    for (int i = 1; i <= pages; i++)
                    {
                        importedPage = pdfCopyProvider.GetImportedPage(reader, i);
                        pdfCopyProvider.AddPage(importedPage);
                    }

                    reader.Close();
                }
                //At the end save the output file
                sourceDocument.Close();
            }
            catch (Exception ex)
            {
                return "error." + ex.Message;
            } 
            return "ok";
        }

        private static int GetPageCount(string file)
        {
            using (StreamReader sr = new StreamReader(File.OpenRead(file)))
            {
                Regex regex = new Regex(@"/Type\s*/Page[^s]");
                MatchCollection matches = regex.Matches(sr.ReadToEnd());
                return matches.Count;
            }
        }
    }
}