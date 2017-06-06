using LogicReinc.Archive.Components;
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
        internal static string[] SearchFields { get; } = new string[] { "ID", "Name", "Summary", "Tags", "Text" };

        public bool Initialized { get; protected set; } = true;

        public string ID { get; set; }

        public string Name { get; set; }
        public string FilePath { get; set; }
        public string Summary { get; set; }
        public List<string> Tags { get; set; } = new List<string>();

        public string Text { get; set; }

        public LRDocument() { }

        public LRDocument(Document doc)
        {
            Initialized = false;
            ID = doc.GetField("ID").StringValue;
            Name = doc.GetField("Name").StringValue;
            Summary = doc.GetField("Summary")?.StringValue;
            FilePath = doc.GetField("FilePath")?.StringValue;
            Tags = doc.GetFields("Tags").Select(x => x.StringValue).ToList();
        }

        public Stream Read(Archive archive)
        {
            if (string.IsNullOrEmpty(archive.Settings.FileEncryptionPassword))
                return new FileStream(Path.Combine(archive.DocumentDirectory.FullName, ID), FileMode.Open);
            else
                return Encryption.CreateDecryptStream(
                    new FileStream(Path.Combine(archive.DocumentDirectory.FullName, ID), FileMode.Open), archive.Settings.FileEncryptionPassword, Encryption.Salt);
        }

        public Document CreateIndexDocument()
        {
            Document doc = new Document();
            doc.Add(new Field("ID", this.ID, Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Name", this.Name, Field.Store.YES, Field.Index.ANALYZED));
            if (!string.IsNullOrEmpty(this.FilePath))
                doc.Add(new Field("FilePath", this.FilePath, Field.Store.YES, Field.Index.NO));
            if (!string.IsNullOrEmpty(this.Summary))
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
        public static LRDocument Archive(Archive archive, string name, string mime, Stream stream) 
            => Archive(archive, name, "", mime, stream);
        public static LRDocument Archive(Archive archive, string name, string summary, string mime, Stream stream, params string[] tags)
            => Archive(archive, name, summary, mime, null, stream, tags);
        public static LRDocument Archive(Archive archive, string name, string summary, string mime, string textOverride, Stream stream, params string[] tags)
            => Archive(archive, null, name, summary, mime, textOverride, stream, tags);
        public static LRDocument Archive(Archive archive, string id, string name, string summary, string mime, string textOverride, Stream stream, params string[] tags)
        {

            LRDocument doc = new LRDocument();
            doc.ID = (id != null) ? id : Guid.NewGuid().ToString();
            doc.Name = name;
            doc.Summary = summary;
            doc.Tags = tags.ToList();

            if (!string.IsNullOrEmpty(textOverride))
                doc.Text = textOverride;

            if (string.IsNullOrEmpty(archive.Settings.FileEncryptionPassword))
                using (FileStream str = new FileStream(archive.BuildFilePath(doc.ID), FileMode.Create))
                {
                    byte[] buffer = new byte[4096];
                    int read = 0;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                        str.Write(buffer, 0, read);
                }
            else
                using (FileStream str = new FileStream(archive.BuildFilePath(doc.ID), FileMode.Create))
                    Encryption.EncryptStream(stream, str, archive.Settings.FileEncryptionPassword);

            if (string.IsNullOrEmpty(textOverride))
            {
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
                        if (archive.FileExtractors.ContainsKey(mime))
                        {
                            doc.Text = archive.FileExtractors[mime].ToText(stream);
                            break;
                        }
                        throw new InvalidDocumentException("Document type not supported");
                }
            }



            archive.Add(doc);

            return doc;
        }
    }
}
