### 📄 11-Empty State og First Use State.md

# 11 - Empty states og first-use states

## 📘 Grundregel
- Empty states skal altid svare på tre spørgsmål: *Hvad er tomt?*, *Hvorfor er det relevant?* og *Hvad er næste skridt?*. De skal guide brugeren aktivt til den næste naturlige handling, adskille filtrerede, fejlede og låste tilstande fra reelt tomme data, og altid afklare loading før visning.

---

## 🔍 Anvendelse / varianter / typer

| Variant | Bootstrap & CSS modifier | Kontekst & Beskrivelse | Eksempel på handling |
| :--- | :--- | :--- | :--- |
| **First-use empty** | `.empty-state` + `.empty-first-use` | Brugeren har aldrig oprettet noget endnu. Venlig, forklarende og guidende tone. | "Opret have" |
| **Context empty** | `.empty-state` + `.empty-context` | En specifik entity mangler underdata (f.eks. en have uden bede eller filer). Konteksten nævnes i overskriften. | "Opret bed i Villa Solbakken" |
| **Filtered empty** | `.empty-state` + `.empty-filtered` | Data findes i systemet, men ingen poster matcher de aktive filtre. Tydelig visuel adskillelse fra "ingen data overhovedet". | "Nulstil filtre" |
| **Search empty** | `.empty-state` + `.empty-search` | Søgeterm gav ingen resultater. Viser den specifikke søgeterm i copy (fx *"Ingen resultater for 'lavenddel'"*). | "Ryd søgning" |
| **Permission empty** | `.empty-state` + `.empty-restricted` | Data findes, men brugerens rolle/rettighed giver ikke adgang til indholdet. | "Kontakt haveejer" |
| **Processing empty** | `.empty-state` + `.empty-processing` | Midlertidig tilstand (f.eks. thumbnail oprettes eller preview er under generering). | "Vis metadata" / "Download original" |
| **Error empty** | `.empty-state` + `.empty-error` | Data kunne ikke hentes pga. system-, API- eller netværksfejl. Må ikke ligne en almindelig tom visning. | "Prøv igen" |

### Beslutningsflow ved datahentning
Ved indlæsning af enhver datakilde evalueres tilstanden i følgende faste rækkefølge (se `13-Empty_vs_error_vs_no_access.md` for detaljer):
1. **Loading:** Vis skeleton/spinner indtil hentning er afklaret.
2. **Error:** Hvis API/netværk fejlede ➔ vis `.empty-error` med "Prøv igen".
3. **Restricted:** Hvis brugeren mangler rettigheder ➔ vis `.empty-restricted` med forklaring.
4. **Filtered/Search:** Hvis data udelukkes af filtre/søgning ➔ vis `.empty-filtered` / `.empty-search` med nulstil-mulighed.
5. **Empty:** Hvis der reelt ingen data findes ➔ vis `.empty-first-use` / `.empty-context` med opret-handling.

### Skalering og Placering
- **Full-page empty state (`.empty-state`):** Anvendes når en hel side eller hovedvisning mangler indhold (f.eks. ingen haver i hele kontoen).
- **Inline empty state (`.empty-state.empty-state-inline`):** Anvendes i afgrænsede sektioner, cards, paneler eller tabs (f.eks. en have der mangler filer eller medlemmer).

---

## 🚫 Regler (Do / Don't)
- **Do:** Vis altid en loading-tilstand (skeleton eller spinner), indtil dataindlæsningen er endeligt afklaret, for at undgå et uhensigtsmæssigt flash af en empty state.
- **Do:** Hav som udgangspunkt én primær handling knyttet til en empty state (`.btn-primary`). Sekundær handling tillades kun, hvis den direkte hjælper brugeren videre.
- **Do:** Opbyg tekst efter formlen: `[Situation]` + `[Kontekst / Hvorfor]` + `[Handling]`.
- **Do:** Evaluér altid loading-, fejl- og adgangstilstande i henhold til beslutningsflowet, før `MgpEmptyState` renderes.- **Don't:** Brug ikke mekanisk, vag eller teknisk copy som "Ingen data", "0 resultater" eller "Poster ikke fundet".
- **Do:** Search empty states skal altid eksplicit vise den indtastede søgeterm i overskrift eller forklaring (fx *"Ingen resultater for 'lavenddel'"*), så brugeren straks kan identificere og rette eventuelle stavefejl.
- **Don't:** Brug ikke tunge eller spraglede illustrationer. Anvend enkle ikoncirkler med et tydeligt botanisk eller funktionelt symbol (fx ✿, 🌿, ▧, ⌕, 🔒, !).
- **Don't:** Forveksl ikke Error empty eller StatusMessage med en almindelig tom tilstand.
- **Edge cases:** Skelne skarpt mellem `MgpEmptyState` og `MgpStatusMessage`: Hvis et nyligt oprettet eller redigeret element skjules af et aktivt filter eller en søgeterm ved retur til oversigten, skal der vises en inline `MgpStatusMessage` med en direkte *"Nulstil filter"*-handling på den eksisterende liste – **ikke** en ny `MgpEmptyState`.

---

## 🧩 Komponentpåvirkning
- **Type:** Pattern Component
- **Nye Razor-komponenter:**
  - `MgpEmptyState.razor` (Genanvendelig komponent med parametre: Anvendes specifikt ved (`first-use`, `context`, `filtered`, `search`, `restricted`, `processing`, `error`). Desuden `Variant`, `Title`, `Description`, `Icon`, `IsInline` samt render-fragments til primære og sekundære handlinger).
- **Ændrede Razor-komponenter:**
  - Ingen direkte ændringer i eksisterende Razor-komponenter, men komponenten integreres i fremtidige liste- og detaljevisninger.

---

## 🪙 Tokenpåvirkning
- `--mgp-surface`: Baggrund for empty state containeren.
- `--mgp-surface-muted`: Baggrund for ikon-cirklen ved filtered og search states.
- `--mgp-border`: Stiplet kantfarve for containeren (`dashed 1px`).
- `--mgp-primary-soft`: Baggrund for ikon-cirklen ved first-use og context states.
- `--mgp-primary-dark`: Ikon- og tekstfarve i primære ikoncirkler.
- `--mgp-text-muted`: Tekstfarve på sekundære forklaringer og dæmpede ikoner.
- `--mgp-danger-bg`: Lys rød baggrund for ikon-cirklen ved `.empty-error` (`#FFF4F3`).
- `--mgp-danger`: Tekst- og ikonfarve ved fejltilstande (`#9F3A38`).
- `--mgp-danger-border`: Soft rød kantfarve ved fejltilstande (`rgba(159, 58, 56, 0.35)`).

---

## 💻 CSS & Bootstrap

```css
/* Generel Empty State Container */
.empty-state {
  display: grid;
  gap: var(--space-md);
  justify-items: center;
  text-align: center;
  padding: var(--space-2xl) var(--space-lg);
  border: 1px dashed var(--mgp-border);
  border-radius: var(--radius-lg);
  background: var(--mgp-surface);
}

/* Inline Variant (Sektioner / Cards / Tabs) */
.empty-state-inline {
  justify-items: start;
  text-align: left;
  padding: var(--space-lg);
}

/* Empty State Ikon-cirkel */
.empty-icon {
  width: 3rem;
  height: 3rem;
  border-radius: 999px;
  display: grid;
  place-items: center;
  background: var(--mgp-primary-soft);
  color: var(--mgp-primary-dark);
  font-weight: var(--font-weight-bold);
  font-size: 1.4rem;
}

/* Indhold & Typografi */
.empty-content {
  display: grid;
  gap: var(--space-xs);
  max-width: 52ch;
}

.empty-content h2,
.empty-content h3 {
  margin: 0;
  color: var(--mgp-text);
}

.empty-content p {
  margin: 0;
  color: var(--mgp-text-muted);
}

/* Handlingsområde */
.empty-actions {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
  justify-content: center;
}

.empty-state-inline .empty-actions {
  justify-content: flex-start;
}

/* Variant-specifik CSS */
.empty-filtered .empty-icon,
.empty-search .empty-icon {
  background: var(--mgp-surface-muted);
  color: var(--mgp-text-muted);
}

.empty-error {
  border-color: var(--mgp-danger-border);
}

.empty-error .empty-icon {
  background: var(--mgp-danger-bg);
  color: var(--mgp-danger);
}

/* Responsiv tilpasning (Mobil <= 640px) */
@media (max-width: 640px) {
  .empty-state {
    padding: var(--space-xl) var(--space-md);
  }

  .empty-actions {
    width: 100%;
  }

  .empty-actions .btn {
    width: 100%;
  }
}
```