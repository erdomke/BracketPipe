using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngleParse
{
  public class HtmlCommentNode : HtmlNode
  {

    public bool DownlevelRevealedConditional { get; set; }

    #region ctor

    /// <summary>
    /// Sets the default values.
    /// </summary>
    /// <param name="type">The type of the tag token.</param>
    /// <param name="position">The token's position.</param>
    public HtmlCommentNode(TextPosition position, string value)
        : base(HtmlTokenType.Comment, position, value)
    {
    }

    #endregion
  }
}
