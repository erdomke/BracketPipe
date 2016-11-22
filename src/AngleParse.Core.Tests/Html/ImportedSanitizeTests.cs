using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngleParse.Core.Tests.Html
{
  public class HtmlSanitize
  {
    private HtmlSanitizeSettings _settings = HtmlSanitizeSettings.Default();

    public string Sanitize(string html)
    {
      using (var reader = new HtmlReader(html))
      {
        return reader.Sanitize(_settings).ToHtml();
      }
    }
  }

  /// <summary>
  /// Tests for <see cref="HtmlSanitizer"/>.
  /// </summary>
  [TestFixture]
  public class HtmlSanitizerTests
  {
    public HtmlSanitize Sanitizer { get; set; }

    public HtmlSanitizerTests()
    {
      Sanitizer = new HtmlSanitize();
    }

    /// <summary>
    /// A test for Xss locator
    /// </summary>
    [Test]
    public void XSSLocatorTest()
    {
      // Arrange
      var sanitizer = Sanitizer;

      // Act
      string htmlFragment = "<a href=\"'';!--\"<XSS>=&{()}\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = @"<a href=""'';!--"">=&amp;{()}""&gt;";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector
    /// Example <!-- <IMG SRC="javascript:alert('XSS');"> -->
    /// </summary>
    [Test]
    public void ImageXSS1Test()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Action
      string htmlFragment = "<IMG SRC=\"javascript:alert('XSS');\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector without quotes and semicolon.
    /// Example <!-- <IMG SRC=javascript:alert('XSS')> -->
    /// </summary>
    [Test]
    public void ImageXSS2Test()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC=javascript:alert('XSS')>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image xss vector with case insensitive.
    /// Example <!-- <IMG SRC=JaVaScRiPt:alert('XSS')> -->
    /// </summary>
    [Test]
    public void ImageCaseInsensitiveXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC=JaVaScRiPt:alert('XSS')>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with Html entities
    /// Example <!-- <IMG SRC=javascript:alert(&quot;XSS&quot;)> -->
    /// </summary>
    [Test]
    public void ImageHtmlEntitiesXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC=javascript:alert(&quot;XSS&quot;)>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with grave accent
    /// Example <!-- <IMG SRC=`javascript:alert("RSnake says, 'XSS'")`> -->
    /// </summary>
    [Test]
    public void ImageGraveAccentXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC=`javascript:alert(\"RSnake says, 'XSS'\")`>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with malformed
    /// Example <!-- <IMG \"\"\"><SCRIPT>alert(\"XSS\")</SCRIPT>\"> -->
    /// </summary>
    [Test]
    public void ImageMalformedXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG \"\"\"><SCRIPT>alert(\"XSS\")</SCRIPT>\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "\"&gt;";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with ImageFromCharCode
    /// Example <!-- <IMG SRC=javascript:alert(String.fromCharCode(88,83,83))> -->
    /// </summary>
    [Test]
    public void ImageFromCharCodeXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC=javascript:alert(String.fromCharCode(88,83,83))>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with UTF-8 Unicode
    /// Example <!-- <IMG SRC=&#106;&#97;&#118;&#97;&#115;&#99;&#114;&#105;&#112;&#116;&#58;&#97;&#108;&#101;&#114;&#116;&#40;&#39;&#88;&#83;&#83;&#39;&#41;> -->
    /// </summary>
    [Test]
    public void ImageUTF8UnicodeXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC=&#106;&#97;&#118;&#97;&#115;&#99;&#114;&#105;&#112;&#116;&#58;&#97;&#108;&#101;&#114;&#116;&#40;&#39;&#88;&#83;&#83;&#39;&#41;>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with Long UTF-8 Unicode
    /// Example <!-- <IMG SRC=&#0000106&#0000097&#0000118&#0000097&#0000115&#0000099&#0000114&#0000105&#0000112&#0000116&#0000058&#0000097&#0000108&#0000101&#0000114&#0000116&#0000040&#0000039&#0000088&#0000083&#0000083&#0000039&#0000041> -->
    /// </summary>
    [Test]
    public void ImageLongUTF8UnicodeXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC=&#0000106&#0000097&#0000118&#0000097&#0000115&#0000099&#0000114&#0000105&#0000112&#0000116&#0000058&#0000097&#0000108&#0000101&#0000114&#0000116&#0000040&#0000039&#0000088&#0000083&#0000083&#0000039&#0000041>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with Hex encoding without semicolon
    /// Example <!-- <IMG SRC=&#x6A&#x61&#x76&#x61&#x73&#x63&#x72&#x69&#x70&#x74&#x3A&#x61&#x6C&#x65&#x72&#x74&#x28&#x27&#x58&#x53&#x53&#x27&#x29> -->
    /// </summary>
    [Test]
    public void ImageHexEncodeXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC=&#x6A&#x61&#x76&#x61&#x73&#x63&#x72&#x69&#x70&#x74&#x3A&#x61&#x6C&#x65&#x72&#x74&#x28&#x27&#x58&#x53&#x53&#x27&#x29>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with embedded tab
    /// Example <!-- <IMG SRC=\"jav	ascript:alert('XSS');\"> -->
    /// </summary>
    [Test]
    public void ImageEmbeddedTabXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC=\"jav	ascript:alert('XSS');\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with embedded encoded tab
    /// Example <!-- <IMG SRC="jav&#x09;ascript:alert('XSS');"> -->
    /// </summary>
    [Test]
    public void ImageEmbeddedEncodedTabXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC=\"jav&#x09;ascript:alert('XSS');\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with embedded new line
    /// Example <!-- <IMG SRC="jav&#x0A;ascript:alert('XSS');"> -->
    /// </summary>
    [Test]
    public void ImageEmbeddedNewLineXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC=\"jav&#x0A;ascript:alert('XSS');\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with embedded carriage return
    /// Example <!-- <IMG SRC=\"jav&#x0D;ascript:alert('XSS');\"> -->
    /// </summary>
    [Test]
    public void ImageEmbeddedCarriageReturnXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC=\"jav&#x0D;ascript:alert('XSS');\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with Multiline using ASCII carriage return
    /// Example <!-- <IMG
    /// SRC
    /// =
    /// "
    /// j
    /// a
    /// v
    /// a
    /// s
    /// c
    /// r
    /// i
    /// p
    /// t
    /// :
    /// a
    /// l
    /// e
    /// r
    /// t
    /// (
    /// '
    /// X
    /// S
    /// S
    /// '
    /// )
    /// "
    ///> -->
    /// </summary>
    [Test]
    public void ImageMultilineInjectedXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = @"<IMG
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
>
";

      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "\r\n";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with Null breaks up Javascript directive
    /// Example <!-- perl -e 'print "<IMG SRC=java\0script:alert(\"XSS\")>";' > out -->
    /// </summary>
    [Test]
    public void ImageNullBreaksUpXSSTest1()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC=java\0script:alert(\"XSS\")>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with Null breaks up cross site scripting vector
    /// Example <!-- <image src=" perl -e 'print "<SCR\0IPT>alert(\"XSS\")</SCR\0IPT>";' > out "> -->
    /// </summary>
    [Test]
    public void ImageNullBreaksUpXSSTest2()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<SCR\0IPT>alert(\"XSS\")</SCR\0IPT>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with spaces and Meta characters
    /// Example <!-- <IMG SRC=" &#14;  javascript:alert('XSS');"> -->
    /// </summary>
    [Test]
    public void ImageSpaceAndMetaCharXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC=\" &#14;  javascript:alert('XSS');\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with half open html
    /// Example <!-- <IMG SRC="javascript:alert('XSS')" -->
    /// </summary>
    [Test]
    public void ImageHalfOpenHtmlXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC=\"javascript:alert('XSS')\"";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with double open angle bracket
    /// Example <!-- <image src=http://ha.ckers.org/scriptlet.html < -->
    /// </summary>
    [Test]
    public void ImageDoubleOpenAngleBracketXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;

      // Act
      string htmlFragment = "<image src=http://ha.ckers.org/scriptlet.html <";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Dic Xss vector with Javascript escaping
    /// Example <!-- <div style="\";alert('XSS');//"> -->
    /// </summary>
    [Test]
    public void DivJavascriptEscapingXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<div style=\"\";alert('XSS');//\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with input image
    /// Example <!-- <INPUT TYPE="IMAGE" SRC="javascript:alert('XSS');"> -->
    /// </summary>
    [Test]
    public void ImageInputXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<INPUT TYPE=\"IMAGE\" SRC=\"javascript:alert('XSS');\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<input type=\"IMAGE\">";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with Dynsrc
    /// Example <!-- <IMG DYNSRC="javascript:alert('XSS')"> -->
    /// </summary>
    [Test]
    public void ImageDynsrcXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG DYNSRC=\"javascript:alert('XSS')\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image Xss vector with Lowsrc
    /// Example <!-- <IMG LOWSRC="javascript:alert('XSS')"> -->
    /// </summary>
    [Test]
    public void ImageLowsrcXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG LOWSRC=\"javascript:alert('XSS')\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Xss vector with BGSound
    /// Example <!-- <BGSOUND SRC="javascript:alert('XSS');"> -->
    /// </summary>
    [Test]
    public void BGSoundXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<BGSOUND SRC=\"javascript:alert('XSS');\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for BR with Javascript Include
    /// Example <!-- <BR SIZE="&{alert('XSS')}"> -->
    /// </summary>
    [Test]
    public void BRJavascriptIncludeXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<BR SIZE=\"&{alert('XSS')}\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<br>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for P with url in style
    /// Example <!-- <p STYLE="behavior: url(www.ha.ckers.org);"> -->
    /// </summary>
    [Test]
    public void PWithUrlInStyleXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<p STYLE=\"behavior: url(www.ha.ckers.org);\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      // intentionally keep it failing to get notice when reviewing unit tests so can disucss
      string expected = "<p>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image with vbscript
    /// Example <!-- <IMG SRC='vbscript:msgbox("XSS")'> -->
    /// </summary>
    [Test]
    public void ImageWithVBScriptXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC='vbscript:msgbox(\"XSS\")'>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image with Mocha
    /// Example <!-- <IMG SRC="mocha:[code]"> -->
    /// </summary>
    [Test]
    public void ImageWithMochaXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC=\"mocha:[code]\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image with Livescript
    /// Example <!-- <IMG SRC="Livescript:[code]"> -->
    /// </summary>
    [Test]
    public void ImageWithLivescriptXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG SRC=\"Livescript:[code]\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Iframe
    /// Example <!-- <IFRAME SRC="javascript:alert('XSS');"></IFRAME> -->
    /// </summary>
    [Test]
    public void IframeXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IFRAME SRC=\"javascript:alert('XSS');\"></IFRAME>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Frame
    /// Example <!-- <FRAMESET><FRAME SRC="javascript:alert('XSS');"></FRAMESET> -->
    /// </summary>
    [Test]
    public void FrameXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<FRAMESET><FRAME SRC=\"javascript:alert('XSS');\"></FRAMESET>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Table
    /// Example <!-- <TABLE BACKGROUND="javascript:alert('XSS')"> -->
    /// </summary>
    [Test]
    public void TableXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<TABLE BACKGROUND=\"javascript:alert('XSS')\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<table>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for TD
    /// Example <!-- <TABLE><TD BACKGROUND="javascript:alert('XSS')"> -->
    /// </summary>
    [Test]
    public void TDXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<TABLE><TD BACKGROUND=\"javascript:alert('XSS')\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<table><td>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div Background Image
    /// Example <!-- <DIV STYLE="background-image: url(javascript:alert('XSS'))"> -->
    /// </summary>
    [Test]
    public void DivBackgroundImageXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<DIV STYLE=\"background-image: url(javascript:alert('XSS'))\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div Background Image  with unicoded XSS
    /// Example <!-- <DIV STYLE="background-image:\0075\0072\006C\0028'\006a\0061\0076\0061\0073\0063\0072\0069\0070\0074\003a\0061\006c\0065\0072\0074\0028.1027\0058.1053\0053\0027\0029'\0029"> -->
    /// </summary>
    [Test]
    public void DivBackgroundImageWithUnicodedXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = @"<DIV STYLE=""background-image:\0075\0072\006C\0028'\006a\0061\0076\0061\0073\0063\0072\0069\0070\0074\003a\0061\006c\0065\0072\0074\0028\0027\0058\0053\0053\0027\0029'\0029"">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div Background Image  with extra characters
    /// Example <!-- <DIV STYLE="background-image: url(&#1;javascript:alert('XSS'))"> -->
    /// </summary>
    [Test]
    public void DivBackgroundImageWithExtraCharactersXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<DIV STYLE=\"background-image: url(&#1;javascript:alert('XSS'))\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for DIV expression
    /// Example <!-- <DIV STYLE="width: expression(alert('XSS'));"> -->
    /// </summary>
    [Test]
    public void DivExpressionXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<DIV STYLE=\"width: expression(alert('XSS'));\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Image with break up expression
    /// Example <!-- <IMG STYLE="xss:expr/*XSS*/ession(alert('XSS'))"> -->
    /// </summary>
    [Test]
    public void ImageStyleExpressionXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<IMG STYLE=\"xss:expr/*XSS*/ession(alert('XSS'))\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for AnchorTag with break up expression
    /// Example <!-- exp/*<A STYLE='no\xss:noxss("*//*");xss:&#101;x&#x2F;*XSS*//*/*/pression(alert("XSS"))'> -->
    /// </summary>
    [Test]
    public void AnchorTagStyleExpressionXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "exp/*<A STYLE='no\\xss:noxss(\"*//*\");xss:&#101;x&#x2F;*XSS*//*/*/pression(alert(\"XSS\"))'>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "exp/*<a>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for BaseTag
    /// Example <!-- <BASE HREF="javascript:alert('XSS');//"> -->
    /// </summary>
    [Test]
    public void BaseTagXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<BASE HREF=\"javascript:alert('XSS');//\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for EMBEDTag
    /// Example <!-- <EMBED SRC="http://ha.ckers.org/xss.swf" AllowScriptAccess="always"></EMBED> -->
    /// </summary>
    [Test]
    public void EmbedTagXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<EMBED SRC=\"http://ha.ckers.org/xss.swf\" AllowScriptAccess=\"always\"></EMBED>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for EMBEDSVG
    /// Example <!-- <EMBED SRC="data:image/svg+xml;base64,PHN2ZyB4bWxuczpzdmc9Imh0dH A6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcv MjAwMC9zdmciIHhtbG5zOnhsaW5rPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hs aW5rIiB2ZXJzaW9uPSIxLjAiIHg9IjAiIHk9IjAiIHdpZHRoPSIxOTQiIGhlaWdodD0iMjAw IiBpZD0ieHNzIj48c2NyaXB0IHR5cGU9InRleHQvZWNtYXNjcmlwdCI+YWxlcnQoIlh TUyIpOzwvc2NyaXB0Pjwvc3ZnPg==" type="image/svg+xml" AllowScriptAccess="always"></EMBED> -->
    /// </summary>
    [Test]
    public void EmbedSVGXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<EMBED SRC=\"data:image/svg+xml;base64,PHN2ZyB4bWxuczpzdmc9Imh0dH A6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcv MjAwMC9zdmciIHhtbG5zOnhsaW5rPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5L3hs aW5rIiB2ZXJzaW9uPSIxLjAiIHg9IjAiIHk9IjAiIHdpZHRoPSIxOTQiIGhlaWdodD0iMjAw IiBpZD0ieHNzIj48c2NyaXB0IHR5cGU9InRleHQvZWNtYXNjcmlwdCI+YWxlcnQoIlh TUyIpOzwvc2NyaXB0Pjwvc3ZnPg==\" type=\"image/svg+xml\" AllowScriptAccess=\"always\"></EMBED>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for XML namespace
    /// Example <!-- <HTML xmlns:xss>  <?import namespace="xss" implementation="http://ha.ckers.org/xss.htc">  <xss:xss>XSS</xss:xss></HTML> -->
    /// </summary>
    [Test]
    public void XmlNamespaceXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<HTML xmlns:xss><?import namespace=\"xss\" implementation=\"http://ha.ckers.org/xss.htc\"><xss:xss>XSS</xss:xss></HTML>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for XML with CData
    /// Example <!-- <XML ID=I><X><C><![CDATA[<IMG SRC="javas]]><![CDATA[cript:alert('XSS');">]]></C></X></xml><SPAN DATASRC=#I DATAFLD=C DATAFORMATAS=HTML></SPAN> -->
    /// </summary>
    [Test]
    public void XmlWithCDataXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<XML ID=I><X><C><![CDATA[<IMG SRC=\"javas]]><![CDATA[cript:alert('XSS');\">]]></C></X></xml><SPAN DATASRC=#I DATAFLD=C DATAFORMATAS=HTML></SPAN>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<span></span>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for XML with Comment obfuscation
    /// </summary>
    [Test]
    public void XmlWithCommentObfuscationXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<XML ID=\"xss\"><I><B>&lt;IMG SRC=\"javas<!-- -->cript:alert('XSS')\"&gt;</B></I></XML><SPAN DATASRC=\"#xss\" DATAFLD=\"B\" DATAFORMATAS=\"HTML\"></SPAN>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<span></span>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for XML with Embedded script
    /// Example <!-- <XML SRC="xsstest.xml" ID=I></XML><SPAN DATASRC=#I DATAFLD=C DATAFORMATAS=HTML></SPAN> -->
    /// </summary>
    [Test]
    public void XmlWithEmbeddedScriptXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<XML SRC=\"xsstest.xml\" ID=I></XML><SPAN DATASRC=#I DATAFLD=C DATAFORMATAS=HTML></SPAN>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<span></span>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Html + Time
    /// Example <!-- <HTML><BODY><?xml:namespace prefix="t" ns="urn:schemas-microsoft-com:time"><?import namespace="t" implementation="#default#time2"><t:set attributeName="innerHTML" to="XSS&lt;SCRIPT DEFER&gt;alert(&quot;XSS&quot;)&lt;/SCRIPT&gt;"></BODY></HTML> -->
    /// </summary>
    [Test]
    public void HtmlPlusTimeXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<HTML><BODY><?xml:namespace prefix=\"t\" ns=\"urn:schemas-microsoft-com:time\"><?import namespace=\"t\" implementation=\"#default#time2\"><t:set attributeName=\"innerHTML\" to=\"XSS&lt;SCRIPT DEFER&gt;alert(&quot;XSS&quot;)&lt;/SCRIPT&gt;\"></BODY></HTML>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for AnchorTag with javascript link location
    /// Example <!-- <A HREF="javascript:document.location='http://www.google.com/'">XSS</A> -->
    /// </summary>
    [Test]
    public void AnchorTagJavascriptLinkLocationXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<A HREF=\"javascript:document.location='http://www.google.com/'\">XSS</A>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<a>XSS</a>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div with no filter evasion
    /// Example <!-- <Div style="background-color: http://www.codeplex.com?url=<SCRIPT SRC=http://ha.ckers.org/xss.js></SCRIPT>"> -->
    /// </summary>
    [Test]
    public void DivNoFilterEvasionXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<Div style=\"background-color: http://www.codeplex.com?url=<SCRIPT SRC=http://ha.ckers.org/xss.js></SCRIPT>\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div with style expression and no filter evasion
    /// Example <!-- <Div style="background-color: expression(<SCRIPT SRC=http://ha.ckers.org/xss.js></SCRIPT>)"> -->
    /// </summary>
    [Test]
    public void DivStyleExpressionNoFilterEvasionXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<Div style=\"background-color: expression(<SCRIPT SRC=http://ha.ckers.org/xss.js></SCRIPT>)\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for AnchorTag with non alpha non digit xss
    /// Example <!-- <A HREF="http://www.codeplex.com?url=<SCRIPT/XSS SRC="http://ha.ckers.org/xss.js"></SCRIPT>">XSS</A> -->
    /// </summary>
    [Test]
    public void AnchorTagNonAlphaNonDigitXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<A HREF=\"http://www.codeplex.com?url=<SCRIPT/XSS SRC=\"http://ha.ckers.org/xss.js\"></SCRIPT>\">XSS</A>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = @"<a href=""http://www.codeplex.com/?url=%3CSCRIPT/XSS%20SRC="">""&gt;XSS</a>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div with non alpha non digit xss
    /// Example <!-- <Div style="background-color: http://www.codeplex.com?url=<SCRIPT/XSS SRC=http://ha.ckers.org/xss.js></SCRIPT>"> -->
    /// </summary>
    [Test]
    public void DivNonAlphaNonDigitXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<Div style=\"background-color: http://www.codeplex.com?url=<SCRIPT/XSS SRC=\"http://ha.ckers.org/xss.js\"></SCRIPT>\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>\"&gt;";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div with style expression and non alpha non digit xss
    /// Example <!-- <Div style="background-color: expression(<SCRIPT/XSS SRC="http://ha.ckers.org/xss.js"></SCRIPT>)"> -->
    /// </summary>
    [Test]
    public void DivStyleExpressionNonAlphaNonDigitXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<Div style=\"background-color: expression(<SCRIPT/XSS SRC=\"http://ha.ckers.org/xss.js\"></SCRIPT>)\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>)\"&gt;";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div with non alpha non digit part 3 xss
    /// Example <!-- <Div style="background-color: http://www.codeplex.com?url=<SCRIPT/SRC=http://ha.ckers.org/xss.js></SCRIPT>"> -->
    /// </summary>
    [Test]
    public void DivNonAlphaNonDigit3XSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<Div style=\"background-color: http://www.codeplex.com?url=<SCRIPT/SRC=\"http://ha.ckers.org/xss.js\"></SCRIPT>\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>\"&gt;";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div with style expression and non alpha non digit part 3 xss
    /// Example <!-- <Div style="background-color: expression(<SCRIPT/SRC="http://ha.ckers.org/xss.js"></SCRIPT>)"> -->
    /// </summary>
    [Test]
    public void DivStyleExpressionNonAlphaNonDigit3XSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<Div style=\"background-color: expression(<SCRIPT/SRC=\"http://ha.ckers.org/xss.js\"></SCRIPT>)\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>)\"&gt;";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for AnchorTag with Extraneous open brackets xss
    /// Example <!-- <A HREF="http://www.codeplex.com?url=<<SCRIPT>alert("XSS");//<</SCRIPT>">XSS</A> -->
    /// </summary>
    [Test]
    public void AnchorTagExtraneousOpenBracketsXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<A HREF=\"http://www.codeplex.com?url=<<SCRIPT>alert(\"XSS\");//<</SCRIPT>\">XSS</A>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = @"<a href=""http://www.codeplex.com/?url=%3C%3CSCRIPT%3Ealert("">""&gt;XSS</a>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div with Extraneous open brackets xss
    /// Example <!-- <Div style="background-color: http://www.codeplex.com?url=<<SCRIPT>alert("XSS");//<</SCRIPT>"> -->
    /// </summary>
    [Test]
    public void DivExtraneousOpenBracketsXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<Div style=\"background-color: http://www.codeplex.com?url=<<SCRIPT>alert(\"XSS\");//<</SCRIPT>\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>\"&gt;";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div with style expression and Extraneous open brackets xss
    /// Example <!-- <Div style="background-color: expression(<<SCRIPT>alert("XSS");//<</SCRIPT>)"> -->
    /// </summary>
    [Test]
    public void DivStyleExpressionExtraneousOpenBracketsXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;

      // Act
      string htmlFragment = "<Div style=\"background-color: expression(<<SCRIPT>alert(\"XSS\");//<</SCRIPT>)\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>)\"&gt;";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div with No closing script tags xss
    /// Example <!-- <Div style="background-color: http://www.codeplex.com?url=<SCRIPT SRC=http://ha.ckers.org/xss.js?<B>"> -->
    /// </summary>
    [Test]
    public void DivNoClosingScriptTagsXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<Div style=\"background-color: http://www.codeplex.com?url=<SCRIPT SRC=http://ha.ckers.org/xss.js?<B>\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div with style expression and No closing script tags xss
    /// Example <!-- <Div style="background-color: expression(<SCRIPT SRC=http://ha.ckers.org/xss.js?<B>)"> -->
    /// </summary>
    [Test]
    public void DivStyleExpressionNoClosingScriptTagsXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<Div style=\"background-color: expression(<SCRIPT SRC=http://ha.ckers.org/xss.js?<B>)\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for AnchorTag with Protocol resolution in script tags xss
    /// Example <!-- <A HREF="http://www.codeplex.com?url=<SCRIPT SRC=//ha.ckers.org/.j>">XSS</A> -->
    /// </summary>
    [Test]
    public void AnchorTagProtocolResolutionScriptXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<A HREF=\"http://www.codeplex.com?url=<SCRIPT SRC=//ha.ckers.org/.j>\">XSS</A>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = @"<a href=""http://www.codeplex.com/?url=%3CSCRIPT%20SRC=//ha.ckers.org/.j%3E"">XSS</a>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div with Protocol resolution in script tags xss
    /// Example <!-- <Div style="background-color: http://www.codeplex.com?url=<SCRIPT SRC=//ha.ckers.org/.j>"> -->
    /// </summary>
    [Test]
    public void DivProtocolResolutionScriptXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<Div style=\"background-color: http://www.codeplex.com?url=<SCRIPT SRC=//ha.ckers.org/.j>\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div with style expression and Protocol resolution in script tags xss
    /// Example <!-- <Div style="background-color: expression(<SCRIPT SRC=//ha.ckers.org/.j>)"> -->
    /// </summary>
    [Test]
    public void DivStyleExpressionProtocolResolutionScriptXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<Div style=\"background-color: expression(<SCRIPT SRC=//ha.ckers.org/.j>)\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for AnchorTag with no single quotes or double quotes or semicolons xss
    /// Example <!-- <A HREF="http://www.codeplex.com?url=<SCRIPT>a=/XSS/alert(a.source)</SCRIPT>">XSS</A> -->
    /// </summary>
    [Test]
    public void AnchorTagNoQuotesXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<A HREF=\"http://www.codeplex.com?url=<SCRIPT>a=/XSS/alert(a.source)</SCRIPT>\">XSS</A>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = @"<a href=""http://www.codeplex.com/?url=%3CSCRIPT%3Ea=/XSS/alert(a.source)%3C/SCRIPT%3E"">XSS</a>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div with no single quotes or double quotes or semicolons xss
    /// Example <!-- <Div style="background-color: http://www.codeplex.com?url=<SCRIPT>a=/XSS/alert(a.source)</SCRIPT>"> -->
    /// </summary>
    [Test]
    public void DivNoQuotesXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<Div style=\"background-color: http://www.codeplex.com?url=<SCRIPT>a=/XSS/alert(a.source)</SCRIPT>\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div with style expression and no single quotes or double quotes or semicolons xss
    /// Example <!-- <Div style="background-color: expression(<SCRIPT>a=/XSS/alert(a.source)</SCRIPT>)"> -->
    /// </summary>
    [Test]
    public void DivStyleExpressionNoQuotesXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<Div style=\"background-color: expression(<SCRIPT>a=/XSS/alert(a.source)</SCRIPT>)\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for AnchorTag with US-ASCII encoding xss
    /// Example <!-- <A HREF="http://www.codeplex.com?url=¼script¾alert(¢XSS¢)¼/script¾">XSS</A> -->
    /// </summary>
    [Test]
    public void AnchorTagUSASCIIEncodingXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<A HREF=\"http://www.codeplex.com?url=¼script¾alert(¢XSS¢)¼/script¾\">XSS</A>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = @"<a href=""http://www.codeplex.com/?url=%C2%BCscript%C2%BEalert(%C2%A2XSS%C2%A2)%C2%BC/script%C2%BE"">XSS</a>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for AnchorTag with Downlevel-Hidden block xss
    /// </summary>
    [Test]
    public void AnchorTagDownlevelHiddenBlockXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<A HREF=\"http://www.codeplex.com?url=<!--[if gte IE 4]><SCRIPT>alert('XSS');</SCRIPT><![endif]-->\">XSS</A>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = @"<a href=""http://www.codeplex.com/?url=%3C!--[if%20gte%20IE%204]%3E%3CSCRIPT%3Ealert('XSS');%3C/SCRIPT%3E%3C![endif]--%3E"">XSS</a>";

      try
      {
        Assert.AreEqual(expected, actual);
      }
      catch (Exception)
      {

        //in .net 3.5 there is a bug with URI, and so this test would otherwise fail on .net 3.5 in Appveyor / nunit:
        //http://help.appveyor.com/discussions/problems/1625-nunit-not-picking-up-net-framework-version
        //http://stackoverflow.com/questions/27019061/forcing-nunit-console-runner-to-use-clr-4-5
        string expectedNet35 = @"<a href=""http://www.codeplex.com/?url=%3C!--%5Bif%20gte%20IE%204%5D%3E%3CSCRIPT%3Ealert('XSS');%3C/SCRIPT%3E%3C!%5Bendif%5D--%3E"">XSS</a>";


        Assert.AreEqual(expectedNet35, actual);
      }
    }

    /// <summary>
    /// A test for Div with Downlevel-Hidden block xss
    /// </summary>
    [Test]
    public void DivDownlevelHiddenBlockXSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<Div style=\"background-color: http://www.codeplex.com?url=<!--[if gte IE 4]><SCRIPT>alert('XSS');</SCRIPT><![endif]-->\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = @"<div>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for AnchorTag with Html Quotes Encapsulation 1 xss
    /// Example <!-- <A HREF="http://www.codeplex.com?url=<SCRIPT a=">" SRC="http://ha.ckers.org/xss.js"></SCRIPT>">XSS</A> -->
    /// </summary>
    [Test]
    public void AnchorTagHtmlQuotesEncapsulation1XSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<A HREF=\"http://www.codeplex.com?url=<SCRIPT a=\">\" SRC=\"http://ha.ckers.org/xss.js\"></SCRIPT>\">XSS</A>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = @"<a href=""http://www.codeplex.com/?url=%3CSCRIPT%20a="">"" SRC=""http://ha.ckers.org/xss.js""&gt;""&gt;XSS</a>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for Div with Html Quotes Encapsulation 1 xss
    /// Example <!-- <Div style="background-color: http://www.codeplex.com?url=<SCRIPT a=">" SRC="http://ha.ckers.org/xss.js"></SCRIPT>"> -->
    /// </summary>
    [Test]
    public void DivHtmlQuotesEncapsulation1XSSTest()
    {
      // Arrange
      var sanitizer = Sanitizer;


      // Act
      string htmlFragment = "<Div style=\"background-color: http://www.codeplex.com?url=<SCRIPT a=\">\" SRC=\"http://ha.ckers.org/xss.js\"></SCRIPT>\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div>\" SRC=\"http://ha.ckers.org/xss.js\"&gt;\"&gt;";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// A test for various legal fragments
    /// </summary>
    [Test]
    public void LegalTest()
    {
      // Arrange
      var sanitizer = Sanitizer;

      // Act
      string htmlFragment = "<div style=\"background-color: test\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div style=\"background-color: test\">";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// More tests for legal fragments.
    /// </summary>
    [Test]
    public void MoreLegalTest()
    {
      // Arrange
      var sanitizer = Sanitizer;

      // Act
      string htmlFragment = "<div style=\"background-color: test;\">Test<img src=\"http://www.example.com/test.gif\" style=\"background-image: url(http://www.example.com/bg.jpg); margin: 10px\"></div>";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<div style=\"background-color: test;\">Test<img src=\"http://www.example.com/test.gif\" style=\"background-image: url(&quot;http://www.example.com/bg.jpg&quot;);margin: 10px\"></div>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Misc tests.
    /// </summary>
    [Test]
    public void MiscTest()
    {
      var sanitizer = Sanitizer;

      var html = @"<SCRIPT/SRC=""http://ha.ckers.org/xss.js""></SCRIPT>";
      var actual = sanitizer.Sanitize(html);
      var expected = "";
      Assert.AreEqual(expected, actual);

      html = @"<DIV STYLE=""padding: &#49;px; mar/*xss*/gin: ex/*XSS*/pression(alert('xss')); background-image:\0075\0072\006C\0028\0022\006a\0061\0076\0061\0073\0063\0072\0069\0070\0074\003a\0061\006c\0065\0072\0074\0028\0027\0058\0053\0053\0027\0029\0022\0029"">";
      actual = sanitizer.Sanitize(html);
      expected = @"<div style=""padding: 1px;"">";
      Assert.AreEqual(expected, actual);

      html = @"<!--[if gte IE 4]><SCRIPT>alert('XSS');</SCRIPT><![endif]--><!-- Comment -->";
      actual = sanitizer.Sanitize(html);
      expected = "";
      Assert.AreEqual(expected, actual);

      html = @"<STYLE>@im\port'\ja\vasc\ript:alert(""XSS"")';</STYLE>";
      actual = sanitizer.Sanitize(html);
      expected = "";
      Assert.AreEqual(expected, actual);

      html = @"<div onload!#$%&()*~+-_.,:;?@[/|\]^`=alert(""XSS"")>";
      actual = sanitizer.Sanitize(html);
      expected = "<div>";
      Assert.AreEqual(expected, actual);

      html = @"<SCRIPT/XSS SRC=""http://ha.ckers.org/xss.js""></SCRIPT>";
      actual = sanitizer.Sanitize(html);
      expected = "";
      Assert.AreEqual(expected, actual);

      html = "<IMG SRC=javascript:alert(\"XSS\")>\"";
      actual = sanitizer.Sanitize(html);
      expected = "\"";
      Assert.AreEqual(expected, actual);

      html = "<IMG SRC=java\0script:alert(\"XSS\")>\"";
      actual = sanitizer.Sanitize(html);
      expected = "\"";
      Assert.AreEqual(expected, actual);

      html = @"<IMG SRC=""jav&#x0D;ascript:alert('XSS');"">";
      actual = sanitizer.Sanitize(html);
      expected = "";
      Assert.AreEqual(expected, actual);

      html = @"<IMG SRC=""jav&#x0A;ascript:alert('XSS');"">";
      actual = sanitizer.Sanitize(html);
      expected = "";
      Assert.AreEqual(expected, actual);

      html = @"<IMG SRC=""jav&#x09;ascript:alert('XSS');"">";
      actual = sanitizer.Sanitize(html);
      expected = "";
      Assert.AreEqual(expected, actual);

      html = @"<div style=""background-color: red""><sCRipt>hallo</scripT></div><a href=""#"">Test</a>";
      actual = sanitizer.Sanitize(html);
      expected = @"<div style=""background-color: red""></div><a href=""#"">Test</a>";
      Assert.AreEqual(expected, actual);

      html = @"<IMG SRC=""jav	ascript:alert('XSS');"">";
      actual = sanitizer.Sanitize(html);
      expected = "";
      Assert.AreEqual(expected, actual);

      html = @"<IMG SRC="" &#14;  javascript:alert('XSS');"">";
      actual = sanitizer.Sanitize(html);
      expected = "";
      Assert.AreEqual(expected, actual);

      html = @"<IMG SRC=&#106;&#97;&#118;&#97;&#115;&#99;&#114;&#105;&#112;&#116;&#58;&#97;&#108;&#101;&#114;&#116;&#40;&#39;&#88;&#83;&#83;&#39;&#41;>";
      actual = sanitizer.Sanitize(html);
      expected = "";
      Assert.AreEqual(expected, actual);

      html = @"<IMG SRC=&#0000106&#0000097&#0000118&#0000097&#0000115&#0000099&#0000114&#0000105&#0000112&#0000116&#0000058&#0000097&#0000108&#0000101&#0000114&#0000116&#0000040&#0000039&#0000088&#0000083&#0000083&#0000039&#0000041>";
      actual = sanitizer.Sanitize(html);
      expected = "";
      Assert.AreEqual(expected, actual);

      html = @"<IMG SRC=&#x6A&#x61&#x76&#x61&#x73&#x63&#x72&#x69&#x70&#x74&#x3A&#x61&#x6C&#x65&#x72&#x74&#x28&#x27&#x58&#x53&#x53&#x27&#x29>";
      actual = sanitizer.Sanitize(html);
      expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Tests disallowed tags.
    /// </summary>
    [Test]
    public void DisallowedTagTest()
    {
      var sanitizer = Sanitizer;

      var html = @"<bla>Hallo</bla>";
      var actual = sanitizer.Sanitize(html);
      var expected = "";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Tests disallowed HTML attributes.
    /// </summary>
    [Test]
    public void DisallowedAttributeTest()
    {
      var sanitizer = Sanitizer;

      var html = @"<div bla=""test"">Test</div>";
      var actual = sanitizer.Sanitize(html);
      var expected = @"<div>Test</div>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Tests sanitization of attributes that contain a URL.
    /// </summary>
    [Test]
    public void UrlAttributeTest()
    {
      var sanitizer = Sanitizer;

      var html = @"<a href=""mailto:test@example.com"">test</a>";
      var actual = sanitizer.Sanitize(html);
      var expected = @"<a>test</a>";
      Assert.AreEqual(expected, actual);

      html = @"<a href=""http:xxx"">test</a>";
      actual = sanitizer.Sanitize(html);
      expected = @"<a>test</a>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Tests disallowed css properties.
    /// </summary>
    [Test]
    public void DisallowedStyleTest()
    {
      var sanitizer = Sanitizer;

      var html = @"<div style=""margin: 8px; bla: 1px"">test</div>";
      var actual = sanitizer.Sanitize(html);
      var expected = @"<div style=""margin: 8px;"">test</div>";
      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Tests sanitization of URLs that are contained in CSS property values.
    /// </summary>
    [Test]
    public void UrlStyleTest()
    {
      var sanitizer = Sanitizer;

      var html = @"<div style=""padding: 10px; background-image: url(mailto:test@example.com)""></div>";
      var actual = sanitizer.Sanitize(html);
      var expected = @"<div style=""padding: 10px;""></div>";
      Assert.AreEqual(expected, actual);
    }

    // test below from http://genshi.edgewall.org/

    [Test]
    public void SanitizeUnchangedTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<a href=""#"">fo<br />o</a>";
      Assert.AreEqual(@"<a href=""#"">fo<br>o</a>", sanitizer.Sanitize(html));

      html = @"<a href=""#with:colon"">foo</a>";
      Assert.AreEqual(@"<a>foo</a>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeEscapeTextTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<a href=""#"">fo&amp;</a>";
      Assert.AreEqual(@"<a href=""#"">fo&amp;</a>", sanitizer.Sanitize(html));

      html = @"<a href=""#"">&lt;foo&gt;</a>";
      Assert.AreEqual(@"<a href=""#"">&lt;foo&gt;</a>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeEntityrefTextTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<a href=""#"">fo&ouml;</a>";
      Assert.AreEqual(@"<a href=""#"">foö</a>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeEscapeAttrTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<div title=""&lt;foo&gt;""></div>";
      Assert.AreEqual(@"<div title=""&lt;foo&gt;""></div>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeCloseEmptyTagTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<a href=""#"">fo<br>o</a>";
      Assert.AreEqual(@"<a href=""#"">fo<br>o</a>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeInvalidEntityTest()
    {
      var sanitizer = Sanitizer;
      var html = @"&junk;";
      Assert.AreEqual(@"&amp;junk;", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeRemoveScriptElemTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<script>alert(""Foo"")</script>";
      Assert.AreEqual(@"", sanitizer.Sanitize(html));
      html = @"<SCRIPT SRC=""http://example.com/""></SCRIPT>";
      Assert.AreEqual(@"", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeRemoveOnclickAttrTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<div onclick=\'alert(""foo"")\' />";
      Assert.AreEqual(@"<div></div>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeRemoveCommentsTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<div><!-- conditional comment --></div>";
      Assert.AreEqual(@"<div></div>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeRemoveStyleScriptsTest()
    {
      var sanitizer = Sanitizer;
      // Inline style with url() using javascript: scheme
      var html = @"<DIV STYLE='background: url(javascript:alert(""foo""))'>";
      Assert.AreEqual(@"<div>", sanitizer.Sanitize(html));
      // Inline style with url() using javascript: scheme, using control char
      html = @"<DIV STYLE='background: url(&#1;javascript:alert(""foo""))'>";
      Assert.AreEqual(@"<div>", sanitizer.Sanitize(html));
      // Inline style with url() using javascript: scheme, in quotes
      html = @"<DIV STYLE='background: url(""javascript:alert(foo)"")'>";
      Assert.AreEqual(@"<div>", sanitizer.Sanitize(html));
      // IE expressions in CSS not allowed
      html = @"<DIV STYLE='width: expression(alert(""foo""));'>";
      Assert.AreEqual(@"<div>", sanitizer.Sanitize(html));
      html = @"<DIV STYLE='width: e/**/xpression(alert(""foo""));'>";
      Assert.AreEqual(@"<div>", sanitizer.Sanitize(html));
      html = @"<DIV STYLE='background: url(javascript:alert(""foo""));color: #fff'>";
      Assert.AreEqual(@"<div style=""color: #fff"">", sanitizer.Sanitize(html));

      // Inline style with url() using javascript: scheme, using unicode
      // escapes
      html = @"<DIV STYLE='background: \75rl(javascript:alert(""foo""))'>";
      Assert.AreEqual(@"<div>", sanitizer.Sanitize(html));
      html = @"<DIV STYLE='background: \000075rl(javascript:alert(""foo""))'>";
      Assert.AreEqual(@"<div>", sanitizer.Sanitize(html));
      html = @"<DIV STYLE='background: \75 rl(javascript:alert(""foo""))'>";
      Assert.AreEqual(@"<div>", sanitizer.Sanitize(html));
      html = @"<DIV STYLE='background: \000075 rl(javascript:alert(""foo""))'>";
      Assert.AreEqual(@"<div>", sanitizer.Sanitize(html));
      html = @"<DIV STYLE='background: \000075
rl(javascript:alert(""foo""))'>";
      Assert.AreEqual(@"<div>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeRemoveStylePhishingTest()
    {
      var sanitizer = Sanitizer;
      // The position property is not allowed
      var html = @"<div style=""position:absolute;top:0""></div>";
      Assert.AreEqual(@"<div style=""top:0""></div>", sanitizer.Sanitize(html));
      // Normal margins get passed through
      html = @"<div style=""margin:10px 20px""></div>";
      Assert.AreEqual(@"<div style=""margin:10px 20px""></div>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeRemoveSrcJavascriptTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<img src=\'javascript:alert(""foo"")\'>";
      Assert.AreEqual(@"", sanitizer.Sanitize(html));
      // Case-insensitive protocol matching
      html = @"<IMG SRC=\'JaVaScRiPt:alert(""foo"")\'>";
      Assert.AreEqual(@"", sanitizer.Sanitize(html));
      // Grave accents (not parsed)
      // Protocol encoded using UTF-8 numeric entities
      html = @"<IMG SRC=\'&#106;&#97;&#118;&#97;&#115;&#99;&#114;&#105;&#112;&#116;&#58;alert(""foo"")\'>";
      Assert.AreEqual(@"", sanitizer.Sanitize(html));
      // Protocol encoded using UTF-8 numeric entities without a semicolon
      // (which is allowed because the max number of digits is used)
      html = @"<IMG SRC=\'&#0000106&#0000097&#0000118&#0000097&#0000115&#0000099&#0000114&#0000105&#0000112&#0000116&#0000058alert(""foo"")\'>";
      Assert.AreEqual(@"", sanitizer.Sanitize(html));
      // Protocol encoded using UTF-8 numeric hex entities without a semicolon
      // (which is allowed because the max number of digits is used)
      html = @"<IMG SRC=\'&#x6A&#x61&#x76&#x61&#x73&#x63&#x72&#x69&#x70&#x74&#x3A;alert(""foo"")\'>";
      Assert.AreEqual(@"", sanitizer.Sanitize(html));
      // Embedded tab character in protocol
      html = @"<IMG SRC=\'jav\tascript:alert(""foo"");\'>";
      Assert.AreEqual(@"", sanitizer.Sanitize(html));
      // Embedded tab character in protocol, but encoded this time
      html = @"<IMG SRC=\'jav&#x09;ascript:alert(""foo"");\'>";
      Assert.AreEqual(@"", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeExpressionTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<div style=""top:expression(alert())"">XSS</div>";
      Assert.AreEqual(@"<div>XSS</div>", sanitizer.Sanitize(html));
    }

    [Test]
    public void capitalExpressionTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<div style=""top:EXPRESSION(alert())"">XSS</div>";
      Assert.AreEqual(@"<div>XSS</div>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeUrlWithJavascriptTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<div style=""background-image:url(javascript:alert())"">XSS</div>";
      Assert.AreEqual(@"<div>XSS</div>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeCapitalUrlWithJavascriptTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<div style=""background-image:URL(javascript:alert())"">XSS</div>";
      Assert.AreEqual(@"<div>XSS</div>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeUnicodeEscapesTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<div style=""top:exp\72 ess\000069 on(alert())"">XSS</div>";
      Assert.AreEqual(@"<div>XSS</div>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeBackslashWithoutHexTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<div style=""top:e\xp\ression(alert())"">XSS</div>";
      Assert.AreEqual(@"<div>XSS</div>", sanitizer.Sanitize(html));
      html = @"<div style=""top:e\\xp\\ression(alert())"">XSS</div>";
      Assert.AreEqual(@"<div>XSS</div>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeUnsafePropsTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<div style=""POSITION:RELATIVE"">XSS</div>";
      Assert.AreEqual(@"<div>XSS</div>", sanitizer.Sanitize(html));

      html = @"<div style=""behavior:url(test.htc)"">XSS</div>";
      Assert.AreEqual(@"<div>XSS</div>", sanitizer.Sanitize(html));

      html = @"<div style=""-ms-behavior:url(test.htc) url(#obj)"">XSS</div>";
      Assert.AreEqual(@"<div>XSS</div>", sanitizer.Sanitize(html));

      html = @"<div style=""-o-link:'javascript:alert(1)';-o-link-source:current"">XSS</div>";
      Assert.AreEqual(@"<div>XSS</div>", sanitizer.Sanitize(html));

      html = @"<div style=""-moz-binding:url(xss.xbl)"">XSS</div>";
      Assert.AreEqual(@"<div>XSS</div>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeCssHackTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<div style=""*position:static"">XSS</div>";
      Assert.AreEqual(@"<div>XSS</div>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizePropertyNameTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<div style=""display:none;border-left-color:red;userDefined:1;-moz-user-selct:-moz-all"">prop</div>";
      Assert.AreEqual(@"<div style=""display:none;border-left-color:red;"">prop</div>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeUnicodeExpressionTest()
    {
      var sanitizer = Sanitizer;
      // Fullwidth small letters
      var html = @"<div style=""top:ｅｘｐｒｅｓｓｉｏｎ(alert())"">XSS</div>";
      Assert.AreEqual(@"<div>XSS</div>", sanitizer.Sanitize(html));
      // Fullwidth capital letters
      html = @"<div style=""top:ＥＸＰＲＥＳＳＩＯＮ(alert())"">XSS</div>";
      Assert.AreEqual(@"<div>XSS</div>", sanitizer.Sanitize(html));
      // IPA extensions
      html = @"<div style=""top:expʀessɪoɴ(alert())"">XSS</div>";
      Assert.AreEqual(@"<div>XSS</div>", sanitizer.Sanitize(html));
    }

    [Test]
    public void SanitizeUnicodeUrlTest()
    {
      var sanitizer = Sanitizer;
      // IPA extensions
      var html = @"<div style=""background-image:uʀʟ(javascript:alert())"">XSS</div>";
      Assert.AreEqual(@"<div>XSS</div>", sanitizer.Sanitize(html));
    }

    [Test]
    public void ProtocolRelativeTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<a href=""//www.example.com/test"">Test</a>";
      Assert.AreEqual(@"<a href=""//www.example.com/test"">Test</a>", sanitizer.Sanitize(html));
    }

    [Test]
    public void JavaScriptIncludeAndAngleBracketsTest()
    {
      // Arrange
      var sanitizer = Sanitizer;

      // Act
      string htmlFragment = "<BR SIZE=\"&{alert('XSS&gt;')}\">";
      string actual = sanitizer.Sanitize(htmlFragment);

      // Assert
      string expected = "<br>";
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void SanitizeNonClosedTagTest()
    {
      var sanitizer = Sanitizer;
      var html = @"<div>Hallo <p><b>Bold<br>Ballo";
      Assert.AreEqual(@"<div>Hallo <p><b>Bold<br>Ballo", sanitizer.Sanitize(html));
    }

    [Test]
    public void RussianTextTest()
    {
      // Arrange
      var s = Sanitizer;

      // Act
      var htmlFragment = "Тест";
      var actual = s.Sanitize(htmlFragment);

      // Assert
      var expected = htmlFragment;
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CssKeyTest()
    {
      // Arrange
      var s = Sanitizer;

      // Act
      var htmlFragment = @"<div style=""\000062ackground-image: URL(http://www.example.com/bg.jpg)"">Test</div>";
      var actual = s.Sanitize(htmlFragment);

      // Assert
      var expected = @"<div style=""background-image: url(&quot;http://www.example.com/bg.jpg&quot;)"">Test</div>";
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void QuotedBackgroundImageTest()
    {
      // https://github.com/mganss/HtmlSanitizer/issues/44

      // Arrange
      var s = Sanitizer;

      // Act
      var htmlFragment = "<div style=\"background-image: url('some/random/url.img')\"></div>";
      var actual = s.Sanitize(htmlFragment);

      // Assert
      var expected = "<div style=\"background-image: url(&quot;some/random/url.img&quot;)\"></div>";
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void QuotedBackgroundImageFromIE9()
    {
      // Arrange
      var s = Sanitizer;

      // Act
      var htmlFragment = "<span style='background-image: url(\"/api/users/defaultAvatar\");'></span>";
      var actual = s.Sanitize(htmlFragment);

      // Assert
      var expected = "<span style=\"background-image: url(&quot;/api/users/defaultAvatar&quot;);\"></span>";
      Assert.AreEqual(expected, actual);
    }

    [Test]
    public void UriHashTest()
    {
      var s = Sanitizer;
      var html = @"<a href=""http://domain.com/index.html?test=#value#"">test</a>";

      var actual = s.Sanitize(html);

      Assert.AreEqual(html, actual);
    }

    [Test]
    public void FragmentTest()
    {
      var s = Sanitizer;
      var html = @"<script>alert('test');</script><p>Test</p>";

      var actual = s.Sanitize(html);

      Assert.AreEqual("<p>Test</p>", actual);
    }

    [Test]
    public void OpenTagFragmentTest()
    {
      // https://github.com/mganss/HtmlSanitizer/issues/75

      var s = Sanitizer;
      var html = "<p>abc<script>xyz</p>";

      var actual = s.Sanitize(html);

      Assert.AreEqual("<p>abc", actual);
    }

    public static IEnumerable<T> Shuffle<T>(IEnumerable<T> source, Random rng)
    {
      T[] elements = source.ToArray();
      for (int i = elements.Length - 1; i >= 0; i--)
      {
        // Swap element "i" with a random earlier element it (or itself)
        // ... except we don't really need to swap it fully, as we can
        // return it immediately, and afterwards it's irrelevant.
        int swapIndex = rng.Next(i + 1);
        yield return elements[swapIndex];
        elements[swapIndex] = elements[i];
      }
    }
  }
}
