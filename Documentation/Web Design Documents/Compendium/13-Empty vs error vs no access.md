### 📄 13-Empty vs error vs no access.md

# 13 - Empty vs. error vs. no access

## 📘 Grundregel
- UI’et skal med tydelige visuelle og sproglige signaler adskille, om der mangler data (empty), om data er skjult af søgning/filtre (filtered/search), om indlæsningen fejlede (error), eller om adgang mangler (no access). Loading skal altid afklares før nogen af disse tilstande vises, og den primære handling skal altid svare direkte på årsagen til den aktuelle tilstand.

---

## 🔍 Anvendelse / varianter / typer

### Tilstandsoversigt & Visuel Differentiation

| State | Betydning | Visuel tone & Farve | Ikon-type | Primær handling (State-action) |
| :--- | :--- | :--- | :--- | :--- |
| **Empty** | Ingen data oprettet endnu | Venlig / Soft (`--mgp-primary-soft`) | Botanisk / Konstruktiv (✿, 🌿, ⇧) | Opret / Upload / Invitér |
| **Filtered empty** | Data findes, men udelukkes af filtre | Neutral / Muted (`--mgp-surface-muted`) | Søgning / Filter (⌕, ▧) | Nulstil filtre |
| **Search empty** | Fri tekstsøgning gav ingen match | Neutral / Muted (`--mgp-surface-muted`) | Søgeglas (⌕) | Ryd søgning |
| **Error** | Teknisk fejl, API- eller netværksfejl | Tydelig Advarsel (`--mgp-danger-bg`) | Udråbstegn (!) | Prøv igen (Retry) |
| **No access** | Brugerens rolle giver ikke adgang | Rolig / Respektfuld (`--mgp-surface-muted`)| Lås (🔒) | Kontakt ejer / Anmod om adgang |
| **Processing** | Data uploadet, men preview/thumb oprettes | Information (`--mgp-surface-muted`) | Process / Clock (⚙) | Vis metadata / Download original |
| **Loading** | Uafklaret tilstand under hentning | Shimmer / Skeleton | Ingen / Skeleton | Ingen (eller Annullér) |

> - **Bemærkning om adgangshandlinger:** Handlingerne *"Fjern adgang"* og *"Tilbagekald invitation"* håndterer brugerrettigheder og tokens – de må ikke sprogligt eller visuelt sidestilles med almindelig datasletning. De skal ledsages af forklarende konsekvenstekst for både personen og dataene.

---

### Beslutningsflow (State Evaluation Order)

Ved indlæsning af enhver datakilde skal systemet evaluere tilstanden i følgende faste rækkefølge:

```text
1. Loader data stadig?
   └─► Ja: Vis Loading / Skeleton
   └─► Nej: Fortsæt

2. Fejlede datahentningen (API/Netværk/Parsing)?
   └─► Ja: Vis Error state (.empty-error)
   └─► Nej: Fortsæt

3. Mangler brugeren rettigheder/adgang?
   └─► Ja: Vis No access / Restricted state (.empty-restricted)
   └─► Nej: Fortsæt

4. Findes der data, men udelukket af aktive filtre/søgning/arkivstatus?
   └─► Ja: Vis Filtered/Search empty (.empty-filtered / .empty-search) med eksplicit visning af søgeterm og direkte mulighed for 'Nulstil filtre' eller 'Ryd søgning' (fx "Ingen resultater for 'lavenddel'. [Ryd søgning]").
   └─► Nej: Fortsæt

5. Findes der ingen data overhovedet?
   └─► Ja: Vis Empty / First-use / Context empty (.empty-first-use / .empty-context)
   └─► Nej: Vis Data / Indhold

6. (Special state) Er data modtaget, men under behandling?
   └─► Ja: Vis Processing state (.empty-processing)
```

---

### Handlingers synlighed: Skjul vs. Disable

| Situtation | Rigtige handling | Begrundelse |
| :--- | :--- | :--- |
| **Rollen kan aldrig udføre handlingen** | **Skjul handlingen** | Undgår visuel støj for uautoriserede roller (fx ser en viewer ikke "Slet have"). |
| **Handlingen er forventet, men midlertidigt låst** | **Disable handlingen** | Fastholder kontekst og forklarer årsag via badge eller tooltip (fx "Download ikke tilgængelig for din rolle"). |

---

## 🚫 Regler (Do / Don't)
- **Do:** Anvend altid *State-action-reglen*: Handlingsknappens primære funktion skal adressere årsagen til tilstanden.
- **Do:** Sikr at teksten altid bærer den primære betydning, mens farver og ikoner blot understøtter visuelt (WCAG a11y).
- **Do:** Vis den nøjagtige søgeterm i teksten ved Search empty (fx *"Ingen resultater for 'lavenddel'"*).
- **Do:** Behandl en tom visning pga. arkiverede data som en *Filtered empty state* med en direkte handling til at åbne arkivvisningen – det må aldrig forveksles med en systemfejl, manglende adgang eller tom first-use state.
- **Do:** Skelne skarpt mellem søgning (fritekst) og filtrering (egenskaber) i tomme tilstande, så brugeren guides præcist til enten at rydde søgefeltet eller nulstille det specifikke filter.
- **Don't:** Skjul aldrig en fejl bag en empty state. Visning af "Ingen filer" ved en API-fejl nedbryder brugerens tillid.
- **Don't:** Vis aldrig empty, error eller no access før loading-tilstanden er endeligt afklaret.
- **Don't:** Brug ikke teknisk eller aggressivt sprog som "Fejl 403", "Access Denied", "Tom database" eller "Du må ikke...".
- **Don't:** Vis **aldrig** systemfejl (`Error`), netværksnedbrud eller adgangsbegrænsninger (`No access`) udelukkende som en toast. Da toasts forsvinder automatisk, risikerer brugeren at miste forklaringen på den manglende data. Anvend i stedet altid en persistent inline `MgpStatusMessage` eller en dedikeret restricted/error empty state.
- **Edge cases:** Ved sikkerhedsfølsomme entiteter, hvor systemet ikke må afsløre, om et objekt eksisterer, skjules objektet helt fra oversigten, eller der anvendes en generisk og rolig formulering: *"Ingen data tilgængelige med den aktuelle adgang"*. Der må aldrig vises låste kort eller specifikke titler på sikkerhedsfølsomme entiteter.

---

## 🧩 Komponentpåvirkning
- **Type:** Pattern Component / Architectural Rule
- **Nye Razor-komponenter:**
  - Ingen (Anvender og konsoliderer eksisterende `MgpEmptyState.razor`).
- **Ændrede Razor-komponenter:**
  - `MgpEmptyState.razor` (Understøtter parametrene `Variant="Empty|Filtered|Search|Error|Restricted|Processing"` og eksplicitte knap-slots for state-specific actions).
  - `MgpMediaCard.razor` (Tilpasset til at vise restricted og processing states for enkelte elementer).
  - `MgpStatusMessage.razor` (Anvendes til transient feedback, når et nyoprettet/ændret element skjules af aktive filtre).

---

## 🪙 Tokenpåvirkning
- `--mgp-surface`: Baggrund for tilstandsoversigter.
- `--mgp-surface-muted`: Ikon-baggrund ved filtered, search og restricted states.
- `--mgp-primary-soft`: Ikon-baggrund ved normale empty states.
- `--mgp-danger-bg`: Soft rød baggrund for ikon-cirkel ved `.empty-error` (`#FFF4F3`).
- `--mgp-danger`: Tekst- og ikonfarve ved fejltilstande (`#9F3A38`).
- `--mgp-danger-border`: Soft rød kantfarve ved fejltilstande (`rgba(159, 58, 56, 0.35)`).

---

## 💻 CSS & Bootstrap

```css
/* ==========================================================================
   State differentiation modifiers for .empty-state
   ========================================================================== */

/* State: Empty (First-use / Context) */
.empty-state.empty-first-use .empty-icon,
.empty-state.empty-context .empty-icon {
  background: var(--mgp-primary-soft);
  color: var(--mgp-primary-dark);
}

/* State: Filtered & Search */
.empty-state.empty-filtered .empty-icon,
.empty-state.empty-search .empty-icon {
  background: var(--mgp-surface-muted);
  color: var(--mgp-text-muted);
}

/* State: Error */
.empty-state.empty-error {
  border-color: var(--mgp-danger-border);
}

.empty-state.empty-error .empty-icon {
  background: var(--mgp-danger-bg);
  color: var(--mgp-danger);
}

/* State: Restricted / No Access */
.empty-state.empty-restricted {
  border-style: solid;
}

.empty-state.empty-restricted .empty-icon {
  background: var(--mgp-surface-muted);
  color: var(--mgp-text-muted);
}

/* State: Processing */
.empty-state.empty-processing .empty-icon {
  background: var(--mgp-surface-muted);
  color: var(--mgp-primary-dark);
}
```