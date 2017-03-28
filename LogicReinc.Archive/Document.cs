using LogicReinc.Archive.DocumentTypes;
using LogicReinc.Archive.Exceptions;
using Lucene.Net.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicReinc.Archive
{
    public class LRDocumentResult : LRDocument
    {
        public LRDocumentResult(float score, Document doc) : base(doc)
        {
            Initialized = false;
            Score = score;
        }

        public float Score { get; set; }
    }

    public class LRDocument
    {
        public static string[] Fields { get; } = new string[] { "ID", "Name", "Summary", "Tags", "Text" };

        public bool Initialized { get; protected set; } = true;

        public string ID { get; set; }

        public string Name { get; set; }
        public string Summary { get; set; }
        public List<string> Tags { get; set; }

        public string Text { get; set; }

        public LRDocument() { }

        public LRDocument(Document doc)
        {
            Initialized = false;
            ID = doc.GetField("ID").StringValue;
            Name = doc.GetField("Name").StringValue;
            Summary = doc.GetField("Summary").StringValue;
            Tags = doc.GetFields("Tags").Select(x => x.StringValue).ToList();
        }

        public FileStream Read(Archive archive)
        {
            return new FileStream(Path.Combine(archive.DocumentDirectory.FullName, ID), FileMode.Open);
        }

        public Document CreateIndexDocument()
        {
            Document doc = new Document();
            doc.Add(new Field("ID", this.ID, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Name", this.Name, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("Summary", this.Summary, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("Text", this.Text, Field.Store.NO, Field.Index.ANALYZED));

            foreach (string tag in this.Tags)
                doc.Add(new Field("Tags", tag, Field.Store.YES, Field.Index.ANALYZED));
            return doc;
        }

        public static LRDocument Archive(Archive archive, string name, string text) => Archive(archive, name, "", text);
        public static LRDocument Archive(Archive archive, string name, string summary, string text, params string[] tags)
        {
            using (MemoryStream str = new MemoryStream(Encoding.UTF8.GetBytes(text)))
                return Archive(archive, name, summary, "text/plain", str, tags);
        }
        public static LRDocument Archive(Archive archive, string name, string mime, Stream stream) => Archive(archive, name, "", mime, stream);
        public static LRDocument Archive(Archive archive, string name, string summary, string mime, Stream stream, params string[] tags)
        {
            LRDocument doc = new LRDocument();
            doc.ID = Guid.NewGuid().ToString();
            doc.Name = name;
            doc.Summary = summary;
            doc.Tags = tags.ToList();


            using (FileStream str = new FileStream(Path.Combine(archive.DocumentDirectory.FullName, doc.ID), FileMode.CreateNew))
            {
                byte[] buffer = new byte[4096];
                int read = 0;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    str.Write(buffer, 0, read);
            }

            stream.Seek(0, SeekOrigin.Begin);

            switch (mime)
            {
                case "text/plain":
                    byte[] tdata = new byte[stream.Length];
                    stream.Read(tdata, 0, tdata.Length);
                    doc.Text = Encoding.UTF8.GetString(tdata);
                    break;
                case "application/vnd.oasis.opendocument.text":
                    doc.Text = ODT.ToText(stream);
                    break;
                case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
                    doc.Text = DOCX.ToText(stream);
                    break;
                case "application/pdf":
                    doc.Text = PDF.ToText(stream);
                    break;
                default:
                    if(archive.FileExtractors.ContainsKey(mime))
                    {
                        doc.Text = archive.FileExtractors[mime].ToText(stream);
                        break;
                    }
                    throw new InvalidDocumentException("Document type not supported");
            }

            

            doc.CreateIndexDocument();

            return doc;
        }

    }
}
