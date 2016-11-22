using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AngleParse
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
      return formattable.ToString(null, _htmlProvider);
    }
    public static string Format(string format, params object[] args)
    {
      return string.Format(_htmlProvider, format, args);
    }

    private static FormatProvider _htmlProvider = new FormatProvider();

    private class FormatProvider : IFormatProvider, ICustomFormatter
    {
      public string Format(string format, object arg, IFormatProvider formatProvider)
      {
        var value = default(string);
        var raw = format == "!";
        if (raw)
          format = string.Empty;

        if (arg is IFormattable)
          value = ((IFormattable)arg).ToString(format, CultureInfo.CurrentCulture);
        else if (arg != null)
          value = arg.ToString();

        if (raw)
          return value;

        return Pool.NewStringBuilder().AppendHtmlEncoded(value).ToString();
      }

      public object GetFormat(Type formatType)
      {
        if (formatType == typeof(ICustomFormatter))
          return this;
        else
          return null;
      }
    }

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
            if (htmlWriter == null)
              writer.WriteComment(token.Value);
            else
              htmlWriter.WriteComment(token.Value, ((HtmlCommentNode)token).DownlevelRevealedConditional);
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
