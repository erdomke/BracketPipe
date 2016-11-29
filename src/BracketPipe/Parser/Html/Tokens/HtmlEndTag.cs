using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BracketPipe
{
  public sealed class HtmlEndTag : HtmlTagNode
  {
    #region Properties

    public override HtmlTokenType Type { get { return HtmlTokenType.EndTag; } }

    #endregion

    #region ctor

    public HtmlEndTag(String name) : base(TextPosition.Empty, name) { }
    internal HtmlEndTag(TextPosition position) : base(position) { }
    public HtmlEndTag(TextPosition position, String name) : base (position, name) { }

    #endregion
  }
}
