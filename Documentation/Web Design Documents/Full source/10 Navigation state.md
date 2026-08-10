# 10. Navigation state og return-flow

## Overordnet princip

> **Når brugeren afbryder en kontekst for at udføre en relateret handling, skal brugeren tilbage til samme kontekst — helst samme position, samme filtre, samme sortering og samme visuelle punkt.**

Det handler ikke kun om “tilbage til samme side”. Det handler om at bevare brugerens **arbejdskontekst**.

Eksempel:

```text
Brugeren er på plantelisten
- scrollet 70% ned
- filter: Sol
- søgning: "lavendel"
- sortering: Almindeligt navn
- valgt visning: compact list

Brugeren klikker: Tilføj plante
Brugeren gemmer planten

Forventning:
Tilbage til plantelisten med samme filter, søgning, sortering, visning og scrollposition.
```

Det er præcis den slags, der får en app til at føles professionel.

***

# 10.1 Hvad er “state”?

Jeg ville skelne mellem flere typer state.

## 1. Side-state

Hvilken side brugeren er på.

```text
/planter
/materialer
/haver/villa-solbakken/bede
```

## 2. Query-state

Filtre, søgning, sortering, pagination og view-mode.

```text
?search=lavendel&type=staude&light=sol&sort=name&view=compact
```

## 3. UI-state

Ting der ikke nødvendigvis bør være i URL’en, men som stadig betyder noget:

* sidebar collapsed/expanded
* åbne/klappede sektioner
* valgt tab
* valgt kort
* valgt række
* aktivt preview panel
* scrollposition

## 4. Workflow-state

At brugeren kom fra et bestemt flow.

```text
Fra planteliste → tilføj plante → tilbage til planteliste
Fra haveoverblik → upload fil → tilbage til haveoverblik
Fra beddetalje → opret materiale → tilbage til beddetalje
```

## 5. Selection-state

Hvad brugeren havde markeret eller arbejdede med:

* valgt have
* valgt bed
* valgt filter
* valgt fil
* aktivt preview
* markeret række

***

# 10.2 Hvad bør gemmes hvor?

Jeg vil foreslå denne opdeling.

| State-type               | Hvor bør den gemmes?                | Hvorfor                                               |
| ------------------------ | ----------------------------------- | ----------------------------------------------------- |
| Filter/søgning/sortering | URL query string                    | Kan deles, reloades og back/forward virker            |
| Aktuel tab               | URL eller query                     | Tabs repræsenterer ofte rigtig navigation             |
| Pagination               | URL query                           | Brugeren skal kunne komme tilbage til samme side      |
| View-mode                | URL eller local storage             | Afhænger af om det er sidevalg eller brugerpræference |
| Scrollposition           | Browser/session state               | Skal gendannes ved return-flow                        |
| Sidebar collapsed        | Local storage                       | Brugerpræference                                      |
| Midlertidig form data    | Session/component state             | Skal ikke nødvendigvis i URL                          |
| Preview åbent            | Ofte URL/query eller lokal UI-state | Afhænger af om preview skal kunne deles               |

***

# 10.3 URL’en bør bære vigtig liste-state

For oversigter anbefaler jeg, at URL’en bærer den state, brugeren forventer at kunne vende tilbage til.

Eksempel:

```text
/planter?search=lavendel&lys=sol&type=staude&sort=name&view=compact
```

Eller:

```text
/haver/123/bede?sort=reference&view=list
```

Det giver flere fordele:

* browser back/forward virker naturligt
* refresh mister ikke kontekst
* links kan deles
* “tilbage efter opret” er nemmere
* state bliver synlig og forudsigelig

## Designregel

> Hvis state ændrer hvilket indhold brugeren ser, bør den som hovedregel ligge i URL’en.

Det gælder især:

* søgning
* filter
* sortering
* pagination
* aktiv tab
* view-mode, hvis den ændrer informationsarkitekturen

***

# 10.4 Scrollposition er en del af brugerens arbejdssted

Det du beskriver med “samme punkt på siden” er vigtigt.

Brugeren tænker ikke:

> “Jeg var på `/planter`.”

Brugeren tænker:

> “Jeg var nede ved lavendel-området i listen.”

Derfor bør scrollposition gendannes ved return-flow.

## Designregel

> Når brugeren vender tilbage fra en relateret opret/rediger/preview-handling, skal scrollposition gendannes, hvis indholdet stadig eksisterer.

Eksempel:

```text
Liste → Opret plante → Gem → Tilbage til liste ved samme scrollposition
```

Hvis den nye plante er relevant for listen, kan man gøre to ting:

1. returnere til samme position og vise statusbesked
2. returnere til den nye plante i listen og highlight den kort

Jeg vil faktisk anbefale nr. 2 i nogle flows.

***

# 10.5 “Return to context” efter opret/rediger

Når brugeren udfører en handling, bør vi definere et eksplicit return-flow.

## Eksempel: Tilføj plante fra planteliste

```text
/planter?search=lavendel&lys=sol&sort=name
→ /planter/opret?returnUrl=...
→ Gem
→ tilbage til returnUrl
→ vis status: "Planten er oprettet"
→ highlight ny plante, hvis den er synlig i listen
```

## Eksempel: Upload fil fra haveoverblik

```text
/haver/123?tab=filer
→ /haver/123/filer/upload?returnUrl=...
→ Upload
→ tilbage til /haver/123?tab=filer
→ vis status: "Filen er uploadet"
```

## Eksempel: Rediger materiale fra beddetalje

```text
/haver/123/bede/4?tab=materialer
→ /materialer/456/rediger?returnUrl=...
→ Gem
→ tilbage til beddetaljens materialetab
```

## Designregel

> Opret/rediger-sider bør kende deres “return context”, hvis de startes fra en konkret arbejdskontekst.

***

# 10.6 “Tilbage”-knap må ikke altid være browser-back

Der er forskel på:

```text
Browser back
```

og

```text
Tilbage til plantelisten
```

Hvis brugeren åbner en side direkte, findes der ikke nødvendigvis en meningsfuld intern back-stack.

Derfor bør UI’et bruge kontekstuelle tilbageknapper:

```html
<button class="btn btn-secondary">Tilbage til planter</button>
```

eller mere specifikt:

```html
<button class="btn btn-secondary">Tilbage til Villa Solbakken</button>
```

## Designregel

> Vis “Tilbage til \[kontekst]” når appen kender return context. Brug ikke kun generisk “Tilbage”.

Gode labels:

```text
Tilbage til plantelisten
Tilbage til Villa Solbakken
Tilbage til beddet
Tilbage til filer
Tilbage til materialelisten
```

Mindre gode labels:

```text
Tilbage
OK
Luk
```

***

# 10.7 Status efter return

Når brugeren kommer tilbage, skal appen bekræfte hvad der skete.

Eksempel:

```text
[Status message]
Planten er oprettet
Lavendel er tilføjet til plantelisten.
```

Eller:

```text
Filen er uploadet
Haveskitse maj.pdf er tilføjet til Villa Solbakken.
```

Dette passer rigtig godt til vores status message cards.

## Designregel

> Return-flow bør ledsages af en kort statusbesked, så brugeren forstår at handlingen lykkedes.

Statusbeskeden skal dog ikke overtage siden. Den bør være inline og rolig.

***

# 10.8 Highlight det nye eller ændrede element

Hvis brugeren opretter eller redigerer noget, kan det være en stor hjælp at vise det visuelt, når de kommer tilbage.

Eksempel:

```text
Plante oprettet → returner til liste → ny plante har selected/highlight state i få sekunder
```

UI:

```html
<article class="card card-selected">
  <h3>Lavendel</h3>
  <p class="latin-name">Lavandula angustifolia</p>
</article>
```

## Designregel

> Efter return-flow bør det relevante element markeres kortvarigt, hvis det findes i den aktuelle visning.

Hvis elementet ikke matcher de aktuelle filtre, skal brugeren have forklaring:

```text
Planten blev oprettet, men vises ikke med det aktuelle filter.
[Nulstil filter]
```

Det er et vigtigt edge case.

***

# 10.9 Når ny data ikke passer i det aktuelle filter

Eksempel:

* brugeren står på filter `Lys = Sol`
* opretter en plante med `Lys = Skygge`
* kommer tilbage til listen
* planten vises ikke

Hvis vi bare returnerer uden forklaring, tror brugeren måske, at oprettelsen fejlede.

## UI-princip

Vis en statusbesked:

```text
Planten er oprettet
Den vises ikke i listen, fordi det aktuelle filter er "Sol".
[Nulstil filter]
```

Dette er en af de vigtigste små UX-detaljer.

***

# 10.10 Back stack vs. returnUrl

Jeg vil skelne mellem to mekanismer.

## Browserhistorik

God til:

* almindelig navigation
* tilbage fra detaljeside
* back/forward i lister

## returnUrl / return state

God til:

* opret/rediger-flow
* modal/sidepanel-flow
* upload-flow
* workflows der kan startes fra flere steder

Eksempel:

```text
/planter/opret?returnUrl=/planter%3Fsearch%3Dlavendel%26lys%3Dsol
```

Alternativt kan man gemme return context i session state, hvis URL’en bliver for lang.

## Designregel

> Brug URL-state til synlig listekontekst og returnUrl/session-state til workflow-retur.

***

# 10.11 Modal vs. side navigation

Nogle handlinger kan undgå at forlade siden helt.

## Opret i modal eller drawer

Hvis handlingen er kort:

* upload fil
* invitér kunde
* hurtig opret materiale
* hurtig opret plante light-version

kan man bruge modal/drawer, så listepositionen bevares naturligt.

Fordel:

* scrollposition bevares
* brugeren mister ikke kontekst
* hurtigt flow

Ulempe:

* ikke egnet til lange formularer
* kan blive trangt på mobil
* kræver god focus management

## Fuldt sideflow

Bedre til:

* kompleks planteformular
* stor haveformular
* detaljeret materialeredigering
* tung fil-preview/full viewer

## Designregel

> Brug modal/drawer til korte, kontekstuelle handlinger. Brug fuld side til længere redigering.

***

# 10.12 Pagination og virtualiserede lister

Hvis lister bliver store, kommer vi måske til:

* pagination
* infinite scroll
* virtualisering
* lazy-loaded resultater

Her er state endnu vigtigere.

## Pagination

State bør ligge i URL:

```text
/planter?page=4&search=lavendel
```

## Infinite scroll

Her bør appen kunne gendanne:

* query
* antal loadede resultater
* scrollposition
* evt. anchor-id for elementet

## Virtualisering

Her er scrollposition alene måske ikke nok. Bedre er:

```text
returnToItemId=...
```

Eksempel:

```text
/planter?search=lavendel&returnTo=plant-123
```

## Designregel

> For store lister bør return-flow prioritere element-anchor over ren pixel-scroll.

Altså: hellere “scroll til plante 123” end “scroll til 1847px”, hvis listen kan ændre sig.

***

# 10.13 Anchor-baseret return

Det bedste mønster for store dynamiske lister er ofte:

```text
returnToId
```

Eksempel:

```text
/planter?search=lavendel&highlight=plant-123
```

Når siden loader:

* anvend filter/search
* find element med id
* scroll elementet ind i view
* marker det kortvarigt

## Fordel

Dette fungerer bedre end pixel-scroll, hvis:

* elementer er blevet tilføjet
* billeder loader langsomt
* kort har variabel højde
* listen er sorteret
* skærmstørrelsen ændrer sig

## Designregel

> Brug anchor-id/highlight-id til return-flow, når brugeren har arbejdet med et bestemt objekt.

***

# 10.14 State og preview-paneler

Hvis brugeren åbner preview af en fil fra en liste, bør man overveje om preview-state skal være i URL.

Eksempel:

```text
/haver/123/filer?preview=file-456
```

Fordel:

* reload bevarer preview
* link kan deles
* back lukker preview
* forward åbner preview igen

Det er især godt for filer og dokumenter.

## Designregel

> Preview af konkret objekt kan med fordel være URL-state, hvis det føles som en egentlig visning.

For små hover-previews behøver det ikke.

***

# 10.15 State og tabs

Tabs på detail-sider bør ofte være URL-state.

Eksempel:

```text
/haver/123?tab=bede
/haver/123?tab=filer
/haver/123?tab=medlemmer
```

Hvis brugeren uploader en fil fra `tab=filer`, skal de tilbage til `tab=filer`.

## Designregel

> Tabs, der repræsenterer meningsfulde sektioner, bør kunne gendannes via URL.

***

# 10.16 State og sidebar

Vi har allerede talt om collapsed sidebar som brugervalg.

Det bør gemmes som brugerpræference, fx senere i local storage.

## Designregel

> Layoutpræferencer som collapsed sidebar gemmes som brugerpræference, ikke som side-state.

Så:

```text
sidebarCollapsed = true
```

skal ikke nødvendigvis være i URL’en.

***

# 10.17 UI-guidelines

Jeg ville formulere konkrete UI-guidelines sådan:

## Tilbageknapper

Brug specifik tekst:

```text
Tilbage til plantelisten
Tilbage til Villa Solbakken
Tilbage til filer
```

## Statusbesked efter return

Vis en statusbesked øverst i indholdsområdet:

```text
Planten er oprettet
Lavendel er nu tilgængelig i plantelisten.
```

## Highlight

Marker det relevante element:

```text
Ny/opdateret plante kort får selected state kortvarigt
```

## Hvis element ikke vises

Forklar hvorfor:

```text
Planten blev oprettet, men vises ikke med det aktuelle filter.
```

## Bevar filterbar

Når brugeren kommer tilbage, skal filterbaren vise samme values.

## Bevar view-mode

Hvis brugeren var i compact view, skal de tilbage til compact view.

## Bevar scroll

Hvis der ikke er et konkret anchor-id, gendan scrollposition.

***

# 10.18 Hvad skal dokumenteres i designsystemet?

Jeg ville dokumentere disse mønstre:

## `Return context`

```text
Den side og tilstand brugeren skal tilbage til efter en handling.
```

## `Return action`

```text
En knap eller handling med specifik tekst: Tilbage til [kontekst].
```

## `Return status`

```text
Statusbesked der bekræfter handlingen efter retur.
```

## `Highlight target`

```text
Objekt der kort markeres efter oprettelse/redigering.
```

## `Empty filtered result after create`

```text
Når nyt element ikke matcher aktivt filter, forklares det.
```

## `Persistent view preference`

```text
Brugerens layoutvalg, fx collapsed sidebar eller compact view.
```

***

# 10.19 Konkret beslutningstabel

| Situation                       | Anbefalet adfærd                                      |
| ------------------------------- | ----------------------------------------------------- |
| Opret objekt fra liste          | Returnér til samme liste-state                        |
| Rediger objekt fra liste        | Returnér til samme scroll/anchor                      |
| Upload fil fra have             | Returnér til samme have og samme tab                  |
| Preview fil fra liste           | Luk preview uden at ændre liste-state                 |
| Download fil                    | Bliv på samme side                                    |
| Slet element                    | Bliv på listen, vis status og fjern/highlight ændring |
| Nyt element matcher ikke filter | Vis status med forklaring                             |
| Browser refresh                 | Gendan URL-båret state                                |
| Sidebar collapse                | Gendan brugerpræference                               |
| Lang liste                      | Brug anchor-id frem for pixel-scroll hvis muligt      |

***

# 10.20 Min anbefalede beslutning

Jeg ville låse principperne sådan:

1. **Brugeren skal tilbage til samme arbejdskontekst efter en handling.**
2. **Liste-state bør bevares: søgning, filter, sortering, pagination og view-mode.**
3. **Scrollposition eller anchor-id skal gendannes ved return-flow.**
4. **Tabs på detail-sider bør være URL-state.**
5. **Return-flow skal have specifik tilbageknaptekst.**
6. **Efter return vises en rolig statusbesked.**
7. **Nye eller ændrede elementer highlightes kortvarigt.**
8. **Hvis elementet ikke vises pga. filter, skal UI’et forklare det.**
9. **Layoutpræferencer gemmes som brugerpræferencer, ikke nødvendigvis i URL.**
10. **Korte handlinger kan ske i modal/drawer for at bevare kontekst.**
11. **Lange handlinger kan være fuld side, men skal kende return context.**
12. **For store/dynamiske lister bør anchor-id foretrækkes frem for pixel-scroll.**

***

# Kort formulering til dokumentationen

Hvis du vil have en kort version til design-dokumentet, ville jeg skrive:

> **Navigation state:** Når en bruger forlader en oversigt eller detaljeside for at udføre en relateret handling, skal appen bevare brugerens arbejdskontekst. Ved retur skal søgning, filtre, sortering, aktiv tab, view-mode og så vidt muligt scrollposition gendannes. Hvis handlingen opretter eller ændrer et objekt, bør objektet markeres kortvarigt efter retur. Hvis objektet ikke vises på grund af aktive filtre, skal UI’et forklare hvorfor og tilbyde en relevant handling, fx “Nulstil filter”. Layoutpræferencer som collapsed sidebar gemmes som brugerpræferencer.