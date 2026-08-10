# 20. Detail page layout

## Overordnet princip

> **En detail page skal give brugeren roligt overblik over ét objekt, tydelige handlinger og adgang til relateret indhold uden at miste konteksten.**

Detail pages er der, hvor brugeren går fra “jeg leder efter noget” til “jeg arbejder med dette objekt”.

Det betyder, at siden skal være mere struktureret end en liste og mere kontekstuel end en formular.

***

## 20.1 Hvad er en detail page?

En detail page er siden for ét konkret objekt:

* én have
* ét bed
* én plante
* ét materiale
* én fil
* én butik
* ét medlem/invitation
* senere måske ét projekt eller én opgave

Eksempel:

```text
Villa Solbakken
Kundehave · Aarhus N · Aktiv

[Redigér] [Upload fil] [Print]
```

En detail page skal både kunne:

* vise objektets vigtigste data
* vise status
* vise relaterede data
* give relevante handlinger
* understøtte redigering
* understøtte print, hvor relevant
* bevare navigation state

***

# 20.2 Standard layoutstruktur

Jeg vil anbefale denne grundstruktur:

```text
1. Breadcrumb / tilbage til kontekst
2. Detail header
   - titel
   - metadata
   - badges/status
   - primary actions
3. Inline status / alerts
4. Context tabs
5. Summary cards
6. Main content
7. Related sections
   - filer
   - relationer
   - medlemmer
   - aktivitet/status
8. Danger zone / arkivering og sletning
```

Dette matcher retningen i dine designbeslutninger, hvor detail page layout netop skal samle title/header, metadata, actions, context tabs, summary cards, related lists, files, print og edit flow.

***

# 20.3 Breadcrumb / tilbage til kontekst

Før headeren bør der være en kontekstuel tilbagevej.

Eksempel:

```text
← Tilbage til haver
```

eller mere specifikt:

```text
← Tilbage til Villa Solbakken
```

For en plante åbnet fra et bed:

```text
← Tilbage til Staudehaven mod syd
```

## Designregel

> **Detail pages skal kende deres return context, når de åbnes fra en konkret arbejdskontekst.**

Dette hænger direkte sammen med navigation state-principperne.

***

# 20.4 Detail header

Headeren skal være sidens anker.

Den bør indeholde:

* titel
* undertitel/metadata
* status badges
* primary actions
* evt. sekundære actions

## Eksempel: havedetalje

```text
Villa Solbakken
Kundehave · Aarhus N · Senest opdateret i dag

[Aktiv] [2 medlemmer] [3 filer]

[Redigér] [Upload fil] [Print]
```

## Eksempel: plantedetalje

```text
Lavendel
Lavandula angustifolia

[Sol] [Staude] [30–60 cm]

[Redigér] [Tilføj til bed]
```

## Designregel

> **Headeren skal fortælle hvad objektet er, hvilken status det har, og hvad brugeren primært kan gøre.**

***

# 20.5 Primary actions

Primary actions skal være få og relevante.

## Gode primary actions

For have:

```text
Redigér
Upload fil
Print
```

For bed:

```text
Redigér bed
Tilføj plante
Tilføj materiale
```

For plante:

```text
Redigér plante
Tilføj til bed
```

For fil:

```text
Vis preview
Download
```

## Undgå

* for mange knapper i header
* destruktive handlinger som primære actions
* tekniske handlinger i topniveau
* actions brugeren ikke har adgang til

## Designregel

> **Header actions skal være de 1–3 mest sandsynlige næste handlinger. Destruktive handlinger placeres længere nede eller i menu/danger zone.**

***

# 20.6 Metadata og badges

Metadata skal give kontekst uden at dominere.

## Metadata

Eksempler:

* type
* lokation
* ejer
* status
* oprettet/senest opdateret
* relation
* dimensioner
* filtype
* størrelse

## Badges

Bruges til:

* Aktiv
* Arkiveret
* Midlertidig
* Begrænset adgang
* Sol / skygge
* Rolle
* Status

## Designregel

> **Metadata skal være scanbart og badges skal bruges til status/klassifikation — ikke som dekorativ støj.**

***

# 20.7 Context tabs

Detail pages kan have tabs, hvis objektet har flere tydelige underområder.

## Havedetalje tabs

```text
Overblik
Bede
Filer
Medlemmer
Aktivitet
Indstillinger
```

## Beddetalje tabs

```text
Overblik
Planter
Materialer
Filer
Noter
```

## Plantedetalje tabs

```text
Overblik
Anvendelse
Filer
Noter
```

## Designregel

> **Tabs bruges kun, når undersektionerne er meningsfulde og stabile.**

Tabs bør ikke bruges bare for at skjule kompleksitet. Hvis siden kun har lidt indhold, er almindelige sektioner bedre.

***

# 20.8 Tabs som URL-state

Aktiv tab bør gemmes i URL-state.

Eksempel:

```text
/haver/123?tab=filer
/haver/123?tab=bede
/bede/456?tab=planter
```

## Hvorfor?

* refresh bevarer tab
* return-flow fungerer
* links kan deles
* upload fra “Filer” kommer tilbage til “Filer”

## Designregel

> **Aktiv tab på detail pages bør være URL-state.**

***

# 20.9 Summary cards

Efter header/tabs bør detail pages ofte have summary cards.

## Eksempel: havedetalje

```text
6 bede
3 filer
2 medlemmer
1 invitation
```

## Eksempel: beddetalje

```text
12 planter
4 materialer
3,50 × 1,20 m
Sol
```

## Eksempel: filside

```text
PDF
2,4 MB
Permanent
Preview klar
```

## Designregel

> **Summary cards bruges til de 3–5 vigtigste nøgletal eller statusser.**

Ikke alle datafelter skal være summary cards.

***

# 20.10 Main content

Main content skal afhænge af objektet.

## Have

* beskrivelse
* beliggenhed
* struktur/overblik
* relaterede bede
* filer
* medlemmer

## Bed

* dimensioner
* lysforhold
* jordtype
* planteplan
* materialer
* filer

## Plante

* navn
* latinsk navn
* type
* lys/vanding/jord
* blomstring
* højde/bredde
* anvendelse i bede

## Materiale

* type
* mål
* farve
* leverandør/butik
* købsinfo
* anvendelse i projekter/bede

## Fil

* preview
* metadata
* tilknytning
* download/fuld størrelse
* permissions/status

## Designregel

> **Main content skal vise objektets kerneinformation først og relationer derefter.**

***

# 20.11 Related sections

Detail pages skal ofte vise relaterede data.

Eksempler:

## På havedetalje

* Bede
* Filer
* Medlemmer
* Invitationer
* Seneste aktivitet

## På beddetalje

* Planter i bed
* Materialer i bed
* Filer
* Noter

## På plantedetalje

* Bruges i disse bede
* Relaterede filer
* Noter

## På materialedetalje

* Bruges i projekter/bede
* Butikker/links
* Købsinfo

## Designregel

> **Relaterede sektioner skal vise nok til overblik og linke videre til fuld liste/detalje.**

Det vil sige: ikke nødvendigvis alle relaterede data på én detail page.

***

# 20.12 Filer på detail pages

Filer er vigtige nok til at have en fast sektion.

## Eksempel

```text
Filer
Tegninger, referencefotos og dokumenter knyttet til Villa Solbakken.

[Upload fil]

Haveskitse maj.pdf
Referencefoto-bed.jpg
```

Regler fra Demo9 gælder:

* metadata + thumbnail først
* preview ved aktiv handling
* original/download separat
* restricted state hvis adgang mangler
* processing state hvis thumbnail/preview oprettes

## Designregel

> **Filer på detail pages skal følge samme media-card/preview/download-principper som filoversigten.**

***

# 20.13 Status og feedback

Detail pages bør have plads til inline status.

Eksempler:

```text
Haven er arkiveret
Villa Solbakken findes nu under arkiverede haver.
```

```text
Filen er uploadet
Haveskitse maj.pdf er tilføjet til Villa Solbakken.
```

```text
Du har ikke adgang til filerne
Kontakt haveejeren, hvis du skal kunne se dem.
```

## Designregel

> **Status efter handlinger på detail pages skal vises inline tæt på den sektion, handlingen påvirker.**

***

# 20.14 Edit flow

Der er to hovedmuligheder:

## Inline/edit mode på samme side

Godt til:

* korte redigeringer
* status
* beskrivelse
* enkelte felter

## Separat edit page

Godt til:

* lange formularer
* kompleks validering
* oprettelse/redigering af centrale data
* mobil

Eksempel:

```text
/haver/123
→ Redigér
/haver/123/rediger?returnUrl=/haver/123?tab=overblik
```

## Designregel

> **Korte ændringer kan ske inline. Længere redigering bør være separat form page med return context.**

***

# 20.15 Collapsible cards

Jeg ville indføre et generelt mønster:

> **Sekundære, sjældent brugte eller lavfrekvente detail cards kan være collapsible. Statuskritiske eller ofte brugte cards bør som udgangspunkt være åbne.**

***

# Hvilke kort bør kunne foldes sammen?

## Medlemmer — ja, men med summary synlig

**Medlemmer** er et godt kandidatkort til collapsible, især hvis der kan være flere medlemmer eller invitationer.

Men headeren på kortet bør altid vise et kort overblik:

```text
Medlemmer
2 aktive · 1 invitation afventer
[Udvid]
```

Når collapsed:

```text
Medlemmer
2 aktive · 1 invitation afventer
```

Når expanded:

```text
Medlemmer
Mikael Sloth · Ejer
Anne Solbakken · Kunde
kunde@example.dk · Invitation afventer

[Administrér medlemmer]
```

## Designregel

> **Collapsede cards skal stadig vise nok summary til, at brugeren forstår status uden at åbne kortet.**

Så medlemmer må gerne være collapsed, men ikke bare som:

```text
Medlemmer
```

Det skal være:

```text
Medlemmer · 2 aktive · 1 invitation afventer
```

***

## Print — ja, næsten altid collapsed

**Print** er en sekundær handling. Den er vigtig, men ikke noget brugeren skal se hele tiden.

Jeg ville gøre print-kortet collapsed som standard:

```text
Print og eksport
Printvenligt haveoverblik
[Udvid]
```

Expanded:

```text
Print og eksport
Udskriv haveoverblik, bedliste eller filoversigt.

[Print haveoverblik]
[Print materialeliste]
```

Alternativt kan print slet ikke være et kort, men en sekundær action i header:

```text
[Print]
```

eller under en “Flere handlinger”-menu senere.

## Min vurdering

For MyGardenPlanner ville jeg enten:

1. have **Print** som sekundær header action, hvis print er vigtigt på siden  
   eller
2. have **Print og eksport** som collapsible card i sidekolonnen.

Men jeg ville ikke lade et stort printkort stå åbent permanent, hvis det kun indeholder én eller to handlinger.

***

## Danger zone — ja, men med forsigtighed

**Danger zone / Arkivering og sletning** er et oplagt collapsible card, fordi det netop ikke skal dominere siden.

Collapsed:

```text
Arkivering og sletning
Avancerede handlinger
[Udvid]
```

Expanded:

```text
Arkivering og sletning
Handlinger her kan skjule eller fjerne data.

[Arkivér have]
[Slet permanent]
```

Jeg ville faktisk anbefale, at danger zone er **collapsed som default** — især på detail pages, hvor brugeren oftest er der for at læse, redigere, tilføje filer eller navigere videre.

## Men vigtigt

Danger zone må ikke blive “skjult” på en måde, hvor brugeren ikke kan finde arkivering igen.

Så overskriften skal være tydelig:

Godt:

```text
Arkivering og sletning
```

Mindre godt:

```text
Flere
```

eller:

```text
Avanceret
```

Hvis “Arkivér have” er en forventet handling, skal brugeren kunne finde den uden gætteri.

## Designregel

> **Danger zone må gerne være collapsed, men labelen skal tydeligt fortælle, at arkivering/sletning findes der.**

***

## Hvilke kort bør normalt ikke foldes sammen?

## Metadata — typisk nej

Metadata i sidepanelet er ofte med til at forklare objektets kontekst:

```text
Ejer
Oprettet
Senest ændret
Adgang
Status
```

Jeg ville som udgangspunkt lade metadata være åbent, men holde det kort.

Hvis metadata bliver lang, kan man dele det:

```text
Metadata
- Ejer
- Oprettet
- Senest ændret

Flere oplysninger
[collapsed]
```

## Summary cards — nej

Summary cards bør ikke være collapsible. De er netop hurtige nøgletal.

```text
6 bede
3 filer
2 medlemmer
1 invitation
```

Hvis de foldes sammen, mister de deres formål.

## Statusbeskeder — nej

Inline statusbeskeder bør ikke collapse, især ikke hvis de forklarer:

* fejl
* no access
* uploadstatus
* destructive result
* invitation afventer
* oprettet objekt skjult af filter

Status skal være synlig, indtil den ikke længere er relevant.

***

# Default open/closed matrix

Jeg ville formulere default sådan:

| Kort/sektion             |                                    Default | Begrundelse                    |
| ------------------------ | -----------------------------------------: | ------------------------------ |
| Metadata                 |                                       Åben | Giver kontekst                 |
| Summary cards            |                                       Åben | Hurtigt overblik               |
| Primære related sections |                                       Åben | Bede/filer er centrale         |
| Medlemmer                |                       Åben eller collapsed | Afhænger af rolle og vigtighed |
| Invitationer             | Åben hvis noget afventer, ellers collapsed | Status kan være vigtig         |
| Print                    |                                  Collapsed | Sekundær handling              |
| Danger zone              |                                  Collapsed | Skal findes, men ikke dominere |
| Aktivitet/log            |                                  Collapsed | Ofte sekundært                 |
| Avancerede metadata      |                                  Collapsed | Undgår støj                    |
| Status/error/no access   |                                       Åben | Må ikke skjules                |

***

# Collapsible card guidelines

Jeg ville definere et component pattern:

```text
CollapsibleSectionCard
```

Med følgende regler:

## 1. Headeren skal være informativ

Ikke bare:

```text
Medlemmer
```

Men:

```text
Medlemmer · 2 aktive · 1 invitation
```

## 2. Collapsed state skal vise status

Eksempel:

```text
Filer · 3 filer · 1 processing
```

```text
Arkivering og sletning · Avancerede handlinger
```

## 3. Kritiske badges må være synlige i collapsed state

Eksempel:

```text
Medlemmer
[1 invitation afventer]
```

## 4. Brugerens valg kan huskes

Hvis brugeren åbner/lukker et kort, kan det gemmes i:

* local UI-state
* session state
* evt. local storage for præference

Men jeg ville ikke nødvendigvis lægge collapsible sidekort i URL’en, medmindre de repræsenterer egentlig navigation.

## 5. Brug ikke collapse til at skjule nødvendige handlinger

Hvis handlingen er primær for sidens formål, skal den ikke gemmes væk.

***

## Dynamisk default

Det kunne være en god regel:

> **Cards med attention state åbnes som default. Cards uden attention state kan være collapsed.**

Eksempel:

* Medlemmer med afventende invitation → åben
* Medlemmer uden særlige statusser → collapsed
* Print → collapsed
* Danger zone → collapsed
* Upload processing → åben
* No access → åben
* Error → åben

***

# Min anbefalede beslutning

Jeg ville låse princippet sådan:

> **Sidekolonnens sekundære cards må gerne være collapsible for at reducere visuel støj. Metadata og aktuelle statusbeskeder bør normalt være åbne. Print og danger zone bør som udgangspunkt være collapsed. Medlemmer kan være collapsed, men skal vise summary og åbnes automatisk, hvis der er noget, brugeren bør reagere på — fx en afventende invitation. Collapsed headers skal altid vise nok information til, at brugeren kan forstå status uden at åbne kortet.**

# 20.16 Print

Nogle detail pages skal kunne printes.

Ifølge det oprindelige designoplæg skal visse sider kunne printes, og vi har tidligere arbejdet med printprincipper. Detail pages bør derfor have en tydelig printstrategi, når indholdet har dokumentationsværdi.

## Print-egnede detail pages

* Haveoverblik
* Bedoverblik
* Materialeliste
* Planteliste
* Fil-/dokumentoversigt
* Kundeoverblik

## Designregel

> **Detail pages med dokumentationsværdi skal have printvenlig struktur, hvor navigation/actions skjules og indhold vises som dokument.**

***

# 20.17 Danger zone

Arkivering/sletning bør ikke ligge som primær handling i header.

Placér i:

```text
Arkivering og sletning
[Arkivér have]
[Slet permanent]
```

## Designregel

> **Destruktive handlinger på detail pages placeres i separat sektion, ikke i headerens primary actions.**

***

# 20.18 Permissions

Detail page layout skal kunne variere efter rolle.

## Owner/editor

* kan se actions
* kan redigere
* kan administrere relevante sektioner

## Viewer

* read-only layout
* ingen admin/destructive actions
* evt. disabled expected actions med forklaring

## Restricted

* no-access state
* evt. metadata hvis tilladt

## Designregel

> **Detail pages skal have read-only og restricted varianter, ikke kun edit-varianter.**

***

# 20.19 Loading

Detail pages bør loade i lag.

## Staged loading

1. Header/title
2. Metadata
3. Summary cards
4. Main content
5. Related files/thumbnails
6. Activity/secondary data

## Designregel

> **Detail pages skal vise primær kontekst først og loade tunge relaterede sektioner lokalt.**

Dette hænger sammen med Demo11.

***

# 20.20 Mobile layout

På mobil bør detail page blive mere sekventiel:

```text
Title
Metadata
Primary actions
Tabs/dropdown
Summary cards
Content sections
Related lists
Danger zone
```

## Mobile guidelines

* actions fuld bredde
* tabs kan blive horisontalt scroll eller dropdown
* summary cards i én kolonne
* tables bliver stacked rows
* preview kan blive full-screen
* sticky header skal ikke fylde for meget

## Designregel

> **Detail page på mobil skal prioritere læsning og handling i én kolonne.**

***

# 20.21 Standard detail page skeleton

Jeg ville definere en standard Blazor-komponentstruktur cirka sådan:

```text
DetailPage
  Breadcrumb / ReturnLink
  DetailHeader
    Title
    Subtitle/Metadata
    Badges
    PrimaryActions
  StatusSlot
  ContextTabs
  SummaryGrid
  MainContent
  RelatedSections
  DangerZone
```

Og underkomponenter:

```text
DetailHeader
SummaryCard
ContextTabs
RelatedListSection
FilesSection
ReadOnlySection
DangerZone
StatusMessage
```

***

# 20.22 Default layout pr. objekt

## Havedetalje

```text
Return: Tilbage til haver
Header: Villa Solbakken + metadata + actions
Tabs: Overblik / Bede / Filer / Medlemmer / Aktivitet / Indstillinger
Summary: Bede, filer, medlemmer, invitationer
Main: Beskrivelse og struktur
Related: Bede, filer
Danger: Arkivering/sletning
```

## Beddetalje

```text
Return: Tilbage til haven
Header: Staudehaven mod syd + metadata
Tabs: Overblik / Planter / Materialer / Filer / Noter
Summary: Planter, materialer, mål, lys
Main: Dimensioner og beskrivelse
Related: Planter, materialer, filer
Danger: Arkivér bed
```

## Plantedetalje

```text
Return: Tilbage til plantelisten eller bed
Header: Lavendel + latinsk navn
Tabs: Overblik / Anvendelse / Filer / Noter
Summary: Lys, type, højde, blomstring
Main: Dyrkningsdata
Related: Bruges i bede
Actions: Redigér / Tilføj til bed
```

## Materialedetalje

```text
Return: Tilbage til materialer
Header: Materialenavn + type
Tabs: Overblik / Køb / Anvendelse / Filer
Summary: Type, mål, farve, pris
Main: Beskrivelse, dimensioner, leverandør
Related: Bruges i projekter/bede
```

## Fildetalje

```text
Return: Tilbage til filer eller objekt
Header: Filnavn + type/status
Main: Preview
Sidebar/metadata: størrelse, tilknytning, status
Actions: Åbn fuld størrelse / Download
Restricted/processing states efter behov
```

***

# 20.23 Beslutningstabel

| Element           | Formål                       | Regel                                |
| ----------------- | ---------------------------- | ------------------------------------ |
| Breadcrumb/return | Bevare kontekst              | Specifik “Tilbage til …”             |
| Header            | Identitet og primær handling | Titel, metadata, status, 1–3 actions |
| Badges            | Status/klassifikation        | Brug sparsomt                        |
| Tabs              | Underområder                 | Kun når sektioner er stabile         |
| Summary cards     | Nøgletal/status              | 3–5 vigtigste                        |
| Main content      | Kerneinformation             | Før relationer                       |
| Related sections  | Overblik over relationer     | Vis nok, link videre                 |
| Files section     | Dokumentation                | Følg media-principper                |
| Status slot       | Feedback                     | Inline, tæt på årsag                 |
| Danger zone       | Destruktive handlinger       | Adskilt fra header                   |
| Print             | Dokumentation                | Skjul navigation/actions             |
| Mobile            | Læsning og handling          | Én kolonne                           |

***

# Anbefalet designbeslutning

Jeg ville låse principperne sådan:

1. **Detail pages skal give overblik over ét objekt og dets relationer.**
2. **Headeren skal indeholde titel, metadata, status og få primary actions.**
3. **Destruktive handlinger må ikke være primary actions i header.**
4. **Context tabs bruges kun til stabile, meningsfulde undersektioner.**
5. **Aktiv tab bør være URL-state.**
6. **Summary cards bruges til 3–5 vigtigste nøgletal/statusser.**
7. **Main content viser kerneinformation før relationer.**
8. **Relaterede sektioner viser overblik og linker videre.**
9. **Filer på detail pages følger media/preview/download-principperne.**
10. **Read-only og restricted states skal være designet ind fra starten.**
11. **Status efter handlinger vises inline tæt på den relevante sektion.**
12. **Detail pages loader i lag, med primær kontekst først.**
13. **Printvenlige detail pages skal skjule navigation/actions og præsentere indhold som dokument.**
14. **Mobil detail layout er én kolonne med tydelige actions.**

***

# Kort dokumentationstekst

Du kan bruge denne direkte:

> **Detail page layout:** En detail page viser ét objekt og skal give tydeligt overblik over identitet, status, primære handlinger og relaterede data. Standardstrukturen er: return link, detail header med titel/metadata/badges/actions, inline status, context tabs, summary cards, main content, related sections, files section og danger zone. Tabs bruges kun til stabile undersektioner og bør være URL-state. Destruktive handlinger placeres adskilt fra headerens primary actions. Detail pages skal understøtte read-only/restricted states, staged loading, printvenlig visning og mobil layout i én kolonne.