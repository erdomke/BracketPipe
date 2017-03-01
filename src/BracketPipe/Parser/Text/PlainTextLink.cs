using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BracketPipe
{
  [DebuggerDisplay("<a href={Href}>{Text,nq}</a>")]
  public class PlainTextLink
  {
    public HtmlStartTag Tag { get; set; }
    public string Href { get; set; }
    public string Text { get; set; }
    public int Offset { get; set; }
  }
}
