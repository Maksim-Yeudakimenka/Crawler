using System;
using CrawlerLibrary;

namespace ConsoleApplication
{
  class Program
  {
    static void Main(string[] args)
    {
      var dataProcessor = new InputDataProcessor();
      var loaderOptions = new CrawlerOptions();

      ShowMenu(dataProcessor, loaderOptions);
      StartCrawler(loaderOptions, dataProcessor);
    }

    private static void ShowMenu(InputDataProcessor dataProcessor, CrawlerOptions crawlerOptions)
    {
      while (true)
      {
        Console.Write("Start url: ");
        var url = Console.ReadLine();

        if (dataProcessor.TrySetResourceUrl(url, crawlerOptions) == false)
        {
          ShowErrorMessage("URL is not correct. Please, try again.");
          continue;
        }

        Console.Write("Directory name: ");
        var directory = Console.ReadLine();

        try
        {
          dataProcessor.CreateSiteDirectory(directory, crawlerOptions);
        }
        catch (Exception e)
        {
          ShowErrorMessage(e.Message);
          continue;
        }

        Console.Write("Link depth (0-5): ");
        var depthString = Console.ReadLine();

        if (dataProcessor.ProcessDepth(depthString, crawlerOptions) == false)
        {
          ShowErrorMessage("Depth must be a number between 0 and 5. Please, try again.");
          continue;
        }

        Console.Write("Types of files to download (e.g. jpg, pdf): ");
        var fileTypes = Console.ReadLine();

        if (dataProcessor.ProcessFileTypes(fileTypes, crawlerOptions) == false)
        {
          ShowErrorMessage("File types are not correct. Please, try again.");
          continue;
        }

        Console.WriteLine("Domain restriction type: ");
        Console.WriteLine("--- a) No restrictions");
        Console.WriteLine("--- b) Current domain");
        Console.WriteLine("--- c) Not above current url");
        var restrictionTypeKey = Console.ReadKey();

        switch (restrictionTypeKey.KeyChar)
        {
          case 'a':
            crawlerOptions.Restrictions = DomainRestriction.None;
            break;
          case 'b':
            crawlerOptions.Restrictions = DomainRestriction.CurrentDomain;
            break;
          case 'c':
            crawlerOptions.Restrictions = DomainRestriction.NotAboveCurrentUrl;
            break;
          default:
            ShowErrorMessage("Key is not correct! Please, try again.");
            continue;
        }

        Console.WriteLine();
        Console.WriteLine("Trace on/off (y/n): ");
        var traceOnOffKey = Console.ReadKey();

        switch (traceOnOffKey.KeyChar)
        {
          case 'y':
            crawlerOptions.TraceEnabled = true;
            break;
          case 'n':
            crawlerOptions.TraceEnabled = false;
            break;
          default:
            ShowErrorMessage("Key is not correct! Please, try again.");
            continue;
        }

        Console.WriteLine();
        Console.WriteLine("Loading started...");
        Console.WriteLine();

        break;
      }
    }

    private static void StartCrawler(CrawlerOptions crawlerOptions, InputDataProcessor dataProcessor)
    {
      var documentProcessor = new DocumentProcessor(crawlerOptions.SavingDirectory);
      using (var siteLoader = new Crawler(crawlerOptions, dataProcessor, documentProcessor))
      {
        try
        {
          siteLoader.LoadSite(crawlerOptions.Depth).Wait();
        }
        catch (Exception e)
        {
          Console.WriteLine(e.Message);
        }
      }

      Console.WriteLine("Finished!");
      Console.ReadKey();
    }

    private static void ShowErrorMessage(string message)
    {
      Console.WriteLine();
      Console.WriteLine(message);
      Console.ReadKey();
      Console.Clear();
    }
  }
}