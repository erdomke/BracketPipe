using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngleParse.Core.Tests.Html
{
  [TestFixture]
  class SanitizeTests
  {
    private void TestSanitize(string input, string expected)
    {
      using (var reader = new HtmlReader(input))
      {
        var rendered = reader.Sanitize().ToHtml();
        Assert.AreEqual(expected, rendered);
      }
    }

    [Test]
    public void Sanitize_Basic()
    {
      TestSanitize(@"<div><p>stuff <img src=""javascript:alert('hit')"" /> <img data-test=""something"" src=""thing.png"" /> <img src=""http://www.google.com/thing.png"" /> and more</p></div><div><script>alert('bad stuff');</script>With content</div>"
        , "<div><p>stuff  <img src=\"thing.png\"> <img src=\"http://www.google.com/thing.png\"> and more</p></div><div>With content</div>");
    }

    [Test]
    public void Sanitize_CheatSheet()
    {
      TestSanitize(@"<p><SCRIPT SRC=http://xss.rocks/xss.js></SCRIPT></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=""javascript: alert('XSS'); ""></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=javascript:alert('XSS')></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=JaVaScRiPt:alert('XSS')></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=javascript:alert(""XSS"")></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=`javascript:alert(""RSnake says, 'XSS'"")`></p>", "<p></p>");
      TestSanitize(@"<p><a onmouseover=""alert(document.cookie)"">xxs link</a></p>", "<p><a>xxs link</a></p>");
      TestSanitize(@"<p><a onmouseover=alert(document.cookie)>xxs link</a></p>", "<p><a>xxs link</a></p>");
      TestSanitize(@"<p><IMG """"""><SCRIPT>alert(""XSS"")</SCRIPT>""></p>", "<p><img>\"&gt;</p>");
      TestSanitize(@"<p><IMG SRC=javascript:alert(String.fromCharCode(88,83,83))></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=# onmouseover=""alert('xxs')""></p>", "<p><img src=\"#\"></p>");
      TestSanitize(@"<p><IMG SRC= onmouseover=""alert('xxs')""></p>", "<p><img src=\"onmouseover=&quot;alert('xxs')&quot;\"></p>");
      TestSanitize(@"<p><IMG onmouseover=""alert('xxs')""></p>", "<p><img></p>");
      TestSanitize(@"<p><IMG SRC=/ onerror=""alert(String.fromCharCode(88, 83, 83))""></img></p>", "<p><img src=\"/\"></p>");
      TestSanitize(@"<p><img src=x onerror=""&#0000106&#0000097&#0000118&#0000097&#0000115&#0000099&#0000114&#0000105&#0000112&#0000116&#0000058&#0000097&#0000108&#0000101&#0000114&#0000116&#0000040&#0000039&#0000088&#0000083&#0000083&#0000039&#0000041""></p>", "<p><img src=\"x\"></p>");
      TestSanitize(@"<p><IMG SRC=&#106;&#97;&#118;&#97;&#115;&#99;&#114;&#105;&#112;&#116;&#58;&#97;&#108;&#101;&#114;&#116;&#40;&#39;&#88;&#83;&#83;&#39;&#41;></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=&#0000106&#0000097&#0000118&#0000097&#0000115&#0000099&#0000114&#0000105&#0000112&#0000116&#0000058&#0000097&#0000108&#0000101&#0000114&#0000116&#0000040&#0000039&#0000088&#0000083&#0000083&#0000039&#0000041></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=&#x6A&#x61&#x76&#x61&#x73&#x63&#x72&#x69&#x70&#x74&#x3A&#x61&#x6C&#x65&#x72&#x74&#x28&#x27&#x58&#x53&#x53&#x27&#x29></p>", "<p></p>");
      TestSanitize("<p><IMG SRC=\"jav\tascript:alert('XSS');\"></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=""jav&#x09;ascript:alert('XSS');""></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=""jav&#x0A;ascript:alert('XSS');""></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=""jav&#x0D;ascript:alert('XSS');""></p>", "<p></p>");
      TestSanitize("<p><IMG SRC=\"java\0script:alert('XSS');\"></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC="" &#14;  javascript:alert('XSS');""></p>", "<p></p>");
      TestSanitize(@"<p><SCRIPT/XSS SRC=""http://xss.rocks/xss.js""></SCRIPT></p>", "<p></p>");
      TestSanitize(@"<p><BODY onload!#$%&()*~+-_.,:;?@[/|\]^`=alert(""XSS"")></BODY></p>", "<p></p>");
      TestSanitize(@"<p><SCRIPT/SRC=""http://xss.rocks/xss.js""></SCRIPT></p>", "<p></p>");

    }
  }
}
