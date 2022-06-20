using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperPlane2.Models
{
    public class LazyMergeDocument : IDataObject
    {

        private string sourceFileName;
        private string[] pageFileNames;

        private string text;
        private string suffix;

        public LazyMergeDocument(string text, string suffix, string sourceFileName, string[] pageFileNames) {
            this.text = text;
            this.suffix = suffix;
            this.sourceFileName = sourceFileName;
            this.pageFileNames = pageFileNames;
        }

        private string? GetExportFileName() {

            string baseName = Path.GetFileNameWithoutExtension(sourceFileName);
            string fileName = Path.Combine(Path.GetTempPath(), string.Format("{0} {1}.pdf", baseName, suffix));
            if (fileName == null) {
                return null;
            }

            string? dirName = Path.GetDirectoryName(fileName);
            if (dirName == null) {
                return null;
            }
            if(!Directory.Exists(dirName)) {
                Directory.CreateDirectory(dirName);
            }

            List<PDFDocument> pages = new List<PDFDocument>();
            foreach (string pageFileName in pageFileNames) {
                pages.Add(new PDFDocument(pageFileName));
            }

            PDFDocument doc = new PDFDocument(sourceFileName);
            doc.WritePages(pages, fileName);
            return fileName;
        }

        // Returns: The data associated with the specified format, or null.
        public object GetData(string format)
        {
            if (format == DataFormats.Text)
            {
                return text;
            }
            if (format == DataFormats.FileDrop) {
                string? fileName = GetExportFileName();
                if (fileName != null) {
                    return (object)new string[] {
                        fileName,
                    };
                }
            }
            return null;
        }

        public bool GetDataPresent(string format)
        {
            if (format == DataFormats.FileDrop) {
                return true;
            }
            if (format == DataFormats.Text) {
                return true;
            }
            return false;
        }

        public string[] GetFormats()
        {
            return new string[] { DataFormats.Text, DataFormats.FileDrop };
        }

        public object GetData(Type format) { return GetData(format.ToString()); }
        public object GetData(string format, bool autoConvert) { return GetData(format); }

        public bool GetDataPresent(Type format) { return GetDataPresent(format.ToString()); }
        public bool GetDataPresent(string format, bool autoConvert) { return GetDataPresent(format); }

        public string[] GetFormats(bool autoConvert) { return GetFormats(); }
        public void SetData(object data) { throw new Exception("Unimplemented"); }
        public void SetData(string format, object data) { SetData(data); }
        public void SetData(Type format, object data) { SetData(data); }
        public void SetData(string format, bool autoConvert, object data) { SetData(data); }
    }
}
