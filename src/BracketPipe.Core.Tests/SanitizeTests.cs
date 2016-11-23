using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BracketPipe.Core.Tests
{
  [TestFixture]
  class SanitizeTests
  {
    private void TestSanitize(string input, string expected, HtmlSanitizeSettings settings = null)
    {
      using (var reader = new HtmlReader(input))
      {
        var rendered = reader.Sanitize(settings ?? HtmlSanitizeSettings.Default()).ToHtml();
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
    public void Sanitize_CheatSheet_Anchor()
    {
      TestSanitize("<a href=\"'';!--\"<XSS>=&{()}\">", @"<a href=""'';!--"">=&amp;{()}""&gt;");
      TestSanitize(@"<p><a onmouseover=""alert(document.cookie)"">xxs link</a></p>", "<p><a>xxs link</a></p>");
      TestSanitize(@"<p><a onmouseover=alert(document.cookie)>xxs link</a></p>", "<p><a>xxs link</a></p>");
      TestSanitize("<A HREF=\"javascript:document.location='http://www.google.com/'\">XSS</A>", "<a>XSS</a>");
      TestSanitize("<A HREF=\"http://www.codeplex.com?url=<SCRIPT/XSS SRC=\"http://ha.ckers.org/xss.js\"></SCRIPT>\">XSS</A>", @"<a href=""http://www.codeplex.com/?url=%3CSCRIPT/XSS%20SRC="">""&gt;XSS</a>");
      TestSanitize("<A HREF=\"http://www.codeplex.com?url=<<SCRIPT>alert(\"XSS\");//<</SCRIPT>\">XSS</A>", @"<a href=""http://www.codeplex.com/?url=%3C%3CSCRIPT%3Ealert("">""&gt;XSS</a>");
    }

    [Test]
    public void Sanitize_CheatSheet_Script()
    {
      TestSanitize(@"<p><SCRIPT SRC=http://xss.rocks/xss.js></SCRIPT></p>", "<p></p>");
      TestSanitize(@"<p><SCR\0IPT SRC=http://xss.rocks/xss.js></SCR\0IPT></p>", "<p></p>");
      TestSanitize(@"<p><SCRIPT/XSS SRC=""http://xss.rocks/xss.js""></SCRIPT></p>", "<p></p>");
      TestSanitize(@"<p><SCRIPT/SRC=""http://xss.rocks/xss.js""></SCRIPT></p>", "<p></p>");
      TestSanitize(@"<p><<SCRIPT>alert(""XSS"");//<</SCRIPT></p>", "<p>&lt;</p>");
      TestSanitize(@"<p><SCRIPT SRC=http://xss.rocks/xss.js?< B ></p>", "<p>");
      TestSanitize(@"<p><SCRIPT SRC=//xss.rocks/.j></SCRIPT></p>", "<p></p>");
    }

    [Test]
    public void Sanitize_CheatSheet_Img()
    {
      TestSanitize(@"<p><IMG SRC=""javascript:alert('XSS');""></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=""javascript: alert('XSS'); ""></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=javascript:alert('XSS')></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=JaVaScRiPt:alert('XSS')></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=javascript:alert(""XSS"")></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=javascript:alert(&quot;XSS&quot;)></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=`javascript:alert(""RSnake says, 'XSS'"")`></p>", "<p></p>");
      TestSanitize(@"<p><IMG """"""><SCRIPT>alert(""XSS"")</SCRIPT>""></p>", "<p>\"&gt;</p>");
      TestSanitize(@"<p><IMG SRC=javascript:alert(String.fromCharCode(88,83,83))></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=# onmouseover=""alert('xxs')""></p>", "<p><img src=\"#\"></p>");
      TestSanitize(@"<p><IMG SRC= onmouseover=""alert('xxs')""></p>", "<p><img src=\"onmouseover=%22alert('xxs')%22\"></p>");
      TestSanitize(@"<p><IMG onmouseover=""alert('xxs')""></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=/ onerror=""alert(String.fromCharCode(88, 83, 83))""></img></p>", "<p><img src=\"/\"></p>");
      TestSanitize(@"<p><img src=x onerror=""&#0000106&#0000097&#0000118&#0000097&#0000115&#0000099&#0000114&#0000105&#0000112&#0000116&#0000058&#0000097&#0000108&#0000101&#0000114&#0000116&#0000040&#0000039&#0000088&#0000083&#0000083&#0000039&#0000041""></p>", "<p><img src=\"x\"></p>");
      TestSanitize(@"<p><IMG SRC=&#106;&#97;&#118;&#97;&#115;&#99;&#114;&#105;&#112;&#116;&#58;&#97;&#108;&#101;&#114;&#116;&#40;&#39;&#88;&#83;&#83;&#39;&#41;></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=&#0000106&#0000097&#0000118&#0000097&#0000115&#0000099&#0000114&#0000105&#0000112&#0000116&#0000058&#0000097&#0000108&#0000101&#0000114&#0000116&#0000040&#0000039&#0000088&#0000083&#0000083&#0000039&#0000041></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=&#x6A&#x61&#x76&#x61&#x73&#x63&#x72&#x69&#x70&#x74&#x3A&#x61&#x6C&#x65&#x72&#x74&#x28&#x27&#x58&#x53&#x53&#x27&#x29></p>", "<p></p>");
      TestSanitize("<p><IMG SRC=\"jav\tascript:alert('XSS');\"></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=""jav&#x09;ascript:alert('XSS');""></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=""jav&#x0A;ascript:alert('XSS');""></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=""jav&#x0D;ascript:alert('XSS');""></p>", "<p></p>");
      TestSanitize(@"<p><IMG
SRC
=
""
j
a
v
a
s
c
r
i
p
t
:
a
l
e
r
t
(
'
X
S
S
'
)
""
></p>", "<p></p>");
      TestSanitize("<p><IMG SRC=\"java\0script:alert('XSS');\"></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC="" &#14;  javascript:alert('XSS');""></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=""javascript:alert('XSS')""</p>", "<p>");
      TestSanitize(@"<p><IMG DYNSRC=""javascript:alert('XSS')""></p>", "<p></p>");
      TestSanitize(@"<p><IMG LOWSRC=""javascript:alert('XSS')""></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC='vbscript:msgbox(""XSS"")'></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=""mocha:[code]""></p>", "<p></p>");
      TestSanitize(@"<p><IMG SRC=""livescript:[code]""></p>", "<p></p>");
      TestSanitize(@"<p><IMAGE src=http://xss.rocks/scriptlet.html <</p>", "<p>");
    }

    [Test]
    public void Sanitize_CheatSheet_Body()
    {
      TestSanitize(@"<p><BODY onload!#$%&()*~+-_.,:;?@[/|\]^`=alert(""XSS"")></BODY></p>", "<p></p>");

      var bodySettings = HtmlSanitizeSettings.Default();
      bodySettings.AllowedTags.Add("body");
      TestSanitize(@"<BODY BACKGROUND=""javascript:alert('XSS')""></BODY>", "<body></body>", bodySettings);
      TestSanitize(@"<BODY onload!#$%&()*~+-_.,:;?@[/|\]^`=alert(""XSS"")></BODY>", "<body></body>", bodySettings);
      TestSanitize(@"<BODY ONLOAD=alert('XSS')></BODY>", "<body></body>", bodySettings);
    }

    [Test]
    public void Sanitize_CheatSheet_StyleAttr()
    {
      TestSanitize(@"<IMG src=""#"" STYLE=""width:100px;xss:5px;background:expr/*XSS*/ession(alert('XSS'));height:100px;"">", @"<img src=""#"" style=""width:100px;height:100px;"">");
      TestSanitize(@"<IMG src=""#"" STYLE=""xss:expr/*XSS*/ession(alert('XSS'))"">", @"<img src=""#"">");
      TestSanitize("<div style=\"\";alert('XSS');//\"></div>", "<div></div>");
      TestSanitize(@"<XSS STYLE=""xss:expression(alert('XSS'))"">", @"");
      TestSanitize(@"<XSS STYLE=""behavior: url(xss.htc);"">", @"");
      TestSanitize(@"<p STYLE=""behavior: url(xss.htc);""></p>", @"<p></p>");
      TestSanitize(@"<DIV STYLE=""background-image: url(javascript:alert('XSS'))""></div>", @"<div></div>");
      TestSanitize(@"<DIV STYLE=""background-image:\0075\0072\006C\0028'\006a\0061\0076\0061\0073\0063\0072\0069\0070\0074\003a\0061\006c\0065\0072\0074\0028.1027\0058.1053\0053\0027\0029'\0029""></div>", @"<div></div>");
      TestSanitize(@"<DIV STYLE=""background-image: url(&#1;javascript:alert('XSS'))""></div>", @"<div></div>");
      TestSanitize(@"<DIV STYLE=""width: expression(alert('XSS'));""></div>", @"<div></div>");
      TestSanitize("exp/*<A STYLE='no\\xss:noxss(\"*//*\");xss:&#101;x&#x2F;*XSS*//*/*/pression(alert(\"XSS\"))'>", "exp/*<a>");
      TestSanitize("<Div style=\"background-color: http://www.codeplex.com?url=<SCRIPT SRC=http://ha.ckers.org/xss.js></SCRIPT>\">", "<div>");
      TestSanitize("<Div style=\"background-color: expression(<SCRIPT SRC=http://ha.ckers.org/xss.js></SCRIPT>)\">", "<div>");
      TestSanitize("<Div style=\"background-color: http://www.codeplex.com?url=<SCRIPT/XSS SRC=\"http://ha.ckers.org/xss.js\"></SCRIPT>\">", "<div>\"&gt;");
      TestSanitize("<Div style=\"background-color: expression(<SCRIPT/XSS SRC=\"http://ha.ckers.org/xss.js\"></SCRIPT>)\">", "<div>)\"&gt;");
      TestSanitize("<Div style=\"background-color: http://www.codeplex.com?url=<SCRIPT/SRC=\"http://ha.ckers.org/xss.js\"></SCRIPT>\">", "<div>\"&gt;");
      TestSanitize("<Div style=\"background-color: expression(<SCRIPT/SRC=\"http://ha.ckers.org/xss.js\"></SCRIPT>)\">", "<div>)\"&gt;");
    }

    [Test]
    public void Sanitize_CheatSheetStyleSheet()
    {
      var styleSettings = HtmlSanitizeSettings.Default();
      styleSettings.AllowedTags.Add("style");

      TestSanitize(@"<STYLE>@import'http://xss.rocks/xss.css';</STYLE>", @"<style></style>", styleSettings);
      TestSanitize(@"<STYLE>BODY{-moz-binding:url(""http://xss.rocks/xssmoz.xml#xss"")}</STYLE>", @"<style>BODY{}</style>", styleSettings);
      TestSanitize(@"<STYLE>@im\port'\ja\vasc\ript:alert(""XSS"")';</STYLE>", @"<style></style>", styleSettings);
      TestSanitize(@"<STYLE TYPE=""text/javascript"">alert('XSS');</STYLE>", @"<style type=""text/javascript"">;</style>", styleSettings);
      TestSanitize(@"<STYLE>.XSS{background-image:url(""javascript:alert('XSS')"");}</STYLE><A CLASS=XSS></A>", @"<style>.XSS{}</style><a></a>", styleSettings);
      TestSanitize(@"<STYLE type=""text/css"">BODY{background:url(""javascript:alert('XSS')"")}</STYLE>", @"<style type=""text/css"">BODY{}</style>", styleSettings);
    }

    [Test]
    public void Sanitize_CheatSheet_Meta()
    {
      TestSanitize(@"<p><LINK REL=""stylesheet"" HREF=""javascript:alert('XSS');""></p>", @"<p></p>");
      TestSanitize(@"<p><LINK REL=""stylesheet"" HREF=""http://xss.rocks/xss.css""></p>", @"<p></p>");
      TestSanitize(@"<p><META HTTP-EQUIV=""Link"" Content=""<http://xss.rocks/xss.css>; REL=stylesheet""></p>", @"<p></p>");
      TestSanitize(@"<p><META HTTP-EQUIV=""refresh"" CONTENT=""0;url=javascript:alert('XSS');""></p>", @"<p></p>");
      TestSanitize(@"<p><META HTTP-EQUIV=""refresh"" CONTENT=""0;url=data:text/html base64,PHNjcmlwdD5hbGVydCgnWFNTJyk8L3NjcmlwdD4K""></p>", @"<p></p>");
      TestSanitize(@"<p><META HTTP-EQUIV=""refresh"" CONTENT=""0; URL=http://;URL=javascript:alert('XSS');""></p>", @"<p></p>");
    }

    [Test]
    public void Sanitize_CheatSheet_Other()
    {
      TestSanitize(@"<p><iframe src=http://xss.rocks/scriptlet.html <</p>", "<p>");
      TestSanitize(@"<p><INPUT TYPE=""IMAGE"" SRC=""javascript:alert('XSS');""></p>", "<p><input type=\"IMAGE\"></p>");
      TestSanitize(@"<p><BGSOUND SRC=""javascript:alert('XSS');""></p>", "<p></p>");
      TestSanitize(@"<p><IFRAME SRC=""javascript:alert('XSS');""></IFRAME></p>", "<p></p>");
      TestSanitize(@"<p><IFRAME SRC=# onmouseover=""alert(document.cookie)""></IFRAME></p>", "<p></p>");
      TestSanitize(@"<p><FRAMESET><FRAME SRC=""javascript:alert('XSS');""></FRAMESET></p>", "<p></p>");
      TestSanitize(@"<p><TABLE BACKGROUND=""javascript:alert('XSS')""></TABLE></p>", "<p><table></table></p>");
      TestSanitize(@"<p><TABLE><TD BACKGROUND=""javascript:alert('XSS')""></TD></TABLE></p>", "<p><table><td></td></table></p>");
      TestSanitize(@"<p><!--[if gte IE 4]>
 <SCRIPT>alert('XSS');</SCRIPT>
 <![endif]--></p>", "<p></p>");
      TestSanitize(@"<p><BASE HREF=""javascript:alert('XSS');//""></p>", "<p></p>");
      TestSanitize(@"<p><OBJECT TYPE=""text/x-scriptlet"" DATA=""http://xss.rocks/scriptlet.html""></OBJECT></p>", "<p></p>");
      TestSanitize(@"<p><EMBED SRC=""http://ha.ckers.org/xss.swf"" AllowScriptAccess=""always""></p>", "<p></p>");
      TestSanitize(@"<p><XML ID=""xss""><I><B><IMG SRC=""javas<!-- -->cript:alert('XSS')""></B></I></XML></p>", "<p></p>");
      TestSanitize(@"<HTML><BODY>
<?xml:namespace prefix=""t"" ns=""urn:schemas-microsoft-com:time"">
<?import namespace=""t"" implementation=""#default#time2"">
<t:set attributeName=""innerHTML"" to=""XSS<SCRIPT DEFER>alert(""XSS"")</SCRIPT>"">
</BODY></HTML>", "");
      TestSanitize("<EMBED SRC=\"data:image/svg+xml;base64,PHN2ZyB4bWxuczpzdmc9Imh0dH A6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcv MjAwMC9zdmciIHhtbG5zOnhsaW5rPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hs aW5rIiB2ZXJzaW9uPSIxLjAiIHg9IjAiIHk9IjAiIHdpZHRoPSIxOTQiIGhlaWdodD0iMjAw IiBpZD0ieHNzIj48c2NyaXB0IHR5cGU9InRleHQvZWNtYXNjcmlwdCI+YWxlcnQoIlh TUyIpOzwvc2NyaXB0Pjwvc3ZnPg==\" type=\"image/svg+xml\" AllowScriptAccess=\"always\"></EMBED>", "");
      TestSanitize("<HTML xmlns:xss><?import namespace=\"xss\" implementation=\"http://ha.ckers.org/xss.htc\"><xss:xss>XSS</xss:xss></HTML>", "");

      var bgsoundSettings = HtmlSanitizeSettings.Default();
      bgsoundSettings.AllowedTags.Add("BGSOUND");
      TestSanitize(@"<p><BGSOUND SRC=""javascript:alert('XSS');""></p>", "<p><bgsound></p>", bgsoundSettings);
    }

    [Test]
    public void Sanitize_StyleSheet()
    {
      var sheet = @"<style>@import 'custom.css';
@import url(""chrome://communicator/skin/"");
@import ""common.css"" screen, projection;
@import url('landscape.css') screen and(orientation: landscape);

.XSS {
  background-image:url(""javascript: alert('XSS')"");
  color:red
}

@media (min-width:476px)
{
  .PageHeader .menu
  {
    display:inline-block;
    float:right;
  }
}</style>";
      var expected = "<style>\r\n\r\n\r\n\r\n\r\n.XSS {\r\n  \r\n  color:red\r\n}\r\n\r\n@media (min-width:476px)\r\n{\r\n  .PageHeader .menu\r\n  {\r\n    display:inline-block;\r\n    float:right;\r\n  }\r\n}</style>";

      var styleSettings = HtmlSanitizeSettings.Default();
      styleSettings.AllowedTags.Add("style");
      TestSanitize(sheet, expected, styleSettings);
    }
  }
}
