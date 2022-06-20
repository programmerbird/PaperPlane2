using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaperPlane2.Models
{
    public abstract class DocumentAction
    {

        public virtual void Init(MainForm form) { 
            
        }
        public virtual void Execute(MainForm form) {
            Init(form);
            Redo(form);
        }
        public virtual void Undo(MainForm form) { 
        }
        public virtual void Redo(MainForm form) { 
            
        }
    }
}
