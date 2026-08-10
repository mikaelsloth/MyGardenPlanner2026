### 📄 06-Formularer.md

# 06 - Formularer

## 📘 Grundregel
- Formularer opbygges som struktureret og guidet arbejde frem for rå databaseindtastning. Felter indrettes i en konsekvent tre-lags opbygning (label, input/control, help/error-besked) med reserveret vertikal plads for at forhindre layout-hop ved validering. Synlige labels er altid påkrævet, og alle formularhandlinger samles i bunden.

---

## 🔍 Anvendelse / varianter / typer

| Formulartype | Bootstrap & CSS basis | Brugerkontekst |
| :--- | :--- | :--- |
| **Simple form** | `.form` | Korte formularer med få felter (f.eks. login, enkle indstillinger). |
| **Entity form** | `.form` + `.form-grid` | Opret/rediger stamdata for Have, Bed, Plante, Materiale eller Butik. |
| **Sectioned form** | `.form` + `.form-section.card` | Lange formularer opdelt i opgavespecifikke kort-sektioner. |
| **Inline form** | `.form-inline` | Hurtig redigering af enkelte felter direkte i kort eller lister. |
| **Upload form** | `.upload-zone` | Upload og vedhæftning af tegninger, dokumenter og thumbnails. Skal altid indeholde synlig filvælgerknap. |
| **Filter / search form** | `.filter-bar` | Kompakt søge- og filtreringslinje sammensat af `MgpSearchInput`, `MgpFilterBar`, `MgpSortSelect` og `MgpViewModeToggle` placeret over oversigtslister. |
| **Read-only form** | `.read-only-section` + `MgpReadOnlySection` | Formular eller sektion uden redigeringsadgang. Viser data i et rent tekstbaseret præsentationslayout frem for deaktiverede felter. |

---

## 🚫 Regler (Do / Don't)
- **Do:** Alle formularfelter skal have en synlig label (`.field-label`) eller en tydelig programmatisk label (`aria-label`/`aria-labelledby`). Placeholder må **aldrig** benyttes som eneste identifikation af et felt.
- **Do:** Reserver vertikal plads til hjælpe- og fejltekster (`min-height: 1.25em`) i alle felter for at sikre en stabil formularrytme uden layout-hop ved valideringsfejl.
- **Do:** Markér kun felter med "Påkrævet" eller "Valgfri" i label-teksten, hvor det giver reel værdi for brugeren. Undgå at overmarkere alle felter.
- **Do:** Sørg for at `.upload-zone` altid har en eksplicit, tastatur-fokusérbar knap ("Vælg fil"). Drag-and-drop må aldrig stå som eneste mulighed.
- **Do:** Formular- og filtervisninger skal tydeligt skelne mellem tre tilstande: **Loading** (henter data), **Filtered empty** (`MgpEmptyState` med modifier `.empty-filtered` når intet matcher søge/filterkriterier) og **Validerings-/Fejlstatus** (`MgpStatusMessage`).
- **Do:** Hvis brugeren har læseadgang, men ikke redigeringsadgang til en hel formular, skal data præsenteres via et læsevisningslayout (`MgpReadOnlySection`) i stedet for at vise deaktiverede formularkontroller overalt. Deaktiverede felter reserveres til enkelte felter i et ellers redigerbart skema.
- **Do:** Vis feltvalideringsfejl direkte ved det enkelte felt via `.field-message`. Formularbrede fejl, overordnede valideringsmeddelelser og handlingsresultater skal vises som en persistent inline `MgpStatusMessage` (fx `.status-form.status-danger`) placeret øverst i formularen – og må **aldrig** vises udelukkende som en toast.
- **Don't:** Anvend ikke omfattende inline-redigering direkte i tabeller eller compact rows. Inline-redigering i datatabeller begrænses til enkle skift (fx toggle eller statusvalg). Kompleks dataredigering skal altid foregå via dedikerede formularer, modaler eller `Detail View`.
- **Don't:** Placer ikke Gem-/Annuller-knapper under hver enkel sektion i lange formularer; saml altid den samlede formularhandling i `.form-actions` nederst.
- **Don't:** Brug ikke inline-redigering (`Inline form`) til komplekse data eller formularer med mange afhængigheder.
- **Edge cases:** På mobilskærme (<= 640px) skal alle formularer og grid-layouts automatisk folde sammen til 1 kolonne med synlige labels og feltvalidering direkte ved felterne. Handlingsknapper (.form-actions .btn) skal strække sig i fuld bredde (width: 100%), og touch targets på alle inputs og knapper skal overholde minimum 44px i højden.

---

## 🧩 Komponentpåvirkning
- **Type:** Pattern
- **Nye/ændrede Razor-komponenter:**
  - `MgpFormField.razor` (Enkelt felt-wrapper der styrer label, required/optional-indikator, control-slot samt reserveret plads til fejl/hjælpetekst).
  - `MgpFormSection.razor` (Sektions-card til opdeling af lange formularer med support for validerings-accent).
  - `MgpUploadZone.razor` (Drag-and-drop filupload-zone med ikon, explicit filvælgerknap, tilstandsfeedback og knap).
  - `MgpFilterBar.razor` (Kompakt søge- og filtrerings-bar til listevisninger).
  - `MgpSearchInput.razor` (Søgefelt med debounce, integreret loader, clear-knap og `aria-label`).
  - `MgpSortSelect.razor` (Dropdown-komponent til valg af sorteringsfelt og retning).
  - `MgpViewModeToggle.razor` (Knapgruppe til visningsskift mellem cards, liste, tabel og thumbnail grid).

---

## 🪙 Tokenpåvirkning
- `--mgp-surface`: Baggrund for inputs, textareas, selects og upload-zoner.
- `--mgp-border`: Standard kantfarve på feltkontroller og sektioner.
- `--mgp-primary`: Kantfarve og focus-ring ved aktiv feltfokus.
- `--mgp-focus-ring`: Puls/skygge ved feltfokus (`rgba(63, 107, 74, 0.18)`).
- `--mgp-text`: Tekstfarve i felter og overskrifter.
- `--mgp-text-muted`: Farve til hjælpetekst, optional-labels og form-intro.
- `--mgp-danger`: Kant- og tekstfarve ved valideringsfejl.
- `--mgp-danger-bg`: Dæmpet tint ved kritiske feltfejl.

---

## 💻 CSS & Bootstrap

```css
/* Formular grundstruktur */
.form {
  display: grid;
  gap: var(--space-lg);
}

.form-header {
  display: grid;
  gap: var(--space-xs);
}

.form-intro {
  color: var(--mgp-text-muted);
  max-width: 68ch;
  margin: 0;
}

.form-section {
  display: grid;
  gap: var(--space-md);
}

.form-section-header {
  display: flex;
  justify-content: space-between;
  gap: var(--space-md);
  align-items: flex-start;
}

.form-section-error {
  border-left: 5px solid var(--mgp-danger);
}

/* Formular Grid Layouts */
.form-grid {
  display: grid;
  gap: var(--space-md);
}

.form-grid-2 { grid-template-columns: repeat(2, minmax(0, 1fr)); }
.form-grid-3 { grid-template-columns: repeat(3, minmax(0, 1fr)); }
.form-grid-4 { grid-template-columns: repeat(4, minmax(0, 1fr)); }

/* Felt-struktur (3-lags opbygning) */
.field {
  display: grid;
  gap: var(--space-xs);
}

.field-label {
  display: flex;
  gap: var(--space-xs);
  align-items: baseline;
  flex-wrap: wrap;
  font-size: .92rem;
  font-weight: var(--font-weight-bold);
  color: var(--mgp-text);
}

.required-label,
.optional-label {
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-medium);
  color: var(--mgp-text-muted);
}

/* Controls (Bootstrap override & udvidelse) */
.form-control,
.form-select,
.form-textarea {
  width: 100%;
  border: 1px solid var(--mgp-border);
  background: var(--mgp-surface);
  color: var(--mgp-text);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
  font: inherit;
  outline: none;
  transition: border-color .15s ease, box-shadow .15s ease;
}

.form-control:focus,
.form-select:focus,
.form-textarea:focus {
  border-color: var(--mgp-primary);
  box-shadow: 0 0 0 4px var(--mgp-focus-ring);
}

.form-textarea {
  min-height: 7rem;
  resize: vertical;
}

/* Hjælpe- og fejltekst (Stabil vertikal plads) */
.help,
.field-message {
  font-size: .84rem;
  min-height: 1.25em;
}

.help {
  color: var(--mgp-text-muted);
}

.help-empty {
  visibility: hidden;
}

.field-message {
  color: var(--mgp-danger);
}

.field-error .form-control,
.field-error .form-select,
.field-error .form-textarea {
  border-color: var(--mgp-danger);
  box-shadow: 0 0 0 4px rgba(159, 58, 56, .12);
}

/* Toggles & Checkbox Rækker */
.check-row {
  display: flex;
  gap: var(--space-sm);
  align-items: flex-start;
  padding: var(--space-sm) var(--space-md);
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-md);
  background: var(--mgp-surface);
}

/* Upload Zone */
.upload-zone {
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: var(--space-md);
  align-items: center;
  padding: var(--space-lg);
  border: 1px dashed rgba(63, 107, 74, .35);
  border-radius: var(--radius-lg);
  background: var(--mgp-surface);
}

.upload-icon {
  width: 2.75rem;
  height: 2.75rem;
  border-radius: 999px;
  display: grid;
  place-items: center;
  background: var(--mgp-primary-soft);
  color: var(--mgp-primary-dark);
  font-weight: var(--font-weight-bold);
}

/* Filter Bar */
.filter-bar {
  display: grid;
  grid-template-columns: 1fr auto auto;
  gap: var(--space-md);
  align-items: end;
}

/* Form Actions Layout */
.form-actions {
  display: flex;
  gap: var(--space-sm);
  align-items: center;
  margin-top: var(--space-md);
}

.form-actions-split {
  display: flex;
  justify-content: space-between;
  gap: var(--space-md);
  align-items: center;
  width: 100%;
}

/* Mobil & Responsive Tilpasninger */
@media (max-width: 640px) {
  .form-grid-2,
  .form-grid-3,
  .form-grid-4,
  .filter-bar,
  .upload-zone {
    grid-template-columns: 1fr;
  }

  .form-actions {
    flex-direction: column;
  }

  .form-actions .btn {
    width: 100%;
    justify-content: center;
  }

  .form-actions-split {
    flex-direction: column-reverse;
    align-items: stretch;
  }
}

/* Print Overstyring */
@media print {
  .form-actions,
  .upload-zone,
  .filter-bar {
    display: none !important;
  }

  .form-control,
  .form-select,
  .form-textarea {
    border: 0 !important;
    background: transparent !important;
    padding: 0 !important;
  }
}
```