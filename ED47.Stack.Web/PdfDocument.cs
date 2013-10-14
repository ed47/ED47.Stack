using System;
using System.IO;
using EvoPdf.HtmlToPdf;
using System.Web;

namespace ED47.Stack.Web
{
    public class PdfDocument
    {

        private PdfConverter _Converter;
        public PdfConverter Converter
        {
            get
            {
                return _Converter;
            }
        }


        private String _Content;
        public string Content
        {
            get
            {
                return _Content;
            }
            set 
            {
                _Content = value;
            }
        }

        private string _Header = " ";
        public string Header
        {
            get
            {
                return _Header;
            }
            set
            {
                _Header = value;
            }
        }

        private string _Footer = " ";
        public string Footer
        {
            get
            {
                return _Footer;
            }
            set
            {
                _Footer = value;
            }
        }

        public PdfDocument(string content)
        {
            this.Content = content;
            
            PdfConverter pdf = new PdfConverter();
            pdf.LicenseKey = "0vng8uHh8uPi5fLg/OLy4eP84+D86+vr6w==";
            pdf.PdfDocumentOptions.PdfPageSize = PdfPageSize.A4;
        
            pdf.PdfDocumentOptions.PdfCompressionLevel = PdfCompressionLevel.Normal;

            pdf.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Portrait;
            pdf.PdfDocumentOptions.FitWidth = false;
            pdf.PdfDocumentOptions.ShowHeader = false;// header != "";
            pdf.PdfDocumentOptions.ShowFooter = false;// footer != "";
            pdf.AvoidTextBreak = true;
            pdf.AvoidImageBreak = true;
            _Converter = pdf;


        }


        private string _Filename = null;
        public string Filename
        {
            get
            {
                return _Filename;
            }
        }

        /// <summary>
        /// Create a pdf and return the file path
        /// </summary>
        /// <param name="content">The html content</param>
        /// <returns></returns>
        public string CreateInTemporaryFile()
        {
            string temp = Path.GetTempPath();

            if (!Directory.Exists(temp))
                Directory.CreateDirectory(temp);
            string path = temp + "\\" + Guid.NewGuid().ToString() + ".pdf";
            path = CreateInFile(path);
            this._Filename = path;
            return path;
            
        }


        /// <summary>
        /// Create a pdf and return the file path
        /// </summary>
        /// <param name="content">The html content</param>
        /// <returns></returns>
        public string CreateInFile( string filename)
        { 
            this._Filename = Path.GetDirectoryName(filename) + "\\" + Path.GetFileName(filename);

            using (var s = File.Open(_Filename, FileMode.OpenOrCreate))
            {
                _Converter.SavePdfFromHtmlStringToStream(Content, s);
            }

            return this._Filename;
        }

        /// <summary>
        /// Create a pdf write it into a stream
        /// </summary>
        /// <param name="content">The html content</param>
        /// <param name="s">The stream</param>
        public  void CreateInStream(Stream s)
        {
            byte[] pdfbyte = _Converter.GetPdfBytesFromHtmlString(Content, HttpContext.Current.Request.ApplicationPath);
            s.Write(pdfbyte, 0, pdfbyte.Length);
            s.Flush();
        }

        //public FileResponse CreateFileResponse(string filename)
        //{
        //    FileResponse res = new FileResponse("application/pdf", filename);
        //    this.CreateInStream(res.OutputStream);
        //    CurrentResponse.SetMode(ResponseMode.InStream);
        //    res.Flush();
        //    return res;
        //}

        public void CreateInHttpResponse(string attachName, bool endResponse=false)
        {
            var c = System.Web.HttpContext.Current;
            if (c != null) {
                //CurrentResponse.SetMode(ResponseMode.InStream);
                byte[] pdfbyte = _Converter.GetPdfBytesFromHtmlString(Content, HttpContext.Current.Request.ApplicationPath);
                System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                response.Clear();
                response.AddHeader("Content-Type", "application/pdf");
                response.AddHeader("Content-Disposition", "attachment; filename=" + attachName + "; size=" + pdfbyte.Length);
                response.Flush();
                response.BinaryWrite(pdfbyte);
                response.Flush();
                
                if (endResponse)
                    response.End();
            }
        }
        
    }
}
