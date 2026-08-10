### 📄 18-Search filter og sortering.md

# 18 - Search, filter og sortering

## 📘 Grundregel
- Søgning finder (tekstmatch), filtre indsnævrer (egenskaber), sortering prioriterer (rækkefølge), og view-mode ændrer præsentation. Alle fire funktioner samt pagination og scope-afgrænsning skal bindes til URL-state, så arbejdsrum, return-flow og historik bevares uden at hente tunge originalressourcer.

---

## 🔍 Anvendelse / varianter / typer

### Kontrolelementer og Mønstre

| Funktion | Komponent / Klasse | Formål & Adfærd | Eksempel |
| :--- | :--- | :--- | :--- |
| **Søgning** | `MgpSearchInput` / `.search-input` | Fritekstmatch på specifikke feltdata. Kræver debounce (300ms) og clear-knap (`×`). | `[Søg planter...]` |
| **Filter** | `MgpFilterBar` / `.filter-bar` | Indsnævring ud fra strukturerede egenskaber. Primære filtre vises direkte, øvrige under "Flere filtre". | `[Lys: Sol]` `[Type: Staude]` |
| **Active Chips** | `MgpFilterChip` / `.filter-chip` | Tydelig visning af aktive filtre med mulighed for enkeltvis fjernelse og "Nulstil alle". | `[Sol ×]` `[Stauder ×]` `[Nulstil alle]` |
| **Sortering** | `MgpSortSelect` / `.sort-select` | Ændrer rækkefølgen af resultater. Skal angive både sorteringsfelt og retning. | `[Sortér: Navn A–Å]` |
| **View-mode** | `MgpViewModeToggle` / `.view-mode-toggle` | Skifter præsentationsform uden at ændre datagrundlaget. Understøtter op til 4 tilstande: `cards` (Cards), `compact` (Compact rows), `table` (DataTable) og `grid` (Thumbnail grid). | `[Cards] [Liste] [Tabel] [Grid]` |
| **Scope** | `MgpScopeFilter` / `.scope-filter` | Bestemmer dataindholdets tilstand (Aktive / Arkiverede / Alle). | `[Status: Aktive]` |
| **Mobile Drawer**| `MgpFilterDrawer` / `.filter-drawer` | Bottom sheet / interaction drawer på mobil (`<= 640px`) til komplekst filtervalg. | `[Filtrer (2)]` `[Sortér]` |

---

## 🚫 Regler (Do / Don't)
- **Do:** Gem altid search, filter, sort, view-mode og pagination i URL query string (fx `/planter?search=lavendel&lys=sol&sort=name&view=compact&page=1`), så arbejdsrummet bevares i browserhistorik og return-flows.
- **Do:** Vis altid aktive filtre som synlige chips med enkeltvis fjernelse (`×`) samt en samlet *"Nulstil alle"*-handling.
- **Do:** Nulstil altid pagination til side 1, så snart brugeren ændrer et filter, en sortering eller en søgeterm.
- **Do:** Vis den indtastede søgeterm direkte i empty state ved søgning uden match (fx *"Ingen resultater for 'lavenddel'"*).
- **Do:** Benyt en mobil interaction drawer (`MgpFilterDrawer`) med "Anvend filtre" og "Nulstil" på små skærme (`<= 640px`).
- **Do:** På mobilskærme (<= 640px) skal den horisontale filterbar skifte fra desktop inline toolbar (.filter-bar) til en kompakt mobil-handling (.mobile-filter-actions), der åbner filtre i en skærmtilpasset drawer / bottom sheet (MgpFilterDrawer).
- **Do:** Sørg for at filtermuligheder og tællinger respekterer brugerens permissions, så der ikke tilbydes filtre, der altid giver 0 resultater eller adgangsafslag.
- **Do:** Alle søge- og filterkontroller (`SearchInput`, `FilterChip`, `FilterDrawer`, `SortSelect`) skal kunne betjenes fuldstændigt via tastatur med synlig focus state og korrekt ARIA-fejl/statusannoncering.
- **Do:** Søge- og filterresultater med mange elementer bør som udgangspunkt benytte compact rows eller compact density (`[data-density="compact"]`) for hurtig scanning. Fritekstsøgning eller filterændring må dog **aldrig** ændre brugerens eksplicit valgte view-mode (`cards`, `table`, `grid`) uventet.
- **Don't:** Placer ikke `MgpViewModeToggle` på sider, hvor kun én visning giver mening (fx enkle dashboards). Knapgruppen til visningsskift må kun benyttes, hvor skift mellem f.eks. cards, compact rows, tabel eller thumbnail grid giver en reel, opgavespecifik værdi for brugeren.
- **Don't:** Bland aldrig søgning, filtrering og sortering sammen i samme UI-kontrol eller rullemenu.
- **Don't:** Vis aldrig en first-use empty state, når resultatet er tomt pga. aktive filtre eller søgning – anvend altid `FilteredEmptyState` eller `SearchEmptyState`.
- **Don't:** Lad aldrig filtrering eller fritekstsøgning udløse indlæsning af tunge originalressourcer (fx PDF-filer eller højtopløselige billeder); hent kun metadata og lette thumbnails.
- **Edge cases:** Hvis et nyligt oprettet eller redigeret element i et return-flow skjules af et aktivt filter, må UI'et ikke vise en empty state. Vis i stedet en inline statusbesked (`MgpStatusMessage`) øverst på listen med forklaring og direkte mulighed for at nulstille filteret.

---

## 🧩 Komponentpåvirkning
- **Type:** Pattern / Foundation Component
- **Nye Razor-komponenter:**
  - `MgpSearchInput.razor`: Søgefelt med debounce, integreret loader, clear-handling og `aria-label`.
  - `MgpFilterBar.razor`: Layout-container for søgning, filterknapper, sortering og view-mode skift.
  - `MgpFilterChip.razor`: Interaktiv chip med lukkeknap til aktive filterværdier.
  - `MgpActiveFilterSummary.razor`: Bånd der opsamler aktive chips og tilbyder "Nulstil alle".
  - `MgpSortSelect.razor`: Dropdown-komponent til valg af sorteringsfelt og retning.
  - `MgpViewModeToggle.razor`: Knapgruppe til visningsskift (Cards, Liste, Tabel, Thumbnail Grid).
  - `MgpFilterDrawer.razor`: Mobil/tablet off-canvas interaction drawer til udvidede filtre.

---

## 🪙 Tokenpåvirkning
Genbruger eksisterende globale tokens til konsistent visuel forankring:
- `--mgp-surface`: Baggrund for search-input, filter-bar og drawers.
- `--mgp-surface-muted`: Baggrund for filter-chips, inactive view-mode knapper og dropdowns.
- `--mgp-border`: Kantfarve for felter, filter-bars og chips.
- `--mgp-primary-soft`: Hover- og aktiv-baggrund for valgte view-modes og aktive filter-chips.
- `--mgp-primary-dark`: Tekstfarve på aktive filter-elementer.
- `--mgp-text-muted`: Farve på placeholders, ikoner og inaktive valg.
- `--mgp-focus-ring`: Fokusmarkering ved tastaturnavigation på filtre og chips.

---

## 💻 CSS & Bootstrap

```css
/* ==========================================================================
   FilterBar, SearchInput & Active Filter Chips
   ========================================================================== */

/* FilterBar Container */
.filter-bar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-sm) var(--space-md);
  background: var(--mgp-surface);
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-md);
  margin-bottom: var(--space-md);
}

.filter-bar-main {
  display: flex;
  flex: 1 1 auto;
  align-items: center;
  gap: var(--space-sm);
  flex-wrap: wrap;
}

.filter-bar-actions {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
  margin-left: auto;
}

/* SearchInput med clear-knap */
.search-input-group {
  position: relative;
  min-width: 240px;
  flex: 1 1 auto;
}

.search-input-group .form-control {
  padding-right: 2.25rem;
}

.search-clear-btn {
  position: absolute;
  right: 0.5rem;
  top: 50%;
  transform: translateY(-50%);
  background: transparent;
  border: 0;
  color: var(--mgp-text-muted);
  padding: 0.25rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.search-clear-btn:hover {
  color: var(--mgp-text);
}

/* Active Filter Summary & Chips */
.active-filter-summary {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
  flex-wrap: wrap;
  margin-bottom: var(--space-md);
}

.filter-chip {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0.2rem 0.6rem;
  background: var(--mgp-primary-soft);
  color: var(--mgp-primary-dark);
  border: 1px solid rgba(63, 107, 74, 0.2);
  border-radius: 999px;
  font-size: var(--font-size-xs);
  font-weight: 600;
}

.filter-chip-remove {
  background: transparent;
  border: 0;
  color: currentColor;
  padding: 0;
  font-size: 0.9rem;
  line-height: 1;
  cursor: pointer;
  opacity: 0.7;
}

.filter-chip-remove:hover {
  opacity: 1;
}

/* ViewModeToggle Knapgruppe */
.view-mode-toggle {
  display: inline-flex;
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-md);
  overflow: hidden;
  background: var(--mgp-surface-muted);
}

.view-mode-btn {
  border: 0;
  background: transparent;
  padding: 0.375rem 0.65rem;
  color: var(--mgp-text-muted);
  cursor: pointer;
}

.view-mode-btn.active {
  background: var(--mgp-surface);
  color: var(--mgp-primary-dark);
  box-shadow: 0 1px 2px rgba(0,0,0,0.05);
}

/* Mobil Filter Drawer (< 640px) */
@media (max-width: 640px) {
  .filter-bar-main {
    flex-direction: column;
    align-items: stretch;
  }

  .search-input-group {
    width: 100%;
  }

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
  }
}
```