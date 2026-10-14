using System;
using System.Collections.Generic;
using GoogleCSE;

namespace GoogleCSE.TestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("  Google Cloud Agent Search (.NET Wrapper) Tester  ");
            Console.WriteLine("=================================================\n");

            string projectId = Environment.GetEnvironmentVariable("PROJECT_ID");
            string appId = Environment.GetEnvironmentVariable("APP_ID");
            string apiKey = Environment.GetEnvironmentVariable("API_KEY");

            if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine("Set PROJECT_ID, APP_ID, and API_KEY before starting the test app.");
                return;
            }

            var searcher = new GoogleSearch(projectId, appId, apiKey, pageSize: 10, maxPages: 2);

            while (true)
            {
                Console.WriteLine("\n-------------------------------------------------");
                Console.Write("Enter search query (or type 'exit' to quit): ");
                string query = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(query) || query.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                Console.Write("Enter siteSearch URL pattern (optional, press Enter to skip): ");
                string siteSearch = Console.ReadLine()?.Trim();

                Console.WriteLine($"\nExecuting search for '{query}'...");

                try
                {
                    GoogleSearchResults results = searcher.SearchDetailed(query, siteSearch);

                    Console.WriteLine($"\nSearch completed in {results.SearchTime:F3} seconds.");
                    Console.WriteLine($"Total Estimated Results: {results.TotalResults}");
                    Console.WriteLine($"Results Returned: {results.Results.Count}\n");

                    if (results.Results.Count == 0)
                    {
                        Console.WriteLine("No search results found.");
                    }
                    else
                    {
                        for (int i = 0; i < results.Results.Count; i++)
                        {
                            var item = results.Results[i];
                            Console.WriteLine($"[{i + 1}] {item.Title}");
                            Console.WriteLine($"    URL:  {item.Url}");
                            if (!string.IsNullOrEmpty(item.Mime))
                            {
                                Console.WriteLine($"    Mime: {item.Mime}");
                            }
                            if (!string.IsNullOrEmpty(item.Description))
                            {
                                Console.WriteLine($"    Snippet: {item.Description}");
                            }
                            Console.WriteLine();
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[Error during search]: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"  Detail: {ex.InnerException.Message}");
                    }
                }
            }

            Console.WriteLine("\nThank you for using GoogleCSE Tester. Goodbye!");
        }

    }
}
