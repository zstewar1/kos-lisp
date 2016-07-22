from jinja2 import Environment, PackageLoader

config = {
    'block_start_string': '{%',
    'block_end_string': '%}',
    'variable_start_string': '%%',
    'variable_end_string': '%%',
    'comment_start_string': '{#',
    'comment_end_string': '#}',
    'trim_blocks': True,
    'lstrip_blocks': True,
}

def compile(
    ast, filename, template_set, base_template, *, extra_globals={}, extra_config={}):
  envconf = config.copy()
  envconf.update(extra_config)
  env = Environment(loader=PackageLoader('koscomp', template_set), **envconf)
  env.globals.update(extra_globals)
  template = env.get_template(base_template)
  with open(filename, 'w') as outfile:
    for partial in template.generate(ast):
      outfile.write(partial)
