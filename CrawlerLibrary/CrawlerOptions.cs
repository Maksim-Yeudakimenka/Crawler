using System;

namespace CrawlerLibrary
{
  public class CrawlerOptions
  {
    public Uri ResourceUrl { get; set; }

    public int Depth { get; set; }

    public string SavingDirectory { get; set; }

    public string[] FileFormats { get; set; }

    public DomainRestriction Restrictions { get; set; }

    public bool TraceEnabled { get; set; }
  }
}