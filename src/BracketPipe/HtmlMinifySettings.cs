using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BracketPipe
{
  public class HtmlMinifySettings
  {
    private HashSet<string> _blockLevelElement;
    private HashSet<string> _preserveInnerSpaceTags;
    private HashSet<string> _preserveSurroundingSpaceTags;

#if NET35
    /// <summary>
    /// Tags for elements which start a new block
    /// </summary>
    public HashSet<string> BlockLevelElements { get { return _blockLevelElement; } }
    /// <summary>
    /// Tags for which inner space should be preserved
    /// </summary>
    public HashSet<string> PreserveInnerSpaceTags { get { return _preserveInnerSpaceTags; } }
    /// <summary>
    /// Tags for which surrounding space should be preserved
    /// </summary>
    public HashSet<string> PreserveSurroundingSpaceTags { get { return _preserveSurroundingSpaceTags; } }
#else
    /// <summary>
    /// Tags for elements which start a new block
    /// </summary>
    public ISet<string> BlockLevelElements { get { return _blockLevelElement; } }
    /// <summary>
    /// Tags for which inner space should be preserved
    /// </summary>
    public ISet<string> PreserveInnerSpaceTags { get { return _preserveInnerSpaceTags; } }
    /// <summary>
    /// Tags for which surrounding space should be preserved
    /// </summary>
    public ISet<string> PreserveSurroundingSpaceTags { get { return _preserveSurroundingSpaceTags; } }
#endif

    public HtmlMinifySettings()
    {
      _blockLevelElement = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      _preserveInnerSpaceTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      _preserveSurroundingSpaceTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    private HtmlMinifySettings(bool setDefaults)
    {
      _blockLevelElement = new HashSet<string>(new string[]
      {
        "address",
        "article",
        "aside",
        "blockquote",
        "canvas",
        "dd",
        "div",
        "dl",
        "fieldset",
        "figcaption",
        "figure",
        "footer",
        "form",
        "h1",
        "h2",
        "h3",
        "h4",
        "h5",
        "h6",
        "header",
        "hgroup",
        "hr",
        "li",
        "main",
        "nav",
        "noscript",
        "ol",
        "output",
        "p",
        "pre",
        "section",
        "table",
        "tfoot",
        "ul",
        "video",
      }, StringComparer.OrdinalIgnoreCase);
      _preserveInnerSpaceTags = new HashSet<string>(new string[]
      {
        "pre",
        "textarea",
        "style",
      }, StringComparer.OrdinalIgnoreCase);
      _preserveSurroundingSpaceTags = new HashSet<string>(new string[]
      {
        "img",
        "input",
        "wbr",
      }, StringComparer.OrdinalIgnoreCase);

    }

    private static HtmlMinifySettings _default = new HtmlMinifySettings(true);

    internal static HtmlMinifySettings ReadOnlyDefault { get { return _default; } }

    public static HtmlMinifySettings Default()
    {
      return new HtmlMinifySettings(true);
    }
  }
}
