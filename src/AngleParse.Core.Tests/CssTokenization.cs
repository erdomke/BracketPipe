namespace AngleParse.Core.Tests
{
  using System.Linq;
  using NUnit.Framework;

  [TestFixture]
  public class CssTokenizationTests
  {
    [Test]
    public void CssParserIdentifier()
    {
      var teststring = "h1 { background: blue; }";
      var tokenizer = new CssTokenizer(new TextSource(teststring));
      var token = tokenizer.NextToken();
      var type = typeof(string).GetType().GetField("");

      Assert.AreEqual(CssTokenType.Ident, token.Type);
    }

    [Test]
    public void CssParserAtRule()
    {
      var teststring = "@media { background: blue; }";
      var tokenizer = new CssTokenizer(new TextSource(teststring));
      var token = tokenizer.NextToken();
      Assert.AreEqual(CssTokenType.AtKeyword, token.Type);
    }

    [Test]
    public void CssParserUrlUnquoted()
    {
      var url = "http://someurl";
      var teststring = "url(" + url + ")";
      var tokenizer = new CssTokenizer(new TextSource(teststring));
      var token = tokenizer.NextToken();
      Assert.AreEqual(url, token.Data);
    }

    [Test]
    public void CssParserUrlDoubleQuoted()
    {
      var url = "http://someurl";
      var teststring = "url(\"" + url + "\")";
      var tokenizer = new CssTokenizer(new TextSource(teststring));
      var token = tokenizer.NextToken();
      Assert.AreEqual(url, token.Data);
    }

    [Test]
    public void CssParserUrlSingleQuoted()
    {
      var url = "http://someurl";
      var teststring = "url('" + url + "')";
      var tokenizer = new CssTokenizer(new TextSource(teststring));
      var token = tokenizer.NextToken();
      Assert.AreEqual(url, token.Data);
    }

    [Test]
    public void CssTokenizerOnlyCarriageReturn()
    {
      var teststring = "\r";
      var tokenizer = new CssTokenizer(new TextSource(teststring));
      var token = tokenizer.NextToken();
      Assert.AreEqual("\n", token.Data);
    }

    [Test]
    public void CssTokenizerCarriageReturnLineFeed()
    {
      var teststring = "\r\n";
      var tokenizer = new CssTokenizer(new TextSource(teststring));
      var token = tokenizer.NextToken();
      Assert.AreEqual("\n", token.Data);
    }

    [Test]
    public void CssTokenizerOnlyLineFeed()
    {
      var teststring = "\n";
      var tokenizer = new CssTokenizer(new TextSource(teststring));
      var token = tokenizer.NextToken();
      Assert.AreEqual("\n", token.Data);
    }

    [Test]
    public void CssNormalizeTest()
    {
      var nparts1 = new CssTokenizer(@"background-image:url(javascript:alert('XSS'))").Normalize().ToArray();
      var nparts2 = new CssTokenizer(@"background-image:\0075\0072\006C\0028'\006a\0061\0076\0061\0073\0063\0072\0069\0070\0074\003a\0061\006c\0065\0072\0074\0028\0027\0058\0053\0053\0027\0029'\0029").Normalize().ToArray();
      Assert.AreEqual(nparts1.Length, nparts2.Length);
      var nparts3 = new CssTokenizer(@"background-image:\0075\0072\006C\0028\006a\0061\0076\0061\0073\0063\0072\0069\0070\0074\003a\0061\006c\0065\0072\0074\0028\0027\0058\0053\0053\0027\0029\0029").Normalize().ToArray();
      Assert.AreEqual(nparts1.Length, nparts3.Length);
      var nparts4 = new CssTokenizer(@"background-image:\0075r\006C\0028'\006a\0061\0076\0061\0073\0063\0072\0069\0070\0074\003a\0061\006c\0065\0072\0074\0028\0027\0058\0053\0053\0027\0029'\0029").Normalize().ToArray();
      Assert.AreEqual(nparts1.Length, nparts4.Length);

      var nparts11 = new CssTokenizer(@"xss:expression(alert('XSS'))").Normalize().ToArray();
      var nparts12 = new CssTokenizer(@"xss:expr/*XSS*/ession(alert('XSS'))").Normalize().ToArray();
      Assert.AreEqual(nparts11.Length, nparts12.Length);
      var nparts13 = new CssTokenizer(@"xss:expr/*XSS*/ess/*XSS*/ion(alert('XSS'))").Normalize().ToArray();
      Assert.AreEqual(nparts11.Length, nparts13.Length);
    }
  }
}
