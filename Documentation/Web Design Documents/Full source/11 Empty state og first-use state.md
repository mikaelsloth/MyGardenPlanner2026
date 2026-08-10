# 11. Empty states og first-use states

## Overordnet princip

Empty states skal svare på tre spørgsmål:

1. **Hvad er tomt?**\
   Fx “Der er ingen bede i denne have endnu.”

2. **Hvorfor betyder det noget?**\
   Fx “Bede hjælper dig med at opdele haven i arbejdsområder.”

3. **Hvad kan brugeren gøre nu?**\
   Fx “Opret det første bed.”

Eksempel:

```text
Ingen bede endnu
Opret det første bed i Villa Solbakken for at begynde planlægningen.

[Opret bed]
```

Det er meget bedre end:

```text
Ingen data fundet
```

***

# 11.1 Empty state er ikke én ting

Jeg vil skelne mellem flere typer tomme tilstande.

| Type             | Betydning                                 | Eksempel                          |
| ---------------- | ----------------------------------------- | --------------------------------- |
| First-use empty  | Brugeren har aldrig oprettet noget endnu  | Ingen haver endnu                 |
| Context empty    | En konkret entity mangler underdata       | Denne have har ingen bede         |
| Filtered empty   | Data findes, men ikke med aktuelle filtre | Ingen planter matcher “sol + blå” |
| Search empty     | Søgeterm gav ingen resultater             | Ingen resultater for “lavenddel”  |
| Permission empty | Brugeren kan ikke se data pga. adgang     | Du har ikke adgang til filer      |
| Temporary empty  | Data er under oprettelse/behandling       | Thumbnail oprettes                |
| Error-empty      | Noget kunne ikke hentes                   | Vi kunne ikke hente filerne       |

Det er vigtigt, at disse **ikke ligner hinanden for meget**.

***

# 11.2 First-use state

First-use state er det brugeren møder, når en funktion bruges første gang.

Eksempel:

* ny bruger har ingen haver
* ny have har ingen bede
* ny planteoversigt har ingen custom planter
* ny have har ingen filer
* ingen kunder inviteret endnu

## Designprincip

> First-use states skal være venlige, forklarende og lidt guidende — men ikke lange onboarding-tekster.

De skal ikke undervise i hele systemet. De skal hjælpe brugeren med næste skridt.

## Eksempel: Ingen haver endnu

```html
<div class="empty-state empty-state-primary">
  <div class="empty-icon">✿</div>
  <div>
    <h2>Ingen haver endnu</h2>
    <p>Opret din første have for at begynde at planlægge bede, planter, materialer og filer.</p>
  </div>
  <div class="empty-actions">
    <button class="btn btn-primary">Opret have</button>
  </div>
</div>
```

## Tekstprincip

God first-use copy:

```text
Ingen haver endnu
Opret din første have for at begynde planlægningen.
```

Mindre god:

```text
Der findes ingen poster i databasen.
```

***

# 11.3 Context empty state

Context empty opstår, når brugeren har en kontekst, men en undersektion er tom.

Eksempel:

* Villa Solbakken har ingen bede
* Bedet har ingen planter
* Haven har ingen filer
* Haven har ingen medlemmer
* Projektet har ingen materialer

## Eksempel: Ingen bede i have

```text
Ingen bede i Villa Solbakken endnu
Bede hjælper dig med at opdele haven i overskuelige arbejdsområder.

[Opret bed]
```

Her er konteksten vigtig: vi siger ikke bare “Ingen bede”, men “Ingen bede i Villa Solbakken”.

## Eksempel: Ingen filer i have

```text
Ingen filer endnu
Upload tegninger, referencefotos eller dokumenter, så de er samlet på haven.

[Upload fil]
```

***

# 11.4 Filtered empty state

Filtered empty er nok en af de vigtigste i lister.

Eksempel:

Brugeren søger efter planter med:

```text
Lys = Sol
Type = Staude
Farve = Blå
Søgning = lavendel
```

Og intet matcher.

Her skal UI’et ikke sige:

```text
Ingen planter endnu
```

For det er forkert. Der findes måske planter — bare ikke med disse filtre.

## Rigtig tekst

```text
Ingen planter matcher dine filtre
Prøv at ændre søgningen eller nulstil filtrene.

[Nulstil filtre]
```

## Hvis søgning er årsagen

```text
Ingen resultater for “lavenddel”
Kontrollér stavningen, eller prøv med et bredere søgeord.

[Ryd søgning]
```

## Designprincip

> Empty states skal fortælle forskellen på “der findes intet” og “dit filter viser intet”.

Det er vigtigt for brugerens tillid.

***

# 11.5 Empty state efter oprettelse, hvor filter skjuler nyt objekt

Det her hænger direkte sammen med navigation state, som vi talte om.

Eksempel:

* brugeren står på plantelisten med filter `Lys = Sol`
* brugeren opretter en plante med `Lys = Skygge`
* brugeren returnerer til listen
* planten vises ikke

Hvis UI’et bare viser listen uden den nye plante, kan brugeren tro, at noget gik galt.

## God statusbesked

```text
Planten er oprettet
Den vises ikke i listen, fordi det aktive filter er “Sol”.

[Nulstil filter]
```

Dette er en særlig type “post-action empty/hidden state”.

## Designregel

> Hvis en ny eller ændret entity ikke vises pga. aktive filtre, skal UI’et forklare hvorfor.

***

# 11.6 Permission empty state

Permission empty opstår, når noget findes, men brugeren ikke har adgang.

Eksempel:

* kunden kan se haveoverblik, men ikke tilbudsfiler
* bruger kan se metadata, men ikke downloade originalfil
* bruger kan se have, men ikke medlemmer

## Eksempel

```text
Du har ikke adgang til disse filer
Filerne findes, men din rolle giver ikke adgang til at se eller downloade dem.

[Kontakt haveejer]
```

Det er bedre end bare at skjule alt, hvis det ellers skaber forvirring.

## Designregel

> Hvis brugeren forventer data, men ikke kan se det pga. adgang, skal UI’et forklare situationen roligt.

Men: hvis det er sikkerhedsmæssigt bedre slet ikke at afsløre, at data findes, skal UI’et være mere generisk:

```text
Ingen filer tilgængelige
```

Det er en produkt-/sikkerhedsbeslutning.

***

# 11.7 Error-empty state

Error-empty må ikke ligne en almindelig tom tilstand.

Eksempel:

```text
Filer kunne ikke hentes
Der opstod en fejl under indlæsningen. Prøv igen.

[Prøv igen]
```

## Brug error-empty når

* API-kald fejler
* filer ikke kan hentes
* netværksfejl
* preview ikke kan oprettes
* permissions lookup fejler
* uploadstatus ikke kan læses

## Designregel

> Error-empty skal altid have en handling, hvis brugeren kan gøre noget.

Typiske handlinger:

* Prøv igen
* Gå tilbage
* Kontakt support
* Genindlæs siden

***

# 11.8 Temporary / processing empty state

Relevant for filer og thumbnails.

Eksempel:

```text
Thumbnail oprettes
Filen er uploadet, men forhåndsvisningen er ikke klar endnu.

[Vis metadata]
```

Eller:

```text
Preview ikke klar
Du kan downloade originalfilen, mens preview bliver oprettet.

[Download original]
```

Det er ikke en fejl — det er en midlertidig tilstand.

## Designregel

> Processing states skal forklare, at systemet arbejder, og hvad brugeren kan gøre imens.

***

# 11.9 Empty state komponent

Jeg vil foreslå en generel komponent:

```text
EmptyState
```

Med varianter:

```text
empty-default
empty-first-use
empty-filtered
empty-search
empty-restricted
empty-error
empty-processing
```

## Struktur

```html
<div class="empty-state empty-first-use">
  <div class="empty-icon">✿</div>

  <div class="empty-content">
    <h2>Ingen haver endnu</h2>
    <p>Opret din første have for at begynde planlægningen.</p>
  </div>

  <div class="empty-actions">
    <button class="btn btn-primary">Opret have</button>
  </div>
</div>
```

## CSS-forslag

```css
.empty-state {
  display: grid;
  gap: var(--space-md);
  justify-items: center;
  text-align: center;
  padding: var(--space-2xl) var(--space-lg);
  border: 1px dashed var(--mgp-border);
  border-radius: var(--radius-lg);
  background: var(--mgp-surface);
}

.empty-state-inline {
  justify-items: start;
  text-align: left;
  padding: var(--space-lg);
}

.empty-icon {
  width: 3rem;
  height: 3rem;
  border-radius: 999px;
  display: grid;
  place-items: center;
  background: var(--mgp-primary-soft);
  color: var(--mgp-primary-dark);
  font-weight: var(--font-weight-bold);
  font-size: 1.4rem;
}

.empty-content {
  display: grid;
  gap: var(--space-xs);
  max-width: 52ch;
}

.empty-content p {
  margin: 0;
  color: var(--mgp-text-muted);
}

.empty-actions {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
  justify-content: center;
}
```

Til error:

```css
.empty-error {
  border-color: rgba(159, 58, 56, .35);
}

.empty-error .empty-icon {
  background: #fff4f3;
  color: var(--mgp-danger);
}
```

Til filtered:

```css
.empty-filtered .empty-icon {
  background: var(--mgp-surface-muted);
  color: var(--mgp-text-muted);
}
```

***

# 11.10 Full-page vs. inline empty states

Vi bør have to størrelser.

## Full-page empty state

Bruges når hele siden er tom.

Eksempel:

* ingen haver endnu
* ingen planter endnu
* ingen materialer endnu

```html
<div class="empty-state">
  ...
</div>
```

## Inline empty state

Bruges inde i en sektion/card/tab.

Eksempel:

* denne have har ingen filer
* dette bed har ingen planter
* ingen invitationer på denne have

```html
<div class="empty-state empty-state-inline">
  ...
</div>
```

## Designregel

> Brug stor empty state når hele siden mangler indhold. Brug inline empty state når kun en sektion er tom.

***

# 11.11 Empty states i MyGardenPlanner

Her er konkrete forslag.

## Haver

```text
Ingen haver endnu
Opret din første have for at begynde at planlægge bede, planter, materialer og filer.

[Opret have]
```

## Bede

```text
Ingen bede endnu
Bede hjælper dig med at opdele haven i overskuelige arbejdsområder.

[Opret bed]
```

## Planter

```text
Ingen planter endnu
Opret planter, så de kan bruges i dine bede og projekter.

[Opret plante]
```

## Materialer

```text
Ingen materialer endnu
Tilføj materialer som sten, jord, kanter eller belægning til dine projekter.

[Opret materiale]
```

## Filer

```text
Ingen filer endnu
Upload tegninger, referencefotos eller dokumenter, så de er samlet på haven.

[Upload fil]
```

## Invitationer

```text
Ingen invitationer endnu
Invitér en kunde eller samarbejdspartner til at se eller deltage i haveplanen.

[Invitér person]
```

## Butikker

```text
Ingen butikker endnu
Tilføj butikker og leverandører, så du kan samle indkøb og webshoplinks.

[Opret butik]
```

***

# 11.12 Empty state med sekundær handling

Nogle empty states bør have både primær og sekundær handling.

Eksempel: ingen planter.

```text
Ingen planter endnu
Opret en plante manuelt, eller start med at gennemgå forslag.

[Opret plante] [Se eksempel]
```

Men vi skal passe på ikke at give for mange valg.

## Designregel

> Empty states bør som udgangspunkt have én primær handling. Sekundær handling bruges kun, hvis den tydeligt hjælper.

***

# 11.13 Empty state og onboarding

First-use states kan også fungere som let onboarding.

Men jeg vil undgå lange forklaringer.

Dårligt:

```text
Velkommen til MyGardenPlanner. Her kan du bruge vores avancerede...
```

Bedre:

```text
Start med en have
En have samler bede, filer, planter og medlemmer ét sted.

[Opret have]
```

## Progressiv onboarding

I stedet for én stor onboarding bør appen guide lokalt:

* ingen haver → opret have
* ingen bede → opret bed
* ingen filer → upload fil
* ingen medlemmer → invitér kunde

## Designregel

> Onboarding sker bedst kontekstuelt, dér hvor brugeren mangler næste skridt.

***

# 11.14 Empty state og billeder/ikoner

Vi skal være forsigtige med illustrationer.

I vores designretning bør empty states ikke have store spraglede illustrationer.

Brug hellere:

* enkel ikon-cirkel
* lille botanisk symbol
* dokumentikon
* uploadikon
* låseikon ved no access
* lup ved søgning/filter

Eksempel:

| State            | Ikon      |
| ---------------- | --------- |
| Ingen haver      | ✿         |
| Ingen bede       | ◌         |
| Ingen filer      | ⇧ eller ▧ |
| Ingen planter    | 🌿        |
| Ingen materialer | ▣         |
| Ingen resultater | ⌕         |
| Ingen adgang     | 🔒        |
| Fejl             | !         |

## Designregel

> Empty-state ikoner skal støtte betydningen, ikke dekorere for meget.

***

# 11.15 Empty state og responsiv adfærd

På mobil skal empty states være kompakte nok til ikke at føles som en hel landingpage hver gang.

Mobilprincipper:

* én kolonne
* korte tekster
* fuld bredde på primær knap
* ikke for stor vertikal luft
* ikon max 3rem
* handling tydelig

CSS:

```css
@media (max-width: 640px) {
  .empty-state {
    padding: var(--space-xl) var(--space-md);
  }

  .empty-actions .btn {
    width: 100%;
  }
}
```

***

# 11.16 Empty state og loading

Empty state må ikke vises for tidligt, mens data stadig loader.

Ellers får brugeren et “flash” af:

```text
Ingen filer endnu
```

og derefter kommer filerne.

## Designregel

> Loading state skal vises før empty state, indtil data faktisk er afklaret.

Flow:

```text
Loading → data fundet → vis liste
Loading → ingen data → vis empty state
Loading → fejl → vis error state
```

Det er vigtigt i Blazor Server, hvor dataindlæsning kan give små pauser.

***

# 11.17 Empty state og skeletons

For større lister kan skeleton være bedre end spinner.

Eksempel:

```html
<div class="skeleton-card"></div>
<div class="skeleton-card"></div>
<div class="skeleton-card"></div>
```

Men til empty states bør skeleton kun være loading — ikke tom tilstand.

Guideline:

```text
Skeleton = vi henter data
Empty state = vi har hentet data, og der er intet at vise
```

***

# 11.18 Empty state og status messages

Empty states og status messages har forskellig rolle.

| Komponent      | Brug                                        |
| -------------- | ------------------------------------------- |
| Empty state    | Der mangler indhold i en sektion eller side |
| Status message | Der er en status, handling eller besked     |
| Error empty    | En hel visning kan ikke vises pga. fejl     |

Eksempel:

Efter upload:

```text
Filen er uploadet
Haveskitse maj.pdf er tilføjet.
```

Det er en status message, ikke empty state.

***

# 11.19 Guidelines for tekst

Jeg ville lave en lille copy-formel:

```text
[Situation]
[Hvorfor / kontekst]
[Handling]
```

## Eksempler

### First-use

```text
Ingen haver endnu
Opret din første have for at begynde planlægningen.
[Opret have]
```

### Filtered empty

```text
Ingen planter matcher filtrene
Prøv at ændre søgningen eller nulstil filtrene.
[Nulstil filtre]
```

### Permission empty

```text
Du har ikke adgang til filerne
Kontakt haveejeren, hvis du skal kunne se eller downloade dem.
```

### Error empty

```text
Filer kunne ikke hentes
Prøv igen, eller vend tilbage senere.
[Prøv igen]
```

***

# 11.20 Hvad vi bør undgå

Undgå:

```text
Ingen data
Tom liste
0 resultater
Fejl
Ikke fundet
```

Medmindre det står sammen med forklaring og handling.

Undgå også:

* for mange knapper
* store illustrationer på hver tom sektion
* tekniske databaseord
* empty states der skjuler fejl
* error states der ligner almindelig tom visning

***

# 11.21 Beslutningstabel

| Situation                 | UI                            | Primær handling        |
| ------------------------- | ----------------------------- | ---------------------- |
| Ingen haver               | Full-page first-use empty     | Opret have             |
| Ingen bede i have         | Inline context empty          | Opret bed              |
| Ingen filer               | Inline empty                  | Upload fil             |
| Ingen planter overhovedet | Full-page first-use empty     | Opret plante           |
| Ingen filterresultater    | Filtered empty                | Nulstil filtre         |
| Ingen søgeresultater      | Search empty                  | Ryd søgning            |
| Ingen adgang              | Restricted empty              | Forklar / kontakt ejer |
| Data loader               | Skeleton/loading              | Ingen eller annullér   |
| Datafejl                  | Error empty                   | Prøv igen              |
| Ny data skjult af filter  | Status + filtered explanation | Nulstil filter         |

***

# 11.22 Anbefalet designbeslutning

Jeg ville låse empty state-principperne sådan:

1. **Empty states skal hjælpe brugeren videre.**
2. **First-use states skal være venlige og handlingsorienterede.**
3. **Filtered empty må ikke forveksles med “ingen data”.**
4. **Search empty skal vise søgeterm og give mulighed for at rydde søgning.**
5. **Permission empty skal forklares roligt, hvis det er hensigtsmæssigt.**
6. **Error empty skal være tydeligt forskellig fra almindelig empty.**
7. **Loading skal afklares før empty state vises.**
8. **Empty states bør som udgangspunkt have én primær handling.**
9. **Inline empty states bruges i sektioner; full-page empty bruges når hele siden mangler indhold.**
10. **Ikoner skal være enkle og understøtte betydningen.**
11. **Hvis ny data ikke vises pga. filter, skal UI’et forklare hvorfor.**
12. **Empty state-komponenten bør være genbrugelig på tværs af appen.**

***

# Kort dokumentationsformulering

Du kan bruge denne direkte i dokumentationen:

> **Empty states:** En tom tilstand skal forklare hvad der mangler, hvorfor det er relevant, og hvad brugeren kan gøre nu. First-use states hjælper brugeren i gang med første naturlige handling. Filtered empty states skal tydeligt adskilles fra reelt tomme lister, og bør tilbyde at rydde eller nulstille filtre. Error og no-access states må ikke ligne almindelige empty states. Loading skal altid afklares før empty state vises. Empty states bør som udgangspunkt have én tydelig primær handling og bruge en rolig, enkel visuel stil.