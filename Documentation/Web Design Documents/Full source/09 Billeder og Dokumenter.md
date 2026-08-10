# 9. Billeder og dokumenter

Mit take er, at vi bør skelne meget tydeligt mellem:

1. **Upload**
2. **Thumbnail / preview**
3. **Fuld visning**
4. **Download**
5. **Rettigheder / adgang**
6. **Performance**
7. **Livscyklus / midlertidig vs. permanent**

Det er vigtigt, fordi billeder og dokumenter hurtigt kan gøre en ellers rolig app tung — både visuelt og teknisk.

***

## Grundprincip

Jeg ville formulere hovedreglen sådan:

> **Brugeren skal kunne se nok til at forstå filen hurtigt — men appen skal først hente tungt indhold, når brugeren aktivt beder om det.**

Det betyder:

* lister og grids viser **metadata + thumbnail**
* preview åbnes kun ved bevidst klik
* fuld størrelse åbnes kun ved eksplicit handling
* download er en separat handling
* store filer må aldrig hentes implicit i en liste
* billed- og dokumentvisning skal respektere adgang og filens status

Det passer også med det, du allerede har lagt op til: filmodellen har metadata, binære data og thumbnails, og oplægget nævner hurtig load samt at billeder skal håndteres i rigtige størrelser via controllere og thumbnail-service med tre størrelser. citeDesignLayer1.txtDesign oplæg for sider til layer 1.md

***

# 9.1 Upload-principper

Upload skal være en **kontrolleret arbejdshandling**, ikke bare et teknisk fil-input.

## Upload UI bør vise

* hvad brugeren kan uploade
* hvor filen bliver tilknyttet
* tilladte filtyper
* eventuelle begrænsninger
* uploadstatus
* fejlbeskeder
* mulighed for at annullere/fjerne fil
* efter upload: filkort med thumbnail/ikon

Eksempel på princip:

```text
Upload tegning eller dokument
PDF, JPG eller PNG. Thumbnail oprettes automatisk.
[Tilknyt til: Villa Solbakken]
[Vælg fil]
```

Det matcher vores upload-zone fra formularpunktet.

## Upload bør have tydelige states

| State      | UI                                      |
| ---------- | --------------------------------------- |
| Empty      | Upload-zone med instruktion             |
| Selected   | Filnavn, filtype, størrelse, fjern-knap |
| Uploading  | Progress/status                         |
| Success    | Status message + media card             |
| Error      | Fejlbesked + retry                      |
| Restricted | Forklaring på manglende rettighed       |

For tilgængelighed bør uploadfeltet have tydelige labels, instruktioner, filtypebegrænsninger og statusfeedback. Kilden om accessible file uploads fremhæver netop labels, `aria-describedby`, klare instruktioner, filrestriktioner og upload-progress/statusfeedback som vigtige elementer. [\[blog.filestack.com\]](https://blog.filestack.com/html-file-upload-accessibility/)

***

# 9.2 Upload skal ikke være “drag-and-drop only”

Drag-and-drop er fint som ekstra mulighed, men ikke som eneste mulighed.

## Designregel

> Upload-zone må gerne understøtte drag-and-drop, men der skal altid være en tydelig “Vælg fil”-knap.

Årsager:

* fungerer bedre på mobil
* fungerer bedre med tastatur
* er mere forståeligt
* er mere robust
* er mere tilgængeligt

Dårligt:

```text
Slip filer her
```

Bedre:

```text
Upload tegning eller dokument
PDF, JPG eller PNG.
[Vælg fil]
Du kan også trække filen hertil.
```

***

# 9.3 Thumbnail vs. preview vs. fuld størrelse

Jeg vil definere tre visningsniveauer.

## 1. Thumbnail

Bruges i:

* filgrids
* media cards
* listevisninger
* relaterede filer i have/bed/plante

Formål:

* hurtigt visuelt genkendelse
* lav vægt
* hurtig load
* må gerne være beskåret/kompakt

## 2. Preview

Bruges når brugeren klikker “Vis” eller “Preview”.

Formål:

* større, men stadig kontrolleret visning
* god nok til at vurdere indhold
* bør ikke nødvendigvis hente originalfilen
* kan vises i modal/sidepanel/detailvisning

## 3. Fuld størrelse / original

Bruges når brugeren eksplicit vælger:

* “Åbn fuld størrelse”
* “Download”
* “Åbn original”

Formål:

* maksimal kvalitet
* tung ressource
* må kræve aktiv handling

## Designregel

> Thumbnail er standard. Preview er aktiv visning. Original hentes kun ved eksplicit handling.

Dette er især vigtigt for performance. Lazy loading kan reducere initial belastning ved at udskyde hentning af billeder, indtil de nærmer sig eller vises i viewport. [\[sitepoint.com\]](https://www.sitepoint.com/five-techniques-lazy-load-images-website-performance/)

***

# 9.4 Skal alle thumbnails kunne klikkes?

Mit svar: **nej, ikke automatisk**.

Det afhænger af kontekst.

## Klikbart thumbnail giver mening når

* kortet repræsenterer én fil
* klik åbner preview
* brugeren tydeligt forventer preview
* der ikke er andre konkurrerende handlinger
* brugeren har rettighed til at se filen

Eksempel:

```text
[thumbnail] Haveskitse maj.pdf
Klik på kort/thumbnail → preview
Download-knap → download
```

## Klikbart thumbnail bør undgås når

* brugeren ikke har adgang til preview
* filen er for stor/kræver særskilt handling
* filen potentielt er følsom
* kortet har mange handlinger
* thumbnail blot er dekorativ
* klik kan forveksles med selection
* billedet ligger i et multiselect-grid

## Min anbefaling

Vi bør have tre varianter:

| Variant                   | Klikadfærd                              |
| ------------------------- | --------------------------------------- |
| `media-card-previewable`  | Klik på thumbnail/kort åbner preview    |
| `media-card-actions-only` | Kun knapper er klikbare                 |
| `media-card-restricted`   | Preview/download vises kun hvis tilladt |

Det giver bedre kontrol end at gøre alt klikbart.

***

# 9.5 Preview modes

Jeg foreslår disse preview modes.

## Inline preview

Bruges til små, ufarlige previews:

* thumbnail-grid
* lille billedvisning
* filkort med større preview

Godt til:

* billeder
* små skitser
* simple dokumentikoner

## Sidepanel preview

God til arbejdsflow:

* brugeren bliver på listen
* preview åbner i højre panel
* metadata og handlinger vises samtidig

Velegnet til:

* billeder
* PDF-forsider
* tegninger
* dokumenter

## Modal preview

God til fokuseret visning:

* større billede/PDF-preview
* mørkere baggrund
* tydelige handlinger

Ulempe:

* kan afbryde workflow
* kræver god keyboard/focus-håndtering

## Full page viewer

God til:

* store PDF’er
* store tegninger
* dokumenter der kræver zoom/pan
* print/eksport

## Anbefaling

For MyGardenPlanner ville jeg vælge:

```text
Lister/grids → thumbnails
Klik “Vis” → preview sidepanel eller modal
Klik “Åbn fuld størrelse” → full page viewer/original
Klik “Download” → download original
```

***

# 9.6 Download-principper

Download skal være separat fra preview.

## Hvorfor?

Preview og download er forskellige brugerintentioner:

* Preview = “jeg vil se hvad det er”
* Download = “jeg vil have filen lokalt”

Hvis klik på thumbnail downloader en fil, bliver det hurtigt utrygt.

## Designregel

> Download må aldrig være implicit primærklik på thumbnail. Download skal være en tydelig knap eller menu-handling.

Eksempel:



***

# 9.7 Rettigheder og restriktioner

Ja, det giver mening at have restriktioner.

Ikke alle brugere bør nødvendigvis kunne:

* se preview
* downloade original
* slette fil
* gøre midlertidig fil permanent
* dele fil
* åbne fuld størrelse

## UI bør skelne mellem

| Situation                         | UI                                             |
| --------------------------------- | ---------------------------------------------- |
| Kan se preview, men ikke download | Vis preview-knap, skjul/deaktivér download     |
| Kan se metadata, men ikke indhold | Vis låst card med forklaring                   |
| Fil er midlertidig og udløber     | Vis statusbadge                                |
| Fil mangler thumbnail             | Vis filtype-fallback                           |
| Fil er for stor til preview       | Vis metadata + “Download” eller “Åbn original” |
| Filtype kan ikke previewes        | Vis ikon + download                            |
| Fil er under behandling           | Vis “Thumbnail oprettes…”                      |

## Vigtig designregel

> Skjul handlinger, brugeren aldrig kan udføre. Deaktivér kun handlinger, hvis brugeren kan forstå hvorfor.

Eksempel:

```text
Download
Ikke tilgængelig for din rolle
```

kan være nyttigt, hvis rettigheden er relevant at forklare.

***

# 9.8 Performance-principper

Her vil jeg være ret konsekvent.

## 1. Lister må kun hente thumbnails og metadata

Aldrig originalfil.

```text
Filnavn
Content type
Oprettet
Status
Thumbnail small
```

## 2. Brug de tre thumbnail-størrelser strategisk

Da du allerede har en thumbnail-service med tre størrelser, bør UI definere hvor de bruges:

| Størrelse | Brug                                     |
| --------- | ---------------------------------------- |
| Small     | compact list, små media cards            |
| Medium    | thumbnail grid, standard cards           |
| Large     | preview, sidepanel, større billedvisning |

Det er en UI-kontrakt mellem frontend og controller/service.

## 3. Lazy load billeder uden for viewport

For billedtunge sider bør thumbnails uden for viewport lazy loades. Native lazy loading kan implementeres med `loading="lazy"` på billeder, og lazy loading betyder at billeder først hentes, når de sandsynligvis skal bruges. [\[sitepoint.com\]](https://www.sitepoint.com/five-techniques-lazy-load-images-website-performance/)

## 4. Reserver billedplads

Så layoutet ikke hopper.



## 5. Brug fallback-visning

Hvis thumbnail mangler:

```text
PDF
DOC
IMG
...
```

## 6. Original hentes kun ved eksplicit handling

Det er især vigtigt, fordi dine filer kan have binær `FilData`, mens thumbnails er afledte data med størrelse, bredde, højde og content type. citeDesignLayer1.txt

***

# 9.9 Filkortets information hierarchy

Et godt media card bør vise:

1. Thumbnail / filtypeikon
2. Filnavn
3. Type
4. Tilknytning
5. Status
6. Handlinger

Eksempel:

```text
[PDF]
Haveskitse maj.pdf
PDF · Tegning · Villa Solbakken

[Midlertidig] [Udløber snart]

[Vis] [Download] [Slet]
```

## Brug badges til

* Midlertidig
* Permanent
* Udløber snart
* Thumbnail oprettes
* Preview ikke muligt
* Låst
* Stor fil

***

# 9.10 UI-varianter

Jeg foreslår disse komponenter:

## `UploadZone`

Til upload.

* tom state
* selected state
* uploading
* success/error

## `MediaCard`

Standard kort med thumbnail og handlinger.

## `MediaCompactRow`

Til lange lister.

## `ThumbnailGrid`

Til billed-/filoversigter.

## `PreviewPanel`

Sidepanel eller modal.

## `FullViewer`

Større visning til PDF/billede.

## `FileStatusMessage`

Statusbesked for upload/processing/restrictions.

***

# 9.11 Preview UI

Preview bør indeholde:

* titel/filnavn
* content type
* tilknytning
* previewområde
* metadata
* handlinger
* luk/tilbage
* evt. “åbn fuld størrelse”

Eksempel:

```text
Haveskitse maj.pdf
PDF · Tegning · Villa Solbakken

[preview canvas / image / PDF first page]

[Åbn fuld størrelse] [Download] [Luk]
```

## Hvis preview ikke understøttes

```text
Preview ikke muligt
Denne filtype kan ikke vises i appen.
[Download]
```

## Hvis filen er for stor

```text
Preview er begrænset
Originalfilen er stor. Du kan åbne fuld størrelse eller downloade den.
[Åbn fuld størrelse] [Download]
```

***

# 9.12 Preview vs. download — beslutningstabel

| Filtype        | Thumbnail                      | Preview                        | Fuld størrelse   | Download |
| -------------- | ------------------------------ | ------------------------------ | ---------------- | -------- |
| JPG/PNG/WebP   | Ja                             | Ja                             | Ja               | Ja       |
| PDF            | Ja, hvis thumbnail findes      | Ja/sidepanel hvis understøttet | Ja               | Ja       |
| DOCX/XLSX      | Ikon eller genereret thumbnail | Måske nej                      | Åbn/download     | Ja       |
| Ukendt fil     | Ikon                           | Nej                            | Nej/kun download | Ja       |
| Meget stor fil | Small/medium thumb             | Begrænset                      | Eksplicit        | Ja       |
| Restricted     | Måske metadata                 | Nej                            | Nej              | Nej      |

Jeg ville ikke love preview for alle dokumenttyper. UI’et bør kunne vise “Preview ikke muligt” uden at det føles som en fejl.

***

# 9.13 Midlertidige filer og lifetime

Din model har `AttachmentLifetime` samt `ExpiresAt`, så UI bør gøre det tydeligt, hvis en fil er midlertidig eller udløber. citeDesignLayer1.txt

## UI-princip

Midlertidig fil skal ikke skjules som en teknisk detalje.

Eksempel:

```text
[Midlertidig] Udløber 25. juni
[Gør permanent]
```

## Handling

Hvis brugeren har rettighed:

```text
Gør permanent
```

Hvis ikke:

```text
Midlertidig fil
Kontakt haveejeren for at gøre filen permanent.
```

***

# 9.14 Accessibility

For upload, preview og downloads skal vi være ekstra opmærksomme.

## Upload

* rigtigt label
* instruktioner koblet via `aria-describedby`
* tastaturbrug
* statusbeskeder
* fejlbeskeder
* ikke drag-and-drop som eneste input

Accessible upload-kilden fremhæver netop labels, `aria-describedby`, klare instruktioner, filbegrænsninger, progress/status feedback, keyboard navigation og test med skærmlæsere. [\[blog.filestack.com\]](https://blog.filestack.com/html-file-upload-accessibility/)

## Thumbnail

* `alt` skal beskrive indhold, hvis billedet er informativt
* dekorative thumbnails kan have tom alt
* filtypeikoner skal have tekst i UI’et

## Preview

* modal/sidepanel skal have fokusstyring
* Esc/luk
* tydeligt navn
* keyboard navigation

## Download

* knaptekst skal være konkret:
  * “Download PDF”
  * “Download original”
  * “Download tegning”
* ikke kun ikon

***

# 9.15 Sikkerhed og tillid

Selvom vi primært taler UI, bør designet støtte sikkerhed:

* vis filtype
* vis filnavn
* vis ejer/kontekst
* vis status
* undgå automatisk åbning/download
* bekræft sletning
* vis hvis filen er midlertidig
* vis hvis adgang er begrænset

## Sletning

Slet bør være outline danger og adskilt:

```text
[Vis] [Download] [Slet]
```

Ved bekræftelse:

```text
Slet fil?
Filen fjernes fra Villa Solbakken.
[Annullér] [Slet fil]
```

***

# 9.16 Min anbefalede beslutning

Jeg ville låse disse principper:

1. **Lister henter kun metadata og thumbnails.**
2. **Thumbnail er ikke det samme som original.**
3. **Preview kræver aktiv brugerhandling.**
4. **Download kræver separat eksplicit handling.**
5. **Ikke alle filer skal kunne previewes.**
6. **Ikke alle brugere skal nødvendigvis kunne downloade.**
7. **Restricted states skal være tydelige og rolige.**
8. **Midlertidige filer skal markeres tydeligt.**
9. **Store filer skal behandles som eksplicit handling.**
10. **Upload skal have klare states: empty, selected, uploading, success, error.**
11. **Drag-and-drop er supplement, ikke eneste metode.**
12. **Alt UI skal kunne fungere uden at hente originalfilen.**
13. **Thumbnail-størrelser skal bruges konsekvent: small/medium/large.**
14. **Preview-modal/sidepanel skal være keyboard- og screenreader-venlig.**
15. **Performance er et designkrav, ikke kun et teknisk krav.**