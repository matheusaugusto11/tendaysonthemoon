"""Curadoria dos dados do NASA DONKI usados em Dez Dias na Lua (design D9 de eventos-diarios-com-decisao).

Para cada uma das 20 erupções de Assets/Resources/ErupcoesDONKI.json:
  - baixa os FLR, SEP e CME de uma janela curta em volta da data (porta pública do CCMC, sem chave);
  - liga a erupção ao primeiro SEP medido por instrumento (nunca "MODEL: ...");
  - junta as CMEs ligadas às erupções e escreve Assets/Resources/CmesDONKI.json.

As respostas brutas ficam em Docs/donki/<TIPO>_<início>_<fim>.json e não são baixadas de novo.
A data de cada consulta fica em Docs/donki/consultas.json. Rodar de novo, com ou sem rede,
gera os mesmos arquivos.

Uso: python Docs/donki/curar.py   (na raiz do repositório)
"""
import datetime as dt
import json
import os
import urllib.request

RAIZ = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
PASTA = os.path.join(RAIZ, "Docs", "donki")
ERUPCOES = os.path.join(RAIZ, "Assets", "Resources", "ErupcoesDONKI.json")
CMES = os.path.join(RAIZ, "Assets", "Resources", "CmesDONKI.json")
CONSULTAS = os.path.join(PASTA, "consultas.json")
PORTA = "https://ccmc.gsfc.nasa.gov/DONKI/WS/get/{tipo}?startDate={ini}&endDate={fim}"
MIN_CMES, MAX_CMES = 6, 10

consultas = json.load(open(CONSULTAS, encoding="utf-8")) if os.path.exists(CONSULTAS) else {}


def baixar(tipo, ini, fim):
    """Devolve a lista do DONKI para a janela, baixando só se o arquivo bruto não existir."""
    nome = f"{tipo}_{ini}_{fim}.json"
    caminho = os.path.join(PASTA, nome)
    if not os.path.exists(caminho):
        url = PORTA.format(tipo=tipo, ini=ini, fim=fim)
        req = urllib.request.Request(url, headers={"User-Agent": "dez-dias-na-lua-curadoria"})
        with urllib.request.urlopen(req, timeout=60) as r:
            corpo = r.read()
        with open(caminho, "wb") as f:
            f.write(corpo)
        consultas[nome] = dt.date.today().isoformat()
    texto = open(caminho, encoding="utf-8").read().strip()
    return (json.loads(texto) if texto else []) or [], consultas.get(nome, "")


def dia(s, delta=0):
    return (dt.date.fromisoformat(s[:10]) + dt.timedelta(days=delta)).isoformat()


def medido(sep):
    """SEP com pelo menos um instrumento que não é modelo de previsão."""
    return any(not (i.get("displayName") or "").startswith("MODEL:") for i in (sep.get("instruments") or []))


def ligados(ev, prefixo):
    return [l["activityID"] for l in (ev.get("linkedEvents") or []) if prefixo in l.get("activityID", "")]


def velocidade(cme):
    analises = cme.get("cmeAnalyses") or []
    melhor = next((a for a in analises if a.get("isMostAccurate")), analises[0] if analises else None)
    return int(round(melhor["speed"])) if melhor and melhor.get("speed") else 0


def main():
    dados = json.load(open(ERUPCOES, encoding="utf-8"))
    cmes = {}       # id -> (registro, flr ligado, data da consulta)
    for r in dados["records"]:
        flr_id, d0 = r["id"], r["startTime"]
        flrs, _ = baixar("FLR", dia(d0, -1), dia(d0, 1))
        seps, data_sep = baixar("SEP", dia(d0), dia(d0, 4))
        janela_cme, data_cme = baixar("CME", dia(d0, -1), dia(d0, 1))

        flr = next((f for f in flrs if f.get("flrID") == flr_id), {})
        ids_sep = set(ligados(flr, "-SEP-")) | {s["sepID"] for s in seps if flr_id in ligados(s, "-FLR-")}
        # Só SEP medido e que começa depois do clarão: o cartão conta "primeiro o clarão, depois as partículas".
        candidatos = sorted((s for s in seps if s["sepID"] in ids_sep and medido(s) and s["eventTime"] >= d0),
                            key=lambda s: s["eventTime"])
        sep = candidatos[0] if candidatos else None
        r["sepId"] = sep["sepID"] if sep else ""
        r["sepTime"] = sep["eventTime"] if sep else ""
        r["sepUrl"] = sep["link"] if sep else ""
        r["sepRetrievedOn"] = data_sep if sep else ""

        ids_cme = set(ligados(flr, "-CME-")) | {c["activityID"] for c in janela_cme if flr_id in ligados(c, "-FLR-")}
        for c in janela_cme:
            if c["activityID"] in ids_cme and velocidade(c) > 0:
                cmes[c["activityID"]] = (c, flr_id, data_cme)

    # Completa com as CMEs mais rápidas das mesmas janelas, sem ligação, se faltarem.
    if len(cmes) < MIN_CMES:
        extras = []
        for nome in sorted(os.listdir(PASTA)):
            if nome.startswith("CME_"):
                lista = json.loads(open(os.path.join(PASTA, nome), encoding="utf-8").read() or "[]") or []
                extras += [(c, "", consultas.get(nome, "")) for c in lista if c["activityID"] not in cmes and velocidade(c) > 0]
        extras.sort(key=lambda t: -velocidade(t[0]))
        for c, f, d in extras[: MIN_CMES - len(cmes)]:
            cmes[c["activityID"]] = (c, f, d)

    escolhidas = sorted(cmes.values(), key=lambda t: (t[1] == "", -velocidade(t[0])))[:MAX_CMES]
    escolhidas.sort(key=lambda t: t[0]["startTime"])

    dados["fonte"] = ("NASA DONKI (CCMC), catálogos FLR e SEP. Dado arquivado: FLR consultado em 13/11/2026, "
                      "SEP em 15/11/2026. Lista curada em Docs/DADOS-NASA.md, seção 2.3.")
    ordem = ["id", "isTest", "eventType", "startTime", "classLabel", "gameIntensity", "sourceUrl", "retrievedOn",
             "sepId", "sepTime", "sepUrl", "sepRetrievedOn"]
    escrever(ERUPCOES, dados["fonte"], [{k: r[k] for k in ordem} for r in dados["records"]])

    regs = [{"id": c["activityID"], "startTime": c["startTime"], "speed": velocidade(c), "sourceUrl": c["link"],
             "linkedFlr": f, "retrievedOn": d} for c, f, d in escolhidas]
    escrever(CMES, "NASA DONKI (CCMC), catálogo CME. Dado arquivado, consultado em 15/11/2026. "
                   "Lista curada em Docs/DADOS-NASA.md, seção 2.5.", regs)

    with open(CONSULTAS, "w", encoding="utf-8") as f:
        json.dump(consultas, f, ensure_ascii=False, indent=1, sort_keys=True)
        f.write("\n")
    com_sep = sum(1 for r in dados["records"] if r["sepId"])
    print(f"Erupções: {len(dados['records'])}, com SEP medido: {com_sep}. CMEs: {len(regs)} "
          f"({sum(1 for r in regs if r['linkedFlr'])} ligadas).")


def escrever(caminho, fonte, registros):
    linhas = ",\n".join("    " + json.dumps(r, ensure_ascii=False, separators=(", ", ": ")).replace("{", "{ ", 1)[:-1] + " }"
                        for r in registros)
    with open(caminho, "w", encoding="utf-8", newline="\n") as f:
        f.write("{\n  \"fonte\": " + json.dumps(fonte, ensure_ascii=False) + ",\n  \"records\": [\n" + linhas + "\n  ]\n}\n")


if __name__ == "__main__":
    main()
