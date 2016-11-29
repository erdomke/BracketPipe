using BracketPipe;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BracketPipe.Tests
{
  [TestFixture]
  class GenerateNodesTest
  {
    [Test]
    public void HtmlFromNodes()
    {
      Assert.AreEqual("<!DOCTYPE html><body><div id=\"first\" class=\"start\"><p style=\"color:red;\">Paragraph text</p></div></body>", GetHtml().ToHtml());
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
