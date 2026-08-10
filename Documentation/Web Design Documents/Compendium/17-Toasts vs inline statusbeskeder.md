### 📄 17-Toasts vs inline statusbeskeder.md

# 17 - Toasts vs. inline statusbeskeder

## 📘 Grundregel
- Inline statusbeskeder (`.status-message`) er standard feedback-mønsteret i MyGardenPlanner og anvendes til kontekstuel, vigtig eller handlingskrævende information. Toasts (`.toast`) bruges udelukkende til korte, ikke-kritiske bekræftelser (f.eks. "Link kopieret") og må **aldrig** bære fejl, valideringsmeddelelser, adgangsbegrænsninger eller destruktive resultater, da forsvindende information nedbryder brugerens tillid og skaber overbliksfejl.

---

## 🔍 Anvendelse / varianter / typer

### Feedback-typer og differentiering

| Type | Formål & Kontekst | Levetid | Placering | Attention Level |
| :--- | :--- | :--- | :--- |  :--- |
| **Inline statusbeskeder** | Vigtig eller kontekstuel status, forklaringer på destruktive handlinger eller adgang. | Persistent (bliver stående til tilstanden ændres) | I den relevante sektion, over skemaet eller ved objektet | **Level 1 / 2 / 3** (afhængigt af scope) |
| **Toast** | Kort, ikke-kritisk bekræftelse af gennemført global handling. | Transient (forsvinder automatisk efter 3-5 sek.) | Nederst til højre på desktop, nederst på mobil | **Level 0 / 1** |
| **Status Banner** | Global information der påvirker hele appen, kontoen eller siden. | Persistent (indtil løst eller lukket) | Øverst på siden under hoved-header | **Level 3** |
| **Object Status** | Transient tilstand for et specifikt objekt (f.eks. "Thumbnail oprettes..."). | Midlertidig / Statusbetinget | Direkte i/på det berørte card eller tabelrække | |
| **Field Message** | Valideringsfejl eller specifik instruktion til et enkelt formularfelt. | Persistent til feltet rettes | Direkte under formularkontrollen | |

---

### Inline status mønstre & Varianter (`.status-message`)

Inline statusbeskeder anvender en fast variant og scope-struktur:

| Variant | Bootstrap & CSS Klasse | Anvendelse |
| :--- | :--- | :--- |
| **Success** | `.status-success` | Bekræftelse på vellykket oprettelse, opdatering eller retur-flow. |
| **Warning** | `.status-warning` | Advarsel om noget der kræver opmærksomhed, men ikke blokerer systemet. |
| **Danger / Error** | `.status-danger` | Systemfejl, uploadfejl, mislykkede transaktioner eller destruktive resultater. |
| **Info** | `.status-info` | Neutral information om systemtilstand eller baggrundsprocesser. |
| **Processing** | `.status-processing` | Systemet arbejder på et objekt (fx billedbehandling eller eksport). |
| **Restricted** | `.status-restricted` | Adgangs- og permission-begrænsning for den konkrete visning. |

#### Scope-placering (`.status-[scope]`)
- **Page-level (`.status-page`):** Placeres øverst i `main content` lige under page header. (f.eks. *"Haven er arkiveret"*).
- **Section-level (`.status-section`):** Placeres øverst i en sektion eller card-grid (f.eks. *"Filer kunne ikke hentes"*).
- **Object-level (`.status-object`):** Placeres i et kort, række eller listeelement (f.eks. *"Thumbnail oprettes..."*).
- **Form-level (`.status-form`):** Placeres øverst i en formular før felterne (f.eks. *"Formularen indeholder valideringsfejl"*).
- **Detail Status Slot (`.detail-status-slot` / `.status-page`):** Dedikeret placering umiddelbart under detail headeren til fremvisning af inline bekræftelser, uploadstatus, arkiveringsnoter eller advarsler.

---

### Toast vs. Inline Matrix

| Handling | Anbefalet Feedback Pattern | Begrundelse |
| :--- | :--- | :--- |
| **Gem formular** | Toast eller Inline success | Toast ved små ændringer; Inline ved større workflows med return-flow. |
| **Opret plante / have** | Inline success + highlight | Brugeren skal forstå hvor objektet blev placeret (jf. `10-Navigation_State_og_Return_Flow.md`). |
| **Upload fil** | Inline success i filsektion | Skal forblive synlig ved filen. Toast kan kun bruges som supplerende sekundær bekræftelse. |
| **Slet fil / Arkivér have** | Inline statusbesked | Destruktive resultater er kritiske og må aldrig auto-dismisses. |
| **Kopiér link til udklipsholder** | Toast | Kort, ikke-kritisk handling uden behov for genlæsning. |
| **Send invitation** | Toast + opdatering af liste | Visuel bekræftelse + listen afspiller invitationens nye status række. |
| **Invitation fejlede** | Inline error | Kræver forklaring og ny handling ("Prøv igen"). |
| **No Access / Permissions** | Inline restricted state | Brugeren må ikke gætte hvorfor indhold mangler. |
| **Fjern plante fra bed** | Inline status med [Fortryd] | Undo-handlinger placeres i kontekst ved listen/objektet. |

---

## 🚫 Regler (Do / Don't)
- **Do:** Anvend inline statusbeskeder (`.status-message`) som standard feedback-valg i MyGardenPlanner.
- **Do:** Placér altid statusbeskeden så tæt på årsagen eller objektet som muligt (page, section, object, form).
- **Do:** Sørg for at toasts indeholder maksimalt én kort sætning (fx *"Ændringer gemt"* eller *"Link kopieret"*).
- **Do:** Understøt altid manuel lukning (`aria-label="Luk"`) og annoncering for skærmlæsere (`aria-live="polite"`).
- **Do:** Vigtige statusbeskeder skal benytte passende programmatiske live-regioner (`aria-live="polite"`). Brug `role="alert"` og `aria-live="assertive"` forsigtigt og kun til kritiske fejl, der kræver øjeblikkelig opmærksomhed.
- **Do:** Kritiske statusbeskeder (`.status-danger`, `.status-warning`, `.status-restricted`) må **ikke** reduceres aggressivt i størrelse eller skjule forklarende tekster, selvom de optræder i en compact context (`[data-density="compact"]`). Beskedens tydelighed og handlingsanvisning vægtes altid højere end visuel kompakthed.
- **Don't:** Vis **aldrig** fejl, adgangsbegrænsninger (`restricted`), validering eller destruktive resultater udelukkende som en toast.
- **Don't:** Vis aldrig filter- eller søgeresultater, nulstilling af filtre eller tomme søgeresultater som toasts. Status på søgning og filtrering hører altid til direkte i listens arbejdsområde som inline statusbeskeder eller dedikerede empty states.
- **Don't:** Brug ikke toasts til information som brugeren skal kunne genlæse for at forstå appens tilstand eller return-flow.
- **Don't:** Lad aldrig toasts dække primære handlingsknapper, formularhandlinger eller den mobile bund-navigation.
- **Don't:** Lad aldrig toasts på mobil placeres således, at de dækker for primære handlingsknapper, formularhandlinger eller sticky elementer, og anvend aldrig toasts som eneste feedback for kritiske fejl eller destruktive tilstande.
- **Edge cases:** Ved mobilskærme (`<= 640px`) stables toasts (`.toast-stack`) i bunden med fuld bredde, placeret med tilstrækkelig margin, så de ikke overlapper bundnavigationen eller sticky handlinger. Ved print skjules `.toast-stack` og `.toast` helt (`display: none !important`), mens inline statusbeskeder printes, hvis de udgør en del af dokumentationsteksten.

---

## 🧩 Komponentpåvirkning
- **Type:** Foundation / Pattern
- **Nye/ændrede Razor-komponenter:**
  - `MgpStatusMessage.razor` (Persistent inline statusbesked med support for varianter (`Success`, `Warning`, `Danger`, `Info`, `Processing`, `Restricted`), scopes (`Page`, `Section`, `Object`, `Form`), og indbyggede handlinger/Undo).
  - `MgpToast.razor` (Transient notifikation til korte bekræftelser med auto-dismiss og manuel luk).
  - `MgpToastContainer.razor` (Fastforankret viewport-stack til håndtering af maks 3 samtidige toasts).
  - `MgpStatusBanner.razor` (Global side- eller kontobred statusbesked).
  - `MgpObjectStatus.razor` (Kompakt statusindikator til rækker, thumbnails og cards).

---

## 🪙 Tokenpåvirkning
- `--mgp-surface`: Baggrund for statusbeskeder og toasts.
- `--mgp-border`: Kantfarve for indramning af beskeder.
- `--mgp-primary`: Status-dot farve ved `status-success`.
- `--mgp-accent`: Status-dot farve ved `status-warning`.
- `--mgp-danger`: Status-dot og border tint ved `status-danger`.
- `--mgp-text-muted`: Statustekst og dæmpede oplysninger.
- `--mgp-shadow-md`: Skygge på transient toast-stack.

---

## 💻 CSS & Bootstrap

```css
/* ==========================================================================
   Inline Status Messages & Toasts Layout
   ========================================================================== */

/* Inline Status Message Baseline */
.status-message {
  display: flex;
  gap: var(--space-sm);
  align-items: flex-start;
  padding: var(--space-sm) var(--space-md);
  border-radius: var(--radius-md);
  border: 1px solid var(--mgp-border);
  background: var(--mgp-surface);
}

.status-message-content {
  display: flex;
  justify-content: space-between;
  gap: var(--space-md);
  align-items: center;
  width: 100%;
}

.status-message-text {
  display: grid;
  gap: 0.125rem;
}

.status-message-text strong {
  color: var(--mgp-text);
  font-size: var(--font-size-base);
}

.status-message-text p {
  margin: 0;
  color: var(--mgp-text-muted);
  font-size: var(--font-size-sm);
}

/* Status Indicator Dots */
.status-dot {
  width: 0.75rem;
  height: 0.75rem;
  border-radius: 50%;
  margin-top: 0.35rem;
  flex: 0 0 auto;
  background: var(--mgp-text-muted);
}

.status-success .status-dot    { background: var(--mgp-primary); }
.status-info .status-dot       { background: var(--mgp-text-muted); }
.status-warning .status-dot    { background: var(--mgp-accent); }
.status-danger .status-dot     { background: var(--mgp-danger); }
.status-processing .status-dot { background: var(--mgp-primary-dark); }
.status-restricted .status-dot { background: var(--mgp-text-muted); }

/* Scope Modifiers */
.status-page { margin-bottom: var(--space-lg); }
.status-section { margin-bottom: var(--space-md); }
.status-form { margin-bottom: var(--space-md); }
.status-object { padding: var(--space-xs) var(--space-sm); font-size: var(--font-size-sm); }

/* Toast Viewport Container & Stack */
.toast-stack {
  position: fixed;
  right: var(--space-lg);
  bottom: var(--space-lg);
  display: grid;
  gap: var(--space-sm);
  z-index: 1080;
  max-width: min(360px, calc(100vw - 2rem));
  pointer-events: none;
}

.toast {
  pointer-events: auto;
  background: var(--mgp-surface);
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-md);
  padding: var(--space-sm) var(--space-md);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-md);
}

.toast-body {
  font-size: var(--font-size-sm);
  color: var(--mgp-text);
  font-weight: var(--font-weight-medium);
}

/* Responsiv & Print Tilpasninger */
@media (max-width: 640px) {
  .status-message-content {
    flex-direction: column;
    align-items: flex-start;
  }

  .toast-stack {
    left: var(--space-md);
    right: var(--space-md);
    bottom: var(--space-md);
    max-width: none;
  }
}

@media print {
  .toast-stack,
  .toast {
    display: none !important;
  }

  .status-message {
    box-shadow: none !important;
    border: 1px solid #cccccc !important;
    background: #ffffff !important;
    break-inside: avoid;
  }
}
```