### 📄 20-Detail Page Layout.md

# 20 - Detail Page Layout

## 📘 Grundregel
- En detail page skal give et overskueligt, roligt overblik over ét konkret objekt, dets identitet, status, primære handlinger og relaterede data. Destruktive handlinger må aldrig placeres som primære actions i headeren, den aktive tab skal altid styres i URL-state (`?tab=...`), og sekundære sidekort kan gøres sammenfoldelige (collapsible) uden at skjule kritiske statusser eller fejl.

---

## 🔍 Anvendelse / varianter / typer

### Standard Layoutstruktur for Detail Page
```text
1. Breadcrumb / Context ReturnLink  (← Tilbage til [kontekst])
2. Detail Header                    (Titel, metadata, status badges, 1–3 primary actions)
3. StatusSlot                       (Inline .status-message til feedback/advarsler)
4. Context Tabs                     (?tab=overblik|bede|filer|medlemmer)
5. Summary Cards Grid               (3–5 vigtigste nøgletal/statusser)
6. Main Content Area                (Objektets kerneinformationer og beskrivelser)
7. Related Sections                 (Bede, planter, filer, medlemmer, aktivitet)
8. Danger Zone                      (Collapsible sektion til arkivering og permanent sletning)
```

> **Detail Density Opsætning:** Detail-sider anvender differentieret density: Hovedområdet (*Main Content*) kører altid **Default density** for at sikre optimal læsekomfort. Sekundære sidekolonnekort (*Sidebar / Collapsible Cards*) kan benytte **Compact density** (`[data-density="compact"]`) til visning af relaterede lister, filer og historik for at udnytte den lodrette plads bedst muligt.

---

### Matrix for Collapsible Cards (Default Open/Closed)

| Kort / Sektion | Default Tilstand | Begrundelse / Krav |
| :--- | :--- | :--- |
| **Metadata & Kernedata** | Åben | Giver øjeblikkelig kontekst for objektet. |
| **Summary Cards** | Åben | Hurtige nøgletal/statusser må ikke skjules. |
| **Primære Related Sections** | Åben | Central relateret data (f.eks. Bede på en Havedetalje). |
| **Medlemmer & Invitationer** | Collapsed *(Åben ved attention)* | Skal vise summary i header (*"2 aktive · 1 afventer"*). Åbner automatisk ved afventende handling. |
| **Print & Eksport** | Collapsed | Sekundær handling i sidekolonnen, når den ikke er i headeren. |
| **Danger Zone** | Collapsed | Skal findes med tydelig overskrift, men må ikke dominere arbejdsrummet. |
| **Aktivitetslog / Historik** | Collapsed | Sekundære data. |
| **Status / Error / No Access** | **Altid Åben** | Kritiske advarsler, fejl og manglende rettigheder må **aldrig** foldes sammen. |

- **Attention-styret tilstand:** Sekundære sidekort og sektioner med aktiv opmærksomhed (**Level 2** eller **Level 3**) åbnes automatisk som default.
- **Collapsed Summary:** Når sektionen er sammenfoldet (Level 0/1), skal headeren vise et klart resumé i overskriften via `MgpAttentionSummary` (fx *"Medlemmer · 2 aktive · 1 invitation afventer"*).

---

### Objekt-specifikke Detail Mønstre

| Objekt | Primary Actions (Header) | Primary Context Tabs | Primært Main Content & Related |
| :--- | :--- | :--- | :--- |
| **Have** | `[Redigér]` `[Upload fil]` `[Print]` | Overblik, Bede, Filer, Medlemmer, Indstillinger | Beskrivelse, beliggenhed, relaterede bede, filoversigt. |
| **Bed** | `[Redigér bed]` `[Tilføj plante]` `[Tilføj materiale]` | Overblik, Planter, Materialer, Filer, Noter | Dimensioner, lysforhold, jordtype, planteplan. |
| **Plante** | `[Redigér plante]` `[Tilføj til bed]` | Overblik, Anvendelse, Filer, Noter | Botanisk data, dyrkningsforhold, blomstring, bedanvendelse. |
| **Materiale**| `[Redigér materiale]` `[Købslink]` | Overblik, Køb, Anvendelse, Filer | Mål, farve, leverandør, anvendelse i projekter/bede. |
| **Fil** | `[Åbn preview]` `[Download]` | Overblik, Rettigheder, Historik | Preview/thumbnail, filmetadata, tilknyttet objekt. |

---

## 🚫 Regler (Do / Don't)
- **Do:** Anvende en specifik return-kontekst i breadcrumb (f.eks. `"← Tilbage til Villa Solbakken"` frem for blot `"← Tilbage"`).
- **Do:** Gemme aktiv context tab direkte i URL query string (`/haver/123?tab=filer`), så fanebladet bevares ved genindlæsning, deling og retur fra redigering.
- **Do:** Lade sammenfoldede kort (`MgpCollapsibleSectionCard`) altid vise tilstrækkelig status i overskriften (f.eks. *"Medlemmer · 2 aktive · 1 invitation afventer"*).
- **Do:** Anvende staged loading (indlæs header, metadata og summary først, derefter tungere relaterede sektioner lokalt).
- **Do:** Indbygge tre klare adgangstilstande fra starten: **Owner/Editor** (fuld adgang), **Viewer / Read-only** (læsevisning uden handlinger) og **Restricted** (afvist adgang med forklaring).
- **Do:** DetailPage skal opbygges ud fra tilgængelighedsbaselinen: logisk overskriftshierarki (`h1` -> `h2`), forudsigelig focus order, kontekstuelt return-link, synligt status-slot, samt tastaturtilgængelige context tabs og collapsible sections.
- **Don't:** Placere destruktive eller irreversible handlinger som primære knapper i detail headeren.
- **Don't:** Bruge context tabs på sider med sparsomt indhold blot for at skjule kompleksitet; anvend almindelige sektions-cards i stedet.
- **Don't:** Skjule statusbeskeder, advarsler, valideringsfejl eller adgangsbegrænsninger inde i et sammenfoldet kort.
- **Edge cases:**
  - **Mobil (<= 640px):** Layoutet reetableres i én sekventiel kolonne. Primary actions placeres tæt på det berørte indhold og udvides til fuld bredde. Context tabs omdannes til horisontal scroll (overflow-x: auto) eller dropdown, og media previews åbnes i full-screen visning.
  - **Print:** Navigation, tabs, header-actions og Danger Zone skjules helt. Indholdet præsenteres som et rent, fladt A4-dokument.

---

## 🧩 Komponentpåvirkning
- **Type:** Domain Component / Layout Pattern
- **Nye Razor-komponenter:**
  - `MgpDetailPage.razor`: Overordnet container-komponent der styrer staged loading, grid-layout og status-slot.
  - `MgpDetailHeader.razor`: Header-sektion med titel, undertitel, status-badges og 1–3 primary action-knapper.
  - `MgpCollapsibleSectionCard.razor`: Sammenfoldeligt card til sidekolonne med informativ status i overskriften.
  - `MgpSummaryGrid.razor` / `MgpSummaryCard.razor`: Grid- og kortkomponent til 3–5 nøgletal.
- **Ændrede Razor-komponenter:**
  - `MgpContextTabs.razor`: Tilpasset til at binde aktiv tab til URL query parameter (`?tab=...`).
  - `MgpDangerZone.razor`: Udvidet med support for collapsible tilstand med overskriften *"Arkivering og sletning"*.

---

## 🪙 Tokenpåvirkning
Nye tokens til detail page layout føjet til det globale lag:

```css
:root {
  --mgp-detail-header-gap: var(--bs-spacer-3, 1rem);
  --mgp-summary-card-min-width: 140px;
  --mgp-collapsible-border: var(--mgp-border);
}
```

Eksisterende genbrugte tokens:
- `--mgp-surface`, `--mgp-surface-muted`, `--mgp-border`, `--mgp-primary-soft`, `--mgp-primary-dark`, `--mgp-danger-border`, `--mgp-text-muted`.

---

## 💻 CSS & Bootstrap

```css
/* ==========================================================================
   Detail Page Layout Styles
   ========================================================================== */

/* Detail Container & Grid */
.detail-page {
  display: grid;
  gap: var(--bs-spacer-4, 1.5rem);
}

.detail-layout-grid {
  display: grid;
  grid-template-columns: 1fr 320px;
  gap: var(--bs-spacer-4, 1.5rem);
  align-items: start;
}

/* Detail Header */
.detail-header {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--mgp-border);
}

.detail-header-top {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 1rem;
  flex-wrap: wrap;
}

.detail-header-title-group {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.detail-header-title {
  font-size: 1.75rem;
  font-weight: 700;
  color: var(--mgp-text);
  margin: 0;
}

.detail-header-meta {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--mgp-text-muted);
  font-size: 0.875rem;
  flex-wrap: wrap;
}

.detail-header-actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

/* Inline Status Slot */
.detail-status-slot {
  margin-bottom: 0.5rem;
}

/* Summary Cards Grid */
.summary-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(var(--mgp-summary-card-min-width), 1fr));
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.summary-card {
  background: var(--mgp-surface);
  border: 1px solid var(--mgp-border);
  border-radius: var(--bs-border-radius-lg);
  padding: 0.75rem 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.summary-card-label {
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  color: var(--mgp-text-muted);
}

.summary-card-value {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--mgp-text);
}

/* Collapsible Section Card */
.collapsible-card {
  background: var(--mgp-surface);
  border: 1px solid var(--mgp-collapsible-border);
  border-radius: var(--bs-border-radius-lg);
  overflow: hidden;
}

.collapsible-card-header {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.875rem 1rem;
  background: transparent;
  border: none;
  text-align: left;
  cursor: pointer;
  transition: background-color 0.15s ease-in-out;
}

.collapsible-card-header:hover {
  background-color: var(--mgp-surface-muted);
}

.collapsible-card-title {
  font-size: 0.9375rem;
  font-weight: 600;
  color: var(--mgp-text);
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.collapsible-card-summary {
  font-size: 0.8125rem;
  color: var(--mgp-text-muted);
  font-weight: 400;
}

.collapsible-card-body {
  padding: 1rem;
  border-top: 1px solid var(--mgp-border);
}

/* Responsiv & Mobil (< 940px) */
@media (max-width: 939.98px) {
  .detail-layout-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .detail-header-top {
    flex-direction: column;
    align-items: stretch;
  }

  .detail-header-actions {
    width: 100%;
  }

  .detail-header-actions .btn {
    flex: 1;
    justify-content: center;
  }
}
```