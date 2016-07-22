def js_string_escape(string):
  string = string.replace('\\', '\\\\')
  string = string.replace('"', '\\"')
  string = string.replace('\n', '\\n')
  string = string.replace('\r', '\\r')
  return '"' + string + '"'

template_globals = {
    'js_string_escape': js_string_escape,
}
