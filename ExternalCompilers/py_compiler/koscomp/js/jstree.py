import abc
import json
from numbers import Real
from typing import *


__all__ = [
    'PrimitiveTypes',
    'Indenter',
    'JAst',
    'JStatement',
    'JExpression',
    'JAssignableExpression',
    'JProgram',
    'Empty',
    'ExprStmt',
    'Primitive',
    'VarRef',
    'VarDecl',
    'Block',
    'If',
    'While',
    'Conditional',
    'FDecl',
    'FCall',
    'Array',
    'Object',
    'Index',
    'Deref',
    'Assign',
    'Return',
    'Try',
    'Throw',
    'Binop',
    'Uniop',
]


PrimitiveTypes = Union[str, int, bool, Real, None]


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
  def generate_unwrapped(self, indent: Indenter) -> str:
    return self.generate(indent)

  def __getitem__(self, key: Union['JExpression', PrimitiveTypes]) -> 'Index':
    return Index(self, key)

  def __call__(self, *args: Union['JExpression', PrimitiveTypes]) -> 'FCall':
    return FCall(self, *args)

  def __add__(self, other: Union['JExpression', PrimitiveTypes]) -> 'Binop':
    return Binop('+', self, other)

  def __sub__(self, other: Union['JExpression', PrimitiveTypes]) -> 'Binop':
    return Binop('-', self, other)

  def __mul__(self, other: Union['JExpression', PrimitiveTypes]) -> 'Binop':
    return Binop('*', self, other)

  def __truediv__(self, other: Union['JExpression', PrimitiveTypes]) -> 'Binop':
    return Binop('/', self, other)

  def as_statement(self) -> 'ExprStmt':
    return ExprStmt(self)


class JAssignableExpression(JExpression):
  pass


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


class Empty(JStatement):
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


class VarRef(JAssignableExpression):
  def __init__(self, name: str) -> None:
    self.name = name

  def generate(self, indent: Indenter):
    return self.name

  @staticmethod
  def to_expression(value: Union[JExpression, str]) -> JExpression:
    if isinstance(value, JExpression):
      return value
    return VarRef(value)

  @staticmethod
  def to_var_ref(value: Union['VarRef', str]) -> 'VarRef':
    if isinstance(value, VarRef):
      return value
    return VarRef(value)


class VarDecl(JStatement):
  def __init__(
      self, name: Union[VarRef, str], initializer: Optional[JExpression]=None) -> None:
    self.name = VarRef.to_var_ref(name)
    self.initializer = initializer

  def generate(self, indent: Indenter) -> str:
    if self.initializer is None:
      return ''.join(['var ', self.name.generate(indent), ';'])
    return ''.join([
      'var ',
      self.name.generate(indent),
      ' = ',
      self.initializer.generate(indent),
      ';',
    ])


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
        self.condition.generate_unwrapped(indent.increase()),
        ') ',
        self.body.generate(indent),
    ]
    if self.otherwise:
      out.extend([' else ', self.otherwise.generate(indent)])
    return ''.join(out)


class While(JStatement):
  def __init__(
      self, condition: JExpression, body: JStatement) -> None:
    self.condition = condition
    self.body = body

  def generate(self, indent: Indenter):
    inner_indent = indent.increase()
    return ''.join([
        'while (',
        self.condition.generate_unwrapped(inner_indent),
        ') ',
        self.body.generate(indent),
    ])


class Conditional(JExpression):
  def __init__(
      self,
      condition: JExpression,
      value_if_true: JExpression,
      value_if_false: JExpression) -> None:
    self.condition = condition
    self.value_if_true = value_if_true
    self.value_if_false = value_if_false

  def generate(self, indent: Indenter):
    i1 = indent.increase()
    i2 = i1.increase()
    result = [
        '(',
        self.condition.generate(i2),
        '\n',
        i1.indent,
        '? ',
        self.value_if_true.generate(i2),
        '\n',
        i1.indent,
        ': ',
        self.value_if_false.generate(i2),
        ')',
    ]
    return ''.join(result)

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
  def __init__(
      self,
      elements: List[
        Tuple[
          Union[JExpression, PrimitiveTypes],
          Union[JExpression, PrimitiveTypes]]]=[]) -> None:
    self.elements = [
        (Primitive.to_expression(key), Primitive.to_expression(value))
        for key, value in elements
    ]

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
    return ''.join(obj)


class Index(JAssignableExpression):
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


class Deref(JAssignableExpression):
  def __init__(self, obj: Union[JExpression, str], *elements: str) -> None:
    if isinstance(obj, str):
      self.obj = VarRef(obj) # type: JExpression
    else:
      self.obj = obj
    self.elements = elements

  def generate(self, indent: Indenter) -> str:
    return '.'.join((self.obj.generate(indent),) + self.elements)


class Assign(JStatement):
  def __init__(self, to: JAssignableExpression, value: JExpression) -> None:
    self.to = to
    self.value = value

  def generate(self, indent: Indenter) -> str:
    return ''.join([self.to.generate(indent), ' = ', self.value.generate(indent), ';'])


class Return(JStatement):
  def __init__(self, value: JExpression) -> None:
    self.value = value

  def generate(self, indent: Indenter) -> str:
    return ''.join(['return ', self.value.generate(indent), ';'])


class Try(JStatement):
  def __init__(
      self,
      protected: Union[Block, List[JStatement]],
      catch: Optional[Tuple[Union[VarRef, str], Union[Block, List[JStatement]]]]=None,
      finally_: Optional[Union[Block, List[JStatement]]]=None) -> None:
    assert any([catch, finally_]), 'must provide at least one of catch, finally'
    self.protected = Block.to_block(protected)
    self.catch = (VarRef.to_var_ref(catch[0]), Block.to_block(catch[1])) \
        if catch is not None else None
    self.finally_ = Block.to_block(finally_) if finally_ is not None else None

  def generate(self, indent: Indenter) -> str:
    tcf = [
        'try ',
        self.protected.generate(indent),
    ]
    if self.catch is not None:
      tcf.extend([
        ' catch(',
        self.catch[0].generate(indent.increase()),
        ') ',
        self.catch[1].generate(indent),
      ])
    if self.finally_ is not None:
      tcf.extend([
        ' finally ',
        self.finally_.generate(indent)
      ])
    return ''.join(tcf)


class Throw(JStatement):
  def __init__(self, value: JExpression) -> None:
    self.value = value

  def generate(self, indent: Indenter) -> str:
    return ''.join(['throw ', self.value.generate(indent), ';'])


class Binop(JExpression):
  PRECEDENCE = {
      '^': 5,
      '*': 4,
      '/': 4,
      '+': 3,
      '-': 2,
  }

  ASSOCIATIVE = {'*', '+'}

  @classmethod
  def precedence(cls, operator: str) -> int:
    return cls.PRECEDENCE.get(operator, -1)

  @classmethod
  def is_associative(cls, operator: str) -> bool:
    return operator in cls.ASSOCIATIVE

  def __init__(
      self,
      operator: str,
      lhs: Union[JExpression, PrimitiveTypes],
      rhs: Union[JExpression, PrimitiveTypes],
      *expressions: Union[JExpression, PrimitiveTypes]) -> None:
    self.operator = operator
    exprs = [Primitive.to_expression(lhs), Primitive.to_expression(rhs)]
    exprs.extend(map(Primitive.to_expression, expressions))
    self.expressions = [] # type: List[JExpression]
    for expr in exprs:
      if isinstance(expr, Binop) \
          and self.is_associative(self.operator) \
          and expr.operator == self.operator:
        self.expressions.extend(expr.expressions)
      else:
        self.expressions.append(expr)

  def generate(self, indent: Indenter) -> str:
    return ''.join(['(', self.generate(indent), ')'])

  def generate_unwrapped(self, indent: Indenter) -> str:
    return (' ' + self.operator + ' ').join(
        [self.maybe_unwrap(expression, indent) for expression in self.expressions])

  def maybe_unwrap(self, expression: JExpression, indent: Indenter) -> str:
    if isinstance(expression, Binop) \
        and self.precedence(expression.operator) <= self.precedence(self.operator):
      return expression.generate(indent)
    else:
      return expression.generate_unwrapped(indent)


class Uniop(JExpression):
  def __init__(self, operator: str, value: JExpression) -> None:
    self.operator = operator
    self.value = value

  def generate(self, indent: Indenter) -> str:
    return ''.join(['(', self.generate_unwrapped(indent), ')'])

  def generate_unwrapped(self, indent: Indenter) -> str:
    return self.operator + self.value.generate(indent)
