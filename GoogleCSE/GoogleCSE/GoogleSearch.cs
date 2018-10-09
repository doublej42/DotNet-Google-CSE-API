using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace GoogleCSE
{
    /// <summary>
    /// This represents a single search result
    /// </summary>
    public class GoogleSearchResult
    {
        /// <summary>
        /// The mine type of the resulting link
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
        /// Description that should show in results
        /// </summary>
        public string Description { get; set; }
    }

    /// <summary>
    /// A collection for storing more advanced information about the search.
    /// </summary>
    public class GoogleSearchResults
    {
        /// <summary>
        /// The normal organic search results
        /// </summary>
        public List<GoogleSearchResult> Results { get; set; }
        /// <summary>
        /// Specially designed search results. Advertisements. 
        /// </summary>
        public List<GoogleSearchResult> Promotions { get; set; }
        /// <summary>
        /// Possible search refinement labels 
        /// </summary>
        public Dictionary<string, string> Labels { get; set; }
        /// <summary>
        /// Total results, all these results might not actually be available.
        /// </summary>
        public long TotalResults { get; set; }
        /// <summary>
        /// Amount of time taken to preform the search.
        /// </summary>
        public double SearchTime { get; set; }
    }


    // ReSharper disable InconsistentNaming
    /// <summary>
    /// No longer used, left in as to not change the api.
    /// </summary>
    public enum GoogleSearchMethod { XML, CSE };
    // ReSharper restore InconsistentNaming

    /// <summary>
    /// Class to instantiate for searching a Google custom search engine. You will need to provide your cx
    /// XML Docs: https://developers.google.com/custom-search/docs/xml_results
    /// See Also Google api docs at : https://developers.google.com/custom-search/json-api/v1/overview
    /// API Details: https://developers.google.com/custom-search/json-api/v1/reference/cse/list
    /// You can get your CSE from https://www.google.com/cse/
    /// </summary>
    public class GoogleSearch
    {
        private const string GoogleUrl = "http://www.google.com/search";
        private const string GoogleCseUrl = "https://www.googleapis.com/customsearch/v1";
        private readonly int _maxPages;

        /// <summary>
        /// The API used. This was the XML API but will likely move towards the CSE Atom api as a default in the future. XML gives a larger page size.
        /// </summary>
        public GoogleSearchMethod Method;

        /// <summary>
        /// Options used to build the URL. 
        /// </summary>
        public readonly Dictionary<string, string> Options = new Dictionary<string, string>();

        /// <summary>
        /// Create a Google searcher. 
        /// </summary>
        /// <param name="cx">Your search engine ID from https://www.google.com/cse/ </param>
        /// <param name="hl">(optional) language</param>
        /// <param name="extraOptions">(optional)If you need to supply more custom options passed to google please include them here. These options will override any defaults</param>
        /// <param name="pageSize">(optional) Number of results per page, Default are 10, this is also the max</param>
        /// <param name="maxPages">(optional) Number of pages to grab. Larger number will be slower.  Also each page is a new search on your paid search limit. Default is 10 as this will give you the max 100 results.</param>
        /// <param name="start">The result offset, use to page through results. WARNING: CSE starts at 1 but XML starts at zero. If you want the 11th result (page 2) on CSE set this to 11. On XML set this to 10 </param>
        /// <param name="method">(optional) API Used to gather the results. Default isCSE https://developers.google.com/custom-search/json-api/v1/reference/cse/list the other option was XML but is no longer supported by google. https://developers.google.com/custom-search/docs/xml_results 
        /// Only the CSE interface gives promotions as a separate object. Note this is a change of default from old versions.
        /// </param>
        /// <param name="key">Api Key , needed if you want more than 100 queries per day https://console.developers.google.com/apis/credentials </param>
        /// <param name="userIp">The Users Ip for imposing per user limits https://support.google.com/cloud/answer/7035610 </param>
        /// <param name="quotaUser">The users unique name for capping by non web request. https://support.google.com/cloud/answer/7035610 </param>
        public GoogleSearch(string cx, string key, string hl = "en", Dictionary<string, string> extraOptions = null, int pageSize = 10, int maxPages = 10, int start = 1, GoogleSearchMethod method = GoogleSearchMethod.CSE, string userIp = null, string quotaUser = null)
        {

            Options["client"] = "google-csbe";
            Options["cx"] = cx;
            Options["hl"] = hl;
            Options["num"] = pageSize.ToString(CultureInfo.InvariantCulture);
            if (start > 1)
            {
                Options["start"] = start.ToString(CultureInfo.InvariantCulture);
            }
            if (!string.IsNullOrEmpty(key))
            {
                Options["key"] = key;
            }
            if (!string.IsNullOrEmpty(userIp))
            {
                Options["userIP"] = key;
            }
            if (!string.IsNullOrEmpty(quotaUser))
            {
                Options["quotaUser"] = key;
            }
            _maxPages = maxPages;
            if (_maxPages * pageSize > 100)
            {
                //will crash if you ask for more than the first 100 results. This is a Google change.
                _maxPages = 100 / pageSize;
            }
            Method = method;
            if (extraOptions == null) return;
            foreach (var option in extraOptions)
            {
                Options[option.Key] = option.Value;
            }
        }

        /// <summary>
        /// Build a query URL
        /// </summary>
        /// <returns>string: The URL used by the various search functions</returns>
        protected string QueryUrl()
        {
            var sb = new StringBuilder();
            sb.Append(GoogleCseUrl);
            if (Options.ContainsKey("start"))
            {
                int start;
                if (int.TryParse(Options["start"], out start))
                {
                    if (start >= 100)
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                    start = start - 1; // zero indexed
                    var num = int.Parse(Options["num"]);
                    if (start + num > 100) //Google 100 result limit cases error 400
                    {
                        Options["num"] = (100 - start).ToString();
                    }
                }
            }
            for (var i = 0; i < Options.Keys.Count; i++)
            {
                var key = Options.Keys.ElementAt(i);
                sb.Append(i == 0 ? '?' : '&');
                sb.Append(key);
                sb.Append('=');
                sb.Append(HttpUtility.UrlEncode(Options[key]));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get the search results 
        /// </summary>
        /// <param name="term">Term to search</param>
        /// <param name="label">If you are using custom refinements this will help you search part of your site</param>
        /// <returns>List of Search Results</returns>
        public List<GoogleSearchResult> Search(string term, string label = "")
        {

            if (!string.IsNullOrWhiteSpace(label))
            {
                Options["q"] = term + " more:" + label;
            }
            else
            {
                Options["q"] = term;
            }
            var url = QueryUrl();
            return RecursiveResults(url);
        }

        /// <summary>
        /// Searches Google and returns a Google search results object. This includes details about refinements (labels) and total results, time to search (CSE only) and promotional results.
        /// </summary>
        /// <param name="term">Term to search</param>
        /// <param name="label">If you are using custom refinements this will help you search part of your site</param>
        /// <returns>GoogleSearchResults object. See object for more details</returns>
        public GoogleSearchResults SearchDetailed(string term, string label = "")
        {
            if (!string.IsNullOrWhiteSpace(label))
            {
                Options["q"] = term + " more:" + label;
            }
            else
            {
                Options["q"] = term;
            }
            var url = QueryUrl();
            return RecursiveResultDetailed(url);
        }

        /// <summary>
        /// Used to search multiple pages and combine them with details
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private GoogleSearchResults RecursiveResultDetailed(string url)
        {
            var ret = new GoogleSearchResults
            {
                Results = new List<GoogleSearchResult>(),
                Promotions = new List<GoogleSearchResult>(),
                Labels = new Dictionary<string, string>(),
                TotalResults = 0,
                SearchTime = 0
            };

            GoogleJsonFormat jResult = null;
            try
            {
                var json = new WebClient().DownloadString(url);
                jResult = JsonConvert.DeserializeObject<GoogleJsonFormat>(json);
            }
            catch (Exception)
            {
                //This happens if you reach your daily or user limit 
            }

            if (jResult != null)
            {
                //handle the first page
                ret.Results = jResult.items.Select(r => new GoogleSearchResult()
                {
                    Mime = r.mime,
                    Url = r.link,
                    Title = r.title,
                    Description = r.snippet
                }).ToList();

                if (jResult.queries.nextPage != null && jResult.queries.nextPage.Any())
                {
                    //there is a next page
                    string oldstart = null;
                    if (Options.ContainsKey("start"))
                    {
                        oldstart = Options["start"];
                    }
                    Options["start"] = jResult.queries.nextPage[0].startIndex.ToString();
                    try
                    {
                        //let the simpler function handle the next page.
                        var nextUrl = QueryUrl();
                        var theRestOfThePages = RecursiveResults(nextUrl, 2);
                        ret.Results.AddRange(theRestOfThePages);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        //it won't search past 100 results
                    }
                    if (oldstart != null)
                    {
                        Options["start"] = oldstart;
                    }
                    else
                    {
                        Options.Remove("start");
                    }
                }

                if (jResult.promotions != null)
                {
                    ret.Promotions = jResult.promotions.Select(r => new GoogleSearchResult()
                    {
                        Mime = "text/html",
                        Url = r.link,
                        Title = r.title,
                        Description = r.bodyLines[0].title
                    }).ToList();
                }

                if (jResult.context.facets != null)
                {
                    foreach (var labelGroup in jResult.context.facets)
                    {
                        foreach (var label in labelGroup)
                        {
                            ret.Labels.Add(label.label, label.anchor);
                        }
                    }
                }
                ret.TotalResults = long.Parse(jResult.searchInformation.totalResults);
                ret.SearchTime = jResult.searchInformation.searchTime;
            }
            return ret;
        }



        /// <summary>
        /// Walk through pages of results
        /// </summary>
        /// <param name="url"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private List<GoogleSearchResult> RecursiveResults(string url, int depth = 1)
        {
            var ret = new List<GoogleSearchResult>();
            GoogleJsonFormat jResult = null;
            try
            {
                var json = new WebClient().DownloadString(url);
                jResult = JsonConvert.DeserializeObject<GoogleJsonFormat>(json);
            }
            catch (Exception)
            {
                //This happens if you reach your daily or user limit 
            }

            if (jResult != null)
            {
                ret = jResult.items.Select(r => new GoogleSearchResult()
                {
                    Mime = r.mime,
                    Url = r.link,
                    Title = r.title,
                    Description = r.snippet
                }).ToList();

                if (depth < _maxPages && jResult.queries.nextPage != null && jResult.queries.nextPage.Any())
                {
                    //there is a next page
                    string oldstart = null;
                    if (Options.ContainsKey("start"))
                    {
                        oldstart = Options["start"];
                    }
                    Options["start"] = jResult.queries.nextPage[0].startIndex.ToString();
                    try
                    {
                        var nextUrl = QueryUrl();
                        var theRestOfThePages = RecursiveResults(nextUrl, ++depth);
                        ret.AddRange(theRestOfThePages);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        //it won't search past 100 results
                    }
                    if (oldstart != null)
                    {
                        Options["start"] = oldstart;
                    }
                    else
                    {
                        Options.Remove("start");
                    }
                }
                return ret;
            }
            return ret;
        }


    }

}
