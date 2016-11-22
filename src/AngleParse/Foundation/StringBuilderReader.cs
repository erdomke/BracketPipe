using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngleParse
{
  /// <summary>Implements a <see cref="T:System.IO.TextReader" /> that reads from a string.</summary>
  /// <filterpriority>2</filterpriority>
  public class StringBuilderReader : TextReader
  {
    /// <summary>Initializes a new instance of the <see cref="T:System.IO.StringReader" /> class that reads from the specified string.</summary>
    /// <param name="s">The string to which the <see cref="T:System.IO.StringReader" /> should be initialized. </param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="s" /> parameter is null. </exception>
    public StringBuilderReader(StringBuilder s)
    {
      if (s == null)
      {
        throw new ArgumentNullException("s");
      }
      _s = s;
      _length = ((s == null) ? 0 : s.Length);
    }

    /// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.StringReader" /> and optionally releases the managed resources.</summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
    protected override void Dispose(bool disposing)
    {
      _s = null;
      _pos = 0;
      _length = 0;
      base.Dispose(disposing);
    }

    /// <summary>Returns the next available character but does not consume it.</summary>
    /// <returns>An integer representing the next character to be read, or -1 if no more characters are available or the stream does not support seeking.</returns>
    /// <exception cref="T:System.ObjectDisposedException">The current reader is closed. </exception>
    /// <filterpriority>2</filterpriority>
    public override int Peek()
    {
      if (_s == null)
        throw new InvalidOperationException("Reader is closed");
      if (_pos == _length)
        return -1;

      return (int)_s[_pos];
    }

    /// <summary>Reads the next character from the input string and advances the character position by one character.</summary>
    /// <returns>The next character from the underlying string, or -1 if no more characters are available.</returns>
    /// <exception cref="T:System.ObjectDisposedException">The current reader is closed. </exception>
    /// <filterpriority>2</filterpriority>
    public override int Read()
    {
      if (_s == null)
        throw new InvalidOperationException("Reader is closed");
      if (_pos == _length)
        return -1;

      var pos = _pos;
      _pos = pos + 1;
      return (int)_s[pos];
    }

    /// <summary>Reads a block of characters from the input string and advances the character position by <paramref name="count" />.</summary>
    /// <returns>The total number of characters read into the buffer. This can be less than the number of characters requested if that many characters are not currently available, or zero if the end of the underlying string has been reached.</returns>
    /// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (<paramref name="index" /> + <paramref name="count" /> - 1) replaced by the characters read from the current source. </param>
    /// <param name="index">The starting index in the buffer. </param>
    /// <param name="count">The number of characters to read. </param>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="buffer" /> is null. </exception>
    /// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. </exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="index" /> or <paramref name="count" /> is negative. </exception>
    /// <exception cref="T:System.ObjectDisposedException">The current reader is closed. </exception>
    /// <filterpriority>2</filterpriority>
    public override int Read(char[] buffer, int index, int count)
    {
      if (buffer == null)
        throw new ArgumentNullException("buffer");
      if (index < 0)
        throw new ArgumentOutOfRangeException("index");
      if (count < 0)
        throw new ArgumentOutOfRangeException("count");
      if (buffer.Length - index < count)
        throw new ArgumentException();
      if (_s == null)
        throw new InvalidOperationException("Reader is closed");

      var num = _length - _pos;
      if (num > 0)
      {
        if (num > count)
          num = count;
        _s.CopyTo(_pos, buffer, index, num);
        _pos += num;
      }
      return num;
    }

    /// <summary>Reads all characters from the current position to the end of the string and returns them as a single string.</summary>
    /// <returns>The content from the current position to the end of the underlying string.</returns>
    /// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string. </exception>
    /// <exception cref="T:System.ObjectDisposedException">The current reader is closed. </exception>
    /// <filterpriority>2</filterpriority>
    public override string ReadToEnd()
    {
      if (_s == null)
        throw new InvalidOperationException("Reader is closed");

      var result = _s.ToString(_pos, _length - _pos);
      _pos = _length;
      return result;
    }

    /// <summary>Reads a line of characters from the current string and returns the data as a string.</summary>
    /// <returns>The next line from the current string, or null if the end of the string is reached.</returns>
    /// <exception cref="T:System.ObjectDisposedException">The current reader is closed. </exception>
    /// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string. </exception>
    /// <filterpriority>2</filterpriority>
    public override string ReadLine()
    {
      if (_s == null)
        throw new InvalidOperationException("Reader is closed");

      int i;
      for (i = _pos; i < _length; i++)
      {
        char c = _s[i];
        if (c == '\r' || c == '\n')
        {
          var result = _s.ToString(_pos, i - _pos);
          _pos = i + 1;

          if (c == '\r' && _pos < _length && _s[_pos] == '\n')
            _pos++;

          return result;
        }
      }

      if (i > _pos)
      {
        var result2 = _s.ToString(_pos, i - _pos);
        _pos = i;
        return result2;
      }
      return null;
    }

    /// <summary>Reads a line of characters asynchronously from the current string and returns the data as a string.</summary>
    /// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the next line from the string reader, or is null if all the characters have been read.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">The number of characters in the next line is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The string reader has been disposed.</exception>
    /// <exception cref="T:System.InvalidOperationException">The reader is currently in use by a previous read operation. </exception>
    public override Task<string> ReadLineAsync()
    {
      return Task.FromResult<string>(this.ReadLine());
    }

    /// <summary>Reads all characters from the current position to the end of the string asynchronously and returns them as a single string.</summary>
    /// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains a string with the characters from the current position to the end of the string. </returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">The number of characters is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The string reader has been disposed.</exception>
    /// <exception cref="T:System.InvalidOperationException">The reader is currently in use by a previous read operation. </exception>
    public override Task<string> ReadToEndAsync()
    {
      return Task.FromResult<string>(this.ReadToEnd());
    }

    /// <summary>Reads a specified maximum number of characters from the current string asynchronously and writes the data to a buffer, beginning at the specified index.</summary>
    /// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the string has been reached.</returns>
    /// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (<paramref name="index" /> + <paramref name="count" /> - 1) replaced by the characters read from the current source.</param>
    /// <param name="index">The position in <paramref name="buffer" /> at which to begin writing.</param>
    /// <param name="count">The maximum number of characters to read. If the end of the string is reached before the specified number of characters is written into the buffer, the method returns.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="buffer" /> is null.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
    /// <exception cref="T:System.ArgumentException">The sum of <paramref name="index" /> and <paramref name="count" /> is larger than the buffer length.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The string reader has been disposed.</exception>
    /// <exception cref="T:System.InvalidOperationException">The reader is currently in use by a previous read operation. </exception>
    public override Task<int> ReadBlockAsync(char[] buffer, int index, int count)
    {
      if (buffer == null)
        throw new ArgumentNullException("buffer");
      if (index < 0 || count < 0)
        throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count");
      if (buffer.Length - index < count)
        throw new ArgumentException();

      return Task.FromResult<int>(this.ReadBlock(buffer, index, count));
    }

    /// <summary>Reads a specified maximum number of characters from the current string asynchronously and writes the data to a buffer, beginning at the specified index. </summary>
    /// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the string has been reached.</returns>
    /// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (<paramref name="index" /> + <paramref name="count" /> - 1) replaced by the characters read from the current source.</param>
    /// <param name="index">The position in <paramref name="buffer" /> at which to begin writing.</param>
    /// <param name="count">The maximum number of characters to read. If the end of the string is reached before the specified number of characters is written into the buffer, the method returns.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///   <paramref name="buffer" /> is null.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///   <paramref name="index" /> or <paramref name="count" /> is negative.</exception>
    /// <exception cref="T:System.ArgumentException">The sum of <paramref name="index" /> and <paramref name="count" /> is larger than the buffer length.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The string reader has been disposed.</exception>
    /// <exception cref="T:System.InvalidOperationException">The reader is currently in use by a previous read operation. </exception>
    public override Task<int> ReadAsync(char[] buffer, int index, int count)
    {
      if (buffer == null)
        throw new ArgumentNullException("buffer");
      if (index < 0 || count < 0)
        throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count");
      if (buffer.Length - index < count)
        throw new ArgumentException();

      return Task.FromResult<int>(this.Read(buffer, index, count));
    }

    private StringBuilder _s;

    private int _pos;

    private int _length;
  }
}
