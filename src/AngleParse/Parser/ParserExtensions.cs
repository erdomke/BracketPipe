using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngleParse
{
  public static class ParserExtensions
  {
    public static string ToHtml(this IEnumerable<HtmlToken> reader, HtmlWriterSettings settings)
    {
      using (var sw = new StringWriter())
      using (var w = new HtmlTextWriter(sw, settings))
      {
        HtmlTagToken tag;
        foreach (var token in reader)
        {
          switch (token.Type)
          {
            case HtmlTokenType.Character:
              w.WriteString(token.Data);
              break;
            case HtmlTokenType.Comment:
              w.WriteComment(token.Data);
              break;
            case HtmlTokenType.Doctype:
              var docType = (HtmlDoctypeToken)token;
              w.WriteDocType(token.Name, docType.PublicIdentifier, docType.SystemIdentifier, null);
              break;
            case HtmlTokenType.StartTag:
              tag = token.AsTag();
              w.WriteStartElement(tag.Name);
              foreach (var attr in tag.Attributes)
              {
                w.WriteAttributeString(attr.Key, attr.Value);
              }
              if (HtmlTextWriter.VoidElements.Contains(tag.Name))
                w.WriteEndElement();
              break;
            case HtmlTokenType.EndTag:
              w.WriteEndElement(token.Name);
              break;
          }
        }
        w.Flush();
        return sw.ToString();
      }
    }
  }
}
