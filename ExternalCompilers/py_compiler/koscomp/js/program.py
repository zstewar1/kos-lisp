"""Generate the program shell template from the Json formatted list"""

from koscomp.js.jstree import *

def declare_constant(constant) -> JExpression:
  if constant.Type == 'Bool':
    if constant.Value:
      return Deref('BoolType', 'T')
    else:
      return Deref('BoolType', 'F')
  elif constant.Type == 'Cons':
    return FCall(
        Deref('ConsType', 'Create'),
        Index('ReferencedConstants', constant.Car),
        Index('ReferencedConstants', constant.Car))
  elif constant.Type == 'Keyword':
    return FCall(Deref('KeywordSymbolType', 'Create'), constant.Identifier)
  elif constant.Type == 'Nil':
    return Deref('NilType', 'Nil')
  elif constant.Type == 'Symbol':
    return FCall(Deref('SymbolType', 'Create'), constant.Identifier)
  elif constant.Type == 'Number':
    return FCall(Deref('NumberType', 'Create'), constant.Value)
  elif constant.Type == 'String':
    return FCall(Deref('StringType', 'Create'), constant.Value)
  raise TypeError('Unexpected constant type "%s"' % constant.Type)

def compile(ast, filename) -> str:
  prog = [
      VarDecl('ReferencedConstants', Array()),
      FDecl([], [
        Deref('ReferencedConstants', 'push')(declare_constant(constant)).as_statement()
        for constant in ast.ReferencedConstants
      ])().as_statement()
  ]
  text = JProgram(prog).generate(Indenter())
  with open(filename, 'w') as outfile:
    outfile.write(text)
