using System.Collections.Generic;

namespace TK.Twitter.Crawl;

public class PagingResult<T>
{
    public List<T> Items { get; set; }

    public int TotalCount { get; set; }

    public PagingResult()
    {

    }

    public PagingResult(List<T> items, int totalCount)
    {
        this.Items = items;
        this.TotalCount = totalCount;
    }
}
