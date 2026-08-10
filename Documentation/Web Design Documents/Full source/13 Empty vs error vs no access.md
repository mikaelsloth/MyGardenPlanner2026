# 13. Empty vs. error vs. no access

## Overordnet princip

> **UI’et skal tydeligt fortælle, om der ikke findes noget, om noget er skjult af filtre, om noget ikke kunne hentes, eller om brugeren ikke har adgang.**

Det er vigtigt, fordi brugeren reagerer forskelligt i hver situation:

| Situation      | Brugerens spørgsmål           |
| -------------- | ----------------------------- |
| Empty          | “Hvad gør jeg nu?”            |
| Filtered empty | “Hvordan finder jeg det?”     |
| Error          | “Kan jeg prøve igen?”         |
| No access      | “Hvorfor må jeg ikke se det?” |
| Loading        | “Arbejder systemet stadig?”   |

Hvis de ligner hinanden for meget, mister brugeren tillid.

***

## 13.1 Grundlæggende forskel

Jeg vil definere dem sådan:

| State          | Betydning                                | Tone                 | Handling                     |
| -------------- | ---------------------------------------- | -------------------- | ---------------------------- |
| Empty          | Der findes ikke noget endnu              | Hjælpende            | Opret / upload / invitér     |
| Filtered empty | Noget findes, men ikke med aktive filtre | Forklarende          | Nulstil filter / ryd søgning |
| Error          | Noget gik galt                           | Tydelig, men rolig   | Prøv igen / gå tilbage       |
| No access      | Brugeren har ikke adgang                 | Rolig og respektfuld | Kontakt ejer / forklaring    |
| Loading        | Data er ikke afklaret endnu              | Neutral              | Vent / evt. annullér         |

***

# 13.2 Empty: “Der er ikke noget endnu”

Empty bruges kun, når systemet faktisk ved, at der ikke findes relevant data i den givne kontekst.

## Eksempel

```text
Ingen filer endnu
Upload tegninger, referencefotos eller dokumenter, så de er samlet på haven.

[Upload fil]
```

## UI-kendetegn

* Neutral eller positiv tone
* Enkel ikon-cirkel
* Primær handling
* Ingen advarselsfarve
* Ingen “fejl”-sprog

## Brug når

* haven har ingen filer
* bedet har ingen planter
* brugeren har ingen haver
* der er ingen invitationer endnu
* materialelisten er tom

## Undgå

```text
Ingen data fundet
```

Det er teknisk og passivt.

## Bedre

```text
Ingen materialer endnu
Tilføj materialer som sten, jord eller kanter, så de kan bruges i projekter.

[Opret materiale]
```

***

# 13.3 Filtered empty: “Der findes noget, men ikke her”

Filtered empty må aldrig ligne first-use empty.

Hvis brugeren har aktive filtre eller søgning, er situationen ikke “tom”; den er “ingen match”.

## Eksempel

```text
Ingen planter matcher dine filtre
Prøv at ændre søgningen eller nulstil filtrene.

[Nulstil filtre]
```

## UI-kendetegn

* Neutral, lidt lavere visuel vægt end first-use empty
* Viser aktive filtre eller søgeterm
* Har handling til at rydde/nulstille
* Ikke primary “Opret” som første forslag, medmindre det giver særlig mening

## Brug når

* søgning ikke matcher
* filter udelukker alle resultater
* aktiv tab har filtrerede data
* sortering/pagination fører til tom side

## God detalje

Vis aktive filtre:

```text
Aktive filtre:
[Sol] [Staude] [Blå]
```

## Vigtig edge case

Hvis brugeren lige har oprettet noget, som ikke matcher filtrene:

```text
Planten er oprettet
Den vises ikke i listen, fordi det aktive filter er “Sol”.

[Nulstil filter]
```

Det er en statusbesked kombineret med filtered empty-logik.

***

# 13.4 Search empty: “Søgetermen gav ingen resultater”

Search empty er en variant af filtered empty, men bør nævne søgetermen.

## Eksempel

```text
Ingen resultater for “lavenddel”
Kontrollér stavningen, eller prøv med et bredere søgeord.

[Ryd søgning]
```

## UI-kendetegn

* Søgeterm vises tydeligt
* Handling: “Ryd søgning”
* Eventuelt sekundært: “Opret plante”, hvis relevant

## Brug når

* fri tekstsøgning er den primære årsag
* søgeterm kan være stavet forkert
* listen ellers har data

***

# 13.5 Error: “Noget gik galt”

Error state skal være tydeligt forskellig fra empty.

Hvis appen ikke kunne hente filer, er det ikke det samme som “ingen filer”.

## Eksempel

```text
Filer kunne ikke hentes
Der opstod en fejl under indlæsningen. Prøv igen.

[Prøv igen]
```

## UI-kendetegn

* Tydelig, men ikke voldsom
* Danger/fejlmarkering
* Konkret handling
* Ingen “opret”-handling som primær
* Må ikke give indtryk af, at data ikke findes

## Brug når

* API-kald fejler
* netværksfejl
* upload fejler
* preview fejler
* permission lookup fejler
* data ikke kan parses
* serverfejl

## Designregel

> **Error state skal aldrig skjules som empty state.**

Hvis brugeren ser “Ingen filer”, men der i virkeligheden var en fejl, mister de tillid.

***

# 13.6 No access: “Du må ikke se det”

No access er ikke en fejl i systemet. Det er en adgangsbegrænsning.

## Eksempel

```text
Du har ikke adgang til disse filer
Filerne findes, men din rolle giver ikke adgang til at se eller downloade dem.

[Kontakt haveejer]
```

## UI-kendetegn

* Rolig tone
* Låseikon eller dæmpet ikon
* Ingen danger-farve som standard
* Forklaring hvis hensigtsmæssigt
* Eventuelt disabled actions med forklaring
* Respektfuldt sprog

## Brug når

* brugerrollen ikke må se filer
* bruger må se metadata, men ikke original
* bruger må se have, men ikke medlemmer
* kunde/viewer ikke må redigere
* download ikke er tilladt

## Vigtig sikkerhedsvariant

Nogle gange bør UI’et ikke afsløre, at data findes.

I så fald bruges mere generisk tekst:

```text
Ingen filer tilgængelige
Der er ingen filer, du kan se med den aktuelle adgang.
```

## Designregel

> **Hvis det er sikkert og hjælpsomt, forklar adgangsbegrænsningen. Hvis ikke, brug en mere generisk “ikke tilgængelig”-tekst.**

***

# 13.7 Loading: “Vi ved det ikke endnu”

Loading er ikke empty, error eller no access. Det er en uafklaret tilstand.

## Eksempel

```text
Henter filer…
```

eller skeletons.

## Designregel

> **Vis aldrig empty, error eller no access før loading er afklaret.**

Flowet bør være:

```text
Loading → data → vis data
Loading → ingen data → empty
Loading → ingen match → filtered empty
Loading → fejl → error
Loading → adgang nægtet → no access
```

***

# 13.8 Visuel differentiering

Jeg vil bruge disse visuelle signaler:

| State     | Ikon             | Border         | Farve         | Handling         |
| --------- | ---------------- | -------------- | ------------- | ---------------- |
| Empty     | ✿ / 🌿 / ⇧       | dashed neutral | primær-soft   | primary create   |
| Filtered  | ⌕                | dashed neutral | muted         | secondary reset  |
| Search    | ⌕                | dashed neutral | muted         | secondary clear  |
| Error     | !                | danger border  | danger soft   | retry            |
| No access | 🔒               | neutral border | muted         | contact/request  |
| Loading   | skeleton/spinner | none/neutral   | muted shimmer | none/retry later |

## Designregel

> **Farve må understøtte betydning, men teksten skal bære betydningen.**

Det er især vigtigt for accessibility.

***

# 13.9 Tone of voice

## Empty

Venlig og hjælpsom:

```text
Ingen bede endnu
Opret det første bed for at begynde planlægningen.
```

## Filtered empty

Praktisk og neutral:

```text
Ingen planter matcher dine filtre
Prøv at ændre søgningen eller nulstil filtrene.
```

## Error

Klar og handlingsorienteret:

```text
Filer kunne ikke hentes
Prøv igen.
```

## No access

Respektfuld og rolig:

```text
Du har ikke adgang til disse filer
Kontakt haveejeren, hvis du skal kunne se dem.
```

## Undgå

* “Fejl 403”
* “Ingen data”
* “Access denied”
* “Unknown error”
* “Tom database”
* “Du må ikke…”

***

# 13.10 Handlinger: Hvad skal knappen være?

| State           | Primær handling                    |
| --------------- | ---------------------------------- |
| Empty           | Opret / upload / invitér           |
| Filtered empty  | Nulstil filtre                     |
| Search empty    | Ryd søgning                        |
| Error           | Prøv igen                          |
| No access       | Kontakt ejer / Anmod om adgang     |
| Loading         | Ingen eller annullér               |
| Processing      | Vis metadata / prøv senere         |
| Restricted file | Forklaring, evt. disabled download |

## Vigtig regel

> **Den primære handling skal matche årsagen til state.**

Hvis der ikke er data, er “Opret” rigtigt.\
Hvis der er filter, er “Nulstil filter” rigtigt.\
Hvis der er fejl, er “Prøv igen” rigtigt.\
Hvis der ikke er adgang, er “Kontakt ejer” eller “Anmod om adgang” rigtigt.

***

# 13.11 Hvornår skal handlinger skjules vs disabled?

Det her er vigtigt ved no access.

## Skjul handlingen når

* brugeren aldrig kan udføre den
* handlingen ikke er relevant for rollen
* det vil støje at vise den

Eksempel:

En viewer ser ikke “Slet have”.

## Disable handlingen når

* handlingen normalt findes her
* brugeren forventer den
* forklaring hjælper
* tilstanden kan ændre sig

Eksempel:

```text
Download
Ikke tilgængelig for din rolle
```

## Designregel

> **Skjul irrelevante handlinger. Disable forventede handlinger, hvis forklaring skaber værdi.**

***

# 13.12 Eksempler fra MyGardenPlanner

## Filer

### Empty

```text
Ingen filer endnu
Upload tegninger, referencefotos eller dokumenter, så de er samlet på haven.

[Upload fil]
```

### Error

```text
Filer kunne ikke hentes
Der opstod en fejl under indlæsningen.

[Prøv igen]
```

### No access

```text
Du har ikke adgang til filerne
Din rolle giver ikke adgang til at se eller downloade filer i denne have.

[Kontakt haveejer]
```

### Processing

```text
Thumbnail oprettes
Filen er uploadet, men forhåndsvisningen er ikke klar endnu.

[Vis metadata]
```

***

## Planter

### Empty

```text
Ingen planter endnu
Opret planter, så de kan bruges i dine bede og projekter.

[Opret plante]
```

### Filtered empty

```text
Ingen planter matcher filtrene
Prøv at ændre søgningen eller nulstil filtrene.

[Nulstil filtre]
```

### Search empty

```text
Ingen resultater for “lavenddel”
Kontrollér stavningen, eller prøv med et bredere søgeord.

[Ryd søgning]
```

***

## Invitationer

### Empty

```text
Ingen invitationer endnu
Invitér en kunde eller samarbejdspartner til at se eller deltage i haveplanen.

[Invitér person]
```

### Error

```text
Invitationer kunne ikke hentes
Prøv igen.

[Prøv igen]
```

### No access

```text
Du kan ikke administrere invitationer
Din rolle giver ikke adgang til at invitere eller ændre medlemmer.
```

***

# 13.13 Komponentvarianter

Jeg ville gøre dette til én genbrugelig komponent:

```text
StateMessage / EmptyState
```

Med `variant`:

```text
empty
filtered
search
error
restricted
processing
loading
```

## Eksempel

```html
<div class="empty-state empty-error">
  <div class="empty-icon">!</div>
  <div class="empty-content">
    <h2>Filer kunne ikke hentes</h2>
    <p>Der opstod en fejl under indlæsningen. Prøv igen.</p>
  </div>
  <div class="empty-actions">
    <button class="btn btn-danger">Prøv igen</button>
  </div>
</div>
```

## CSS-idé

```css
.empty-error {
  border-color: rgba(159, 58, 56, .35);
}

.empty-error .empty-icon {
  background: #fff4f3;
  color: var(--mgp-danger);
}

.empty-restricted .empty-icon,
.empty-filtered .empty-icon,
.empty-search .empty-icon {
  background: var(--mgp-surface-muted);
  color: var(--mgp-text-muted);
}
```

***

# 13.14 Beslutningsflow

Jeg ville dokumentere flowet sådan:

```text
1. Loader data stadig?
   → Loading/skeleton

2. Fejlede datahentning?
   → Error state

3. Mangler bruger adgang?
   → No access / restricted state

4. Findes der data, men ikke med aktuelle filtre?
   → Filtered/search empty

5. Findes der ingen data?
   → Empty / first-use / context empty

6. Er data under behandling?
   → Processing state
```

Det er en praktisk beslutningsmodel for udvikling.

***

# 13.15 Min anbefalede beslutning

Jeg ville låse principperne sådan:

1. **Empty, error og no access må aldrig bruge samme tekst eller visuelle vægt.**
2. **Loading skal afklares før empty/error/no access vises.**
3. **Empty hjælper brugeren videre med opret/upload/invitér.**
4. **Filtered/search empty hjælper brugeren med at ændre eller nulstille filter.**
5. **Error forklarer at noget gik galt og tilbyder retry.**
6. **No access forklarer adgangsbegrænsning roligt, hvis det er sikkert at gøre det.**
7. **No access er ikke en error-state.**
8. **Farver understøtter, men teksten bærer betydningen.**
9. **Skjul irrelevante handlinger; disable forventede handlinger med forklaring.**
10. **Brug én genbrugelig komponent med klare variants.**

***

# Kort dokumentationstekst

Du kan bruge denne direkte:

> **Empty vs. error vs. no access:** Tomme tilstande skal tydeligt adskilles fra fejl og adgangsbegrænsninger. Empty betyder, at der ikke findes data endnu, og skal hjælpe brugeren videre med næste naturlige handling. Filtered/search empty betyder, at data findes, men ikke matcher aktuelle filtre eller søgning, og skal tilbyde at rydde eller nulstille. Error betyder, at noget gik galt, og skal forklare problemet samt tilbyde retry. No access betyder, at brugeren ikke har adgang, og skal forklares roligt, hvis det er hensigtsmæssigt og sikkert. Loading skal altid afklares, før en af disse states vises.