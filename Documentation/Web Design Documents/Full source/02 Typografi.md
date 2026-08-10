# 2. Typografi

## Anbefalet retning

Jeg vil anbefale typografiretningen:

## **“Praktisk, rolig og professionel systemtypografi”**

Det betyder:

* ingen dekorative fonte
* ingen tunge eksterne font-loads i første version
* god læsbarhed i formularer, cards og tabeller
* tydeligt hierarki mellem overskrifter, metadata og handlinger
* blødere udtryk gennem vægt, spacing og farver — ikke gennem fancy skrifttyper

Min anbefaling er derfor at starte med **system font stack**:

```css
font-family: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
```

Det er allerede brugt i demosiden, og jeg synes faktisk, det passer godt til den farvepalette vi nu har justeret. Fordelen er, at den loader hurtigt, virker på alle platforme og føles moderne uden at stjæle opmærksomhed fra haveindholdet. Det matcher også ønsket om hurtig load og god anvendelse på mobil, tablet og desktop.

***

# Hvorfor ikke en “pænere” webfont med det samme?

Vi *kan* senere vælge en ekstern font som f.eks. Inter, Source Sans, Lora eller Nunito Sans, men jeg vil ikke starte der.

Grunden er:

1. **Hurtig load er et mål** i opgavebeskrivelsen.
2. Appen er et arbejdsredskab — ikke en marketingfrontpage.
3. Brugerne skal læse navne, beskrivelser, planteegenskaber, dokumenttitler, materialer, invitationer og formularfelter.
4. Systemfonte giver god native rendering på både Windows, iOS, Android og macOS.

Så min anbefaling er:

> **Brug systemfonte i applikationen. Overvej kun ekstern font senere, hvis brandudtrykket viser sig at mangle karakter.**

***

# Foreslået typografisk hierarki

Her er mit forslag til tekstniveauer.

## 1. Sideoverskrift / H1

Bruges på dashboard, have-detalje, planteoversigt osv.

```css
--font-size-h1: clamp(2rem, 4vw, 3.25rem);
--line-height-heading: 1.12;
--letter-spacing-heading: -0.03em;
--font-weight-heading: 800;
```

Eksempel:

```text
Mine haver
Villa Solbakken
Planter
Materialer
```

Jeg vil bruge store H1’er sparsomt — især på landing/dashboard. På almindelige arbejdssider må H1 gerne være mere kompakt.

***

## 2. Sektionstitel / H2

Bruges til områder som “Aktive haver”, “Seneste filer”, “Bede”, “Medlemmer”.

```css
--font-size-h2: clamp(1.35rem, 2.2vw, 2rem);
--line-height-heading: 1.15;
--font-weight-heading: 800;
```

H2 skal hjælpe brugeren med at scanne siden hurtigt.

***

## 3. Card-title / H3

Bruges på cards:

```css
--font-size-h3: 1.05rem;
--line-height-heading: 1.2;
--font-weight-card-title: 800;
```

Eksempel:

```text
Villa Solbakken
Haveskitse maj.pdf
Lavendel
Chaussésten
```

Her er det vigtigt, at cardtitler er tydelige, men ikke så store at kortene bliver tunge.

***

## 4. Brødtekst

Bruges til beskrivelser og almindeligt indhold:

```css
--font-size-body: 1rem;
--line-height-body: 1.55;
--font-weight-body: 400;
```

Jeg vil ikke gå under `1rem` som standard, fordi appen skal være rar at læse på mobile enheder og tablets.

***

## 5. Metadata / sekundær tekst

Bruges til datoer, statusinfo, filtype, latinsk navn, opdateret-dato osv.

```css
--font-size-meta: .9rem;
--line-height-meta: 1.4;
--font-weight-meta: 400;
```

Eksempel:

```text
Kundehave · opdateret for nylig
PDF · midlertidig fil
Lavandula angustifolia
```

Denne tekst bør bruge `--mgp-text-muted`, så den ikke konkurrerer med titlerne.

***

## 6. Labels i formularer

Labels skal være tydelige, fordi brugeren kommer til at oprette og redigere haver, planter, materialer, butikker, kontakter og invitationer. Modellerne har mange felter som navn, beskrivelse, adresse, kontakt, planteegenskaber, materialeinfo og filmetadata.

```css
--font-size-label: .92rem;
--font-weight-label: 750;
```

Jeg vil undgå labels med for lav kontrast. Labels skal være mørke og tydelige.

***

## 7. Knapper og navigation

Knapper og sidebar-links bør være semibold/bold, fordi de er handlings- og navigationspunkter.

```css
--font-size-button: .95rem;
--font-weight-action: 750;
```

Jeg vil dog ikke bruge for store bogstaver eller uppercase på knapper, fordi det hurtigt bliver administrativt/tungt.

Godt:

```text
Opret ny have
Upload tegning
Invitér kunde
```

Mindre godt:

```text
OPRET NY HAVE
UPLOAD TEGNING
INVITÉR KUNDE
```

Uppercase kan bruges til små sektionslabels i sidebar, som vi allerede gør:

```text
OVERBLIK
ARBEJDE
KONTO
```

***

# Særligt for denne app

## Latinske plantenavne

Da `Plante` har både `AlmindeligtNavn` og `LatinskNavn`, vil jeg give latinske navne en særskilt typografisk stil.

Forslag:

```css
.latin-name {
    color: var(--mgp-text-muted);
    font-style: italic;
    font-size: var(--font-size-meta);
}
```

Eksempel:

```html
<h3>Lavendel</h3>
<p class="latin-name">Lavandula angustifolia</p>
```

Det giver fagligt præg uden at gøre UI’et tungt.

***

## Tal, dimensioner og mængder

Da flere modeller indeholder dimensioner, mængder, datoer og størrelser — f.eks. bede med dimensioner, materialer med mængder og thumbnails med width/height — bør tal være nemme at sammenligne.

Jeg anbefaler:

```css
.numeric {
    font-variant-numeric: tabular-nums;
}
```

Eksempel:

```html
<span class="numeric">3,50 × 1,20 m</span>
<span class="numeric">800 × 600 px</span>
```

Det gør lister og kort mere rolige, fordi tallene står mere stabilt.

***

## Lange beskrivelser

`Have`, `Bed`, `Plante`, `Materiale`, `Projekt`-relaterede modeller og filmetadata kan alle have beskrivelser/kommentarer.

Derfor bør brødtekst have:

```css
max-width: 68ch;
line-height: 1.55;
```

Det forhindrer meget lange tekstlinjer på store skærme.

***

# Konkret CSS-forslag til demosiden

Jeg vil tilføje dette til `:root`:

```css
:root {
  /* Typography */
  --font-sans: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
  --font-mono: ui-monospace, SFMono-Regular, Menlo, Consolas, "Liberation Mono", monospace;

  --font-size-xs: .78rem;
  --font-size-sm: .9rem;
  --font-size-base: 1rem;
  --font-size-md: 1.08rem;
  --font-size-lg: 1.25rem;
  --font-size-xl: clamp(1.35rem, 2.2vw, 2rem);
  --font-size-xxl: clamp(2rem, 4vw, 3.25rem);

  --line-height-tight: 1.15;
  --line-height-base: 1.55;
  --line-height-relaxed: 1.7;

  --font-weight-normal: 400;
  --font-weight-medium: 600;
  --font-weight-bold: 750;
  --font-weight-heavy: 800;

  --letter-spacing-heading: -0.03em;
}
```

Og opdatere basisreglerne:

```css
body {
  font-family: var(--font-sans);
  font-size: var(--font-size-base);
  line-height: var(--line-height-base);
  color: var(--mgp-text);
  background: var(--mgp-bg);
}

h1,
h2,
h3 {
  line-height: var(--line-height-tight);
  letter-spacing: var(--letter-spacing-heading);
  color: var(--mgp-text);
}

h1 {
  font-size: var(--font-size-xxl);
  font-weight: var(--font-weight-heavy);
}

h2 {
  font-size: var(--font-size-xl);
  font-weight: var(--font-weight-heavy);
}

h3 {
  font-size: var(--font-size-md);
  font-weight: var(--font-weight-heavy);
}

p {
  margin-top: 0;
}

.lead {
  font-size: var(--font-size-md);
  line-height: var(--line-height-base);
  color: var(--mgp-text-muted);
  max-width: 68ch;
}

.meta {
  font-size: var(--font-size-sm);
  color: var(--mgp-text-muted);
  line-height: 1.4;
}

.label,
label {
  font-size: .92rem;
  font-weight: var(--font-weight-bold);
}

.btn,
.nav-link {
  font-size: .95rem;
  font-weight: var(--font-weight-bold);
}

.nav-section-title {
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-bold);
  letter-spacing: .08em;
  text-transform: uppercase;
}

.latin-name {
  color: var(--mgp-text-muted);
  font-style: italic;
  font-size: var(--font-size-sm);
}

.numeric {
  font-variant-numeric: tabular-nums;
}

code,
.mono {
  font-family: var(--font-mono);
  font-size: .92em;
}
```

***

# Mit konkrete typografivalg

Jeg vil anbefale, at vi beslutter følgende som version 1:

| Område               | Valg                                   |
| -------------------- | -------------------------------------- |
| Primær font          | System font stack                      |
| Ekstern font         | Nej, ikke i første version             |
| H1                   | Stor, tung, let negativ letter-spacing |
| H2                   | Tydelig sektionstitel                  |
| H3/card-title        | Kompakt og kraftig                     |
| Brødtekst            | 1rem / line-height 1.55                |
| Metadata             | .9rem, muted                           |
| Labels               | Semibold/bold, god kontrast            |
| Knapper              | Semibold/bold, ikke uppercase          |
| Latinske plantenavne | Italic + muted                         |
| Tal/dimensioner      | Tabular nums                           |

***

# Visuelt princip

Typografien skal ikke sige “se mig”. Den skal sige:

> “Her er et professionelt arbejdsrum, hvor du hurtigt kan forstå haven, materialerne, planterne og næste handling.”

Det passer godt til det mål, du beskrev: roligt overblik, logisk struktur, god læsbarhed og brug på mange skærmstørrelser.

Min anbefaling: **Lad os beholde systemtypografi som basis og tilføje de typografiske CSS-variabler ovenfor til demosiden.**