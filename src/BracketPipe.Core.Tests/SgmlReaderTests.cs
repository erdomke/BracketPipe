using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BracketPipe.Core.Tests
{
  /// <summary>
  /// Tests lifted from the SgmlReader project
  /// </summary>
  [TestFixture]
  class SgmlReaderTests
  {
    [Test]
    public void RoundTrip_Word()
    {
      var html = @"<html xmlns:v=""urn:schemas-microsoft-com:vml""
xmlns:o=""urn:schemas-microsoft-com:office:office""
xmlns:w=""urn:schemas-microsoft-com:office:word""
xmlns:m=""http://schemas.microsoft.com/office/2004/12/omml""
xmlns=""http://www.w3.org/TR/REC-html40"">

<body lang=EN-US style='tab-interval:.5in'>

<div class=WordSection1>

<p class=MsoNormal>Test <b style='mso-bidi-font-weight:normal'>document<o:p></o:p></b></p>

<p class=MsoListParagraphCxSpFirst style='text-indent:-.25in;mso-list:l0 level1 lfo1'><![if !supportLists]><span
style='font-family:Symbol;mso-fareast-font-family:Symbol;mso-bidi-font-family:
Symbol'><span style='mso-list:Ignore'>·<span style='font:7.0pt ""Times New Roman""'>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
</span></span></span><![endif]>With bullets</p>

</div>

</body>

</html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<html xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:w=""urn:schemas-microsoft-com:office:word"" xmlns:m=""http://schemas.microsoft.com/office/2004/12/omml"" xmlns=""http://www.w3.org/TR/REC-html40"">

<body lang=""EN-US"" style=""tab-interval:.5in"">

<div class=""WordSection1"">

<p class=""MsoNormal"">Test <b style=""mso-bidi-font-weight:normal"">document<o:p></o:p></b></p>

<p class=""MsoListParagraphCxSpFirst"" style=""text-indent:-.25in;mso-list:l0 level1 lfo1""><![if !supportLists]><span style=""font-family:Symbol;mso-fareast-font-family:Symbol;mso-bidi-font-family:
Symbol""><span style=""mso-list:Ignore"">·<span style=""font:7.0pt &quot;Times New Roman&quot;"">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
</span></span></span><![endif]>With bullets</p>

</div>

</body>

</html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader02()
    {
      var html = @"<html>
<body><span text=""foo>bar""/>
</body>
</html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<html>
<body><span text=""foo&gt;bar""></span>
</body>
</html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader03()
    {
      var html = @"<html>
<body><span text=""foo<bar""/>
</body>
</html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<html>
<body><span text=""foo&lt;bar""></span>
</body>
</html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader04()
    {
      var html = @"<html>
<body>
<tag>&test&nbsp&nbsp blah blah</tag>
</body>
</html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<html>
<body>
<tag>&amp;test&nbsp;&nbsp; blah blah</tag>
</body>
</html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader05()
    {
      var html = @"<html>
<body>
<p>bad char: <span>&#1048576;</span></p>
</body>
</html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<html>
<body>
<p>bad char: <span>􀀀</span></p>
</body>
</html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader06()
    {
      var html = @"<html>
<body>
<P class=MsoNormal dir=ltr
style=""MARGIN: 0pt;"" align=left><?xml:namespace
prefix = st1 ns = ""urn:schemas-microsoft-com:office:smarttags""
/><ST1:PERSONNAME></ST1:PERSONNAME></P>
</body>
</html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<html>
<body>
<p class=""MsoNormal"" dir=""ltr"" style=""MARGIN: 0pt;"" align=""left""><!--?xml:namespace
prefix = st1 ns = ""urn:schemas-microsoft-com:office:smarttags""
/--><st1:personname></st1:personname></p>
</body>
</html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader07()
    {
      var html = @"<html>
<body>
<DIV STYLE=""top:214px; left:139px; position:absolute; font-size:26px;""><NOBR><SPAN STYLE=""font-family:""Wingdings 2"";""></SPAN></NOBR></DIV>
</body>
</html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<html>
<body>
<div style=""top:214px; left:139px; position:absolute; font-size:26px;""><nobr><span style=""font-family:"" wingdings 2"";""></span></nobr></div>
</body>
</html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader08()
    {
      var html = @"<html>
<body>
<script type=""text/javascript"">/*<![CDATA[*/
var test = '<div>""test""</div>';
/*]]>*/</script>
<p>test</p>
</body>
</html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(html, rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader09()
    {
      var html = @"<html>
<body>This <P>is bad </P> XHTML.</body>
</html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<html>
<body>This <p>is bad </p> XHTML.</body>
</html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader11()
    {
      var html = @"<html>
<body><a href=""http://www.cnn.com/""' title=""cnn.com"">cnn</a></body>
</html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<html>
<body><a href=""http://www.cnn.com/"" title=""cnn.com"">cnn</a></body>
</html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader15()
    {
      var html = @"<html xmlns=""http://www.w3.org/1999/xhtml""><head /><body><table u1:str="""" x:str=""""></table></body></html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<html xmlns=""http://www.w3.org/1999/xhtml""><head></head><body><table u1:str x:str></table></body></html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader18()
    {
      var html = @"<html>
    <body>
        <script type=""text/javascript"">/*<![CDATA[*/ /*<![CDATA[*/ test /*]]>*/ /*]]&gt;*/</script>
    </body>
</html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<html>
    <body>
        <script type=""text/javascript"">/*<![CDATA[*/ /*<![CDATA[*/ test /*]]>*/ /*]]&gt;*/</script>
    </body>
</html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader21()
    {
      var html = @"<html><body>
<p class=""MsoNormal"">
  <span style=""font-size: 10pt;"" arial="""" ,="""" sans-serif="""" ;;="""" font-family:dummy:="""" font-family:="""" font-family:foo:="""" arial;="""" font-size:="""" 13.3333px;="""">
    <span class=""Apple-style-span"" style=""font-family: Arial; font-size: 13.3333px;"">-lm</span>
  </span>
</p>
</body></html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<html><body>
<p class=""MsoNormal"">
  <span style=""font-size: 10pt;"" arial , sans-serif ;; font-family:dummy: font-family: font-family:foo: arial; font-size: 13.3333px;>
    <span class=""Apple-style-span"" style=""font-family: Arial; font-size: 13.3333px;"">-lm</span>
  </span>
</p>
</body></html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader22()
    {
      var html = @"﻿<html><body>do <![if !supportLists]>not<![endif]> lose this text</body></html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"﻿<html><body>do <![if !supportLists]>not<![endif]> lose this text</body></html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader23()
    {
      var html = @"﻿<html xmlns=""http://implicit"" xmlns:n=""http://explicit""><foo attr1=""1"" n:attr2=""2"" /><n:foo attr1=""1"" n:attr2=""2"" /></html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"﻿<html xmlns=""http://implicit"" xmlns:n=""http://explicit""><foo attr1=""1"" n:attr2=""2""></foo><n:foo attr1=""1"" n:attr2=""2""></n:foo></html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader24()
    {
      var html = @"﻿<html xmlns:n=""http://explicit""><foo attr1=""1"" n:attr2=""2"" /><n:foo attr1=""1"" n:attr2=""2"" /></html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"﻿<html xmlns:n=""http://explicit""><foo attr1=""1"" n:attr2=""2""></foo><n:foo attr1=""1"" n:attr2=""2""></n:foo></html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader28()
    {
      var html = @"<html xmlns:o=""http://microsoft.com""><body>A<o:p></o:p>B<o:p></o:p></body></html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<html xmlns:o=""http://microsoft.com""><body>A<o:p></o:p>B<o:p></o:p></body></html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader33()
    {
      var html = @"<!DOCTYPE html PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"">
<html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<!DOCTYPE html PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"">
<html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader34()
    {
      var html = @"<HTML><BODY>
<LINK href=""a.css"" type=""text/css"" rel=""stylesheet"" />
</body>
</HTML> ";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<html><body>
<link href=""a.css"" type=""text/css"" rel=""stylesheet"">
</body>
</html> ", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader36()
    {
      var html = @"<html>
<head>
<script language=""JavaScript"">
<!--
--></script>
</head>
<body>
<p>hello</p>
</body>
</html> ";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<html>
<head>
<script language=""JavaScript"">
<!--
--></script>
</head>
<body>
<p>hello</p>
</body>
</html> ", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader39()
    {
      var html = @"<html><class=""black"">Text………</html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<html><class=""black"">Text………</class=""black""></html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader41()
    {
      var html = @"﻿<html>
  <img src=""img.gif"" height""4"" width= 2 >
</html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"﻿<html>
  <img src=""img.gif"" height""4"" width=""2"">
</html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader46()
    {
      var html = @"blah <b>foo</b>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(html, rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader48()
    {
      var html = @"﻿﻿<html>
<body>
<p>&#x5a;&#90;&#90 test &#90</p>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"﻿﻿<html>
<body>
<p>ZZZ test Z</p>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader55()
    {
      var html = @"﻿<html>
  <body style=""&amp;&quot;&lt;&gt;&apos;"">&amp;&quot;&lt;&gt;&apos;</body>
</html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"﻿<html>
  <body style=""&amp;&quot;&lt;&gt;'"">&amp;""&lt;&gt;'</body>
</html>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader57()
    {
      var html = @"<p>&#</p>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<p>&amp;#</p>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader58()
    {
      var html = @"<p>&#;</p>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<p>&amp;#;</p>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader59()
    {
      var html = @"<p>&#x</p>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<p>&amp;#x</p>", rendered);
      }
    }

    [Test]
    public void RoundTrip_SgmlReader60()
    {
      var html = @"<p>&#x;</p>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<p>&amp;#x;</p>", rendered);
      }
    }
  }
}
