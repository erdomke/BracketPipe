using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngleParse
{
  public static class Html
  {
    public static string Sanitize(TextSource html, HtmlSanitizeSettings settings = null)
    {
      var sb = new StringBuilder(html.Length);
      using (var reader = new HtmlReader(html))
      using (var sw = new StringWriter(sb))
      {
        reader.Sanitize(settings).ToHtml(new HtmlWriterSettings());
        return sw.ToString();
      }
    }

    public static void Sanitize(TextSource html, HtmlTextWriter writer, HtmlSanitizeSettings settings = null)
    {
      using (var reader = new HtmlReader(html))
      {
        reader.Sanitize(settings).ToHtml(writer);
      }
    }
  }
}
