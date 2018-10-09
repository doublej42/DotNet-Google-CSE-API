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
        public void TestNoResults()
        {
            var gs = new GoogleSearch(TestConfigs.Cseid, maxPages: 2, pageSize: 10, key: TestConfigs.Key);
            var results = gs.Search("dfhgjkdfhgdfjkbgjkerbgdfuibvefuibvuybruyfbruo");
            Assert.IsFalse(gs.Options.ContainsKey("start"));
            Assert.IsFalse(results.Any());
        }


        [TestMethod]
        public void TestUserIpCse()
        {
            var gs = new GoogleSearch(TestConfigs.Cseid, maxPages: 1, pageSize: 5, key: TestConfigs.Key, method: GoogleSearchMethod.CSE,userIp: "192.168.1.101" );
            var results = gs.Search("dam");
            Assert.IsTrue(results.Any());
            Assert.IsTrue(results.Count == 5);
        }


        [TestMethod]
        public void TestSpecialCaseCse()
        {
            var gs = new GoogleSearch(TestConfigs.Cseid, maxPages: 1, pageSize: 5, key: TestConfigs.Key);
            gs.Options.Add("fileType","pdf");
            var results = gs.Search("dam");
            Assert.IsTrue(results.Any());
            Assert.IsTrue(results.Count == 5);
            Assert.IsTrue(results.All(r => r.Url.Contains(".pdf")));
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

            gs.Options["start"] = "85"; // first results is 85th, so 16
            results = gs.Search("the");
            Assert.IsTrue(results.Count == 16);
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
