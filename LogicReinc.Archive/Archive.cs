using Lucene.Net.Documents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LogicReinc.Archive
{
    public class Archive
    {
        public string Directory { get; private set; }
        private string Password { get; set; }
        
        public DirectoryInfo RootDirectory { get; private set; }
        public DirectoryInfo IndexDirectory { get; private set; }
        public DirectoryInfo DocumentDirectory { get; private set; }

        public LuceneService Lucene { get; private set; }

        public Archive(string directory, string password = "")
        {
            if (string.IsNullOrEmpty(directory))
                directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Directory = directory;
            Password = password;

            RootDirectory = new DirectoryInfo(directory);
            if (!RootDirectory.Exists)
                RootDirectory.Create();

            IndexDirectory = new DirectoryInfo(Path.Combine(directory, "Indexes"));
            if (!IndexDirectory.Exists)
                IndexDirectory.Create();

            DocumentDirectory = new DirectoryInfo(Path.Combine(directory, "Documents"));
            if (!DocumentDirectory.Exists)
                DocumentDirectory.Create();

            Lucene = new LuceneService(IndexDirectory.FullName, password);
        }

        public void Close()
        {
            Lucene.Close();
        }

        public void Add(LRDocument document)
        {
            Lucene.AddIndex(document);
        }

        public LRDocument Get(string id)
        {
            Document doc = Lucene.GetDocument(id);
            if (doc != null)
                return new LRDocument(doc);
            return null;
        }
        
        public List<LRDocumentResult> Search(params string[] queries)
        {
            return Lucene.Find(LRDocument.Fields, queries).OrderByDescending(x => x.Score).ToList();
        }

        public List<LRDocumentResult> Search(string text)
        {
            return Lucene.Find(LRDocument.Fields, text).OrderByDescending(x => x.Score).ToList();
        }

        public List<LRDocumentResult> SearchTags(params string[] tags)
        {
            return Lucene.Find(new string[] { "Tags" }, tags).OrderByDescending(x => x.Score).ToList();
        }


        public FileStream Read(string id)
        {
            return Get(id)?.Read(this) ?? null;
        }
    }
}
