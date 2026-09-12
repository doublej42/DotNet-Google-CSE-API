DotNet Google Vertex AI Search API
==================================

A lightweight C# wrapper for Google Vertex AI Search. The retired Google Custom Search Engine API is not supported.

## Authentication

The client accepts exactly these credentials:

- `PROJECT_ID`: Google Cloud project ID
- `APP_ID`: Vertex AI Search app/engine ID
- `API_KEY`: Google Cloud API key with Discovery Engine API access

```powershell
$env:PROJECT_ID="your-project-id"
$env:APP_ID="your-app-id"
$env:API_KEY="your-api-key"
```

## Usage

```csharp
using GoogleCSE;

var searcher = new GoogleSearch(
    projectId: "your-project-id",
    appId: "your-app-id",
    apiKey: "YOUR_API_KEY");

var results = searcher.Search("dotnet tutorials");

foreach (var item in results)
{
    Console.WriteLine($"{item.Title}: {item.Url}");
}
```

To limit a website search to a URL pattern, pass `siteSearch` as the second argument:

```csharp
var results = searcher.Search(
    "dotnet tutorials",
    siteSearch: "https://example.com/docs/*");
```

This sends the Vertex AI Search filter expression `siteSearch:"https://example.com/docs/*"`.

Optional constructor arguments configure the Vertex request: `location`, `pageSize`, `maxPages`, and `start`.

## Test Application

The included console application reads only `PROJECT_ID`, `APP_ID`, and `API_KEY`.

```powershell
dotnet run --project GoogleCSE/GoogleCSE.TestApp/GoogleCSE.TestApp.csproj
```


See:

https://docs.cloud.google.com/generative-ai-app-builder/docs/migrate-from-cse#api-key-deploy