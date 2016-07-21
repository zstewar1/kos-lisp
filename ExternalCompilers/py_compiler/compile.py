#!/usr/bin/env python3

import argparse
import json
import sys

def main():
  args = get_args()
  try:
    data = json.load(args.source)
  finally:
    args.source.close()

  output = args.output if args.output else 'a.out'

  if args.compiler == 'raw':
    from koscomp import raw_output
    raw_output.compile(data, output)

def get_args():
  parser = argparse.ArgumentParser(
      description='Compile KOSLisp JSON AST to code in some language')
  parser.add_argument(
      '-o', '--output', default=None, help='Where to write the compiled output file.')
  parser.add_argument(
      'source', default=sys.stdin, type=argparse.FileType('r'), nargs='?',
      help='Where to read the JSON AST from. Defaults to standard in.')
  parser.add_argument(
      '-c', '--compiler', choices=('raw'), default='raw',
      help='Which compiler module to use.')

  return parser.parse_args()

if __name__ == '__main__':
  main()
