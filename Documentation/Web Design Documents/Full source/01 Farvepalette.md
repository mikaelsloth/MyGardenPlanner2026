# 1. Farvepalette

## Anbefalet designretning

Jeg anbefaler en palette med arbejdstitlen:

## **“Nordisk havearkitekt”**

Det vil sige:

* rolig
* naturlig
* professionel
* let og luftig
* ikke for “grøn app”-agtig
* ikke for dekorativ
* egnet til data, cards, formularer og print

Farverne skal ikke konkurrere med indholdet. I denne type app er indholdet typisk haver, plantevalg, materialer, dokumenter, tegninger, billeder og kunderelationer. Derfor bør UI’et være en neutral ramme omkring arbejdet — ikke et visuelt show i sig selv.

***

# Primær palette

Jeg foreslår denne palette som udgangspunkt:

| Rolle             |                                          Farve |       Hex | Brug                                                          |
| ----------------- | ---------------------------------------------: | --------: | ------------------------------------------------------------- |
| Primær grøn       | <https://placehold.co/18x18/3F6B4A/3F6B4A.png> | `#3F6B4A` | Primære knapper, aktiv navigation, links, fokusmarkering      |
| Mørk tekstgrøn    | <https://placehold.co/18x18/243128/243128.png> | `#243128` | Overskrifter, vigtig tekst, ikoner                            |
| Varm sand         | <https://placehold.co/18x18/FAF8F2/FAF8F2.png> | `#FAF8F2` | App-baggrund, rolige flader                                   |
| Lys salvie        | <https://placehold.co/18x18/DDE8D8/DDE8D8.png> | `#DDE8D8` | Sektioner, badges, lette highlights                           |
| Terracotta accent | <https://placehold.co/18x18/B86B4B/B86B4B.png> | `#B86B4B` | Sekundære handlinger, diskrete advarsler, vigtige markeringer |

Det holder sig inden for ønsket om maks. 3-5 farver og matcher kravet om have-tema, god kontrast og roligt visuelt udtryk. [\[onedrive.live.com\]](https://onedrive.live.com?cid=64031C85D39BC2D5&id=64031C85D39BC2D5!s6043866348544816bb7b2b0d5911f5c2)

***

# Hvorfor netop disse farver?

## 1. Primær grøn — `#3F6B4A`

Dette bør være appens identitetsfarve. Den er mørk nok til at virke professionel, men stadig tydeligt forbundet med have, planter og natur.

Brug den til:

* primære knapper
* aktiv menu i sidebar
* links
* fokusrammer
* små statusmarkeringer
* logo/brand-elementer

Eksempel:

```css
--color-primary: #3F6B4A;
```

Jeg vil undgå meget kraftige limegrønne eller græsgrønne farver, fordi de hurtigt kan få appen til at virke hobbyagtig eller “for frisk”. Her skal appen også kunne bruges af havearkitekter i en professionel kundekontekst. Målgruppen arbejder netop med kundehaver, dokumenter, tegninger, bede og planlægning.

***

## 2. Mørk tekstgrøn — `#243128`

Denne farve er til tekst og ikoner. Jeg vil bruge en mørk grøn-sort i stedet for ren sort, fordi den føles blødere og passer bedre til resten af paletten.

Brug den til:

* hovedoverskrifter
* brødtekst
* sidebar-ikoner
* labels
* korttitler

```css
--color-text: #243128;
```

Det hjælper med at holde udtrykket roligt, hvilket er et centralt mål i opgavebeskrivelsen.

***

## 3. Varm sand — `#FAF8F2`

Denne bør være den primære sidebaggrund.

Det er bedre end ren hvid, fordi appen ellers kan føles klinisk og teknisk. En varm sandfarve giver en mild, naturlig base og gør cards og formularfelter mere behagelige at læse på større skærme.

Brug den til:

* body background
* dashboard-baggrund
* tomme områder omkring cards
* printvenlige baggrunde, dog med justering i print-CSS

```css
--color-bg: #FAF8F2;
```

Da designet skal have god spacing og være let at overskue, hjælper en varm baggrund med at skabe rolige visuelle zoner uden at introducere mange farver.

***

## 4. Lys salvie — `#DDE8D8`

Dette er en blød støttefarve. Den skal ikke dominere, men bruges til at gruppere indhold.

Brug den til:

* badge backgrounds
* infofelter
* valgt card
* aktive filtre
* rolige sektionsbaggrunde
* “Aktuel have”-området i sidebar

```css
--color-primary-soft: #DDE8D8;
```

Den passer godt til cards og sektioner, hvilket er relevant, fordi opgavebeskrivelsen specifikt nævner, at cards gerne må bruges til at gruppere indhold.

***

## 5. Terracotta accent — `#B86B4B`

Denne skal bruges sparsomt. Den giver varme og menneskelighed uden at paletten bliver spraglet.

Brug den til:

* sekundære highlights
* advarselslignende, men ikke kritiske states
* “udløber snart”
* “midlertidig fil”
* små labels
* visuel kontrast på dashboardet

```css
--color-accent: #B86B4B;
```

Denne farve kan blandt andet bruges til filstatus, fordi modellen for vedhæftede filer har `Lifetime` og `ExpiresAt`, så UI’et sandsynligvis skal kunne vise midlertidige eller udløbende filer på en forståelig måde.

***

# Supplerende neutrale farver

Selvom hovedpaletten bør holdes på 5 farver, har vi brug for neutrale gråtoner til borders, disabled states og inputfelter. De tæller ikke som brandfarver, men som UI-neutrals.

```css
--color-surface: #FFFFFF;
--color-border: #D8D2C7;
--color-muted: #6F766D;
--color-muted-bg: #EFEAE0;
--color-danger: #9F3A38;
--color-warning-bg: #F8E8D8;
--color-success-bg: #E4EFE1;
```

Jeg vil dog holde “rigtige” brandfarver til de fem ovenfor. Ellers bliver designet hurtigt uklart.

***

# State-farver

Appen kommer til at have mange objekter: haver, bede, planter, materialer, invitationer, thumbnails og vedhæftede filer. Modellerne viser blandt andet arkivering på `Have` og `Bed`, invitationstatus på `HaveInvitation`, aktiv-status på `HaveMedlem` og fil-lifetime på `VedhaeftetFil`.  Derfor bør vi definere state-farver tidligt.

| State       | Farveforslag | Brug                                           |
| ----------- | -----------: | ---------------------------------------------- |
| Aktiv / OK  |    `#3F6B4A` | Aktiv have, aktivt medlem, gennemført handling |
| Neutral     |    `#6F766D` | Metadata, sekundær tekst                       |
| Afventer    |    `#B86B4B` | Invitation pending, fil udløber snart          |
| Arkiveret   |    `#8A8F86` | Arkiverede haver/bede                          |
| Fejl / slet |    `#9F3A38` | Destruktive handlinger                         |

Vigtigt: Jeg ville ikke bruge rød som almindelig accent. Rød bør reserveres til sletning, fejl eller kritiske advarsler.

***

# Praktisk CSS-forslag

Jeg vil samle paletten i en central CSS-fil, f.eks.:

```css
:root {
    /* Brand */
    --mgp-primary: #3F6B4A;
    --mgp-primary-dark: #2F5138;
    --mgp-primary-soft: #DDE8D8;
    --mgp-accent: #B86B4B;

    /* Text */
    --mgp-text: #243128;
    --mgp-text-muted: #6F766D;

    /* Surfaces */
    --mgp-bg: #FAF8F2;
    --mgp-surface: #FFFFFF;
    --mgp-surface-muted: #EFEAE0;
    --mgp-border: #D8D2C7;

    /* States */
    --mgp-danger: #9F3A38;
    --mgp-warning-bg: #F8E8D8;
    --mgp-success-bg: #E4EFE1;
}
```

Og derefter mappe det ind i Bootstrap-venlige klasser:

```css
.btn-primary {
    background-color: var(--mgp-primary);
    border-color: var(--mgp-primary);
}

.btn-primary:hover,
.btn-primary:focus {
    background-color: var(--mgp-primary-dark);
    border-color: var(--mgp-primary-dark);
}

.text-primary {
    color: var(--mgp-primary) !important;
}

.bg-app {
    background-color: var(--mgp-bg);
}

.bg-soft {
    background-color: var(--mgp-primary-soft);
}

.card {
    background-color: var(--mgp-surface);
    border-color: var(--mgp-border);
}
```

Det passer godt til ønsket om Bootstrap som udgangspunkt og central CSS frem for side-specifik styling.

***

# Hvordan farverne bør bruges på siderne

## Dashboard

```text
Baggrund: varm sand
Cards: hvid
Primære handlinger: grøn
Highlights: lys salvie
Små opmærksomhedspunkter: terracotta
```

Dashboardet skal fungere som centralt punkt, så det bør bruge farverne til at skabe overblik — ikke dekoration.

***

## Have-detaljeside

```text
Headerområde: hvid cardflade
Aktiv have-status: lys salvie
Primære actions: grøn
Bede/cards: hvid
Arkiverede elementer: dæmpet neutral
```

`Have` har relationer til adresse, kontakt, bede og medlemmer, så farverne bør hjælpe med at skelne mellem “overblik”, “struktur” og “handlinger”.

***

## Planteoversigt

```text
Plantenavn: mørk tekstgrøn
Latinsk navn: muted
Planteegenskaber: små salvie-badges
Blomstring/farve: diskret accent, ikke store farvefelter
```

`Plante` har både almindeligt navn, latinsk navn, højde, bredde, lysforhold, blomstring, farve og jordforhold, så UI’et får brug for rolige badges og metadata.

***

## Materialer

```text
Materialenavn: mørk tekstgrøn
Type/dimensioner: muted
Farve/materialeinfo: små neutrale chips
```

`Materiale` har navn, dimensioner, mængde, typeinfo, farve og kommentar, så farverne bør primært strukturere informationen — ikke vise materialefarver for kraftigt.

***

## Filer og thumbnails

```text
Billedthumbs: neutral ramme
PDF/dokumenter: ikon på lys salvie
Midlertidige filer: terracotta badge
Valgte filer: grøn border
```

Det passer til modellen, hvor `VedhaeftetFil` har filnavn, content type, oprettelsesdato, ejertype, ejer-id, lifetime og expiration, mens `VedhaeftetThumbnail` har thumbnail-størrelse, bredde, højde, content type og data.

***

# Do / Don’t for paletten

## Do

* Brug grøn til handling og navigation.
* Brug sandfarvet baggrund til ro.
* Brug hvide cards til læsbarhed.
* Brug terracotta sparsomt.
* Brug dæmpede badges frem for kraftige farveblokke.
* Hold farver ens på tværs af sider.

## Don’t

* Brug ikke mange forskellige grønne nuancer.
* Brug ikke store dekorative plantebaggrunde.
* Brug ikke rød/orange som generel accent.
* Brug ikke ren sort som standardtekst, hvis vi vil bevare det bløde udtryk.
* Brug ikke farve alene til at kommunikere status — tekst/ikon bør følge med.

***

# Min anbefalede beslutning

Jeg vil anbefale, at vi låser denne palette som **første version**:

```css
--mgp-primary: #3F6B4A;
--mgp-primary-dark: #2F5138;
--mgp-primary-soft: #DDE8D8;
--mgp-accent: #B86B4B;
--mgp-text: #243128;
--mgp-bg: #FAF8F2;
--mgp-surface: #FFFFFF;
--mgp-border: #D8D2C7;
--mgp-muted: #6F766D;
```

Det giver en god balance mellem haveidentitet, ro, professionelt udtryk og praktisk anvendelighed i Bootstrap/Blazor-komponenter.