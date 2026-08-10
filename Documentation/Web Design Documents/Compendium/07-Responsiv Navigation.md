### 📄 07-Responsiv Navigation.md

# 07 - Responsiv Navigation

## 📘 Grundregel
- Navigationen opdeles i to niveauer: **Global App-navigation** (hvor er jeg?) og **Lokal Kontekstnavigation** (hvad arbejder jeg med?). Global navigation vises som permanent sidebar på desktop (med bevidst valgfri collapsed nav rail) og som topbar med off-canvas drawer på tablet/mobil. Lokal kontekstnavigation tilgås via horisontalt scrollbare context tabs på entitets-detaljesider.

---

## 🔍 Anvendelse / varianter / typer

### Navigationsmønstre
| Skærmtype | Global Navigation | Lokal Navigation |
| :--- | :--- | :--- |
| **Desktop (≥940px)** | Permanent venstre sidebar (280px) eller valgfri Nav Rail (72px) | Context Tabs øverst på detaljesider |
| **Tablet / Mobil (<940px)** | Sticky Topbar + Off-Canvas Drawer (340px max) | Horisontalt scrollbare Context Tabs |
| **Detail Page (<640px)** | Sticky Topbar + Off-Canvas Drawer (340px max) | Horisontalt scrollbare Context Tabs (`white-space: nowrap; overflow-x: auto;`) eller kontekstuel dropdown-menu |

> **Kontekstnote (Navigation State & Persistence):**
> Context Tabs (lokal kontekstnavigation) skal altid bindes til URL'ens query string (f.eks. `?tab=bede`), så brugeren ved return-flow eller sideload tilgår den korrekte kontekst. Layoutpræferencer som collapsed sidebar/nav rail gemmes derimod i `localStorage` som brugerpræference og udelades fra URL'en (jf. `10_Navigation_State_og_Return_Flow.md`).

> **Kontekstnote (FilterDrawer vs. Global Drawer):**
> `MgpFilterDrawer` (mobil filtervisning) er en lokal *interaction drawer* for den konkrete liste/side og **ikke** en del af den globale app-navigation (`NavDrawer`). Den skal dog overholde samme tilgængeligheds- (WCAG focus trap, ESC-lukning, `aria-expanded`) og responsive principper som den globale drawer.

### Navigationslink States
| State | Beskrivelse / Visualisering |
| :--- | :--- |
| **Default** | Muted tekst, gennemsigtig baggrund |
| **Hover** | Mild baggrunds-tint (`--mgp-primary-soft`), tydelig cursor |
| **Active** | Fremhævet baggrund (`--mgp-primary-soft`), primær mørk tekst (`--mgp-primary-dark`), kontur/border-markering |
| **Focus** | Tastaturnavigation med Bootstrap focus ring (`box-shadow`) |
| **Attention / Badge** | Indikeres via `.nav-attention-count` (fx uafsluttede invitationer eller udløbende filer). Navigationen må visuelt indikere opmærksomhedsbehov (Level 2/3), men må ikke overfyldes med røde prikker eller føles som en alarmcentral. |

---

## 🚫 Regler (Do / Don't)

- **Do:** Skelne skarpt mellem global app-structure (sidebar/drawer) og lokale side-funktionaliteter (context tabs).
- **Do:** Sikre fuld tilgængelighed (WCAG 2.1 AA) i mobil drawer: korrekt brug af `aria-expanded`, `aria-controls`, fangede fokusfælder (focus trap) samt lukning ved ESQ-tast.
- **Do:** Bevare uafhængig intern scroll (`overflow-y: auto`) i desktop sidebar ved mange menupunkter.
- **Do:** Navigationen skal være permission-aware uden at skabe blindgyder. Navigationspunkter til områder, som brugeren aldrig har adgang til, skjules helt. Hvis brugeren har delvis læseadgang, skal nav-punktet føre til en side med en tydelig, afklaret læsevisning eller restricted state.
- **Do:** Anvend en bottom sheet / interaction drawer (`MgpFilterDrawer`) til komplekst filtervalg på mobilskærme (`<= 640px`) fremfor at presse filterbaren sammen horisontalt.
- **Do:** Do: På mobilskærme (<640px) skal lokal kontekstnavigation (ContextTabs) på detail pages håndteres enten som en horisontalt scrollbar fanebladsrække (white-space: nowrap; overflow-x: auto;) eller som en ren kontekstuel dropdown-menu for at undgå layout-shift og uhensigtsmæssige linjeskift.
- **Do:** Mobil topbar (.mobile-header) skal holdes kompakt (max 56px / --mgp-mobile-header-height) og begrænses til titel/tilbagevej samt én valgfri overflow-menu (⋯), så arbejdsfladen ikke indskrænkes. Bottom navigation udelades i v1 til fordel for topbar og off-canvas drawer.
- **Do:** Navigation drawer (off-canvas) skal implementere streng fokusstyring: Fokus flyttes ind i drawer ved åbning, fanges inde i menuen (focus trap), kan lukkes med Escape-tasten, og fokus returneres automatisk til udløserknappen ved lukning.
- **Don't:** Tvinge automatisk collapsed sidebar igennem baseret på sidetype; collapsibility skal være et eksplicit brugervalg og understøttes af tydelige tooltips.
- **Don't:** Overforbruge badges i navigationen. Brug dem kun, når en handling direkte kræver brugerens opmærksomhed.
- **Edge cases:** 
  - **Mange context tabs på mobil:** Anvend `white-space: nowrap` og `overflow-x: auto` uden at brække linjen eller skabe layout-shift.
  - **Lange formularer på mobil:** Mobil-topbaren forbliver sticky, men skal holdes lav (`56px`), så den ikke reducerer arbejdsområdet for tastaturet.

---

## 🧩 Komponentpåvirkning

- **Type:** Foundation / Pattern Component
- **Nye/ændrede Razor-komponenter:**
  - `AppShell.razor`: Layout-wrapper der styrer grid/flex for sidebar, topbar og main content area.
  - `NavMenu.razor`: Genanvendelig navigationsliste (deles mellem desktop sidebar og mobile drawer).
  - `NavDrawer.razor`: Off-canvas wrapper-komponent til mobil/tablet med backdrop.
  - `ContextTabs.razor`: Komponent til lokal faneblads-navigation inde på entity-detaljesider.
  - `NavBadge.razor`: Lille status- og opmærksomhedsindikator til nav-links.

---

## 🪙 Tokenpåvirkning

Nye layout-tokens der føjes til det globale lag:

```css
:root {
  --mgp-sidebar-width: 280px;
  --mgp-sidebar-collapsed-width: 72px;
  --mgp-mobile-header-height: 56px;
  --mgp-nav-item-radius: var(--bs-border-radius);
}
```

Eksisterende genbrugte tokens:
- `--mgp-surface`, `--mgp-border`, `--mgp-primary-soft`, `--mgp-primary-dark`, `--mgp-text-muted`, `--mgp-warning-bg`.

---

## 💻 CSS & Bootstrap

```css
/* ==========================================================================
   App Layout & Responsive Sidebar / Drawer
   ========================================================================== */

/* Layout Container */
.app-shell {
  display: grid;
  grid-template-columns: var(--mgp-sidebar-width) 1fr;
  min-height: 100vh;
}

.app-shell.sidebar-collapsed {
  grid-template-columns: var(--mgp-sidebar-collapsed-width) 1fr;
}

/* Desktop Sidebar */
.desktop-sidebar {
  background: var(--mgp-surface);
  border-right: 1px solid var(--mgp-border);
  height: 100vh;
  position: sticky;
  top: 0;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
}

/* Collapsed Desktop State (Nav Rail) */
.sidebar-collapsed .nav-label,
.sidebar-collapsed .nav-section-title,
.sidebar-collapsed .brand-text {
  display: none;
}

.sidebar-collapsed .nav-link {
  justify-content: center;
  padding: var(--bs-spacer-2);
}

/* Nav Link Item */
.nav-link-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.5rem 0.75rem;
  color: var(--mgp-text-muted);
  border-radius: var(--mgp-nav-item-radius);
  text-decoration: none;
  font-weight: 500;
  transition: background-color 0.15s ease-in-out, color 0.15s ease-in-out;
}

.nav-link-item:hover {
  background-color: var(--mgp-primary-soft);
  color: var(--mgp-primary-dark);
}

.nav-link-item.active {
  background-color: var(--mgp-primary-soft);
  color: var(--mgp-primary-dark);
  font-weight: 600;
}

/* Attention Badge */
.nav-badge {
  margin-left: auto;
  min-width: 1.25rem;
  height: 1.25rem;
  padding: 0 0.35rem;
  border-radius: 999px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background-color: var(--mgp-warning-bg);
  color: #653521;
  font-size: 0.75rem;
  font-weight: 700;
}

/* Context Tabs (Lokal Navigation) */
.context-tabs {
  display: flex;
  gap: 0.5rem;
  overflow-x: auto;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid var(--mgp-border);
  -webkit-overflow-scrolling: touch;
}

.context-tab {
  border: 1px solid transparent;
  background: transparent;
  color: var(--mgp-text-muted);
  border-radius: 999px;
  padding: 0.375rem 0.875rem;
  font-weight: 600;
  white-space: nowrap;
  text-decoration: none;
}

.context-tab.active {
  background-color: var(--mgp-primary-soft);
  color: var(--mgp-primary-dark);
  border-color: rgba(63, 107, 74, 0.18);
}

/* Mobile Header & Off-Canvas Mobile Navigation (< 940px) */
.mobile-header {
  display: none;
}

@media (max-width: 939.98px) {
  .app-shell {
    grid-template-columns: 1fr;
  }

  .desktop-sidebar {
    display: none;
  }

  .mobile-header {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    height: var(--mgp-mobile-header-height);
    padding: 0 1rem;
    background: rgba(250, 248, 242, 0.92);
    backdrop-filter: blur(8px);
    border-bottom: 1px solid var(--mgp-border);
    position: sticky;
    top: 0;
    z-index: 1020;
  }

  .nav-drawer {
    position: fixed;
    inset: 0 auto 0 0;
    width: min(86vw, 340px);
    background: var(--mgp-surface);
    border-right: 1px solid var(--mgp-border);
    padding: 1.25rem;
    z-index: 1050;
    box-shadow: var(--bs-box-shadow-lg);
    transform: translateX(-100%);
    transition: transform 0.25s ease-in-out;
  }

  .nav-drawer.open {
    transform: translateX(0);
  }

  .drawer-backdrop {
    position: fixed;
    inset: 0;
    background: rgba(36, 49, 40, 0.4);
    z-index: 1040;
  }
}
```