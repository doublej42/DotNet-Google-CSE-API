using Newtonsoft.Json;
using System.Collections.Generic;

namespace GoogleCSE
{
    public class VertexSearchRequest
    {
        [JsonProperty("servingConfig", NullValueHandling = NullValueHandling.Ignore)]
        public string ServingConfig { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("filter", NullValueHandling = NullValueHandling.Ignore)]
        public string Filter { get; set; }

        [JsonProperty("contentSearchSpec", NullValueHandling = NullValueHandling.Ignore)]
        public VertexContentSearchSpec ContentSearchSpec { get; set; }
    }

    public class VertexContentSearchSpec
    {
        [JsonProperty("snippetSpec", NullValueHandling = NullValueHandling.Ignore)]
        public VertexSnippetSpec SnippetSpec { get; set; }
    }

    public class VertexSnippetSpec
    {
        [JsonProperty("returnSnippet")]
        public bool ReturnSnippet { get; set; } = true;
    }

    public class VertexSearchResponse
    {
        [JsonProperty("results")]
        public List<VertexSearchResultItem> Results { get; set; }

        [JsonProperty("totalSize")]
        public long TotalSize { get; set; }

        [JsonProperty("attributionToken")]
        public string AttributionToken { get; set; }

        [JsonProperty("nextPageToken")]
        public string NextPageToken { get; set; }

    }

    public class VertexSearchResultItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("document")]
        public VertexDocument Document { get; set; }
    }

    public class VertexDocument
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("schemaId")]
        public string SchemaId { get; set; }

        [JsonProperty("structData")]
        public Dictionary<string, object> StructData { get; set; }

        [JsonProperty("derivedStructData")]
        public Dictionary<string, object> DerivedStructData { get; set; }
    }

}
