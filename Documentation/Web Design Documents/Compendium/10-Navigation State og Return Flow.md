### 📄 10-Navigation State og Return Flow.md

# 10 - Navigation State og Return-Flow

## 📘 Grundregel
- Når brugeren afbryder en arbejdskontekst for at udføre en relateret handling (f.eks. oprette, redigere eller uploade), skal appen bevare brugerens samlede arbejdskontekst ved retur. Søgning, filtre, sortering, aktiv tab, view-mode og scroll-position/anchor-id skal gendannes automatisk. Udførte handlinger bekræftes med en rolig statusbesked og midlertidig visual highlight af det berørte element.

---

## 🔍 Anvendelse / varianter / typer

### State-typer og Opbevaring
| State-type | Opbevaring | Formål / Opførsel |
| :--- | :--- | :--- |
| **Query-state** | URL query parameters (`?search=..&lys=..&sort=..&view=..&page=..&tab=..`) | Filtre, søgning, sortering, pagination (`page`), view-mode og aktiv tab. Kan deles, genindlæses og understøtter browser back/forward. |
| **Workflow-state** | `returnUrl` (eller Session) | Kender den nøjagtige udgangskontekst under opret/rediger/upload-flows, så brugeren returneres korrekt. |
| **Selection / Anchor** | URL / Anchor parameter (`?returnToId=123`) | Identificerer og auto-scroller direkte til det berørte element efter oprettelse eller opdatering. |
| **UI-præference** | LocalStorage | Layout- og visningsvalg på tværs af sessions (f.eks. collapsed sidebar eller foretrukket kompakt-visning). |

### Return-flow Mønstre
| Mønster | Hvornår bruges det | Opførsel |
| :--- | :--- | :--- |
| **Modal / Drawer** | Korte, kontekstuelle handlinger (f.eks. hurtig-upload, tilknytning) | Bevarer baggrundens liste-state og scrollposition intakt uden sidetransition. |
| **Fuld Side-flow** | Komplekse formularer og tunge redigeringsforløb | Benytter `returnUrl` samt anchor-id til at genoprette den oprindelige visning og position ved retur. |
| **Destruktive handlinger** | Gennemførelse af sletning, arkivering eller fjernelse af adgang | Brugeren bevarer sin overordnede liste- eller arbejds-kontekst, og handlingen bekræftes øjeblikkeligt med en rolig `MgpStatusMessage` (f.eks. *"Filen 'Haveskitse maj.pdf' blev slettet"*). |
| **Arkivering / Gendannelse** | Arkivering eller reaktivering af en entitet | Ved arkivering fjernes objektet fra den aktive liste, og der vises statusbeskeden: *"Haven 'Villa Solbakken' blev arkiveret. [Vis arkiverede]"*. Ved gendannelse returneres objektet til den aktive liste og fremhæves med `.target-highlight`. |

---

## 🚫 Regler (Do / Don't)
- **Do:** Gemme alt state, der ændrer det synlige indhold, direkte i URL'en (søgning, filter, tabs, sortering).
- **Do:** Anvende specifikke, kontekstuelle tilbageknapper (f.eks. `"Tilbage til plantelisten"`) fremfor anonyme `"Tilbage"`.
- **Do:** Highlighte det nye/redigerede element kortvarigt (2-3 sekunder) ved retur til oversigten.
- **Do:** Bekræft altid fuldførte return-flow handlinger (fx efter oprettelse eller redigering af en enhed) med en persistent inline `MgpStatusMessage` placeret øverst på destinationssiden (`.status-page`). Beskeden skal forblive synlig, så brugeren i sit eget tempo kan verificere handlingen, og må **ikke** erstattes af en forsvindende toast.
- **Do:** Gem altid søgetermer, aktive filtre, sorteringsvalg, visningsform (view-mode) og sidenummer i URL query string, så hele arbejdsrummet genoprettes præcist ved browser refresh, deling af link eller retur fra et opret/rediger-flow.
- **Do:** Gem aktiv context tab på detail pages direkte i URL'ens query string (`/haver/123?tab=filer`), så brugeren ved genindlæsning, deling af link eller retur fra et edit-flow (`/haver/123/rediger?returnUrl=/haver/123?tab=filer`) vender tilbage til det nøjagtige faneblad.
- **Don't:** Nulstille brugerens valgte filtre eller scroll-position ved retur fra et opret-/rediger-flow.
- **Don't:** Efterlade brugeren uden forklaring, hvis et nyligt oprettet element filtreres væk af aktive søgekriterier.
- **Don't:** Genindlæsning af data eller brug af loading states aldrig må "ryste" brugerens position eller nulstille URL query-parameters, valgte filtre eller scroll-positioner.	
- **Edge cases:** 
  - **Element skjult af filter efter gem:** Hvis et nyligt oprettet eller redigeret element ikke vises i oversigten pga. et aktivt filter eller en søgeterm, må UI'et **ikke** vise en standard empty state. Der skal i stedet vises en inline `MgpStatusMessage` med forklaring og direkte nulstillingshandling: *"Planten blev oprettet, men vises ikke med det aktuelle filter 'Sol'. [Nulstil filter]"*.
  - **Store/Virtuelle lister:** Prioritér anchor-id / `returnToId` til at scrolle elementet i view fremfor upålidelig pixel-baseret scroll.

---

## 🧩 Komponentpåvirkning
- **Type:** Pattern Component
- **Nye Razor-komponenter:**
  - `ContextBackButton.razor`: Kontekstuel knap/link med dynamisk label (`Tilbage til [X]`) baseret på `returnUrl`.
  - `HighlightContainer.razor`: Wrapper-komponent til midlertidig visuel markering af oprettede/ændrede elementer.
- **Ændrede Razor-komponenter:**
  - `StatusMessage.razor`: Skal understøtte inline knapper/handlinger (f.eks. *"Nulstil filter"*) direkte i bekræftelsesbeskeden.

---

## 🪙 Tokenpåvirkning
Nye tokens til highlight/return-feedback føjet til det globale lag:

```css
:root {
  --mgp-highlight-bg: rgba(63, 107, 74, 0.15);
  --mgp-highlight-fade-duration: 2.5s;
}
```

Eksisterende genbrugte tokens:
- `--mgp-surface`, `--mgp-border`, `--mgp-primary-soft`, `--mgp-primary-dark`, `--mgp-text-muted`, `--mgp-warning-bg`.

---

## 💻 CSS & Bootstrap

```css
/* ==========================================================================
   Navigation State & Return-Flow Styles
   ========================================================================== */

/* Highlight af oprettet/redigeret element ved retur */
@keyframes mgp-fade-highlight {
  0% {
    background-color: var(--mgp-highlight-bg);
    box-shadow: 0 0 0 2px var(--mgp-primary-soft);
  }
  100% {
    background-color: transparent;
    box-shadow: none;
  }
}

.target-highlight {
  animation: mgp-fade-highlight var(--mgp-highlight-fade-duration) ease-out forwards;
  scroll-margin-top: 5rem; /* Sikrer tilstrækkelig afstand under sticky header ved auto-scroll */
}

/* Kontekstuel Tilbageknap */
.btn-context-back {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--mgp-text-muted);
  text-decoration: none;
  font-weight: 500;
  font-size: 0.875rem;
  padding: 0.25rem 0.5rem;
  border-radius: var(--bs-border-radius);
  transition: background-color 0.15s ease-in-out, color 0.15s ease-in-out;
}

.btn-context-back:hover {
  background-color: var(--mgp-primary-soft);
  color: var(--mgp-primary-dark);
}
```