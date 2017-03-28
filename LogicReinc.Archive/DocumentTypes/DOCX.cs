using DocumentFormat.OpenXml.Packaging;
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
    public class DOCX
    {

        public static string ToText(Stream stream)
        {
            StringBuilder builder = new StringBuilder();
            using (WordprocessingDocument wdDoc = WordprocessingDocument.Open(stream, false))
            {

                NameTable nt = new NameTable();
                XmlDocument xdoc = new XmlDocument(nt);
                xdoc.Load(wdDoc.MainDocumentPart.GetStream());

                XmlNamespaceManager nm = new XmlNamespaceManager(nt);
                string tNs = Namespaces.GetNamespace(xdoc);
                if (tNs != null)
                    nm.AddNamespace("ns", tNs);
                else
                    nm.AddNamespace("ns", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");

                XmlNodeList pNodes = xdoc.SelectNodes("//ns:p", nm);
                foreach (XmlNode pNode in pNodes)
                {
                    XmlNodeList tNodes = pNode.SelectNodes(".//ns:t", nm);
                    foreach (XmlNode tNode in tNodes)
                        builder.Append(tNode.InnerText);
                    builder.Append(Environment.NewLine);
                }

            }
            return builder.ToString();
        }
    }
}
