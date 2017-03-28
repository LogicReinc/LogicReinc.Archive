using LogicReinc.Archive.Components;
using LogicReinc.Archive.DocumentTypes;
using LogicReinc.Archive.Exceptions;
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

        internal Dictionary<string, IDocTypeExtractor> FileExtractors { get; } = new Dictionary<string, IDocTypeExtractor>();

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

        public void RegisterExtractor<T>(string mime) where T : IDocTypeExtractor
        {
            FileExtractors.Add(mime, (IDocTypeExtractor)Activator.CreateInstance<T>());
        }

        public void Add(LRDocument document)
        {
            Lucene.AddIndex(document);
        }

        public LRDocument ProcessFromPath(string name, string path)
            => ProcessFromPath(name, "", path);
        public LRDocument ProcessFromPath(string name, string summary, string path, params string[] tags)
        {
            string ext = Path.GetExtension(path).Trim('.');
            if (!FileTypes.MimeMap.ContainsKey(ext))
                throw new InvalidDocumentException("Extension not recognized, add DocTypeExtractor/MimeMap entry for custom types");

            string mime = FileTypes.MimeMap[ext];

            using (FileStream stream = new FileStream(path, FileMode.Open))
                return Process(name, summary, mime, stream, tags);
        }

        public LRDocument Process(string name, string text)
            => LRDocument.Archive(this, name, text);
        public LRDocument Process(string name, string summary, string text, params string[] tags)
            => LRDocument.Archive(this, name, summary, text, tags);
        public LRDocument Process(string name, string mime, Stream stream)
            => LRDocument.Archive(this, name, "", mime, stream);
        public LRDocument Process(string name, string summary, string mime, Stream stream, params string[] tags)
            => LRDocument.Archive(this, name, summary, mime, stream, tags);

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
