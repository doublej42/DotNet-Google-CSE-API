DotNet-Google-CSE-API
=====================

Simple wrapper class for basic searching using Google Custom Search Engine for Business. 

Note the default is to search 20 pages of results.

It can't return more than 100 results due to Google api.

Example Usage:
```c#
var gs = new GoogleSearch("YOUR SEARCH ID",key: "YOURAPIKEY"); 
//basic search
var results = gs.Search("SEARCH TERM");
//search in a refinment label
var results2 = gs.SearchDetailed("SEARCH TERM", "LABEL");

var gs2 = new GoogleSearch("YOUR SEARCH ID",  maxPages: 2, pageSize: 20, method: GoogleSearchMethod.XML)
var fortyResults = gs2.SearchDetailed("SEARCH TERM"); 
```


Install via https://www.nuget.org/packages/GoogleCustomSearchEngine/

