using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AngleParse
{
  public class HtmlString
    : IEquatable<string>
    , IEquatable<HtmlString>
  {
    private string _html;

    public int Length { get { return _html.Length; } }

    public HtmlString(string html)
    {
      if (html == null)
        throw new ArgumentNullException("html");
      _html = html;
    }

    public bool Equals(string other)
    {
      return string.Equals(_html, other);
    }

    public bool Equals(HtmlString other)
    {
      if (other == null)
        return false;
      return string.Equals(_html, other._html);
    }

    public override bool Equals(object obj)
    {
      if (obj is string)
        return Equals((string)obj);
      if (obj is HtmlString)
        return Equals((HtmlString)obj);
      return false;
    }

    public override int GetHashCode()
    {
      return _html.GetHashCode();
    }

    public override string ToString()
    {
      return _html;
    }

    public static implicit operator string(HtmlString value)
    {
      return value._html;
    }

    public static bool operator ==(HtmlString a, HtmlString b)
    {
      // If both are null, or both are same instance, return true.
      if (System.Object.ReferenceEquals(a, b))
        return true;

      // If one is null, but not both, return false.
      if (((object)a == null) || ((object)b == null))
        return false;

      // Return true if the fields match:
      return a.Equals(b);
    }
    public static bool operator !=(HtmlString a, HtmlString b)
    {
      return !(a == b);
    }

    public static bool operator ==(HtmlString a, string b)
    {
      // If both are null, or both are same instance, return true.
      if (System.Object.ReferenceEquals(a, b))
        return true;

      // If one is null, but not both, return false.
      if (((object)a == null) || ((object)b == null))
        return false;

      // Return true if the fields match:
      return a.Equals(b);
    }
    public static bool operator !=(HtmlString a, string b)
    {
      return !(a == b);
    }

    public static bool operator ==(string b, HtmlString a)
    {
      // If both are null, or both are same instance, return true.
      if (System.Object.ReferenceEquals(a, b))
        return true;

      // If one is null, but not both, return false.
      if (((object)a == null) || ((object)b == null))
        return false;

      // Return true if the fields match:
      return a.Equals(b);
    }
    public static bool operator !=(string b, HtmlString a)
    {
      return !(a == b);
    }
  }
}
