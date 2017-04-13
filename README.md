DotNet-Google-CSE-API
=====================

Simple wrapper class for basic searching using Google Custom Search Engine for Business. 

Note the default is to search 10 pages of results.

It can't return more than 100 results due to Google api.

Example Usage:
```csharp
var gs = new GoogleSearch("YOUR SEARCH ID",key: "YOURAPIKEY"); 
//basic search
var results = gs.Search("SEARCH TERM");
//search in a refinment label
var results2 = gs.SearchDetailed("SEARCH TERM", "LABEL");

var gs2 = new GoogleSearch("YOUR SEARCH ID",  maxPages: 2, pageSize: 20, method: GoogleSearchMethod.XML)
var fortyResults = gs2.SearchDetailed("SEARCH TERM"); 
```


Install via https://www.nuget.org/packages/GoogleCustomSearchEngine/

If there is an issue due to user limits or daily limits it will fail with as many results as it could get before the limit was reached.

```csharp
var gs = new GoogleSearch("YOUR SEARCH ID", maxPages: 2, pageSize: 10, key: "YOURAPIKEY", method: GoogleSearchMethod.CSE,userIp: "192.168.1.101" );
```

Search for just PDF files

```csharp
var gs = new GoogleSearch("YOUR SEARCH ID", maxPages: 1, pageSize: 5, key: "YOURAPIKEY");
gs.Options.Add("fileType","pdf");
var results = gs.Search("dam");
```
