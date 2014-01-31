using System;
using System.Linq;
using GoogleCSE;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SearchUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var gs = new GoogleSearch(TestConfigs.Cseid, maxPages: 2); 
            var results = gs.Search("dam");
            Assert.IsTrue(results.Any());
            Assert.IsTrue(results.Count <= 40);
            results = gs.Search("dam","Businesses");
            Assert.IsTrue(results.Count == 2);
        }
    }
}
