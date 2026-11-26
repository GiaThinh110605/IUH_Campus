"""Stage whole hierarchies (add / change / delete) from the working scene onto the INDEX scene.

usage: python scene_stage2.py <scene> <GameObjectName> [...]
For each name, the subtree rooted at that (non-stripped) GameObject is computed in BOTH the index and the
working file: GameObjects, Transforms, components, nested PrefabInstances (via m_TransformParent) and their
stripped blocks. Every block in the union takes its working version (or is deleted if absent there).
Top-level roots that are new / gone are added to / removed from SceneRoots.m_Roots.
"""
import re, subprocess, sys

path, names = sys.argv[1], set(sys.argv[2:])
HDR = re.compile(r'^--- !u!(\d+) &(-?\d+)(.*)$', re.M)

def split(text):
    heads = list(HDR.finditer(text))
    pre = text[:heads[0].start()]
    out = []
    for i, m in enumerate(heads):
        end = heads[i + 1].start() if i + 1 < len(heads) else len(text)
        out.append((m.group(1), m.group(2), text[m.start():end]))
    return pre, out

def ref(body, key):
    m = re.search(key + r': \{fileID: (-?\d+)', body)
    return m.group(1) if m else None

def subtree(blocks):
    by = {fid: (t, body) for t, fid, body in blocks}
    go_of, father, pi_parent, owner_pi = {}, {}, {}, {}
    for t, fid, body in blocks:
        g = ref(body, 'm_GameObject')
        if g: go_of[fid] = g
        if t in ('4', '224'): father[fid] = ref(body, 'm_Father') or '0'
        if t == '1001': pi_parent[fid] = ref(body, 'm_TransformParent') or '0'
        p = ref(body, 'm_PrefabInstance')
        if p and p != '0': owner_pi[fid] = p
    roots = set()
    for t, fid, body in blocks:
        if t == '1' and 'stripped' not in body.split('\n', 1)[0]:
            m = re.search(r'm_Name: (.*)', body)
            if m and m.group(1).strip() in names: roots.add(fid)
    tr_of_go = {g: tr for tr, g in go_of.items() if by.get(tr, ('',))[0] in ('4', '224')}
    keep_tr = {tr_of_go[g] for g in roots if g in tr_of_go}
    changed = True
    while changed:
        changed = False
        for tr, f in father.items():
            if f in keep_tr and tr not in keep_tr: keep_tr.add(tr); changed = True
        for pi, parent in pi_parent.items():
            if parent in keep_tr:
                for fid, owner in owner_pi.items():
                    if owner == pi and by[fid][0] in ('4', '224') and fid not in keep_tr:
                        keep_tr.add(fid); changed = True
    gos = {go_of[tr] for tr in keep_tr if tr in go_of}
    pis = {pi for pi, parent in pi_parent.items() if parent in keep_tr}
    ids = set(keep_tr) | gos | pis
    for fid, g in go_of.items():
        if g in gos: ids.add(fid)
    for fid, owner in owner_pi.items():
        if owner in pis: ids.add(fid)
    top = {tr for tr in keep_tr if father.get(tr, '0') == '0' and tr in go_of}
    return ids, top

base = subprocess.run(['git', 'show', ':' + path], capture_output=True, encoding='utf-8', check=True).stdout
work = open(path, encoding='utf-8', newline='').read().replace('\r\n', '\n')
bpre, bblocks = split(base)
_, wblocks = split(work)
bids, btop = subtree(bblocks)
wids, wtop = subtree(wblocks)
S = bids | wids
wmap = {fid: body for t, fid, body in wblocks}
present = {fid for _, fid, _ in bblocks}
out, added, changed, removed = [], 0, 0, 0
for t, fid, body in bblocks:
    if t == '1660057539':
        lines = body.rstrip('\n').split('\n')
        lines = [l for l in lines if not any('{fileID: %s}' % r in l for r in (btop - wtop))]
        existing = set(re.findall(r'\{fileID: (-?\d+)\}', '\n'.join(lines)))
        lines += ['    - {fileID: %s}' % r for r in sorted(wtop - btop) if r not in existing]
        news = [wmap[f] for f in sorted(S) if f not in present and f in wmap]
        added += len(news)
        out.extend(news)
        out.append('\n'.join(lines) + '\n')
        continue
    if fid in S:
        if fid in wmap:
            if wmap[fid] != body: changed += 1
            out.append(wmap[fid])
        else:
            removed += 1
        continue
    out.append(body)
result = bpre + ''.join(out)
sha = subprocess.run(['git', 'hash-object', '-w', '--stdin'], input=result.encode('utf-8'), capture_output=True, check=True).stdout.decode().strip()
subprocess.run(['git', 'update-index', '--cacheinfo', '100644,%s,%s' % (sha, path)], check=True)
print('names', sorted(names), 'added', added, 'changed', changed, 'removed', removed, 'roots+', sorted(wtop - btop), 'roots-', sorted(btop - wtop))
