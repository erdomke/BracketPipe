using System.Collections.Generic;
using System.Linq;

namespace BracketPipe
{
  /// <summary>
  /// Settings controlling the rendering of HTML as text
  /// </summary>
  public class TextWriterSettings
  {
    private HashSet<string> _elementsToSkip = new HashSet<string>(new string[] {
      "title",
      "script",
      "style",
      "embed",
      "object",
      "noscript",
      "canvas",
      "dialog",
      "menu",
      "applet",
      "noembed",
      "nav",
    });

    /// <summary>
    /// Gets the tag names of HTML elements for which the content
    /// will not be rendered
    /// </summary>
    /// <value>
    /// The elements to skip.
    /// </value>
    public HashSet<string> ElementsToSkip { get { return _elementsToSkip; } }

    internal bool SkipElement(HtmlStartTag start)
    {
      if (_elementsToSkip.Contains(start.Value))
        return true;

      var style = start["style"];
      if (!string.IsNullOrEmpty(style))
      {
        foreach (var token in new CssTokenizer(style).Normalize().OfType<CssPropertyToken>())
        {
          switch (token.Data)
          {
            case "display":
              if (token.ArgumentCount == 1 && token.ArgumentTokens.Single().Data == "none")
                return true;
              break;
            case "visibility":
              if (token.ArgumentCount == 1 && token.ArgumentTokens.Single().Data == "hidden")
                return true;
              break;
          }
        }
      }
      
      return false;
    }
  }
}
