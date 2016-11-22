using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngleParse
{
  public static partial class Html
  {
    public static string Sanitize(TextSource html, HtmlSanitizeSettings settings = null)
    {
      var sb = new StringBuilder(html.Length);
      using (var reader = new HtmlReader(html))
      using (var sw = new StringWriter(sb))
      {
        reader.Sanitize(settings).ToHtml(sw, new HtmlWriterSettings());
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

    /// <summary>
    /// Remove possible malicious tags and content from HTML. Attempts to prevent XSS patterns
    /// described on https://www.owasp.org/index.php/XSS_Filter_Evasion_Cheat_Sheet
    /// </summary>
    public static IEnumerable<HtmlNode> Sanitize(this IEnumerable<HtmlNode> reader)
    {
      return Sanitize(reader, HtmlSanitizeSettings.ReadOnlyDefault);
    }
    /// <summary>
    /// Remove possible malicious tags and content from HTML. Attempts to prevent XSS patterns
    /// described on https://www.owasp.org/index.php/XSS_Filter_Evasion_Cheat_Sheet
    /// </summary>
    public static IEnumerable<HtmlNode> Sanitize(this IEnumerable<HtmlNode> reader, HtmlSanitizeSettings settings)
    {
      var removeDepth = -1;
      var inStyle = false;
      settings = settings ?? HtmlSanitizeSettings.ReadOnlyDefault;

      foreach (var token in reader)
      {
        switch (token.Type)
        {
          case HtmlTokenType.Text:
            if (removeDepth < 0)
            {
              if (inStyle)
                yield return new HtmlNode(HtmlTokenType.Text, token.Position, SanitizeCss(token.Value, settings, true));
              else
                yield return token;
            }
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
                if (token.Value == "style")
                  inStyle = true;

                var allowed = AllowedAttributes(tag, settings).ToArray();

                if (tag.Value == "img" && !allowed.Any(k => k.Key == "src"))
                {
                  if (!HtmlTextWriter.VoidElements.Contains(tag.Value) && !tag.IsSelfClosing)
                    removeDepth = 0;
                }
                else
                {
                  var newTag = new HtmlTagNode(tag.Type, tag.Position, tag.Value);
                  newTag.IsSelfClosing = tag.IsSelfClosing;
                  foreach (var attr in allowed)
                  {
                    newTag.Attributes.Add(attr);
                  }
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
            if (removeDepth < 0 && settings.AllowedTags.Contains(token.Value))
              yield return token;
            else
              removeDepth--;

            if (token.Value == "style")
              inStyle = false;
            break;
        }
      }
    }

    private static IEnumerable<KeyValuePair<string, string>> AllowedAttributes(HtmlTagNode tag, HtmlSanitizeSettings settings)
    {
      for (var i = 0; i < tag.Attributes.Count; i++)
      {
        if (!settings.AllowedAttributes.Contains(tag.Attributes[i].Key))
        {
          // Do nothing
        }
        else if (string.Equals(tag.Attributes[i].Key, "style", StringComparison.OrdinalIgnoreCase))
        {
          var style = SanitizeCss(tag.Attributes[i].Value, settings, false);
          if (!string.IsNullOrWhiteSpace(style))
            yield return new KeyValuePair<string, string>(tag.Attributes[i].Key, style);
        }
        else if (settings.UriAttributes.Contains(tag.Attributes[i].Key))
        {
          var url = SanitizeUrl(tag.Attributes[i].Value, settings);
          if (url != null)
            yield return new KeyValuePair<string, string>(tag.Attributes[i].Key, url);
        }
        else if (!tag.Attributes[i].Value.StartsWith("&{"))
        {
          yield return tag.Attributes[i];
        }
      }
    }

    private static string SanitizeCss(string css, HtmlSanitizeSettings settings, bool styleTag)
    {
      using (var sw = new StringWriter())
      using (var writer = new CssWriter(sw))
      {
        foreach (var token in new CssTokenizer(css).Normalize())
        {
          var prop = token as CssPropertyToken;
          var group = token as CssAtGroupToken;
          if (prop != null)
          {
            if (settings.AllowedCssProps.Contains(prop.Data))
            {
              var removeProp = false;
              foreach (var arg in prop)
              {
                if (arg.Type == CssTokenType.Function && !settings.AllowedCssFunctions.Contains(arg.Data))
                {
                  removeProp = true;
                }
                else if (arg.Type == CssTokenType.Url)
                {
                  var url = SanitizeUrl(arg.Data, settings);
                  if (url == null)
                    removeProp = true;
                }
                else if (arg.Data.IndexOf('<') >= 0 || arg.Data.IndexOf('>') >= 0)
                {
                  removeProp = true;
                }
              }

              if (!removeProp)
                writer.Write(token);
            }
          }
          else if (group != null)
          {
            if (settings.AllowedCssAtRules.Contains(group.Data))
              writer.Write(group);
          }
          else if (styleTag && (token.Type != CssTokenType.Function || settings.AllowedCssFunctions.Contains(token.Data)))
          {
            writer.Write(token);
          }
        }
        sw.Flush();
        return sw.ToString();
      }
    }

    /// <summary>
    /// Sanitizes a URL.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="baseUrl">The base URL relative URLs are resolved against (empty or null for no resolution).</param>
    /// <returns>The sanitized URL or null if no safe URL can be created.</returns>
    private static string SanitizeUrl(string url, HtmlSanitizeSettings settings)
    {
      var uri = GetSafeUri(url, settings);

      if (uri == null) return null;

      try
      {
        return uri.IsAbsoluteUri ? uri.AbsoluteUri : uri.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped);
      }
      catch (Exception)
      {
        return null;
      }
    }

    /// <summary>
    /// Tries to create a safe <see cref="Uri"/> object from a string.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>The <see cref="Uri"/> object or null if no safe <see cref="Uri"/> can be created.</returns>
    private static Uri GetSafeUri(string url, HtmlSanitizeSettings settings)
    {
      Uri uri;
      if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri)
          || !uri.IsAbsoluteUri && !IsWellFormedRelativeUri(uri)
          || uri.IsAbsoluteUri && !settings.AllowedSchemes.Contains(uri.Scheme, StringComparer.OrdinalIgnoreCase))
        return null;
      if (!uri.IsAbsoluteUri && uri.ToString().IndexOf(':') > 0)
        return null;

      return uri;
    }

    private static readonly Uri _exampleUri = new Uri("http://www.example.com/");
    private static bool IsWellFormedRelativeUri(Uri uri)
    {
      Uri absoluteUri;
      return uri.OriginalString.IndexOf(':') < 0
        && Uri.TryCreate(_exampleUri, uri, out absoluteUri);
    }
  }
}
