﻿using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LDirectory = Lucene.Net.Store.Directory;

namespace LogicReinc.Archive
{
    public class LuceneService : IDisposable
    {
        private Analyzer Analyzer { get; set; }

        public IndexWriter Writer { get; private set; }
        public LDirectory IndexDirectory { get; private set; }
        public string Path { get; private set; }

        public LuceneService(string directory, string localisation = "")
        {
            Path = directory;
            Initialize(localisation);
        }

        public void Initialize(string localisation)
        {
            switch (localisation)
            {
                default:
                    Analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
                    break;
            }

            if(Directory.Exists(Path))
            {
                IndexDirectory = Lucene.Net.Store.FSDirectory.Open(Path);
                Writer = new IndexWriter(IndexDirectory, Analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED);
            }
        }

        public void AddIndex(Document document)
        {
            Writer.AddDocument(document);
            Writer.Commit();
        }

        public void RemoveIndex(string id)
        {
            TermQuery query = new TermQuery(new Term("ID", id));
            Writer.DeleteDocuments(query);
            Writer.Commit();
        }

        public Document GetDocument(string id)
        {
            TermQuery query = new TermQuery(new Term("ID", id));
            using (IndexReader reader = Writer.GetReader())
            using (IndexSearcher searcher = new IndexSearcher(reader))
            {
                TopDocs docs = searcher.Search(query, (reader.MaxDoc > 0) ? reader.MaxDoc : 1);
                int? did = docs.ScoreDocs.FirstOrDefault()?.Doc;
                if (did != null)
                    return reader.Document(did.Value);
                return null;
            }
        }

        public List<LRDocumentResult> Find(string[] fields, string text)
        {
            using (IndexReader reader = Writer.GetReader())
            using (IndexSearcher searcher = new IndexSearcher(reader))
            {
                MultiFieldQueryParser parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, fields, Analyzer);

                Query query = parser.Parse(text);
                TopDocs docs = searcher.Search(query, (reader.MaxDoc > 0) ? reader.MaxDoc : 1);
                return docs.ScoreDocs.Select(x => new LRDocumentResult(x.Score, reader.Document(x.Doc))).ToList();
            }
        }
        public List<LRDocumentResult> Find(string[] fields, params string[] keywords)
        {
            using (IndexReader reader = Writer.GetReader())
            using (IndexSearcher searcher = new IndexSearcher(reader))
            {
                MultiFieldQueryParser parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, fields, Analyzer);
                BooleanQuery query = new BooleanQuery();
                foreach (string keyword in keywords)
                    query.Add(parser.Parse(keyword), Occur.SHOULD);

                TopDocs docs = searcher.Search(query, (reader.MaxDoc > 0) ? reader.MaxDoc : 1);
                return docs.ScoreDocs.Select(x => new LRDocumentResult(x.Score, reader.Document(x.Doc))).ToList();
            }
        }

        public void Optimize()
        {
            Writer.Optimize();
        }
        
        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            Writer.Commit();
            Writer.Dispose();
            Analyzer.Dispose();
            IndexDirectory.Dispose();
        }
    }
}
