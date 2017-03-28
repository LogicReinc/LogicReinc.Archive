using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicReinc.Archive.Components
{
    public static class FileTypes
    {
        public static Dictionary<string, string> MimeMap { get; } = new Dictionary<string, string>()
        {
            ["txt"] = "text/plain",
            ["docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ["pdf"] = "application/pdf",
            ["odt"] = "application/vnd.oasis.opendocument.text"
        };
    }
}
