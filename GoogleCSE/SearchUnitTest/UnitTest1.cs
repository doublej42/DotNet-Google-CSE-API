using System;
using GoogleCSE;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SearchUnitTest
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void ConstructorUsesRequiredVertexCredentials()
        {
            var search = new GoogleSearch("project", "app", "key");

            Assert.AreEqual("project", search.ProjectId);
            Assert.AreEqual("app", search.AppId);
            Assert.AreEqual("key", search.ApiKey);
        }

        [TestMethod]
        public void ConstructorRejectsMissingCredentials()
        {
            AssertMissingCredential("", "app", "key");
            AssertMissingCredential("project", "", "key");
            AssertMissingCredential("project", "app", "");
        }

        [TestMethod]
        public void ServingConfigUsesAppId()
        {
            var search = new GoogleSearch("project", "app", "key", location: "eu");

            Assert.AreEqual(
                "projects/project/locations/eu/collections/default_collection/engines/app/servingConfigs/default_search",
                search.GetServingConfigResourceName());
        }

        private static void AssertMissingCredential(string projectId, string appId, string apiKey)
        {
            try
            {
                new GoogleSearch(projectId, appId, apiKey);
                Assert.Fail("Expected missing credentials to be rejected.");
            }
            catch (ArgumentException)
            {
            }
        }

    }
}
