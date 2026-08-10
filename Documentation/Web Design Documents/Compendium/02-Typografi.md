### 🛠️ Berørte filer
- `02-Typografi.md`: Oprettelse og fuld strukturering af typografidokumentationen ud fra råmaterialet.

---

### 📄 02-Typografi.md

# 02 - Typografi

## 📘 Grundregel
Anvend en ren, rolig og performant systemtypografi uden eksterne font-loads i version 1 (`system-ui`). Typografien skal sikre øjeblikkelig indlæsning, høj tilgængelighed på tværs af platforme (iOS, Android, Windows, macOS) og et tydeligt visuelt hierarki, der fremhæver fagligt indhold (haver, planter, materialer) frem for dekorativ styling.

---

## 🔍 Anvendelse / varianter / typer

### Typografisk hierarki

| Text niveau | Token / Klasse | Størrelse / Vægt | Line-height | Formål / Anvendelse |
| :--- | :--- | :--- | :--- | :--- |
| **H1 (Sideoverskrift)** | `h1`, `.h1` | `clamp(2rem, 4vw, 3.25rem)` / 800 | 1.15 | Hovedoverskrifter på dashboard, have-detalje og hovedsektioner. |
| **H2 (Sektionstitel)** | `h2`, `.h2` | `clamp(1.35rem, 2.2vw, 2rem)` / 800 | 1.15 | Sektionsopdeling (f.eks. "Bede", "Seneste filer", "Medlemmer"). |
| **H3 (Card-titel)** | `h3`, `.h3` | `1.08rem` (`--mgp-font-size-md`) / 800 | 1.2 | Titler på kort (f.eks. plantenavn, materialenavn, dokumentnavn). |
| **Brødtekst** | `p`, `body` | `1rem` (`--mgp-font-size-base`) / 400 | 1.55 | Almindelig læsetekst, beskrivelser. Max bredde 68ch. |
| **Metadata / Sekundær**| `.meta` | `0.9rem` (`--mgp-font-size-sm`) / 400 | 1.4 | Datoer, filtyper, sekundære noter, dæmpet tekst. |
| **Labels & Formular** | `label`, `.label` | `0.92rem` / 750 (semibold/bold) | 1.2 | Formular-labels med høj kontrast for hurtig scanning. |
| **Knapper & Nav** | `.btn`, `.nav-link`| `0.95rem` / 750 (semibold/bold) | 1.2 | Handlingselementer og navigationslinks i Sentence case. |
| **Sektions-labels (Nav)**| `.nav-section-title` | `0.78rem` / 750 + UPPERCASE | 1.2 | Små kategori-overskrifter i sidebar/navigation. |

### Domænespecifik typografi

| Type | Klasse | Styling | Eksempel |
| :--- | :--- | :--- | :--- |
| **Latinsk plantenavn** | `.latin-name` | Italic, `--mgp-text-muted`, `.9rem` | *Lavandula angustifolia* |
| **Tal, mål & dimensioner**| `.numeric` | `tabular-nums` | `3,50 × 1,20 m` / `800 × 600 px` |
| **Kodedata & ID'er** | `.mono`, `code`| `--mgp-font-mono` | `UUID-9041-X` |

---

## 🚫 Regler (Do / Don't)
- **Do:** Brug sentence case på knapper (f.eks. "Opret ny have", ikke "OPRET NY HAVE"). UPPERCASE reserveres udelukkende til små sektionstitler i navigationen (`.nav-section-title`).
- **Do:** Brug altid klassen `.numeric` på tal, mål og dimensioner, så cifferbredden er ens og nem at sammenligne vertikalt.
- **Do:** Begræns lang brødtekst til `max-width: 68ch` for optimal læsbarhed.
- **Don't:** Indlæs ikke eksterne webfonte (Google Fonts/Inter/Lora) i v1 af hensyn til ydeevne og offline-først tilgang.
- **Don't:** Gå aldrig under `1rem` (`16px`) på brødtekst på mobilenheder.

---

## 🧩 Komponentpåvirkning
- **Type:** Foundation / Domain Component
- **Nye Razor-komponenter:**
  - `<MgpPlantName>`: Domænekomponent til visning af almindelige og latinske plantenavne.
- **Ændrede komponenter:** Alle UI-komponenter (`<MgpButton>`, `<MgpCard>`, `<MgpInput>`) opdateres til at konsumere de nye typografiske tokens.

```razor
@* Eksempel på domænekomponenten MgpPlantName.razor *@
<div class="d-inline-flex flex-column">
    <span class="fw-bold text-dark">@CommonName</span>
    @if (!string.IsNullOrWhiteSpace(LatinName))
    {
        <span class="latin-name">@LatinName</span>
    }
</div>

@code {
    [Parameter, EditorRequired] public string CommonName { get; set; } = default!;
    [Parameter] public string? LatinName { get; set; }
}
```

---

## 🪙 Tokenpåvirkning
Etablering af typografiske tokens i :root:
--mgp-font-sans, --mgp-font-mono
--mgp-font-size-xs (.78rem), --mgp-font-size-sm (.9rem), --mgp-font-size-base (1rem), --mgp-font-size-md (1.08rem), --mgp-font-size-lg (1.25rem), --mgp-font-size-xl (clamp), --mgp-font-size-xxl (clamp)
--mgp-line-height-tight (1.15), --mgp-line-height-base (1.55), --mgp-line-height-relaxed (1.7)
--mgp-font-weight-normal (400), --mgp-font-weight-bold (750), --mgp-font-weight-heavy (800)
--mgp-letter-spacing-heading (-0.03em)

---

## 💻 CSS & Bootstrap
```css
:root {
  /* Font Families */
  --mgp-font-sans: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
  --mgp-font-mono: ui-monospace, SFMono-Regular, Menlo, Consolas, "Liberation Mono", monospace;

  /* Font Sizes */
  --mgp-font-size-xs: 0.78rem;
  --mgp-font-size-sm: 0.9rem;
  --mgp-font-size-base: 1rem;
  --mgp-font-size-md: 1.08rem;
  --mgp-font-size-lg: 1.25rem;
  --mgp-font-size-xl: clamp(1.35rem, 2.2vw, 2rem);
  --mgp-font-size-xxl: clamp(2rem, 4vw, 3.25rem);

  /* Line Heights */
  --mgp-line-height-tight: 1.15;
  --mgp-line-height-base: 1.55;
  --mgp-line-height-relaxed: 1.7;

  /* Weights */
  --mgp-font-weight-normal: 400;
  --mgp-font-weight-bold: 750;
  --mgp-font-weight-heavy: 800;

  /* Spacing */
  --mgp-letter-spacing-heading: -0.03em;
}

body {
  font-family: var(--mgp-font-sans);
  font-size: var(--mgp-font-size-base);
  line-height: var(--mgp-line-height-base);
  color: var(--mgp-text);
  background-color: var(--mgp-bg);
}

h1, .h1, h2, .h2, h3, .h3 {
  line-height: var(--mgp-line-height-tight);
  letter-spacing: var(--mgp-letter-spacing-heading);
  color: var(--mgp-text);
}

h1, .h1 { font-size: var(--mgp-font-size-xxl); font-weight: var(--mgp-font-weight-heavy); }
h2, .h2 { font-size: var(--mgp-font-size-xl); font-weight: var(--mgp-font-weight-heavy); }
h3, .h3 { font-size: var(--mgp-font-size-md); font-weight: var(--mgp-font-weight-heavy); }

p {
  margin-top: 0;
  max-width: 68ch;
}

.meta {
  font-size: var(--mgp-font-size-sm);
  color: var(--mgp-text-muted);
  line-height: 1.4;
}

label, .label {
  font-size: 0.92rem;
  font-weight: var(--mgp-font-weight-bold);
}

.btn, .nav-link {
  font-size: 0.95rem;
  font-weight: var(--mgp-font-weight-bold);
}

.nav-section-title {
  font-size: var(--mgp-font-size-xs);
  font-weight: var(--mgp-font-weight-bold);
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

/* Domænespecifik typografi */
.latin-name {
  color: var(--mgp-text-muted);
  font-style: italic;
  font-size: var(--mgp-font-size-sm);
}

.numeric {
  font-variant-numeric: tabular-nums;
}

code, .mono {
  font-family: var(--mgp-font-mono);
  font-size: 0.92em;
}
```
