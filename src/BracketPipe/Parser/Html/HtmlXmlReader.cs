using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace BracketPipe
{
  /// <summary>
  /// An XML reader wrapper of a list of HTML nodes.
  /// </summary>
  /// <seealso cref="System.Xml.XmlReader" />
  public class HtmlXmlReader : XmlReader
  {
    private readonly IEnumerator<HtmlNode> _enumerator;

    private int _attrIndex;
    private int _depth;
    private ReadState _state = ReadState.Initial;

    /// <summary>
    /// Gets the number of attributes on the current node.
    /// </summary>
    public override int AttributeCount
    {
      get
      {
        var tag = Current as HtmlStartTag;
        if (tag == null)
          return 0;
        return tag.Attributes.Count;
      }
    }

    /// <summary>
    /// Gets the base URI of the current node.
    /// </summary>
    public override string BaseURI
    {
      get
      {
        return string.Empty;
      }
    }

    /// <summary>
    /// Gets the depth of the current node in the XML document.
    /// </summary>
    public override int Depth
    {
      get
      {
        return 0;
      }
    }

    /// <summary>
    /// Gets a value indicating whether the reader is positioned at the end of the stream.
    /// </summary>
    public override bool EOF { get { return _state == ReadState.EndOfFile; } }

    /// <summary>
    /// Gets a value indicating whether the current node is an empty element (for example, &lt;MyElement/&gt;).
    /// </summary>
    public override bool IsEmptyElement
    {
      get
      {
        var tag = Current as HtmlStartTag;
        if (tag == null)
          return false;
        return tag.IsEmpty;
      }
    }

    /// <summary>
    /// Gets the local name of the current node.
    /// </summary>
    public override string LocalName { get { return GetName().Last(); } }

    private readonly static string[] _emptyStringArray = new string[] { "" };

    private string[] GetName()
    {
      if (_attrIndex >= 0)
        return ((HtmlStartTag)Current).Attributes[_attrIndex].Key.Split(':');
      if (_attrIndex < -1)
        return _emptyStringArray;
      if (Current == null)
        return _emptyStringArray;
      if (Current.Type == HtmlTokenType.EndTag
        || Current.Type == HtmlTokenType.StartTag
        || Current.Type == HtmlTokenType.Doctype)
        return Current.Value.Split(':');
      return _emptyStringArray;
    }

    /// <summary>
    /// Gets the namespace URI (as defined in the W3C Namespace specification) of the node on which the reader is positioned.
    /// </summary>
    public override string NamespaceURI
    {
      get
      {
        return string.Empty;
      }
    }

    private readonly XmlNameTable _table = new NameTable();

    /// <summary>
    /// Gets the <see cref="T:System.Xml.XmlNameTable" /> associated with this implementation.
    /// </summary>
    public override XmlNameTable NameTable
    {
      get { return _table; }
    }

    /// <summary>
    /// Gets the type of the current node.
    /// </summary>
    public override XmlNodeType NodeType
    {
      get
      {
        if (Current == null)
          return XmlNodeType.None;
        if (_attrIndex >= 0)
          return XmlNodeType.Attribute;
        if (_attrIndex < -1)
          return XmlNodeType.Text;
        switch (Current.Type)
        {
          case HtmlTokenType.Comment:
            return XmlNodeType.Comment;
          case HtmlTokenType.Doctype:
            return XmlNodeType.DocumentType;
          case HtmlTokenType.EndTag:
            return XmlNodeType.EndElement;
          case HtmlTokenType.StartTag:
            return XmlNodeType.Element;
          case HtmlTokenType.Text:
            var val = Current.Value ?? "";
            for (var i = 0; i < val.Length; i++)
            {
              if (!char.IsWhiteSpace(val[i]))
                return XmlNodeType.Text;
            }
            return _depth > 0 ? XmlNodeType.SignificantWhitespace : XmlNodeType.Whitespace;
        }
        return XmlNodeType.None;
      }
    }

    /// <summary>
    /// Gets the namespace prefix associated with the current node.
    /// </summary>
    public override string Prefix
    {
      get
      {
        //var name = GetName();
        //if (name.Length > 1)
        //  return name[0];
        return string.Empty;
      }
    }

    /// <summary>
    /// Gets the state of the reader.
    /// </summary>
    public override ReadState ReadState { get { return _state; } }

    private HtmlNode Current { get { return _enumerator.Current; } }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlXmlReader"/> class.
    /// </summary>
    protected HtmlXmlReader()
    {
      _enumerator = this as IEnumerator<HtmlNode>;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlXmlReader"/> class.
    /// </summary>
    /// <param name="enumerator">The enumerator.</param>
    public HtmlXmlReader(IEnumerator<HtmlNode> enumerator)
    {
      _enumerator = enumerator;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlXmlReader"/> class.
    /// </summary>
    /// <param name="enumerable">The enumerable.</param>
    public HtmlXmlReader(IEnumerable<HtmlNode> enumerable)
    {
      _enumerator = enumerable.GetEnumerator();
    }

    /// <summary>
    /// Gets the text value of the current node.
    /// </summary>
    public override string Value
    {
      get
      {
        if (_attrIndex >= 0)
          return ((HtmlStartTag)Current).Attributes[_attrIndex].Value;
        if (_attrIndex < -1)
          return ((HtmlStartTag)Current).Attributes[_attrIndex * -1 - 2].Value;
        if (Current == null)
          return string.Empty;
        switch (Current.Type)
        {
          case HtmlTokenType.Comment:
          case HtmlTokenType.Text:
            return Current.Value;
        }
        return string.Empty;
      }
    }

    /// <summary>
    /// Gets the value of the attribute with the specified index.
    /// </summary>
    /// <param name="i">The index of the attribute. The index is zero-based. (The first attribute has index 0.)</param>
    /// <returns>
    /// The value of the specified attribute. This method does not move the reader.
    /// </returns>
    public override string GetAttribute(int i)
    {
      var tag = Current as HtmlStartTag;
      if (tag == null)
        return string.Empty;
      return tag.Attributes[i].Value;
    }

    /// <summary>
    /// Gets the value of the attribute with the specified <see cref="P:System.Xml.XmlReader.Name" />.
    /// </summary>
    /// <param name="name">The qualified name of the attribute.</param>
    /// <returns>
    /// The value of the specified attribute. If the attribute is not found or the value is String.Empty, null is returned.
    /// </returns>
    public override string GetAttribute(string name)
    {
      return GetAttribute(name, null);
    }

    /// <summary>
    /// Gets the value of the attribute with the specified <see cref="P:System.Xml.XmlReader.LocalName" /> and <see cref="P:System.Xml.XmlReader.NamespaceURI" />.
    /// </summary>
    /// <param name="name">The local name of the attribute.</param>
    /// <param name="namespaceURI">The namespace URI of the attribute.</param>
    /// <returns>
    /// The value of the specified attribute. If the attribute is not found or the value is String.Empty, null is returned. This method does not move the reader.
    /// </returns>
    public override string GetAttribute(string name, string namespaceURI)
    {
      var tag = Current as HtmlStartTag;
      if (tag == null)
        return string.Empty;
      return tag[name];
    }

    /// <summary>
    /// Resolves a namespace prefix in the current element's scope.
    /// </summary>
    /// <param name="prefix">The prefix whose namespace URI you want to resolve. To match the default namespace, pass an empty string.</param>
    /// <returns>
    /// The namespace URI to which the prefix maps or null if no matching prefix is found.
    /// </returns>
    public override string LookupNamespace(string prefix)
    {
      return string.Empty;
    }

    /// <summary>
    /// Moves to the attribute with the specified <see cref="P:System.Xml.XmlReader.Name" />.
    /// </summary>
    /// <param name="name">The qualified name of the attribute.</param>
    /// <returns>
    /// <c>true</c> if the attribute is found; otherwise, <c>false</c>. If <c>false</c>, the reader's position does not change.
    /// </returns>
    public override bool MoveToAttribute(string name)
    {
      return MoveToAttribute(name, null);
    }

    /// <summary>
    /// When overridden in a derived class, moves to the attribute with the specified <see cref="P:System.Xml.XmlReader.LocalName" /> and <see cref="P:System.Xml.XmlReader.NamespaceURI" />.
    /// </summary>
    /// <param name="name">The local name of the attribute.</param>
    /// <param name="ns">The namespace URI of the attribute.</param>
    /// <returns>
    /// <c>true</c> if the attribute is found; otherwise, <c>false</c>. If <c>false</c>, the reader's position does not change.
    /// </returns>
    public override bool MoveToAttribute(string name, string ns)
    {
      var tag = Current as HtmlStartTag;
      if (tag != null)
      {
        for (var i = 0; i < tag.Attributes.Count; i++)
        {
          if (string.Equals(tag.Attributes[i].Key, name, StringComparison.OrdinalIgnoreCase))
          {
            _attrIndex = i;
            return true;
          }
        }
      }
      _attrIndex = -1;
      return false;
    }

    /// <summary>
    /// Moves to the element that contains the current attribute node.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the reader is positioned on an attribute (the reader moves to the element that owns the attribute); <c>false</c> if the reader is not positioned on an attribute (the position of the reader does not change).
    /// </returns>
    public override bool MoveToElement()
    {
      _attrIndex = -1;
      return Current != null && Current.Type == HtmlTokenType.StartTag;
    }

    /// <summary>
    /// Moves to the first attribute.
    /// </summary>
    /// <returns>
    /// <c>true</c> if an attribute exists (the reader moves to the first attribute); otherwise, <c>false</c> (the position of the reader does not change).
    /// </returns>
    public override bool MoveToFirstAttribute()
    {
      var tag = Current as HtmlStartTag;
      if (tag == null || tag.Attributes.Count < 1)
      {
        _attrIndex = -1;
        return false;
      }
      _attrIndex = 0;
      return true;
    }

    /// <summary>
    /// Moves to the next attribute.
    /// </summary>
    /// <returns>
    /// <c>true</c> if there is a next attribute; <c>false</c> if there are no more attributes.
    /// </returns>
    public override bool MoveToNextAttribute()
    {
      var tag = Current as HtmlStartTag;
      if (tag == null || _attrIndex + 1 >= tag.Attributes.Count)
      {
        _attrIndex = -1;
        return false;
      }
      _attrIndex++;
      return true;
    }

    /// <summary>
    /// Parses the attribute value into one or more <see cref="XmlNodeType.Text"/>, <see cref="XmlNodeType.EntityReference"/>, or <see cref="XmlNodeType.EndEntity"/> nodes.
    /// </summary>
    /// <returns>
    /// <c>true</c> if there are nodes to return. <c>false</c> if the reader is not positioned on an attribute node when the initial call is made or if all the attribute values have been read. An empty attribute, such as, <c>misc=""</c>, returns <c>true</c> with a single node with a value of <see cref="String.Empty"/>.
    /// </returns>
    public override bool ReadAttributeValue()
    {
      if (_attrIndex < -1)
      {
        _attrIndex = _attrIndex * -1 - 2;
        return false;
      }
      else
      {
        _attrIndex = _attrIndex * -1 - 2;
        return true;
      }
    }

    /// <summary>
    /// Resolves the entity reference for <see cref="XmlNodeType.EntityReference"/> nodes.
    /// </summary>
    public override void ResolveEntity()
    {
      // Do nothing
    }

    /// <summary>
    /// Reads the next node from the stream.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the next node was read successfully; otherwise, <c>false</c>.
    /// </returns>
    public override bool Read()
    {
      var result = _enumerator.MoveNext();
      OnAfterRead(result);
      return result;
    }

    /// <summary>
    /// Tracking code called after each read operation.
    /// </summary>
    protected virtual void OnAfterRead(bool success)
    {
      _attrIndex = -1;
      _state = success ? ReadState.Interactive : ReadState.EndOfFile;
      if (success)
      {
        if (Current.Type == HtmlTokenType.StartTag && !IsEmptyElement)
          _depth++;
        else if (Current.Type == HtmlTokenType.EndTag)
          _depth--;
      }
    }

#if NET35
    public override bool HasValue
    {
      get { return true; }
    }
#endif
#if !PORTABLE
    public override void Close()
    {
      
    }
#endif
  }
}
