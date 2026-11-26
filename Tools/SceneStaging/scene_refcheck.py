import re, subprocess, sys
path = sys.argv[1]
text = subprocess.run(['git', 'show', ':' + path], capture_output=True, encoding='utf-8', check=True).stdout
ids = set(re.findall(r'^--- !u!\d+ &(-?\d+)', text, re.M))
dups = len(re.findall(r'^--- !u!\d+ &(-?\d+)', text, re.M)) - len(ids)
# local references only (no guid on the same brace)
refs = set(re.findall(r'\{fileID: (-?\d+)\}', text)) - {'0'}
missing = sorted(r for r in refs if r not in ids)
print('blocks', len(ids), 'duplicates', dups, 'unresolved local refs', len(missing), missing[:10])
