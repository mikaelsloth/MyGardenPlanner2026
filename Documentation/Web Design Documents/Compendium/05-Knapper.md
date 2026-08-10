### 📄 05-Knapper.md

# 05 - Knapper

## 📘 Grundregel
- Én tydelig primær handling pr. område. Sekundære handlinger skal holdes visuelt dæmpede, og destruktive handlinger skal udformes som outline (aldrig fyldt rød som standard) for at forhindre støj i brugergrænsefladen.

---

## 🔍 Anvendelse / varianter / typer

### Knaphierarki og typer

| Knaptype | Bootstrap / Custom klasse | Brugssituation | Visuel Styrke |
| :--- | :--- | :--- | :--- |
| **Primary** | `.btn-primary` | Hovedhandling (f.eks. Gem, Opret, Upload, Åbn have). Max 1 pr. sektion. | Høj |
| **Secondary** | `.btn-secondary` | Standard for øvrige handlinger (Redigér, Annullér, Download, Detaljer). Anvendes til filterhandlinger som *"Nulstil alle"*, *"Ryd søgning"* og *"Anvend filtre"*. | Medium/Lav |
| **Accent** | `.btn-accent` | Workflow-skift og varme highlights (f.eks. Invitér kunde, Send igen). | Medium |
| **Danger** | `.btn-danger` | Destruktive handlinger (Slet, Fjern adgang). Fyldes ikke ud som standard. | Tydelig (Outline) |
| **Ghost / Text** | `.btn-ghost` | Lavprioriterede handlinger i tætte layouts (f.eks. filterhandlinger som *"Nulstil alle"*, *"Ryd søgning"* og *"Anvend filtre"*, Vis mere). | Lav |
| **Icon Button** | `.btn-icon` | Kompakte handlinger i lister og kort. Skal altid ledsages af `aria-label`. | Lav/Medium |

### Størrelser

| Størrelse | Klasse | Anvendelse |
| :--- | :--- | :--- |
| **Small** | `.btn-sm` | Compact cards, tabeller, datagrid-rækker. |
| **Default** | `.btn` | Standard formularer, kort-handlinger og modals. |
| **Large** | `.btn-lg` | Hero-sektioner og vigtige landingsside-handlinger. |

### Layout & Alignment patterns

1. **Formularhandlinger (`.form-actions`):** Nederst i formularer, placeres primær handling først eller sidst efter standardflow.
2. **Felt-aligned knap (`.button-aligned`):** Bruges til knapper i samme grid-række som input/select (f.eks. filter-bar), så knappen flugter med selve feltet og ikke feltets label.
3. **Kort-handlinger (`.card-actions`):** Tættere knaprække i bunden af kort.
4. **Mobil responsivitet (`.btn-mobile-full`):** Fuld bredde på mobilskærme (< 640px) i formular-flows.

---

## ## 🔍 Anvendelse / varianter / typer

### Knaphierarki og typer

| Knaptype | Bootstrap / Custom klasse | Brugssituation | Visuel Styrke |
| :--- | :--- | :--- | :--- |
| **Primary** | `.btn-primary` | Hovedhandling (f.eks. Gem, Opret, Upload, Åbn have). Max 1 pr. sektion. | Høj |
| **Secondary** | `.btn-secondary` | Standard for øvrige handlinger (Redigér, Annullér, Download, Detaljer). | Medium/Lav |
| **Accent** | `.btn-accent` | Workflow-skift og varme highlights (f.eks. Invitér kunde, Send igen). | Medium |
| **Danger** | `.btn-danger` | Destruktive handlinger (Slet, Fjern adgang). Fyldes ikke ud som standard. | Tydelig (Outline) |
| **Ghost / Text** | `.btn-ghost` | Lavprioriterede handlinger i tætte layouts (f.eks. Nulstil filter, Vis mere, Ryd søgning). | Lav |
| **Icon Button** | `.btn-icon` | Kompakte handlinger i lister og kort. Skal altid ledsages af `aria-label`. | Lav/Medium |

### Størrelser

| Størrelse | Klasse | Anvendelse |
| :--- | :--- | :--- |
| **Small** | `.btn-sm` | Compact cards, tabeller, datagrid-rækker. |
| **Default** | `.btn` | Standard formularer, kort-handlinger og modals. |
| **Large** | `.btn-lg` | Hero-sektioner og vigtige landingsside-handlinger. |

### Layout & Alignment patterns

1. **Formularhandlinger (`.form-actions`):** Nederst i formularer, placeres primær handling først eller sidst efter standardflow.
2. **Felt-aligned knap (`.button-aligned`):** Bruges til knapper i samme grid-række som input/select (f.eks. filter-bar), så knappen flugter med selve feltet og ikke feltets label.
3. **Kort-handlinger (`.card-actions`):** Tættere knaprække i bunden af kort.
4. **Mobil responsivitet (`.btn-mobile-full`):** Fuld bredde på mobilskærme (< 640px) i formular-flows.
5. **Opdelte formularhandlinger (`.form-actions-split`):** Anvendes i bunden af formularer med destruktive muligheder. Gem og Annullér samles i en `.btn-row` til venstre/start, mens `.btn-danger` placeres til højre/slut via `justify-content: space-between`.

---

## 🚫 Regler (Do / Don't)
- **Do:** Brug altid konkrete, handlingsorienterede labels (f.eks. "Opret have" i stedet for "OK" eller "Submit").
- **Do:** Brug `.button-aligned` med dummy label-placeholder og helper-placeholder, når en knap står i et felt-grid ved siden af formularfelter.
- **Do:** Forsyn altid ikon-knapper uden synlig tekst (`.btn-icon`) med et beskrivende `aria-label` eller en visuelt skjult tekst (`.sr-only`).
- **Do:** Empty states skal som udgangspunkt kun indeholde én primær handling (`.btn-primary`). Sekundære handlinger anvendes kun, hvis de direkte hjælper brugeren videre (fx *"Importér data"* eller *"Se eksempler"*).
- **Do:** Knapper skal deaktiveres (`disabled`), vise `.btn-spinner` og skifte tekst til en aktiv bydeform/handlingsform (fx "Gemmer...", "Uploader...", "Sletter...") for at forhindre dobbeltklik og bekræfte handlingen direkte.
- **Do:** Overhold *State-action-reglen*: Knappens primære funktion i tilstandsvisninger skal svare direkte på årsagen til tilstanden (Empty ➔ Opret/Upload, Filtered ➔ Nulstil, Error ➔ Prøv igen, No Access ➔ Kontakt/Anmod).
- **Do:** Ved destruktive handlinger skal bekræftelsesknappen være af typen `.btn-danger`, have en handlingsspecifik label (f.eks. *"Slet fil"* eller *"Fjern adgang"*) og placeres til højre/sidst i handlingselementer (`.form-actions-split` eller modal-actions).
- **Do:** Gør altid brug af `.permission-hint` (eller tooltip/hjælpetekst) ved deaktiverede knapper for at forklare rollebetingede begrænsninger (f.eks. *"Kun haveejere kan slette filer"*).
- **Do:** Anvend eksplicitte, kontekstuelle filter-handlingstekster som *"Anvend filtre"*, *"Nulstil alle"* og *"Ryd søgning"*. Filter-handlinger i filterbaren skal holdes visuelt dæmpede (Secondary eller Ghost) for ikke at stjæle opmærksomhed fra sidens primære handling (`.btn-primary`).
- **Do:** Sikr at alle knapper har et finger-venligt klikbart område på minimum 44×44 px (`--mgp-touch-target-min`).
- **Do:** Bevar altid en synlig og kontrastrig focus state (`:focus-visible`) ved tastaturnavigation (`outline: 3px solid var(--mgp-primary-dark); outline-offset: 2px;`).
- **Don't:** Vis ikke deaktiverede knapper for administrative eller destruktive handlinger, som brugerens rolle aldrig vil kunne udføre – skjule dem helt fra UI'et.
- **Don't:** Brug aldrig `.btn-primary` til destruktive eller irreversible handlinger, og undgå uspecifikke knaptekster som *"OK"* eller *"Ja"*.
- **Don't:** Brug ikke mere end én `.btn-primary` pr. card eller sektion.
- **Don't:** Brug ikke fyldt rød baggrund til destruktive knapper; brugen af outline forhindrer et aggressivt UI.
- **Don't:** Gør ikke automatisk alle card-actions til fuld bredde på mobil; reserver `.btn-mobile-full` til primære formularknapper.
- **Edge cases:** Klikbare kort (hvor hele kortet fungerer som link) må ikke indeholde uafhængige interaktive knapper af hensyn til HTML-validitet og tilgængelighed (WCAG).

---

## 🧩 Komponentpåvirkning
- **Type:** Pattern / Foundation Component
- **Nye/ændrede Razor-komponenter:**
  - `<MgpButton>`: Generisk knap-komponent med parametre: `Variant` (Primary, Secondary, Accent, Danger, Ghost), `Size` (Sm, Default, Lg), `IsIconOnly` (bool), `IsLoading` (bool), `Disabled` (bool), `OnClick` (EventCallback).
  - `<MgpFormActions>`: Wrapper-komponent til formularknapper i bunden af et skema.
  - `<MgpButtonAligned>`: Wrapper-komponent til inline-knapper i filter-barer eller grid-felter.

---

## 🪙 Tokenpåvirkning
Genbruger og kobler direkte til de definerede globale interaktions- og border-tokens fra `01_Farvepalette.md`:
- `--mgp-primary`, `--mgp-primary-dark`, `--mgp-primary-soft`, `--mgp-accent`, `--mgp-surface`, `--mgp-surface-muted`, `--mgp-border`
- `--mgp-border-hover`: Anvendes til `.btn-secondary:hover`.
- `--mgp-focus-ring`: Anvendes til `.btn:focus-visible`.
- `--mgp-danger`, `--mgp-danger-bg`, `--mgp-danger-border`, `--mgp-danger-border-hover`: Anvendes til `.btn-danger` tilstande.

---

## 💻 CSS & Bootstrap

```css
/* ==========================================================================
   MGP Knapvarianter (Udvidelser af Bootstrap .btn)
   ========================================================================== */

/* Primary Button */
.btn-primary {
  background-color: var(--mgp-primary);
  color: var(--mgp-surface);
  border-color: var(--mgp-primary);
}

.btn-primary:hover,
.btn-primary:focus-visible {
  background-color: var(--mgp-primary-dark);
  border-color: var(--mgp-primary-dark);
  color: var(--mgp-surface);
}

/* Secondary Button */
.btn-secondary {
  background-color: var(--mgp-surface);
  color: var(--mgp-primary-dark);
  border-color: var(--mgp-border);
}

.btn-secondary:hover,
.btn-secondary:focus-visible {
  background-color: var(--mgp-surface-muted);
  border-color: var(--mgp-border-hover);
  color: var(--mgp-primary-dark);
}

/* Accent Button */
.btn-accent {
  background-color: var(--mgp-accent);
  color: var(--mgp-surface);
  border-color: var(--mgp-accent);
}

.btn-accent:hover,
.btn-accent:focus-visible {
  filter: brightness(0.95);
  color: var(--mgp-surface);
}

/* Danger Button (Outline standard via tokens) */
.btn-danger {
  background-color: var(--mgp-surface);
  color: var(--mgp-danger);
  border-color: var(--mgp-danger-border);
}

.btn-danger:hover,
.btn-danger:focus-visible {
  background-color: var(--mgp-danger-bg);
  border-color: var(--mgp-danger-border-hover);
  color: var(--mgp-danger);
}

/* Ghost / Text Button */
.btn-ghost {
  background-color: transparent;
  color: var(--mgp-primary-dark);
  border-color: transparent;
  padding-inline: var(--space-xs);
}

.btn-ghost:hover,
.btn-ghost:focus-visible {
  background-color: var(--mgp-primary-soft);
  color: var(--mgp-primary-dark);
}

/* Icon Button */
.btn-icon {
  width: 2.5rem;
  height: 2.5rem;
  padding: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

/* Knapstørrelser */
.btn-sm {
  padding: var(--space-xs) var(--space-sm);
  font-size: var(--font-size-sm);
}

.btn {
  padding: var(--space-sm) var(--space-md);
  font-size: 0.95rem;
}

.btn-lg {
  padding: var(--space-md) var(--space-lg);
  font-size: var(--font-size-base);
}

/* States: Focus, Disabled & Loading */
.btn:focus-visible {
  outline: none;
  box-shadow: 0 0 0 4px var(--mgp-focus-ring);
}

.btn:disabled,
.btn[aria-disabled="true"] {
  opacity: 0.55;
  cursor: not-allowed;
  pointer-events: none;
}

.btn-spinner {
  width: 1em;
  height: 1em;
  border: 2px solid currentColor;
  border-right-color: transparent;
  border-radius: 999px;
  animation: spin 0.7s linear infinite;
  display: inline-block;
  margin-right: var(--space-xs);
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Layout Grupper & Alignments */
.btn-row {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
  align-items: center;
}

.card-actions {
  margin-top: var(--space-sm);
  display: flex;
  gap: var(--space-xs);
  flex-wrap: wrap;
}

.form-actions {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
  margin-top: var(--space-lg);
}

.form-actions-split {
  display: flex;
  justify-content: space-between;
  gap: var(--space-md);
  align-items: center;
  width: 100%;
  margin-top: var(--space-lg);
}

/* Grid-aligned knapper i feltgrupper */
.button-aligned {
  display: grid;
  gap: var(--space-xs);
  align-self: start;
}

.button-aligned .btn {
  align-self: start;
}

.field-label-placeholder {
  visibility: hidden;
  font-size: 0.92rem;
  font-weight: var(--font-weight-bold);
  line-height: 1.4;
  min-height: 1.4em;
}

/* Responsiv adfærd */
@media (max-width: 640px) {
  .btn-mobile-full,
  .form-actions .btn {
    width: 100%;
    justify-content: center;
  }
  
  .form-actions-split {
    flex-direction: column-reverse;
    align-items: stretch;
  }  
}
```