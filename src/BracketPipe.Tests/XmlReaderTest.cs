using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Xunit;

namespace BracketPipe.Tests
{

  public class XmlReaderTest
  {
    private void TestXmlVariants(string html, string expected, string expectedNoIndent = null)
    {
      expectedNoIndent = expectedNoIndent ?? expected;

      using (var reader = new HtmlReader(html))
      {
        var elem = XDocument.Load(reader);
        Assert.Equal(expected, elem.ToString());
      }

      using (var reader = new HtmlReader(html))
      {
        var list = reader.ToList();
        using (var xml = list.AsXmlReader())
        {
          var elem = XDocument.Load(xml);
          Assert.Equal(expected, elem.ToString());
        }
      }

      using (var reader = new HtmlReader(html))
      {
        var doc = new XmlDocument();
        doc.Load(reader);
        Assert.Equal(expectedNoIndent, doc.OuterXml);
      }

      using (var reader = new HtmlReader(html))
      {
        var list = reader.ToList();
        using (var xml = list.AsXmlReader())
        {
          var doc = new XmlDocument();
          doc.Load(xml);
          Assert.Equal(expectedNoIndent, doc.OuterXml);
        }
      }
    }

    [Fact]
    public void XmlReader_Simple()
    {
      const string html = "<div class='stuff'>content that is <b>important</b><br>with another&nbsp;line</div>";
      const string expected = "<div class=\"stuff\">content that is <b>important</b><br />with another line</div>";

      TestXmlVariants(html, expected);
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

      const string expected = @"<!DOCTYPE html PUBLIC """" """"[]>

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
</html>";
      const string expectedNoIndent = "<!DOCTYPE html[]><html lang=\"en\"><head><meta charset=\"utf-8\" /><title>The HTML5 Herald</title><meta name=\"description\" content=\"The HTML5 Herald\" /><meta name=\"author\" content=\"SitePoint\" /><link rel=\"stylesheet\" href=\"css/styles.css?v=1.0\" /><!--[if lt IE 9]>\n    <script src=\"https://cdnjs.cloudflare.com/ajax/libs/html5shiv/3.7.3/html5shiv.js\"></script>\n  <![endif]--></head><body><input type=\"text\" required=\"\" /><script src=\"js/scripts.js\"></script></body></html>";

      TestXmlVariants(html, expected, expectedNoIndent);
    }
  }
}
