using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngleParse.Core.Tests
{
  [TestFixture]
  class Majestic12
  {
    private void TestParser(string data, string expected)
    {
      using (var reader = new HtmlReader(data))
      {
        var rendered = reader.ToHtml();
        Assert.AreEqual(expected, rendered);
      }
    }
    private void TestParser(string data)
    {
      TestParser(data, data);
    }


    [Test]
    public void Majestic12_OpenTagsWithoutAttributes()
    {
      TestParser("<p>");

      // add spacing
      TestParser("<p >", "<p>");
      TestParser("<p  >", "<p>");
      TestParser("< p  >", "&lt; p  &gt;");
      TestParser("<  p  >", "&lt;  p  &gt;");
      TestParser("  <p     " + "\n\r" + ">  ", "  <p>  ");
      TestParser("  <p     " + "\r\n" + ">  ", "  <p>  ");
      TestParser("  <p     " + "\n" + ">  ", "  <p>  ");
      TestParser("  <p     " + "\r" + ">  ", "  <p>  ");

      TestParser("<ho>");

      TestParser("<hr width=90%>", "<hr width=\"90%\">");

      TestParser("<hr>");
      TestParser("<br>");

      TestParser("<brrr    name=", "");
      TestParser("<brrr    name", "");

      TestParser("<br style='whooo'>", "<br style=\"whooo\">");
    }


    [Test]
    public void Majestic12_OpenTagsWithAttributes()
    {
      TestParser("<td x:num=\"38669\" abc=xl29 z:num=\"12345\">", "<td x:num=\"38669\" abc=\"xl29\" z:num=\"12345\">");

      TestParser("<a href=\"test\"title=\"some title\">", "<a href=\"test\" title=\"some title\">");
      TestParser("<a href='test'title='some title'>", "<a href=\"test\" title=\"some title\">");

      TestParser("<font    color   = blue>", "<font color=\"blue\">");

      TestParser("<font   color   = blue><b>", "<font color=\"blue\"><b>");

      TestParser("<a href=\"?url=&lt;\">");

      TestParser("<a href=''>", "<a href>");
      TestParser("<a href>", "<a href>");
    }

    [Test]
    public void Majestic12_Text()
    {
      // empty text
      TestParser("");

      TestParser("                    whitespace        ");

      TestParser("some text");
      TestParser("some text with entities &amp; blah blah &amp; Co ");

      TestParser("some text with wrong entities &aoped;", "some text with wrong entities &amp;aoped;");
      TestParser("some text with wrong entities &aopeddddddddddd;", "some text with wrong entities &amp;aopeddddddddddd;");
      TestParser("some text with wrong entities &", "some text with wrong entities &amp;");
    }

    [Test]
    public void Majestic12_Script()
    {
      TestParser("<SCRipt> some javascript here</SCRI blah", "<script> some javascript here</SCRI blah");

      // intentionally lack script at the end - parser should not fail
      TestParser("<script> some javascript here", "<script> some javascript here");

      TestParser("<script> some javascript here</script>", "<script> some javascript here</script>");

      // FIXIT: this test case really shows that our current behavior is wrong...
      TestParser("<script><!-- some javascript here<b>--></script>", "<script><!-- some javascript here<b>--></script>");
    }

    [Test]
    public void Majestic12_ClosedTagsWithoutAttributes()
    {
      TestParser("</p>");
      TestParser("</b>");

      TestParser("</ho>");

      TestParser("</hr>");
      TestParser("</br>");

      TestParser("</p    >", "</p>");

      TestParser("</P    >", "</p>");

      TestParser("</clsss>", "</clsss>");

      TestParser("</class>", "</class>");
      TestParser("</CLASS>", "</class>");
      TestParser("</CLass>", "</class>");

      TestParser("<class />", "<class></class>");
      TestParser("<CLASS/>", "<class></class>");
      TestParser("<CLass / >", "<class>");
    }

    [Test]
    public void Majestic12_CrlfBeforeClose()
    {
      TestParser(@"<SPAN font-weight:bold; color:#000000""
>MyText </SPAN
></P>", @"<span font-weight:bold; color:#000000"">MyText </span></p>");
    }
  }
}
