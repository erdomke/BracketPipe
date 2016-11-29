namespace BracketPipe
{
  using BracketPipe.Extensions;
  using System;

  /// <summary>
  /// The abstract base class of any HTML token.
  /// </summary>
  public abstract class HtmlNode
  {
    #region Fields

    readonly TextPosition _position;
    String _value;

    #endregion

    #region ctor

    public HtmlNode(TextPosition position)
        : this(position, null)
    {
    }

    public HtmlNode(TextPosition position, String value)
    {
      _position = position;
      _value = value;
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
    public abstract HtmlTokenType Type { get; }

    #endregion

    #region Methods

    public override string ToString()
    {
      return string.Format("{0} - {1}", Type, Value);
    }

    #endregion
  }
}
