using PaperPlane2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperPlane2.Actions
{
    public class RotateDocumentPageAction : DocumentAction
    {
        private int degree;
        private List<PDFDocument>? pages;
        public RotateDocumentPageAction(int degree) {
            this.degree = degree;

        }

        public override void Redo(MainForm form)
        {
            pages = form.ListSelectedPages();
            if (pages == null) {
                return;
            }

            foreach (var page in pages) {
                page.Rotate(degree);
            }
            form.ReloadDocumentThumbnails();
        }

        public override void Undo(MainForm form)
        {
            if (pages == null) {
                return;
            }


            foreach (var page in pages) {
                page.Rotate(-degree);
            }

            form.ReloadDocumentThumbnails();
        }
    }
}
