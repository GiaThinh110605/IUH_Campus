"""Stage changed PrefabInstance blocks (by instance name in m_Modifications) from the working scene onto the INDEX scene.
usage: python scene_stage_pi.py <scene> <InstanceName> [...]
"""
import re, subprocess, sys

path, names = sys.argv[1], sys.argv[2:]
HDR = re.compile(r'^--- !u!(\d+) &(-?\d+)(.*)$', re.M)

def split(text):
    heads = list(HDR.finditer(text))
    pre = text[:heads[0].start()]
    out = []
    for i, m in enumerate(heads):
        end = heads[i + 1].start() if i + 1 < len(heads) else len(text)
        out.append((m.group(1), m.group(2), text[m.start():end]))
    return pre, out

base = subprocess.run(['git', 'show', ':' + path], capture_output=True, encoding='utf-8', check=True).stdout
work = open(path, encoding='utf-8', newline='').read().replace('\r\n', '\n')
bpre, bblocks = split(base)
_, wblocks = split(work)
wmap = {fid: body for t, fid, body in wblocks}
out, changed = [], []
for t, fid, body in bblocks:
    if t == '1001' and any(re.search(r'value: %s\n' % re.escape(n), body) for n in names) and fid in wmap:
        if wmap[fid] != body: changed.append(fid)
        out.append(wmap[fid]); continue
    out.append(body)
result = bpre + ''.join(out)
sha = subprocess.run(['git', 'hash-object', '-w', '--stdin'], input=result.encode('utf-8'), capture_output=True, check=True).stdout.decode().strip()
subprocess.run(['git', 'update-index', '--cacheinfo', '100644,%s,%s' % (sha, path)], check=True)
print('changed PI blocks', changed)
