using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BracketPipe
{
  public sealed class HtmlText : HtmlNode
  {
    #region Properties

    public override HtmlTokenType Type { get { return HtmlTokenType.Text; } }

    #endregion

    #region ctor

    public HtmlText(string value) : base(TextPosition.Empty, value) { }
    /// <summary>
    /// Sets the default values.
    /// </summary>
    /// <param name="type">The type of the tag token.</param>
    /// <param name="position">The token's position.</param>
    public HtmlText(TextPosition position, string value) : base(position, value) { }

    #endregion
  }
}
