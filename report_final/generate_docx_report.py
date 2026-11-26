# -*- coding: utf-8 -*-
"""
Generates Bao_Cao_Cuoi_Ky_IUH_Campus_Guardian.docx matching ReportTemplate.pdf
(teacher's exact format) + all 7 academic rules (Times New Roman, pure black text,
numbered equations/figures/tables, Word VML vector diagrams — zero pasted diagram images).
"""
import os
from docx import Document
from docx.shared import Cm, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.enum.section import WD_SECTION
from docx.oxml import OxmlElement, parse_xml
from docx.oxml.ns import nsdecls, qn

from report_spec import METADATA, SUMMARY, LIST_OF_FIGURES, LIST_OF_TABLES, REFERENCES
from report_chapters import CHAPTERS
from report_diagrams_docx import add_docx_vector_figure

BLACK = RGBColor(0, 0, 0)


def _set_run_font(run, size_pt=13, bold=False, italic=False):
    run.font.name = "Times New Roman"
    run.font.size = Pt(size_pt)
    run.font.bold = bold
    run.font.italic = italic
    run.font.color.rgb = BLACK
    rPr = run._r.get_or_add_rPr()
    rFonts = rPr.find(qn("w:rFonts"))
    if rFonts is None:
        rFonts = OxmlElement("w:rFonts")
        rPr.insert(0, rFonts)
    rFonts.set(qn("w:ascii"), "Times New Roman")
    rFonts.set(qn("w:hAnsi"), "Times New Roman")
    rFonts.set(qn("w:cs"), "Times New Roman")


def _add_field(paragraph, field_name):
    run = paragraph.add_run()
    _set_run_font(run, size_pt=9.5)
    run._r.append(parse_xml(r'<w:fldChar %s w:fldCharType="begin"/>' % nsdecls("w")))
    run._r.append(parse_xml(r'<w:instrText %s xml:space="preserve"> %s </w:instrText>' % (nsdecls("w"), field_name)))
    run._r.append(parse_xml(r'<w:fldChar %s w:fldCharType="separate"/>' % nsdecls("w")))
    run._r.append(parse_xml(r'<w:fldChar %s w:fldCharType="end"/>' % nsdecls("w")))


def _set_paragraph_bottom_border(paragraph):
    pPr = paragraph._p.get_or_add_pPr()
    pBdr = parse_xml(f'<w:pBdr {nsdecls("w")}><w:bottom w:val="single" w:sz="4" w:space="4" w:color="000000"/></w:pBdr>')
    pPr.append(pBdr)


def _set_paragraph_top_border(paragraph):
    pPr = paragraph._p.get_or_add_pPr()
    pBdr = parse_xml(f'<w:pBdr {nsdecls("w")}><w:top w:val="single" w:sz="4" w:space="4" w:color="000000"/></w:pBdr>')
    pPr.append(pBdr)


def _set_table_borders(tbl, visible=True):
    tblPr = tbl._tbl.tblPr
    val = "single" if visible else "none"
    sz = "4" if visible else "0"
    borders = parse_xml(
        f'<w:tblBorders {nsdecls("w")}>'
        f'<w:top w:val="{val}" w:sz="{sz}" w:space="0" w:color="000000"/>'
        f'<w:left w:val="{val}" w:sz="{sz}" w:space="0" w:color="000000"/>'
        f'<w:bottom w:val="{val}" w:sz="{sz}" w:space="0" w:color="000000"/>'
        f'<w:right w:val="{val}" w:sz="{sz}" w:space="0" w:color="000000"/>'
        f'<w:insideH w:val="{val}" w:sz="{sz}" w:space="0" w:color="000000"/>'
        f'<w:insideV w:val="{val}" w:sz="{sz}" w:space="0" w:color="000000"/>'
        f'</w:tblBorders>'
    )
    tblPr.append(borders)


def _set_cell_margins(cell, top=55, bottom=55, left=85, right=85):
    tcPr = cell._tc.get_or_add_tcPr()
    tcPr.append(parse_xml(
        f'<w:tcMar {nsdecls("w")}>'
        f'<w:top w:w="{top}" w:type="dxa"/><w:bottom w:w="{bottom}" w:type="dxa"/>'
        f'<w:left w:w="{left}" w:type="dxa"/><w:right w:w="{right}" w:type="dxa"/>'
        f'</w:tcMar>'
    ))


def _configure_content_section(sec, logo_path):
    sec.page_width = Cm(21.0)
    sec.page_height = Cm(29.7)
    sec.left_margin = Cm(2.5)
    sec.right_margin = Cm(2.0)
    sec.top_margin = Cm(2.25)
    sec.bottom_margin = Cm(2.25)
    sec.header.is_linked_to_previous = False
    sec.footer.is_linked_to_previous = False

    sectPr = sec._sectPr
    pgNumType = OxmlElement("w:pgNumType")
    pgNumType.set(qn("w:fmt"), "decimal")
    pgNumType.set(qn("w:start"), "1")
    sectPr.append(pgNumType)

    # Header matching ReportTemplate.pdf
    hp = sec.header.paragraphs[0]
    hp.alignment = WD_ALIGN_PARAGRAPH.LEFT
    if os.path.exists(logo_path):
        hp.add_run().add_picture(logo_path, width=Cm(0.75))
        _set_run_font(hp.add_run("   "), size_pt=9.5)
    _set_run_font(hp.add_run("Trường Đại học Công nghiệp TP. Hồ Chí Minh — Khoa Công nghệ Thông tin"), size_pt=9.5)
    _set_paragraph_bottom_border(hp)

    # Footer matching ReportTemplate.pdf
    fp = sec.footer.paragraphs[0]
    fp.alignment = WD_ALIGN_PARAGRAPH.LEFT
    _set_paragraph_top_border(fp)
    _set_run_font(fp.add_run("Báo cáo đồ án môn học Công nghệ mới trong phát triển ứng dụng CNTT — Niên khóa 2026\tTrang "), size_pt=9.5)
    _add_field(fp, "PAGE")


def _add_cover_page(doc, logo_path):
    sec0 = doc.sections[0]
    sec0.page_width = Cm(21.0)
    sec0.page_height = Cm(29.7)
    sec0.left_margin = Cm(2.5)
    sec0.right_margin = Cm(2.0)
    sec0.top_margin = Cm(2.2)
    sec0.bottom_margin = Cm(2.2)
    sec0.header.is_linked_to_previous = False
    sec0.footer.is_linked_to_previous = False
    fp = sec0.footer.paragraphs[0]
    fp.alignment = WD_ALIGN_PARAGRAPH.LEFT
    _set_paragraph_top_border(fp)
    _set_run_font(fp.add_run("1 Nguồn hình biểu trưng (Logo): Cổng thông tin Trường Đại học Công nghiệp TP. Hồ Chí Minh (https://iuh.edu.vn)."), size_pt=9.5)

    for txt in (METADATA["university"], METADATA["faculty"]):
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.space_after = Pt(2)
        _set_run_font(p.add_run(txt), size_pt=13)

    if os.path.exists(logo_path):
        p_img = doc.add_paragraph()
        p_img.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_img.paragraph_format.space_before = Pt(24)
        p_img.paragraph_format.space_after = Pt(4)
        p_img.add_run().add_picture(logo_path, width=Cm(3.2))

        p_cap = doc.add_paragraph()
        p_cap.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p_cap.paragraph_format.space_after = Pt(24)
        _set_run_font(p_cap.add_run("Biểu trưng Trường Đại học Công nghiệp TP. Hồ Chí Minh"), size_pt=12, italic=True)
        rs = p_cap.add_run("1")
        _set_run_font(rs, size_pt=10, italic=True)
        rs.font.superscript = True

    p_cs = doc.add_paragraph()
    p_cs.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_cs.paragraph_format.space_after = Pt(6)
    _set_paragraph_bottom_border(p_cs)
    _set_run_font(p_cs.add_run(METADATA["course"]), size_pt=15, bold=True)

    p_tt = doc.add_paragraph()
    p_tt.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_tt.paragraph_format.space_before = Pt(8)
    p_tt.paragraph_format.space_after = Pt(28)
    _set_paragraph_bottom_border(p_tt)
    _set_run_font(p_tt.add_run(METADATA["title"]), size_pt=14, bold=True)

    tbl = doc.add_table(rows=2, cols=2)
    tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    _set_table_borders(tbl, visible=False)
    for row_idx, (lbl, val) in enumerate([
        ("GVHD:", METADATA["advisor"]),
        ("SV thực hiện:", "\n".join(f"{a['name']} — {a['role']}" for a in METADATA["authors"])),
    ]):
        c0, c1 = tbl.cell(row_idx, 0), tbl.cell(row_idx, 1)
        c0.width, c1.width = Cm(5.2), Cm(11.3)
        p0 = c0.paragraphs[0]
        p0.alignment = WD_ALIGN_PARAGRAPH.RIGHT
        _set_run_font(p0.add_run(lbl), size_pt=13)
        p1 = c1.paragraphs[0]
        p1.alignment = WD_ALIGN_PARAGRAPH.LEFT
        _set_run_font(p1.add_run(val), size_pt=13, bold=(row_idx == 0))

    p_loc = doc.add_paragraph()
    p_loc.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p_loc.paragraph_format.space_before = Pt(64)
    _set_run_font(p_loc.add_run(METADATA["location_date"]), size_pt=12.5)


def _add_h1(doc, text):
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(8)
    p.paragraph_format.space_after = Pt(8)
    p.paragraph_format.keep_with_next = True
    _set_run_font(p.add_run(text), size_pt=16, bold=True)
    return p


def _add_h2(doc, text):
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(12)
    p.paragraph_format.space_after = Pt(6)
    p.paragraph_format.keep_with_next = True
    _set_run_font(p.add_run(text), size_pt=14, bold=True)
    return p


def _add_body(doc, text, indent=True):
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    if indent:
        p.paragraph_format.first_line_indent = Cm(0.7)
    p.paragraph_format.line_spacing = 1.3
    p.paragraph_format.space_after = Pt(6)
    _set_run_font(p.add_run(text), size_pt=13)
    return p


def _add_bullet(doc, text):
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    p.paragraph_format.left_indent = Cm(0.8)
    p.paragraph_format.first_line_indent = Cm(-0.35)
    p.paragraph_format.line_spacing = 1.3
    p.paragraph_format.space_after = Pt(5)
    _set_run_font(p.add_run("• " + text), size_pt=13)


def _add_equation(doc, parts, eq_num):
    tbl = doc.add_table(rows=1, cols=2)
    tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    _set_table_borders(tbl, visible=False)
    c0, c1 = tbl.cell(0, 0), tbl.cell(0, 1)
    c0.width, c1.width = Cm(14.5), Cm(2.0)
    p0 = c0.paragraphs[0]
    p0.alignment = WD_ALIGN_PARAGRAPH.CENTER
    for txt, kind in parts:
        r = p0.add_run(txt)
        if kind == "italic":
            _set_run_font(r, size_pt=13, italic=True)
        elif kind == "sub":
            _set_run_font(r, size_pt=13)
            r.font.subscript = True
        elif kind == "sup":
            _set_run_font(r, size_pt=13)
            r.font.superscript = True
        else:
            _set_run_font(r, size_pt=13)
    p1 = c1.paragraphs[0]
    p1.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    _set_run_font(p1.add_run(eq_num), size_pt=13)


def _add_table(doc, headers, rows, col_ratios, caption):
    pcap = doc.add_paragraph()
    pcap.paragraph_format.space_before = Pt(8)
    pcap.paragraph_format.space_after = Pt(4)
    pcap.paragraph_format.keep_with_next = True
    _set_run_font(pcap.add_run(caption), size_pt=12, bold=True)

    tbl = doc.add_table(rows=1 + len(rows), cols=len(headers))
    tbl.alignment = WD_TABLE_ALIGNMENT.CENTER
    _set_table_borders(tbl, visible=True)
    total_w = 16.5
    for r_idx, row in enumerate([headers] + rows):
        for c_idx, val in enumerate(row):
            cell = tbl.cell(r_idx, c_idx)
            cell.width = Cm(total_w * col_ratios[c_idx])
            _set_cell_margins(cell)
            cp = cell.paragraphs[0]
            cp.alignment = WD_ALIGN_PARAGRAPH.CENTER if r_idx == 0 else WD_ALIGN_PARAGRAPH.LEFT
            cp.paragraph_format.space_before = Pt(1)
            cp.paragraph_format.space_after = Pt(1)
            cp.paragraph_format.line_spacing = 1.15
            _set_run_font(cp.add_run(val), size_pt=11.5, bold=(r_idx == 0))
    doc.add_paragraph().paragraph_format.space_after = Pt(4)


def create_report_docx(docx_path, base_dir):
    doc = Document()
    normal = doc.styles["Normal"]
    normal.font.name = "Times New Roman"
    normal.font.size = Pt(13)
    normal.font.color.rgb = BLACK

    logo_path = os.path.join(base_dir, "iuh_logo.jpg")
    _add_cover_page(doc, logo_path)

    sec1 = doc.add_section(WD_SECTION.NEW_PAGE)
    _configure_content_section(sec1, logo_path)

    # Tóm tắt (4 nội dung bắt buộc, không hình/bảng)
    _add_h1(doc, "Tóm tắt")
    for hd, body_txt in SUMMARY:
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
        p.paragraph_format.first_line_indent = Cm(0.7)
        p.paragraph_format.line_spacing = 1.3
        p.paragraph_format.space_after = Pt(6)
        _set_run_font(p.add_run(hd + ": "), size_pt=13, bold=True)
        _set_run_font(p.add_run(body_txt), size_pt=13)
    doc.add_page_break()

    # Mục lục + Danh mục Hình vẽ + Danh mục Bảng
    _add_h1(doc, "Mục lục")
    p_s = _add_body(doc, "Tóm tắt", indent=False)
    for r in p_s.runs:
        r.bold = True
    for ch in CHAPTERS:
        p = _add_body(doc, ch["title"], indent=False)
        p.paragraph_format.space_after = Pt(2)
        for r in p.runs:
            r.bold = True
        for blk in ch["blocks"]:
            if blk["type"] == "h2":
                ps = _add_body(doc, "   " + blk["text"], indent=False)
                ps.paragraph_format.space_after = Pt(2)
    p_r = _add_body(doc, "Tài liệu tham khảo", indent=False)
    for r in p_r.runs:
        r.bold = True

    _add_h2(doc, "Danh mục các hình vẽ")
    for num, desc in LIST_OF_FIGURES:
        _add_body(doc, f"{num}: {desc}", indent=False).paragraph_format.space_after = Pt(2)

    _add_h2(doc, "Danh mục các bảng")
    for num, desc in LIST_OF_TABLES:
        _add_body(doc, f"{num}: {desc}", indent=False).paragraph_format.space_after = Pt(2)
    doc.add_page_break()

    # Sections 1..9
    for ch in CHAPTERS:
        _add_h1(doc, ch["title"])
        for blk in ch["blocks"]:
            t = blk["type"]
            if t == "h2":
                _add_h2(doc, blk["text"])
            elif t == "para":
                _add_body(doc, blk["text"])
            elif t == "bullets":
                for it in blk["items"]:
                    _add_bullet(doc, it)
            elif t == "equation":
                _add_equation(doc, blk["parts"], blk["number"])
            elif t == "figure":
                add_docx_vector_figure(doc, blk["id"])
                pcap = doc.add_paragraph()
                pcap.alignment = WD_ALIGN_PARAGRAPH.CENTER
                pcap.paragraph_format.space_before = Pt(4)
                pcap.paragraph_format.space_after = Pt(10)
                _set_run_font(pcap.add_run(blk["caption"]), size_pt=12, italic=True)
            elif t == "table":
                _add_table(doc, blk["headers"], blk["rows"], blk["col_ratios"], blk["caption"])

    _add_h1(doc, "Tài liệu tham khảo")
    for num, ref_txt in REFERENCES:
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
        p.paragraph_format.left_indent = Cm(0.8)
        p.paragraph_format.first_line_indent = Cm(-0.8)
        p.paragraph_format.line_spacing = 1.3
        p.paragraph_format.space_after = Pt(6)
        _set_run_font(p.add_run(f"[{num}] {ref_txt}"), size_pt=13)

    doc.save(docx_path)
    print(f"Generated DOCX matching ReportTemplate.pdf: {docx_path}")


if __name__ == "__main__":
    base = "/Users/mac/Unity/IUH_Campus/report_final"
    create_report_docx(os.path.join(base, "Bao_Cao_Cuoi_Ky_IUH_Campus_Guardian.docx"), base)
