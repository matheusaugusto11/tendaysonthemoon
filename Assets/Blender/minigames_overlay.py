"""Desenha o HUD e a mecanica de cada minigame sobre os fundos 3D (minigames_render.py).
Uso: python Assets/Blender/minigames_overlay.py <pasta_com_bg_*.png_e_anch_*.json> [pasta_saida=Docs/minigames]
Estilo do HUD copiado do jogo web (paineis azul-marinho, barra laranja, chips).
"""
import sys, os, json, math
from PIL import Image, ImageDraw, ImageFont, ImageFilter

SRC = sys.argv[1]
ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
DST = sys.argv[2] if len(sys.argv) > 2 else os.path.join(ROOT, "Docs", "minigames")
os.makedirs(DST, exist_ok=True)
W, H, S = 1600, 900, 2
FD = r"C:\Windows\Fonts"
def F(size, bold=False, sym=False):
    f = "seguisym.ttf" if sym else ("segoeuib.ttf" if bold else "segoeui.ttf")
    return ImageFont.truetype(os.path.join(FD, f), int(size * S))
NAVY = (14, 19, 38, 232); LINE = (58, 69, 120, 255); ORANGE = (255, 159, 67, 255); CYAN = (77, 208, 225, 255); GREEN = (107, 212, 37, 255)
YELLOW = (255, 210, 63, 255); RED = (235, 70, 60, 255); WHITE = (255, 255, 255, 255); MUTED = (170, 182, 214, 255); PURPLE = (199, 125, 255, 255)

class Cv:
    def __init__(self):
        self.im = Image.new("RGBA", (W * S, H * S), (0, 0, 0, 0)); self.d = ImageDraw.Draw(self.im, "RGBA")
    def _p(self, pts): return [(x * S, y * S) for x, y in pts]
    def rect(self, x0, y0, x1, y1, fill=NAVY, outline=None, r=18, w=3):
        self.d.rounded_rectangle([x0 * S, y0 * S, x1 * S, y1 * S], radius=r * S, fill=fill, outline=outline, width=int(w * S) if outline else 0)
    def circle(self, cx, cy, r, fill=None, outline=None, w=3):
        self.d.ellipse([(cx - r) * S, (cy - r) * S, (cx + r) * S, (cy + r) * S], fill=fill, outline=outline, width=int(w * S) if outline else 0)
    def ell(self, cx, cy, rx, ry, fill=None, outline=None, w=3):
        self.d.ellipse([(cx - rx) * S, (cy - ry) * S, (cx + rx) * S, (cy + ry) * S], fill=fill, outline=outline, width=int(w * S) if outline else 0)
    def poly(self, pts, fill=None, outline=None, w=3):
        self.d.polygon(self._p(pts), fill=fill)
        if outline: self.d.line(self._p(pts + [pts[0]]), fill=outline, width=int(w * S), joint="curve")
    def line(self, pts, fill=WHITE, w=3): self.d.line(self._p(pts), fill=fill, width=int(w * S), joint="curve")
    def dash(self, p0, p1, fill=WHITE, w=3, on=16, off=12):
        L = math.hypot(p1[0] - p0[0], p1[1] - p0[1]); n = int(L // (on + off)) + 1; ux, uy = (p1[0] - p0[0]) / L, (p1[1] - p0[1]) / L
        for i in range(n):
            a = i * (on + off); b = min(a + on, L)
            self.line([(p0[0] + ux * a, p0[1] + uy * a), (p0[0] + ux * b, p0[1] + uy * b)], fill, w)
    def arc(self, cx, cy, r, a0, a1, fill=WHITE, w=4):
        self.d.arc([(cx - r) * S, (cy - r) * S, (cx + r) * S, (cy + r) * S], a0, a1, fill=fill, width=int(w * S))
    def text(self, x, y, t, size=24, fill=WHITE, bold=False, anchor="la", sym=False, shadow=False):
        if shadow: self.d.text((x * S + 2, y * S + 2), t, font=F(size, bold, sym), fill=(0, 0, 0, 170), anchor=anchor)
        self.d.text((x * S, y * S), t, font=F(size, bold, sym), fill=fill, anchor=anchor)
    def tw(self, t, size, bold=False): return self.d.textlength(t, font=F(size, bold)) / S
    def glow(self, cx, cy, r, color, steps=14):
        for i in range(steps, 0, -1):
            a = int(70 * (1 - i / steps) ** 2) + 4; rr = r * i / steps
            self.circle(cx, cy, rr, fill=(*color[:3], a))
    def star(self, cx, cy, r, fill=YELLOW, n=5, inner=.45, rot=-90):
        pts = []
        for i in range(n * 2):
            a = math.radians(rot + i * 180 / n); rr = r if i % 2 == 0 else r * inner
            pts.append((cx + math.cos(a) * rr, cy + math.sin(a) * rr))
        self.poly(pts, fill=fill)
    def chip(self, x, y, t, fill=NAVY, color=WHITE, size=22, right=False):
        w = self.tw(t, size, True) + 36
        if right: x -= w
        self.rect(x, y, x + w, y + 44, fill, LINE, 22, 2.5); self.text(x + w / 2, y + 22, t, size, color, True, "mm"); return w
    def poly_pts_grid(self, c, u, v): # bilinear no quadrilatero: near-left, far-left, near-right, far-right
        tl, tr, bl, br = c
        return tuple((1 - u) * (1 - v) * tl[k] + (1 - u) * v * tr[k] + u * (1 - v) * bl[k] + u * v * br[k] for k in (0, 1))
    def hud(self, tag, title, t_text, stat, bar, bar_color=ORANGE, stat2=None):
        w = max(self.tw(title, 30, True), self.tw(tag, 16, True)) + 44
        self.rect(24, 22, 24 + w, 104, NAVY, LINE, 18, 2.5)
        self.text(44, 30, tag.upper(), 15, ORANGE, True); self.text(44, 52, title, 30, WHITE, True)
        self.rect(24, 112, 24 + w, 128, (20, 26, 52, 235), LINE, 8, 2); self.rect(27, 115, 27 + (w - 6) * bar, 125, bar_color, None, 5)
        x = W - 24
        for t, c in ((t_text, WHITE), (stat, YELLOW), (stat2, CYAN)):
            if t: x -= self.chip(x, 24, t, color=c, right=True) + 10
        self.rect(24, H - 68, 24 + 220, H - 24, NAVY, LINE, 22, 2.5); self.text(24 + 110, H - 46, "?  Como jogar", 21, MUTED, True, "mm")
    def dpad(self, x, y, s=64):
        for dx, dy, ch in ((0, -1, "▲"), (-1, 0, "◀"), (1, 0, "▶"), (0, 1, "▼")):
            cx, cy = x + dx * (s + 8), y + dy * (s + 8)
            self.rect(cx - s / 2, cy - s / 2, cx + s / 2, cy + s / 2, (14, 19, 38, 215), LINE, 16, 2.5); self.text(cx, cy, ch, 28, WHITE, False, "mm", sym=True)
    def btn(self, x0, y0, x1, y1, t, fill=ORANGE, color=(30, 20, 5, 255), size=30):
        self.rect(x0, y0, x1, y1, fill, (255, 255, 255, 90), 20, 2.5); self.text((x0 + x1) / 2, (y0 + y1) / 2, t, size, color, True, "mm")
    def drop(self, cx, cy, r, color=(120, 215, 255, 235)):
        self.poly([(cx, cy - r * 1.9), (cx - r * .85, cy - r * .2), (cx + r * .85, cy - r * .2)], color); self.circle(cx, cy, r, color)
        self.circle(cx - r * .3, cy - r * .2, r * .25, (255, 255, 255, 200))

def A(name):
    return json.load(open(os.path.join(SRC, f"anch_{name}.json")))
def P(a, k): return (a[k][0], a[k][1])

OUT = []
def finish(cv, idx, slug, name):
    bg = Image.open(os.path.join(SRC, f"bg_{name}.png")).convert("RGBA")
    ov = cv.im.resize((W, H), Image.LANCZOS)
    out = Image.alpha_composite(bg, ov).convert("RGB")
    fn = f"{idx:02d}-{slug}.png"; out.save(os.path.join(DST, fn)); OUT.append(fn); print(fn)

# ------------------------------------------------------------------ 01 coletar pedras
def g_amostra():
    a = A("amostra"); cv = Cv()
    for i, v in enumerate((3, 2, 1)):
        x, y = P(a, f"pedra{i}")
        cv.glow(x, y + 60, 90, (255, 255, 200))
        cv.rect(x - 44, y - 54, x + 44, y - 10, NAVY, LINE, 14, 2.5); cv.star(x - 20, y - 32, 14); cv.text(x + 6, y - 32, str(v), 26, WHITE, True, "lm")
        cv.poly([(x - 9, y - 10), (x + 9, y - 10), (x, y + 2)], NAVY)
    cx, cy = 880, 700  # astronauta (marcador do jogador)
    cv.ell(cx, cy + 38, 52, 14, (0, 0, 0, 100)); cv.circle(cx, cy, 34, (255, 159, 67, 255), WHITE, 4); cv.text(cx, cy, "VOCÊ", 17, (30, 20, 5, 255), True, "mm")
    cv.dash((cx, cy - 36), P(a, "pedra0"), (255, 255, 255, 190), 3)
    cv.hud("Campo de pedras", "Busque pedras e volte antes do ar acabar", "", "Mochila 1/2  ·  Ciência 3", .62, CYAN, "O₂ gasto: 1 de 3")
    cv.dpad(1420, 740, 62); return cv
# ------------------------------------------------------------------ 02 painel solar
def g_painel():
    a = A("painel"); cv = Cv(); pc = P(a, "panel_c")
    sx, sy = 1330, 120
    cv.glow(sx, sy, 150, (255, 230, 150)); cv.circle(sx, sy, 38, (255, 250, 225, 255))
    cv.dash(pc, (sx, sy), (255, 243, 196, 230), 4)
    ang = math.atan2(sy - pc[1], sx - pc[0]); ax, ay = pc[0] + math.cos(ang + .5) * 150, pc[1] + math.sin(ang + .5) * 150
    cv.circle(pc[0], pc[1], 120, (14, 19, 38, 120), LINE, 3)
    cv.line([pc, (ax, ay)], (143, 180, 255, 255), 7); cv.poly([(ax, ay), (ax - 26 * math.cos(ang + .5) + 13 * math.sin(ang + .5), ay - 26 * math.sin(ang + .5) - 13 * math.cos(ang + .5)), (ax - 26 * math.cos(ang + .5) - 13 * math.sin(ang + .5), ay - 26 * math.sin(ang + .5) + 13 * math.cos(ang + .5))], (143, 180, 255, 255))
    cv.arc(pc[0], pc[1], 120, math.degrees(ang) - 12, math.degrees(ang) + 12, GREEN, 8)
    cv.hud("Painéis solares", "Siga o Sol com o painel", "6 s", "Luz capturada: 72%", .4, ORANGE)
    for i, (x, ch) in enumerate(((1330, "⟲"), (1470, "⟳"))):
        cv.circle(x, 760, 56, (14, 19, 38, 225), LINE, 3); cv.text(x, 758, ch, 52, WHITE, False, "mm", sym=True)
    return cv
# ------------------------------------------------------------------ 03 vazamento
def g_vazamento():
    a = A("vazamento"); cv = Cv()
    for i in range(4):
        x, y = P(a, f"furo{i}")
        if i < 2:
            cv.rect(x - 34, y - 34, x + 34, y + 34, (150, 155, 165, 255), (60, 65, 75, 255), 6, 3)
            for dx in (-22, 22):
                for dy in (-22, 22): cv.circle(x + dx, y + dy, 4, (70, 75, 85, 255))
        else:
            cv.circle(x, y, 22, (5, 5, 10, 255), (90, 95, 105, 255), 5)
            for k in range(9):
                t = k / 8; cv.circle(x + 20 + t * 150 + math.sin(k * 2.1) * 8, y - 8 - t * 85 + math.cos(k * 1.7) * 6, 16 + t * 34, (235, 245, 255, int(215 * (1 - t * .7))), (150, 175, 210, int(200 * (1 - t * .8))), 2)
    x, y = P(a, "furo2")
    for r, al in ((54, 210), (80, 140), (108, 80)): cv.circle(x, y, r, None, (255, 255, 255, al), 3)
    cv.hud("Abrigo (hub)", "Tape os furos, rápido!", "8 s", "Furos tapados: 2/4", .55, CYAN, "O₂ perdido: −1")
    return cv
# ------------------------------------------------------------------ 04 antena
def g_antena():
    a = A("antena"); cv = Cv(); dx, dy = P(a, "prato"); ex, ey = P(a, "terra")
    ang = math.degrees(math.atan2(ey - (dy - 40), ex - dx))
    cv.glow(ex, ey, 120, (90, 160, 255)); cv.circle(ex, ey, 62, None, GREEN, 5); cv.text(ex, ey + 92, "Terra", 22, WHITE, True, "mm", shadow=True)
    for r in (60, 90, 120): cv.arc(dx, dy - 40, r, ang - 20, ang + 20, (255, 255, 255, 215), 5)
    cv.dash((dx, dy - 40), (ex, ey), (143, 180, 255, 220), 3)
    cv.rect(1478, 220, 1518, 640, NAVY, LINE, 18, 2.5); cv.rect(1472, 300, 1524, 350, ORANGE, WHITE, 14, 2.5)
    cv.text(1498, 190, "▲", 30, WHITE, False, "mm", sym=True); cv.text(1498, 672, "▼", 30, WHITE, False, "mm", sym=True)
    cv.hud("Antena", "Mantenha a antena na Terra", "5 s", "Sinal: 64%", .64, GREEN, "Energia: −1")
    for i in range(10): cv.rect(24 + i * 34, H - 108, 24 + i * 34 + 28, H - 92, GREEN if i < 6 else (40, 48, 80, 220), None, 5)
    return cv
# ------------------------------------------------------------------ 05 estufa
def g_estufa():
    a = A("estufa"); cv = Cv(); levels = (.55, .30, .85)
    for i, lv in enumerate(levels):
        x, y = P(a, f"p{i}"); x0, y0 = x - 14, y - 250 + i * 8
        cv.rect(x0, y0, x0 + 28, y0 + 150, (14, 19, 38, 235), LINE, 10, 2.5)
        cv.rect(x0 + 3, y0 + 52, x0 + 25, y0 + 98, (107, 212, 37, 200), None, 4)
        yy = y0 + 150 - 150 * lv; cv.poly([(x0 + 38, yy), (x0 + 54, yy - 9), (x0 + 54, yy + 9)], WHITE)
        cv.rect(x0 + 3, y0 + 150 - 150 * lv, x0 + 25, y0 + 147, (70, 170, 255, 230), None, 4)
        cv.circle(x, y - 78, 6, (0, 0, 0, 0))
    x, y = P(a, "p0")
    cv.poly([(x + 60, y - 150), (x + 150, y - 150), (x + 135, y - 90), (x + 70, y - 90)], (200, 205, 215, 255), (60, 65, 75, 255)); cv.line([(x + 60, y - 140), (x + 20, y - 120)], (200, 205, 215, 255), 8)
    for k in range(5): cv.drop(x - 4 + k * 6 - 10, y - 90 + k * 20, 6)
    cv.hud("Estufa", "Regue na medida certa", "7 s", "Plantas ok: 2/3", .66, GREEN)
    return cv
# ------------------------------------------------------------------ 06 troque as pecas
def g_falha():
    a = A("falha"); cv = Cv(); c = [P(a, "panel_tl"), P(a, "panel_tr"), P(a, "panel_bl"), P(a, "panel_br")]; cols, rows = 5, 2
    burnt = {(1, 0), (3, 1), (4, 0)}; fixed = {(0, 1)}
    for i in range(cols):
        for j in range(rows):
            q = [cv.poly_pts_grid(c, i / cols, j / rows), cv.poly_pts_grid(c, (i + 1) / cols, j / rows), cv.poly_pts_grid(c, (i + 1) / cols, (j + 1) / rows), cv.poly_pts_grid(c, i / cols, (j + 1) / rows)]
            if (i, j) in burnt: cv.poly(q, (60, 30, 15, 200), (255, 150, 40, 255), 4)
            elif (i, j) in fixed: cv.poly(q, (107, 212, 37, 110), GREEN, 4)
            else: cv.poly(q, (80, 140, 230, 40), (255, 255, 255, 120), 2)
    for (i, j) in burnt:
        x, y = cv.poly_pts_grid(c, (i + .5) / cols, (j + .5) / rows); cv.star(x, y, 38, (255, 220, 80, 255), 8, .4); cv.star(x, y, 20, (255, 255, 255, 255), 8, .4)
    x, y = cv.poly_pts_grid(c, 2.5 / cols, 0.5 / rows)
    cv.hud("Painéis solares", "Troque as peças queimadas", "9 s", "Peças trocadas: 1/3", .33, ORANGE, "O₂ gasto: 1")
    return cv
# ------------------------------------------------------------------ 07 agua
def g_agua():
    a = A("agua"); cv = Cv(); px, py = P(a, "cano")
    drops = [(px - 40, 250), (px + 60, 330), (px + 10, 430), (px + 150, 360), (px - 110, 480), (px + 210, 520), (px + 90, 600)]
    for x, y in drops: cv.drop(x, y, 14)
    for x in (px + 20, px + 120): cv.dash((x, 190), (x, 230), (120, 215, 255, 120), 3, 6, 6)
    tx, ty = 860, 720
    cv.poly([(tx - 120, ty - 70), (tx + 120, ty - 70), (tx + 90, ty + 30), (tx - 90, ty + 30)], (230, 240, 245, 245), (60, 70, 90, 255), 4)
    cv.poly([(tx - 108, ty - 30), (tx + 108, ty - 30), (tx + 90, ty + 26), (tx - 90, ty + 26)], (60, 190, 235, 235))
    cv.text(tx, ty - 50, "TANQUE", 20, (40, 60, 90, 255), True, "mm")
    cv.text(tx - 190, ty - 10, "◀", 46, WHITE, False, "mm", sym=True, shadow=True); cv.text(tx + 190, ty - 10, "▶", 46, WHITE, False, "mm", sym=True, shadow=True)
    cv.hud("Módulo de Água", "Pegue as gotas de água limpa", "10 s", "Gotas: 7", .45, CYAN, "Perdidas: 1")
    return cv
# ------------------------------------------------------------------ 08 poeira
def g_poeira():
    a = A("poeira"); cv = Cv(); c = [P(a, "panel_tl"), P(a, "panel_tr"), P(a, "panel_bl"), P(a, "panel_br")]; cols, rows = 4, 3
    dust = {(0, 0): 3, (1, 0): 2, (2, 0): 0, (3, 0): 3, (0, 1): 0, (1, 1): 3, (2, 1): 1, (3, 1): 0, (0, 2): 2, (1, 2): 0, (2, 2): 3, (3, 2): 1}
    for (i, j), lv in dust.items():
        q = [cv.poly_pts_grid(c, i / cols, j / rows), cv.poly_pts_grid(c, (i + 1) / cols, j / rows), cv.poly_pts_grid(c, (i + 1) / cols, (j + 1) / rows), cv.poly_pts_grid(c, i / cols, (j + 1) / rows)]
        if lv == 0: cv.poly(q, (80, 150, 255, 70), (140, 200, 255, 255), 3)
        else: cv.poly(q, (150, 120, 85, 80 + lv * 50), (230, 200, 150, 220), 3)
    p = [cv.poly_pts_grid(c, .15 + k * .09, .35 + math.sin(k * .8) * .25) for k in range(8)]
    cv.line(p, (255, 255, 255, 200), 8)
    for k in range(8): cv.circle(p[k][0], p[k][1], 10 - k * .8, (255, 255, 255, 120))
    cv.circle(p[-1][0], p[-1][1], 26, (255, 255, 255, 80), WHITE, 4)
    cv.hud("Painéis solares", "Limpe a poeira dos painéis", "12 s", "Limpeza: 55%", .55, CYAN)
    return cv
# ------------------------------------------------------------------ 09 micro
def g_micro():
    a = A("micro"); cv = Cv()
    cv.rect(520, 560, 1080, 760, NAVY, LINE, 24, 3); cv.text(800, 590, "Olhe a ordem... agora repita!", 22, MUTED, True, "mm")
    cols = [(125, 227, 240), (182, 240, 125), (255, 179, 240), (255, 210, 63)]
    for k, col in enumerate(cols):
        x, y = 600 + k * 134, 675; lit = (k == 2)
        if lit: cv.glow(x, y, 80, col)
        cv.poly([(x, y - 46), (x + 36, y - 6), (x, y + 46), (x - 36, y - 6)], (*col, 255 if lit else 150), WHITE if lit else (255, 255, 255, 120), 4 if lit else 2)
        cv.poly([(x, y - 46), (x + 14, y - 6), (x, y + 10), (x - 14, y - 6)], (255, 255, 255, 120 if lit else 60))
    for k in range(3): cv.circle(740 + k * 60 + 0, 735, 8, GREEN if k < 2 else (60, 70, 110, 255))
    mx, my = P(a, "micro"); cv.dash((mx + 10, my - 30), (700, 560), (255, 255, 255, 120), 3)
    cv.hud("Laboratório", "Repita a ordem dos cristais", "", "Rodada 2/3", .66, ORANGE, "Ciência: 1")
    return cv
# ------------------------------------------------------------------ 10 upload
def g_upload():
    a = A("upload"); cv = Cv(); dx, dy = P(a, "prato"); ex, ey = P(a, "terra")
    ang = math.degrees(math.atan2(ey - (dy - 40), ex - dx))
    for r in (70, 105, 140): cv.arc(dx, dy - 40, r, ang - 22, ang + 22, (255, 255, 255, 210), 5)
    cv.dash((dx, dy - 40), (ex, ey), (143, 180, 255, 160), 3)
    cv.rect(380, 640, 1220, 760, NAVY, LINE, 24, 3); cv.text(800, 662, "Sinal", 18, MUTED, True, "mm")
    cv.rect(410, 680, 1190, 726, (24, 32, 62, 255), LINE, 12, 2.5); cv.rect(800, 682, 930, 724, (107, 212, 37, 215), None, 8)
    mx = 730; cv.poly([(mx - 16, 672), (mx + 16, 672), (mx, 700)], WHITE); cv.rect(mx - 3, 680, mx + 3, 726, WHITE, None, 2)
    cv.btn(630, 780, 970, 850, "ENVIAR", ORANGE, (30, 20, 5, 255), 36)
    for k in range(3): cv.rect(1230 + k * 0, 0, 1230, 0, None)
    for k, st in enumerate((1, 2, 0)):
        x = 1010 + k * 70; cv.rect(x, 790, x + 56, 840, GREEN if st == 1 else (ORANGE if st == 2 else (50, 60, 100, 230)), WHITE if st == 2 else None, 10, 3); cv.text(x + 28, 815, str(k + 1), 24, (20, 20, 20, 255) if st else MUTED, True, "mm")
    cv.hud("Antena", "Envie os dados na hora certa", "", "Pacote 2/3", .66, CYAN, "Energia: −2")
    return cv
# ------------------------------------------------------------------ 11 filtros
def g_filtros():
    a = A("filtros"); cv = Cv(); import random; random.seed(3)
    for i in range(3):
        x, y = P(a, f"fan{i}")
        for k in range(8 - i * 2):
            cv.circle(x + random.uniform(-70, 70) * (1 + i * .3), y + random.uniform(-60, 60) * (1 + i * .3), random.uniform(5, 11), (150, 112, 70, 235), (90, 65, 40, 255), 2)
    x, y = P(a, "fan1")
    cv.circle(x - 10, y + 10, 52, (77, 208, 225, 60), CYAN, 4); cv.line([(x - 10, y + 10), (x + 150, y + 150)], (200, 205, 215, 255), 14); cv.line([(x - 10, y + 10), (x + 150, y + 150)], (80, 85, 95, 255), 4)
    cv.rect(x + 110, y + 130, x + 220, y + 190, (200, 205, 215, 255), (60, 65, 75, 255), 14, 3)
    cv.hud("Módulo de Energia", "Aspire a poeira do filtro", "9 s", "Poeira: 11 restantes", .5, CYAN, "Energia: −1")
    cv.dpad(1420, 740, 62); return cv
# ------------------------------------------------------------------ 12 exercicio
def g_exercicio():
    a = A("exercicio"); cv = Cv()
    cv.rect(560, 150, 1040, 230, NAVY, LINE, 22, 3); cv.text(800, 172, "Velocidade", 18, MUTED, True, "mm"); cv.rect(590, 190, 1010, 212, (24, 32, 62, 255), LINE, 10, 2); cv.rect(592, 192, 592 + 270, 210, GREEN, None, 8)
    cv.text(800, 262, "esquerdo · direito · esquerdo · direito...", 22, WHITE, True, "mm", shadow=True)
    for x, name, on in ((480, "ESQUERDO", True), (1120, "DIREITO", False)):
        y = 735
        cv.circle(x, y, 86, (255, 159, 67, 255) if on else (14, 19, 38, 225), WHITE if on else LINE, 4)
        cv.ell(x, y + 6, 24, 38, (255, 255, 255, 235) if on else (200, 210, 235, 255)); cv.ell(x, y - 42, 16, 12, (255, 255, 255, 235) if on else (200, 210, 235, 255))
        cv.text(x, y + 112, name, 20, WHITE, True, "mm", shadow=True)
    cv.text(480, 640, "◀", 40, WHITE, False, "mm", sym=True, shadow=True); cv.text(1120, 640, "▶", 40, MUTED, False, "mm", sym=True, shadow=True)
    for k in range(8): cv.circle(670 + k * 37, 760, 11, GREEN if k < 5 else (60, 70, 110, 255))
    cv.hud("Módulo do Dormitório", "Corra alternando os pés", "6 s", "Passos: 24", .6, GREEN, "7,2 km/h")
    return cv
# ------------------------------------------------------------------ 13 braco
def g_braco():
    a = A("braco"); cv = Cv(); gx, gy = P(a, "garra"); bx, by = P(a, "caixa")
    cv.circle(gx, gy + 60, 74, (255, 255, 255, 25), WHITE, 4); cv.line([(gx - 90, gy + 60), (gx - 50, gy + 60)], WHITE, 3); cv.line([(gx + 50, gy + 60), (gx + 90, gy + 60)], WHITE, 3)
    cv.arc(bx, by, 62, -90, 160, GREEN, 9)
    for sx in (-1, 1):
        for sy in (-1, 1):
            cv.line([(bx + sx * 78, by + sy * 66 - sy * 26), (bx + sx * 78, by + sy * 66), (bx + sx * 78 - sx * 26, by + sy * 66)], YELLOW, 5)
    cv.hud("Braço robótico", "Agarre a caixa com o braço robótico", "8 s", "Agarre: 70%", .7, GREEN, "Energia: −1")
    cv.dpad(1420, 740, 62); return cv
# ------------------------------------------------------------------ 14 rover
def g_rover():
    a = A("rover"); cv = Cv()
    cv.rect(0, 0, 0, 0, None)
    for i in range(4):
        x, y = P(a, f"c{i}"); cv.ell(x, y, 62 - i * 6, 22 - i * 2, (235, 70, 60, 50), RED, 4)
        cv.text(x, y - 38 + i * 4, "!", 28 - i * 2, RED, True, "mm", shadow=True)
    for i in range(4):
        x, y = P(a, f"s{i}"); cv.glow(x, y, 46 - i * 5, (120, 255, 255)); cv.star(x, y, 14 - i * 1.5, (255, 255, 255, 255), 4, .35)
    rx, ry = P(a, "rover"); cv.line([(rx - 120, ry + 40), (rx - 28, ry - 100)], (255, 255, 255, 110), 3); cv.line([(rx + 120, ry + 40), (rx + 28, ry - 100)], (255, 255, 255, 110), 3)
    cv.hud("Passeio de rover", "Dirija o rover pelas crateras", "", "Amostras: 2", .4, ORANGE, "Crateras: 0")
    for k, ch in enumerate(("▲", "▼")):
        cv.rect(1440, 640 + k * 100, 1520, 720 + k * 100, (14, 19, 38, 225), LINE, 18, 3); cv.text(1480, 680 + k * 100, ch, 34, WHITE, False, "mm", sym=True)
    cv.text(1480, 600, "Pista", 20, MUTED, True, "mm", shadow=True)
    return cv
# ------------------------------------------------------------------ 15 co2
def g_co2():
    a = A("co2"); cv = Cv()
    def hexa(cx, cy, r, fill=None, outline=None, w=4): cv.poly([(cx + math.cos(math.radians(60 * k)) * r, cy + math.sin(math.radians(60 * k)) * r) for k in range(6)], fill, outline, w)
    cv.rect(610, 150, 990, 330, NAVY, LINE, 22, 3); cv.text(800, 175, "Encaixe", 20, MUTED, True, "mm"); hexa(800, 255, 56, (10, 14, 30, 255), CYAN, 6)
    cv.line([(800, 330), (800, 560)], (255, 255, 255, 110), 3)
    xs = (520, 800, 1080)
    for k, x in enumerate(xs):
        y = 690; ok = (k == 1); cv.rect(x - 90, y - 90, x + 90, y + 90, (14, 19, 38, 235), GREEN if ok else LINE, 20, 5 if ok else 3)
        if ok: cv.glow(x, y, 120, (107, 212, 37), 8)
        if k == 0: cv.circle(x, y, 44, (200, 215, 235, 255), (60, 70, 100, 255), 3)
        if k == 1: hexa(x, y, 48, (200, 215, 235, 255), (60, 70, 100, 255), 3)
        if k == 2: cv.rect(x - 40, y - 40, x + 40, y + 40, (200, 215, 235, 255), (60, 70, 100, 255), 8, 3)
    cv.circle(800 + 30, 740, 26, (255, 255, 255, 90), WHITE, 3)
    cv.hud("Módulo de Água", "Encaixe o filtro certo", "7 s", "Rodada 2/3", .66, CYAN, "Energia: −1")
    return cv
# ------------------------------------------------------------------ 16 treino
def g_treino():
    cv = Cv(); cv.rect(480, 150, 1120, 210, (140, 20, 20, 235), RED, 18, 3); cv.text(800, 180, "ALERTA DE EMERGÊNCIA · SIGA A LISTA", 26, WHITE, True, "mm")
    cv.rect(24, 160, 410, 560, NAVY, LINE, 22, 3); cv.text(44, 178, "LISTA 2 DE 3", 17, ORANGE, True)
    steps = (("Fechar a escotilha", GREEN, "ok"), ("Vestir máscara", GREEN, "ok"), ("Ligar o oxigênio", ORANGE, "now"), ("Chamar a base", None, "")); y = 220
    for k, (t, col, st) in enumerate(steps):
        cv.circle(66, y + 20, 22, col if col else (50, 60, 100, 255), WHITE if st == "now" else None, 4); cv.text(66, y + 20, "✓" if st == "ok" else str(k + 1), 22, (20, 20, 20, 255) if col else MUTED, True, "mm", sym=(st == "ok"))
        cv.text(104, y + 20, t, 24, WHITE if st != "" else MUTED, True, "lm"); y += 80
    cols = [(235, 70, 60), (255, 210, 63), (77, 208, 225), (107, 212, 37)]
    for k, c in enumerate(cols):
        x = 530 + k * 150; y0 = 740; on = (k == 2)
        cv.rect(x, y0 - 62, x + 120, y0 + 62, (*c, 255) if on else (*c, 170), WHITE if on else None, 20, 4)
        sh = [(x + 60, y0 - 26), (x + 90, y0 + 22), (x + 30, y0 + 22)] if k == 0 else None
        if k == 0: cv.poly(sh, (255, 255, 255, 235))
        if k == 1: cv.circle(x + 60, y0, 26, (255, 255, 255, 235))
        if k == 2: cv.rect(x + 34, y0 - 26, x + 86, y0 + 26, (255, 255, 255, 235), None, 6)
        if k == 3: cv.star(x + 60, y0, 30, (255, 255, 255, 235), 4, .4, 0)
    cv.hud("Abrigo (hub)", "Siga a lista de emergência", "9 s", "Lista 2/3", .66, RED)
    return cv

GAMES = [(1, "coletar-pedras", "amostra", g_amostra), (2, "painel-solar", "painel", g_painel), (3, "vazamento-de-ar", "vazamento", g_vazamento), (4, "apontar-antena", "antena", g_antena),
         (5, "regar-estufa", "estufa", g_estufa), (6, "trocar-pecas", "falha", g_falha), (7, "gotas-de-agua", "agua", g_agua), (8, "limpar-poeira", "poeira", g_poeira),
         (9, "microscopio", "micro", g_micro), (10, "enviar-dados", "upload", g_upload), (11, "filtros-de-ar", "filtros", g_filtros), (12, "exercicio", "exercicio", g_exercicio),
         (13, "braco-robotico", "braco", g_braco), (14, "rover", "rover", g_rover), (15, "filtro-co2", "co2", g_co2), (16, "treino-de-emergencia", "treino", g_treino)]
only = [a for a in sys.argv if a.startswith("--only=")]; only = only[0].split("=")[1].split(",") if only else None
for idx, slug, name, fn in GAMES:
    if only and name not in only: continue
    finish(fn(), idx, slug, name)
# folha de contato
if not only:
    ims = [Image.open(os.path.join(DST, f)).resize((480, 270)) for f in OUT]; sheet = Image.new("RGB", (480 * 4, 270 * 4))
    for i, im in enumerate(ims): sheet.paste(im, ((i % 4) * 480, (i // 4) * 270))
    sheet.save(os.path.join(DST, "00-todos.png"))
