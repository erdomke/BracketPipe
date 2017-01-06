using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BracketPipe
{
  public class MarkdownWriterSettings
  {
    public int LineLength { get; set; }
    public string NewLineChars { get; set; }
    public char QuoteChar { get; set; }
    //public bool ReplaceConsecutiveSpaceNonBreaking { get; set; }

    public MarkdownWriterSettings()
    {
      this.LineLength = 80;
      this.NewLineChars = Environment.NewLine;
      this.QuoteChar = '"';
      //this.ReplaceConsecutiveSpaceNonBreaking = false;
    }
  }
}
