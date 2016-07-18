using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ZStewart.KOSLisp.Compile.Generators.Json {
  /// <summary>
  /// Represents a module which is included in the program.
  /// </summary>
  public class JModule {
    private List<JAst> operations;

    /// <summary>
    /// Whether this is a builtin module or lisp module.
    /// </summary>
    public bool IsBuiltin { get { return operations == null; } }

    /// <summary>
    /// The module's string identifier.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// The collection of operations that make up this module (if not builtin).
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public IReadOnlyList<JAst> Operations {
      get {
        return operations?.AsReadOnly();
      }
    }

    internal JModule(bool isBuiltin, string identifier) {
      Identifier = identifier;
      if (!isBuiltin) {
        operations = new List<JAst>();
      }
    }

    /// <summary>
    /// Adds an operation to the module's operation list.
    /// </summary>
    internal void AddOperation(JAst op) {
      checkBuiltin();
      operations.Add(op);
    }

    /// <summary>
    /// Raises an error if the module is a builtin module. Used for checks when dealing
    /// with the operations list.
    /// </summary>
    private void checkBuiltin() {
      if (IsBuiltin) {
        throw new InvalidOperationException("Builtin modules do not have operations");
      }
    }
  }
}
