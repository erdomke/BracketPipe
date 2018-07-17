using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BracketPipe.Tests
{

  public class TextileTests
  {
    public string ParseHTML(string html)
    {
      var sb = new StringBuilder();
      sb.EnsureCapacity(html.Length);
      var settings = new MarkdownWriterSettings() { NewLineChars = "\n" };
      using (var sw = new StringWriter(sb))
      using (var reader = new HtmlReader(html, false))
      using (var writer = new TextileWriter(sw, settings))
      {
        reader.ToHtml(writer);
        sw.Flush();
        return sb.ToString();
      }
    }

    //http://redcloth.org/textile/
    #region Documentation

    [Fact]
    public void Textile_Doc_Strong()
    {
      string s = "<p>Don&#8217;t <strong>ever</strong> pull this lever.</p>";
      string t = ParseHTML(s);

      Assert.Equal("Don't *ever* pull this lever.", t);
    }

    [Fact]
    public void Textile_Doc_Stress()
    {
      string s = "<p>You didn&#8217;t actually <em>believe</em> her, did you?</p>";
      string t = ParseHTML(s);

      Assert.Equal("You didn't actually _believe_ her, did you?", t);
    }

    [Fact]
    public void Textile_Doc_StylisticOffset()
    {
      string s = @"<p>Search results for <b>Textile</b>:</p>
<h4><a href=""http://en.wikipedia.org/wiki/Textile_(markup_language)""><b>Textile</b> (markup language) &#8211; Wikipedia</a></h4>
<p><b>Textile</b> is a lightweight markup language originally developed by Dean Allen and billed as a &#8220;humane Web text generator&#8221;.  <b>Textile</b> converts its marked-up text &#8230;</p>";
      string t = ParseHTML(s);

      Assert.Equal("Search results for **Textile**:\n\nh4. [\"**Textile** (markup language) - Wikipedia\":http://en.wikipedia.org/wiki/Textile_(markup_language)]\n\n**Textile** is a lightweight markup language originally developed by Dean Allen and billed as a \"humane Web text generator\". **Textile** converts its marked-up text ...", t);
    }

    [Fact]
    public void Textile_Doc_AlternateVoice()
    {
      string s = @"<p>I just got the weirdest feeling of <i>déjà vu</i>.</p>";
      string t = ParseHTML(s);

      Assert.Equal(@"I just got the weirdest feeling of __déjà vu__.", t);
    }

    [Fact]
    public void Textile_Doc_Citation()
    {
      string s = @"<p>My wife&#8217;s favorite book is <cite>The Count of Monte Cristo</cite> by Dumas.</p>";
      string t = ParseHTML(s);

      Assert.Equal(@"My wife's favorite book is ??The Count of Monte Cristo?? by Dumas.", t);
    }

    [Fact]
    public void Textile_Doc_InsertionDeletion()
    {
      string s = @"<p>The news networks declared <del>Al Gore</del> <ins>George W. Bush</ins> the winner in Florida.</p>";
      string t = ParseHTML(s);

      Assert.Equal(@"The news networks declared [-Al Gore-] [+George W. Bush+] the winner in Florida.", t);
    }

    [Fact]
    public void Textile_Doc_SuperscriptSubscript()
    {
      string s = @"<p>f(x, n) = log<sub>4</sub>x<sup>n</sup></p>";
      string t = ParseHTML(s);

      Assert.Equal(@"f(x, n) = log[~4~]x[^n^]", t);
    }

    [Fact]
    public void Textile_Doc_Link()
    {
      string s = @"<p>This is a link to a <a href=""http://en.wikipedia.org/wiki/Textile_(markup_language)"">Wikipedia article about Textile</a>.</p>";
      string t = ParseHTML(s);

      Assert.Equal(@"This is a link to a [""Wikipedia article about Textile"":http://en.wikipedia.org/wiki/Textile_(markup_language)].", t);
    }

    [Fact]
    public void Textile_Doc_Image()
    {
      string s = @"<p><img src=""http://www.w3.org/Icons/valid-html401"" title=""This page is valid HTML"" alt=""This page is valid HTML"" /></p>";
      string t = ParseHTML(s);

      Assert.Equal(@"!http://www.w3.org/Icons/valid-html401(This page is valid HTML)!", t);
    }

    [Fact]
    public void Textile_Doc_ClassOrId()
    {
      string s = @"<p class=""my-class"">This is a paragraph that has a class and this <strong id=""special-phrase"">emphasized phrase</strong> has an id.</p>";
      string t = ParseHTML(s);

      Assert.Equal(@"p(my-class). This is a paragraph that has a class and this *(#special-phrase)emphasized phrase* has an id.", t);
    }

    [Fact]
    public void Textile_Doc_ClassAndId()
    {
      string s = @"<div class=""myclass"" id=""myid"">This div has both a CSS class and ID.</div>";
      string t = ParseHTML(s);

      Assert.Equal(@"div(myclass#myid). This div has both a CSS class and ID.", t);
    }

    [Fact]
    public void Textile_Doc_Style()
    {
      string s = @"<p style=""color:blue;letter-spacing:.5em;"">Spacey blue</p>";
      string t = ParseHTML(s);

      Assert.Equal(@"p{color:blue;letter-spacing:.5em}. Spacey blue", t);
    }

    [Fact]
    public void Textile_Doc_Language()
    {
      string s = @"<p lang=""fr"">Parlez-vous français ?</p>";
      string t = ParseHTML(s);

      Assert.Equal(@"p[fr]. Parlez-vous français ?", t);
    }
    #endregion

    #region paragraphs

    [Fact]
    public void P_NoStyle()
    {
      string s = @"<p>my text</p><p>Yeah it is</p><p>woot</p>";
      string t = ParseHTML(s);

      Assert.Equal("my text\n\nYeah it is\n\nwoot", t);
    }

    [Fact]
    public void P_Style()
    {
      string s = "<p >my text</p><p style=\"text-align: center;\">Yeah it is</p><p>woot</p>";
      string t = ParseHTML(s);

      Assert.Equal("my text\n\np{text-align: center}. Yeah it is\n\nwoot", t);
    }

    [Fact]
    public void P_MultiStyle()
    {
      string s = "<p >my text</p><p style=\"text-align: center; color: red;\">Yeah it is</p><p>woot</p>";
      string t = ParseHTML(s);

      Assert.Equal("my text\n\np{text-align: center; color: red}. Yeah it is\n\nwoot", t);
    }

    [Fact]
    public void P_noPara()
    {
      string s = "<p >my text</p>Random text<p>woot</p>";
      string t = ParseHTML(s);

      Assert.Equal("my text\n\nRandom text\n\nwoot", t);
    }

    [Fact]
    public void P_class()
    {
      string s = "<p class=\"wowza\">my text</p>Random text<p>woot</p>";
      string t = ParseHTML(s);

      Assert.Equal("p(wowza). my text\n\nRandom text\n\nwoot", t);
    }

    #endregion

    #region lists

    // TODO: Fix these

    //[Fact]
    //public void UL_nestedOL()
    //{
    //  string s = "<ul><li>li1</li><li>li2</li><li><ol><li>num1</li><li>num2</li></ol></li><li>li3</li></ul>";
    //  string t = ParseHTML(s);

    //  Assert.Equal("* li1\n* li2\n## num1\n## num2\n* li3\n\n", t);
    //}

    //[Fact]
    //public void UL_nestedOL_n_Text()
    //{
    //  string s = "<ul><li>li1</li><li>li2</li><li>extra<ol><li>num1</li><li>num2</li></ol></li><li>li3</li></ul>";
    //  string t = ParseHTML(s);

    //  Assert.Equal("* li1\n* li2\n* extra\n## num1\n## num2\n* li3\n\n", t);
    //}

    #endregion

    #region pre/code

    [Fact]
    public void PRE_simple()
    {
      string s = "some text <pre>hey this is my pre</pre> more text";
      string t = ParseHTML(s);

      Assert.Equal("some text\n\npre.. hey this is my pre\n\np. more text", t);
    }

    [Fact]
    public void PRE_newlines()
    {
      string s = "some text <pre>hey\n\nthis\nis\nmy\n\n\npre</pre> more text";
      string t = ParseHTML(s);

      Assert.Equal("some text\n\npre.. hey\n\nthis\nis\nmy\n\n\npre\n\np. more text", t);
    }

    [Fact]
    public void PRE_class()
    {
      string s = "some text <pre class=\"prettyprint\">hey\n\nthis\nis\nmy\n\n\npre</pre> more text";
      string t = ParseHTML(s);

      Assert.Equal("some text\n\npre(prettyprint).. hey\n\nthis\nis\nmy\n\n\npre\n\np. more text", t);
    }

    [Fact]
    public void PRECODE_simple()
    {
      string s = "some text <pre><code>hey this is my code\nyeah man</code></pre> more text";
      string t = ParseHTML(s);

      Assert.Equal("some text\n\nbc.. hey this is my code\nyeah man\n\np. more text", t);
    }

    [Fact]
    public void PRECODE_header()
    {
      string s = "some text <pre><code>hey this is my code\nyeah man</code></pre> <h2>header</h2> more text";
      string t = ParseHTML(s);

      Assert.Equal("some text\n\nbc.. hey this is my code\nyeah man\n\nh2. header\n\nmore text", t);
    }

    [Fact]
    public void CODE_simple()
    {
      string s = "<p>some text <code>code</code> more text</p>";
      string t = ParseHTML(s);

      Assert.Equal("some text @code@ more text", t);
    }

    #endregion

    #region glyphs

    [Fact]
    public void GLYPH_replace()
    {
      string s = "some text &nbsp; &amp; &copy; ";
      string t = ParseHTML(s);

      Assert.Equal("some text \u00A0 & (c)", t);
    }

    [Fact]
    public void GLYPH_charCode()
    {
      string s = "some text &#169; &#179; &#8721;";
      string t = ParseHTML(s);

      Assert.Equal("some text (c) ³ ∑", t);
    }

    #endregion

    #region links

    [Fact]
    public void LINK_basic()
    {
      string s = "text! and <a href=\"/Project/something/Wiki/Wowza\">a link</a> ok!!";
      string t = ParseHTML(s);

      Assert.Equal("text! and [\"a link\":/Project/something/Wiki/Wowza] ok!!", t);
    }

    [Fact]
    public void LINK_title()
    {
      string s = "text! and <a title=\"title\" href=\"http://something.com/Project/something/Wiki/Wowza\">a link</a> ok!!";
      string t = ParseHTML(s);

      Assert.Equal("text! and [\"a link(title)\":http://something.com/Project/something/Wiki/Wowza] ok!!", t);
    }

    [Fact]
    public void LINK_class()
    {
      string s = "text! and <a class=\"something\" href=\"http://something.com/Project/something/Wiki/Wowza\">a link</a> ok!!";
      string t = ParseHTML(s);

      Assert.Equal("text! and [\"(something)a link\":http://something.com/Project/something/Wiki/Wowza] ok!!", t);
    }

    [Fact]
    public void LINK_style()
    {
      string s = "text! and <a style=\"background:#0f0; color:#00c;\" title=\"my title\" href=\"http://something.com/Project/something/Wiki/Wowza\">a link</a> ok!!";
      string t = ParseHTML(s);

      Assert.Equal("text! and [\"{background:#0f0; color:#00c}a link(my title)\":http://something.com/Project/something/Wiki/Wowza] ok!!", t);
    }


    #endregion

    #region images

    [Fact]
    public void IMAGE_basic()
    {
      string s = "text! and <img src=\"/meow/omg.jpg\" /> ok!!";
      string t = ParseHTML(s);

      Assert.Equal("text! and !/meow/omg.jpg! ok!!", t);
    }

    [Fact]
    public void IMAGE_class()
    {
      string s = "text! and <img class=\"mycl\" style=\"color:#fff;\" src=\"/meow/omg.jpg\" /> ok!!";
      string t = ParseHTML(s);

      Assert.Equal("text! and !(mycl)/meow/omg.jpg! ok!!", t);
    }

    [Fact]
    public void IMAGE_style()
    {
      string s = "text! and <img style=\"color:#fff;\" src=\"/meow/omg.jpg\" /> ok!!";
      string t = ParseHTML(s);

      Assert.Equal("text! and !{color:#fff}/meow/omg.jpg! ok!!", t);
    }

    #endregion
  }
}
