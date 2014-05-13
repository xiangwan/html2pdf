using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NLog;
using WkHtmlToXSharp;

namespace Html2Pdf.Core
{
    public class Converter{
        private static Logger logger = LogManager.GetCurrentClassLogger();
        static Converter()
        {
            WkHtmlToXLibrariesManager.Register(new Win64NativeBundle());
            WkHtmlToXLibrariesManager.Register(new Win32NativeBundle());
        }

        public static string Run(string pdfPath, string htmlFullPath, int pdfFontSize) {
            var htmlTemp = Path.Combine(new DirectoryInfo(htmlFullPath).Parent.FullName,
                    Guid.NewGuid().ToString() + ".html");  
            File.Copy(htmlFullPath,htmlTemp);
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

                converter.ObjectSettings.Page = htmlTemp;
                converter.ObjectSettings.Web.EnablePlugins = true;
                converter.ObjectSettings.Web.EnableJavascript = true;
                converter.ObjectSettings.Web.Background = true;
                converter.ObjectSettings.Web.LoadImages = true;
                converter.ObjectSettings.Load.LoadErrorHandling = LoadErrorHandlingType.abort;
                converter.ObjectSettings.Web.MinimumFontSize = pdfFontSize;
                Byte[] bufferPDF = converter.Convert();
                System.IO.File.WriteAllBytes(pdfUrl, bufferPDF);
                converter.Dispose();

                #endregion
            }
            catch (Exception ex) {
                File.Delete(htmlTemp);
                logger.Error(ex);
                return "error." + ex.Message;
            }
            File.Delete(htmlTemp);
            return "ok";
        }

        public static string Combine(string[] files, string pdfFileName)
        {
            if (!files.Any())
            {
                return "empty";
            }
            string outputPdfPath = Path.Combine(new DirectoryInfo(files[0]).Parent.Parent.FullName, pdfFileName+".pdf");
             
            try
            {
                using (FileStream stream = new FileStream(outputPdfPath, FileMode.Create))
                using (Document doc = new Document())
                using (PdfCopy pdf = new PdfCopy(doc, stream))
                {
                    doc.Open(); 
                    doc.AddTitle(pdfFileName);
/*
                    doc.NewPage();  
                    doc.Add(new Paragraph(pdfFileName));
                    doc.Add(new Paragraph("power by "));
                    Anchor anchor = new Anchor("html2pdf");
                    anchor.Reference = "https://github.com/xiangwan/html2pdf"; 
                    doc.Add(anchor);*/
                    
                    var rootOutline = pdf.RootOutline;
                    PdfReader reader = null;
                    PdfImportedPage page = null;
                    PdfContentByte cb = pdf.DirectContent;
                    PdfWriter wrt = cb.PdfWriter;
                    var pageIndex = 1;
                    files.ToList().ForEach(file =>
                    {
                        reader = new PdfReader(file);
                        var n = reader.NumberOfPages;
                        for (int i = 0; i < n; i++)
                        {
                            page = pdf.GetImportedPage(reader, i + 1);
                            pdf.AddPage(page);
                        }
                       
                        var title = new FileInfo(file).Name.Replace(".pdf","");
                        var oline = new PdfOutline(rootOutline,
                            PdfAction.GotoLocalPage(pageIndex, new PdfDestination(pageIndex), wrt), title);
                        rootOutline.AddKid(oline);
                        pageIndex += n;
                        pdf.FreeReader(reader);
                        reader.Close();
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return "error." + ex.Message;
            }
            return "ok";
        }

    }
}