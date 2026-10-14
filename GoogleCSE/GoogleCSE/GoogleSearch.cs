using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;

namespace GoogleCSE
{
    /// <summary>
    /// This represents a single search result
    /// </summary>
    public class GoogleSearchResult
    {
        /// <summary>
        /// The mime type or format of the resulting link
        /// </summary>
        public string Mime { get; set; }
        /// <summary>
        /// The URL of the result
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Title of the result
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Description or snippet that should show in results
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// A collection for storing search results and metadata.
    /// </summary>
    public class GoogleSearchResults
    {
        /// <summary>
        /// The organic search results
        /// </summary>
        public List<GoogleSearchResult> Results { get; set; }
        /// <summary>
        /// Promotional or advertisement results. Google Cloud Agent Search does not return promotions.
        /// </summary>
        public List<GoogleSearchResult> Promotions { get; set; }
        /// <summary>
        /// Total estimated results
        /// </summary>
        public long TotalResults { get; set; }
        /// <summary>
        /// Amount of time taken to perform the search (in seconds).
        /// </summary>
        public double SearchTime { get; set; }
    }

    /// <summary>
    /// Client for searching with the Google Cloud Agent Search API.
    /// </summary>
    public class GoogleSearch
    {
        private int _maxPages;

        /// <summary>
        /// Google Cloud project ID or project number for Agent Search.
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// Agent Search engine ID.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Datastore location/region (e.g., "global", "us", "eu"). Default is "global".
        /// </summary>
        public string Location { get; set; } = "global";

        private const string ServingConfigId = "default_search";

        /// <summary>
        /// GCP API key for authorization.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Results per page (default: 10).
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Offset starting index (1-based, default: 1).
        /// </summary>
        public int Start { get; set; } = 1;

        /// <summary>
        /// Maximum number of pages to retrieve.
        /// </summary>
        public int MaxPages
        {
            get => _maxPages;
            set => _maxPages = value;
        }

        /// <summary>
        /// <summary>
        /// Create a Google Cloud Agent Search client.
        /// </summary>
        /// <param name="projectId">Your GCP Project ID or Project Number</param>
        /// <param name="appId">Your Agent Search engine ID</param>
        /// <param name="apiKey">Your GCP API Key</param>
        /// <param name="location">(optional) GCP region/location (default: "global")</param>
        /// <param name="pageSize">(optional) Results per page, default 10</param>
        /// <param name="maxPages">(optional) Max pages to fetch, default 10</param>
        /// <param name="start">(optional) Offset index starting at 1, default 1</param>
        public GoogleSearch(
            string projectId,
            string appId,
            string apiKey,
            string location = "global",
            int pageSize = 10,
            int maxPages = 10,
            int start = 1)
        {
            if (string.IsNullOrWhiteSpace(projectId)) throw new ArgumentException("PROJECT_ID is required.", nameof(projectId));
            if (string.IsNullOrWhiteSpace(appId)) throw new ArgumentException("APP_ID is required.", nameof(appId));
            if (string.IsNullOrWhiteSpace(apiKey)) throw new ArgumentException("API_KEY is required.", nameof(apiKey));

            ProjectId = projectId;
            AppId = appId;
            ApiKey = apiKey;
            Location = string.IsNullOrEmpty(location) ? "global" : location;
            PageSize = pageSize > 0 ? pageSize : 10;
            _maxPages = maxPages > 0 ? maxPages : 10;
            Start = start > 0 ? start : 1;
        }

        /// <summary>
        /// Get the search results.
        /// </summary>
        /// <param name="term">Term to search</param>
        /// <param name="siteSearch">URL pattern that limits the search, such as "https://example.com/docs/*"</param>
        /// <returns>List of Search Results</returns>
        public List<GoogleSearchResult> Search(string term, string siteSearch = "")
        {
            return SearchVertexDetailed(term, siteSearch).Results;
        }

        /// <summary>
        /// Searches and returns detailed search results (including total count, execution time, etc.).
        /// </summary>
        /// <param name="term">Term to search</param>
        /// <param name="siteSearch">URL pattern that limits the search, such as "https://example.com/docs/*"</param>
        /// <returns>GoogleSearchResults object</returns>
        public GoogleSearchResults SearchDetailed(string term, string siteSearch = "")
        {
            return SearchVertexDetailed(term, siteSearch);
        }

        private GoogleSearchResults SearchVertexDetailed(string term, string siteSearch = "")
        {
            var ret = new GoogleSearchResults
            {
                Results = new List<GoogleSearchResult>(),
                Promotions = new List<GoogleSearchResult>(),
                TotalResults = 0,
                SearchTime = 0
            };

            var sw = System.Diagnostics.Stopwatch.StartNew();

            int startOffset = Start > 0 ? Start - 1 : 0;

            int currentOffset = startOffset;
            int pageCount = 0;

            while (pageCount < _maxPages)
            {
                VertexSearchResponse response = null;
                try
                {
                    response = FetchVertexPage(term, siteSearch, currentOffset, PageSize);
                }
                catch (WebException webEx)
                {
                    string errorDetails = webEx.Message;
                    if (webEx.Response != null)
                    {
                        try
                        {
                            using (var reader = new System.IO.StreamReader(webEx.Response.GetResponseStream()))
                            {
                                errorDetails += " Details: " + reader.ReadToEnd();
                            }
                        }
                        catch { }
                    }

                    if (pageCount == 0)
                    {
                        throw new Exception($"Google Agent Search API Error: {errorDetails}", webEx);
                    }
                    break;
                }
                catch (Exception ex)
                {
                    if (pageCount == 0)
                    {
                        throw new Exception($"Google Agent Search Error: {ex.Message}", ex);
                    }
                    break;
                }

                if (response == null) break;

                ret.TotalResults = response.TotalSize;

                if (response.Results == null || response.Results.Count == 0)
                {
                    break;
                }

                foreach (var item in response.Results)
                {
                    var mapped = MapVertexResultToGoogleSearchResult(item);
                    ret.Results.Add(mapped);
                }

                currentOffset += response.Results.Count;
                pageCount++;

                if (response.Results.Count < PageSize || currentOffset >= response.TotalSize)
                {
                    break;
                }
            }

            sw.Stop();
            ret.SearchTime = sw.Elapsed.TotalSeconds;

            return ret;
        }

        private VertexSearchResponse FetchVertexPage(string term, string siteSearch, int offset, int pageSize)
        {
            string url = GetVertexEndpointUrl();

            string queryText = term;
            string filterText = null;

            if (!string.IsNullOrWhiteSpace(siteSearch))
            {
                filterText = $"siteSearch:\"{EscapeFilterValue(siteSearch)}\"";
            }

            var requestBodyObj = new VertexSearchRequest
            {
                ServingConfig = GetServingConfigResourceName(),
                Query = queryText,
                PageSize = pageSize,
                Offset = offset,
                Filter = filterText,
                ContentSearchSpec = new VertexContentSearchSpec
                {
                    SnippetSpec = new VertexSnippetSpec { ReturnSnippet = true }
                }
            };

            string jsonBody = JsonConvert.SerializeObject(requestBodyObj);

            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json; charset=utf-8";

                string responseJson = webClient.UploadString(url, "POST", jsonBody);
                return JsonConvert.DeserializeObject<VertexSearchResponse>(responseJson);
            }
        }

        private static string EscapeFilterValue(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        public string GetServingConfigResourceName()
        {
            string location = string.IsNullOrEmpty(Location) ? "global" : Location;
            return $"projects/{ProjectId}/locations/{location}/collections/default_collection/engines/{AppId}/servingConfigs/{ServingConfigId}";
        }

        private string GetVertexEndpointUrl()
        {
            string resourceName = GetServingConfigResourceName();
            string baseUrl = $"https://discoveryengine.googleapis.com/v1/{resourceName}:searchLite";

            if (!string.IsNullOrEmpty(ApiKey))
            {
                baseUrl += "?key=" + HttpUtility.UrlEncode(ApiKey);
            }

            return baseUrl;
        }

        private GoogleSearchResult MapVertexResultToGoogleSearchResult(VertexSearchResultItem item)
        {
            var res = new GoogleSearchResult();
            if (item == null || item.Document == null) return res;

            JObject derived = item.Document.DerivedStructData != null ? JObject.FromObject(item.Document.DerivedStructData) : null;
            JObject structData = item.Document.StructData != null ? JObject.FromObject(item.Document.StructData) : null;

            // 1. Title
            res.Title = derived?["title"]?.ToString()
                     ?? structData?["title"]?.ToString()
                     ?? item.Document.Id
                     ?? string.Empty;

            // 2. URL
            res.Url = derived?["link"]?.ToString()
                   ?? derived?["url"]?.ToString()
                   ?? structData?["link"]?.ToString()
                   ?? structData?["url"]?.ToString()
                   ?? item.Document.Name
                   ?? string.Empty;

            // 3. Description / Snippet
            string snippet = null;
            if (derived?["snippets"] is JArray snippets && snippets.Count > 0)
            {
                snippet = snippets[0]?["snippet"]?.ToString() ?? snippets[0]?["htmlSnippet"]?.ToString();
            }
            if (string.IsNullOrEmpty(snippet) && derived?["extractive_answers"] is JArray answers && answers.Count > 0)
            {
                snippet = answers[0]?["content"]?.ToString();
            }
            if (string.IsNullOrEmpty(snippet))
            {
                snippet = structData?["description"]?.ToString() ?? structData?["snippet"]?.ToString();
            }
            res.Description = snippet ?? string.Empty;

            // 4. Mime
            res.Mime = derived?["mime"]?.ToString()
                    ?? derived?["fileFormat"]?.ToString()
                    ?? structData?["mime"]?.ToString()
                    ?? structData?["fileFormat"]?.ToString()
                    ?? string.Empty;

            return res;
        }

    }
}
