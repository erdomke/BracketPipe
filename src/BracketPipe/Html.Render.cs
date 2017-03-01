using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace BracketPipe
{
  public static partial class Html
  {
    public static StringBuilder AppendHtmlEncoded(this StringBuilder builder, string value)
    {
      builder.EnsureCapacity(builder.Length + value.Length + 8);
      for (var i = 0; i < value.Length; i++)
      {
        switch (value[i])
        {
          case Symbols.Ampersand: builder.Append("&amp;"); break;
          case Symbols.NoBreakSpace: builder.Append("&nbsp;"); break;
          case Symbols.GreaterThan: builder.Append("&gt;"); break;
          case Symbols.LessThan: builder.Append("&lt;"); break;
          case Symbols.DoubleQuote: builder.Append("&quot;"); break;
          case Symbols.SingleQuote: builder.Append("&apos;"); break;
          default: builder.Append(value[i]); break;
        }
      }
      return builder;
    }

    public static string Format(IFormattable formattable)
    {
      return formattable.ToString(null, HtmlFormatProvider.Instance);
    }
    public static string Format(string format, params object[] args)
    {
      return string.Format(HtmlFormatProvider.Instance, format, args);
    }

    /// <summary>
    /// Render parsed HTML to a string
    /// </summary>
    public static HtmlString ToHtml(this IEnumerable<HtmlNode> reader)
    {
      using (var sw = new StringWriter())
      {
        ToHtml(reader, sw, new HtmlWriterSettings());
        return new HtmlString(sw.ToString());
      }
    }

    /// <summary>
    /// Render parsed HTML to a string
    /// </summary>
    public static HtmlString ToHtml(this IEnumerable<HtmlNode> reader, HtmlWriterSettings settings)
    {
      using (var sw = new StringWriter())
      {
        ToHtml(reader, sw, settings);
        return new HtmlString(sw.ToString());
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
    /// Convert parsed HTML to markdown
    /// </summary>
    public static string ToMarkdown(this IEnumerable<HtmlNode> reader)
    {
      using (var sw = new StringWriter())
      {
        ToMarkdown(reader, sw, new MarkdownWriterSettings());
        return sw.ToString();
      }
    }

    /// <summary>
    /// Convert parsed HTML to markdown
    /// </summary>
    public static string ToMarkdown(this IEnumerable<HtmlNode> reader, MarkdownWriterSettings settings)
    {
      using (var sw = new StringWriter())
      {
        ToMarkdown(reader, sw, settings);
        return sw.ToString();
      }
    }

    /// <summary>
    /// Convert parsed HTML to markdown
    /// </summary>
    public static string ToMarkdown(TextSource html, MarkdownWriterSettings settings = null)
    {
      var sb = Pool.NewStringBuilder();
      sb.EnsureCapacity(html.Length);
      using (var sw = new StringWriter(sb))
      using (var reader = new HtmlReader(html, false))
      {
        reader.ToMarkdown(sw, settings);
        sw.Flush();
        return sb.ToPool();
      }
    }

    /// <summary>
    /// Convert parsed HTML to markdown
    /// </summary>
    public static void ToMarkdown(this IEnumerable<HtmlNode> reader, TextWriter writer, MarkdownWriterSettings settings)
    {
      using (var w = new MarkdownWriter(writer, settings))
      {
        ToHtml(reader, w);
        w.Flush();
      }
    }


    /// <summary>
    /// Convert parsed HTML to plain text
    /// </summary>
    public static string ToPlainText(this IEnumerable<HtmlNode> reader)
    {
      using (var sw = new StringWriter())
      {
        ToPlainText(reader, sw);
        return sw.ToString();
      }
    }

    /// <summary>
    /// Convert parsed HTML to plain text
    /// </summary>
    public static string ToPlainText(TextSource html)
    {
      var sb = Pool.NewStringBuilder();
      sb.EnsureCapacity(html.Length);
      using (var sw = new StringWriter(sb))
      using (var reader = new HtmlReader(html, false))
      {
        reader.ToPlainText(sw);
        sw.Flush();
        return sb.ToPool();
      }
    }

    /// <summary>
    /// Convert parsed HTML to plain text
    /// </summary>
    public static void ToPlainText(this IEnumerable<HtmlNode> reader, TextWriter writer)
    {
      using (var w = new PlainTextWriter(writer))
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
      HtmlStartTag tag;
      var htmlWriter = writer as HtmlTextWriter;

      foreach (var token in reader)
      {
        switch (token.Type)
        {
          case HtmlTokenType.Text:
            writer.WriteString(token.Value);
            break;
          case HtmlTokenType.Comment:
            if (htmlWriter == null)
              writer.WriteComment(token.Value);
            else
              htmlWriter.WriteComment(token.Value, ((HtmlComment)token).DownlevelRevealedConditional);
            break;
          case HtmlTokenType.Doctype:
            var docType = (HtmlDoctype)token;
            writer.WriteDocType(token.Value, docType.PublicIdentifier, docType.SystemIdentifier, null);
            break;
          case HtmlTokenType.StartTag:
            tag = (HtmlStartTag)token;
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
