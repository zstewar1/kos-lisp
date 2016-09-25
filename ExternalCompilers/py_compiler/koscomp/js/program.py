"""Generate the program shell template from the Json formatted list"""

from koscomp.js.jstree import *
from koscomp.js.operations import convert, get_intermediate, block_expression


def constant(index) -> JExpression:
  return VarRef('ReferencedConstants')[index]


def declare_constant(constant) -> JExpression:
  if constant.Type == 'Bool':
    if constant.Value:
      return Deref('BoolType', 'T')
    else:
      return Deref('BoolType', 'F')
  elif constant.Type == 'Cons':
    return Deref('ConsType', 'Create')(
        Index('ReferencedConstants', constant.Car),
        Index('ReferencedConstants', constant.Cdr))
  elif constant.Type == 'Keyword':
    return Deref('KeywordSymbolType', 'Create')(constant.Identifier)
  elif constant.Type == 'Nil':
    return Deref('NilType', 'Nil')
  elif constant.Type == 'Symbol':
    return Deref('SymbolType', 'Create')(constant.Identifier)
  elif constant.Type == 'Number':
    return Deref('NumberType', 'Create')(constant.Value)
  elif constant.Type == 'String':
    return Deref('StringType', 'Create')(constant.Value)
  raise TypeError('Unexpected constant type "%s"' % constant.Type)


def declare_module(ident, module) -> JExpression:
  if module.IsBuiltin:
    import_func = [
        VarDecl('mod', VarRef('Builtins')[ident]()),
        Assign(VarRef('Modules')[ident], FDecl([], [Return(VarRef('mod'))])),
        Return(VarRef('mod')),
    ]
  else:
    import_func = [
        VarDecl('importFunc', VarRef('Modules')[ident]),
        VarDecl('mod', Deref('ModuleType', 'Create')(constant(module.Identifier))),
        Assign(VarRef('Modules')[ident], FDecl([], [Return(VarRef('mod'))])),
        Try(
          [convert(op).emit().as_statement() for op in module.Operations],
          ('err', [
            Assign(VarRef('Modules')[ident], VarRef('importFunc')),
            Throw(VarRef('err')),
          ])),
        Return(VarRef('mod')),
  ]
  return FDecl([], import_func)


def compile(ast, filename) -> str:
  prog = [] # type: List[JStatement]
  prog.append(VarDecl('ReferencedConstants', Array()))
  prog.extend([
    Deref('ReferencedConstants', 'push')(declare_constant(constant)).as_statement()
    for constant in ast.ReferencedConstants
  ])
  prog.extend([
    VarDecl(
      'Modules',
      Object([
        (ident, declare_module(ident, module))
        for ident, module in ast.Modules.items()
      ])),
    VarRef('Modules')['--main--']().as_statement(),
  ])
  text = JProgram(prog).generate(Indenter())
  with open(filename, 'w') as outfile:
    outfile.write(text)
