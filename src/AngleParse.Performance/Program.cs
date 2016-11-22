using Ganss.XSS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleParse;
using System.IO;

namespace AngleParse.Performance
{
  class Program
  {
    static void Main(string[] args)
    {
      var sanitizer = new HtmlSanitizer();
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
      }
      Console.ReadLine();

    }
  }
}
