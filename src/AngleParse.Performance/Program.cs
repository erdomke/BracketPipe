using Ganss.XSS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleParse;
using System.IO;
using WebMarkupMin.Core;

namespace AngleParse.Performance
{
  class Program
  {
    static void Main(string[] args)
    {
      /*var sanitizer = new HtmlSanitizer();
      var html = @"<script>alert('xss')</script><div onload=""alert('xss')"""
          + @"style=""background-color: test"">Test<img src=""test.gif"""
          + @"style=""background-image: url(javascript:alert('xss')); margin: 10px""></div>";
      var memStream = new MemoryStream(Encoding.UTF8.GetBytes(html));

      Stopwatch st;
      for (var j = 0; j < 5; j++)
      {
        st = Stopwatch.StartNew();
        for (var i = 0; i < 5000; i++)
        {
          var sanitized = sanitizer.Sanitize(html, "http://www.example.com");
        }
        Console.WriteLine("HtmlSanitizer {0}ms", st.ElapsedMilliseconds);

        st = Stopwatch.StartNew();
        for (var i = 0; i < 5000; i++)
        {
          var sanitized = Html.Sanitize(html);
        }
        Console.WriteLine("AngleParse {0}ms", st.ElapsedMilliseconds);
      }*/


      const string htmlInput = @"<!DOCTYPE html>
<html>
    <head>
        <meta charset=""utf-8"" />
        <title>The test document</title>
        <link href=""favicon.ico"" rel=""shortcut icon"" type=""image/x-icon"" />
        <meta name=""viewport"" content=""width=device-width"" />
        <link rel=""stylesheet"" type=""text/css"" href=""/Content/Site.css"" />
    </head>
    <body>
        <p>Lorem ipsum dolor sit amet...</p>

        <script src=""http://ajax.aspnetcdn.com/ajax/jQuery/jquery-1.9.1.min.js""></script>
        <script>
            (window.jquery) || document.write('<script src=""/Scripts/jquery-1.9.1.min.js""><\/script>');
        </script>
    </body>
</html>";

      var htmlMinifier = new HtmlMinifier();
      for (var j = 0; j < 5; j++)
      {
        var st = Stopwatch.StartNew();
        for (var i = 0; i < 5000; i++)
        {
          var result = htmlMinifier.Minify(htmlInput);
        }
        Console.WriteLine("WebMarkupMin {0}ms", st.ElapsedMilliseconds);

        st = Stopwatch.StartNew();
        for (var i = 0; i < 5000; i++)
        {
          var result = Html.Minify(htmlInput);
        }
        Console.WriteLine("AngleParse {0}ms", st.ElapsedMilliseconds);
      }

      Console.ReadLine();

    }
  }
}
