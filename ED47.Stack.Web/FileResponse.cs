using System;
using System.IO;
using System.Web;

namespace ED47.Stack.Web
{
    public class FileResponse
    {
        private HttpResponse _Response;

        public FileResponse(string contentType, string fileName)
        {
            FileName = fileName;
            ContentType = contentType;
            var c = HttpContext.Current;
            _Response = c.Response;
            _Response.Clear();
            _Response.ClearContent();
            _Response.ClearHeaders();
            _Response.AddHeader("Content-Type", ContentType);
            _Response.AddHeader("Content-Disposition", "attachment; filename=\"" + FileName + "\";");
        }

        public FileResponse(string contentType, string fileName, Stream content)
            : this(contentType, fileName)
        {
            var data = new byte[content.Length];
            content.Read(data, 0, Convert.ToInt32(content.Length));
            Write(data);
            Flush();
        }

        public FileResponse(string contentType, string fileName, byte[] content)
            : this(contentType, fileName)
        {
            Write(content);
            Flush();
        }

        public FileResponse(string contentType, string fileName, FileInfo file)
            : this(contentType, fileName)
        {
            var fs = file.OpenRead();

            var buffer = new byte[file.Length];
            fs.Read(buffer, 0, buffer.Length);
            Write(buffer);
            fs.Close();

            Flush();
        }


        public FileResponse(string contentType, string fileName, string content)
            : this(contentType, fileName)
        {
            if (File.Exists(content))
            {
                Write(File.ReadAllBytes(content));
            }
            else
            {
                Write(content);
            }
            Flush();
        }

        public string ContentType { get; set; }

        public string FileName { get; set; }

        public Stream OutputStream
        {
            get { return _Response.OutputStream; }
        }


        public void Write(string data)
        {
            _Response.Write(data);
        }


        public void Write(byte[] data)
        {
            _Response.BinaryWrite(data);
        }

        public void Flush()
        {
            _Response.Flush();
            _Response.End();
        }
    }
}