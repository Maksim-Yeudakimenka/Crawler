using System;
using System.IO;
using HtmlAgilityPack;

namespace CrawlerLibrary
{
  public class DocumentProcessor
  {
    private readonly string _saveDirectory;

    public DocumentProcessor(string saveDirectory)
    {
      _saveDirectory = saveDirectory;
    }

    public HtmlDocument SaveDocument(Stream htmlStream, Uri uri)
    {
      var fileName = this.GetFileName(uri.AbsoluteUri, ".html");

      var html = new HtmlDocument();
      html.Load(htmlStream);
      html.Save(_saveDirectory + "\\" + fileName);
      return html;
    }

    public string GetFileName(string url, string extension)
    {
      return url.Replace(':', '_')
        .Replace('.', '_')
        .Replace('/', '_')
        .Replace('?', '_')
        .Replace('*', '_')
        .Replace('"', '_')
        .Replace('<', '_')
        .Replace('>', '_')
        .Replace('|', '_')
        .Replace('+', '_')
        .Replace('%', '_')
        .Replace('!', '_')
        .Replace('@', '_')
        .Trim()
             + extension;
    }
  }
}