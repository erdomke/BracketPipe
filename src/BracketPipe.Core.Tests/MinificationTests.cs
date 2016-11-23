using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BracketPipe.Core.Tests
{
  [TestFixture]
  class MinificationTests
  {
    [Test]
    public void Minify_SpaceNormalizationBetweenAttributes()
    {
      Assert.AreEqual("<p title=\"bar\">foo</p>", Html.Minify("<p title=\"bar\">foo</p>"));
      Assert.AreEqual("<img src=\"test\">", Html.Minify("<img src=\"test\"/>"));
      Assert.AreEqual("<p title=\"bar\">foo</p>", Html.Minify("<p title = \"bar\">foo</p>"));
      Assert.AreEqual("<p title=\"bar\">foo</p>", Html.Minify("<p title\n\n\t  =\n     \"bar\">foo</p>"));
      Assert.AreEqual("<img src=\"test\">", Html.Minify("<img src=\"test\" \n\t />"));
      Assert.AreEqual("<input title=\"bar\" id=\"boo\" value=\"hello world\">", Html.Minify("<input title=\"bar\"       id=\"boo\"    value=\"hello world\">"));
    }

    [Test]
    public void Minify_SpaceNormalizationAroundText()
    {
      Assert.AreEqual("<p>blah</p>", Html.Minify(" <p>blah</p>\n\n\n   "));

      foreach (var el in new string[] { "a", "abbr", "acronym", "b", "big", "del", "em", "font", "i", "ins", "kbd",
                                        "mark", "s", "samp", "small", "span", "strike", "strong", "sub", "sup",
                                        "time", "tt", "u", "var", "bdi", "bdo", "button", "cite", "code", "dfn",
                                        "math", "q", "rt", "rp", "svg" })
      {
        Assert.AreEqual("foo <" + el + ">baz</" + el + "> bar", Html.Minify("foo <" + el + ">baz</" + el + "> bar"));
        Assert.AreEqual("foo<" + el + ">baz</" + el + ">bar", Html.Minify("foo<" + el + ">baz</" + el + ">bar"));
        Assert.AreEqual("foo <" + el + ">baz</" + el + ">bar", Html.Minify("foo <" + el + ">baz</" + el + ">bar"));
        Assert.AreEqual("foo<" + el + ">baz</" + el + "> bar", Html.Minify("foo<" + el + ">baz</" + el + "> bar"));
        Assert.AreEqual("foo <" + el + ">baz </" + el + ">bar", Html.Minify("foo <" + el + "> baz </" + el + "> bar"));
        Assert.AreEqual("foo<" + el + "> baz </" + el + ">bar", Html.Minify("foo<" + el + "> baz </" + el + ">bar"));
        Assert.AreEqual("foo <" + el + ">baz </" + el + ">bar", Html.Minify("foo <" + el + "> baz </" + el + ">bar"));
        Assert.AreEqual("foo<" + el + "> baz </" + el + ">bar", Html.Minify("foo<" + el + "> baz </" + el + "> bar"));
        Assert.AreEqual("<div>foo <" + el + ">baz</" + el + "> bar</div>", Html.Minify("<div>foo <" + el + ">baz</" + el + "> bar</div>"));
        Assert.AreEqual("<div>foo<" + el + ">baz</" + el + ">bar</div>", Html.Minify("<div>foo<" + el + ">baz</" + el + ">bar</div>"));
        Assert.AreEqual("<div>foo <" + el + ">baz</" + el + ">bar</div>", Html.Minify("<div>foo <" + el + ">baz</" + el + ">bar</div>"));
        Assert.AreEqual("<div>foo<" + el + ">baz</" + el + "> bar</div>", Html.Minify("<div>foo<" + el + ">baz</" + el + "> bar</div>"));
        Assert.AreEqual("<div>foo <" + el + ">baz </" + el + ">bar</div>", Html.Minify("<div>foo <" + el + "> baz </" + el + "> bar</div>"));
        Assert.AreEqual("<div>foo<" + el + "> baz </" + el + ">bar</div>", Html.Minify("<div>foo<" + el + "> baz </" + el + ">bar</div>"));
        Assert.AreEqual("<div>foo <" + el + ">baz </" + el + ">bar</div>", Html.Minify("<div>foo <" + el + "> baz </" + el + ">bar</div>"));
        Assert.AreEqual("<div>foo<" + el + "> baz </" + el + ">bar</div>", Html.Minify("<div>foo<" + el + "> baz </" + el + "> bar</div>"));
      }

      Assert.AreEqual("<p>foo <img> bar</p>", Html.Minify("<p>foo <img> bar</p>"));
      Assert.AreEqual("<p>foo<img>bar</p>", Html.Minify("<p>foo<img>bar</p>"));
      Assert.AreEqual("<p>foo <img>bar</p>", Html.Minify("<p>foo <img>bar</p>"));
      Assert.AreEqual("<p>foo<img> bar</p>", Html.Minify("<p>foo<img> bar</p>"));
      Assert.AreEqual("<p>foo <wbr> bar</p>", Html.Minify("<p>foo <wbr> bar</p>"));
      Assert.AreEqual("<p>foo<wbr>bar</p>", Html.Minify("<p>foo<wbr>bar</p>"));
      Assert.AreEqual("<p>foo <wbr>bar</p>", Html.Minify("<p>foo <wbr>bar</p>"));
      Assert.AreEqual("<p>foo<wbr> bar</p>", Html.Minify("<p>foo<wbr> bar</p>"));
      Assert.AreEqual("<p>foo <wbr baz moo> bar</p>", Html.Minify("<p>foo <wbr baz moo=\"\"> bar</p>"));
      Assert.AreEqual("<p>foo<wbr baz moo>bar</p>", Html.Minify("<p>foo<wbr baz moo=\"\">bar</p>"));
      Assert.AreEqual("<p>foo <wbr baz moo>bar</p>", Html.Minify("<p>foo <wbr baz moo=\"\">bar</p>"));
      Assert.AreEqual("<p>foo<wbr baz moo> bar</p>", Html.Minify("<p>foo<wbr baz moo=\"\"> bar</p>"));
      Assert.AreEqual("<p><a href=\"#\"><code>foo</code></a> bar</p>", Html.Minify("<p>  <a href=\"#\">  <code>foo</code></a> bar</p>"));
      Assert.AreEqual("<p><a href=\"#\"><code>foo </code></a>bar</p>", Html.Minify("<p><a href=\"#\"><code>foo  </code></a> bar</p>"));
      Assert.AreEqual("<p><a href=\"#\"><code>foo</code></a> bar</p>", Html.Minify("<p>  <a href=\"#\">  <code>   foo</code></a> bar   </p>"));
      Assert.AreEqual("<div>Empty not</div>", Html.Minify("<div> Empty <!-- or --> not </div>"));

      //Assert.AreEqual("<li><i></i> <b></b> foo</li>", Html.Minify("<li><i></i> <b></b> foo</li>"));
      //Assert.AreEqual("<li><i></i> <b></b> foo</li>", Html.Minify("<li><i> </i> <b></b> foo</li>"));
      //Assert.AreEqual("<li><i></i> <b></b> foo</li>", Html.Minify("<li> <i></i> <b></b> foo</li>"));
      //Assert.AreEqual("<li><i></i> <b></b> foo</li>", Html.Minify("<li><i></i> <b> </b> foo</li>"));
      //Assert.AreEqual("<li><i></i> <b></b> foo</li>", Html.Minify("<li> <i> </i> <b> </b> foo</li>"));
      //Assert.AreEqual("<div><a href=\"#\"><span><b>foo </b><i>bar</i></span></a></div>", Html.Minify("<div> <a href=\"#\"> <span> <b> foo </b> <i> bar </i> </span> </a> </div>"));
    }

    [Test]
    public void Minify_SimpleCommentHandling()
    {
      Assert.AreEqual("a c", Html.Minify(" a <? b ?> c "));
      Assert.AreEqual("a c", Html.Minify("<!-- d --> a <? b ?> c "));
      Assert.AreEqual("a c", Html.Minify(" <!-- d -->a <? b ?> c "));
      Assert.AreEqual("a c", Html.Minify(" a<!-- d --> <? b ?> c "));
      Assert.AreEqual("a c", Html.Minify(" a <!-- d --><? b ?> c "));
      Assert.AreEqual("a c", Html.Minify(" a <? b ?><!-- d --> c "));
      Assert.AreEqual("a c", Html.Minify(" a <? b ?> <!-- d -->c "));
      Assert.AreEqual("a c", Html.Minify(" a <? b ?> c<!-- d --> "));
      Assert.AreEqual("a c", Html.Minify(" a <? b ?> c <!-- d -->"));
    }

    [Test]
    public void Minify_ConditionalComments()
    {
      var input = "<![if IE 5]>test<![endif]>";
      Assert.AreEqual(input, Html.Minify(input));

      input = "<!--[if IE 6]>test<![endif]-->";
      Assert.AreEqual(input, Html.Minify(input));

      input = "<!--[if IE 7]>-->test<!--<![endif]-->";
      Assert.AreEqual(input, Html.Minify(input));

      input = "<!--[if IE 8]><!-->test<!--<![endif]-->";
      Assert.AreEqual(input, Html.Minify(input));

      input = "<!--[if lt IE 5.5]>test<![endif]-->";
      Assert.AreEqual(input, Html.Minify(input));

      input = "<!--[if (gt IE 5)&(lt IE 7)]>test<![endif]-->";
      Assert.AreEqual(input, Html.Minify(input));

      input = "<!--[if IE 7]>\r\n\r\n   \t\r\n   \t\t " +
            "<link rel=\"stylesheet\" href=\"/css/ie7-fixes.css\" type=\"text/css\" />\r\n\t" +
          "<![endif]-->";
      Assert.AreEqual(input, Html.Minify(input));
    }

    [Test]
    public void Minify_CleanClassStyleAttributes()
    {
      var input = "<p class=\" foo bar  \">foo bar baz</p>";
      Assert.AreEqual("<p class=\"foo bar\">foo bar baz</p>", Html.Minify(input));

      input = "<p class=\" foo      \">foo bar baz</p>";
      Assert.AreEqual("<p class=\"foo\">foo bar baz</p>", Html.Minify(input));

      input = "<p class=\"\n  \n foo   \n\n\t  \t\n   \">foo bar baz</p>";
      var output = "<p class=\"foo\">foo bar baz</p>";
      Assert.AreEqual(output, Html.Minify(input));

      input = "<p class=\"\n  \n foo   \n\n\t  \t\n  class1 class-23 \">foo bar baz</p>";
      output = "<p class=\"foo class1 class-23\">foo bar baz</p>";
      Assert.AreEqual(output, Html.Minify(input));

      input = "<p style=\"    color: red; background-color: rgb(100, 75, 200);  \"></p>";
      output = "<p style=\"color: red; background-color: rgb(100, 75, 200)\"></p>";
      Assert.AreEqual(output, Html.Minify(input));

      input = "<p style=\"font-weight: bold  ; \"></p>";
      output = "<p style=\"font-weight: bold\"></p>";
      Assert.AreEqual(output, Html.Minify(input));
    }
  }
}
