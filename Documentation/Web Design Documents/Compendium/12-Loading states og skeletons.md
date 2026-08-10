### 📄 12-Loading states og skeletons.md

# 12 - Loading states og skeletons

## 📘 Grundregel
- Loading states repræsenterer en midlertidig, uafklaret tilstand og skal gøre ventetid forudsigelig, rolig og lokal uden at stoppe brugerens flow eller forårsage layout-hop (CLS). Skeletons anvendes til kendte strukturer som kort, rækker og dashboards, mens spinners er forbeholdt korte knap-handlinger. Datastatus skal være endeligt afklaret, før der skiftes til indhold, empty, error eller restricted state (jf. `13-Empty_vs_error_vs_no_access.md`).

---

## 🔍 Anvendelse / varianter / typer

| Loading Pattern | Bootstrap & CSS basis | Brugerkontekst & Adfærd |
| :--- | :--- | :--- |
| **Card Skeleton** | `.skeleton-card` | Indlæsning af samlinger (fx haver, bede, materialer). Reserverer plads til overskrift, tekst og handlinger. |
| **Row Skeleton** | `.skeleton-row` | Kompakte lister, tabeller og filoverblik. Bevarer den visuelle række-rytmik. |
| **Media Skeleton** | `.skeleton-media` | Billed- og dokumentkort. Thumbnail loader uafhængigt af metadata. |
| **Inline Loading** | `.inline-loading` | Delsektioner eller paneler med ukendt ventetid. Viser en diskret spinner samt statustekst. |
| **Loading Button** | `.btn` + `.btn-spinner` | Korte brugerhandlinger (fx "Gemmer...", "Uploader..."). Knappen deaktiveres for at forhindre dobbeltklik. |
| **Progress Bar** | `.progress` + `.progress-bar` | Målbar fremdrift (fx upload/download af filer). Må aldrig fakes ved ukendt ventetid. |
| **Stalled / Timeout State** | `.loading-stalled` | Langvarig ventetid. Skifter fra passiv skeleton til forklarende status med retry-handling. |
| **Destructive Loading Button** | `.btn-danger` + `.btn-spinner` | Bekræftede destruktive handlinger i modaler og inline-forms. Knappen deaktiveres, og teksten skifter til aktiv bydeform (fx *"Sletter..."*, *"Arkiverer..."*). |
| **Object Status (Processing)** | `.object-status` + `.status-processing` | Transient status for et specifikt objekt under behandling (fx "Thumbnail oprettes..."). Vises direkte på kortet/rækken uden at blokere resten af visningen. |
| **Search / Filter Loading** | `.inline-loading` / Debounce | Indlæsning under fritekstsøgning eller filterændring. Ved tastning afventes debounce (300ms), hvorefter der vises en diskret inline "Søger..." indikator eller row-skeleton før resultater/empty state renderes. |
| **Table Skeleton** | `.skeleton-table` | Indlæsning af datatabeller. Viser strukturerede rækker med neutrale linjeceller. |
| **Thumbnail Grid Skeleton** | `.skeleton-thumbnail-grid` | Indlæsning af mediegalleri. Viser et grid af firkantede skeleton-blokke. |
| **Staged Loading (Detail Page)** | `MgpDetailPage` + local skeletons | Gradvis trinvvis indlæsning af detail-sider i 5 faser: 1. Header/titel, 2. Metadata, 3. Summary cards, 4. Main content, 5. Relaterede sektioner og tunge data (filer, medlemmer, aktivitet) senest. |

> **Attention Classification:** Processing states (fx *"Thumbnail oprettes..."* eller *"Preview behandles..."*) udgør **Level 1** eller **Level 2** attention. De informerer roligt om, at systemet arbejder, og må **aldrig** benytte advarsels- eller fejlfarver, som kan forveksles med systemfejl.

---

## 🚫 Regler (Do / Don't)
- **Do:** Vis altid en loading state (skeleton/spinner) indtil datastatus er fuldt afklaret, og skift derefter eksplicit til enten indhold, empty state, error state eller restricted state.- **Do:** Konstruér skeletons med samme højde og aspect-ratio (`min-height`, `aspect-ratio: 1 / 1`) som det forventede indhold for at undgå layout shifts.
- **Do:** Afgræns altid indlæsningen så tæt på den specifikke komponent som muligt (lokal indlæsning fremfor global blokering).
- **Do:** Understøt `prefers-reduced-motion: reduce` ved automatisk at deaktivere shimmer-animationer.
- **Do:** Vis altid en kort loading-tilstand (inline "Søger..." eller skeleton) ved søgning og filtrering, hvis resultatet ikke kan leveres øjeblikkeligt, så UI'et ikke blinker eller viser en tom tilstand for tidligt.
- **Do:** På mobil skal loading states altid være lokale og stabile (fx lokal skeleton eller knap-spinner), så UI'et ikke hopper (CLS) eller nulstiller brugerens scroll-position.
- **Do:** Konstruér altid skeletons, så deres højde, padding og aspect-ratio matcher den **aktive view-mode og density** (`comfortable`, `default` eller `compact`). En skeleton i compact-visning skal være tilsvarende tæt (fx 32–36px rækkehøjde) for at forhindre layout-hop (CLS), når data indlæses.
- **Don't:** Brug ikke spinners til hele sider, kortsamlinger eller store lister; anvend skeletons.
- **Don't:** Nulstil ikke brugerens aktuelt valgte filtre, sortering eller scroll-position under genindlæsning.
- **Don't:** Skjul ikke metadata for dokumenter/billeder, blot fordi thumbnail eller preview endnu ikke er genereret.
- **Edge cases:** Ved netværksfejl eller fastlåst indlæsning skal skeleton-tilstanden efter en passende timeout (fx 8-10 sek.) erstatte shimmer med en `MgpStatusMessage` eller `empty-error` med mulighed for manuel genindlæsning ("Prøv igen").

---

## 🧩 Komponentpåvirkning
- **Type:** Pattern / Foundation Component
- **Nye Razor-komponenter:**
  - `MgpSkeletonCard.razor` (Skeleton placeholder til entitetskort).
  - `MgpSkeletonRow.razor` (Skeleton placeholder til kompakte rækker og tabelvisninger).
  - `MgpSkeletonMediaCard.razor` (Skeleton placeholder til fil- og dokumentvisninger).
  - `MgpInlineLoading.razor` (Diskret lokal indikator med spinner og statustekst).
  - `MgpLoadingButton.razor` (Knapkomponent med integreret spinner, disabled state og aktiv handlingstext).
  - `MgpObjectStatus.razor` (del-komponent til kort og lister).
- **Ændrede Razor-komponenter:**
  - `MgpCard.razor` / `MgpMediaCard.razor` (Integrerer uafhængig skeleton-tilstand under indlæsning).
  - `MgpDetailPage.razor`: Styring af staged loading-sekvensen, som indlæser primær kontekst først og udskyder tunge relaterede sektioner med lokale skeletons.

---

## 🪙 Tokenpåvirkning
- `--mgp-surface`: Baggrund for skeleton-containere og rækker.
- `--mgp-surface-muted`: Baseline-farve for grå skeleton-elementer (linjer, ikoner, thumbnails).
- `--mgp-primary-soft`: Shimmer-gradientens lysere højdepunkt under animation.
- `--mgp-border`: Kantfarve for skeleton-containere.
- `--mgp-text-muted`: Tekstfarve til inline loading og statustekster.

---

## 💻 CSS & Bootstrap

```css
/* ==========================================================================
   Skeleton & Loading States
   ========================================================================== */

.skeleton-card,
.skeleton-row,
.skeleton-media {
  pointer-events: none;
  user-select: none;
}

/* Shimmer Animation Baseline */
.skeleton-line,
.skeleton-block,
.skeleton-icon,
.skeleton-pill,
.skeleton-thumbnail {
  background: linear-gradient(
    90deg,
    var(--mgp-surface-muted) 25%,
    var(--mgp-primary-soft) 50%,
    var(--mgp-surface-muted) 75%
  );
  background-size: 200% 100%;
  animation: mgp-skeleton-shimmer 1.4s ease-in-out infinite;
}

@keyframes mgp-skeleton-shimmer {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}

/* Skeleton Elementer */
.skeleton-line {
  height: 0.875rem;
  border-radius: var(--radius-sm, 4px);
}

.skeleton-title { width: 60%; height: 1.125rem; }
.skeleton-meta { width: 40%; }
.skeleton-actions { width: 30%; }

.skeleton-block {
  height: 6rem;
  border-radius: var(--radius-md);
}

.skeleton-icon {
  width: 2.5rem;
  height: 2.5rem;
  border-radius: 50%;
}

.skeleton-pill {
  width: 4rem;
  height: 1.5rem;
  border-radius: 999px;
}

.skeleton-thumbnail {
  width: 100%;
  aspect-ratio: 1 / 1;
  border-radius: var(--radius-md);
}

/* Structure Containers */
.skeleton-card {
  display: grid;
  gap: var(--space-sm);
  padding: var(--space-md);
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-md);
  background: var(--mgp-surface);
  min-height: 11rem;
}

.skeleton-row {
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: var(--space-md);
  align-items: center;
  padding: var(--space-sm) var(--space-md);
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-md);
  background: var(--mgp-surface);
}

.skeleton-row-content {
  display: grid;
  gap: var(--space-xs);
}

/* Inline Loading */
.inline-loading {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  color: var(--mgp-text-muted);
  font-size: var(--font-size-sm);
}

/* Accessibility: Reduced Motion Overstyring */
@media (prefers-reduced-motion: reduce) {
  .skeleton-line,
  .skeleton-block,
  .skeleton-icon,
  .skeleton-pill,
  .skeleton-thumbnail,
  .btn-spinner {
    animation: none !important;
    background: var(--mgp-surface-muted) !important;
  }
}
```