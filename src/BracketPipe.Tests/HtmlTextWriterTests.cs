using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace BracketPipe.Core.Tests
{
  [TestFixture]
  class HtmlTextWriterTests
  {
    [Test]
    public void Format_Basic()
    {
      Assert.AreEqual("<p style=\"a&lt;&gt;&amp;&quot;&apos;b\">a&lt;&gt;&amp;&quot;&apos;b</p><div>c'd</div>", Html.Format("<p style=\"{0}\">{0}</p>{1:!}", "a<>&\"'b", "<div>c'd</div>"));
    }
    [Test]
    public void Format_Formattable()
    {
      var escape = "a<>&\"'b";
      var raw = "<div>c'd</div>";

      Assert.AreEqual("<p style=\"a&lt;&gt;&amp;&quot;&apos;b\">a&lt;&gt;&amp;&quot;&apos;b</p><div>c'd</div>", Html.Format((IFormattable)$"<p style=\"{escape}\">{escape}</p>{raw:!}"));
    }

    [Test]
    public void XmlToHtml()
    {
      using (var s = new StringWriter())
      using (var w = new HtmlTextWriter(s))
      {
        var xml = new XElement("body",
          new XElement("div", new XAttribute("style", "color:red"), "A > value"),
          new XElement("input", new XAttribute("type", "text"), new XAttribute("value", "start")),
          new XElement("p", "para & more"));

        xml.WriteTo(w);
        w.Flush();

        Assert.AreEqual("<body><div style=\"color:red\">A &gt; value</div><input type=\"text\" value=\"start\"><p>para &amp; more</p></body>", s.ToString());
      }
    }

    [Test]
    public void RoundTrip_Svg02()
    {
      //var html = @"<svg xmlns=""http://www.w3.org/2000/svg""><path d=""M182,65 L256,93 L354,65"" /></svg>";
      var html = @"<svg version=""1.1"" xmlns:user=""urn:user-scripts"" xmlns:msxsl=""urn:schemas-microsoft-com:xslt"" xmlns:aras=""http://www.aras.com"" xmlns:data=""http://www.aras.com/customer"" xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink""><defs><marker id=""Triangle"" viewBox=""0 0 10 6"" refX=""22"" refY=""3"" markerWidth=""10"" markerHeight=""6"" markerUnits=""userSpaceOnUse"" orient=""auto"" style=""fill:#999""><path d=""M 0 0 L 10 3 L 0 6 z"" /></marker></defs><path id=""path_74BAF626F20A48A59CEACE75D5CE166A"" style=""stroke: rgb(60%, 60%, 60%); fill:none; stroke-width: 1px; marker-end: url(#Triangle)"" d=""M182,65 L256,93 L354,65"" /><path id=""path_00A55285D7EE441E8D08F356B6BED0A9"" style=""stroke: rgb(60%, 60%, 60%); fill:none; stroke-width: 1px; marker-end: url(#Triangle)"" d=""M182,65 L182,20 L540,20"" /><path id=""path_0B14FEF62A4644A7A57903E4F41B0A30"" style=""stroke: rgb(60%, 60%, 60%); fill:none; stroke-width: 1px; marker-end: url(#Triangle)"" d=""M182,65 L182,165 L540,165"" /></svg>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.Minify().ToHtml();
        Assert.AreEqual(html, rendered);
      }
    }

    [Test]
    public void BuildHtml()
    {
      using (var s = new StringWriter())
      using (var w = new HtmlTextWriter(s))
      {
        w["html"]
          ["body"]
            ["div", "style", "color:red", "id", "1234"]
              .Text("A > value")
            ["/div"]
            ["input", "type", "text", "value", "start"]
            ["p"].Text("para & more")["/p"]
          ["/body"]
         ["/html"].Flush();
        Assert.AreEqual("<html><body><div style=\"color:red\" id=\"1234\">A &gt; value</div><input type=\"text\" value=\"start\"><p>para &amp; more</p></body></html>", s.ToString());
      }
    }

    [Test]
    public void WriterNamespaceDefault()
    {
      using (var s = new StringWriter())
      using (var w = new HtmlTextWriter(s))
      {
        w.WriteStartElement("root", "urn:1");
        w.WriteStartElement("item", "urn:2");
        w.WriteEndElement();
        w.WriteEndElement();
        w.Flush();
        var str = s.ToString();
        Assert.AreEqual("<root xmlns=\"urn:1\"><item xmlns=\"urn:2\"></item></root>", str);
      }
    }

    [Test]
    public void WriterNamespaceNestedSameDefault()
    {
      using (var s = new StringWriter())
      using (var w = new HtmlTextWriter(s))
      {
        w.WriteStartElement("root", "urn:1");
        w.WriteStartElement("item", "urn:1");
        w.WriteEndElement();
        w.WriteEndElement();
        w.Flush();
        var str = s.ToString();
        Assert.AreEqual("<root xmlns=\"urn:1\"><item></item></root>", str);
      }
    }

    [Test]
    public void WriterNamespaceManualDeclaration()
    {
      using (var s = new StringWriter())
      using (var w = new HtmlTextWriter(s))
      {
        w.WriteStartElement("root");
        w.WriteAttributeString("xmlns", "x", null, "urn:1");
        w.WriteStartElement("item", "urn:1");
        w.WriteEndElement();
        w.WriteStartElement("item", "urn:1");
        w.WriteEndElement();
        w.WriteEndElement();
        w.Flush();
        var str = s.ToString();
        Assert.AreEqual("<root xmlns:x=\"urn:1\"><x:item></x:item><x:item></x:item></root>", str);
      }
    }

    [Test]
    public void WriterNamespaceOverrideCurrent()
    {
      using (var s = new StringWriter())
      using (var w = new HtmlTextWriter(s))
      {
        w.WriteStartElement("x", "root", "123");
        w.WriteStartElement("item");
        w.WriteAttributeString("xmlns", "x", null, "abc");
        w.WriteEndElement();
        w.WriteEndElement();
        w.Flush();
        var str = s.ToString();
        Assert.AreEqual("<x:root xmlns:x=\"123\"><item xmlns:x=\"abc\"></item></x:root>", str);
      }
    }

    [Test]
    public void WriterNamespaceManualOverride()
    {
      using (var s = new StringWriter())
      using (var w = new HtmlTextWriter(s))
      {
        w.WriteStartElement("x", "node", "123");
        w.WriteAttributeString("xmlns", "x", null, "order");
        w.WriteEndElement();
        w.Flush();
        var str = s.ToString();
        Assert.AreEqual("<x:node xmlns:x=\"order\"></x:node>", str);
      }
    }

    [Test]
    public void WriterNamespaceMultipleDeclarations()
    {
      using (var s = new StringWriter())
      using (var w = new HtmlTextWriter(s))
      {
        w.WriteStartElement("x", "root", "urn:1");
        w.WriteStartElement("y", "item", "urn:1");
        w.WriteAttributeString("abc", "urn:1", "xyz");
        w.WriteEndElement();
        w.WriteEndElement();
        w.Flush();
        var str = s.ToString();
        Assert.AreEqual("<x:root xmlns:x=\"urn:1\"><y:item y:abc=\"xyz\" xmlns:y=\"urn:1\"></y:item></x:root>", str);
      }
    }

    [Test]
    public void WriterCharacterConversion()
    {
      using (var s = new StringWriter())
      using (var w = new HtmlTextWriter(s))
      {
        w.WriteStartElement("myRoot");
        w.WriteAttributeString("attr", "&\"");
        w.WriteString("<&>");
        w.WriteEndElement();
        w.Flush();
        var str = s.ToString();
        Assert.AreEqual("<myRoot attr=\"&amp;&quot;\">&lt;&amp;&gt;</myRoot>", str);
      }
    }

    [Test]
    public void WriterAttributeNoValue()
    {
      using (var s = new StringWriter())
      using (var w = new HtmlTextWriter(s))
      {
        w.WriteStartElement("input");
        w.WriteAttributeString("type", "text");
        w.WriteAttributeString("required", null);
        w.WriteEndElement();
        w.Flush();
        var str = s.ToString();
        Assert.AreEqual("<input type=\"text\" required>", str);
      }
    }

    [Test]
    public void RoundTrip_Basic()
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
        var rendered = reader.ToHtml();
        Assert.AreEqual(html, rendered);
      }
    }

    [Test]
    public void RoundTrip_VoidElementCloseTag()
    {
      var html = @"<p><img src></img></p>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<p><img src></p>", rendered);
      }
    }

    [Test]
    public void RoundTrip_Svg()
    {
      var html = @"<html>
<head>
  <title>SVG test</title>
  <meta name=""description"" content=""The HTML5 Herald"">
  <meta name=""author"" content=""SitePoint"">

</head>
<body>
  <input type=""checkbox"" checked>
<svg version=""1.1"" id=""Layer_1"" xmlns=""http://www.w3.org/2000/svg"" xmlns:xlink=""http://www.w3.org/1999/xlink"" x=""0px"" y=""0px"" width=""612px"" height=""502.174px"" viewBox=""0 65.326 612 502.174"" enable-background=""new 0 65.326 612 502.174"" xml:space=""preserve"" class=""logo"">

<ellipse class=""ground"" cx=""283.5"" cy=""487.5"" rx=""259"" ry=""80"" />
<path class=""kiwi"" d=""M210.333,65.331C104.367,66.105-12.349,150.637,1.056,276.449c4.303,40.393,18.533,63.704,52.171,79.03
  c36.307,16.544,57.022,54.556,50.406,112.954c-9.935,4.88-17.405,11.031-19.132,20.015c7.531-0.17,14.943-0.312,22.59,4.341
  c20.333,12.375,31.296,27.363,42.979,51.72c1.714,3.572,8.192,2.849,8.312-3.078c0.17-8.467-1.856-17.454-5.226-26.933
  c-2.955-8.313,3.059-7.985,6.917-6.106c6.399,3.115,16.334,9.43,30.39,13.098c5.392,1.407,5.995-3.877,5.224-6.991
  c-1.864-7.522-11.009-10.862-24.519-19.229c-4.82-2.984-0.927-9.736,5.168-8.351l20.234,2.415c3.359,0.763,4.555-6.114,0.882-7.875
  c-14.198-6.804-28.897-10.098-53.864-7.799c-11.617-29.265-29.811-61.617-15.674-81.681c12.639-17.938,31.216-20.74,39.147,43.489
  c-5.002,3.107-11.215,5.031-11.332,13.024c7.201-2.845,11.207-1.399,14.791,0c17.912,6.998,35.462,21.826,52.982,37.309
  c3.739,3.303,8.413-1.718,6.991-6.034c-2.138-6.494-8.053-10.659-14.791-20.016c-3.239-4.495,5.03-7.045,10.886-6.876
  c13.849,0.396,22.886,8.268,35.177,11.218c4.483,1.076,9.741-1.964,6.917-6.917c-3.472-6.085-13.015-9.124-19.18-13.413
  c-4.357-3.029-3.025-7.132,2.697-6.602c3.905,0.361,8.478,2.271,13.908,1.767c9.946-0.925,7.717-7.169-0.883-9.566
  c-19.036-5.304-39.891-6.311-61.665-5.225c-43.837-8.358-31.554-84.887,0-90.363c29.571-5.132,62.966-13.339,99.928-32.156
  c32.668-5.429,64.835-12.446,92.939-33.85c48.106-14.469,111.903,16.113,204.241,149.695c3.926,5.681,15.819,9.94,9.524-6.351
  c-15.893-41.125-68.176-93.328-92.13-132.085c-24.581-39.774-14.34-61.243-39.957-91.247
  c-21.326-24.978-47.502-25.803-77.339-17.365c-23.461,6.634-39.234-7.117-52.98-31.273C318.42,87.525,265.838,64.927,210.333,65.331
  z M445.731,203.01c6.12,0,11.112,4.919,11.112,11.038c0,6.119-4.994,11.111-11.112,11.111s-11.038-4.994-11.038-11.111
  C434.693,207.929,439.613,203.01,445.731,203.01z"" />
<filter id=""pictureFilter"">
  <feGaussianBlur stdDeviation=""15"" />
</filter>
</svg>
</body>
</html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(html, rendered);
      }
    }

    [Test]
    public void RoundTrip_EntityName()
    {
      var html = @"<p>a&PlusMinus;b</p>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(@"<p>a±b</p>", rendered);
      }
    }


    [Test]
    public void RoundTrip_MathMl()
    {
      var html = @"<html xmlns=""http://www.w3.org/1999/xhtml"" lang=""en"" xml:lang=""en"">

<head>
<title>MathML's Hello Square</title>
</head>

<body>

<p> This is a perfect square:</p>

<math xmlns=""http://www.w3.org/1998/Math/MathML"">
  <mrow>
   <msup>
     <mfenced>
       <mrow>
         <mi>a</mi>
         <mo>+</mo>
         <mi>b</mi>
       </mrow>
     </mfenced>
     <mn>2</mn>
   </msup>
 </mrow>
</math>

</body>
</html>";
      using (var reader = new HtmlReader(html))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(html, rendered);
      }
    }
  }
}
