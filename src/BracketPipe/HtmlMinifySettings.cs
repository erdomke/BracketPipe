using System;
using System.Collections.Generic;

namespace BracketPipe
{
  /// <summary>
  /// Settings used for minifying HTML (i.e. removing white space and comments)
  /// </summary>
  public class HtmlMinifySettings
  {
    private readonly HashSet<string> _inlineElement;
    private readonly HashSet<string> _preserveInnerSpaceTags;
    private readonly HashSet<string> _preserveSurroundingSpaceTags;
    private readonly HashSet<string> _scriptTypesToCompress;

#if NET35
    /// <summary>
    /// Tags for elements which appear inline (i.e. within a block)
    /// </summary>
    public HashSet<string> InlineElements { get { return _inlineElement; } }
    /// <summary>
    /// Tags for which inner space should be preserved
    /// </summary>
    public HashSet<string> PreserveInnerSpaceTags { get { return _preserveInnerSpaceTags; } }
    /// <summary>
    /// Tags for which surrounding space should be preserved
    /// </summary>
    public HashSet<string> PreserveSurroundingSpaceTags { get { return _preserveSurroundingSpaceTags; } }
    /// <summary>
    /// Script MIME types to to compress.
    /// </summary>
    public HashSet<string> ScriptTypesToCompress { get { return _scriptTypesToCompress; } }
#else
    /// <summary>
    /// Tags for elements which appear inline (i.e. within a block)
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
    /// <summary>
    /// Script MIME types to to compress.
    /// </summary>
    public ISet<string> ScriptTypesToCompress { get { return _scriptTypesToCompress; } }
#endif

    public HtmlMinifySettings()
    {
      _inlineElement = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      _preserveInnerSpaceTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      _preserveSurroundingSpaceTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      _scriptTypesToCompress = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    private HtmlMinifySettings(bool setDefaults)
    {
      _inlineElement = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
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
      };
      _preserveInnerSpaceTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
      {
        "pre",
        "textarea",
        "style",
      };
      _preserveSurroundingSpaceTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
      {
        "img",
        "input",
        "wbr",
      };
      _scriptTypesToCompress = new HashSet<string>()
      {
        "application/javascript",
        "application/json",
      };
    }

    private static HtmlMinifySettings _default = new HtmlMinifySettings(true);

    internal static HtmlMinifySettings ReadOnlyDefault { get { return _default; } }

    /// <summary>
    /// Create a new settings object using default values
    /// </summary>
    /// <returns>A new instance of <see cref="HtmlMinifySettings"/></returns>
    public static HtmlMinifySettings Default()
    {
      return new HtmlMinifySettings(true);
    }
  }
}
