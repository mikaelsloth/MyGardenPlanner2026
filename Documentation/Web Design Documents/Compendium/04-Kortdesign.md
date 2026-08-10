### 📄 04-Kortdesign.md

# 04 - Kortdesign

## 📘 Grundregel
- Cards opbygges modulært med neutral baggrund, diskret kant og let skygge, hvor indhold og metadata altid prioritere højest. Farver anvendes udelukkende til status, valgtilstand og accent, og destruktive handlinger skal altid holdes visuelt sekundære (f.eks. outline/link-knapper).
- Cards er forbeholdt overblik og visuel browsing af få eller mellemstore datamængder (1–20 enheder). Ved store datamængder (> 20 enheder), søge-/filterresultater eller kolonne-sammenligning skal brugerfladen anvende compact rows (`.compact-entity-row`) eller datatabeller (`.data-table`) jf. `19-Tables_vs_cards_vs_compact_lists.md`.

---

## 🔍 Anvendelse / varianter / typer

| Card-type | Bootstrap & CSS basis | Brugerkontekst |
| :--- | :--- | :--- |
| **Standard / Entity card** | `.card` + `.card-entity` | Generiske objekter (Haver, Bede, Materialer) med fast layout: Header, Beskrivelse, Meta og Handling. |
| **Plant card** | `.card` + `.card-plant` | Botanisk visning med badges/chips for vækstbetingelser, dimensioner og egenskaber. |
| **Media card** | `.card` + `.card-media` | Dokumenter og filer med preview-thumbnail til venstre, metadata samt valgfri preview- og levetidsbadges (`.media-card-previewable`, `.media-card-restricted`). |
| **Compact entity card** | `.card` + `.card-compact` | Tæt listevisning med højrereducerede rækker til lange oversigter. |
| **Compact entity row** | `.compact-entity-row` / `.card-compact` | Tæt listevisning og standard til lange oversigter, søgeresultater og filterlister (> 20 enheder). |
| **Action card** | `.card` + `.card-action` | Handlingsrettede kort (fx "Opret ny have" eller "Upload fil") med fremhævet ikon og primærknap. |
| **Alert / Accent card** | `.card` + `.card-attention` / `.card-selected` | Enheder der kræver opmærksomhed (venstre accent-streg) eller er i valgt tilstand. |
| **Status message card** | `.status-message` + `.status-[type]` | Kompakte system- og arbejdsbeskeder med visuel dot-indikator for hurtigt overblik. Understøtter varianterne `.status-success`, `.status-info`, `.status-warning`, `.status-danger`, `.status-processing` og `.status-restricted` samt scope-klasser (`.status-page`, `.status-section`, `.status-object`, `.status-form`). |
| **Archived card** | `.card` + `.card-archived` | Historiske eller inaktive enheder (haver, bede). Vises dæmpet med `.badge-archived` og primær `[Gendan]`-handling. |
| **Restricted card** | `.card` + `.card-restricted` | Adgangsbegrænsede objekter, hvor brugeren må vide, entiteten eksisterer, men ikke tilgå indholdet. Viser metadata, men deaktiverer handlinger. |
| **Collapsible section card** | `.collapsible-card` + `MgpCollapsibleSectionCard` | Sekundære sidekort på detail pages (fx medlemmer, print, danger zone). Headeren skal vise status/summary i sammefoldet tilstand. Advarsler og fejl må aldrig skjules. |
| **Attention Card** | `.card` + `.card-attention` + `.has-attention-level-[1|2|3]` | Objekter med opmærksomhedsbehov. Anvender `MgpAttentionBadge` og `MgpAttentionSummary`. |

> **Differentiering mellem dæmpede korttilstande:**  
    > - `.card-muted`: Midlertidigt inaktive eller deaktiverede flader i et aktivt arbejdsflow.  
    > - `.card-archived`: Historisk gemte entiteter (opacity `0.90`), som tilbyder en `[Gendan]`-handling.  
    > - `.card-restricted`: Objekter låst af brugerens rolle. Viser metadata og titel, men deaktiverer interaktion med forklarende permission-status.

> **Bemærkning til komponentroller:**  
    > - `MgpStatusMessage`: Kompakt besked om en gennemført handling eller transient tilstand (fx *"Filen er uploadet"* eller *"Det oprettede element skjules af et aktivt filter"*).  
    > - `MgpEmptyState`: Bruges til hele sektioner, cards eller sider, når datastatus er afklaret som tom, filtreret, fejlet eller låst (jf. `13-Empty_vs_error_vs_no_access.md`).  
    > - `MgpCard`: Bruges udelukkende som visuel ramme om eksisterende indhold og entiteter.

> **Bemærkning om visningskontekst:**  
> Cards er optimeret til visuel scanning og browsing. Når brugerens primære opgave er søgning, filtrering eller gennemgang af store datamængder, skal visningen automatisk eller via `MgpViewModeToggle` skifte fra cards til compact rows (`.compact-entity-row`) eller datatabel (`.data-table`).

---

## 🚫 Regler (Do / Don't)
- **Do:** Anvend et ægte `<a>`-tag med `.card-clickable` eller `.compact-entity-link`, når hele kortet fungerer som navigation til en detaljeside.
- **Do:** Ledsag altid farvekodede statusser (fx accent-kanter eller status-dots) med klar og tydelig tekst for at overholde tilgængelighedskrav (a11y).
- **Do:** Sørg for at sammenfoldede kort (`.collapsible-card`) altid viser en tilstrækkelig overskrift og status-summary (f.eks. *"Medlemmer · 2 aktive · 1 afventer"*), så brugeren forstår tilstanden uden at åbne kortet. Statuskritiske fejl og advarsler må aldrig foldes sammen.
- **Do:** Brug `MgpAttentionBadge` til eksplisitte tilstande (`Afventer`, `Udløber snart`, `Begrænset adgang`), men undgå at plastre kort til med badges for almindelig metadata. Badges skal indikere status og klassifikation — ikke generelle egenskaber.
- **Don't:** Gør aldrig handlinger på kort afhængige af hover (fx knapper der først opstår ved mouseover); på mobil skal alle relevante kort-handlinger være synlige eller tilgængelige via tap eller en eksplisit "Flere handlinger" (⋯) menu.
- **Don't:** Gør ikke et kort klikbart i sin helhed, hvis det indeholder indbyrdes uafhængige handlinger, valgbokse (checkboxes) eller inline-redigering.
- **Don't:** Brug ikke rød/danger som primær solid fyldknap inde i kort; hold destruktive handlinger som outline eller sekundære tekstlinks.
- **Don't:** Placer aldrig permanente højrisikohandlinger (f.eks. "Slet have permanent") direkte som standard handlinger i et oversigtskort. Højrisikohandlinger skal altid isoleres i en dedikeret `MgpDangerZone` eller under avancerede indstillinger.
- **Edge cases:** På mobilskærme (`<= 640px`) skal flerkolonne-kort (`.card-media`, `.card-compact`, `.card-action`) automatisk folde sammen til enkeltkolonne layout (`1fr`). Ved print fjernes skygger, kanten låses til 1px solid grey, og handlingselementer (`.card-actions`) skjules.

---

## 🧩 Komponentpåvirkning
- **Type:** Foundation / Pattern
- **Nye/ændrede Razor-komponenter:**
  - `MgpCard.razor` (Generisk wrapper med support for varianter, hover-states og accent-border).
  - `MgpEntityCard.razor` (Kort med faste slots: `Header`, `Description`, `MetaRow` og `Actions`).
  - `MgpPlantCard.razor` (Specialiseret enhedskort til botaniske egenskaber og chip-visning).
  - `MgpMediaCard.razor` (Fil- og dokumentkort med integreret thumbnail-preview, support for restricted states og levetidsbadges).
  - `MgpStatusMessage.razor` (Kompakt status- og beskedkomponent med type-specifik dot-indikator).

---

## 🪙 Tokenpåvirkning
- `--mgp-surface`: Kortets standard baggrundsfarve.
- `--mgp-surface-muted`: Billed- eller baggrundsfarve for deaktiverede/sekundære flader.
- `--mgp-border`: Standard kantfarve på alle korttyper.
- `--mgp-primary-soft`: Baggrundsfarve for ikon-cirkel i Action Cards.
- `--mgp-primary-dark`: Ikon- og tekstfarve i aktionselementer.
- `--mgp-accent`: Kant- og dot-farve ved advarsler, opmærksomhed samt midlertidige/udløbende enheder.
- `--mgp-danger`: Dot-farve ved kritiske fejl eller fejlede systemhandlinger.

---

## 💻 CSS & Bootstrap
```css
/* Baseline Bootstrap Card tilpasning med --mgp-* tokens */
.card {
  background: var(--mgp-surface);
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-sm);
  padding: var(--space-md);
}

.card-main {
  display: grid;
  gap: var(--space-sm);
}

.card-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--space-md);
}

.card-description {
  color: var(--mgp-text);
  margin: 0;
  max-width: 68ch;
}

.card-meta-row {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-sm);
}

.card-actions {
  margin-top: var(--space-sm);
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-xs);
}

/* Card Varianter */
.card-entity {
  display: grid;
  gap: var(--space-sm);
}

.card-media {
  display: grid;
  grid-template-columns: 120px 1fr;
  gap: var(--space-md);
  align-items: flex-start;
}

.card-media .thumbnail {
  min-height: 120px;
}

.card-compact {
  display: grid;
  grid-template-columns: 1fr auto auto;
  gap: var(--space-md);
  align-items: center;
  padding: var(--space-sm) var(--space-md);
}

.card-action {
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: var(--space-md);
  align-items: center;
}

.action-icon {
  width: 2.75rem;
  height: 2.75rem;
  border-radius: 999px;
  display: grid;
  place-items: center;
  background: var(--mgp-primary-soft);
  color: var(--mgp-primary-dark);
  font-weight: var(--font-weight-bold);
}

/* Accents, Muted & Selection */
.card-attention {
  border-left: 5px solid var(--mgp-accent);
}

.card-muted {
  background: var(--mgp-surface-muted);
  color: var(--mgp-text-muted);
}

.card-selected {
  border-color: var(--mgp-primary);
  box-shadow: 0 0 0 4px rgba(63, 107, 74, .14);
}

.card-archived {
  background: var(--mgp-surface-muted);
  color: var(--mgp-text-muted);
  border-color: var(--mgp-border);
  opacity: 0.90;
}

.card-archived .card-title {
  color: var(--mgp-text-muted);
}

/* Interaktivitet & Hover-states */
.card-clickable,
.compact-entity-link {
  cursor: pointer;
  color: inherit;
  text-decoration: none;
  transition: transform .15s ease, box-shadow .15s ease, border-color .15s ease, background-color .15s ease;
}

.card-clickable:hover,
.compact-entity-link:hover {
  color: inherit;
  transform: translateY(-1px);
  box-shadow: var(--shadow-md);
  border-color: rgba(63, 107, 74, .28);
}

.card-clickable:focus-within {
  border-color: var(--mgp-primary);
  box-shadow: 0 0 0 4px rgba(63, 107, 74, .14);
}

/* Status Message Card */
.status-message {
  display: flex;
  gap: var(--space-sm);
  align-items: flex-start;
  padding: var(--space-sm);
  border-radius: var(--radius-md);
  border: 1px solid var(--mgp-border);
  background: var(--mgp-surface);
}

.status-message-content {
  display: flex;
  justify-content: space-between;
  gap: var(--space-md);
  align-items: center;
  width: 100%;
}

.status-dot {
  width: .75rem;
  height: .75rem;
  border-radius: 50%;
  margin-top: .42rem;
  flex: 0 0 auto;
  background: var(--mgp-text-muted);
}

.status-success .status-dot { background: var(--mgp-primary); }
.status-info .status-dot    { background: var(--mgp-text-muted); }
.status-warning .status-dot { background: var(--mgp-accent); }
.status-danger .status-dot  { background: var(--mgp-danger); }
.status-processing .status-dot { background: var(--mgp-primary-dark); }
.status-restricted .status-dot { background: var(--mgp-text-muted); }

/* Scope Modifiers */
.status-page { margin-bottom: var(--space-lg); }
.status-section { margin-bottom: var(--space-md); }
.status-form { margin-bottom: var(--space-md); }
.status-object { padding: var(--space-xs) var(--space-sm); font-size: var(--font-size-sm); }

/* Mobil & Print tilpasninger */
@media (max-width: 640px) {
  .card-media,
  .card-compact,
  .card-action {
    grid-template-columns: 1fr;
  }

  .status-message-content {
    align-items: flex-start;
    flex-direction: column;
  }

  .action-icon {
    width: 2.5rem;
    height: 2.5rem;
  }
}

@media print {
  .card,
  .status-message {
    box-shadow: none !important;
    border: 1px solid #ccc !important;
    break-inside: avoid;
  }

  .card-actions,
  .btn-row {
    display: none !important;
  }
}
```