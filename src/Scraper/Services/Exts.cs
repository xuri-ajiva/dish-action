using HtmlAgilityPack;

namespace Scraper.Services;

public static class Exts
{
    public static HtmlNode? FirstOrDefaultByClass(this HtmlNode node, string @class)
    {
        return node.ChildNodes.FirstOrDefault(x => x.Attributes.AttributesWithName("class").Any(a => a.Value == @class));
    }
}
