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
        public ArchiveSettings Settings { get; private set; }
        
        public DirectoryInfo RootDirectory { get; private set; }
        public DirectoryInfo IndexDirectory { get; private set; }
        public DirectoryInfo DocumentDirectory { get; private set; }

        public LuceneService Lucene { get; private set; }

        internal Dictionary<string, IDocTypeExtractor> FileExtractors { get; } = new Dictionary<string, IDocTypeExtractor>();

        public Archive(ArchiveSettings settings)
        {
            Initialize(settings);
        }
        public Archive(string directory)
        {
            Initialize(new ArchiveSettings()
            {
                Directory = directory
            });
        }


        private void Initialize(ArchiveSettings settings)
        {
            Settings = settings;
            
            if (string.IsNullOrEmpty(Settings.Directory))
                Settings.Directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            
            RootDirectory = new DirectoryInfo(Settings.Directory);
            if (!RootDirectory.Exists)
                RootDirectory.Create();

            IndexDirectory = new DirectoryInfo(Path.Combine(Settings.Directory, "Indexes"));
            if (!IndexDirectory.Exists)
                IndexDirectory.Create();

            DocumentDirectory = new DirectoryInfo(Path.Combine(Settings.Directory, "Documents"));
            if (!DocumentDirectory.Exists)
                DocumentDirectory.Create();

            Lucene = new LuceneService(IndexDirectory.FullName);
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
            if (string.IsNullOrEmpty(document.ID))
                document.ID = Guid.NewGuid().ToString();
            if (Settings.HashTags)
                document.Tags = Hashing.HashTags(document.Tags.ToArray()).ToList();
            Lucene.AddIndex(document.CreateIndexDocument());
        }

        public void Add(List<LRDocument> documents)
        {
            foreach(LRDocument document in documents)
            {
                if (string.IsNullOrEmpty(document.ID))
                    document.ID = Guid.NewGuid().ToString();
                if (Settings.HashTags)
                    document.Tags = Hashing.HashTags(document.Tags.ToArray()).ToList();
            }
            Lucene.AddIndexes(documents.Select(x => x.CreateIndexDocument()).ToList());
        }

        public void Remove(string id, bool deleteFile)
        {
            string path = BuildFilePath(id);

            Lucene.RemoveIndex(id);
            if (deleteFile && File.Exists(path))
                File.Delete(path);
        }

        public string BuildFilePath(string id)
        {
            return Path.Combine(DocumentDirectory.FullName, id);
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
        
        public List<LRDocumentResult> SearchQuery(string query)
        {
            return Lucene.Query(query);
        }

        public List<LRDocumentResult> Search(params string[] queries)
        {
            return Lucene.Find(LRDocument.SearchFields, queries).OrderByDescending(x => x.Score).ToList();
        }
        public List<LRDocumentResult> Search(string text)
        {
            return Lucene.Find(LRDocument.SearchFields, text).OrderByDescending(x => x.Score).ToList();
        }
        public List<LRDocumentResult> SearchTags(params string[] tags)
        {
            return Lucene.Find(new string[] { "Tags" }, (!Settings.HashTags) ? tags : Hashing.HashTags(tags)).OrderByDescending(x => x.Score).ToList();
        }
        
        public Stream Read(string id)
        {
            return Get(id)?.Read(this) ?? null;
        }


        public int GetDocumentCount()
        {
            return Lucene.GetDocumentCount();
        }

        public void SetGlobalSalt(string salt)
        {
            Encryption.Salt = salt;
        }
    }
}
