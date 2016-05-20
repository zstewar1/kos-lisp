using Mono.Terminal;
using System;
using System.Collections.Generic;

using ZStewart.KOSLisp.Parse;

namespace ZStewart.KOSLisp {
  public class GetlineSource : Source {
    public event Action OnBeforeReadLine;

    public string Name => "<stdin>";
    public string Prompt { get; set; } = "";

    private readonly LineEditor lineEditor;

    public GetlineSource(string progName, int histSize) {
      lineEditor = new LineEditor(progName, histSize);
    }

    public GetlineSource(string progName) : this(progName, 100) {}

    public IEnumerator<string> GetEnumerator() {
      string line;
      OnBeforeReadLine?.Invoke();
      while((line = lineEditor.Edit(Prompt, "")) != null) {
        yield return line;
        OnBeforeReadLine?.Invoke();
      }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return GetEnumerator();
    }
  }
}
