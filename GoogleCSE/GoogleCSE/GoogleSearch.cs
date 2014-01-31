using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        /// Description that shoudl show in results
        /// </summary>
        public string Description { get; set; }
    }


    /// <summary>
    /// Class to instantiate for searching a google custom search engine. You will need to provide your cx
    /// https://developers.google.com/custom-search/docs/xml_results?hl=en&csw=1
    /// You can get your CSE from https://www.google.com/cse/
    /// </summary>
    public class GoogleSearch
    {
        private const string GoogleUrl = "http://www.google.com/search";
        private readonly int _maxPages;

        /// <summary>
        /// Options used to build the URL. 
        /// </summary>
        public readonly Dictionary<string, string> Options = new Dictionary<string, string>();

        /// <summary>
        /// Create a google searcher
        /// </summary>
        /// <param name="cx">Your search engine ID from https://www.google.com/cse/ </param>
        /// <param name="hl">(optional) language</param>
        /// <param name="extraOptions">(optional)If you need to supply more custom options passed to google please include them here. These options will override any defaults</param>
        /// <param name="pageSize">(optional) Numeber of results per page, max and default are 20</param>
        /// <param name="maxPages">(optional) Number of pages to grab. Larger number will be slower.  Also each page is a new search on your paid search limit.</param>
        public GoogleSearch(string cx, string hl = "en",Dictionary<string, string> extraOptions = null,int pageSize = 20, int maxPages = 20)
        {
            Options["client"] = "google-csbe";
            Options["output"] = "xml_no_dtd";
            Options["cx"] = cx;
            Options["hl"] = hl;
            Options["num"] = pageSize.ToString(CultureInfo.InvariantCulture);
            _maxPages = maxPages;
            if (extraOptions == null) return;
            foreach (var option in extraOptions)
            {
                Options[option.Key] = option.Value;
            }
        }

        /// <summary>
        /// build a query url
        /// </summary>
        /// <returns></returns>
        protected string QueryUrl()
        {
            var sb = new StringBuilder();
            sb.Append(GoogleUrl);
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
        /// <param name="term">term to search</param>
        /// <param name="label">If you are using custom refinements this will help you search part of your site.</param>
        /// <returns></returns>
        public List<GoogleSearchResult> Search(string term, string label = "")
        {

            if (label != string.Empty)
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
        /// Walk through pages fo results
        /// </summary>
        /// <param name="url"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private List<GoogleSearchResult> RecursiveResults(string url, int depth = 1)
        {
            var xResults = XDocument.Load(url);
            var data = xResults.Descendants("RES").FirstOrDefault();
            var ret = new List<GoogleSearchResult>();
            if (data == null) return ret;
            foreach (var r in data.Descendants("R"))
            {
                var tmpR = new GoogleSearchResult();
                var xAttribute = r.Attribute("MIME");
                if (xAttribute != null) tmpR.Mime = xAttribute.Value;
                var xElement = r.Element("U");
                if (xElement != null) tmpR.Url = xElement.Value;
                var element = r.Element("T");
                if (element != null) tmpR.Title = element.Value;
                var xElement1 = r.Element("S");
                if (xElement1 != null) tmpR.Description = xElement1.Value;
                ret.Add(tmpR);
            }
            var nb = data.Element("NB");
            if (nb == null) return ret;
            var nu = nb.Element("NU");
            if (nu == null) return ret;
            if (depth < _maxPages) 
            {
                ret = ret.Union(RecursiveResults("http://www.google.com" + nu.Value, ++depth)).ToList();
            }
            return ret;
        }
    }

}
