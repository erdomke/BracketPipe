using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BracketPipe.Tests
{

  public class OfficialExamples
  {
    [Fact]
    public void Example_KitchenSink()
    {
      var html = @"<div>  <script>alert('xss');</script>
                    <a href=""http://www.google.com/"">Google</a>
                    <a href=""http://www.yahoo.com/"">Yahoo</a>  </div>";
      using (var reader = new HtmlReader(html))
      {
        var result = (string)reader.Sanitize().Minify().ToHtml();
        Assert.Equal(@"<div><a href=""http://www.google.com/"">Google</a> <a href=""http://www.yahoo.com/"">Yahoo</a></div>"
          , result);
      }
    }

    [Fact]
    public void Example_ParseLinks()
    {
      var html = @"<html>  <body>  <script>alert('xss');</script>
                    <a href=""http://www.google.com/"">Google</a>
                    <a href=""http://www.yahoo.com/"">Yahoo</a>  </body>  </html>";
      using (var reader = new HtmlReader(html))
      {
        var urls = reader
          .OfType<HtmlStartTag>()
          .Where(t => t.Value == "a")
          .Select(t => t["href"])
          .ToArray();

        Assert.Equal(new string[]
        {
          "http://www.google.com/",
          "http://www.yahoo.com/"
        }
        , urls);
      }
    }

  }
}
