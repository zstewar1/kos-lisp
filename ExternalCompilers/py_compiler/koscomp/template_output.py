from jinja2 import Environment, PackageLoader

config = {
    'block_start_string': '{%',
    'block_end_string': '%}',
    'variable_start_string': '%%',
    'variable_end_string': '%%',
    'comment_start_string': '{#',
    'comment_end_string': '#}',
}

def compile(ast, filename, template_set, base_template):
  env = Environment(loader=PackageLoader('koscomp', template_set), **config)
  template = env.get_template(base_template)
  with open(filename, 'w') as outfile:
    for partial in template.generate(ast):
      outfile.write(partial)
