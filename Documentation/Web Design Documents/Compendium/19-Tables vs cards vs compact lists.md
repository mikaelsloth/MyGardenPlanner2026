### 📄 19-Tables vs cards vs compact lists.md

# 19 - Tables vs cards vs compact lists

## 📘 Grundregel
- Standard visningsform vælges altid ud fra brugerens aktuelle opgave og datamængde — aldrig ud fra datatypen alene. Cards anvendes til overblik og visuel browsing af få eller mellemstore enheder, Compact Rows er standarden for lange søge- og filterresultater, Tables benyttes til præcis sammenligning, tal, materialer og print, mens Thumbnail Grids reserveres til visuel genkendelse af billeder og dokumenter.

---

## 🔍 Anvendelse / varianter / typer

### 1. Visningsmønstre og anvendelse

| Visning | Bootstrap & CSS basis | Opgave & Brugerkontekst | Default domæneområde |
| :--- | :--- | :--- | :--- |
| **Cards** | `.card` + `.card-entity` | Browsing, hurtigt overblik over få objekter (1–20), visuel scanning med metadata og badges. | Haver, Bede, Dashboard summary cards. |
| **Compact rows** | `.card-compact` / `.compact-entity-row` | Hurtig navigation og scanning i mellemstore til lange lister (20+), søge- og filterresultater. | Store plantelister, Materialer (app-visning), Medlemmer, Dokumentfiler. |
| **Tables** | `.table` + `.data-table` | Strukturerede data, tal, mål, priser, kolonne-sammenligning og udskrift. | Materialelister, Plantesammenligning, Indkøb/Tilbud, Printrapporter. |
| **Thumbnail grid** | `.thumbnail-grid` | Skitser, tegninger, PDF-forsider og fotoarkiver, hvor billedindhold er primær vælger-faktor. | Billedgalleri, Skitsearkiv, Referencetræer. |
| **Detail view** | `MgpDetailPage` / `.detail-view` | Dybdeundersøgelse og arbejde med ét konkret objekt. Realiseres med detail header, context tabs, summary cards, main content og related sections. | Enkelthave, Enkeltbed, Enkeltplante, Projektdetalje, Enkeltfil. |

### View-mode til Density Mapping
- **Cards (`view=cards`):** Primært `Comfortable` eller `Default` density.
- **Compact Rows (`view=compact`):** Altid `Compact` density.
- **Tables (`view=table`):** Altid `Compact` density.
- **Thumbnail Grid (`view=grid`):** `Default` eller `Comfortable` density (for at bevare billedgenkendelse).
- **Print Table (`view=print`):** Altid `Compact` density.

---

### 2. Beslutningsmatrix (Opgave vs. Visning)

```text
               Datamængde & Formål
                        │
      ┌─────────────────┴─────────────────┐
  Få (1–20)                           Mange (20+)
      │                                   │
┌─────┴─────┐                       ┌─────┴─────┐
Browsing? Visuelt?                 Sammenligne? Tal?
  │           │                       │           │
[Cards] [Thumbnail Grid]          [Tables] [Compact Rows]
```

---

## 🚫 Regler (Do / Don't)
- **Do:** Vælg visningsmønster efter brugerens opgave. Gem altid aktiv visning i URL state (fx `/planter?view=compact` eller `/filer?view=grid`) for at bevare arbejdsrum i return-flows.
- **Do:** Prioritér tætheden af metadata og handlinger i forhold til visningen:
  - **Cards:** 2–4 badges, 2–3 handlinger.
  - **Compact rows:** 1–2 badges, 1 primær handling (`[Vis]`).
  - **Tables:** Præcise kolonneværdier, 1 action-kolonne i bunden/højre side.
- **Do:** Anvend semantisk `<table>` ved ægte tabulære data for a11y, og definér en klar mobilstrategi (stacked rows eller konvertering til compact rows).
- **Do:** `DataTable` skal altid anvende semantisk `<table>`, `<thead>`, `<th>` og `<tbody>` markup som uomgængelig baseline frem for layout-divs for at sikre skærmlæserunderstøttelse.
- **Don't:** Anvend ikke tunge cards som standard for store datamængder (> 20 elementer) eller fritekst-søgeresultater.
- **Don't:** Placer ikke `MgpViewModeToggle` på alle sider. Visningsskift må kun tilbydes, hvor det giver reel opgavespecifik værdi (fx Planter, Materialer, Filer).
- **Edge cases:** Ved udskrift (print) konverteres skærmens kort- og række-grids automatisk til en PrintTable (.print-table) for at spare papir. På mobilskærme (<= 640px) skal tabeller med mere end 3-4 kolonner automatisk konverteres til stacked rows med strukturerede label-value par (.data-table-responsive-stacked td::before) eller compact rows for at bevare læsbarhed uden horisontal scroll.

---

## 🧩 Komponentpåvirkning
- **Type:** Pattern / Layout Component
- **Nye Razor-komponenter:**
  - `MgpCompactEntityRow.razor`: Pladseffektiv rækkevisning til lange lister og søgeresultater.
  - `MgpDataTable.razor`: Semantisk tabelkomponent med kolonne-sortering, tæthedsstyring og mobil-fallback.
  - `MgpThumbnailGrid.razor`: Responsivt raster-layout optimeret til medie-thumbnails og lazy loading.
  - `MgpPrintTable.razor`: Flad og monokrom tabelkomponent reserveret til print-output.
- **Ændrede Razor-komponenter:**
  - `MgpViewModeToggle.razor`: Udvidet med support for `cards`, `compact`, `table` og `grid` modes.
  - `MgpCard.razor`: Præciseret til udelukkende at dække overbliks- og browsing-kontekster.

---

## 🪙 Tokenpåvirkning
- `--mgp-density-comfortable`: Afstandsstørrelse til browsing og dashboards (padding: 16px/1rem).
- `--mgp-density-default`: Standard række- og kort-padding (padding: 12px/0.75rem).
- `--mgp-density-compact`: Høj informationstæthed til lange tabeller og søgelister (padding: 6px 12px / 0.375rem 0.75rem).
- `--mgp-surface`: Baggrund for cards, rækker og tabeller.
- `--mgp-surface-muted`: Baggrund for tabel-headers (`<thead>`) og inaktive visningsknapper.
- `--mgp-border`: Adskillelseslinjer for rækker og tabelceller.

---

## 💻 CSS & Bootstrap

```css
/* ==========================================================================
   Visningsmønstre: Compact Rows, Data Tables & Thumbnail Grids
   ========================================================================== */

/* 1. Compact Entity Row */
.compact-entity-row {
  display: grid;
  grid-template-columns: auto 1fr auto auto;
  gap: var(--space-md);
  align-items: center;
  padding: var(--mgp-density-compact, 0.5rem 0.75rem);
  background: var(--mgp-surface);
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-sm);
  transition: background-color 0.15s ease, border-color 0.15s ease;
}

.compact-entity-row:hover {
  border-color: rgba(63, 107, 74, 0.3);
  background: var(--mgp-surface-muted);
}

.compact-row-title {
  font-weight: var(--font-weight-bold);
  color: var(--mgp-text);
  margin: 0;
  font-size: var(--font-size-sm);
}

.compact-row-meta {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
  color: var(--mgp-text-muted);
  font-size: var(--font-size-xs);
}

/* 2. Data Table (Bootstrap Override) */
.data-table {
  width: 100%;
  margin-bottom: var(--space-md);
  color: var(--mgp-text);
  border-collapse: collapse;
  background: var(--mgp-surface);
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-md);
  overflow: hidden;
}

.data-table th,
.data-table td {
  padding: var(--mgp-density-default, 0.75rem 1rem);
  vertical-align: middle;
  border-bottom: 1px solid var(--mgp-border);
}

.data-table th {
  background: var(--mgp-surface-muted);
  font-weight: var(--font-weight-bold);
  font-size: var(--font-size-xs);
  text-transform: uppercase;
  letter-spacing: 0.03em;
  color: var(--mgp-text-muted);
}

.data-table-compact th,
.data-table-compact td {
  padding: var(--mgp-density-compact, 0.375rem 0.75rem);
  font-size: var(--font-size-sm);
}

.data-table tbody tr:hover {
  background: var(--mgp-surface-muted);
}

/* 3. Thumbnail Grid */
.thumbnail-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
  gap: var(--space-md);
}

.thumbnail-grid-item {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  text-align: center;
}

/* 4. Mobil Responsivitet (< 640px) */
@media (max-width: 640px) {
  .compact-entity-row {
    grid-template-columns: 1fr auto;
    gap: var(--space-xs);
  }

  .compact-row-meta {
    grid-column: 1 / -1;
  }

  /* Responsive Table til Stacked Cards på mobil */
  .data-table-responsive-stacked thead {
    display: none;
  }

  .data-table-responsive-stacked tr {
    display: block;
    margin-bottom: var(--space-sm);
    border: 1px solid var(--mgp-border);
    border-radius: var(--radius-md);
    background: var(--mgp-surface);
    padding: var(--space-sm);
  }

  .data-table-responsive-stacked td {
    display: flex;
    justify-content: space-between;
    padding: var(--space-xs) 0;
    border-bottom: 1px solid var(--mgp-surface-muted);
  }

  .data-table-responsive-stacked td::before {
    content: attr(data-label);
    font-weight: var(--font-weight-bold);
    color: var(--mgp-text-muted);
  }
}
```