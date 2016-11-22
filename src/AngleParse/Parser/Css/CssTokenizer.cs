namespace AngleParse
{
  using AngleParse.Css;
  using AngleParse.Extensions;
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Collections;

  /// <summary>
  /// The CSS tokenizer.
  /// See http://dev.w3.org/csswg/css-syntax/#tokenization for more details.
  /// </summary>
  public class CssTokenizer
    : BaseTokenizer
    , IEnumerator<CssToken>
    , IEnumerable<CssToken>
  {
    #region Fields

    private Boolean _valueMode;
    private TextPosition _position;
    private CssToken _current;

    #endregion

    #region Events

    /// <summary>
    /// Fired in case of a parse error.
    /// </summary>
    public event EventHandler<CssErrorEvent> Error;

    #endregion

    #region ctor

    /// <summary>
    /// CSS Tokenization
    /// </summary>
    /// <param name="source">The source code manager.</param>
    public CssTokenizer(TextSource source)
        : base(source)
    {
      _valueMode = false;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets if we are currently in value mode.
    /// </summary>
    public Boolean IsInValue
    {
      get { return _valueMode; }
      set { _valueMode = value; }
    }

    public CssToken Current { get { return _current; } }
    object IEnumerator.Current { get { return _current; } }

    #endregion

    #region Methods

    /// <summary>
    /// Reads the next token.
    /// </summary>
    /// <returns><c>true</c> if the reader has reached the end; <c>false</c> otherwise</returns>
    public bool Read()
    {
      return NextToken().Type != CssTokenType.EndOfFile;
    }

    /// <summary>
    /// Gets the next available token.
    /// </summary>
    /// <returns>The next available token.</returns>
    public CssToken NextToken()
    {
      var current = Advance();
      _position = GetCurrentPosition();
      _current = Data(current);
      return _current;
    }

    internal void RaiseErrorOccurred(CssParseError error, TextPosition position)
    {
      var handler = Error;

      if (handler != null)
      {
        var errorEvent = new CssErrorEvent(error, position);
        handler.Invoke(this, errorEvent);
      }
    }

    #endregion

    #region States

    /// <summary>
    /// 4.4.1. Data state
    /// </summary>
    CssToken Data(Char current)
    {
      _position = GetCurrentPosition();

      switch (current)
      {
        case Symbols.FormFeed:
        case Symbols.LineFeed:
        case Symbols.CarriageReturn:
        case Symbols.Tab:
        case Symbols.Space:
          return NewWhitespace(current);

        case Symbols.DoubleQuote:
          return StringDQ();

        case Symbols.Num:
          return _valueMode ? ColorLiteral() : HashStart();

        case Symbols.Dollar:
          current = Advance();

          if (current == Symbols.Equality)
          {
            return NewMatch(CombinatorSymbols.Ends);
          }

          return NewDelimiter(Back());

        case Symbols.SingleQuote:
          return StringSQ();

        case Symbols.RoundBracketOpen:
          return NewOpenRound();

        case Symbols.RoundBracketClose:
          return NewCloseRound();

        case Symbols.Asterisk:
          current = Advance();

          if (current == Symbols.Equality)
          {
            return NewMatch(CombinatorSymbols.InText);
          }

          return NewDelimiter(Back());

        case Symbols.Plus:
          {
            var c1 = Advance();

            if (c1 != Symbols.EndOfFile)
            {
              var c2 = Advance();
              Back(2);

              if (c1.IsDigit() || (c1 == Symbols.Dot && c2.IsDigit()))
              {
                return NumberStart(current);
              }
            }
            else
            {
              Back();
            }

            return NewDelimiter(current);
          }

        case Symbols.Comma:
          return NewComma();

        case Symbols.Dot:
          {
            var c = Advance();

            if (c.IsDigit())
            {
              return NumberStart(Back());
            }

            return NewDelimiter(Back());
          }

        case Symbols.Minus:
          {
            var c1 = Advance();

            if (c1 != Symbols.EndOfFile)
            {
              var c2 = Advance();
              Back(2);

              if (c1.IsDigit() || (c1 == Symbols.Dot && c2.IsDigit()))
              {
                return NumberStart(current);
              }
              else if (c1.IsNameStart())
              {
                return IdentStart(current);
              }
              else if (c1 == Symbols.ReverseSolidus && !c2.IsLineBreak() && c2 != Symbols.EndOfFile)
              {
                return IdentStart(current);
              }
              else if (c1 == Symbols.Minus && c2 == Symbols.GreaterThan)
              {
                Advance(2);
                return NewCloseComment();
              }
            }
            else
            {
              Back();
            }

            return NewDelimiter(current);
          }

        case Symbols.Solidus:
          current = Advance();

          if (current == Symbols.Asterisk)
          {
            return Comment();
          }

          return NewDelimiter(Back());

        case Symbols.ReverseSolidus:
          current = Advance();

          if (current.IsLineBreak())
          {
            RaiseErrorOccurred(CssParseError.LineBreakUnexpected);
            return NewDelimiter(Back());
          }
          else if (current == Symbols.EndOfFile)
          {
            RaiseErrorOccurred(CssParseError.EOF);
            return NewDelimiter(Back());
          }

          return IdentStart(Back());
        case Symbols.Colon:
          return NewColon();

        case Symbols.Semicolon:
          return NewSemicolon();

        case Symbols.LessThan:
          current = Advance();

          if (current == Symbols.ExclamationMark)
          {
            current = Advance();

            if (current == Symbols.Minus)
            {
              current = Advance();

              if (current == Symbols.Minus)
              {
                return NewOpenComment();
              }

              current = Back();
            }

            current = Back();
          }

          return NewDelimiter(Back());

        case Symbols.At:
          return AtKeywordStart();

        case Symbols.SquareBracketOpen:
          return NewOpenSquare();

        case Symbols.SquareBracketClose:
          return NewCloseSquare();

        case Symbols.Accent:
          current = Advance();

          if (current == Symbols.Equality)
          {
            return NewMatch(CombinatorSymbols.Begins);
          }

          return NewDelimiter(Back());

        case Symbols.CurlyBracketOpen:
          return NewOpenCurly();

        case Symbols.CurlyBracketClose:
          return NewCloseCurly();

        case '0':
        case '1':
        case '2':
        case '3':
        case '4':
        case '5':
        case '6':
        case '7':
        case '8':
        case '9':
          return NumberStart(current);

        case 'U':
        case 'u':
          current = Advance();

          if (current == Symbols.Plus)
          {
            current = Advance();

            if (current.IsHex() || current == Symbols.QuestionMark)
            {
              return UnicodeRange(current);
            }

            current = Back();
          }

          return IdentStart(Back());

        case Symbols.Pipe:
          current = Advance();

          if (current == Symbols.Equality)
          {
            return NewMatch(CombinatorSymbols.InToken);
          }
          else if (current == Symbols.Pipe)
          {
            return NewColumn();
          }

          return NewDelimiter(Back());

        case Symbols.Tilde:
          current = Advance();

          if (current == Symbols.Equality)
          {
            return NewMatch(CombinatorSymbols.InList);
          }

          return NewDelimiter(Back());

        case Symbols.EndOfFile:
          return NewEof();

        case Symbols.ExclamationMark:
          current = Advance();

          if (current == Symbols.Equality)
          {
            return NewMatch(CombinatorSymbols.Unlike);
          }

          return NewDelimiter(Back());

        default:
          if (current.IsNameStart())
          {
            return IdentStart(current);
          }

          return NewDelimiter(current);
      }
    }

    /// <summary>
    /// 4.4.2. Double quoted string state
    /// </summary>
    CssToken StringDQ()
    {
      while (true)
      {
        var current = Advance();

        switch (current)
        {
          case Symbols.DoubleQuote:
          case Symbols.EndOfFile:
            return NewString(FlushBuffer(), Symbols.DoubleQuote);

          case Symbols.FormFeed:
          case Symbols.LineFeed:
            RaiseErrorOccurred(CssParseError.LineBreakUnexpected);
            Back();
            return NewString(FlushBuffer(), Symbols.DoubleQuote, bad: true);

          case Symbols.ReverseSolidus:
            current = Advance();

            if (current.IsLineBreak())
            {
              StringBuffer.AppendLine();
            }
            else if (current != Symbols.EndOfFile)
            {
              StringBuffer.Append(ConsumeEscape(current));
            }
            else
            {
              RaiseErrorOccurred(CssParseError.EOF);
              Back();
              return NewString(FlushBuffer(), Symbols.DoubleQuote, bad: true);
            }

            break;

          default:
            StringBuffer.Append(current);
            break;
        }
      }
    }

    /// <summary>
    /// 4.4.3. Single quoted string state
    /// </summary>
    CssToken StringSQ()
    {
      while (true)
      {
        var current = Advance();

        switch (current)
        {
          case Symbols.SingleQuote:
          case Symbols.EndOfFile:
            return NewString(FlushBuffer(), Symbols.SingleQuote);

          case Symbols.FormFeed:
          case Symbols.LineFeed:
            RaiseErrorOccurred(CssParseError.LineBreakUnexpected);
            Back();
            return NewString(FlushBuffer(), Symbols.SingleQuote, bad: true);

          case Symbols.ReverseSolidus:
            current = Advance();

            if (current.IsLineBreak())
            {
              StringBuffer.AppendLine();
            }
            else if (current != Symbols.EndOfFile)
            {
              StringBuffer.Append(ConsumeEscape(current));
            }
            else
            {
              RaiseErrorOccurred(CssParseError.EOF);
              Back();
              return NewString(FlushBuffer(), Symbols.SingleQuote, bad: true);
            }

            break;

          default:
            StringBuffer.Append(current);
            break;
        }
      }
    }

    /// <summary>
    /// Color literal state.
    /// </summary>
    CssToken ColorLiteral()
    {
      var current = Advance();

      while (current.IsHex())
      {
        StringBuffer.Append(current);
        current = Advance();
      }

      Back();
      return NewColor(FlushBuffer());
    }

    /// <summary>
    /// 4.4.4. Hash state
    /// </summary>
    CssToken HashStart()
    {
      var current = Advance();

      if (current.IsNameStart())
      {
        StringBuffer.Append(current);
        return HashRest();
      }
      else if (IsValidEscape(current))
      {
        current = Advance();
        StringBuffer.Append(ConsumeEscape(current));
        return HashRest();
      }
      else if (current == Symbols.ReverseSolidus)
      {
        RaiseErrorOccurred(CssParseError.InvalidCharacter);
        Back();
        return NewDelimiter(Symbols.Num);
      }
      else
      {
        Back();
        return NewDelimiter(Symbols.Num);
      }
    }

    /// <summary>
    /// 4.4.5. Hash-rest state
    /// </summary>
    CssToken HashRest()
    {
      while (true)
      {
        var current = Advance();

        if (current.IsName())
        {
          StringBuffer.Append(current);
        }
        else if (IsValidEscape(current))
        {
          current = Advance();
          StringBuffer.Append(ConsumeEscape(current));
        }
        else if (current == Symbols.ReverseSolidus)
        {
          RaiseErrorOccurred(CssParseError.InvalidCharacter);
          Back();
          return NewHash(FlushBuffer());
        }
        else
        {
          Back();
          return NewHash(FlushBuffer());
        }
      }
    }

    /// <summary>
    /// 4.4.6. Comment state
    /// </summary>
    CssToken Comment()
    {
      var current = Advance();

      while (current != Symbols.EndOfFile)
      {
        if (current == Symbols.Asterisk)
        {
          current = Advance();

          if (current == Symbols.Solidus)
          {
            return NewComment(FlushBuffer());
          }

          StringBuffer.Append(Symbols.Asterisk);
        }
        else
        {
          StringBuffer.Append(current);
          current = Advance();
        }
      }

      RaiseErrorOccurred(CssParseError.EOF);
      return NewComment(FlushBuffer(), bad: true);
    }

    /// <summary>
    /// 4.4.7. At-keyword state
    /// </summary>
    CssToken AtKeywordStart()
    {
      var current = Advance();

      if (current == Symbols.Minus)
      {
        current = Advance();

        if (current.IsNameStart() || IsValidEscape(current))
        {
          StringBuffer.Append(Symbols.Minus);
          return AtKeywordRest(current);
        }

        Back(2);
        return NewDelimiter(Symbols.At);
      }
      else if (current.IsNameStart())
      {
        StringBuffer.Append(current);
        return AtKeywordRest(Advance());
      }
      else if (IsValidEscape(current))
      {
        current = Advance();
        StringBuffer.Append(ConsumeEscape(current));
        return AtKeywordRest(Advance());
      }
      else
      {
        Back();
        return NewDelimiter(Symbols.At);
      }
    }

    /// <summary>
    /// 4.4.8. At-keyword-rest state
    /// </summary>
    CssToken AtKeywordRest(Char current)
    {
      while (true)
      {
        if (current.IsName())
        {
          StringBuffer.Append(current);
        }
        else if (IsValidEscape(current))
        {
          current = Advance();
          StringBuffer.Append(ConsumeEscape(current));
        }
        else
        {
          Back();
          return NewAtKeyword(FlushBuffer());
        }

        current = Advance();
      }
    }

    /// <summary>
    /// 4.4.9. Ident state
    /// </summary>
    CssToken IdentStart(Char current)
    {
      if (current == Symbols.Minus)
      {
        current = Advance();

        if (current.IsNameStart() || IsValidEscape(current))
        {
          StringBuffer.Append(Symbols.Minus);
          return IdentRest(current);
        }

        Back();
        return NewDelimiter(Symbols.Minus);
      }
      else if (current.IsNameStart())
      {
        StringBuffer.Append(current);
        return IdentRest(Advance());
      }
      else if (current == Symbols.ReverseSolidus && IsValidEscape(current))
      {
        current = Advance();
        StringBuffer.Append(ConsumeEscape(current));
        return IdentRest(Advance());
      }

      return Data(current);
    }

    /// <summary>
    /// 4.4.10. Ident-rest state
    /// </summary>
    CssToken IdentRest(Char current)
    {
      while (true)
      {
        if (current.IsName())
        {
          StringBuffer.Append(current);
        }
        else if (IsValidEscape(current))
        {
          current = Advance();
          StringBuffer.Append(ConsumeEscape(current));
        }
        else if (current == Symbols.RoundBracketOpen)
        {
          var name = FlushBuffer();
          var type = name.GetTypeFromName();
          return type == CssTokenType.Function ? NewFunction(name) : UrlStart(name.ToLowerInvariant());
        }
        else
        {
          Back();
          return NewIdent(FlushBuffer());
        }

        current = Advance();
      }
    }

    /// <summary>
    /// 4.4.11. Transform-function-whitespace state
    /// </summary>
    CssToken TransformFunctionWhitespace(Char current)
    {
      while (true)
      {
        current = Advance();

        if (current == Symbols.RoundBracketOpen)
        {
          Back();
          return NewFunction(FlushBuffer());
        }
        else if (!current.IsSpaceCharacter())
        {
          Back(2);
          return NewIdent(FlushBuffer());
        }
      }
    }

    /// <summary>
    /// 4.4.12. Number state
    /// </summary>
    CssToken NumberStart(Char current)
    {
      while (true)
      {
        if (current.IsOneOf(Symbols.Plus, Symbols.Minus))
        {
          StringBuffer.Append(current);
          current = Advance();

          if (current == Symbols.Dot)
          {
            StringBuffer.Append(current);
            StringBuffer.Append(Advance());
            return NumberFraction();
          }

          StringBuffer.Append(current);
          return NumberRest();
        }
        else if (current == Symbols.Dot)
        {
          StringBuffer.Append(current);
          StringBuffer.Append(Advance());
          return NumberFraction();
        }
        else if (current.IsDigit())
        {
          StringBuffer.Append(current);
          return NumberRest();
        }

        current = Advance();
      }
    }

    /// <summary>
    /// 4.4.13. Number-rest state
    /// </summary>
    CssToken NumberRest()
    {
      var current = Advance();

      while (true)
      {

        if (current.IsDigit())
        {
          StringBuffer.Append(current);
        }
        else if (current.IsNameStart())
        {
          var number = FlushBuffer();
          StringBuffer.Append(current);
          return Dimension(number);
        }
        else if (IsValidEscape(current))
        {
          current = Advance();
          var number = FlushBuffer();
          StringBuffer.Append(ConsumeEscape(current));
          return Dimension(number);
        }
        else
        {
          break;
        }

        current = Advance();
      }

      switch (current)
      {
        case Symbols.Dot:
          current = Advance();

          if (current.IsDigit())
          {
            StringBuffer.Append(Symbols.Dot).Append(current);
            return NumberFraction();
          }

          Back();
          return NewNumber(FlushBuffer());

        case '%':
          return NewPercentage(FlushBuffer());

        case 'e':
        case 'E':
          return NumberExponential(current);

        case Symbols.Minus:
          return NumberDash();

        default:
          Back();
          return NewNumber(FlushBuffer());
      }
    }

    /// <summary>
    /// 4.4.14. Number-fraction state
    /// </summary>
    CssToken NumberFraction()
    {
      var current = Advance();

      while (true)
      {
        if (current.IsDigit())
        {
          StringBuffer.Append(current);
        }
        else if (current.IsNameStart())
        {
          var number = FlushBuffer();
          StringBuffer.Append(current);
          return Dimension(number);
        }
        else if (IsValidEscape(current))
        {
          current = Advance();
          var number = FlushBuffer();
          StringBuffer.Append(ConsumeEscape(current));
          return Dimension(number);
        }
        else
        {
          break;
        }

        current = Advance();
      }

      switch (current)
      {
        case 'e':
        case 'E':
          return NumberExponential(current);

        case '%':
          return NewPercentage(FlushBuffer());

        case Symbols.Minus:
          return NumberDash();

        default:
          Back();
          return NewNumber(FlushBuffer());
      }
    }

    /// <summary>
    /// 4.4.15. Dimension state
    /// </summary>
    CssToken Dimension(String number)
    {
      while (true)
      {
        var current = Advance();

        if (current.IsLetter())
        {
          StringBuffer.Append(current);
        }
        else if (IsValidEscape(current))
        {
          current = Advance();
          StringBuffer.Append(ConsumeEscape(current));
        }
        else
        {
          Back();
          return NewDimension(number, FlushBuffer());
        }
      }
    }

    /// <summary>
    /// 4.4.16. SciNotation state
    /// </summary>
    CssToken SciNotation()
    {
      while (true)
      {
        var current = Advance();

        if (current.IsDigit())
        {
          StringBuffer.Append(current);
        }
        else
        {
          Back();
          return NewNumber(FlushBuffer());
        }
      }
    }

    /// <summary>
    /// 4.4.17. URL state
    /// </summary>
    CssToken UrlStart(String functionName)
    {
      var current = SkipSpaces();

      switch (current)
      {
        case Symbols.EndOfFile:
          RaiseErrorOccurred(CssParseError.EOF);
          return NewUrl(functionName, String.Empty, bad: true);

        case Symbols.DoubleQuote:
          return UrlDQ(functionName);

        case Symbols.SingleQuote:
          return UrlSQ(functionName);

        case Symbols.RoundBracketClose:
          return NewUrl(functionName, String.Empty, bad: false);

        default:
          return UrlUQ(current, functionName);
      }
    }

    /// <summary>
    /// 4.4.18. URL-double-quoted state
    /// </summary>
    CssToken UrlDQ(String functionName)
    {
      while (true)
      {
        var current = Advance();

        if (current.IsLineBreak())
        {
          RaiseErrorOccurred(CssParseError.LineBreakUnexpected);
          return UrlBad(functionName);
        }
        else if (Symbols.EndOfFile == current)
        {
          return NewUrl(functionName, FlushBuffer());
        }
        else if (current == Symbols.DoubleQuote)
        {
          return UrlEnd(functionName);
        }
        else if (current != Symbols.ReverseSolidus)
        {
          StringBuffer.Append(current);
        }
        else
        {
          current = Advance();

          if (current == Symbols.EndOfFile)
          {
            Back(2);
            RaiseErrorOccurred(CssParseError.EOF);
            return NewUrl(functionName, FlushBuffer(), bad: true);
          }
          else if (current.IsLineBreak())
          {
            StringBuffer.AppendLine();
          }
          else
          {
            StringBuffer.Append(ConsumeEscape(current));
          }
        }
      }
    }

    /// <summary>
    /// 4.4.19. URL-single-quoted state
    /// </summary>
    CssToken UrlSQ(String functionName)
    {
      while (true)
      {
        var current = Advance();

        if (current.IsLineBreak())
        {
          RaiseErrorOccurred(CssParseError.LineBreakUnexpected);
          return UrlBad(functionName);
        }
        else if (current == Symbols.EndOfFile)
        {
          return NewUrl(functionName, FlushBuffer());
        }
        else if (current == Symbols.SingleQuote)
        {
          return UrlEnd(functionName);
        }
        else if (current != Symbols.ReverseSolidus)
        {
          StringBuffer.Append(current);
        }
        else
        {
          current = Advance();

          if (current == Symbols.EndOfFile)
          {
            Back(2);
            RaiseErrorOccurred(CssParseError.EOF);
            return NewUrl(functionName, FlushBuffer(), bad: true);
          }
          else if (current.IsLineBreak())
          {
            StringBuffer.AppendLine();
          }
          else
          {
            StringBuffer.Append(ConsumeEscape(current));
          }
        }
      }
    }

    /// <summary>
    /// 4.4.21. URL-unquoted state
    /// </summary>
    CssToken UrlUQ(Char current, String functionName)
    {
      while (true)
      {
        if (current.IsSpaceCharacter())
        {
          return UrlEnd(functionName);
        }
        else if (current.IsOneOf(Symbols.RoundBracketClose, Symbols.EndOfFile))
        {
          return NewUrl(functionName, FlushBuffer());
        }
        else if (current.IsOneOf(Symbols.DoubleQuote, Symbols.SingleQuote, Symbols.RoundBracketOpen) || current.IsNonPrintable())
        {
          RaiseErrorOccurred(CssParseError.InvalidCharacter);
          return UrlBad(functionName);
        }
        else if (current != Symbols.ReverseSolidus)
        {
          StringBuffer.Append(current);
        }
        else if (IsValidEscape(current))
        {
          current = Advance();
          StringBuffer.Append(ConsumeEscape(current));
        }
        else
        {
          RaiseErrorOccurred(CssParseError.InvalidCharacter);
          return UrlBad(functionName);
        }

        current = Advance();
      }
    }

    /// <summary>
    /// 4.4.20. URL-end state
    /// </summary>
    CssToken UrlEnd(String functionName)
    {
      while (true)
      {
        var current = Advance();

        if (current == Symbols.RoundBracketClose)
        {
          return NewUrl(functionName, FlushBuffer());
        }
        else if (!current.IsSpaceCharacter())
        {
          RaiseErrorOccurred(CssParseError.InvalidCharacter);
          Back();
          return UrlBad(functionName);
        }
      }
    }

    /// <summary>
    /// 4.4.22. Bad URL state
    /// </summary>
    CssToken UrlBad(String functionName)
    {
      var current = CurrentChar;
      var curly = 0;
      var round = 1;

      while (current != Symbols.EndOfFile)
      {
        if (current == Symbols.Semicolon)
        {
          Back();
          return NewUrl(functionName, FlushBuffer(), true);
        }
        else if (current == Symbols.CurlyBracketClose && --curly == -1)
        {
          Back();
          return NewUrl(functionName, FlushBuffer(), true);
        }
        else if (current == Symbols.RoundBracketClose && --round == 0)
        {
          return NewUrl(functionName, FlushBuffer(), true);
        }
        else if (IsValidEscape(current))
        {
          current = Advance();
          StringBuffer.Append(ConsumeEscape(current));
        }
        else
        {
          if (current == Symbols.RoundBracketOpen)
          {
            ++round;
          }
          else if (curly == Symbols.CurlyBracketOpen)
          {
            ++curly;
          }

          StringBuffer.Append(current);
        }

        current = Advance();
      }

      RaiseErrorOccurred(CssParseError.EOF);
      return NewUrl(functionName, FlushBuffer(), bad: true);
    }

    /// <summary>
    /// 4.4.23. Unicode-range State
    /// </summary>
    CssToken UnicodeRange(Char current)
    {
      for (var i = 0; i < 6 && current.IsHex(); i++)
      {
        StringBuffer.Append(current);
        current = Advance();
      }

      if (StringBuffer.Length != 6)
      {
        for (var i = 0; i < 6 - StringBuffer.Length; i++)
        {
          if (current != Symbols.QuestionMark)
          {
            current = Back();
            break;
          }

          StringBuffer.Append(current);
          current = Advance();
        }

        return NewRange(FlushBuffer());
      }
      else if (current == Symbols.Minus)
      {
        current = Advance();

        if (current.IsHex())
        {
          var start = FlushBuffer();

          for (var i = 0; i < 6; i++)
          {
            if (!current.IsHex())
            {
              current = Back();
              break;
            }

            StringBuffer.Append(current);
            current = Advance();
          }

          var end = FlushBuffer();
          return NewRange(start, end);
        }
        else
        {
          Back(2);
          return NewRange(FlushBuffer());
        }
      }
      else
      {
        Back();
        return NewRange(FlushBuffer());
      }
    }

    #endregion

    #region Tokens

    CssToken NewMatch(String match)
    {
      return new CssToken(CssTokenType.Match, match, _position);
    }

    CssToken NewColumn()
    {
      return new CssToken(CssTokenType.Column, CombinatorSymbols.Column, _position);
    }

    CssToken NewCloseCurly()
    {
      return new CssToken(CssTokenType.CurlyBracketClose, "}", _position);
    }

    CssToken NewOpenCurly()
    {
      return new CssToken(CssTokenType.CurlyBracketOpen, "{", _position);
    }

    CssToken NewCloseSquare()
    {
      return new CssToken(CssTokenType.SquareBracketClose, "]", _position);
    }

    CssToken NewOpenSquare()
    {
      return new CssToken(CssTokenType.SquareBracketOpen, "[", _position);
    }

    CssToken NewOpenComment()
    {
      return new CssToken(CssTokenType.Cdo, "<!--", _position);
    }

    CssToken NewSemicolon()
    {
      return new CssToken(CssTokenType.Semicolon, ";", _position);
    }

    CssToken NewColon()
    {
      return new CssToken(CssTokenType.Colon, ":", _position);
    }

    CssToken NewCloseComment()
    {
      return new CssToken(CssTokenType.Cdc, "-->", _position);
    }

    CssToken NewComma()
    {
      return new CssToken(CssTokenType.Comma, ",", _position);
    }

    CssToken NewCloseRound()
    {
      return new CssToken(CssTokenType.RoundBracketClose, ")", _position);
    }

    CssToken NewOpenRound()
    {
      return new CssToken(CssTokenType.RoundBracketOpen, "(", _position);
    }

    CssToken NewString(String value, Char quote, Boolean bad = false)
    {
      return new CssStringToken(value, bad, quote, _position);
    }

    CssToken NewHash(String value)
    {
      return new CssKeywordToken(CssTokenType.Hash, value, _position);
    }

    CssToken NewComment(String value, Boolean bad = false)
    {
      return new CssCommentToken(value, bad, _position);
    }

    CssToken NewAtKeyword(String value)
    {
      return new CssKeywordToken(CssTokenType.AtKeyword, value, _position);
    }

    CssToken NewIdent(String value)
    {
      return new CssKeywordToken(CssTokenType.Ident, value, _position);
    }

    CssToken NewFunction(String value)
    {
      var function = new CssFunctionToken(value, _position);
      var token = NextToken();

      while (token.Type != CssTokenType.EndOfFile)
      {
        function.AddArgumentToken(token);

        if (token.Type == CssTokenType.RoundBracketClose)
          break;

        token = NextToken();
      }

      return function;
    }

    CssToken NewPercentage(String value)
    {
      return new CssUnitToken(CssTokenType.Percentage, value, "%", _position);
    }

    CssToken NewDimension(String value, String unit)
    {
      return new CssUnitToken(CssTokenType.Dimension, value, unit, _position);
    }

    CssToken NewUrl(String functionName, String data, Boolean bad = false)
    {
      return new CssUrlToken(functionName, data, bad, _position);
    }

    CssToken NewRange(String range)
    {
      return new CssRangeToken(range, _position);
    }

    CssToken NewRange(String start, String end)
    {
      return new CssRangeToken(start, end, _position);
    }

    CssToken NewWhitespace(Char c)
    {
      return new CssToken(CssTokenType.Whitespace, c.ToString(), _position);
    }

    CssToken NewNumber(String number)
    {
      return new CssNumberToken(number, _position);
    }

    CssToken NewDelimiter(Char c)
    {
      return new CssToken(CssTokenType.Delim, c.ToString(), _position);
    }

    CssToken NewColor(String text)
    {
      return new CssColorToken(text, _position);
    }

    CssToken NewEof()
    {
      return new CssToken(CssTokenType.EndOfFile, String.Empty, _position);
    }

    #endregion

    #region Helpers

    CssToken NumberExponential(Char letter)
    {
      var current = Advance();

      if (current.IsDigit())
      {
        StringBuffer.Append(letter).Append(current);
        return SciNotation();
      }
      else if (current == Symbols.Plus || current == Symbols.Minus)
      {
        var op = current;
        current = Advance();

        if (current.IsDigit())
        {
          StringBuffer.Append(letter).Append(op).Append(current);
          return SciNotation();
        }

        Back();
      }

      var number = FlushBuffer();
      StringBuffer.Append(letter);
      Back();
      return Dimension(number);
    }

    CssToken NumberDash()
    {
      var current = Advance();

      if (current.IsNameStart())
      {
        var number = FlushBuffer();
        StringBuffer.Append(Symbols.Minus).Append(current);
        return Dimension(number);
      }
      else if (IsValidEscape(current))
      {
        current = Advance();
        var number = FlushBuffer();
        StringBuffer.Append(Symbols.Minus).Append(ConsumeEscape(current));
        return Dimension(number);
      }
      else
      {
        Back(2);
        return NewNumber(FlushBuffer());
      }
    }

    String ConsumeEscape(Char current)
    {
      if (current.IsHex())
      {
        var isHex = true;
        var escape = new Char[6];
        var length = 0;

        while (isHex && length < escape.Length)
        {
          escape[length++] = current;
          current = Advance();
          isHex = current.IsHex();
        }

        if (!current.IsSpaceCharacter())
        {
          Back();
        }

        var code = Int32.Parse(new String(escape, 0, length), NumberStyles.HexNumber);

        if (!code.IsInvalid())
        {
          return code.ConvertFromUtf32();
        }

        current = Symbols.Replacement;
      }

      return current.ToString();
    }

    Boolean IsValidEscape(Char current)
    {
      if (current == Symbols.ReverseSolidus)
      {
        current = Advance();
        Back();

        return current != Symbols.EndOfFile && !current.IsLineBreak();
      }

      return false;
    }

    void RaiseErrorOccurred(CssParseError code)
    {
      RaiseErrorOccurred(code, GetCurrentPosition());
    }

    bool IEnumerator.MoveNext()
    {
      return NextToken().Type != CssTokenType.EndOfFile;
    }

    void IEnumerator.Reset()
    {
      throw new NotSupportedException();
    }

    public IEnumerator<CssToken> GetEnumerator()
    {
      return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion
  }
}
