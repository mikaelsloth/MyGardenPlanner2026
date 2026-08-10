# 24. Notifications og attention management

## Overordnet princip

> **Opmærksomhed skal være sparsom, forklarende og handlingsorienteret. Jo vigtigere noget er, desto mere kontekstuel, persistent og tydelig skal beskeden være.**

I MyGardenPlanner skal vi undgå, at UI’et bliver fyldt med røde/orange badges, popups og attention states. Hvis alt råber, råber intet.

Det betyder:

* ikke alle statusser skal være notifications
* ikke alle warnings skal være orange
* ikke alle fejl skal være globale
* ikke alle badges skal have stærk farve
* attention skal bruges til det, brugeren faktisk bør forstå eller handle på

***

## 24.1 Notification vs. status vs. badge vs. alert

Vi bør skelne tydeligt mellem flere typer opmærksomhed.

| Type                    | Formål                                 | Eksempel                               |
| ----------------------- | -------------------------------------- | -------------------------------------- |
| **Badge**               | Viser objektets tilstand               | `Arkiveret`, `Afventer`, `Midlertidig` |
| **Inline status**       | Forklarer en tilstand i kontekst       | “Upload mislykkedes”                   |
| **Toast**               | Kort, ikke-kritisk bekræftelse         | “Link kopieret”                        |
| **Alert**               | Kræver tydelig opmærksomhed            | “Betaling kræver handling”             |
| **Banner**              | Side-/konto-niveau information         | “Adgang udløber snart”                 |
| **Notification center** | Samlet historik/indbakke for hændelser | “3 invitationer afventer”              |

En notification UX-guide fremhæver netop, at notification, alert, validation og indicator har forskellige roller: notification informerer om en systemhændelse, alert er højere urgency, validation bekræfter/korrigerer brugerens input, og indicator viser passivt en tilstand. [\[eleken.co\]](https://www.eleken.co/blog-posts/notification-ux)

## Designregel

> **Brug det mindst forstyrrende mønster, der stadig gør brugeren i stand til at forstå eller handle.**

***

# 24.2 Attention levels

Jeg vil definere fire niveauer.

## Level 0 — neutral status

Bruges til almindelig information.

Eksempler:

```text
Aktiv
Permanent
Preview klar
2 medlemmer
```

UI:

* muted badge
* almindelig metadata
* ingen stærk farve
* ingen toast
* ingen global notification

## Level 1 — lav opmærksomhed

Bruges til ting, der er relevante, men ikke kræver handling nu.

Eksempler:

```text
Invitation sendt
Thumbnail oprettes
Fil er midlertidig
```

UI:

* muted/accent badge
* object-level status
* eventuelt inline status
* ikke global alert

## Level 2 — middel opmærksomhed

Bruges når brugeren bør se informationen og måske handle.

Eksempler:

```text
Invitation afventer svar
Fil udløber snart
Mangler oplysninger
Upload processing tager længere tid
```

UI:

* accent badge
* inline status i relevant sektion
* attention åbner collapsible section som default
* evt. page-level status hvis det påvirker hele siden

## Level 3 — høj opmærksomhed

Bruges når noget kræver handling eller påvirker brugerens mulighed for at arbejde.

Eksempler:

```text
Upload fejlede
Du har ikke adgang
Betaling kræver handling
Fil kunne ikke hentes
Projektwarning der blokerer flow
```

UI:

* persistent inline alert eller banner
* tydelig tekst
* handlingsknap
* ikke kun toast
* ikke kun farve

## Designregel

> **Attention level afgør placering, farve, persistens og om der skal være handling.**

***

# 24.3 Hvad bør give opmærksomhed i MyGardenPlanner?

Ud fra dine noter og de tidligere guidelines vil jeg foreslå følgende.

## Invitation afventer

Dette er typisk **Level 2**.

```text
Invitation afventer svar
kunde@example.dk er inviteret som læser.
```

UI:

* badge på invitationen
* summary card count
* medlemmer-kort åbent hvis afventende invitation er relevant
* ikke global alert medmindre invitationen er central for siden

## Fil udløber snart

Dette er **Level 2**, evt. **Level 3** hvis udløb betyder datatab.

```text
Filen udløber snart
Gør filen permanent, hvis den skal bevares.
```

UI:

* badge på filkort
* inline status i filsektion
* action: `Gør permanent`

## Upload fejlede

Dette er **Level 3**.

```text
Upload mislykkedes
Filen er for stor. Vælg en mindre fil.
```

UI:

* persistent inline status
* action: `Vælg anden fil`
* ikke kun toast

## Manglende oplysninger

Dette er **Level 2** eller **Level 3**, afhængigt af om det blokerer.

```text
Mangler oplysninger
Tilføj lokation for at kunne færdiggøre haveoverblikket.
```

UI:

* inline warning
* link/action til sektion
* ikke nødvendigvis global banner

## Adgang begrænset

Dette er typisk **Level 3**, fordi det påvirker brugerens adgang.

```text
Du har ikke adgang til filerne
Kontakt haveejeren, hvis du skal kunne se dem.
```

UI:

* no-access state
* persistent inline
* ikke toast

## Betaling / entitlement status

Dette kan være **Level 3**, hvis det påvirker adgang eller funktioner.

```text
Abonnement kræver handling
Opdater betaling for at fortsætte med at bruge delte haver.
```

UI:

* page/global banner
* tydelig action
* bør ikke være skjult i sidekolonne

Designnotatet nævner netop disse som relevante attention cases: invitation afventer, fil udløber snart, upload fejlede, projektwarning, manglende oplysninger, adgang begrænset og betaling/entitlement status.

***

# 24.4 Attention skal være kontekstuel

Det vigtigste valg er: **hvor skal beskeden vises?**

## Object-level

Når beskeden hører til ét objekt.

```text
Referencefoto-bed.jpg
[Thumbnail oprettes]
```

## Section-level

Når beskeden hører til en sektion.

```text
Filer
Upload mislykkedes
```

## Page-level

Når beskeden påvirker hele detail page.

```text
Denne have er arkiveret
```

## Global/banner

Når beskeden påvirker hele appen eller kontoen.

```text
Betaling kræver handling
```

## Designregel

> **Placér attention så tæt på årsagen som muligt. Brug global attention kun til globale problemer.**

***

# 24.5 Attention og farver

Vi skal være meget sparsomme med farveintensitet.

## Forslag

| Level   | Farvebrug                  |
| ------- | -------------------------- |
| Level 0 | muted/neutral              |
| Level 1 | muted eller primary-soft   |
| Level 2 | accent/warning             |
| Level 3 | danger eller stærk warning |

Men farve må aldrig stå alene — det har vi allerede besluttet i Accessibility baseline.

## Designregel

> **Orange og rød skal reserveres til reel opmærksomhed. Hvis alt er orange, mister orange sin betydning.**

***

# 24.6 Badges som attention

Badges er gode, men kan hurtigt støje.

## Brug badges til

```text
Afventer
Udløber snart
Upload fejlede
Begrænset adgang
Arkiveret
Midlertidig
```

## Undgå badges til alt

Dårligt:

```text
[PDF] [2,4 MB] [Oprettet] [Aktiv] [Normal] [Klar] [Downloadbar]
```

Det bliver visuelt støj.

## Designregel

> **Badges skal bruges til status og klassifikation, ikke til almindelig metadata.**

***

# 24.7 Attention i summary cards

Summary cards kan bruges til at synliggøre relevant attention uden at overdramatisere.

Eksempel:

```text
1
Invitation afventer
```

eller:

```text
2
Filer kræver handling
```

Men summary cards må ikke blive et dashboard af røde alarmer.

## Designregel

> **Summary cards må gerne vise attention counts, men kun for forhold brugeren kan handle på.**

***

# 24.8 Attention og collapsible sections

Fra Demo19b har vi et godt mønster:

* sektioner uden attention kan være collapsed
* sektioner med attention kan være åbne som default
* collapsed header skal vise summary

Eksempel:

```text
Medlemmer
2 aktive · 1 invitation afventer
```

Hvis collapsed:

```text
Medlemmer · 2 aktive · 1 invitation afventer
```

Hvis der er attention, kan sektionen åbnes automatisk.

## Designregel

> **Attention state kan styre default open/closed state, men må ikke skjules i collapsed content.**

***

# 24.9 Notifications må ikke skabe fatigue

Smashing Magazine beskriver, at høj frekvens af notifikationer kan skabe disruption og “notification fatigue”, hvor brugere ender med at afvise beskeder automatisk.

For MyGardenPlanner betyder det:

* ingen global notification for hver lille status
* ingen toast for hver autosave
* ingen orange badge på alt der bare er “ikke færdigt”
* ingen gentagne warnings for samme problem
* group/summary frem for mange ens beskeder

## Designregel

> **Gentagne eller beslægtede attention states bør samles, ikke gentages som mange separate beskeder.**

Eksempel:

```text
3 filer kræver handling
```

er bedre end:

```text
Fil A udløber snart
Fil B udløber snart
Fil C udløber snart
```

som tre stærke banners.

***

# 24.10 Actionability

Attention bør være handlingsorienteret, når handling er mulig.

## Dårligt

```text
Der er et problem.
```

## Bedre

```text
Upload mislykkedes
Filen er for stor. Vælg en mindre fil.

[Vælg anden fil]
```

Toptal fremhæver, at notification design bør tage højde for, om brugerhandling er nødvendig som følge af informationen.

## Designregel

> **Hvis en attention state kræver handling, skal UI’et tilbyde næste meningsfulde handling.**

***

# 24.11 Tone of voice

MyGardenPlanner skal være rolig og forklarende.

## Brug

```text
Filen udløber snart
Gør filen permanent, hvis den skal bevares.
```

```text
Invitation afventer svar
Du kan tilbagekalde invitationen, hvis den ikke længere skal bruges.
```

```text
Upload mislykkedes
Filen er for stor. Vælg en mindre fil.
```

## Undgå

```text
ADVARSEL!
KRITISK!
FEJL!
Handling påkrævet!!!
```

medmindre det faktisk er kritisk.

## Designregel

> **Attention copy skal være rolig, konkret og løsningsorienteret.**

***

# 24.12 Global notification center?

Jeg ville **ikke** starte med et fuldt notification center i første version, medmindre der er et tydeligt behov.

## Start med

* badges
* inline status
* summary counts
* section-level warnings
* evt. global banner for entitlement/payment

## Senere kan man overveje

```text
Notifications
- Invitation afventer
- Fil udløber snart
- Upload fejlede
```

Men først når der er nok hændelser til at retfærdiggøre en samlet indbakke.

## Designregel

> **Start uden notification center. Indfør det først, hvis brugeren har brug for historik eller samlet overblik over hændelser.**

***

# 24.13 Attention og navigation

Navigation kan vise attention, men meget forsigtigt.

## Godt

```text
Filer · 1
Medlemmer · 1
```

eller en lille badge:

```text
Filer [1]
```

## Dårligt

* røde prikker overalt
* badges uden forklaring
* navigation der føles som en alarmcentral

## Designregel

> **Navigation må vise attention counts, men ikke overtage brugerens fokus uden grund.**

***

# 24.14 Attention og permissions

Permissions er en særlig attention-kategori.

Hvis brugeren ikke har adgang, skal det ikke bare vises som en generisk advarsel.

```text
Du har ikke adgang til filerne
Din rolle giver ikke adgang til at se eller downloade filer i denne have.
```

No-access er en tilstand, ikke nødvendigvis en fejl.

## Designregel

> **Adgangsbegrænsning skal forklares som no-access state, ikke som dramatisk error.**

***

# 24.15 Attention og loading/processing

Processing states skal være informative, men rolige.

## Eksempler

```text
Thumbnail oprettes…
```

```text
Uploader fil…
62% gennemført
```

```text
Preview er under behandling
```

Disse er typisk Level 1 eller Level 2 — ikke error.

## Designregel

> **Processing skal vise at systemet arbejder, uden at ligne en fejl.**

***

# 24.16 Attention og destructive actions

Efter destruktive handlinger skal feedback være tydelig, men ikke nødvendigvis dramatisk.

```text
Filen er slettet
Haveskitse maj.pdf blev fjernet fra Villa Solbakken.
```

Dette skal være inline, ikke kun toast.

## Designregel

> **Destructive results skal bekræftes inline, men ikke permanent markeres som alarm.**

***

# 24.17 Attention matrix

| Situation                | Level | Pattern                                          |
| ------------------------ | ----: | ------------------------------------------------ |
| Aktiv have               |     0 | Badge/metadata                                   |
| Arkiveret have           |     1 | Badge + dæmpet state                             |
| Invitation afventer      |     2 | Badge + section attention                        |
| Fil udløber snart        |     2 | Badge + inline action                            |
| Thumbnail oprettes       |     1 | Object-level processing                          |
| Upload fejlede           |     3 | Inline error + action                            |
| No access                |     3 | No-access state                                  |
| Betaling kræver handling |     3 | Global/page banner                               |
| Manglende optional data  |     1 | Inline hint                                      |
| Manglende required data  |   2/3 | Inline warning + action                          |
| Link kopieret            |   0/1 | Toast                                            |
| Fil uploadet             |     1 | Inline success eller toast afhængigt af kontekst |

***

# 24.18 Komponenter/patterns

Jeg ville definere følgende:

## `AttentionBadge`

Til objektstatus:

```text
Afventer
Udløber snart
Begrænset adgang
```

## `StatusMessage`

Til inline feedback:

```text
success | info | warning | danger | restricted | processing
```

## `AttentionSummary`

Til summary cards eller section headers:

```text
1 invitation afventer
2 filer kræver handling
```

## `GlobalBanner`

Kun til globale forhold:

```text
Betaling kræver handling
```

## `NotificationList` senere

Kun hvis vi får behov for samlet hændelseshistorik.

***

# 24.19 Anbefalet designbeslutning

Jeg ville låse principperne sådan:

1. **Opmærksomhed skal være sparsom, forklarende og handlingsorienteret.**
2. **Ikke alle statusser er notifications.**
3. **Attention levels styrer placering, farve, persistens og handling.**
4. **Placér attention tæt på årsagen.**
5. **Global banners bruges kun til globale eller blokerende forhold.**
6. **Badges bruges til status og klassifikation, ikke almindelig metadata.**
7. **Orange/rød reserveres til reel opmærksomhed.**
8. **Attention må ikke skjules i collapsed sections.**
9. **Attention states med handling skal tilbyde næste naturlige handling.**
10. **Gentagne attention states bør samles i summary.**
11. **No-access er en tilstand, ikke en dramatisk fejl.**
12. **Start uden notification center; tilføj først hvis behovet opstår.**
13. **Notification/attention copy skal være rolig, konkret og løsningsorienteret.**

***

# Kort dokumentationstekst

Du kan bruge denne direkte:

> **Notifications og attention management:** Opmærksomhed i MyGardenPlanner skal være sparsom, forklarende og handlingsorienteret. Ikke alle statusser er notifications; almindelige tilstande vises som badges eller metadata, mens vigtige forhold vises som inline status eller banners afhængigt af urgency og kontekst. Attention skal placeres tæt på årsagen, bruge tekst frem for kun farve, og tilbyde næste naturlige handling, hvis handling er mulig. Orange og rød reserveres til reel opmærksomhed. Gentagne attention states samles i summary, og collapsed sections skal stadig vise status i headeren. Global notification center indføres først, hvis der opstår behov for samlet hændelseshistorik.