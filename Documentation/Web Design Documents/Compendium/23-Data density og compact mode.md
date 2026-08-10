### 📄 23-Data density og compact mode.md

# 23 - Data density og compact mode

## 📘 Grundregel
- Informationstæthed (density) skal tilpasses brugerens opgave: Browsing og onboarding kræver visuel luft, mens intensivt dataarbejde kræver høj kompakthed. Standardvisningen (*Default*) udgør appens normale rytme. *Comfortable* anvendes til overblik og dashboards, mens *Compact* anvendes til lange lister, tabeller, søgeresultater og print. Compact mode må reducere spacing, metadata og synlige knapper, men må **aldrig** forringe tilgængelighed, mindske mobile touch targets under 44px eller sløre kritiske advarsler og confirmations.

---

## 🔍 Anvendelse / varianter / typer

### 1. Tre Density Levels

| Level | CSS Attribute / Token | Spacing & Indhold | Brugerkontekst & Sider |
| :--- | :--- | :--- | :--- |
| **Comfortable** | `[data-density="comfortable"]` | Max luft: Padding 16–24px, fuld metadata, 2–3 synlige knapper, store thumbnails. | Dashboard, landingssider, first-use onboarding, summary cards, havelister med få elementer. |
| **Default** | `[data-density="default"]` | Balanceret: Padding 12–16px, standard metadata, 1–2 synlige knapper. | Standard entitetskort, almindelige lister, formularer, detail-sider, filsektioner. |
| **Compact** | `[data-density="compact"]` | Tæt & effektiv: Padding 6–12px, prioriteret metadata (kun kernefelter), 1 synlig action (`[Vis]`). | Store plantelister, materialetabeller, søge- og filterresultater, printrapporter, admin-lister. |

---

### 2. View Mode vs. Density

> **Skelnen:** *View mode* ændrer selve præsentationsformen (f.eks. fra Cards til Table) og styres via URL-state (`?view=...`). *Density* ændrer informationstæthed, spacing og padding inden for den valgte visning og styres via komponent- eller kontekstlogik (`data-density="..."`).

| View Mode (`?view=...`) | Standard Density Level | Tilladte Density Niveauer |
| :--- | :--- | :--- |
| **Cards** (`view=cards`) | Default | Comfortable, Default, Compact (forsigtigt) |
| **Compact Rows** (`view=compact`) | Compact | Compact, Default |
| **Tables** (`view=table`) | Compact | Compact, Default |
| **Thumbnail Grid** (`view=grid`) | Default | Comfortable, Default |
| **Detail View** (`view=detail`) | Default | Default (Main) / Compact (Sidekolonne) |
| **Print Table** (`view=print`) | Compact | Compact (Document mode) |

---

### 3. Default Density pr. App-Område

| Domæneområde | Default Density | Begrundelse |
| :--- | :--- | :--- |
| **Dashboard & First-use** | Comfortable | Fokus på ro, overblik og onboarding. |
| **Haver & Bede** | Default / Comfortable | Rolig browsing af primære enheder. |
| **Planter (Browsing)** | Default | Visuel identifikation med billeder og badges. |
| **Planter (Søgning/Filter)** | Compact | Hurtig scanning og sammenligning af lange lister. |
| **Materialer & Filer (Liste)** | Compact | Strukturerede data og dokumentarkiv. |
| **Detail Page Content** | Default | Hovedindhold kræver læsero. |
| **Detail Page Sidebar** | Default / Compact | Relaterede lister og historik kan pakkes tættere. |
| **Confirmations & Dialoger** | Default / Comfortable | **Må aldrig blive compact** for at undgå fejlhandlinger. |

---

## 🚫 Regler (Do / Don't)
- **Do:** Styr density deklarativt via HTML-attributten `data-density="comfortable|default|compact"` på containere eller komponenter, så CSS-variabler overstyrer spacing automatisk på tværs af børne-komponenter.
- **Do:** På mobilskærme (`<= 640px`) skal compact mode primært reducere datamængde, tekstlængde og badges — **aldrig** touch targets. Interaktive flader skal opretholde minimum 44×44 px.
- **Do:** Begræns antallet af synlige handlinger i compact visning til 1 primær knap (fx `[Vis]`). Sekundære handlinger placeres i en overflow-menu (`⋯`).
- **Do:** Nedskaler metadata aggressivt ved compact density: Vis kun titel, primær underoverskrift og 1–2 kritiske nøgleværdier (f.eks. sol/staude).
- **Do:** Ved `data-density="compact"` må visuel vægt på attention summary og forklarende hjælpetekster reduceres (fx ved at komprimere teksten), men selve attention-indikatoren og kritiske advarsler (**Level 2 & 3**) må **aldrig** skjules helt.
- **Don't:** Anvend **aldrig** compact density på confirmation-dialoger, danger zones, fejlmeddelelser (`.status-danger`), adgangsbegrænsninger (`.status-restricted`) eller onboarding-flow.
- **Don't:** Opret ikke særskilte C#-komponenter til hvert density-niveau. Brug i stedet parametre (fx `Density="DensityLevel.Compact"`) på eksisterende Razor-komponenter.
- **Don't:** Gem ikke density som en separat URL-parameter. Brug i stedet `view`-parameteren i URL'en (`/planter?view=compact`), da view mode implicit sætter den forventede tæthed.
- **Edge cases:** Ved sammensatte detail-sider (fx Havedetalje) bør main content bruge `Default` density, mens sekundære sidekolonnekort (fx relaterede filer eller medlemsliste) kan skifte til `Compact` density for at spare lodret plads.

---

## 🧩 Komponentpåvirkning
- **Type:** Foundation / Layout Pattern
- **Nye Razor-komponenter:** Ingen (density tilføjes som en tværgående egenskap/parameter).
- **Ændrede Razor-komponenter:**
  - `MgpEntityCard.razor`: Tilføjet support for `[Parameter] public DensityLevel Density { get; set; } = DensityLevel.Default;`.
  - `MgpCompactEntityRow.razor`: Optimeret til altid at understøtte `data-density="compact"`.
  - `MgpDataTable.razor`: Tilpasset med automatisk skift af cell-padding ud fra aktiv `Density`.
  - `MgpFilterBar.razor`: Tilpasset til at understøtte kompakt opstilling på desktop.
  - `MgpDetailPage.razor`: Tilpasset med opdelt density (Default i main, Compact i sekundære sidekort).

---

## 🪙 Tokenpåvirkning
Nye og opdaterede density-tokens defineret i det globale token-lag:

```css
:root {
  /* Default Density Base Tokens */
  --mgp-density-comfortable: var(--mgp-space-md); /* 16px */
  --mgp-density-default:     var(--mgp-space-sm); /* 12px */
  --mgp-density-compact:     0.375rem var(--mgp-space-sm); /* 6px 12px */

  /* Dynamic Component Spacing Tokens (Defaults til Default Density) */
  --mgp-card-padding: 1rem;
  --mgp-row-padding-y: 0.75rem;
  --mgp-row-padding-x: 1rem;
  --mgp-section-gap: 1.5rem;
}

/* Density Overrides via Scoped Attribute Selector */
[data-density="comfortable"] {
  --mgp-card-padding: 1.5rem;
  --mgp-row-padding-y: 1rem;
  --mgp-row-padding-x: 1.25rem;
  --mgp-section-gap: 2rem;
}

[data-density="default"] {
  --mgp-card-padding: 1rem;
  --mgp-row-padding-y: 0.75rem;
  --mgp-row-padding-x: 1rem;
  --mgp-section-gap: 1.5rem;
}

[data-density="compact"] {
  --mgp-card-padding: 0.75rem;
  --mgp-row-padding-y: 0.375rem;
  --mgp-row-padding-x: 0.75rem;
  --mgp-section-gap: 1rem;
}
```

---

## 💻 CSS & Bootstrap

```css
/* ==========================================================================
   Data Density & Compact Mode Rules
   ========================================================================== */

/* 1. Component Level Density Applications */

/* Cards reacting to Density */
.card,
.entity-card {
  padding: var(--mgp-card-padding);
}

/* Entity Rows reacting to Density */
.compact-entity-row,
.list-group-item {
  padding: var(--mgp-row-padding-y) var(--mgp-row-padding-x);
}

/* Data Tables reacting to Density */
.data-table th,
.data-table td {
  padding: var(--mgp-row-padding-y) var(--mgp-row-padding-x);
}

/* Section Gaps reacting to Density */
.section-stack {
  display: grid;
  gap: var(--mgp-section-gap);
}

/* 2. Metadata & Action Truncation in Compact Mode */
[data-density="compact"] .meta-description,
[data-density="compact"] .badge-secondary-group,
[data-density="compact"] .card-secondary-actions {
  display: none !important;
}

[data-density="compact"] .compact-row-title {
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-bold);
}

[data-density="compact"] .compact-row-meta {
  font-size: var(--font-size-xs);
  color: var(--mgp-text-muted);
}

/* 3. Mobile Accessibility Guard (< 640px) */
/* Compact mode reduces text/spacing, NEVER touch targets */
@media (max-width: 640px) {
  [data-density="compact"] .btn,
  [data-density="compact"] .form-control,
  [data-density="compact"] .icon-button {
    min-height: var(--mgp-touch-target-min, 44px) !important;
    min-width: var(--mgp-touch-target-min, 44px) !important;
  }
}
```