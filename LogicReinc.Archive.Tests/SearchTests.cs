using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;

namespace LogicReinc.Archive.Tests
{
    [TestClass]
    public class SearchTests
    {

        public static Archive archive = new Archive("");

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            LRDocument doc = new LRDocument()
            {
                ID = Guid.NewGuid().ToString(),
                Name = "Rhinos",
                Tags = new List<string>()
                {
                    "Rhino",
                    "Animal",
                    "Africa"
                },
                Text = @"
A rhinoceros (/raɪˈnɒsərəs/, meaning 'nose horn'), often abbreviated to rhino, is one of any five extant species of odd-toed ungulates in the family Rhinocerotidae, as well as any of the numerous extinct species. 
Two of these extant species are native to Africa and three to Southern Asia.
Members of the rhinoceros family are characterized by their large size(they are some of the largest remaining megafauna, with all of the species able to reach one tonne or more in weight); as well as by an herbivorous diet; a thick protective skin, 1.5–5 cm thick, formed from layers of collagen positioned in a lattice structure; relatively small brains for mammals this size(400–600 g); and a large horn.They generally eat leafy material, although their ability to ferment food in their hindgut allows them to subsist on more fibrous plant matter, if necessary.Unlike other perissodactyls, the two African species of rhinoceros lack teeth at the front of their mouths, relying instead on their lips to pluck food.[1]"
            };

            LRDocument doc2 = new LRDocument()
            {
                ID = Guid.NewGuid().ToString(),
                Name = "Africa",
                Tags = new List<string>()
                {
                    "Continent",
                    "Poor",
                    "Savage"
                },
                Text = @"
Africa is the world's second-largest and second-most-populous continent. 
At about 30.3 million km² (11.7 million square miles) including adjacent islands, it covers 6% of Earth's total surface area and 20.4 % of its total land area.[2] With 1.2 billion people as of 2016, it accounts for about 16% of the world's human population.[1] The continent is surrounded by the Mediterranean Sea to the north, both the Suez Canal and the Red Sea along the Sinai Peninsula to the northeast, the Indian Ocean to the southeast, and the Atlantic Ocean to the west. 
The continent includes Madagascar and various archipelagos. It contains 54 fully recognized sovereign states (countries), nine territories and two de facto independent states with limited or no recognition.[3]"
            };

            LRDocument doc3 = new LRDocument()
            {
                ID = Guid.NewGuid().ToString(),
                Name = "DEF CON",
                Tags = new List<string>()
                {
                    "Security",
                    "Information",
                    "Technology",
                    "Hackers"
                },
                Text = @"
DEF CON (also written as DEFCON, Defcon, or DC) is one of the world's largest annual hacker conventions, held annually in Las Vegas, Nevada, with the first DEF CON taking place in June 1993. 
Many of the attendees at DEF CON include computer security professionals, journalists, lawyers, federal government employees, security researchers, students, and hackers with a general interest in software, computer architecture, phone phreaking, hardware modification, and anything else that can be 'hacked.' 
The event consists of several tracks of speakers about computer- and hacking-related subjects, as well as social events Wargames and contests in everything from creating the longest Wi-Fi connection and hacking computer systems to who can most effectively cool a beer in the Nevada heat."
            };


            archive.Add(doc);
            archive.Add(doc2);
            archive.Add(doc3);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            archive.Close();
            Thread.Sleep(1000);
            archive.RootDirectory.Delete();
        }

        [TestMethod]
        public void SearchWords()
        {

            List<LRDocumentResult> result = archive.Search("Africa");

            foreach (LRDocumentResult r in result)
                Console.WriteLine($"Found: {r.Name} with score {r.Score}");

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void SearchText()
        {

            List<LRDocumentResult> result = archive.Search("computer");

            foreach (LRDocumentResult r in result)
                Console.WriteLine($"Found: {r.Name} with score {r.Score}");

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void SearchGroup()
        {

            List<LRDocumentResult> result = archive.Search("computer", "Rhinos");

            foreach (LRDocumentResult r in result)
                Console.WriteLine($"Found: {r.Name} with score {r.Score}");

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void SearchStringGroup()
        {

            List<LRDocumentResult> result = archive.Search("computer Rhinos");

            foreach (LRDocumentResult r in result)
                Console.WriteLine($"Found: {r.Name} with score {r.Score}");

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void SearchTags()
        {

            List<LRDocumentResult> result = archive.SearchTags("Africa");

            foreach (LRDocumentResult r in result)
                Console.WriteLine($"Found: {r.Name} with score {r.Score}");

            Assert.AreEqual(1, result.Count);
        }
    }
}
