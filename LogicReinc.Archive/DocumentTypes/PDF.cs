using org.apache.pdfbox.pdmodel;
using org.apache.pdfbox.util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicReinc.Archive.DocumentTypes
{
    public class PDF
    {
        public static string ToText(Stream stream)
        {
            PDDocument doc = null;
            try
            {
                using(java.io.InputStream str = new ikvm.io.InputStreamWrapper(stream))
                    doc = PDDocument.load(str);

                PDFTextStripper stripper = new PDFTextStripper();
                return stripper.getText(doc);
            }
            finally
            {
                if (doc != null)
                    doc.close();
            }
        }
    }
}
