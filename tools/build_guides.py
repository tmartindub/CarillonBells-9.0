"""Build the editable Westminster Chimes and Carillon Bells guide drafts."""

import argparse
from datetime import datetime
from pathlib import Path
import re

from docx import Document
from docx.enum.style import WD_STYLE_TYPE
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_TAB_ALIGNMENT, WD_TAB_LEADER
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Emu, Inches, Pt, RGBColor

PROJECT = Path(__file__).resolve().parents[1]
SOURCES = PROJECT / "docs" / "guides" / "source"
PRODUCT_NAME = "Westminster Chimes and Carillon Bells"
PRODUCT_VERSION = "9.0"
GUIDES = {
    "User": ("User Guide", "Set up a portable installation, manage the music and schedules, and check daily operation.", datetime(2026, 11, 23)),
    "Engineering": ("Engineering Guide", "Maintain the v9 Windows application, understand its runtime and data model, and verify a portable release.", datetime(2026, 11, 23)),
    "Migration": ("Database Migration Guide", "Upgrade a 7.2.4 or 8.1 installation, restore a 7.2.4 database, or migrate to a separate folder.", datetime(2026, 11, 23)),
    "QuickStart": ("Quick Start Guide", "Set up the app, play a test recording, and verify your first schedule.", datetime(2026, 11, 23)),
}


def add_field(paragraph, instruction, initial_text):
    for field_type in ("begin", "instruction", "separate", "result", "end"):
        if field_type == "result":
            paragraph.add_run(initial_text)
            continue
        if field_type == "instruction":
            element = OxmlElement("w:instrText")
            element.set(qn("xml:space"), "preserve")
            element.text = f" {instruction} "
        else:
            element = OxmlElement("w:fldChar")
            element.set(qn("w:fldCharType"), field_type)
        paragraph.add_run()._r.append(element)


def add_inline(paragraph, text):
    for segment in re.split(r"(\*\*[^*]+\*\*|\x60[^\x60]+\x60)", text):
        if segment.startswith("**") and segment.endswith("**"):
            paragraph.add_run(segment[2:-2]).bold = True
        elif segment.startswith("\x60") and segment.endswith("\x60"):
            paragraph.add_run(segment[1:-1]).font.name = "Consolas"
        else:
            paragraph.add_run(segment)


def add_figure(document, path, caption):
    paragraph = document.add_paragraph()
    paragraph.alignment = WD_ALIGN_PARAGRAPH.CENTER
    paragraph.paragraph_format.line_spacing = 1
    paragraph.paragraph_format.space_before = Pt(4)
    paragraph.paragraph_format.space_after = Pt(3)
    paragraph.paragraph_format.keep_with_next = True
    picture = paragraph.add_run().add_picture(str(path))
    scale = min(1, Inches(6.6) / picture.width, Inches(3.6) / picture.height)
    picture.width = Emu(round(picture.width * scale))
    picture.height = Emu(round(picture.height * scale))
    picture._inline.docPr.set("descr", caption)
    document.add_paragraph(caption, style="Caption")


def add_manuscript(document, source, *, quick_start=False):
    first_heading = True
    for line in source.read_text(encoding="utf-8-sig").splitlines():
        text = line.strip()
        if not text:
            continue
        if text.startswith(("## ", "# ")):
            level = 2 if text.startswith("## ") else 1
            paragraph = document.add_heading(text.removeprefix("#" * level + " "), level)
            starts_page = (level == 1 and not first_heading) if quick_start else first_heading
            if starts_page:
                paragraph.paragraph_format.page_break_before = True
            first_heading = False
            continue
        figure = re.fullmatch(r"!\[([^\]]+)\]\((figures/[^)]+\.png)\)", text)
        if figure:
            caption, path = figure.groups()
            add_figure(document, source.parent / path, caption)
            continue
        item = re.fullmatch(r"(\d+)\.\s+(.*)", text)
        if item:
            paragraph = document.add_paragraph(style="Procedure step")
            paragraph.add_run(f"{item[1]}.\t")
            add_inline(paragraph, item[2])
        elif text.startswith("- "):
            add_inline(document.add_paragraph(style="List Bullet"), text[2:])
        else:
            add_inline(document.add_paragraph(), text)


def make_guide(kind, destination):
    title, purpose, last_changed = GUIDES[kind]
    quick_start = kind == "QuickStart"
    document = Document()
    section = document.sections[0]
    section.page_width = Inches(8.5)
    section.page_height = Inches(11)
    section.top_margin = Inches(.76)
    section.bottom_margin = Inches(.72)
    section.left_margin = section.right_margin = Inches(.80)
    section.footer_distance = Inches(.34)
    section.different_first_page_header_footer = True

    styles = document.styles
    for border in styles.element.xpath(".//w:pBdr"):
        border.getparent().remove(border)
    for name in ("Normal", "Title", "Subtitle", "Heading 1", "Heading 2", "List Bullet", "Footer"):
        style = styles[name]
        style.font.name = "Calibri"
        style.font.color.rgb = RGBColor(0, 0, 0)
    normal = styles["Normal"]
    normal.font.size = Pt(11)
    normal.paragraph_format.line_spacing = 1.16
    normal.paragraph_format.space_after = Pt(7)
    normal.paragraph_format.widow_control = True
    styles["Title"].font.size = Pt(30)
    styles["Title"].font.bold = True
    styles["Title"].paragraph_format.space_after = Pt(8)
    styles["Subtitle"].font.size = Pt(25)
    styles["Subtitle"].paragraph_format.space_after = Pt(12 if quick_start else 24)

    for name, size, before, after in (("Heading 1", 21, 20, 6), ("Heading 2", 13, 12, 4)):
        style = styles[name]
        style.font.size = Pt(size)
        style.font.bold = True
        style.paragraph_format.space_before = Pt(before)
        style.paragraph_format.space_after = Pt(after)
        style.paragraph_format.keep_with_next = True
    contents = styles.add_style("Guide contents title", WD_STYLE_TYPE.PARAGRAPH)
    contents.base_style = normal
    contents.font.size = Pt(24)
    contents.font.bold = True
    contents.paragraph_format.keep_with_next = True
    contents.paragraph_format.space_after = Pt(14)
    for level in (1, 2):
        name = f"TOC {level}"
        style = styles[name] if name in styles else styles.add_style(name, WD_STYLE_TYPE.PARAGRAPH)
        style.base_style = normal
        style.font.name = "Calibri"
        style.font.size = Pt(11)
        style.font.color.rgb = RGBColor(0, 0, 0)
        style.font.bold = level == 1
        style.paragraph_format.left_indent = Inches(0 if level == 1 else .22)
        style.paragraph_format.line_spacing = 1.05
        style.paragraph_format.space_before = Pt(4 if level == 1 else 0)
        style.paragraph_format.space_after = Pt(3)
        style.paragraph_format.tab_stops.add_tab_stop(Inches(6.90), WD_TAB_ALIGNMENT.RIGHT, WD_TAB_LEADER.DOTS)

    steps = styles.add_style("Procedure step", WD_STYLE_TYPE.PARAGRAPH)
    steps.base_style = normal
    steps.paragraph_format.left_indent = Inches(.29)
    steps.paragraph_format.first_line_indent = Inches(-.29)
    steps.paragraph_format.tab_stops.add_tab_stop(Inches(.29))
    steps.paragraph_format.space_after = Pt(5)
    styles["List Bullet"].paragraph_format.space_after = Pt(5)

    caption = styles["Caption"]
    caption.base_style = normal
    caption.font.name = "Calibri"
    caption.font.size = Pt(10)
    caption.font.color.rgb = RGBColor(0, 0, 0)
    caption.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.CENTER
    caption.paragraph_format.space_before = Pt(0)
    caption.paragraph_format.space_after = Pt(7)
    caption.paragraph_format.keep_together = True
    caption.paragraph_format.keep_with_next = False

    cover = document.add_paragraph(PRODUCT_NAME, "Title")
    cover.paragraph_format.space_before = Pt(22 if quick_start else 80)
    document.add_paragraph(f"Version {PRODUCT_VERSION}")
    document.add_paragraph(title, "Subtitle")
    document.add_paragraph(purpose)
    document.add_paragraph(f"Last changed {last_changed:%B %d, %Y}")
    if not quick_start:
        document.add_page_break()
    document.add_paragraph("Table of contents", "Guide contents title")
    add_field(document.add_paragraph(), 'TOC \\o "1-2" \\h \\z \\u', "Update this table of contents in Writer.")

    add_manuscript(document, SOURCES / f"{kind}.md", quick_start=quick_start)
    footer = section.footer.paragraphs[0]
    footer.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    footer.add_run("Page ").font.size = Pt(9)
    add_field(footer, "PAGE", "2")

    properties = document.core_properties
    properties.title = f"{PRODUCT_NAME} {title}"
    properties.subject = purpose
    properties.author = f"{PRODUCT_NAME} Project"
    properties.keywords = f"{PRODUCT_NAME}, version {PRODUCT_VERSION}, portable Windows application"
    properties.version = PRODUCT_VERSION
    properties.modified = last_changed
    document.save(destination)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("output", type=Path, help="Directory for DOCX drafts before updating the contents in Writer")
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=True)
    for kind, (title, *_) in GUIDES.items():
        filename = f"CarillonBells_9_0_{title.replace(' ', '_')}.docx"
        destination = args.output / filename
        make_guide(kind, destination)
        print(destination)


if __name__ == "__main__":
    main()
