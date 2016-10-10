using System;
using System.IO;

namespace CrawlerLibrary
{
  public class InputDataProcessor
  {
    public bool TrySetResourceUrl(string url, CrawlerOptions options)
    {
      Uri resourceUrl;
      if (Uri.TryCreate(url, UriKind.Absolute, out resourceUrl))
      {
        options.ResourceUrl = resourceUrl;
        return true;
      }

      return false;
    }

    public bool ProcessDepth(string depthString, CrawlerOptions options)
    {
      int depth;

      if (int.TryParse(depthString, out depth) == false)
      {
        return false;
      }

      if (depth < 0 || depth > 5)
      {
        return false;
      }

      options.Depth = depth;
      return true;
    }

    public void CreateSiteDirectory(string directory, CrawlerOptions options)
    {
      var directoryInfo = Directory.CreateDirectory(directory);
      options.SavingDirectory = directoryInfo.FullName;
    }

    public bool ProcessFileTypes(string sources, CrawlerOptions options)
    {
      if (sources == string.Empty)
      {
        return false;
      }

      options.FileFormats = sources.Split(',');
      return true;
    }
  }
}