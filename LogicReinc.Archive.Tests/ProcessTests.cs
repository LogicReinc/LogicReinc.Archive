using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicReinc.Archive.Tests
{
    [TestClass]
    public class ProcessTests
    {
        const string TestDocODT = "TestDocs/TestOdt.odt";
        const string TestDocDOCX = "TestDocs/TestDocx.docx";
        const string TestDocText = "TestDocs/TestTxt.txt";
        const string TestDocPDF = "TestDocs/TestPDF.pdf";

        const string TestText = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

        static Archive archive = new Archive(new ArchiveSettings()
        {
            Directory = "ProcessTest",
            FileEncryptionPassword = "Testin"
        });
    
        [ClassCleanup]
        public static void Cleanup()
        {
            archive.Close();
            Directory.Delete("ProcessTest");
        }

        [TestMethod]
        public void ProcessODT()
        {
            LRDocument doc = archive.ProcessFromPath("TestOdt", TestDocODT);

            Assert.AreEqual(TestText.Trim(), doc.Text.Trim());
        }

        [TestMethod]
        public void ProcessDOCX()
        {
            LRDocument doc = archive.ProcessFromPath("TestDocx", TestDocDOCX);

            Assert.AreEqual(TestText.Trim(), doc.Text.Trim());
        }

        [TestMethod]
        public void ProcessText()
        {
            LRDocument doc = archive.ProcessFromPath("TestText", TestDocText);

            Assert.AreEqual(TestText.Trim(), doc.Text.Trim());




            using (Stream str = doc.Read(archive))
            using (StreamReader reader = new StreamReader(str))
            {
                Assert.IsTrue(!string.IsNullOrEmpty(reader.ReadToEnd()));
            }
        }

        [TestMethod]
        public void ProcessPDF()
        {
            LRDocument doc = archive.ProcessFromPath("TestPdf", TestDocPDF);

            Assert.AreEqual(TestText.Trim(), doc.Text.Replace("\r\n", "").Trim());
        }
    }
}
