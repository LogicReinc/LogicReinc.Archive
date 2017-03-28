using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicReinc.Archive.Exceptions
{
    public class InvalidDocumentException : Exception
    {
        public InvalidDocumentException(string msg) : base(msg) { }
    }
}
