### 📄 24-Notifications og attention management.md

# 24 - Notifications og attention management

## 📘 Grundregel
- Opmærksomhed i MyGardenPlanner skal være sparsom, forklarende og handlingsorienteret. Ikke alle tilstande udgør en notification; almindelig status vises som muted badges eller metadata, mens reelle opmærksomhedsbehov styres af fire faste Attention Levels (0–3), der placeres tæt på årsagen. Farverne orange og rød reserveres til reel handling.

---

## 🔍 Anvendelse / varianter / typer

### 1. Classification & Attention Matrix

| Attention Level | Urgency & Scope | UI Patterns | Eksempler |
| :--- | :--- | :--- | :--- |
| **Level 0 (Neutral)** | Almindelig information uden akut handling | Muted badge, metadata, transient toast ved direkte brugerinitiering | `Aktiv`, `Permanent`, `Preview klar`, "Link kopieret" |
| **Level 1 (Lav)** | Relevant tilstand, men kræver ikke handling nu | Muted/accent badge, object-level inline status, processing dot | `Invitation sendt`, `Thumbnail oprettes`, `Fil er midlertidig` |
| **Level 2 (Middel)** | Bør ses af brugeren, evt. opfølgning | Accent badge, section inline status, åben collapsible section default, summary count | `Invitation afventer svar`, `Fil udløber snart`, `Mangler oplysninger` |
| **Level 3 (Høj)** | Kræver handling eller blokerer workflow | Persistent inline status/alert, global/page banner, direkte action-knap | `Upload fejlede`, `Du har ikke adgang`, `Betaling/Abonnement kræver handling` |

---

### 2. Attention UI Pattern Differentiation

| Pattern | Formål & Kontekst | Levetid | Placering |
| :--- | :--- | :--- | :--- |
| **Badge / AttentionBadge** | Viser objektets tilstand i tabeller og cards | Persistent til tilstand ændres | På/i det berørte kort eller tabelrække |
| **Inline status** | Forklarer tilstand i kontekst ved berørt objekt/sektion | Persistent | Objekt, sektion, formular eller side |
| **Toast** | Transient bekræftelse af gennemført handling | Transient (3-5 sek) | Bunden af skærmen (viewport) |
| **Alert / Banner** | Vigtig eller blokerende oplysning på side/kontoniveau | Persistent | Øverst i main content eller under global header |
| **AttentionSummary** | Aggregeret tælling og resumé i summary cards og section headers | Persistent | Sektions-headers og summary grids |

---

## 🚫 Regler (Do / Don't)
- **Do:** Placér altid attention-budskabet så tæt på årsagen som muligt (object-level → section-level → page-level → global banner).
- **Do:** Sørg for at attention stater med handling tilbyder den næste naturlige handling direkte i teksten eller komponenten (f.eks. `[Gør permanent]` eller `[Vælg anden fil]`).
- **Do:** Lad attention-behov påvirke collapsible sections, så sektioner med Level 2/3 attention åbnes automatisk som default, og sammenfoldede headers viser resumé (f.eks. *"Medlemmer · 2 aktive · 1 invitation afventer"*).
- **Do:** Saml gentagne eller beslægtede opmærksomhedstilstande i et resumé (f.eks. *"3 filer kræver handling"*) frem for at generere mange separate alarmer.
- **Don't:** Indfør ikke et globalt notification center i v1; brug i stedet direkte kontekstuel placering, badges og inline statusbeskeder.
- **Don't:** Overforbrug ikke rød og orange farve. Hvis alt har stærke opmærksomhedsfarver, mister farven sin signalværdi.
- **Don't:** Vis ikke toasts for autosave, valideringsfejl eller almindelige baggrundsprocesser. Toasts reserveres til eksplicitte transient bekræftelser.
- **Edge cases:** Ved sammensatte sider med multiple opmærksomhedskrav vises kun den mest kritiske tilstand (Level 3) i sidens hoved-banner, mens lavere niveauer (Level 1–2) fastholdes på sektions- eller objektniveau.

---

## 🧩 Komponentpåvirkning
- **Type:** Foundation / Cross-cutting Pattern Layer
- **Nye Razor-komponenter:**
  - `MgpAttentionBadge.razor` (Badge med opmærksomheds-level og status-dot).
  - `MgpAttentionSummary.razor` (Aggregeret tæller og samlingsnote til headers og summary cards).
  - `MgpGlobalBanner.razor` (Bred bannerkomponent til kontoniveau og Level 3 globale alarmer).
- **Ændrede Razor-komponenter:**
  - `MgpStatusMessage.razor` (Eksplicit understøttelse af Attention Level 1–3 mappings).
  - `MgpCollapsibleSectionCard.razor` (Automatisk default-open logik ved aktiv attention state og summary i header).
  - `MgpSummaryCard.razor` (Support for attention counts og `.has-attention`).

---

## 🪙 Tokenpåvirkning
- `--mgp-attention-level-0`: Muted text og border (`--mgp-text-muted`).
- `--mgp-attention-level-1`: Dæmpet accent-tone (`--mgp-primary-soft`).
- `--mgp-attention-level-2`: Advarsel/accent-toner (`--mgp-accent`).
- `--mgp-attention-level-3`: Danger/kritisk-toner (`--mgp-danger`).

---

## 💻 CSS & Bootstrap

```css
/* ==========================================================================
   Attention Management & Utility Classes
   ========================================================================== */

/* Attention Accent Borders */
.has-attention-level-1 { border-left: 3px solid var(--mgp-primary-soft); }
.has-attention-level-2 { border-left: 4px solid var(--mgp-accent); }
.has-attention-level-3 { border-left: 4px solid var(--mgp-danger); }

/* Attention Badge Styling */
.attention-badge {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  padding: 0.2em 0.6em;
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-bold);
  border-radius: 999px;
  background: var(--mgp-surface-muted);
  color: var(--mgp-text);
}

.attention-badge-level-2 {
  background: var(--mgp-accent-soft, #FFF3E0);
  color: var(--mgp-accent-dark, #E65100);
}

.attention-badge-level-3 {
  background: var(--mgp-danger-bg);
  color: var(--mgp-danger);
}

/* Attention Summary Header Note */
.attention-summary-note {
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-medium);
  color: var(--mgp-accent-dark, #E65100);
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
}
```