using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AngleParse
{
  public class HtmlTextWriter : XmlWriter
  {
    private TextWriter _writer;
    private HtmlWriterSettings _settings;

    internal static HashSet<string> VoidElements = new HashSet<string>(new string[]
    {
      "area", "base", "br", "col", "embed", "hr", "img", "input", "keygen", "link", "meta", "param", "source", "track", "wbr"
    }, StringComparer.OrdinalIgnoreCase);

    private int _line = 1;
    private Stack<TagInfo> _openTags = new Stack<TagInfo>();
    private List<NamespaceScope> _scopes = new List<NamespaceScope>();
    private InternalState _state = InternalState.Start;
    private string prefixForXmlNs = null;
    private StringBuilder attrValue;


    public HtmlTextWriter(TextWriter writer) : this(writer, new HtmlWriterSettings()) { }
    public HtmlTextWriter(TextWriter writer, HtmlWriterSettings settings)
    {
      _writer = writer;
      _settings = settings;
    }

    public override WriteState WriteState
    {
      get { return (WriteState)_state; }
    }

    public override void Flush()
    {
      CloseCurrElement();
      _writer.Flush();
    }
    public override Task FlushAsync()
    {
      CloseCurrElement();
      return _writer.FlushAsync();
    }

    public override string LookupPrefix(string ns)
    {
      for (var i = _scopes.Count - 1; i >= 0; i--)
      {
        if (_scopes[i] != null)
        {
          if (_scopes[i].Default == ns)
            return string.Empty;
          var nSpace = _scopes[i].FirstOrDefault(n => n.Ns == ns);
          if (!string.IsNullOrEmpty(nSpace.Prefix))
            return nSpace.Prefix;
        }
      }
      return null;
    }
    private string LookupNs(string prefix)
    {
      for (var i = _scopes.Count - 1; i >= 0; i--)
      {
        if (_scopes[i] != null)
        {
          if (string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(_scopes[i].Default))
            return _scopes[i].Default;
          var ns = _scopes[i].FirstOrDefault(n => n.Prefix == prefix);
          if (!string.IsNullOrEmpty(ns.Ns))
            return ns.Ns;
        }
      }
      return null;
    }

    public override void WriteBase64(byte[] buffer, int index, int count)
    {
      WriteString(Convert.ToBase64String(buffer, index, count));
    }

    public override void WriteCData(string text)
    {
      _writer.Write("<![CDATA[");
      _writer.Write(text);
      _writer.Write("]]>");
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
      WriteString(new string(buffer, index, count));
    }

    public override void WriteComment(string text)
    {
      CloseCurrElement();
      RenderIndent();
      _writer.Write("<!--");
      for (var i = 0; i < text.Length; i++)
      {
        switch (text[i])
        {
          case Symbols.LineFeed: _writer.Write(_settings.NewLineChars); break;
          default: _writer.Write(text[i]); break;
        }
      }
      _writer.Write("-->");
    }

    public override void WriteDocType(string name, string pubid, string sysid, string subset)
    {
      _writer.Write("<!DOCTYPE ");
      _writer.Write(name);
      if (!string.IsNullOrEmpty(pubid))
      {
        _writer.Write(" PUBLIC ");
        _writer.Write(_settings.QuoteChar);
        _writer.Write(pubid);
        _writer.Write(_settings.QuoteChar);
        _writer.Write(' ');
        _writer.Write(_settings.QuoteChar);
        _writer.Write(sysid);
        _writer.Write(_settings.QuoteChar);
      }
      else if (!string.IsNullOrEmpty(sysid))
      {
        _writer.Write(" SYSTEM ");
        _writer.Write(_settings.QuoteChar);
        _writer.Write(sysid);
        _writer.Write(_settings.QuoteChar);
      }
      if (!string.IsNullOrEmpty(subset))
      {
        _writer.Write("[");
        _writer.Write(subset);
        _writer.Write("]");
      }
      _writer.Write('>');
    }

    public override void WriteEndAttribute()
    {
      if (this.attrValue != null)
      {
        PushNamespace(this.prefixForXmlNs, this.attrValue.ToString());
        this.attrValue.ToPool();
      }
      if (_state == InternalState.Attribute)
        _writer.Write(_settings.QuoteChar);
      _state = InternalState.Element;
    }

    public override void WriteEndDocument()
    {
      // Do nothing
    }

    public override void WriteEndElement()
    {
      WriteEndElement(false);
    }
    public void WriteEndElement(string name)
    {
      var info = WriteEndElement(false);
      while (info.Name != name)
      {
        info = WriteEndElement(false);
      }
    }
    private TagInfo WriteEndElement(bool forceFull)
    {
      var tag = _openTags.Count > 0 ? _openTags.Pop() : new TagInfo();
      if (_state == InternalState.Element)
      {
        CloseCurrElement();
        if (!forceFull && VoidElements.Contains(tag.Name))
          return tag;
      }
      _scopes.RemoveAt(_scopes.Count - 1);

      if (_state != InternalState.Content)
        throw new InvalidOperationException();

      if (tag.Line != _line) RenderIndent();
      _writer.Write("</");
      _writer.Write(tag.Name);
      _writer.Write('>');
      return tag;
    }

    public override void WriteEntityRef(string name)
    {
      WriteInternal('&');
      WriteInternal(name);
      WriteInternal(';');
    }

    public override void WriteFullEndElement()
    {
      WriteEndElement(true);
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
      if (_state != InternalState.Element)
        throw new InvalidOperationException();
      if (_settings.NewLineOnAttributes) RenderIndent(_openTags.Count + 1);
      _writer.Write(' ');

      this.attrValue = null;
      this.prefixForXmlNs = null;
      if (prefix != null && prefix.Length == 0)
        prefix = null;

      if (ns == "http://www.w3.org/2000/xmlns/" && prefix == null && localName != "xmlns")
        prefix = "xmlns";

      if (prefix == "xmlns")
      {
        if ("http://www.w3.org/2000/xmlns/" != ns && ns != null)
          throw new ArgumentException("Reserved prefix");

        if (localName == null || localName.Length == 0)
        {
          localName = prefix;
          prefix = null;
          this.prefixForXmlNs = null;
        }
        else
        {
          this.prefixForXmlNs = localName;
          this.attrValue = Pool.NewStringBuilder();
        }
      }
      else if (prefix == null && localName == "xmlns")
      {
        if ("http://www.w3.org/2000/xmlns/" != ns && ns != null)
          throw new ArgumentException("Reserved prefix");
        this.prefixForXmlNs = null;
      }
      else if (ns == null)
      {
        if (prefix != null && string.IsNullOrEmpty(LookupNs(prefix)))
          throw new ArgumentException("Undefined prefix");
      }
      else if (ns.Length == 0)
      {
        prefix = string.Empty;
      }
      else
      {
        this.VerifyPrefixXml(prefix, ns);
        if (prefix != null && string.IsNullOrEmpty(LookupNs(prefix)))
          prefix = null;

        var text = this.LookupPrefix(ns);
        if (text != null && (prefix == null || prefix == text))
        {
          prefix = text;
        }
        else
        {
          if (prefix != null)
            this.PushNamespace(prefix, ns);
        }
      }

      if (prefix != null && prefix.Length != 0)
      {
        _writer.Write(prefix);
        _writer.Write(':');
      }

      _writer.Write(localName);
      _state = InternalState.AttributeStart;
    }

    private void PushNamespace(string prefix, string ns)
    {
      var curr = _scopes.Last();
      if (curr == null)
      {
        curr = new NamespaceScope();
        _scopes[_scopes.Count - 1] = curr;
      }

      if (string.IsNullOrEmpty(prefix))
      {
        curr.Default = ns;
        curr.DefaultWritten = true;
      }
      else
      {
        var match = curr.FirstOrDefault(n => n.Prefix == prefix);
        if (!string.IsNullOrEmpty(match.Ns))
          curr.Remove(match);
        curr.Add(new Namespace() { IsWritten = true, Ns = ns, Prefix = prefix });
      }
    }

    private void VerifyPrefixXml(string prefix, string ns)
    {
      if (prefix != null && prefix.Length == 3
        && (prefix[0] == 'x' || prefix[0] == 'X')
        && (prefix[1] == 'm' || prefix[1] == 'M')
        && (prefix[2] == 'l' || prefix[2] == 'L')
        && "http://www.w3.org/XML/1998/namespace" != ns)
      {
        throw new ArgumentException("Invalid prefix");
      }
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
      CloseCurrElement();
      RenderIndent();

      _writer.Write('<');
      var name = localName;
      if (string.IsNullOrEmpty(prefix) && string.IsNullOrEmpty(ns))
      {
        _scopes.Add(null);
      }
      else if (string.IsNullOrEmpty(prefix))
      {
        prefix = LookupPrefix(ns);
        if (prefix == null)
        {
          _scopes.Add(new NamespaceScope() { Default = ns });
        }
        else
        {
          if (prefix != string.Empty)
            name = prefix + ":" + name;
          _scopes.Add(null);
        }
      }
      else if (string.IsNullOrEmpty(ns))
      {
        ns = LookupNs(prefix);
        if (string.IsNullOrEmpty(ns))
        {
          throw new InvalidOperationException("Namespace cannot be found for the prefix '" + prefix + "'.");
        }
        else
        {
          name = prefix + ":" + name;
          _scopes.Add(null);
        }
      }
      else
      {
        name = prefix + ":" + name;
        _scopes.Add(new NamespaceScope() { new Namespace() { Ns = ns, Prefix = prefix } });
      }

      _writer.Write(name);
      _openTags.Push(new TagInfo(name, _line));

      _state = InternalState.Element;
    }

    public override void WriteString(string text)
    {
      text = text ?? "";
      if (_state == InternalState.AttributeStart)
      {
        if (text.Length <= 0)
          return;
        _writer.Write("=");
        _writer.Write(_settings.QuoteChar);
        _state = InternalState.Attribute;
      }
      if (_state == InternalState.Attribute)
      {
        for (var i = 0; i < text.Length; i++)
        {
          switch (text[i])
          {
            case Symbols.Ampersand: WriteInternal("&amp;"); break;
            case Symbols.NoBreakSpace: WriteInternal("&nbsp;"); break;
            case Symbols.DoubleQuote:
              if (_settings.QuoteChar == Symbols.DoubleQuote)
                WriteInternal("&quot;");
              break;
            case Symbols.SingleQuote:
              if (_settings.QuoteChar == Symbols.SingleQuote)
                WriteInternal("&apos;");
              break;
            case Symbols.LineFeed: WriteInternal(_settings.NewLineChars); break;
            default: WriteInternal(text[i]); break;
          }
        }
        return;
      }

      CloseCurrElement();
      if ((_state == InternalState.Content || _state == InternalState.Start)
        && !(_openTags.Count > 0 && string.Equals(_openTags.Peek().Name, "script", StringComparison.OrdinalIgnoreCase)))
      {
        for (var i = 0; i < text.Length; i++)
        {
          switch (text[i])
          {
            case Symbols.Ampersand: _writer.Write("&amp;"); break;
            case Symbols.NoBreakSpace: _writer.Write("&nbsp;"); break;
            case Symbols.GreaterThan: _writer.Write("&gt;"); break;
            case Symbols.LessThan: _writer.Write("&lt;"); break;
            case Symbols.LineFeed: _writer.Write(_settings.NewLineChars); break;
            default: _writer.Write(text[i]); break;
          }
        }
      }
      else
      {
        _writer.Write(text);
      }
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
      WriteString(ws);
    }

    private void CloseCurrElement()
    {
      if (_state == InternalState.Element)
      {
        var last = _scopes.Last();
        if (last != null)
        {
          foreach (var ns in last.Where(n => !n.IsWritten).ToArray())
          {
            _writer.Write(" xmlns:");
            _writer.Write(ns.Prefix);
            _writer.Write("=");
            _writer.Write(_settings.QuoteChar);
            _state = InternalState.Attribute;
            WriteString(ns.Ns);
            _writer.Write(_settings.QuoteChar);
          }

          if (!last.DefaultWritten && !string.IsNullOrEmpty(last.Default))
          {
            _writer.Write(" xmlns=");
            _writer.Write(_settings.QuoteChar);
            _state = InternalState.Attribute;
            WriteString(last.Default);
            _writer.Write(_settings.QuoteChar);
          }
        }

        _writer.Write('>');
        _state = InternalState.Content;
      }
    }

    private async Task CloseCurrElementAsync()
    {
      if (_state == InternalState.Element)
      {
        var last = _scopes.Last();
        if (last != null)
        {
          foreach (var ns in last.Where(n => !n.IsWritten).ToArray())
          {
            await _writer.WriteAsync(" xmlns:");
            await _writer.WriteAsync(ns.Prefix);
            await _writer.WriteAsync("=");
            await _writer.WriteAsync(_settings.QuoteChar);
            _state = InternalState.Attribute;
            await WriteStringAsync(ns.Ns);
            await _writer.WriteAsync(_settings.QuoteChar);
          }

          if (!last.DefaultWritten && !string.IsNullOrEmpty(last.Default))
          {
            await _writer.WriteAsync(" xmlns=");
            await _writer.WriteAsync(_settings.QuoteChar);
            _state = InternalState.Attribute;
            await WriteStringAsync(last.Default);
            await _writer.WriteAsync(_settings.QuoteChar);
          }
        }

        await _writer.WriteAsync('>');
        _state = InternalState.Content;
      }
    }

    private void WriteInternal(char value)
    {
      if (this.attrValue != null)
        this.attrValue.Append(value);
      _writer.Write(value);
    }
    private void WriteInternal(string value)
    {
      if (this.attrValue != null)
        this.attrValue.Append(value);
      _writer.Write(value);
    }
    private Task WriteInternalAsync(char value)
    {
      if (this.attrValue != null)
        this.attrValue.Append(value);
      return _writer.WriteAsync(value);
    }
    private Task WriteInternalAsync(string value)
    {
      if (this.attrValue != null)
        this.attrValue.Append(value);
      return _writer.WriteAsync(value);
    }

    private void RenderIndent()
    {
      RenderIndent(_openTags.Count);
    }
    private void RenderIndent(int indent)
    {
      if (_settings.Indent)
      {
        if (_state != InternalState.Start)
        {
          _writer.Write(_settings.NewLineChars);
          _line += 1;
        }
        for (var i = 0; i < indent; i++)
        {
          _writer.Write(_settings.IndentChars);
        }
      }
    }

    private class NamespaceScope : List<Namespace>
    {
      public string Default { get; set; }
      public bool DefaultWritten { get; set; }
    }

    private struct Namespace
    {
      public bool IsWritten { get; set; }
      public string Prefix { get; set; }
      public string Ns { get; set; }
    }

    private struct TagInfo
    {
      public string Name { get; set; }
      public int Line { get; set; }

      public TagInfo(string name, int line)
      {
        this.Name = name;
        this.Line = line;
      }
    }

    private static class XmlCharType
    {
      internal static int CombineSurrogateChar(int lowChar, int highChar)
      {
        return lowChar - 56320 | (highChar - 55296 << 10) + 65536;
      }

      internal static bool IsHighSurrogate(int ch)
      {
        return XmlCharType.InRange(ch, 55296, 56319);
      }

      internal static bool IsLowSurrogate(int ch)
      {
        return XmlCharType.InRange(ch, 56320, 57343);
      }

      internal static bool IsSurrogate(int ch)
      {
        return XmlCharType.InRange(ch, 55296, 57343);
      }

      private static bool InRange(int value, int start, int end)
      {
        return value - start <= end - start;
      }
    }

    private enum InternalState
    {
      Start = 0,
      Element = 2,
      Attribute = 3,
      Content = 4,
      AttributeStart = 7,
    }
  }
}
