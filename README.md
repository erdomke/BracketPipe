# AngleParse

AngleParse is a .NET library for parsing angle bracket based hypertext languages like HTML, SVG, and MathML. The parser is built upon the official W3C specification.  It differentiates itself from other libraries such as [AngleSharp](https://github.com/AngleSharp/AngleSharp) (which it is based on) and [HTML Agility Pack](http://htmlagilitypack.codeplex.com/) in that it does not build an in-memory representation of the DOM.  Rather, it focuses on providing a convenient streaming interface for fast processing of HTML documents.  This makes the library ideal for 

* minifying HTML
* sanitizing HTML to prevent XSS attacks
* converting HTML to text
* crawling hyperlinks from HTML documents
* cleaning up MS Word HTML
* ... and any other task that only requires a single traversal of the HTML document

It can also be viewed as a modern update to previous projects such as the [SgmlReader](https://github.com/MindTouch/SGMLReader) and [Majestic-12 HTML Parser](http://www.majestic12.co.uk/projects/html_parser.php)

## Examples

Read the nodes from an HTML string or stream named `html`.

```csharp
using (var reader = new HtmlReader(html))
{
  foreach (var node in reader)
  {
    // Do something
  }
}
```

Render HTML nodes to a string

```csharp
using (var reader = new HtmlReader(html))
{
  var str = reader.ToHtml();
}
```

Sanitize user HTML

```csharp
var str = Html.Sanitize(html);
```

Write HTML

```csharp
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
  var str = s.ToString();
}
```

XML to HTML

```csharp
using (var s = new StringWriter())
using (var w = new HtmlTextWriter(s))
{
  var xml = new XElement("body",
    new XElement("div", new XAttribute("style", "color:red"), "A > value"),
    new XElement("input", new XAttribute("type", "text"), new XAttribute("value", "start")),
    new XElement("p", "para & more"));
  xml.WriteTo(w);
  w.Flush();

  var str = s.ToString();
}
```

## High Performance

Using only the stripped down core of [AngleSharp](https://github.com/AngleSharp/AngleSharp), AngleParse
achives incredibly fast performance.  In fact, it was measured to be over **4.5 times faster** for
HTML sanitization than [HtmlSanitizer](https://github.com/mganss/HtmlSanitizer) which leverages
AngleSharp's full DOM parser.

## Standards Conformance

The parser uses the HTML 5.1 specification, which defines error handling and element correction.  As 
a result, it works exactly as in all modern browsers.

## Portable

It is designed as a portable class library PCL (profile 259) that supports a wide range of platforms. 
The list includes, but is not limited to:

* .NET Core ("netstandard 1.0", see [.NET Platform Standard](https://github.com/dotnet/corefx/blob/master/Documentation/architecture/net-platform-standard.md))
* .NET Framework 4.5
* Windows 8.1
* Windows Phone 8.1 / Windows Phone Silverlight
* Xamarin.Android
* Xamarin.iOS

## License

The MIT License (MIT)

Copyright (c) 2013 - 2016 AngleParse

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
