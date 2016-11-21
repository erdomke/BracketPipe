namespace AngleParse
{
  using AngleParse.Extensions;
  using AngleParse.Html;
  using System;

  /// <summary>
  /// The abstract base class of any HTML token.
  /// </summary>
  public class HtmlNode
  {
    #region Fields

    readonly HtmlTokenType _type;
    readonly TextPosition _position;
    String _value;

    #endregion

    #region ctor

    public HtmlNode(HtmlTokenType type, TextPosition position)
        : this(type, position, null)
    {
    }

    public HtmlNode(HtmlTokenType type, TextPosition position, String name)
    {
      _type = type;
      _position = position;
      _value = name;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the name of a tag token.
    /// </summary>
    public String Value
    {
      get { return _value; }
      internal set { _value = value; }
    }

    /// <summary>
    /// Gets the position of the token.
    /// </summary>
    public TextPosition Position
    {
      get { return _position; }
    }

    /// <summary>
    /// Gets the type of the token.
    /// </summary>
    public HtmlTokenType Type
    {
      get { return _type; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Converts the current token to a tag token.
    /// </summary>
    /// <returns>The tag token instance.</returns>
    public HtmlTagNode AsTag()
    {
      return (HtmlTagNode)this;
    }

    public override string ToString()
    {
      return string.Format("{0} - {1}", Type, Value);
    }

    #endregion
  }
}
