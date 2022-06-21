using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperPlane2.Models
{
    public class Workspace
    {
        public PDFDocument? Document;

        private List<PDFDocument>? pages;
        private Dictionary<string, bool> exportThumbnails;

        public string Root {
            get;
            private set;
        }



        public Workspace(string root) {
            this.Root = root;
            this.pages = null;
            this.exportThumbnails = new Dictionary<string, bool>();

        }


        public void OpenDocument(PDFDocument? document) {

            this.Document = document;
            this.pages = null;
        }

        public PDFDocument? GetDocumentPage(int index) {
            var pages = ListDocumentPages();
            if (pages == null) {
                return null;
            }

            if (index >= pages.Count) {
                return null;
            }

            return pages[index];
        }


        public List<PDFDocument>? ListDocumentPages(PDFDocument document) {
            if (document == null) {
                return null;
            }

            var pagesRoot = Path.Combine(Root, document.UniqueKey, "Pages");
            if (!Directory.Exists(pagesRoot)) {
                Directory.CreateDirectory(pagesRoot);
            }

            return document.SplitPages(pagesRoot);
        }

        public List<PDFDocument>? ListDocumentPages() {
            if (this.pages != null) {
                return this.pages;
            }
            if(this.Document == null) {
                return null;
            }
            var pagesRoot = Path.Combine(Root, Document.UniqueKey, "Pages");
            if (!Directory.Exists(pagesRoot)) {
                Directory.CreateDirectory(pagesRoot);
            }

            var pages = Document.SplitPages(pagesRoot);
            this.pages = pages;
            return pages;
        }


        private string GetThumbnailFileName(PDFDocument page, int size)  {
            return string.Format("{0}-{1}-{2}.png", page.FileName, size, page.Version);
        }

        private int GetThumbnailActualSize(int size) {
            if (size <= 32 * 1.25)
            {
                return 32;
            }
            if (size <= 64 * 1.25)
            {
                return 64;
            }
            else if (size <= 128 * 1.25)
            {
                return 128;
            }
            else if (size <= 256 * 1.25)
            {
                return 256;
            }
            return 512;
        }

        public Image? GetCachePageThumbnailImage(PDFDocument page, int size) {
            int actualSize = GetThumbnailActualSize(size);
            string thumbnailPath = GetThumbnailFileName(page, actualSize);
            if (File.Exists(thumbnailPath)) {
                return Image.FromFile(thumbnailPath);
            }
            return null;
        }

        public Image GetPageThumbnailImage(PDFDocument page, int size) {

            int actualSize = GetThumbnailActualSize(size);
            string thumbnailPath = GetThumbnailFileName(page, actualSize);
            if (!File.Exists(thumbnailPath)) {
                page.ExportThumbnail(thumbnailPath, actualSize);
            }

            return Image.FromFile(thumbnailPath);
        }
        
        public void SaveAs(string fileName) {
            if (Document == null) {
                return;
            }

            var pages = ListDocumentPages();
            if (pages == null) {
                return;
            }

            Document.WritePages(pages, fileName);
        }


        public void SaveAs(string fileName, List<PDFDocument> pages)
        {
            if (Document == null)
            {
                return;
            }

            if (pages == null)
            {
                return;
            }

            Document.WritePages(pages, fileName);
        }
    }
}
