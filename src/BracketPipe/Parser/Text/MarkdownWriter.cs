using BracketPipe.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace BracketPipe
{
  public class MarkdownWriter : XmlWriter
  {
    private TextWriter _writer;
    private Stack<HtmlStartTag> _nodes = new Stack<HtmlStartTag>();
    private int _ignoreDepth = int.MaxValue;
    private int _boldDepth = int.MaxValue;
    private int _italicDepth = int.MaxValue;
    private StringBuilder _buffer;
    private InternalState _state = InternalState.Start;
    private MinifyState _minify = MinifyState.LastCharWasSpace;
    private MarkdownWriterSettings _settings;
    private List<string> _linePrefix = new List<string>();
    private PreserveState _preserveWhitespace = PreserveState.None;
    private bool _outputStarted;

    public MarkdownWriter(TextWriter writer) : this(writer, new MarkdownWriterSettings()) { }
    public MarkdownWriter(TextWriter writer, MarkdownWriterSettings settings)
    {
      _writer = writer;
      _settings = settings ?? new MarkdownWriterSettings();
    }

    public override WriteState WriteState
    {
      get
      {
        return (WriteState)_state;
      }
    }

    public override void Flush()
    {
      _writer.Flush();
    }

    public override string LookupPrefix(string ns)
    {
      throw new NotImplementedException();
    }

    public override void WriteBase64(byte[] buffer, int index, int count)
    {
      WriteInternal(Convert.ToBase64String(buffer, index, count));
    }

    public override void WriteCData(string text)
    {
      WriteInternal(text);
    }

    public override void WriteCharEntity(char ch)
    {
      if (XmlCharType.IsSurrogate((int)ch))
        throw new ArgumentException("Invalid surrogate: Missing low character");

      var num = (int)ch;
      var text = num.ToString("X", NumberFormatInfo.InvariantInfo);
      WriteEntityRef("#x" + text);
    }

    public override void WriteChars(char[] buffer, int index, int count)
    {
      WriteInternal(new string(buffer, index, count));
    }

    public override void WriteComment(string text)
    {
      // Do nothing
    }

    public override void WriteDocType(string name, string pubid, string sysid, string subset)
    {
      // Do nothing
    }

    public override void WriteEndAttribute()
    {
      if (_nodes.Count < 1)
        throw new InvalidOperationException();
      _nodes.Peek().SetAttributeValue(this._buffer.ToPool());
      this._buffer = null;
      _state = InternalState.Element;
    }

    public override void WriteEndDocument()
    {
      // Do nothing
    }

    public override void WriteEndElement()
    {
      WriteStartElementEnd();
      var start = _nodes.Pop();
      if (_ignoreDepth > _nodes.Count)
        _ignoreDepth = int.MaxValue;

      string buffer;
      switch (start.Value)
      {
        case "a":
          if (start.TryGetValue("href", out buffer))
          {
            _writer.Write(']');
            _writer.Write('(');
            _writer.Write(buffer);
            if (start.TryGetValue("title", out buffer))
            {
              _writer.Write(' ');
              _writer.Write('"');
              _writer.Write(buffer);
              _writer.Write('"');
            }
            _writer.Write(')');
          }
          else if (start.Attributes.Count > 0)
          {
            _writer.Write("</a>");
            _preserveWhitespace = PreserveState.None;
          }
          _minify = MinifyState.Compressed;
          break;
        case "b":
        case "strong":
          if (_boldDepth > _nodes.Count)
          {
            _writer.Write("**");
            _boldDepth = int.MaxValue;
          }
          break;
        case "code":
          if (_preserveWhitespace == PreserveState.None)
            _writer.Write('`');
          break;
        case "em":
        case "i":
          if (_italicDepth > _nodes.Count)
          {
            _writer.Write("*");
            _italicDepth = int.MaxValue;
          }
          break;
        case "blockquote":
        case "h1":
        case "h2":
        case "h3":
        case "h4":
        case "h5":
        case "h6":
        case "ol":
        case "p":
        case "ul":
          EndBlock();
          break;
        case "pre":
          EndBlock();
          _preserveWhitespace = PreserveState.None;
          break;
      }
    }

    public override void WriteEntityRef(string name)
    {
      WriteInternal("&");
      WriteInternal(name);
      WriteInternal(";");
    }

    public override void WriteFullEndElement()
    {
      WriteEndElement();
    }

    public override void WriteProcessingInstruction(string name, string text)
    {
      throw new NotSupportedException();
    }

    public override void WriteRaw(string data)
    {
      _writer.Write(data);
    }

    public override void WriteRaw(char[] buffer, int index, int count)
    {
      _writer.Write(buffer, index, count);
    }

    public override void WriteStartAttribute(string prefix, string localName, string ns)
    {
      if (_nodes.Count < 1)
        throw new InvalidOperationException();
      _nodes.Peek().AddAttribute(localName);
      this._buffer = Pool.NewStringBuilder();
      _state = InternalState.Attribute;
    }

    public override void WriteStartDocument()
    {
      // Do nothing
    }

    public override void WriteStartDocument(bool standalone)
    {
      // Do nothing
    }

    public override void WriteStartElement(string prefix, string localName, string ns)
    {
      var start = new HtmlStartTag(localName.ToLowerInvariant());
      _nodes.Push(start);
      switch (start.Value)
      {
        case "b":
        case "strong":
          StartInline();
          if (_boldDepth > _nodes.Count)
          {
            _boldDepth = _nodes.Count;
            _writer.Write("**");
          }
          break;
        case "blockquote":
          StartBlock("> ");
          break;
        case "br":
          _minify = MinifyState.LastCharWasSpace;
          switch (_preserveWhitespace)
          {
            case PreserveState.BeforeContent:
              _preserveWhitespace = PreserveState.Preserve;
              break;
            case PreserveState.InternalLineFeed:
              _writer.Write(_settings.NewLineChars);
              WritePrefix();
              _preserveWhitespace = PreserveState.InternalLineFeed;
              break;
            case PreserveState.None:
              _writer.Write('\\');
              _writer.Write(_settings.NewLineChars);
              WritePrefix();
              break;
            default:
              _preserveWhitespace = PreserveState.InternalLineFeed;
              break;
          }
          break;
        case "code":
          if (_preserveWhitespace == PreserveState.None)
          {
            StartInline();
            _writer.Write('`');
          }
          break;
        case "em":
        case "i":
          StartInline();
          if (_italicDepth > _nodes.Count)
          {
            _italicDepth = _nodes.Count;
            _writer.Write("*");
          }
          break;
        case "h1":
          StartBlock("# ");
          break;
        case "h2":
          StartBlock("## ");
          break;
        case "h3":
          StartBlock("### ");
          break;
        case "h4":
          StartBlock("#### ");
          break;
        case "h5":
          StartBlock("##### ");
          break;
        case "h6":
          StartBlock("###### ");
          break;
        case "title":
        case "script":
        case "style":
          _ignoreDepth = _nodes.Count;
          break;
        case "hr":
          StartBlock("* * *");
          EndBlock();
          _writer.Write(_settings.NewLineChars);
          _minify = MinifyState.LastCharWasSpace;
          break;
        case "li":
          if (_outputStarted)
            _writer.Write(_settings.NewLineChars);
          if (_linePrefix.Count > 0 
            && char.IsDigit(_linePrefix[_linePrefix.Count - 1][0]))
          {
            var value = _linePrefix[_linePrefix.Count - 1];
            value = string.Format("{0}. ", int.Parse(value.Substring(0, value.Length - 2)) + 1);
            _linePrefix[_linePrefix.Count - 1] = value;
          }
          WritePrefix();
          _minify = MinifyState.LastCharWasSpace;
          break;
        case "p":
          StartBlock("");
          break;
        case "pre":
          StartBlock("    ");
          _preserveWhitespace = PreserveState.BeforeContent;
          break;
        case "ol":
          StartList("0. ");
          break;
        case "ul":
          StartList("- ");
          break;
        
      }
      _state = InternalState.Element;
    }

    private void WritePrefix()
    {
      for (var i = 0; i < _linePrefix.Count; i++)
      {
        // Only indent on nested lists
        if (i < _linePrefix.Count - 1
          && (_linePrefix[i][0] == '-' || char.IsDigit(_linePrefix[i][0])))
          _writer.Write(new string(' ', _linePrefix[i].Length));
        else
          _writer.Write(_linePrefix[i]);
      }
    }

    public override void WriteString(string text)
    {
      WriteInternal(text);
    }

    public override void WriteSurrogateCharEntity(char lowChar, char highChar)
    {
      if (!XmlCharType.IsLowSurrogate((int)lowChar) || !XmlCharType.IsHighSurrogate((int)highChar))
        throw new InvalidOperationException("Invalid surrogate pair");

      var num = XmlCharType.CombineSurrogateChar((int)lowChar, (int)highChar);
      WriteInternal("&#x");
      WriteInternal(num.ToString("X", NumberFormatInfo.InvariantInfo));
      WriteInternal(";");
    }

    public override void WriteWhitespace(string ws)
    {
      WriteInternal(ws);
    }

    private void WriteInternal(string value)
    {
      WriteStartElementEnd();

      if (_ignoreDepth <= _nodes.Count)
        return;

      if (_preserveWhitespace == PreserveState.HtmlEncoding)
      {
        WriteEscaped(value);
        _outputStarted = true;
        return;
      }

      if (this._buffer != null)
      {
        this._buffer.Append(value);
      }
      else
      {
        for (var i = 0; i < value.Length; i++)
        {
          if (_preserveWhitespace == PreserveState.InternalLineFeed)
          {
            _writer.Write(_settings.NewLineChars);
            WritePrefix();
            _preserveWhitespace = PreserveState.Preserve;
          }

          if (char.IsWhiteSpace(value[i]))
          {
            if (_preserveWhitespace == PreserveState.BeforeContent
              && value[i] == Symbols.LineFeed)
            {
              _preserveWhitespace = PreserveState.Preserve;
            }
            else if (_preserveWhitespace != PreserveState.None)
            {
              if (value[i] == Symbols.LineFeed)
              {
                _preserveWhitespace = PreserveState.InternalLineFeed;
              }
              else
              {
                _writer.Write(value[i]);
              }
            }
            else
            {
              if (_minify != MinifyState.LastCharWasSpace
                && _minify != MinifyState.BlockEnd)
                _minify = MinifyState.SpaceNeeded;
            }
          }
          else
          {
            switch (_minify)
            {
              case MinifyState.BlockEnd:
                _writer.Write(_settings.NewLineChars);
                _writer.Write(_settings.NewLineChars);
                _minify = MinifyState.Compressed;
                break;
              case MinifyState.SpaceNeeded:
                _writer.Write(' ');
                _minify = MinifyState.Compressed;
                break;
              case MinifyState.LastCharWasSpace:
                _minify = MinifyState.Compressed;
                break;
            }

            if (_preserveWhitespace == PreserveState.None)
            {
              switch (value[i])
              {
                case '\\':
                case '*':
                case '[':
                case '`':
                  _writer.Write('\\');
                  break;
              }
            }
            else if (_preserveWhitespace == PreserveState.BeforeContent)
            {
              _preserveWhitespace = PreserveState.Preserve;
            }
            _writer.Write(value[i]);
          }
        }
        _outputStarted = true;
      }
    }
    private void WriteStartElementEnd()
    {
      if (_nodes.Count > 0 && _state == InternalState.Element)
      {
        var start = _nodes.Peek();
        string buffer;
        switch (start.Value)
        {
          case "a":
            buffer = null;
            StartInline();
            if (start.Attributes.Count > 0
              && (!start.TryGetValue("href", out buffer)
                || string.IsNullOrEmpty(buffer)))
            {
              _writer.Write("<a ");
              foreach (var attr in start)
              {
                _writer.Write(attr.Key);
                if (!string.IsNullOrEmpty(attr.Value))
                {
                  _writer.Write('=');
                  _writer.Write('"');
                  WriteEscaped(attr.Value);
                  _writer.Write('"');
                }
              }
              _writer.Write('>');
              _preserveWhitespace = PreserveState.HtmlEncoding;
            }
            else if (!string.IsNullOrEmpty(buffer))
            {
              _writer.Write('[');
            }
            break;
          case "img":
            StartInline();
            _writer.Write('!');
            _writer.Write('[');
            _writer.Write(start["alt"]);
            _writer.Write(']');
            _writer.Write('(');
            _writer.Write(start["src"]);
            if (start.TryGetValue("title", out buffer))
            {
              _writer.Write(' ');
              _writer.Write('"');
              _writer.Write(buffer);
              _writer.Write('"');
            }
            _writer.Write(')');
            _minify = MinifyState.Compressed;
            break;
        }
        _state = InternalState.Content;
      }
    }

    private void StartInline()
    {
      if (_minify == MinifyState.SpaceNeeded)
      {
        _writer.Write(' ');
        _minify = MinifyState.LastCharWasSpace;
      }
    }
    private void StartList(string prefix)
    {
      if (_minify == MinifyState.Compressed
        || _minify == MinifyState.SpaceNeeded
        || _minify == MinifyState.BlockEnd)
      {
        _writer.Write(_settings.NewLineChars);
        _minify = MinifyState.LastCharWasSpace;
      }
      _linePrefix.Add(prefix ?? string.Empty);
    }
    private void StartBlock(string prefix)
    {
      var prefixRequired = false;
      if (_minify == MinifyState.Compressed
        || _minify == MinifyState.SpaceNeeded
        || _minify == MinifyState.BlockEnd)
      {
        _writer.Write(_settings.NewLineChars);
        _writer.Write(_settings.NewLineChars);
        _minify = MinifyState.LastCharWasSpace;
        prefixRequired = true;
      }
      _linePrefix.Add(prefix ?? string.Empty);
      if (prefixRequired || prefix != string.Empty)
        WritePrefix();
      _outputStarted = true;
    }
    private void EndBlock()
    {
      _minify = MinifyState.BlockEnd;
      if (_linePrefix.Count > 0)
        _linePrefix.RemoveAt(_linePrefix.Count - 1);
    }

    private void WriteEscaped(string text)
    {
      for (var i = 0; i < text.Length; i++)
      {
        switch (text[i])
        {
          case Symbols.Ampersand: _writer.Write("&amp;"); break;
          case Symbols.NoBreakSpace: _writer.Write("&nbsp;"); break;
          case Symbols.GreaterThan: _writer.Write("&gt;"); break;
          case Symbols.LessThan: _writer.Write("&lt;"); break;
          case Symbols.DoubleQuote:
            if (_settings.QuoteChar == Symbols.DoubleQuote)
              _writer.Write("&quot;");
            else
              _writer.Write('"');
            break;
          case Symbols.SingleQuote:
            if (_settings.QuoteChar == Symbols.SingleQuote)
              _writer.Write("&apos;");
            else
              _writer.Write("'");
            break;
          case Symbols.LineFeed: _writer.Write(_settings.NewLineChars); break;
          default: _writer.Write(text[i]); break;
        }
      }
    }


#if !PORTABLE
    public override void Close()
    {
      
    }
#endif

    private enum InternalState
    {
      Start = 0,
      Element = 2,
      Attribute = 3,
      Content = 4,
    }

    private enum PreserveState : byte
    {
      None,
      HtmlEncoding,
      BeforeContent,
      Preserve,
      InternalLineFeed
    }

    private enum MinifyState : byte
    {
      Compressed,
      LastCharWasSpace,
      SpaceNeeded,
      InlineStartAfterSpace,
      BlockEnd
    }
  }
}
