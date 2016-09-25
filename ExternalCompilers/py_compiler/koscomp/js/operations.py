import abc
from numbers import Real
import re
from typing import List, Union, Optional
import typing

from koscomp.gadict import GetAttrDict
from koscomp.js.jstree import *


next_intermediate = 0


def get_intermediate(extra_ident: str='') -> str:
  global next_intermediate
  i = next_intermediate
  next_intermediate += 1
  ident = '_intermediate_%d' % i
  if extra_ident:
    ident = ident + '_' + extra_ident
  return ident


def block_expression(block: Union[Block, List[JStatement]]) -> JExpression:
  return FDecl([], block)()


class JGenerator(object, metaclass=abc.ABCMeta):
  @abc.abstractmethod
  def emit(self) -> JExpression:
    pass


class JBindingGenerator(JGenerator):
  @abc.abstractmethod
  def emit_set(self, value: JExpression) -> JStatement:
    pass

  @abc.abstractmethod
  def emit_declare(self, value: Optional[JExpression]=None) -> JStatement:
    pass

  @property
  @abc.abstractmethod
  def identifier(self) -> str:
    pass


def convert(jast: GetAttrDict) -> JGenerator:
  return {
      'Const': Constant,
      'Defmacro': Defmacro,
      'Defun': Defun,
      'Call': Call,
      'Global': Global,
      'If': Cond,
      'Import': Import,
      'Lambda': Lambda,
      'Let': Let,
      'Local': Local,
      'Progn': Progn,
      'Set': SetVar,
      'Try': TryCatch,
  }[jast.Type](jast)


def convert_binding(jbinding: GetAttrDict) -> JBindingGenerator:
  return {
      'Global': Global,
      'Local': Local,
  }[jbinding.Type](jbinding)


class Local(JBindingGenerator):
  REPLACERE = re.compile(r'[^a-zA-Z0-9_$]', flags=re.A)

  @staticmethod
  def convert_char(match):
    val = match.group()
    assert len(val) == 1
    return '_u%04x' % ord(val)

  def __init__(self, local: GetAttrDict) -> None:
    self._identifier = local.Identifier
    self.uid = local.UniqueId
    self.var = '_local_%d_%s' % (
        self.uid, self.REPLACERE.sub(self.convert_char, self._identifier))

  def emit(self) -> JExpression:
    return VarRef(self.var)

  def emit_set(self, value: JExpression) -> JStatement:
    return Assign(VarRef(self.var), value)

  def emit_declare(self, value: Optional[JExpression]=None) -> JStatement:
    return VarDecl(self.var, value)

  @property
  def identifier(self) -> str:
    return self._identifier


class Global(JBindingGenerator):
  def __init__(self, global_: GetAttrDict) -> None:
    self._identifier = global_.Identifier
    self.symbol = global_.Symbol

  def emit(self) -> JExpression:
    return Deref('ModuleType', 'GetGlobal')(
        VarRef('mod'), VarRef('ReferencedConstants')[self.symbol])

  def emit_set(self, value: JExpression) -> JStatement:
    return Deref('ModuleType', 'SetGlobal')(
        VarRef('mod'), VarRef('ReferencedConstants')[self.symbol], value).as_statement()

  def emit_declare(self, value: Optional[JExpression]=None) -> JStatement:
    return self.emit_set(Primitive.to_expression(value))

  @property
  def identifier(self) -> str:
    return self._identifier


class Constant(JGenerator):
  def __init__(self, const: GetAttrDict) -> None:
    self.value = const.Value

  def emit(self) -> JExpression:
    return VarRef('ReferencedConstants')[self.value]


class Progn(JGenerator):
  def __init__(self, progn: GetAttrDict) -> None:
    self.forms = [convert(form) for form in progn.Forms]

  def emit(self) -> JExpression:
    return self.emit_forms()

  def emit_forms(self) -> JExpression:
    if not self.forms:
      return Deref('NilType', 'Nil')
    expressions = [expr.emit().as_statement() for expr in self.forms[:-1]] # type: List[JStatement]
    expressions.append(Return(self.forms[-1].emit()))
    return block_expression(expressions)

  def forms_block(self) -> List[JStatement]:
    if not self.forms:
      return [Return(Deref('NilType', 'Nil'))]
    expressions = [expr.emit().as_statement() for expr in self.forms[:-1]] # type: List[JStatement]
    expressions.append(Return(self.forms[-1].emit()))
    return expressions


class Let(Progn):
  def __init__(self, let: GetAttrDict) -> None:
    super().__init__(let)
    self.bindings = [
        (
          convert_binding(binding.Binding),
          convert(binding.InitialValue) if 'InitialValue' in binding else None
        )
        for binding in let.Bindings
    ]

  def emit(self) -> JExpression:
    expressions = [
        bind.emit_declare(value.emit() if value is not None else Deref('NilType', 'Nil'))
        for bind, value in self.bindings
    ]
    expressions.extend(self.forms_block())
    return block_expression(expressions)


class Lambda(Progn):
  def __init__(self, lambda_: GetAttrDict) -> None:
    super().__init__(lambda_)
    self.args = [
        (
          arg.Type,
          convert_binding(arg.Binding) if 'Binding' in arg else None,
          convert(arg.Default) if 'Default' in arg else None,
        )
        for arg in lambda_.Args
    ]
    self.function_name = Deref('SymbolType', 'Create')('<lambda>')
    self.arg_props = [
        {
          'type': arg.Type,
          'name': arg.Binding.Identifier if 'Bining' in arg else None,
          'is_optional':
            arg.Type in ('PositionalOrKeyword', 'Keyword') and 'Default' in arg,
        }
        for arg in lambda_.Args
    ]

  def emit(self) -> JExpression:
    return Deref('FunctionType', 'Create')(self.function_name, self.emit_lambda())

  def emit_lambda(self) -> JExpression:
    pargs = get_intermediate('pargs')
    kwargs = get_intermediate('kwargs')
    default_bindings = [
        get_intermediate('default') if arg[2] is not None else None
        for arg in self.args
    ]

    expressions = [
        Assign(VarRef(binding), arg[2].emit())
        for binding, arg in zip(default_bindings, self.args)
        if binding is not None
    ] # type: List[JStatement]
    expressions.append(
        Return(
          FDecl([pargs, kwargs], self.emit_lambda_body(pargs, kwargs, default_bindings))))
    return block_expression(expressions)

  def emit_lambda_body(
      self, pargs: str, kwargs: str, default_bindings: List[str]) -> List[JStatement]:
    body = self.emit_bind_args(pargs, kwargs, default_bindings)
    body.extend(self.forms_block())
    return body

  def emit_bind_args(
      self, pargs: str, kwargs:str, default_bindings: List[str]) -> List[JStatement]:
    collected = get_intermediate('collected_args')
    stmts = [
        Assign(VarRef(collected), Deref('Arguments', 'CollectArguments')(
          VarRef(pargs), VarRef(kwargs),
          Array([Object(list(arg.items())) for arg in self.arg_props])))
    ] # type: List[JStatement]
    for i, (prop, (_, bind, _), default) in \
        enumerate(zip(self.arg_props, self.args, default_bindings)):
      if prop['type'] in ('PositionalOrKeyword', 'Keyword'):
        if prop['is_optional']:
          stmts.extend([
            bind.emit_declare(),
            If(
              Binop('===', VarRef(collected)[i], None),
              bind.emit_set(VarRef(default)),
              bind.emit_set(VarRef(collected)[i])),
          ])
        else:
          stmts.append(bind.emit_declare(VarRef(collected)[i]))
      elif prop['type'] in ('RestCapture', 'RestKwCapture'):
        # TODO(zstewar1): will this need to be converted? In C# this used unmarshall.
        stmts.append(bind.emit_set(VarRef(collected)[i]))
    return stmts


class Defun(Lambda):
  def __init__(self, defun: GetAttrDict) -> None:
    super().__init__(defun)
    self.binding = convert_binding(defun.Name)
    self.function_name = Deref('SymbolType', 'Create')(defun.Name.Identifier)

  def emit(self) -> JExpression:
    inter = get_intermediate()
    expr = [
        VarDecl(inter, super().emit()),
        self.binding.emit_set(VarRef(inter)),
        Return(VarRef(inter)),
    ]
    return block_expression(expr)


class Defmacro(Lambda):
  def __init__(self, defmacro: GetAttrDict) -> None:
    super().__init__(defmacro)
    self.binding = convert_binding(defmacro.Name)
    self.function_name = Deref('SymbolType', 'Create')(defmacro.Name.Identifier)

  def emit(self) -> JExpression:
    inter = get_intermediate()
    expr = [
        VarDecl(inter, super().emit()),
        self.binding.emit_set(VarRef(inter)),
        Return(VarRef(inter)),
    ]
    return block_expression(expr)


class Cond(JGenerator):
  def __init__(self, cond: GetAttrDict) -> None:
    self.condition = convert(cond.Condition)
    self.value_if_true = convert(cond.ValueIfTrue)
    self.value_if_false = convert(cond.ValueIfFalse)

  def emit(self) -> JExpression:
    return Conditional(
        self.condition.emit(), self.value_if_true.emit(), self.value_if_false.emit())


class Call(JGenerator):
  def __init__(self, call: GetAttrDict) -> None:
    self.function = convert(call.Function)
    self.pargs = [convert(arg) for arg in call.PositionalArguments]
    self.kwargs = [(kw, convert(arg)) for kw, arg in call.KeywordArguments]

  def emit(self) -> JExpression:
    return Deref('CallableOperations', 'Call')(
      self.function.emit(),
      Array([arg.emit() for arg in self.pargs]),
      Object([(kw, arg.emit()) for kw, arg in self.kwargs]))


class SetVar(JGenerator):
  def __init__(self, set_var: GetAttrDict) -> None:
    self.variable = convert_binding(set_var.Variable)
    self.value = convert(set_var.Value)

  def emit(self) -> JExpression:
    inter = get_intermediate()
    return block_expression([
      VarDecl(inter, self.value.emit()),
      self.variable.emit_set(VarRef(inter)),
      Return(VarRef(inter)),
    ])


class Import(JGenerator):
  def __init__(self, import_: GetAttrDict) -> None:
    self.module = import_.Module
    self.name = convert_binding(import_.Name) if 'Name' in import_ else None
    self.from_imports = [
        (item.Symbol, convert_binding(item.Destination))
        for item in import_.FromImport
    ] if 'FromImport' in import_ else None
    self.all_import = import_.AllImport

  def emit(self) -> JExpression:
    imported = get_intermediate('imported')
    expr = [VarDecl(imported, VarRef('ImportModule')(self.module))] # type: List[JStatement]
    self.add_bind(expr, imported)
    self.add_from(expr, imported)
    self.add_all(expr, imported)
    expr.append(Return(VarRef(imported)))
    return block_expression(expr)

  def add_bind(self, expr, imported):
    if self.name is not None:
      expr.append(self.name.emit_set(imported))

  def add_from(self, expr, imported):
    if self.from_imports is not None:
      expr.extend(
          bind.emit_set(
            Deref('ModuleType', 'GetGlobal')(
              imported, VarRef('ReferencedConstants')[sym]))
          for sym, bind in self.from_imports)

  def add_all(self, expr, imported):
    if self.all_import:
      enumerator = get_intermediate('enumerator')
      enumcar = get_intermediate('enumcar')
      expr.append(
          If(
            Binop('!==', Deref(imported, '__dict__'), None),
            Block([
              VarDecl(enumerator, Deref('LispObject', 'Call')(
                Deref(imported, '__dict__'),
                Deref('SymbolType', 'Create')('iter'))),
              While(Binop('!==', enumerator, None), Block([
                VarDecl(enumcar, Deref('ListOperations', 'GetCar')(VarRef(enumerator))),
                Deref('ModuleType', 'SetGlobal')(
                  VarRef('mod'),
                  Deref('ListOperations', 'GetCar')(VarRef(enumcar)),
                  Deref('ListOperations', 'GetCdr')(VarRef(enumcar))),
                Assign(
                  VarRef(enumerator),
                  Deref('ListOperations', 'GetCdr')(VarRef(enumerator))),
              ])),
            ])))


class TryCatch(JGenerator):
  def __init__(self, try_: GetAttrDict) -> None:
    self.guarded = convert(try_.Guarded)
    self.catches = [
        (
          convert(catch.ExceptionType) if 'ExceptionType' in catch else None,
          convert_binding(catch.ExceptionBinding) if 'ExceptionBinding' in catch else None,
          convert(catch.Fallback),
        )
        for catch in try_.Catches
    ]
    self.finally_ = convert(try_.Finally)

  def emit(self) -> JExpression:
    return block_expression([
      Try([
        Return(self.guarded.emit()),
      ],
      self.generate_catch(),
      self.generate_finally()),
    ])

  def generate_catch(self):
    if not self.catches:
      return None
    err = get_intermediate('err')
    handlers = []
    for exc_type, exc_binding, fallback in self.catches:
      if exc_type is not None:
        ext = get_intermediate('exception_type')
        handlers.extend([
          Assign(VarRef(ext), exc_type.emit()),
          If(
            Uniop('!', Deref('LispType', 'IsSubtype')(
              VarRef(ext), Deref('ExceptionType', 'Exception'))),
            Throw(Deref('ExceptionType', 'CreateTypeError')(
              'exception to catch must be a sublclass of Exception'))),
        ])
        if exc_binding is not None:
          handlers.append(If(
            Deref('ExceptionType', 'CheckException')(VarRef(ext), VarRef(err)),
            Block([
              exc_binding.emit_declare(VarRef(err)),
              Return(fallback.emit()),
            ])))
        else:
          handlers.append(If(
            Deref('ExceptionType', 'CheckException')(VarRef(ext), VarRef(err)),
            Return(fallback.emit())))
      else:
        handlers.append(Return(fallback.emit()))
    handlers.append(Throw(VarRef(err)))
    return (err, handlers)

  def generate_finally(self):
    if self.finally_ is None:
      return None
    return [self.finally_.emit().as_statement()]

