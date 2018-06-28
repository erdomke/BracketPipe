using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

namespace BracketPipe.Tests
{
  public class GenerateNodesTest
  {
    [Fact]
    public void HtmlFromNodes()
    {
      Assert.Equal("<!DOCTYPE html><body><div id=\"first\" class=\"start\"><p style=\"color:red;\">Paragraph text</p></div></body>", GetHtml().ToHtml());
    }

    [Fact]
    public void ReadToEnd()
    {
      var origString = default(string);
      using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BracketPipe.Tests.LongSample.html"))
      using (var reader = new StreamReader(stream))
      {
        origString = reader.ReadToEnd();
      }

      using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BracketPipe.Tests.LongSample.html"))
      {
        var source = new TextSource(stream);
        var newString = source.ReadToEnd();
        Assert.Equal(origString, newString);
      }
    }

    private IEnumerable<HtmlNode> GetHtml()
    {
      yield return new HtmlDoctype();
      yield return new HtmlStartTag("body");
      yield return new HtmlStartTag("div")
      {
        { "id", "first" },
        { "class", "start" }
      };
      yield return new HtmlStartTag("p").Add("style", "color:red;");
      yield return new HtmlText("Paragraph text");
      yield return new HtmlEndTag("p");
      yield return new HtmlEndTag("div");
      yield return new HtmlEndTag("body");
    }
  }
}
