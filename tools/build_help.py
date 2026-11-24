"""Build searchable local Help from the maintained User Guide manuscript."""
from pathlib import Path
from html import escape
import re
import shutil

root = Path(__file__).resolve().parents[1]
help_dir = root / 'help'
source = root / 'docs/guides/source'
old = (help_dir / 'index.html').read_text(encoding='utf-8')
suffix = old[old.index('</main>') + len('</main>'):]
keyword_old = (help_dir / 'keyword-index.html').read_text(encoding='utf-8')
keyword_suffix = keyword_old[keyword_old.index('</main>') + len('</main>'):]

def inline(text):
    text = escape(text)
    text = re.sub(r'\*\*(.+?)\*\*', r'<strong>\1</strong>', text)
    text = re.sub(r'`(.+?)`', r'<code>\1</code>', text)
    return text

nav = []
keywords = []
parts = []
list_tag = None
section = None
def close_list():
    global list_tag
    if list_tag:
        parts.append(f'</{list_tag}>')
        list_tag = None

for line in (source / 'User.md').read_text(encoding='utf-8-sig').splitlines():
    if not line.strip():
        close_list()
        continue
    if line.startswith('# '):
        close_list()
        if section is not None:
            parts.append('</article>')
        title = line[2:]
        section = 'fonts' if title == 'Choose a readable font' else f'section{len(nav)}'
        nav.append(f'<a href="#{section}">{inline(title)}</a>')
        parts.append(f'<article id="{section}"><h2>{inline(title)}</h2>')
        keywords.append(f'<p><a href="index.html#{section}">{inline(title)}</a></p>')
    elif line.startswith('## '):
        close_list()
        parts.append(f'<h3>{inline(line[3:])}</h3>')
        keywords.append(f'<p><a href="index.html#{section}">{inline(line[3:])}</a></p>')
    elif match := re.fullmatch(r'!\[(.*?)\]\((figures/[^)]+)\)', line):
        close_list()
        caption, relative = match.groups()
        destination = help_dir / 'assets' / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source / relative, destination)
        parts.append(f'<figure><img src="assets/{escape(relative)}" alt="{escape(caption)}" loading="lazy"><figcaption>{inline(caption)}</figcaption></figure>')
    elif match := re.match(r'^(?:([-*]) |(\d+)\. )(.*)', line):
        tag = 'ul' if match[1] else 'ol'
        if tag != list_tag:
            close_list()
            parts.append(f'<{tag}>')
            list_tag = tag
        parts.append(f'<li>{inline(match[3])}</li>')
    else:
        close_list()
        parts.append(f'<p>{inline(line)}</p>')
close_list()
parts.append('</article>')
body = ''.join(parts)
for product in ('Westminster Chimes and Carillon Bells', 'SQLite', 'NLayer', 'WPF'):
    body = ''.join(piece if piece.startswith('<') else piece.replace(product, f'<span class="notranslate" translate="no">{product}</span>') for piece in re.split(r'(<[^>]+>)', body))
head = '<!doctype html><html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Westminster Chimes and Carillon Bells 9.0 Help</title><link rel="stylesheet" href="assets/help.css"></head><body>'
page = head + '<aside><h1 class="notranslate" translate="no">Westminster Chimes and Carillon Bells 9.0</h1><p>Portable desktop help</p><label>Search help <input id="search" type="search"></label><nav>' + ''.join(nav) + '<a href="glossary.html">Glossary</a><a href="keyword-index.html">Keyword index</a></nav><button onclick="window.print()">Print manual</button></aside><main><h1>Westminster Chimes and Carillon Bells User Help</h1><p>Last changed November 24, 2026. Help opens locally. Internet access is needed for automatic translation.</p>' + body + '</main>' + suffix
(help_dir / 'index.html').write_text(page, encoding='utf-8')
(help_dir / 'keyword-index.html').write_text(head + '<main><a href="index.html">Back to help</a><h1>Keyword index</h1>' + ''.join(keywords) + '</main>' + keyword_suffix, encoding='utf-8')
print('Help and keyword index rebuilt from the User Guide; translation and glossary preserved.')
