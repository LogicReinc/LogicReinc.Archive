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
    }
}
