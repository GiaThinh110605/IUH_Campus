# -*- coding: utf-8 -*-
"""
Generates Bao_Cao_Cuoi_Ky_IUH_Campus_Guardian.pdf matching ReportTemplate.pdf
(teacher's exact format) + all 7 academic rules (Times New Roman, pure black text,
numbered equations/figures/tables, footnotes, vector diagrams, two-pass exact TOC/LOF/LOT).
"""
import os
from xml.sax.saxutils import escape
from PIL import Image as PILImage
from reportlab.lib.pagesizes import A4
from reportlab.lib import colors
from reportlab.lib.units import cm
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.enums import TA_LEFT, TA_CENTER, TA_RIGHT, TA_JUSTIFY
from reportlab.platypus import (
    BaseDocTemplate, PageTemplate, Frame,
    Paragraph, Spacer, Table, TableStyle, PageBreak, HRFlowable, Image as RLImage
)
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.pdfgen import canvas

from report_spec import METADATA, SUMMARY, LIST_OF_FIGURES, LIST_OF_TABLES, REFERENCES
from report_chapters import CHAPTERS
from report_diagrams_pdf import build_pdf_figure

FONT_DIR = "/System/Library/Fonts/Supplemental"
pdfmetrics.registerFont(TTFont("TimesVN", os.path.join(FONT_DIR, "Times New Roman.ttf")))
pdfmetrics.registerFont(TTFont("TimesVN-Bold", os.path.join(FONT_DIR, "Times New Roman Bold.ttf")))
pdfmetrics.registerFont(TTFont("TimesVN-Italic", os.path.join(FONT_DIR, "Times New Roman Italic.ttf")))
pdfmetrics.registerFont(TTFont("TimesVN-BoldItalic", os.path.join(FONT_DIR, "Times New Roman Bold Italic.ttf")))
pdfmetrics.registerFontFamily(
    "TimesVN",
    normal="TimesVN",
    bold="TimesVN-Bold",
    italic="TimesVN-Italic",
    boldItalic="TimesVN-BoldItalic",
)


class TrackingDocTemplate(BaseDocTemplate):
    def __init__(self, filename, page_tracker, **kwargs):
        super().__init__(filename, **kwargs)
        self.page_tracker = page_tracker

    def afterFlowable(self, flowable):
        key = getattr(flowable, "_track_key", None)
        if key:
            # Page numbers start at 1 on the page right after Cover (Page 2 -> Trang 1)
            self.page_tracker[key] = str(max(1, self.page - 1))


class TemplateCanvas(canvas.Canvas):
    def __init__(self, *args, **kwargs):
        self._logo_path = kwargs.pop("logo_path", None)
        super().__init__(*args, **kwargs)
        self._saved_states = []

    def showPage(self):
        self._saved_states.append(dict(self.__dict__))
        self._startPage()

    def save(self):
        total = len(self._saved_states)
        for st in self._saved_states:
            self.__dict__.update(st)
            self._draw_chrome(total)
            canvas.Canvas.showPage(self)
        canvas.Canvas.save(self)

    def _draw_chrome(self, total_pages):
        p = self._pageNumber
        w, h = A4
        self.saveState()
        self.setFillColor(colors.black)
        self.setStrokeColor(colors.black)

        # Page 1: Cover page (no border box, matching ReportTemplate.pdf; footnote for IUH logo)
        if p == 1:
            self.setLineWidth(0.5)
            self.line(2.5 * cm, 2.55 * cm, 7.5 * cm, 2.55 * cm)
            self.setFont("TimesVN", 9.5)
            self.drawString(
                2.5 * cm,
                2.15 * cm,
                "1 Nguồn hình biểu trưng (Logo): Cổng thông tin Trường Đại học Công nghiệp TP. Hồ Chí Minh (https://iuh.edu.vn).",
            )
            self.restoreState()
            return

        # Content pages (Page 2 -> Trang 1 / N) matching ReportTemplate.pdf header & footer
        left_x = 2.5 * cm
        right_x = w - 2.0 * cm

        # Small logo + 2 lines header + rule
        text_x = left_x
        if self._logo_path and os.path.exists(self._logo_path):
            try:
                self.drawImage(self._logo_path, left_x, h - 1.72 * cm, width=0.85 * cm, height=0.85 * cm, preserveAspectRatio=True, mask="auto")
                text_x = left_x + 1.05 * cm
            except Exception:
                pass

        self.setFont("TimesVN", 9.5)
        self.drawString(text_x, h - 1.25 * cm, "Trường Đại học Công nghiệp TP. Hồ Chí Minh")
        self.drawString(text_x, h - 1.62 * cm, "Khoa Công nghệ Thông tin")
        self.setLineWidth(0.5)
        self.line(left_x, h - 1.82 * cm, right_x, h - 1.82 * cm)

        # Footer rule + course left + Trang X / Y right
        self.line(left_x, 1.80 * cm, right_x, 1.80 * cm)
        self.setFont("TimesVN", 9.5)
        self.drawString(left_x, 1.35 * cm, "Báo cáo đồ án môn học Công nghệ mới trong phát triển ứng dụng CNTT — Niên khóa 2026")
        self.drawRightString(right_x, 1.35 * cm, f"Trang {p - 1}/{total_pages - 1}")
        self.restoreState()


def make_styles():
    # Unified Times New Roman, 100% pure black text
    return {
        "cover_uni": ParagraphStyle("CoverUni", fontName="TimesVN", fontSize=13, leading=19, alignment=TA_CENTER, textColor=colors.black),
        "cover_course": ParagraphStyle("CoverCourse", fontName="TimesVN-Bold", fontSize=15, leading=21, alignment=TA_CENTER, textColor=colors.black, spaceBefore=6, spaceAfter=8),
        "cover_title": ParagraphStyle("CoverTitle", fontName="TimesVN-Bold", fontSize=14, leading=21, alignment=TA_CENTER, textColor=colors.black, spaceBefore=8, spaceAfter=8),
        "cover_tbl_l": ParagraphStyle("CoverTblL", fontName="TimesVN", fontSize=13, leading=19, alignment=TA_RIGHT, textColor=colors.black),
        "cover_tbl_r": ParagraphStyle("CoverTblR", fontName="TimesVN", fontSize=13, leading=19, alignment=TA_LEFT, textColor=colors.black),
        "cover_bottom": ParagraphStyle("CoverBottom", fontName="TimesVN", fontSize=12.5, leading=18, alignment=TA_CENTER, textColor=colors.black),
        "h1": ParagraphStyle("H1", fontName="TimesVN-Bold", fontSize=16, leading=22, alignment=TA_LEFT, textColor=colors.black, spaceBefore=6, spaceAfter=10, keepWithNext=True),
        "h2": ParagraphStyle("H2", fontName="TimesVN-Bold", fontSize=14, leading=19, alignment=TA_LEFT, textColor=colors.black, spaceBefore=12, spaceAfter=6, keepWithNext=True),
        "h3": ParagraphStyle("H3", fontName="TimesVN-Bold", fontSize=13, leading=18, alignment=TA_LEFT, textColor=colors.black, spaceBefore=8, spaceAfter=4, keepWithNext=True),
        "body": ParagraphStyle("Body", fontName="TimesVN", fontSize=13, leading=19, alignment=TA_JUSTIFY, firstLineIndent=0.7 * cm, spaceAfter=6, textColor=colors.black),
        "bullet": ParagraphStyle("Bullet", fontName="TimesVN", fontSize=13, leading=19, alignment=TA_JUSTIFY, leftIndent=0.8 * cm, firstLineIndent=-0.35 * cm, spaceAfter=5, textColor=colors.black),
        "eq": ParagraphStyle("Eq", fontName="TimesVN", fontSize=13, leading=18, alignment=TA_CENTER, textColor=colors.black),
        "eq_num": ParagraphStyle("EqNum", fontName="TimesVN", fontSize=13, leading=18, alignment=TA_RIGHT, textColor=colors.black),
        "fig_cap": ParagraphStyle("FigCap", fontName="TimesVN-Italic", fontSize=12, leading=16, alignment=TA_CENTER, spaceBefore=4, spaceAfter=10, textColor=colors.black),
        "tbl_cap": ParagraphStyle("TblCap", fontName="TimesVN-Bold", fontSize=12, leading=16, alignment=TA_LEFT, spaceBefore=8, spaceAfter=4, keepWithNext=True, textColor=colors.black),
        "th": ParagraphStyle("TH", fontName="TimesVN-Bold", fontSize=11.5, leading=15, alignment=TA_CENTER, textColor=colors.black),
        "td": ParagraphStyle("TD", fontName="TimesVN", fontSize=11.5, leading=15, alignment=TA_LEFT, textColor=colors.black),
        "toc_b": ParagraphStyle("TOCB", fontName="TimesVN-Bold", fontSize=13, leading=18, alignment=TA_LEFT, textColor=colors.black),
        "toc": ParagraphStyle("TOC", fontName="TimesVN", fontSize=13, leading=18, alignment=TA_LEFT, textColor=colors.black),
        "ref": ParagraphStyle("Ref", fontName="TimesVN", fontSize=13, leading=18, alignment=TA_JUSTIFY, leftIndent=0.8 * cm, firstLineIndent=-0.8 * cm, spaceAfter=6, textColor=colors.black),
    }


def format_equation_html(parts):
    out = []
    for txt, kind in parts:
        esc = escape(txt)
        if kind == "italic":
            out.append(f"<i>{esc}</i>")
        elif kind == "sub":
            out.append(f"<sub>{esc}</sub>")
        elif kind == "sup":
            out.append(f"<super>{esc}</super>")
        else:
            out.append(esc)
    return "".join(out)


def build_cover(story, st, logo_path, avail_w):
    story.append(Spacer(1, 0.8 * cm))
    story.append(Paragraph(METADATA["university"], st["cover_uni"]))
    story.append(Paragraph(METADATA["faculty"], st["cover_uni"]))
    story.append(Spacer(1, 1.4 * cm))

    if os.path.exists(logo_path):
        with PILImage.open(logo_path) as im:
            ow, oh = im.size
        lw = 3.2 * cm
        lh = lw * (oh / ow)
        img = RLImage(logo_path, width=lw, height=lh)
        img.hAlign = "CENTER"
        story.append(img)
        story.append(Paragraph("Biểu trưng Trường Đại học Công nghiệp TP. Hồ Chí Minh<super>1</super>", st["fig_cap"]))

    story.append(Spacer(1, 1.2 * cm))
    story.append(Paragraph(METADATA["course"], st["cover_course"]))
    story.append(HRFlowable(width="100%", thickness=0.7, color=colors.black, spaceBefore=2, spaceAfter=6))
    story.append(Paragraph(escape(METADATA["title"]).replace("\n", "<br/>"), st["cover_title"]))
    story.append(HRFlowable(width="100%", thickness=0.7, color=colors.black, spaceBefore=6, spaceAfter=18))

    story.append(Spacer(1, 1.0 * cm))
    sv_lines = "<br/>".join(f"<b>{escape(a['name'])}</b> — {escape(a['role'])}" for a in METADATA["authors"])
    info_tbl = Table(
        [
            [Paragraph("GVHD:", st["cover_tbl_l"]), Paragraph(f"<b>{escape(METADATA['advisor'])}</b>", st["cover_tbl_r"])],
            [Paragraph("SV thực hiện:", st["cover_tbl_l"]), Paragraph(sv_lines, st["cover_tbl_r"])],
        ],
        colWidths=[avail_w * 0.34, avail_w * 0.66],
    )
    info_tbl.setStyle(TableStyle([
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("TOPPADDING", (0, 0), (-1, -1), 3),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 3),
        ("LEFTPADDING", (0, 0), (-1, -1), 6),
        ("RIGHTPADDING", (0, 0), (-1, -1), 6),
    ]))
    story.append(info_tbl)
    story.append(Spacer(1, 2.8 * cm))
    story.append(Paragraph(METADATA["location_date"], st["cover_bottom"]))
    story.append(PageBreak())


def build_toc_table(rows, st, avail_w):
    data = []
    for title, page_lbl, is_bold in rows:
        p_style = st["toc_b"] if is_bold else st["toc"]
        p_r = ParagraphStyle("TOCR", parent=p_style, alignment=TA_RIGHT)
        data.append([Paragraph(escape(title), p_style), Paragraph(escape(page_lbl or ""), p_r)])
    t = Table(data, colWidths=[avail_w - 1.4 * cm, 1.4 * cm])
    t.setStyle(TableStyle([
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("TOPPADDING", (0, 0), (-1, -1), 2),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 3),
        ("LEFTPADDING", (0, 0), (-1, -1), 0),
        ("RIGHTPADDING", (0, 0), (-1, -1), 0),
    ]))
    return t


def build_story(st, page_map, base_dir):
    story = []
    avail_w = A4[0] - 4.5 * cm  # left 2.5cm, right 2.0cm matching ReportTemplate
    logo_path = os.path.join(base_dir, "iuh_logo.jpg")

    # 1. Cover page (Page 1)
    build_cover(story, st, logo_path, avail_w)

    # 2. Tóm tắt (Trang 1, KHÔNG hình/bảng, đủ 4 phần)
    p_sum = Paragraph("Tóm tắt", st["h1"])
    p_sum._track_key = "summary"
    story.append(p_sum)
    for hd, body_txt in SUMMARY:
        story.append(Paragraph(f"<b>{escape(hd)}:</b> {escape(body_txt)}", st["body"]))
    story.append(PageBreak())

    # 3. Mục lục + Danh mục Hình + Danh mục Bảng
    p_toc = Paragraph("Mục lục", st["h1"])
    p_toc._track_key = "toc"
    story.append(p_toc)
    toc_rows = [("Tóm tắt", page_map.get("summary", "1"), True)]
    for ch in CHAPTERS:
        ch_key = f"ch_{ch['num']}"
        toc_rows.append((ch["title"], page_map.get(ch_key, ""), True))
        for idx, blk in enumerate(ch["blocks"]):
            if blk["type"] == "h2":
                toc_rows.append(("   " + blk["text"], page_map.get(f"{ch_key}_{idx}", ""), False))
    toc_rows.append(("Tài liệu tham khảo", page_map.get("refs", ""), True))
    story.append(build_toc_table(toc_rows, st, avail_w))
    story.append(Spacer(1, 0.5 * cm))

    p_lof = Paragraph("Danh mục các hình vẽ", st["h2"])
    p_lof._track_key = "lof"
    story.append(p_lof)
    lof_rows = [(f"{num}: {desc}", page_map.get(f"fig_{num}", ""), False) for num, desc in LIST_OF_FIGURES]
    story.append(build_toc_table(lof_rows, st, avail_w))
    story.append(Spacer(1, 0.4 * cm))

    p_lot = Paragraph("Danh mục các bảng", st["h2"])
    p_lot._track_key = "lot"
    story.append(p_lot)
    lot_rows = [(f"{num}: {desc}", page_map.get(f"tbl_{num}", ""), False) for num, desc in LIST_OF_TABLES]
    story.append(build_toc_table(lot_rows, st, avail_w))
    story.append(PageBreak())

    # 4. Sections 1 through 9 (matching ReportTemplate.pdf flow)
    for ch in CHAPTERS:
        ch_key = f"ch_{ch['num']}"
        p_ch = Paragraph(escape(ch["title"]), st["h1"])
        p_ch._track_key = ch_key
        story.append(p_ch)

        for idx, blk in enumerate(ch["blocks"]):
            btype = blk["type"]
            if btype == "h2":
                p = Paragraph(escape(blk["text"]), st["h2"])
                p._track_key = f"{ch_key}_{idx}"
                story.append(p)
            elif btype == "h3":
                p = Paragraph(escape(blk["text"]), st["h3"])
                p._track_key = f"{ch_key}_{idx}"
                story.append(p)
            elif btype == "para":
                story.append(Paragraph(escape(blk["text"]), st["body"]))
            elif btype == "bullets":
                for item in blk["items"]:
                    story.append(Paragraph(f"• {escape(item)}", st["bullet"]))
            elif btype == "equation":
                eq_html = format_equation_html(blk["parts"])
                eq_tbl = Table(
                    [[Paragraph(eq_html, st["eq"]), Paragraph(escape(blk["number"]), st["eq_num"])]],
                    colWidths=[avail_w - 1.8 * cm, 1.8 * cm],
                )
                eq_tbl.setStyle(TableStyle([
                    ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
                    ("TOPPADDING", (0, 0), (-1, -1), 4),
                    ("BOTTOMPADDING", (0, 0), (-1, -1), 6),
                ]))
                story.append(eq_tbl)
            elif btype == "figure":
                fig_num = blk["caption"].split(":")[0].strip()
                drawing = build_pdf_figure(blk["id"], width=avail_w)
                if drawing is not None:
                    drawing.hAlign = "CENTER"
                    story.append(Spacer(1, 4))
                    story.append(drawing)
                p_cap = Paragraph(escape(blk["caption"]), st["fig_cap"])
                p_cap._track_key = f"fig_{fig_num}"
                story.append(p_cap)
            elif btype == "table":
                tbl_num = blk["caption"].split(":")[0].strip()
                p_cap = Paragraph(escape(blk["caption"]), st["tbl_cap"])
                p_cap._track_key = f"tbl_{tbl_num}"
                story.append(p_cap)
                col_ws = [avail_w * r for r in blk["col_ratios"]]
                tdata = [[Paragraph(escape(h), st["th"]) for h in blk["headers"]]]
                for r in blk["rows"]:
                    tdata.append([Paragraph(escape(c), st["td"]) for c in r])
                t = Table(tdata, colWidths=col_ws, repeatRows=1)
                t.setStyle(TableStyle([
                    ("GRID", (0, 0), (-1, -1), 0.5, colors.black),
                    ("VALIGN", (0, 0), (-1, -1), "TOP"),
                    ("TOPPADDING", (0, 0), (-1, -1), 4),
                    ("BOTTOMPADDING", (0, 0), (-1, -1), 4),
                    ("LEFTPADDING", (0, 0), (-1, -1), 5),
                    ("RIGHTPADDING", (0, 0), (-1, -1), 5),
                ]))
                story.append(t)
                story.append(Spacer(1, 8))

    # 5. Tài liệu tham khảo (1 -> 10)
    p_ref = Paragraph("Tài liệu tham khảo", st["h1"])
    p_ref._track_key = "refs"
    story.append(p_ref)
    for num, ref_txt in REFERENCES:
        story.append(Paragraph(f"[{num}] {escape(ref_txt)}", st["ref"]))

    return story


def create_report_pdf(pdf_path, base_dir):
    st = make_styles()
    page_map = {}
    logo_path = os.path.join(base_dir, "iuh_logo.jpg")

    def run_pass():
        tracker = {}
        doc = TrackingDocTemplate(
            pdf_path, tracker,
            pagesize=A4,
            leftMargin=2.5 * cm, rightMargin=2.0 * cm,
            topMargin=2.25 * cm, bottomMargin=2.25 * cm,
        )
        frame = Frame(doc.leftMargin, doc.bottomMargin, doc.width, doc.height, id="normal")
        doc.addPageTemplates([PageTemplate(id="main", frames=[frame])])
        story = build_story(st, page_map, base_dir)
        doc.build(story, canvasmaker=lambda *args, **kw: TemplateCanvas(*args, logo_path=logo_path, **kw))
        return tracker

    page_map.update(run_pass())
    run_pass()
    print(f"Generated PDF matching ReportTemplate.pdf: {pdf_path}")


if __name__ == "__main__":
    base = "/Users/mac/Unity/IUH_Campus/report_final"
    create_report_pdf(os.path.join(base, "Bao_Cao_Cuoi_Ky_IUH_Campus_Guardian.pdf"), base)
