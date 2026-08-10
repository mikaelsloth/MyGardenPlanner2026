### 📄 21-Mobile specific interaction guidelines.md

# 21 - Mobile-specific interaction guidelines

## 📘 Grundregel
- Mobil UI skal designes touch-first og overholde tilgængelighedsbaselinen (Emne 22) som normativt grundlag: Ingen hover-afhængighed, finger-venlige touch targets (min. 44×44 px / `--mgp-touch-target-min`), synlig focus-ring og respekt for reduced motion.

---

## 🔍 Anvendelse / varianter / typer

### Mobile Interaktionsmønstre

| Mønster | Bootstrap & CSS basis | Formål & Adfærd |
| :--- | :--- | :--- |
| **Touch Targets & Spacing** | `.btn`, `.icon-button` | Alle interaktive elementer skal have et klikbart felt på min. 44×44 px. Mindre visuelle ikoner indkapsles i en udvidet trykflade. |
| **Full-width Action Flows** | `.btn-row`, `.dialog-actions` | Knapper i formularer, upload-zoner og confirmations udvides til fuld bredde (`width: 100%`) og stables vertikalt. |
| **Kompakt Sticky Topbar** | `.mobile-header` | Maks. 56 px højde (`--mgp-mobile-header-height`). Indeholder kun titel/tilbage-knap og evt. én overflow-menu (`⋯`) for at bevare arbejdsareal. |
| **Mobile Tabs & Navigation** | `.context-tabs` / Dropdown | Konverteres på mobil til enten horisontalt scrollbare tabs (`white-space: nowrap; overflow-x: auto;`) eller en sektions-dropdown. Bottom navigation udelades i v1. |
| **Filter Drawer / Bottom Sheet** | `MgpFilterDrawer` / `.filter-drawer` | Komplekse filter- og sorteringsværktøjer flyttes fra horizontal bar til en bund-forankret drawer med "Anvend" og "Nulstil" knapper. |
| **Stacked Rows / Mobile Cards** | `.data-table-responsive-stacked` | Tabeller omdannes til vertikale stacked rows med tydelige label-value par for at undgå uønsket horisontal scroll. |
| **Full-screen Media Preview** | `MgpMediaPreview` / `.full-viewer` | Dokument- og billedvisning åbnes altid i full-screen overlay med faste top-handlinger (Luk, Download). |
| **Mobile Detail Page** | `.detail-page` (Mobile mode) | Én sekventiel kolonne: Titel, metadata, primære handlinger, status, indhold og sekundære collapsible sektioner. |
| **Mobile Compact Mode** | `[data-density="compact"]` | Reducerer informationsmængde, tekst og visuelle badges på mobil, men **bevarer altid** minimum touch target-størrelser (min. 44×44 px / `--mgp-touch-target-min`). Tæthed opnås ved færre felter — ikke mindre knapper. |
| **Attention Collapsible Sections** | `MgpCollapsibleSectionCard` | Sekundært indhold startes collapsed på mobil, men tvinges åbent ved aktuel opmærksomhedsstatus (f.eks. afventende invitationer). |

---

## 🚫 Regler (Do / Don't)
- **Do:** Sikr at alle knapper, ikoner og links har et klikbart område på minimum 44×44 px med tydelig spacing imellem tilstødende handlinger.
- **Do:** Placer sektionens vigtigste handlinger direkte ved selve indholdet frem for kun i den øverste globale header.
- **Do:** Anvend fuldbreddeknapper (`width: 100%`) placeret vertikalt i mobilformularer, confirmations og filter-drawers.
- **Do:** Åbn altid dokument- og billed-previews i full-screen modal/overlay på mobile enheder (`<= 640px`).
- **Don't:** Gør aldrig funktioner, værktøjstips, korthandlinger eller informationer afhængige af hover-states.
- **Don't:** Lad ikke sticky elementer (topbar, headers) optage mere end 56 px af skærmhøjden på mobilskærme.
- **Don't:** Placer aldrig toasts således, at de overlapper primære handlingsknapper eller formularhandlinger.
- **Edge cases:** 
  - **Mange tabs på mobil:** Anvend enten `overflow-x: auto` uden linjeskift eller konverter fanebladene til en sekventiel dropdown-select.
  - **Datatunge tabeller:** Hvis en tabel med mange kolonner ikke kan foldes meningsfuldt sammen til stacked rows, anvendes bevidst og styret horisontal scroll på selve tabelcontaineren.

---

## 🧩 Komponentpåvirkning
- **Type:** Pattern / Foundation Component
- **Nye Razor-komponenter:**
  - `MgpFilterDrawer.razor`: Bund-forankret drawer/bottom sheet til mobilfiltrering og sortering.
  - `MgpMobileHeader.razor`: Kompakt sticky mobil-topbar med titel, tilbageknap og valgfri overflow-menu.
- **Ændrede Razor-komponenter:**
  - `MgpMediaPreview.razor`: Tilpasset til automatisk full-screen visning på mobilskærme (`<= 640px`).
  - `MgpDataTable.razor`: Udvidet med automatisk fallback til stacked rows med label-value par på mobil.
  - `MgpContextTabs.razor`: Tilpasset med support for touch-scroll og valgfri dropdown-tilstand.

---

## 🪙 Tokenpåvirkning
Nye mobil-specifikke layout-tokens tilføjet til det globale lag:

```css
:root {
  --mgp-touch-target-min: 44px;
  --mgp-mobile-header-height: 56px;
}
```

Eksisterende genbrugte tokens:
- `--mgp-surface`, `--mgp-surface-muted`, `--mgp-border`, `--mgp-primary-soft`, `--mgp-primary-dark`, `--space-xs`, `--space-sm`, `--space-md`.

---

## 💻 CSS & Bootstrap

```css
/* ==========================================================================
   Mobile-specific Interaction Guidelines Overrides (<= 640px)
   ========================================================================== */

@media (max-width: 640px) {
  /* 1. Touch Targets & Minimum Hit Area */
  .btn,
  .form-control,
  .form-select,
  .context-tab {
    min-height: var(--mgp-touch-target-min, 44px);
  }

  .icon-button {
    width: var(--mgp-touch-target-min, 44px);
    height: var(--mgp-touch-target-min, 44px);
    display: inline-grid;
    place-items: center;
    padding: 0;
  }

  /* 2. Full-Width Buttons in Action Flows */
  .btn-row,
  .card-actions,
  .dialog-actions,
  .form-actions {
    flex-direction: column;
    align-items: stretch;
    width: 100%;
  }

  .btn-row .btn,
  .card-actions .btn,
  .dialog-actions .btn,
  .form-actions .btn {
    width: 100%;
    justify-content: center;
  }

  /* 3. Kompakt Sticky Topbar */
  .mobile-header {
    height: var(--mgp-mobile-header-height);
    padding: 0 var(--space-md);
    display: flex;
    align-items: center;
    justify-content: space-between;
    position: sticky;
    top: 0;
    z-index: 1020;
    background: var(--mgp-surface);
    border-bottom: 1px solid var(--mgp-border);
  }

  /* 4. Filter Drawer / Bottom Sheet */
  .filter-drawer {
    position: fixed;
    inset: auto 0 0 0;
    max-height: 85vh;
    background: var(--mgp-surface);
    border-top-left-radius: var(--radius-lg);
    border-top-right-radius: var(--radius-lg);
    padding: var(--space-md);
    box-shadow: var(--shadow-lg);
    z-index: 1060;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
    gap: var(--space-md);
  }

  /* 5. Mobile Single Column Layout Override */
  .detail-layout-grid,
  .summary-grid,
  .form-grid-2,
  .form-grid-3,
  .form-grid-4 {
    grid-template-columns: 1fr;
  }
}
```