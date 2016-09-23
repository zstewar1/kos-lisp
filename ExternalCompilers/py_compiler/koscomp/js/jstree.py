import abc
import json
from numbers import Real
from typing import *


PrimitiveTypes = Union[str, Real]


class Indenter(object):
  def __init__(self, indent_depth: int=0, indentation: str='  ') -> None:
    self.indent_depth = indent_depth
    self.indentation = indentation

  @property
  def indent(self) -> str:
    return self.indent_depth * self.indentation

  def increase(self) -> 'Indenter':
    return Indenter(self.indent_depth + 1, self.indentation)


class JAst(object, metaclass=abc.ABCMeta):
  @abc.abstractmethod
  def generate(self, indent: Indenter) -> str:
    pass


class JStatement(JAst):
  pass


class JExpression(JAst):
  def __getitem__(self, key: Union['JExpression', PrimitiveTypes]) -> 'Index':
    return Index(self, key)

  def __call__(self, *args: Union['JExpression', PrimitiveTypes]) -> 'FCall':
    return FCall(self, *args)

  def as_statement(self) -> 'ExprStmt':
    return ExprStmt(self)


class JProgram(JAst):
  def __init__(self, statements: List[JStatement]=[]) -> None:
    self.statements = statements or []

  def generate(self, indent: Indenter) -> str:
    stmts = []
    for statement in self.statements:
      stmts.extend([
        indent.indent,
        statement.generate(indent),
        '\n',
      ])
    return ''.join(stmts)


class Empty(JExpression):
  def generate(self, indent: Indenter) -> str:
    return ';'


class ExprStmt(JStatement):
  def __init__(self, expr: JExpression) -> None:
    self.expr = expr

  def generate(self, indent: Indenter) -> str:
    return self.expr.generate(indent) + ';'


class Primitive(JExpression):
  def __init__(self, val: PrimitiveTypes) -> None:
    self.val = val

  def generate(self, indent: Indenter) -> str:
    return json.dumps(self.val)

  @staticmethod
  def to_expression(value: Union[JExpression, PrimitiveTypes]) -> JExpression:
    if isinstance(value, JExpression):
      return value
    return Primitive(value)


class VarRef(JExpression):
  def __init__(self, name: str) -> None:
    self.name = name

  def generate(self, indent: Indenter):
    return self.name

  @staticmethod
  def to_expression(value: Union[JExpression, str]) -> JExpression:
    if isinstance(value, JExpression):
      return value
    return VarRef(value)

class VarDecl(JStatement):
  def __init__(self, name: str, initializer: Optional[JExpression]=None) -> None:
    self.name = name
    self.initializer = initializer

  def generate(self, indent: Indenter) -> str:
    if self.initializer is None:
      return 'var %s;' % self.name
    return 'var %s = %s;' % (self.name, self.initializer.generate(indent.increase()))


class Block(JStatement):
  def __init__(self, statements: List[JStatement]=[]) -> None:
    self.statements = statements or []

  def generate(self, indent: Indenter):
    if not self.statements:
      return '{}'
    out = ['{']
    inner_indent = indent.increase()
    for statement in self.statements:
      out.extend(['\n', inner_indent.indent, statement.generate(inner_indent)])
    out.extend(['\n', indent.indent, '}'])
    return ''.join(out)

  @staticmethod
  def to_block(block: Union['Block', List[JStatement]]) -> 'Block':
    if isinstance(block, Block):
      return block
    return Block(block)


class If(JStatement):
  def __init__(
      self,
      condition: JExpression,
      body: JStatement,
      otherwise: Optional[JStatement]=None) -> None:
    self.condition = condition
    self.body = body
    self.otherwise = otherwise

  def generate(self, indent: Indenter):
    out = [
        'if(',
        self.condition.generate(indent.increase()),
        ') ',
        self.body.generate(indent),
    ]
    if self.otherwise:
      out.extend([' else ', self.otherwise.generate(indent)])
    return ''.join(out)


class FDecl(JExpression):
  def __init__(self, args: List[str], body: Union[Block, List[JStatement]]) -> None:
    self.args = args
    self.body = Block.to_block(body)

  def generate(self, indent: Indenter) -> str:
    out = [
        'function(',
        ', '.join(self.args),
        ') ',
        self.body.generate(indent),
    ]
    return ''.join(out)


class FCall(JExpression):
  def __init__(
      self, func: JExpression, *args: Union[JExpression, PrimitiveTypes]) -> None:
    self.func = func
    self.args = tuple(map(Primitive.to_expression, args))

  def generate(self, indent: Indenter) -> str:
    out = [
        self.func.generate(indent),
        '(',
        self.generate_args(indent),
        ')'
    ]
    return ''.join(out)

  def generate_args(self, indent: Indenter) -> str:
    inner_indent = indent.increase()
    if len(self.args) < 5:
      return ', '.join(arg.generate(inner_indent) for arg in self.args)
    else:
      args = ['\n', inner_indent.indent]
      for arg in self.args:
        args.extend([
          arg.generate(inner_indent),
          ',\n',
          inner_indent.indent
        ])
      args[-2:] = ['\n', indent.indent]
      return ''.join(args)


class Array(JExpression):
  def __init__(self, elements: List[JExpression]=[]) -> None:
    self.elements = elements or []

  def generate(self, indent: Indenter) -> str:
    if not self.elements:
      return '[]'
    if len(self.elements) < 5:
      return ''.join([
        '[',
        ', '.join(element.generate(indent) for element in self.elements),
        ']'
      ])
    inner_indent = indent.increase()
    array = ['[\n', inner_indent.indent]
    for element in self.elements:
      array.extend([
        element.generate(inner_indent),
        ',\n',
        inner_indent.indent,
      ])
    array[-2:] = ['\n', indent.indent, ']']
    return ''.join(array)


class Object(JExpression):
  def __init__(self, elements: List[Tuple[JExpression, JExpression]]=[]) -> None:
    self.elements = elements

  def generate(self, indent: Indenter) -> str:
    if not self.elements:
      return '{}'
    inner_indent = indent.increase()
    obj = ['{\n', inner_indent.indent]
    for element in self.elements:
      obj.extend([
        element[0].generate(inner_indent),
        ': ',
        element[1].generate(inner_indent),
        ',\n',
        inner_indent.indent,
      ])
    obj[-2:] = ['\n', indent.indent, '}']


class Index(JExpression):
  def __init__(
      self,
      obj: Union[JExpression, str],
      index: Union[JExpression, PrimitiveTypes]) -> None:
    self.obj = VarRef.to_expression(obj)
    self.index = Primitive.to_expression(index)

  def generate(self, indent: Indenter) -> str:
    return ''.join([
      self.obj.generate(indent),
      '[',
      self.index.generate(indent.increase()),
      ']',
    ])


class Deref(JExpression):
  def __init__(self, obj: Union[JExpression, str], *elements: str) -> None:
    if isinstance(obj, str):
      self.obj = VarRef(obj) # type: JExpression
    else:
      self.obj = obj
    self.elements = elements

  def generate(self, indent: Indenter) -> str:
    return '.'.join((self.obj.generate(indent),) + self.elements)
