### 📄 01-Farvepalette.md

# 01 - Farvepalette

## 📘 Grundregel
UI'et udformes efter konceptet "Nordisk havearkitekt": En rolig, professionel og neutral ramme med dæmpede naturfarver, der fremhæver brugerens indhold (haver, tegninger, data) uden visuel støj. Farve må aldrig stå alene om at kommunikere status, og alle interaktioner/tilstande skal styres via centraliserede tokens.

---

## 🔍 Anvendelse / varianter / typer

### Farvepalette ("Nordisk havearkitekt")

| Farverolle | Hex / Alpha | Token | Formål / Anvendelse |
| :--- | :--- | :--- | :--- |
| **Primær grøn** | `#3F6B4A` | `--mgp-primary` | Hovedhandlinger, aktiv navigation, links, primær border. |
| **Mørk grøn (hover)** | `#2F5138` | `--mgp-primary-dark` | Hover/active states på primære elementer. |
| **Lys salvie** | `#DDE8D8` | `--mgp-primary-soft` | Sektioner, info-badges, valgte elementer, highlight. |
| **Terracotta accent** | `#B86B4B` | `--mgp-accent` | Sekundære highlights, advarsler (f.eks. udløbende filer/invitationer). |
| **Mørk tekstgrøn** | `#243128` | `--mgp-text` | Overskrifter, brødtekst og primære ikoner (erstatter ren sort). |
| **Varm sand** | `#FAF8F2` | `--mgp-bg` | Applikationsbaggrund (skaber ro vs. klinisk hvid). |
| **Flade (Surface)** | `#FFFFFF` | `--mgp-surface` | Cards, formularfelter, modals. |
| **Dæmpet flade** | `#EFEAE0` | `--mgp-surface-muted` | Sekundære feltbaggrunde, inaktive zoner. |
| **Border / Kant** | `#D8D2C7` | `--mgp-border` | Opdelingslinjer, card-borders, input-kanter. |

### Interaktions- og Tilstandstokens (States & Focus)

| Tilstand / Kant | Værdi / Alpha | Token | Formål / Anvendelse |
| :--- | :--- | :--- | :--- |
| **Border Hover** | `rgba(63, 107, 74, 0.28)` | `--mgp-border-hover` | Sekundær hover-kant på interaktive elementer. |
| **Fokus-ring** | `rgba(63, 107, 74, 0.18)` | `--mgp-focus-ring` | Tastaturfokus (`:focus-visible`) på inputs og knapper. |
| **Destruktiv baggrund** | `#FFF4F3` | `--mgp-danger-bg` | Hover-baggrund på outline destruktive knapper/kort. |
| **Destruktiv kant** | `rgba(159, 58, 56, 0.35)` | `--mgp-danger-border` | Outline-kant på destruktive knapper. |
| **Destruktiv kant hover**| `rgba(159, 58, 56, 0.55)` | `--mgp-danger-border-hover` | Hover-kant på destruktive knapper. |
| **Aktiv / OK** | `#3F6B4A` | `--mgp-success-bg` (`#E4EFE1`) | Aktiv have, gennemført handling, aktivt medlem. |
| **Afventer / Warning** | `#B86B4B` | `--mgp-warning-bg` (`#F8E8D8`) | Pending invitation, fil udløber snart. |
| **Arkiveret / Neutral** | `#8A8F86` | `--mgp-state-archived` | Arkiverede haver, inaktive elementer (tekst, ikoner og borders på arkiverede badges/kort). |
| **Fejl / Slet** | `#9F3A38` | `--mgp-danger` | Tekst og ikon-farve på destruktive elementer. |

---

## 🚫 Regler (Do / Don't)
- **Do:** Brug altid mørk tekstgrøn (`#243128`) frem for ren sort (`#000000`) for et blødere og mere naturligt udtryk.
- **Do:** Suppler altid statusfarver med ikoner og forklarende tekst af hensyn til tilgængelighed (WCAG).
- **Do:** Referér altid til tilstandskanter og focus-rings via deres respektive `--mgp-*` tokens frem for hårdkodede `rgba()`- eller hex-værdier.
- **Do:** Anvend `--mgp-state-archived` til ikoner, tekst og borders på arkiverede elementer og badges, mens fladebaggrunden forbliver dæmpet (`--mgp-surface-muted`).
- **Don't:** Brug ikke stærke lime- eller græsgrønne farver; hold udtrykket dæmpet og professionelt.
- **Don't:** Brug ikke rød (`--mgp-danger`) som generel accentfarve – rød reserveres udelukkende til destruktive handlinger og kritiske fejl.
- **Edge cases:** Ved print eller pdf-eksport overskrives baggrundsfarver (`--mgp-bg`, `--mgp-primary-soft`) til ren hvid (`#FFFFFF`) via `@media print`.

---

## 🧩 Komponentpåvirkning
- **Type:** Foundation
- **Nye Razor-komponenter:** Ingen (grundlæggende CSS-konfiguration).
- **Ændrede komponenter:** Påvirker alle fremtidige komponenter (`<MgpButton>`, `<MgpCard>`, `<MgpBadge>` osv.), som skal hente farver via `--mgp-*` CSS-variablerne.

---

## 🪙 Tokenpåvirkning
Etablering af globale `--mgp-*` design-tokens i CSS `:root`:
- Farver: `--mgp-primary`, `--mgp-primary-dark`, `--mgp-primary-soft`, `--mgp-accent`, `--mgp-text`, `--mgp-bg`, `--mgp-surface`, `--mgp-border`
- Interaktion: `--mgp-border-hover`, `--mgp-focus-ring`
- Destruktiv/Danger: `--mgp-danger`, `--mgp-danger-bg`, `--mgp-danger-border`, `--mgp-danger-border-hover`
- States: `--mgp-warning-bg`, `--mgp-success-bg`, `--mgp-state-archived`

---

## 💻 CSS & Bootstrap

```css
:root {
    /* Brand & Accent */
    --mgp-primary: #3F6B4A;
    --mgp-primary-dark: #2F5138;
    --mgp-primary-soft: #DDE8D8;
    --mgp-accent: #B86B4B;

    /* Typografi */
    --mgp-text: #243128;
    --mgp-text-muted: #6F766D;

    /* Overflader & Baggrunde */
    --mgp-bg: #FAF8F2;
    --mgp-surface: #FFFFFF;
    --mgp-surface-muted: #EFEAE0;

    /* Borders & Interaktion */
    --mgp-border: #D8D2C7;
    --mgp-border-hover: rgba(63, 107, 74, 0.28);
    --mgp-focus-ring: rgba(63, 107, 74, 0.18);

    /* Feedback, States & Destruktive tilstande */
    --mgp-danger: #9F3A38;
    --mgp-danger-bg: #FFF4F3;
    --mgp-danger-border: rgba(159, 58, 56, 0.35);
    --mgp-danger-border-hover: rgba(159, 58, 56, 0.55);
    --mgp-warning-bg: #F8E8D8;
    --mgp-success-bg: #E4EFE1;
    --mgp-state-archived: #8A8F86;
}

/* Mapping og udvidelse af Bootstrap standard-klasser */
body {
    background-color: var(--mgp-bg);
    color: var(--mgp-text);
}

.btn-primary {
    background-color: var(--mgp-primary);
    border-color: var(--mgp-primary);
    color: var(--mgp-surface);
}

.btn-primary:hover,
.btn-primary:focus {
    background-color: var(--mgp-primary-dark);
    border-color: var(--mgp-primary-dark);
}

.card {
    background-color: var(--mgp-surface);
    border-color: var(--mgp-border);
}

.bg-soft {
    background-color: var(--mgp-primary-soft) !important;
}

.text-primary {
    color: var(--mgp-primary) !important;
}

/* Print-overstyring */
@media print {
    body, .card, .bg-soft {
        background-color: #FFFFFF !important;
        color: #000000 !important;
    }
}
```