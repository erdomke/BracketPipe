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
    /// Remove possible malicious tags and content from HTML. Attempts to prevent XSS patterns
    /// described on https://www.owasp.org/index.php/XSS_Filter_Evasion_Cheat_Sheet
    /// </summary>
    public static IEnumerable<HtmlNode> Sanitize(this IEnumerable<HtmlNode> reader)
    {
      return Sanitize(reader, HtmlSanitizeSettings.Default());
    }
    /// <summary>
    /// Remove possible malicious tags and content from HTML. Attempts to prevent XSS patterns
    /// described on https://www.owasp.org/index.php/XSS_Filter_Evasion_Cheat_Sheet
    /// </summary>
    public static IEnumerable<HtmlNode> Sanitize(this IEnumerable<HtmlNode> reader, HtmlSanitizeSettings settings)
    {
      var removeDepth = -1;
      bool schemeRemoved;
      foreach (var token in reader)
      {
        switch (token.Type)
        {
          case HtmlTokenType.Text:
            if (removeDepth < 0)
              yield return token;
            break;
          case HtmlTokenType.Comment:
            // No need to risk weird comments that might be interpreted as content (e.g. in IE)
            break;
          case HtmlTokenType.Doctype:
            // Doctypes should not appear in snippets
            break;
          case HtmlTokenType.StartTag:
            var tag = token.AsTag();
            if (removeDepth < 0)
            {
              if (settings.AllowedTags.Contains(token.Value))
              {
                var allowed = AllowedAttributes(tag, settings, out schemeRemoved);
                if (schemeRemoved)
                {
                  if (!HtmlTextWriter.VoidElements.Contains(tag.Value) && !tag.IsSelfClosing)
                    removeDepth = 0;
                }
                else if (allowed.Count == tag.Attributes.Count)
                {
                  yield return token;
                }
                else
                {
                  var newTag = new HtmlTagNode(tag.Type, tag.Position, tag.Value);
                  newTag.IsSelfClosing = tag.IsSelfClosing;
                  newTag.Attributes.AddRange(allowed);
                  yield return newTag;
                }
              }
              else if (!HtmlTextWriter.VoidElements.Contains(tag.Value) && !tag.IsSelfClosing)
              {
                removeDepth = 0;
              }
            }
            else
            {
              if (!HtmlTextWriter.VoidElements.Contains(tag.Value) && !tag.IsSelfClosing)
                removeDepth++;
            }
            break;
          case HtmlTokenType.EndTag:
            if (removeDepth < 0)
              yield return token;
            else
              removeDepth--;
            break;
        }
      }
    }

    private static IList<KeyValuePair<string, string>> AllowedAttributes(HtmlTagNode tag, HtmlSanitizeSettings settings, out bool schemeRemoved)
    {
      var result = default(List<KeyValuePair<string, string>>);
      schemeRemoved = false;
      for (var i = 0; i < tag.Attributes.Count; i++)
      {
        if (!settings.AllowedAttributes.Contains(tag.Attributes[i].Key))
        {
          if (result == null)
          {
            result = new List<KeyValuePair<string, string>>();
            for (var j = 0; j < i; j++)
              result.Add(tag.Attributes[j]);
          }
        }
        else if ((string.Equals(tag.Attributes[i].Key, "src", StringComparison.OrdinalIgnoreCase)
              || string.Equals(tag.Attributes[i].Key, "href", StringComparison.OrdinalIgnoreCase))
            && !IsAllowedScheme(tag.Attributes[i].Value, settings))
        {
          schemeRemoved = true;
          if (result == null)
          {
            result = new List<KeyValuePair<string, string>>();
            for (var j = 0; j < i; j++)
              result.Add(tag.Attributes[j]);
          }
        }
        else if (result != null)
        {
          result.Add(tag.Attributes[i]);
        }
      }
      return result ?? tag.Attributes;
    }

    private static bool IsAllowedScheme(string uri, HtmlSanitizeSettings settings)
    {
      Uri parsed;
      if (!Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out parsed))
        return false;
      if (!parsed.IsAbsoluteUri)
        return uri.IndexOf(':') < 0;
      return settings.AllowedSchemes.Contains(parsed.Scheme);
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
