using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BracketPipe.Tests
{
  [TestFixture]
  class OfficialExamples
  {
    [Test]
    public void Example_KitchenSink()
    {
      var html = @"<div>  <script>alert('xss');</script>
                    <a href=""http://www.google.com/"">Google</a>
                    <a href=""http://www.yahoo.com/"">Yahoo</a>  </div>";
      using (var reader = new HtmlReader(html))
      {
        var result = (string)reader.Sanitize().Minify().ToHtml();
        Assert.AreEqual(@"<div><a href=""http://www.google.com/"">Google</a> <a href=""http://www.yahoo.com/"">Yahoo</a></div>"
          , result);
      }
    }

    [Test]
    public void Example_ParseLinks()
    {
      var html = @"<html>  <body>  <script>alert('xss');</script>
                    <a href=""http://www.google.com/"">Google</a>
                    <a href=""http://www.yahoo.com/"">Yahoo</a>  </body>  </html>";
      using (var reader = new HtmlReader(html))
      {
        var urls = reader
          .OfType<HtmlTagNode>()
          .Where(t => t.Type == HtmlTokenType.StartTag && t.Value == "a")
          .Select(t => t.GetAttribute("href"))
          .ToArray();
        CollectionAssert.AreEqual(new string[]
        {
          "http://www.google.com/",
          "http://www.yahoo.com/"
        }
        , urls);
      }
    }

  }
}
