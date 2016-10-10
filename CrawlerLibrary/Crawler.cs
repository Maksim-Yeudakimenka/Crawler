using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace CrawlerLibrary
{
  public class Crawler : IDisposable
  {
    private readonly HttpClient _client;

    private readonly InputDataProcessor _dataProcessor;
    private readonly DocumentProcessor _documentProcessor;
    private readonly CrawlerOptions _crawlerOptions;

    public Crawler(CrawlerOptions options, InputDataProcessor dataProcessor, DocumentProcessor documentProcessor)
    {
      _client = new HttpClient();

      _dataProcessor = dataProcessor;
      _documentProcessor = documentProcessor;
      _crawlerOptions = options;
    }

    public async Task LoadSite(int depth)
    {
      await RequestPage(depth, _crawlerOptions.ResourceUrl);
    }

    private async Task RequestPage(int depth, Uri url)
    {
      if (_crawlerOptions.TraceEnabled)
      {
        Console.WriteLine("--- " + url.AbsoluteUri + " ---");
      }

      if (depth < 0)
      {
        return;
      }

      depth--;

      var response = await _client.GetAsync(url);
      if (response.StatusCode == HttpStatusCode.NotFound)
      {
        return;
      }

      using (var htmlStream = await response.Content.ReadAsStreamAsync())
      {
        var html = _documentProcessor.SaveDocument(htmlStream, url);
        await ProcessResources(html);

        if (depth >= 0)
        {
          await ProcessHRefs(depth, url, html);
        }
      }
    }

    private async Task ProcessHRefs(int depth, Uri url, HtmlDocument html)
    {
      var hrefUrlOptions = new CrawlerOptions();

      var hrefs = html.DocumentNode.Descendants().Where(n => n.Name == "a" && n.Attributes.Any(a => a.Name == "href"));
      foreach (var link in hrefs)
      {
        var hrefValue = link.GetAttributeValue("href", string.Empty);
        if (hrefValue != string.Empty)
        {
          string refUrl = string.Empty;
          if (hrefValue.Contains("http"))
          {
            refUrl = hrefValue;
          }
          else
          {
            refUrl = url.Scheme + "://" + url.Host + hrefValue;
          }

          if (_dataProcessor.TrySetResourceUrl(refUrl, hrefUrlOptions) && CheckRestriction(hrefUrlOptions.ResourceUrl))
          {
            await RequestPage(depth, hrefUrlOptions.ResourceUrl);
          }
        }
      }
    }

    private async Task ProcessResources(HtmlDocument html)
    {
      var images = html.DocumentNode.Descendants().Where(n => n.Name == "img" && n.Attributes.Any(i => i.Name == "src"));
      var imageUrlOptions = new CrawlerOptions();

      foreach (var image in images)
      {
        var imageRef = image.GetAttributeValue("src", string.Empty);
        if (imageRef != string.Empty)
        {
          foreach (var format in _crawlerOptions.FileFormats)
          {
            if (imageRef.Contains(format) || imageRef.Contains(format.ToUpperInvariant()))
            {
              if (_dataProcessor.TrySetResourceUrl(imageRef, imageUrlOptions) &&
                  CheckRestriction(imageUrlOptions.ResourceUrl))
              {
                var imageResponse = await _client.GetAsync(imageUrlOptions.ResourceUrl);
                if (imageResponse.StatusCode == HttpStatusCode.NotFound)
                {
                  break;
                }

                var imageBytes = await imageResponse.Content.ReadAsByteArrayAsync();

                var fileName = _documentProcessor.GetFileName(imageUrlOptions.ResourceUrl.AbsolutePath, "." + format);
                using (var file = File.Create(_crawlerOptions.SavingDirectory + "//" + fileName))
                {
                  await file.WriteAsync(imageBytes, 0, imageBytes.Length);
                }
              }
            }
          }
        }
      }
    }

    private bool CheckRestriction(Uri currentUrl)
    {
      if (_crawlerOptions.Restrictions == DomainRestriction.None)
      {
        return true;
      }

      if (_crawlerOptions.Restrictions == DomainRestriction.CurrentDomain &&
          _crawlerOptions.ResourceUrl.Host == currentUrl.Host)
      {
        return true;
      }

      if (_crawlerOptions.Restrictions == DomainRestriction.NotAboveCurrentUrl &&
          currentUrl.AbsoluteUri.Contains(_crawlerOptions.ResourceUrl.AbsoluteUri))
      {
        return true;
      }

      return false;
    }

    public void Dispose()
    {
      _client.Dispose();
    }
  }
}