using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BracketPipe
{
  public class HtmlMinifySettings
  {
    private HashSet<string> _inlineElement;
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
    public ISet<string> InlineElements { get { return _inlineElement; } }
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
      _inlineElement = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      _preserveInnerSpaceTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      _preserveSurroundingSpaceTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    private HtmlMinifySettings(bool setDefaults)
    {
      _inlineElement = new HashSet<string>(new string[]
      {
        "a",
        "abbr",
        "acronym",
        "b",
        "bdi",
        "bdo",
        "big",
        "br",
        "button",
        "cite",
        "code",
        "del",
        "dfn",
        "em",
        "font",
        "i",
        "img",
        "input",
        "ins",
        "kbd",
        "label",
        "map",
        "mark",
        "math",
        "meter",
        "object",
        "output",
        "progress",
        "q",
        "ruby",
        "rp",
        "rt",
        "s",
        "samp",
        "script",
        "select",
        "small",
        "span",
        "strike",
        "strong",
        "sub",
        "sup",
        "svg",
        "textarea",
        "time",
        "tt",
        "u",
        "var",
        "wbr"
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
