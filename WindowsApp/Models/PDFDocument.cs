using ImageMagick;
using PdfSharpCore.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperPlane2.Models
{
    public class PDFDocument
    {

        public string FileName
        {
            get;
            private set;
        }


        public string? uniqueKey;
        public string UniqueKey {
            get {
                if (uniqueKey == null) {
                    uniqueKey = Guid.NewGuid().ToString("N");
                }
                return uniqueKey;
            }
        }

        public int Version {
            get;
            private set;
        }

        public PDFDocument(string FileName)
        {
            this.FileName = FileName;

            this.Version = 0;
        }

        public void MarkDirty() {
            this.Version += 1;
        }


        public void WritePages(List<PDFDocument> pages, string fileName) {
            using (var outputDocument = new PdfSharpCore.Pdf.PdfDocument()) {

                using (var inputDocument = PdfReader.Open(this.FileName, PdfDocumentOpenMode.InformationOnly)) {
                    outputDocument.Version = inputDocument.Version;
                    outputDocument.Info.Title = inputDocument.Info.Title;
                    outputDocument.Info.Creator = inputDocument.Info.Creator;
                }


                foreach (var document in pages) {
                    var pdf = PdfReader.Open(document.FileName, PdfDocumentOpenMode.Import);
                    var totalPages = pdf.PageCount;
                    for (int page = 0; page < totalPages; page++) {
                        outputDocument.AddPage(pdf.Pages[page]);
                    }
                }

                outputDocument.Save(fileName);
            }
        }

        public void Rotate(int degree)  {

            if (degree == 0) {
                return;
            }

            using (var inputDocument = PdfReader.Open(FileName, PdfDocumentOpenMode.Modify)) {
                var totalPages = inputDocument.PageCount;
                for (int page = 0; page < totalPages; page++) {

                    var pdfPage = inputDocument.Pages[page];
                    var d = pdfPage.Rotate + degree;
                    if (d < 360) {
                        d += 360;
                    }
                    d = d % 360;

                    if (pdfPage.Orientation == PdfSharpCore.PageOrientation.Portrait)
                    {
                        pdfPage.Rotate = d;
                    }
                    else
                    {
                        pdfPage.Rotate = d;
                    }

                    if (d == 0 || d == 180) {
                        if (pdfPage.Orientation == PdfSharpCore.PageOrientation.Portrait) {
                            pdfPage.Orientation = PdfSharpCore.PageOrientation.Landscape;
                        }
                        else {
                            pdfPage.Orientation = PdfSharpCore.PageOrientation.Portrait;
                        }
                    }
                }

                inputDocument.Save(FileName);
            }

            MarkDirty();
        }

        public List<PDFDocument> SplitPages(string directory)
        {

            List<PDFDocument> pages = new List<PDFDocument>();


            using (var inputDocument = PdfReader.Open(this.FileName, PdfDocumentOpenMode.Import)) {

                var totalPages = inputDocument.PageCount;
                for (int page = 0; page < totalPages; page++) {
                    var outputName = Path.Combine(directory, String.Format("{0}.pdf", page + 1));

                    using (var outputDocument = new PdfSharpCore.Pdf.PdfDocument()) {
                        outputDocument.Version = inputDocument.Version;
                        outputDocument.Info.Title = String.Format("Page {0} of {1}", page + 1, inputDocument.Info.Title);
                        outputDocument.Info.Creator = inputDocument.Info.Creator;

                        outputDocument.AddPage(inputDocument.Pages[page]);
                        outputDocument.Save(outputName);
                    }

                    pages.Add(new PDFDocument(outputName));
                }
            }

            return pages;
        }   

        public void ExportThumbnail(string fileName, int size)
        {

            var settings = new MagickReadSettings();

            double density = size / 5;

            settings.Density = new Density(density);
            settings.FrameIndex = 0; // First page
            settings.FrameCount = 1; // Number of pages

            using (var images = new MagickImageCollection()) {
                // Add all the pages of the pdf file to the collection
                images.Read(this.FileName, settings);

                // Create new image that appends all the pages vertically
                using (var vertical = images.AppendVertically()) {
                    // Save result as a png
                    int border = size / 250;
                    if (border < 1) {
                        border = 1;
                    }

                    vertical.Thumbnail(size - 4 * border, size - 4 * border);
                    vertical.Border(border);
                    vertical.Extent(size, size, Gravity.Center);
                    vertical.Format = MagickFormat.Png;

                    vertical.Write(String.Format("{0}.tmp", fileName));
                }

                File.Move(String.Format("{0}.tmp", fileName), fileName);
            }

        }
    }
}
