using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicReinc.Archive
{
    public class ArchiveSettings
    {
        public string Directory { get; set; }
        public bool Hidden { get; set; }
        public bool HashTags { get; set; }
        public string FileEncryptionPassword { get; set; }
    }
}
