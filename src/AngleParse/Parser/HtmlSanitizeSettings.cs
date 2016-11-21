using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngleParse
{
  public class HtmlSanitizeSettings
  {
    private HashSet<string> _allowedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private HashSet<string> _allowedAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private HashSet<string> _allowedSchemes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// HTML tag names which are permitted through the sanitize routine
    /// </summary>
    public HashSet<string> AllowedTags { get { return _allowedTags; } }
    /// <summary>
    /// HTML attribute names which are permitted through the sanitize routine
    /// </summary>
    public HashSet<string> AllowedAttributes { get { return _allowedAttributes; } }
    /// <summary>
    /// Allowed URI schemes (e.g. in the <c>href</c> attribute of <c>a</c> tags)
    /// </summary>
    public HashSet<string> AllowedSchemes { get { return _allowedSchemes; } }

    public static HtmlSanitizeSettings Default()
    {
      var result = new HtmlSanitizeSettings();
      result._allowedAttributes.UnionWith(new string[]
      {
        "align",
        "alt",
        "bgcolor",
        "border",
        "cellpadding",
        "cellspacing",
        "colspan",
        "coords",
        "datetime",
        "dir",
        "for",
        "height",
        "href",
        "name",
        "rel",
        "rowspan",
        "shape",
        "span",
        "src",
        "target",
        "title",
        "type",
        "valign",
        "value",
        "width",
      });
      result._allowedSchemes.UnionWith(new string[]
      {
        "data",
        "ftp",
        "http",
        "https",
        "mailto",
      });
      result._allowedTags.UnionWith(new string[]
      {
        "a",
        "abbr",
        "acronym",
        "address",
        "area",
        "b",
        "bdo",
        "big",
        "blockquote",
        "br",
        "button",
        "caption",
        "center",
        "cite",
        "code",
        "col",
        "colgroup",
        "dd",
        "del",
        "dfn",
        "dir",
        "div",
        "dl",
        "dt",
        "em",
        "fieldset",
        "font",
        "h1",
        "h2",
        "h3",
        "h4",
        "h5",
        "h6",
        "hr",
        "i",
        "img",
        "input",
        "ins",
        "kbd",
        "label",
        "legend",
        "li",
        "map",
        "menu",
        "ol",
        "optgroup",
        "option",
        "p",
        "pre",
        "q",
        "s",
        "samp",
        "select",
        "small",
        "span",
        "strike",
        "strong",
        "sub",
        "sup",
        "table",
        "tbody",
        "td",
        "textarea",
        "tfoot",
        "th",
        "thead",
        "u",
        "tr",
        "tt",
        "u",
        "ul",
        "var",
      });
      return result;
    }
  }
}
