using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BracketPipe.Core.Tests
{
  /// <summary>
  /// Taken from https://github.com/kangax/html-minifier/blob/gh-pages/tests/minifier.js
  /// </summary>

  public class MinificationTests
  {
    [Fact]
    public void Minify_SpaceNormalizationBetweenAttributes()
    {
      Assert.Equal("<p title=\"bar\">foo</p>", Html.Minify("<p title=\"bar\">foo</p>"));
      Assert.Equal("<img src=\"test\">", Html.Minify("<img src=\"test\"/>"));
      Assert.Equal("<p title=\"bar\">foo</p>", Html.Minify("<p title = \"bar\">foo</p>"));
      Assert.Equal("<p title=\"bar\">foo</p>", Html.Minify("<p title\n\n\t  =\n     \"bar\">foo</p>"));
      Assert.Equal("<img src=\"test\">", Html.Minify("<img src=\"test\" \n\t />"));
      Assert.Equal("<input title=\"bar\" id=\"boo\" value=\"hello world\">", Html.Minify("<input title=\"bar\"       id=\"boo\"    value=\"hello world\">"));
    }

    [Fact]
    public void Minify_SpaceNormalizationAroundText()
    {
      Assert.Equal("<p>blah</p>", Html.Minify(" <p>blah</p>\n\n\n   "));

      foreach (var el in new string[] { "a", "abbr", "acronym", "b", "big", "del", "em", "font", "i", "ins", "kbd",
                                        "mark", "s", "samp", "small", "span", "strike", "strong", "sub", "sup",
                                        "time", "tt", "u", "var", "bdi", "bdo", "button", "cite", "code", "dfn",
                                        "math", "q", "rt", "rp", "svg" })
      {
        Assert.Equal("foo <" + el + ">baz</" + el + "> bar", Html.Minify("foo <" + el + ">baz</" + el + "> bar"));
        Assert.Equal("foo<" + el + ">baz</" + el + ">bar", Html.Minify("foo<" + el + ">baz</" + el + ">bar"));
        Assert.Equal("foo <" + el + ">baz</" + el + ">bar", Html.Minify("foo <" + el + ">baz</" + el + ">bar"));
        Assert.Equal("foo<" + el + ">baz</" + el + "> bar", Html.Minify("foo<" + el + ">baz</" + el + "> bar"));
        Assert.Equal("foo <" + el + ">baz </" + el + ">bar", Html.Minify("foo <" + el + "> baz </" + el + "> bar"));
        Assert.Equal("foo<" + el + "> baz </" + el + ">bar", Html.Minify("foo<" + el + "> baz </" + el + ">bar"));
        Assert.Equal("foo <" + el + ">baz </" + el + ">bar", Html.Minify("foo <" + el + "> baz </" + el + ">bar"));
        Assert.Equal("foo<" + el + "> baz </" + el + ">bar", Html.Minify("foo<" + el + "> baz </" + el + "> bar"));
        Assert.Equal("<div>foo <" + el + ">baz</" + el + "> bar</div>", Html.Minify("<div>foo <" + el + ">baz</" + el + "> bar</div>"));
        Assert.Equal("<div>foo<" + el + ">baz</" + el + ">bar</div>", Html.Minify("<div>foo<" + el + ">baz</" + el + ">bar</div>"));
        Assert.Equal("<div>foo <" + el + ">baz</" + el + ">bar</div>", Html.Minify("<div>foo <" + el + ">baz</" + el + ">bar</div>"));
        Assert.Equal("<div>foo<" + el + ">baz</" + el + "> bar</div>", Html.Minify("<div>foo<" + el + ">baz</" + el + "> bar</div>"));
        Assert.Equal("<div>foo <" + el + ">baz </" + el + ">bar</div>", Html.Minify("<div>foo <" + el + "> baz </" + el + "> bar</div>"));
        Assert.Equal("<div>foo<" + el + "> baz </" + el + ">bar</div>", Html.Minify("<div>foo<" + el + "> baz </" + el + ">bar</div>"));
        Assert.Equal("<div>foo <" + el + ">baz </" + el + ">bar</div>", Html.Minify("<div>foo <" + el + "> baz </" + el + ">bar</div>"));
        Assert.Equal("<div>foo<" + el + "> baz </" + el + ">bar</div>", Html.Minify("<div>foo<" + el + "> baz </" + el + "> bar</div>"));
      }

      Assert.Equal("<p>foo <img> bar</p>", Html.Minify("<p>foo <img> bar</p>"));
      Assert.Equal("<p>foo<img>bar</p>", Html.Minify("<p>foo<img>bar</p>"));
      Assert.Equal("<p>foo <img>bar</p>", Html.Minify("<p>foo <img>bar</p>"));
      Assert.Equal("<p>foo<img> bar</p>", Html.Minify("<p>foo<img> bar</p>"));
      Assert.Equal("<p>foo <wbr> bar</p>", Html.Minify("<p>foo <wbr> bar</p>"));
      Assert.Equal("<p>foo<wbr>bar</p>", Html.Minify("<p>foo<wbr>bar</p>"));
      Assert.Equal("<p>foo <wbr>bar</p>", Html.Minify("<p>foo <wbr>bar</p>"));
      Assert.Equal("<p>foo<wbr> bar</p>", Html.Minify("<p>foo<wbr> bar</p>"));
      Assert.Equal("<p>foo <wbr baz moo> bar</p>", Html.Minify("<p>foo <wbr baz moo=\"\"> bar</p>"));
      Assert.Equal("<p>foo<wbr baz moo>bar</p>", Html.Minify("<p>foo<wbr baz moo=\"\">bar</p>"));
      Assert.Equal("<p>foo <wbr baz moo>bar</p>", Html.Minify("<p>foo <wbr baz moo=\"\">bar</p>"));
      Assert.Equal("<p>foo<wbr baz moo> bar</p>", Html.Minify("<p>foo<wbr baz moo=\"\"> bar</p>"));
      Assert.Equal("<p><a href=\"#\"><code>foo</code></a> bar</p>", Html.Minify("<p>  <a href=\"#\">  <code>foo</code></a> bar</p>"));
      Assert.Equal("<p><a href=\"#\"><code>foo </code></a>bar</p>", Html.Minify("<p><a href=\"#\"><code>foo  </code></a> bar</p>"));
      Assert.Equal("<p><a href=\"#\"><code>foo</code></a> bar</p>", Html.Minify("<p>  <a href=\"#\">  <code>   foo</code></a> bar   </p>"));
      Assert.Equal("<div>Empty not</div>", Html.Minify("<div> Empty <!-- or --> not </div>"));

      //Assert.Equal("<li><i></i> <b></b> foo</li>", Html.Minify("<li><i></i> <b></b> foo</li>"));
      //Assert.Equal("<li><i></i> <b></b> foo</li>", Html.Minify("<li><i> </i> <b></b> foo</li>"));
      //Assert.Equal("<li><i></i> <b></b> foo</li>", Html.Minify("<li> <i></i> <b></b> foo</li>"));
      //Assert.Equal("<li><i></i> <b></b> foo</li>", Html.Minify("<li><i></i> <b> </b> foo</li>"));
      //Assert.Equal("<li><i></i> <b></b> foo</li>", Html.Minify("<li> <i> </i> <b> </b> foo</li>"));
      //Assert.Equal("<div><a href=\"#\"><span><b>foo </b><i>bar</i></span></a></div>", Html.Minify("<div> <a href=\"#\"> <span> <b> foo </b> <i> bar </i> </span> </a> </div>"));
    }

    [Fact]
    public void Minify_SimpleCommentHandling()
    {
      Assert.Equal("a c", Html.Minify(" a <? b ?> c "));
      Assert.Equal("a c", Html.Minify("<!-- d --> a <? b ?> c "));
      Assert.Equal("a c", Html.Minify(" <!-- d -->a <? b ?> c "));
      Assert.Equal("a c", Html.Minify(" a<!-- d --> <? b ?> c "));
      Assert.Equal("a c", Html.Minify(" a <!-- d --><? b ?> c "));
      Assert.Equal("a c", Html.Minify(" a <? b ?><!-- d --> c "));
      Assert.Equal("a c", Html.Minify(" a <? b ?> <!-- d -->c "));
      Assert.Equal("a c", Html.Minify(" a <? b ?> c<!-- d --> "));
      Assert.Equal("a c", Html.Minify(" a <? b ?> c <!-- d -->"));
    }

    [Fact]
    public void Minify_ConditionalComments()
    {
      var input = "<![if IE 5]>test<![endif]>";
      Assert.Equal(input, Html.Minify(input));

      input = "<!--[if IE 6]>test<![endif]-->";
      Assert.Equal(input, Html.Minify(input));

      input = "<!--[if IE 7]>-->test<!--<![endif]-->";
      Assert.Equal(input, Html.Minify(input));

      input = "<!--[if IE 8]><!-->test<!--<![endif]-->";
      Assert.Equal(input, Html.Minify(input));

      input = "<!--[if lt IE 5.5]>test<![endif]-->";
      Assert.Equal(input, Html.Minify(input));

      input = "<!--[if (gt IE 5)&(lt IE 7)]>test<![endif]-->";
      Assert.Equal(input, Html.Minify(input));

      input = "<!--[if IE 7]>\r\n\r\n   \t\r\n   \t\t " +
            "<link rel=\"stylesheet\" href=\"/css/ie7-fixes.css\" type=\"text/css\" />\r\n\t" +
          "<![endif]-->";
      Assert.Equal(input, Html.Minify(input));
    }

    [Fact]
    public void Minify_CleanClassStyleAttributes()
    {
      var input = "<p class=\" foo bar  \">foo bar baz</p>";
      Assert.Equal("<p class=\"foo bar\">foo bar baz</p>", Html.Minify(input));

      input = "<p class=\" foo      \">foo bar baz</p>";
      Assert.Equal("<p class=\"foo\">foo bar baz</p>", Html.Minify(input));

      input = "<p class=\"\n  \n foo   \n\n\t  \t\n   \">foo bar baz</p>";
      var output = "<p class=\"foo\">foo bar baz</p>";
      Assert.Equal(output, Html.Minify(input));

      input = "<p class=\"\n  \n foo   \n\n\t  \t\n  class1 class-23 \">foo bar baz</p>";
      output = "<p class=\"foo class1 class-23\">foo bar baz</p>";
      Assert.Equal(output, Html.Minify(input));

      input = "<p style=\"    color: red; background-color: rgb(100, 75, 200);  \"></p>";
      output = "<p style=\"color: red; background-color: rgb(100, 75, 200)\"></p>";
      Assert.Equal(output, Html.Minify(input));

      input = "<p style=\"font-weight: bold  ; \"></p>";
      output = "<p style=\"font-weight: bold\"></p>";
      Assert.Equal(output, Html.Minify(input));
    }

    [Fact]
    public void Minify_KeepSpaceAfterIcon()
    {
      var input = "<div><i class=\"i-icon\"></i>     Some text     </div>";
      var output = "<div><i class=\"i-icon\"></i> Some text</div>";
      Assert.Equal(output, Html.Minify(input));

      input = "<div><img src=\"test\">     Some text     </div>";
      output = "<div><img src=\"test\"> Some text</div>";
      Assert.Equal(output, Html.Minify(input));

      input = "<div><span><img src=\"test\"></span>     Some text     </div>";
      output = "<div><span><img src=\"test\"></span> Some text</div>";
      Assert.Equal(output, Html.Minify(input));
    }

    [Fact]
    public void Minify_DontAlterWbr()
    {
      var input = "<p>http://this<wbr>.is<wbr>.a<wbr>.really<wbr>.long<wbr>.example<wbr>.com/With<wbr>/deeper<wbr>/level<wbr>/pages<wbr>/deeper<wbr>/level<wbr>/pages<wbr>/deeper<wbr>/level<wbr>/pages<wbr>/deeper<wbr>/level<wbr>/pages<wbr>/deeper<wbr>/level<wbr>/pages</p>";
      Assert.Equal(input, Html.Minify(input));
    }

    [Fact]
    public void Minify_DontConcatenateScriptTags()
    {
      var input = "<script>var thing=2;</script><script>var another=4;</script>";
      Assert.Equal(input, Html.Minify(input));
    }

    [Fact]
    public void Minify_MinifyScriptTags()
    {
      var input = @"<script>function (thing, another) {
  var inner = Math.pow(2, 3);
  return inner;
}</script>";
      Assert.Equal("<script>function(thing,another){var inner=Math.pow(2,3);return inner;}</script>", Html.Minify(input));

      var settings = HtmlMinifySettings.Default();
      settings.ScriptTypesToCompress.Clear();
      Assert.Equal(input, Html.Minify(input, settings));
    }

    [Fact]
    public void Minify_NoSpaceBetweenNestedElements()
    {
      Assert.Equal("<div>Foo</div><div></div>",
  Html.Minify(@"<div>Foo</div>
<div></div>"));
    }
  }
}
