using System;

namespace ZStewart.KOSLisp.Types {
  public class LispTypeObject : LispObject {
    public string __name__;
    public LispObject __bases__;
    public Func<LispObject, LispObject, LispObject> __new__;
    public Func<LispObject, LispObject, LispObject> __init__;
    public Func<LispObject, LispObject, LispObject> __call__;
    public Func<LispObject, LispObject, LispObject> __getattr__;
    public Func<LispObject, LispObject, LispObject> __setattr__;
    public LispObject __dict__;
  }
}
