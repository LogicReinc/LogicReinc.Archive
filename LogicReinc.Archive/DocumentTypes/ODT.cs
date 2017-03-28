using ICSharpCode.SharpZipLib.Zip;
using LogicReinc.Archive.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LogicReinc.Archive.DocumentTypes
{
    public class ODT
    {
        public static string ToText(Stream stream)
        {
            using (ZipFile file = new ZipFile(stream))
            {
                ZipEntry entry = file.GetEntry("content.xml");

                using (Stream eStream = file.GetInputStream(entry))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(eStream);
                    
                    XmlNodeList tNodes = doc.GetElementsByTagName("text:p");

                    StringBuilder builder = new StringBuilder();
                    foreach (XmlNode node in tNodes)
                        builder.AppendLine(node.InnerText);
                    return builder.ToString();
                }

            }
        }
    }
}
