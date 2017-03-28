using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LogicReinc.Archive.Components
{
    public static class Namespaces
    {
        public static string GetNamespace(XmlDocument doc)
        {
            foreach (XmlNode node in doc.ChildNodes)
            {
                if (node.Attributes != null)
                {
                    XmlAttribute val = node.Attributes["xmlns"];
                    if (val != null)
                        return val.Value;
                }
            }
            return null;
        }
    }
}
