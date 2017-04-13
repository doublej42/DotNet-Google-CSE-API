using System;
using System.Linq;
using GoogleCSE;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SearchUnitTest
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TestMethodSimpleXml()
        {
            try
            {
                var gs = new GoogleSearch(TestConfigs.Cseid, maxPages: 2, pageSize: 20, method: GoogleSearchMethod.XML, key: TestConfigs.Key);
                var results = gs.Search("garbage");
                Assert.IsTrue(false); // fail is exception not thrown
            }
            catch (Exception e)
            {
                Assert.AreEqual("The XML API is no longer available as it was only for Google Site Search",e.Message);
            }
        }

        [TestMethod]
        public void TestSimpleCse()
        {
            var gs = new GoogleSearch(TestConfigs.Cseid, maxPages: 2, pageSize: 10, key: TestConfigs.Key, method: GoogleSearchMethod.CSE);
            Assert.IsFalse(gs.Options.ContainsKey("start"));
            var results = gs.Search("dam");
            Assert.IsFalse(gs.Options.ContainsKey("start"));
            Assert.IsTrue(results.Any());
            Assert.IsTrue(results.Count <= 20);
            Assert.IsTrue(results.Count > 10);
            results = gs.Search("dam", "Businesses");
            Assert.IsTrue(results.Count > 1);
            Assert.IsTrue(results.Count < 100);
        }


        [TestMethod]
        public void TestLargeCse()
        {
            var gs = new GoogleSearch(TestConfigs.Cseid, maxPages: 200, pageSize: 10, key: TestConfigs.Key, method: GoogleSearchMethod.CSE);
            Assert.IsFalse(gs.Options.ContainsKey("start"));
            var results = gs.Search("the");
            Assert.IsFalse(gs.Options.ContainsKey("start"));
            Assert.IsTrue(results.Any());
            Assert.IsTrue(results.Count == 100);
        }


        [TestMethod]
        public void TestMethodCseDetails()
        {
            var gs = new GoogleSearch(TestConfigs.Cseid, maxPages: 2,key: TestConfigs.Key, method: GoogleSearchMethod.CSE );
            Assert.IsFalse(gs.Options.ContainsKey("start"));
            var results = gs.SearchDetailed("garbage");
            Assert.IsFalse(gs.Options.ContainsKey("start"));

            Assert.IsTrue(results.Results.Any());
            Assert.IsTrue(results.Results.Count <= 20);
            Assert.IsTrue(results.Results.Count > 10);
            Assert.IsTrue(results.Promotions.Any(r => r.Description.Contains("garbage")));
            Assert.IsTrue(results.Labels.ContainsKey("businesses"));
            gs.Options["start"] = "11";
            var results2 = gs.SearchDetailed("garbage");
            Assert.IsTrue(gs.Options["start"] == "11");
            Assert.AreEqual(results.Results[10].Url,results2.Results[0].Url);
            gs.Options.Remove("start");
            results = gs.SearchDetailed("dam", "Businesses");
            Assert.IsTrue(results.Results.Any());
            Assert.IsTrue(results.Results.Count == results.TotalResults);
        }

       

        

    }
}
