{% macro declare(constant) %}
{% if constant.Type == 'Bool' %}
  {% if constant.Value %}
    BoolType.T
  {% else %}
    BoolType.F
  {% endif %}
{% elif constant.Type == 'Cons' %}
ConsType.Create(ReferencedConstatns[%% constant.Car %%], ReferencedConstants[%% constant.Cdr %%])
{% elif constant.Type == 'Keyword' %}
KeywordSymbolType.Create("%% constant.Identifier %%")
{% elif constant.Type == 'Nil' %}
NilType.Nil
{% elif constant.Type == 'Symbol' %}
SymbolType.Create("%% constant.Identifier %%")
{% elif constant.Type == 'Number' %}
NumberType.Create(%% constant.Value %%)
{% elif constant.Type == 'String' %}
StringType.Create(%% js_string_escape(constant.Value) %%)
{% endif %}
{% endmacro %}
