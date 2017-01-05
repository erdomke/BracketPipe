using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BracketPipe.Tests
{
  //https://github.com/baynezy/Html2Markdown/blob/master/test/Html2Markdown.Test/ConverterTest.cs
  [TestFixture]
  class MarkdownTests
  {

    [Test]
    public void Convert_WhenThereAreHtmlLinks_ThenConvertToMarkDownLinks()
    {
      const string html = @"So this is <a href=""http://www.simonbaynes.com/"">a link</a>. Convert it";
      const string expected = @"So this is [a link](http://www.simonbaynes.com/). Convert it";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreHtmlLinksWithAttributesAfterTheHref_ThenConvertToMarkDownLink()
    {
      const string html = @"So this is <a href=""http://www.simonbaynes.com/"" alt=""example"">a link</a>. Convert it";
      const string expected = @"So this is [a link](http://www.simonbaynes.com/). Convert it";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreHtmlLinksWithAttributesBeforeTheHref_ThenConvertToMarkDownLink()
    {
      const string html = @"So this is <a alt=""example"" href=""http://www.simonbaynes.com/"">a link</a>. Convert it";
      const string expected = @"So this is [a link](http://www.simonbaynes.com/). Convert it";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreHtmlLinksWithTitleAttributeAfterTheHref_ThenConvertToMarkDownLink()
    {
      const string html = @"So this is <a href=""http://www.simonbaynes.com/"" title=""example"">a link</a>. Convert it";
      const string expected = @"So this is [a link](http://www.simonbaynes.com/ ""example""). Convert it";

      CheckConversion(html, expected);
    }


    [Test]
    public void Convert_WhenThereAreHtmlLinksWithTitleAttributeBeforeTheHref_ThenConvertToMarkDownLink()
    {
      const string html = @"So this is <a title=""example"" href=""http://www.simonbaynes.com/"">a link</a>. Convert it";
      const string expected = @"So this is [a link](http://www.simonbaynes.com/ ""example""). Convert it";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreMultipleHtmlLinks_ThenConvertThemToMarkDownLinks()
    {
      const string html = @"So this is <a href=""http://www.simonbaynes.com/"">a link</a> and so is <a href=""http://www.google.com/"">this</a>. Convert them";
      const string expected = @"So this is [a link](http://www.simonbaynes.com/) and so is [this](http://www.google.com/). Convert them";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreEmptyLinks_ThenRemoveThemFromResult()
    {
      const string html = @"So this is <a name=""curio""></a> and so is <a href=""http://www.google.com/"">this</a>. Convert them";
      const string expected = @"So this is <a name=""curio""></a> and so is [this](http://www.google.com/). Convert them";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreStrongTags_ThenConvertToMarkDownDoubleAsterisks()
    {
      const string html = @"So this text is <strong>bold</strong>. Convert it.";
      const string expected = @"So this text is **bold**. Convert it.";

      CheckConversion(html, expected);
    }


    [Test]
    public void Convert_WhenThereAreMultipleStrongTags_ThenConvertToMarkDownDoubleAsterisks()
    {
      const string html = @"So this text is <strong>bold</strong> and <strong>is this</strong>. Convert it.";
      const string expected = @"So this text is **bold** and **is this**. Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreBoldTags_ThenConvertToMarkDownDoubleAsterisks()
    {
      const string html = @"So this text is <b>bold</b>. Convert it.";
      const string expected = @"So this text is **bold**. Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreEmphsisTags_ThenConvertToMarkDownSingleAsterisk()
    {
      const string html = @"So this text is <em>italic</em>. Convert it.";
      const string expected = @"So this text is *italic*. Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreItalicTags_ThenConvertToMarkDownSingleAsterisk()
    {
      const string html = @"So this text is <i>italic</i>. Convert it.";
      const string expected = @"So this text is *italic*. Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreBreakTags_ThenConvertToMarkDownDoubleSpacesWitCarriageReturns()
    {
      const string html = @"So this text has a break.<br/>Convert it.";
      const string expected = @"So this text has a break.\
Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreBreakTagsWithWhitespace_ThenConvertToMarkDownDoubleSpacesWitCarriageReturns()
    {
      const string html = @"So this text has a break.<br />Convert it.";
      const string expected = @"So this text has a break.\
Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreBreakTagsThatAreNotSelfClosing_ThenConvertToMarkDownDoubleSpacesWitCarriageReturns()
    {
      const string html = @"So this text has a break.<br>Convert it.";
      const string expected = @"So this text has a break.\
Convert it.";

      CheckConversion(html, expected);
    }


    [Test]
    public void Convert_WhenThereAreCodeTags_ThenReplaceWithBackTick()
    {
      const string html = @"So this text has code <code>alert();</code>. Convert it.";
      const string expected = @"So this text has code `alert();`. Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereMultilineCodeTags_ThenReplaceWithMultilineMarkdownBlock001()
    {
      const string html = @"So this text has multiline code.
<code>
&lt;p&gt;
  Some code we are looking at
&lt;/p&gt;
</code>";
      const string expected = @"So this text has multiline code. `<p> Some code we are looking at </p>`";

      CheckConversion(html, expected);
    }


    [Test]
    public void Convert_WhenThereMultilineCodeTags_ThenReplaceWithMultilineMarkdownBlock002()
    {
      const string html = @"So this text has multiline code.
<code>
  &lt;p&gt;
    Some code we are looking at
  &lt;/p&gt;
</code>";
      const string expected = @"So this text has multiline code. `<p> Some code we are looking at </p>`";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreH1Tags_ThenReplaceWithMarkDownHeader()
    {
      const string html = @"This code has a <h1>header</h1>. Convert it.";
      const string expected = @"This code has a

# header

. Convert it.";

      CheckConversion(html, expected);
    }


    [Test]
    public void Convert_WhenThereAreH2Tags_ThenReplaceWithMarkDownHeader()
    {
      const string html = @"This code has a <h2>header</h2>. Convert it.";
      const string expected = @"This code has a

## header

. Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreH3Tags_ThenReplaceWithMarkDownHeader()
    {
      const string html = @"This code has a <h3>header</h3>. Convert it.";
      const string expected = @"This code has a

### header

. Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreH4Tags_ThenReplaceWithMarkDownHeader()
    {
      const string html = @"This code has a <h4>header</h4>. Convert it.";
      const string expected = @"This code has a

#### header

. Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreH5Tags_ThenReplaceWithMarkDownHeader()
    {
      const string html = @"This code has a <h5>header</h5>. Convert it.";
      const string expected = @"This code has a

##### header

. Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreH6Tags_ThenReplaceWithMarkDownHeader()
    {
      const string html = @"This code has a <h6>header</h6>. Convert it.";
      const string expected = @"This code has a

###### header

. Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreH1TagsWithAttributes_ThenReplaceWithMarkDownHeader()
    {
      const string html = @"This code has a <h1 title=""header"">header</h1>. Convert it.";
      const string expected = @"This code has a

# header

. Convert it.";

      CheckConversion(html, expected);
    }


    [Test]
    public void Convert_WhenThereAreH2TagsWithAttributes_ThenReplaceWithMarkDownHeader()
    {
      const string html = @"This code has a <h2 title=""header"">header</h2>. Convert it.";
      const string expected = @"This code has a

## header

. Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreH3TagsWithAttributes_ThenReplaceWithMarkDownHeader()
    {
      const string html = @"This code has a <h3 title=""header"">header</h3>. Convert it.";
      const string expected = @"This code has a

### header

. Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreH4TagsWithAttributes_ThenReplaceWithMarkDownHeader()
    {
      const string html = @"This code has a <h4 title=""header"">header</h4>. Convert it.";
      const string expected = @"This code has a

#### header

. Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreH5TagsWithAttributes_ThenReplaceWithMarkDownHeader()
    {
      const string html = @"This code has a <h5 title=""header"">header</h5>. Convert it.";
      const string expected = @"This code has a

##### header

. Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreH6TagsWithAttributes_ThenReplaceWithMarkDownHeader()
    {
      const string html = @"This code has a <h6 title=""header"">header</h6>. Convert it.";
      const string expected = @"This code has a

###### header

. Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreBlockquoteTags_ThenReplaceWithMarkDownBlockQuote()
    {
      const string html = @"This code has a <blockquote>blockquote</blockquote>. Convert it.";
      const string expected = @"This code has a

> blockquote

. Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsABlockquoteTagWithNestedHtml_ThenReplaceWithMarkDownBlockQuote()
    {
      const string html = @"<blockquote><em>“Qualquer coisa que possas fazer ou sonhar, podes começá-la. A ousadia encerra em si mesma genialidade, poder e magia.<br />Ouse fazer, e o poder lhe será dado!”</em><br /><strong>— Johann Wolfgang von Goethe</strong></blockquote>";

      const string expected = @"> *“Qualquer coisa que possas fazer ou sonhar, podes começá-la. A ousadia encerra em si mesma genialidade, poder e magia.\
> Ouse fazer, e o poder lhe será dado!”*\
> **— Johann Wolfgang von Goethe**";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsAMultilineBlockquoteTag_ThenReplaceWithMarkDownBlockQuote()
    {
      const string html = @"<blockquote>
    <p class=""right"" align=""right""><em>“Ao estipular seus objetivos, mire na Lua; pois mesmo que aconteça de você não alcançá-los, ainda estará entre as estrelas!”</em><br />
    <strong>— Dr. Lair Ribeiro</strong></p>
  </blockquote>";

      const string expected = @"> *“Ao estipular seus objetivos, mire na Lua; pois mesmo que aconteça de você não alcançá-los, ainda estará entre as estrelas!”*\
> **— Dr. Lair Ribeiro**";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsABlockquoteTagWithAttributes_ThenReplaceWithMarkDownBlockQuote()
    {
      const string html = @"This code has a <blockquote id=""thing"">blockquote</blockquote>. Convert it.";
      const string expected = @"This code has a

> blockquote

. Convert it.";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreParagraphTags_ThenReplaceWithDoubleLineBreakBeforeAndOneAfter()
    {
      const string html = @"This code has no markup.<p>This code is in a paragraph.</p>Convert it!";
      const string expected = @"This code has no markup.

This code is in a paragraph.

Convert it!";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreParagraphTagsWithAttributes_ThenReplaceWithDoubleLineBreakBeforeAndOneAfter()
    {
      const string html = @"This code has no markup.<p class=""something"">This code is in a paragraph.</p>Convert it!";
      const string expected = @"This code has no markup.

This code is in a paragraph.

Convert it!";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreParagraphTagsWithNewLinesInThem_ThenReplaceWithMarkdownParagraphButNoBreakTags()
    {
      const string html = @"<p>
  text
  text
  text
</p>";
      const string expected = @"text text text";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreHorizontalRuleTags_ThenReplaceWithMarkDownHorizontalRule()
    {
      const string html = @"This code is seperated by a horizonrtal rule.<hr/>Convert it!";
      const string expected = @"This code is seperated by a horizonrtal rule.

* * *
Convert it!";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreHorizontalRuleTagsWithWhiteSpace_ThenReplaceWithMarkDownHorizontalRule()
    {
      const string html = @"This code is seperated by a horizonrtal rule.<hr />Convert it!";
      const string expected = @"This code is seperated by a horizonrtal rule.

* * *
Convert it!";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreHorizontalRuleTagsWithAttributes_ThenReplaceWithMarkDownHorizontalRule()
    {
      const string html = @"This code is seperated by a horizonrtal rule.<hr class=""something"" />Convert it!";
      const string expected = @"This code is seperated by a horizonrtal rule.

* * *
Convert it!";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreHorizontalRuleTagsThatAreNonSelfClosing_ThenReplaceWithMarkDownHorizontalRule()
    {
      const string html = @"This code is seperated by a horizonrtal rule.<hr>Convert it!";
      const string expected = @"This code is seperated by a horizonrtal rule.

* * *
Convert it!";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreImgTags_ThenReplaceWithMarkdownImage()
    {
      const string html = @"This code is with and image <img alt=""something"" title=""convert"" src=""https://assets-cdn.github.com/images/spinners/octocat-spinner-32.gif"" /> Convert it!";
      const string expected = @"This code is with and image ![something](https://assets-cdn.github.com/images/spinners/octocat-spinner-32.gif ""convert"") Convert it!";

      CheckConversion(html, expected);
    }


    [Test]
    public void Convert_WhenThereAreImgTagsWithoutATitle_ThenReplaceWithMarkdownImage()
    {
      const string html = @"This code is with an image <img alt=""something"" src=""https://assets-cdn.github.com/images/spinners/octocat-spinner-32.gif"" /> Convert it!";
      const string expected = @"This code is with an image ![something](https://assets-cdn.github.com/images/spinners/octocat-spinner-32.gif) Convert it!";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereArePreTags_ThenReplaceWithMarkdownPre()
    {
      const string html = @"This code is with a pre tag <pre>
  Predefined text</pre>";
      const string expected = @"This code is with a pre tag

      Predefined text";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreOtherTagsNestedInThePreTag_ThenReplaceWithMarkdownPre()
    {
      const string html = @"<pre><code>Install-Package Html2Markdown
</code></pre>";
      const string expected = @"    Install-Package Html2Markdown";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreMultiplePreTags_ThenReplaceWithMarkdownPre()
    {
      const string html = @"<h2>Installing via NuGet</h2>

<pre><code>Install-Package Html2Markdown
</code></pre>

<h2>Usage</h2>

<pre><code>var converter = new Converter();
var result = converter.Convert(html);
</code></pre>";
      const string expected = @"## Installing via NuGet

    Install-Package Html2Markdown

## Usage

    var converter = new Converter();
    var result = converter.Convert(html);";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreUnorderedLists_ThenReplaceWithMarkdownLists()
    {
      const string html = @"This code is with an unordered list.<ul><li>Yes</li><li>No</li></ul>";
      const string expected = @"This code is with an unordered list.

- Yes
- No";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereAreOrderedLists_ThenReplaceWithMarkdownLists()
    {
      const string html = @"This code is with an unordered list.<ol><li>Yes</li><li>No</li></ol>";
      const string expected = @"This code is with an unordered list.

1. Yes
2. No";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsAnUnorderedListWithANestedOrderList_ThenReplaceWithMarkdownLists()
    {
      const string html = @"This code is with an unordered list.<ul><li>Yes</li><li><ol><li>No</li><li>Maybe</li></ol></li></ul>";
      const string expected = @"This code is with an unordered list.

- Yes
- 
  1. No
  2. Maybe";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsAnOrderedListWithANestedUnorderList_ThenReplaceWithMarkdownLists()
    {
      const string html = @"This code is with an unordered list.<ol><li>Yes</li><li><ul><li>No</li><li>Maybe</li></ul></li></ol>";
      const string expected = @"This code is with an unordered list.

1. Yes
2. 
   - No
   - Maybe";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsAnHtmlDoctype_ThenRemoveFromResult()
    {
      const string html = @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
Doctypes should be removed";
      const string expected = @"Doctypes should be removed";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsAnHtmlTag_ThenRemoveFromResult()
    {
      const string html = @"<html>
<p>HTML tags should be removed</p>
</html>";
      const string expected = @"HTML tags should be removed";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsAnHtmlTagWithAttributes_ThenRemoveFromResult()
    {
      const string html = @"<html xmlns=""http://www.w3.org/1999/xhtml"" xml:lang=""pt-br"">
<p>HTML tags should be removed</p>
</html>";
      const string expected = @"HTML tags should be removed";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsASingleLineComment_ThenRemoveFromResult()
    {
      const string html = @"<!-- a comment -->
<p>Comments should be removed</p>";
      const string expected = @"Comments should be removed";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsAMultiLineComment_ThenRemoveFromResult()
    {
      const string html = @"<!-- 
a comment
-->
<p>Comments should be removed</p>";
      const string expected = @"Comments should be removed";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsAHeadTag_ThenRemoveFromResult()
    {
      const string html = @"<head>
<p>HTML tags should be removed</p>
</head>";
      const string expected = @"HTML tags should be removed";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsAHeadTagWithAttributes_ThenRemoveFromResult()
    {
      const string html = @"<head id=""something"">
<p>HTML tags should be removed</p>
</head>";
      const string expected = @"HTML tags should be removed";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsAMetaTag_ThenRemoveFromResult()
    {
      const string html = @"<meta name=""language"" content=""pt-br"">
<p>Meta tags should be removed</p>";
      const string expected = @"Meta tags should be removed";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsATitleTag_ThenRemoveFromResult()
    {
      const string html = @"<title>Remove me</title>
<p>Title tags should be removed</p>";
      const string expected = @"Title tags should be removed";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsATitleTagWithAttributes_ThenRemoveFromResult()
    {
      const string html = @"<title id=""something"">Remove me</title>
<p>Title tags should be removed</p>";
      const string expected = @"Title tags should be removed";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsALinkTag_ThenRemoveFromResult()
    {
      const string html = @"<link type=""text/css"" rel=""stylesheet"" href=""https://dl.dropboxusercontent.com/u/28729896/modelo-similar-blog-ss-para-sublime-text.css"">
<p>Link tags should be removed</p>";
      const string expected = @"Link tags should be removed";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsABodyTag_ThenRemoveFromResult()
    {
      const string html = @"<body>
<p>Body tags should be removed</p>
</body>";
      const string expected = @"Body tags should be removed";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsABodyTagWithAttributes_ThenRemoveFromResult()
    {
      const string html = @"<body id=""something"">
<p>Body tags should be removed</p>
</body>";
      const string expected = @"Body tags should be removed";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsAnAmpersandEntity_ThenReplaceWithActualCharacter()
    {
      const string html = @"<p>Enties like &amp; should be converted</p>";
      const string expected = @"Enties like & should be converted";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsAnLessThanEntity_ThenReplaceWithActualCharacter()
    {
      const string html = @"<p>Enties like &lt; should be converted</p>";
      const string expected = @"Enties like < should be converted";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsAGreaterThanEntity_ThenReplaceWithActualCharacter()
    {
      const string html = @"<p>Enties like &gt; should be converted</p>";
      const string expected = @"Enties like > should be converted";

      CheckConversion(html, expected);
    }

    [Test]
    public void Convert_WhenThereIsABulletEntity_ThenReplaceWithActualCharacter()
    {
      const string html = @"<p>Enties like &#8226; should be converted</p>";
      const string expected = @"Enties like • should be converted";

      CheckConversion(html, expected);
    }

    //https://github.com/mysticmind/reversemarkdown-net/blob/master/src/ReverseMarkdown.Test/ConverterTests.cs
    [Test]
    public void WhenThereIsEncompassingStrongOrBTag_ThenConvertToMarkdownDoubleAstericks_AnyStrongOrBTagsInsideAreIgnored()
    {
      const string html = @"<strong>Paragraph is encompassed with strong tag and also has <b>bold</b> text words within it</strong>";
      const string expected = @"**Paragraph is encompassed with strong tag and also has bold text words within it**";
      CheckConversion(html, expected);
    }

    [Test]
    public void WhenThereIsSingleAsterickInText_ThenConvertToMarkdownEscapedAsterick()
    {
      const string html = @"This is a sample(*) paragraph";
      const string expected = @"This is a sample(\*) paragraph";
      CheckConversion(html, expected);
    }

    [Test]
    public void WhenThereIsEncompassingEmOrITag_ThenConvertToMarkdownSingleAstericks_AnyEmOrITagsInsideAreIgnored()
    {
      const string html = @"<em>This is a <span><i>sample</i></span> paragraph</em>";
      const string expected = @"*This is a sample paragraph*";
      CheckConversion(html, expected);
    }

    [Test]
    public void WhenThereIsEmptyBlockquoteTag_ThenConvertToMarkdownBlockquote()
    {
      const string html = @"This text has <blockquote></blockquote>. This text appear after header.";
      const string expected = @"This text has

> 

. This text appear after header.";
      CheckConversion(html, expected);
    }

    [Test]
    public void WhenThereIsParagraphTag_ThenConvertToMarkdownDoubleLineBreakBeforeAndAfter()
    {
      const string html = @"This text has markup <p>paragraph.</p> Next line of text";
      const string expected = @"This text has markup

paragraph.

Next line of text";
      CheckConversion(html, expected);
    }

    [Test]
    public void WhenThereIsEmptyPreTag_ThenConvertToMarkdownPre()
    {
      const string html = @"This text has pre tag content <pre><br/ ></pre>Next line of text";
      const string expected = @"This text has pre tag content

    

Next line of text";
      CheckConversion(html, expected);
    }

    [Test]
    public void WhenListItemTextContainsLeadingAndTrailingSpacesAndTabs_TheConvertToMarkdownListItemWithSpacesAndTabsStripped()
    {
      const string html = @"<ol><li>	    This is a text with leading and trailing spaces and tabs		</li></ol>";
      const string expected = @"1. This is a text with leading and trailing spaces and tabs";
      CheckConversion(html, expected);
    }

    [Test]
    public void WhenListContainsNewlineAndTabBetweenTagBorders_CleanupAndConvertToMarkdown()
    {
      const string html = @"<ol>
  <li>
    <strong>Item1</strong></li>
  <li>
    Item2</li></ol>";
      const string expected = @"
1. **Item1**
2. Item2";
      CheckConversion(html, expected);
    }

    [Test]
    public void WhenListContainsMultipleParagraphs_ConvertToMarkdownAndIndentSiblings()
    {
      const string html = @"<ol>
  <li>
    <p>Item1</p>
        <p>Item2</p></li>
  <li>
    <p>Item3</p></li></ol>";
      const string expected = @"
1. Item1

   Item2
2. Item3";
      CheckConversion(html, expected);
    }

    private void CheckConversion(string html, string expected)
    {
      using (var reader = new HtmlReader(html))
      using (var strWriter = new StringWriter())
      using (var writer = new MarkdownWriter(strWriter))
      {
        reader.ToHtml(writer);
        writer.Flush();
        Assert.AreEqual(expected, strWriter.ToString());
      }
    }
  }
}
