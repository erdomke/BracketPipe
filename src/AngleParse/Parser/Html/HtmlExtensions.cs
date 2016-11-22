using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AngleParse
{
  public static class HtmlExtensions
  {
    /// <summary>
    /// Render parsed HTML to a string
    /// </summary>
    public static string ToHtml(this IEnumerable<HtmlNode> reader)
    {
      using (var sw = new StringWriter())
      {
        ToHtml(reader, sw, new HtmlWriterSettings());
        return sw.ToString();
      }
    }

    /// <summary>
    /// Render parsed HTML to a string
    /// </summary>
    public static string ToHtml(this IEnumerable<HtmlNode> reader, HtmlWriterSettings settings)
    {
      using (var sw = new StringWriter())
      {
        ToHtml(reader, sw, settings);
        return sw.ToString();
      }
    }

    /// <summary>
    /// Render parsed HTML to a text writer
    /// </summary>
    public static void ToHtml(this IEnumerable<HtmlNode> reader, TextWriter writer, HtmlWriterSettings settings)
    {
      using (var w = new HtmlTextWriter(writer, settings))
      {
        ToHtml(reader, w);
        w.Flush();
      }
    }

    /// <summary>
    /// Render parsed HTML to an XML writer
    /// </summary>
    public static void ToHtml(this IEnumerable<HtmlNode> reader, XmlWriter writer)
    {
      HtmlTagNode tag;
      var htmlWriter = writer as HtmlTextWriter;

      foreach (var token in reader)
      {
        switch (token.Type)
        {
          case HtmlTokenType.Text:
            writer.WriteString(token.Value);
            break;
          case HtmlTokenType.Comment:
            writer.WriteComment(token.Value);
            break;
          case HtmlTokenType.Doctype:
            var docType = (HtmlDoctypeNode)token;
            writer.WriteDocType(token.Value, docType.PublicIdentifier, docType.SystemIdentifier, null);
            break;
          case HtmlTokenType.StartTag:
            tag = token.AsTag();
            writer.WriteStartElement(tag.Value);
            foreach (var attr in tag.Attributes)
            {
              if (attr.Key != null
                && attr.Key[0] != '"'
                && attr.Key[0] != '\''
                && attr.Key[0] != '<'
                && attr.Key[0] != '=')
                writer.WriteAttributeString(attr.Key, attr.Value);
            }
            if (HtmlTextWriter.VoidElements.Contains(tag.Value)
              || tag.IsSelfClosing)
            {
              if (htmlWriter == null)
                writer.WriteEndElement();
              else
                htmlWriter.WriteEndElement(token.Value);
            }
            break;
          case HtmlTokenType.EndTag:
            if (htmlWriter == null)
              writer.WriteEndElement();
            else
              htmlWriter.WriteEndElement(token.Value);
            break;
        }
      }
    }
  }
}
