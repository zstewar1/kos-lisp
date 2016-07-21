"""Raw output compiler. Simply writes the json data to the output file."""

import json

def compile(ast, filename):
  with open(filename, 'w') as outfile:
    json.dump(ast, outfile)
