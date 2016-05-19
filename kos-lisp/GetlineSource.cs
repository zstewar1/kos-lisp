using Mono.Terminal;
using System.Collections.Generic;

using ZStewart.KOSLisp.Parse;

namespace ZStewart.KOSLisp {
  public class GetLineSource : Source {
    public string Name => "<stdin>";

    public IEnumerator<string> GetEnumerator() {

    }
  }
}
