using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace BracketPipe.Tests
{

  public class XmlReaderTest
  {
    [Fact]
    public void XmlReader_Simple()
    {
      var html = "<div class='stuff'>content that is <b>important</b><br>with another&nbsp;line</div>";
      using (var reader = new HtmlReader(html))
      {
        var elem = XElement.Load(reader);
        Assert.Equal("<div class=\"stuff\">content that is <b>important</b><br />with another line</div>", elem.ToString());
      }
      using (var reader = new HtmlReader(html))
      {
        var doc = new XmlDocument();
        doc.Load(reader);
        Assert.Equal("<div class=\"stuff\">content that is <b>important</b><br />with another line</div>", doc.OuterXml);
      }
    }

    [Fact]
    public void XmlReader_Basic()
    {
      var html = @"<!DOCTYPE html>

<html lang=""en"">
<head>
  <meta charset=""utf-8"">

  <title>The HTML5 Herald</title>
  <meta name=""description"" content=""The HTML5 Herald"">
  <meta name=""author"" content=""SitePoint"">

  <link rel=""stylesheet"" href=""css/styles.css?v=1.0"">

  <!--[if lt IE 9]>
    <script src=""https://cdnjs.cloudflare.com/ajax/libs/html5shiv/3.7.3/html5shiv.js""></script>
  <![endif]-->
</head>

<body>
  <input type=""text"" required>
  <script src=""js/scripts.js""></script>
</body>
</html>";
      using (var reader = new HtmlReader(html))
      {
        var doc = XDocument.Load(reader);
        Assert.Equal(@"<!DOCTYPE html PUBLIC """" """"[]>

<html lang=""en"">
<head>
  <meta charset=""utf-8"" />

  <title>The HTML5 Herald</title>
  <meta name=""description"" content=""The HTML5 Herald"" />
  <meta name=""author"" content=""SitePoint"" />

  <link rel=""stylesheet"" href=""css/styles.css?v=1.0"" />

  <!--[if lt IE 9]>
    <script src=""https://cdnjs.cloudflare.com/ajax/libs/html5shiv/3.7.3/html5shiv.js""></script>
  <![endif]-->
</head>

<body>
  <input type=""text"" required="""" />
  <script src=""js/scripts.js""></script>
</body>
</html>", doc.ToString());
      }
      using (var reader = new HtmlReader(html))
      {
        var doc = new XmlDocument();
        doc.Load(reader);
        Assert.Equal("<!DOCTYPE html[]><html lang=\"en\">\n<head>\n  <meta charset=\"utf-8\" />\n\n  <title>The HTML5 Herald</title>\n  <meta name=\"description\" content=\"The HTML5 Herald\" />\n  <meta name=\"author\" content=\"SitePoint\" />\n\n  <link rel=\"stylesheet\" href=\"css/styles.css?v=1.0\" />\n\n  <!--[if lt IE 9]>\n    <script src=\"https://cdnjs.cloudflare.com/ajax/libs/html5shiv/3.7.3/html5shiv.js\"></script>\n  <![endif]-->\n</head>\n\n<body>\n  <input type=\"text\" required=\"\" />\n  <script src=\"js/scripts.js\"></script>\n</body>\n</html>"
          , doc.OuterXml);
      }
    }
  }
}
