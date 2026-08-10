# 3. Spacing

Spacing er vigtigere her end den måske først virker, fordi appen både skal føles:

* rolig
* overskuelig
* professionel
* ikke for kompakt
* ikke tom/kedelig
* brugbar på mobil, tablet og desktop

I dit oplæg står der netop, at designet skal have god læsbarhed og nemt overblik — “hverken for kompakt eller kedeligt” — og at der skal være passende luft mellem elementer.

## Grundprincip

Jeg vil anbefale et spacing-system baseret på en **4px/8px rytme**.

Det betyder ikke, at alle afstande skal være 8px, men at de fleste værdier bør være multipla af 4 eller 8:

```css
4px, 8px, 12px, 16px, 24px, 32px, 48px, 64px
```

Det giver en rolig og genkendelig rytme på tværs af:

* cards
* formularfelter
* knapper
* sektioner
* lister
* thumbnail grids
* sidebar
* header
* dashboard

***

# Anbefalet spacing-skala

Jeg foreslår denne centrale skala:

```css
:root {
  --space-2xs: .25rem;  /* 4px */
  --space-xs:  .5rem;   /* 8px */
  --space-sm:  .75rem;  /* 12px */
  --space-md:  1rem;    /* 16px */
  --space-lg:  1.5rem;  /* 24px */
  --space-xl:  2rem;    /* 32px */
  --space-2xl: 3rem;    /* 48px */
  --space-3xl: 4rem;    /* 64px */
}
```

Jeg ville bruge denne skala konsekvent i stedet for mange tilfældige værdier som `.7rem`, `.85rem`, `1.25rem`, `2.4rem` osv.

Vi kan stadig have enkelte specialværdier, men som udgangspunkt bør spacing være systematisk.

***

# Praktisk anvendelse

## 1. Side-padding

På desktop:

```css
main {
  padding: var(--space-xl) var(--space-lg) var(--space-2xl);
}
```

Det svarer cirka til:

```css
padding: 32px 24px 48px;
```

På mobil:

```css
main {
  padding: var(--space-lg) var(--space-md) var(--space-xl);
}
```

Altså cirka:

```css
padding: 24px 16px 32px;
```

Det giver nok luft uden at spilde plads på telefon.

***

## 2. Afstand mellem sektioner

Sektioner som Palette, Typografi, Kerneelementer osv. bør have tydelig separation.

```css
.section {
  margin-top: var(--space-xl);
}
```

Hvis en side har store hovedblokke, kan vi bruge:

```css
.section-large {
  margin-top: var(--space-2xl);
}
```

Min anbefaling:

| Element                      |   Spacing |
| ---------------------------- | --------: |
| Mellem hovedsektioner        |    `32px` |
| Mellem store arbejdsblokke   |    `48px` |
| Mellem card-title og indhold |  `8-12px` |
| Mellem relaterede felter     | `12-16px` |
| Mellem urelaterede grupper   | `24-32px` |

***

## 3. Card-padding

Cards er centrale i dit designoplæg, og du har nævnt, at cards gerne må bruges til gruppering.

Jeg anbefaler:

```css
.card {
  padding: var(--space-md);
}
```

Altså 16px som standard.

For større cards, fx hero/summary cards:

```css
.card-lg {
  padding: var(--space-lg);
}
```

Altså 24px.

For meget kompakte cards, fx små metadata-cards:

```css
.card-sm {
  padding: var(--space-sm);
}
```

Altså 12px.

### Min anbefaling

| Card-type                |                      Padding |
| ------------------------ | ---------------------------: |
| Standard card            |                       `16px` |
| Større summary/hero card |                       `24px` |
| Kompakt list-card        |                       `12px` |
| Dashboard hero           | `24-32px` afhængigt af skærm |

***

## 4. Grid-gap

Til cards/lister:

```css
.grid {
  gap: var(--space-md);
}
```

Standard: 16px.

Til større dashboardsektioner:

```css
.grid-comfortable {
  gap: var(--space-lg);
}
```

Standard: 24px.

Til små badges/chips:

```css
.badge-row {
  gap: var(--space-xs);
}
```

Standard: 8px.

***

## 5. Formular-spacing

Formularer skal være behagelige, fordi brugerne kommer til at oprette og redigere haver, bede, planter, materialer, butikker, kontakter, invitationer og filer.

Jeg anbefaler:

```css
.field {
  margin-bottom: var(--space-md);
}
```

For feltgrupper:

```css
.field-group {
  display: grid;
  gap: var(--space-md);
}
```

For større logiske sektioner i formularen:

```css
.form-section {
  margin-top: var(--space-xl);
}
```

### Eksempel

```text
[Havens navn]
8px til help text
16px til næste felt

[Adresse]
16px mellem felter
32px til næste formularsektion
```

Formularer skal ikke være for kompakte. Hvis de bliver for tætte, føles appen hurtigt som et administrationssystem.

***

## 6. Sidebar-spacing

Sidebar skal være kompakt nok til arbejdsbrug, men ikke tæt.

Jeg anbefaler:

```css
.sidebar {
  padding: var(--space-lg);
}

.brand {
  padding-bottom: var(--space-lg);
  margin-bottom: var(--space-lg);
}

.nav-section-title {
  margin: var(--space-lg) 0 var(--space-xs);
}

.nav-list {
  gap: var(--space-2xs);
}

.nav-link {
  padding: .625rem .75rem;
}
```

Her kan vi godt beholde en lidt specialværdi for nav-link padding, fordi klikfladen skal føles rigtig.

Hvis vi vil holde alt helt stringent:

```css
.nav-link {
  padding: var(--space-xs) var(--space-sm);
}
```

Men jeg vil faktisk tillade lidt mere vertikal padding på navigationen.

***

## 7. Header-spacing

Headeren skal være kompakt, fordi den findes på alle sider.

```css
.header-inner {
  padding: var(--space-sm) var(--space-lg);
}
```

Det giver cirka 12px top/bund og 24px sider.

På mobil:

```css
.header-inner {
  padding: var(--space-sm) var(--space-md);
}
```

***

## 8. Hero/landing-spacing

Hero-sektionen på dashboardet må gerne være mere luftig:

```css
.hero {
  padding: clamp(var(--space-lg), 3vw, var(--space-xl));
  gap: var(--space-lg);
}
```

Det betyder:

* minimum 24px
* op til 32px
* responsivt efter skærmbredde

Jeg vil undgå meget store hero-flader, fordi appen er et arbejdsredskab.

***

# Vertikal rytme

Et godt princip:

```css
section + section {
  margin-top: var(--space-xl);
}
```

Og inde i cards:

```css
.card > * + * {
  margin-top: var(--space-sm);
}
```

Men her skal vi være forsigtige, fordi formularer, grids og knap-rækker ofte selv styrer spacing.

Jeg vil hellere bruge små utility-klasser:

```css
.stack-xs { display: grid; gap: var(--space-xs); }
.stack-sm { display: grid; gap: var(--space-sm); }
.stack-md { display: grid; gap: var(--space-md); }
.stack-lg { display: grid; gap: var(--space-lg); }
```

Det bliver meget brugbart i Blazor-komponenter.

Eksempel:

```html
<div class="card stack-sm">
  <h3>Villa Solbakken</h3>
  <p class="meta">Kundehave · opdateret for nylig</p>
  <div class="badge-row">...</div>
</div>
```

***

# Responsiv spacing

Jeg anbefaler, at spacing reduceres en smule på mobil, men ikke for meget.

```css
@media (max-width: 640px) {
  :root {
    --page-x: var(--space-md);
    --page-y: var(--space-lg);
  }
}
```

Alternativt kan vi definere sidepadding sådan:

```css
:root {
  --page-x: var(--space-lg);
  --page-y: var(--space-xl);
}

@media (max-width: 640px) {
  :root {
    --page-x: var(--space-md);
    --page-y: var(--space-lg);
  }
}

main {
  padding: var(--page-y) var(--page-x) var(--space-2xl);
}
```

Det giver et centralt sted at justere hele appens luft.

***

# Min konkrete anbefaling

Jeg vil anbefale, at vi låser følgende spacing-principper i version 1:

```css
:root {
  --space-2xs: .25rem;
  --space-xs: .5rem;
  --space-sm: .75rem;
  --space-md: 1rem;
  --space-lg: 1.5rem;
  --space-xl: 2rem;
  --space-2xl: 3rem;
  --space-3xl: 4rem;

  --page-x: var(--space-lg);
  --page-y: var(--space-xl);
}
```

Og opdaterer centrale elementer sådan:

```css
main {
  padding: var(--page-y) var(--page-x) var(--space-2xl);
}

.sidebar {
  padding: var(--space-lg);
}

.header-inner {
  padding: var(--space-sm) var(--space-lg);
}

.section {
  margin-top: var(--space-xl);
}

.grid {
  gap: var(--space-md);
}

.card {
  padding: var(--space-md);
}

.hero {
  padding: clamp(var(--space-lg), 3vw, var(--space-xl));
  gap: var(--space-lg);
}

.field {
  margin-bottom: var(--space-md);
}

.badge-row,
.card-actions {
  gap: var(--space-xs);
}
```

Og på mobil:

```css
@media (max-width: 640px) {
  :root {
    --page-x: var(--space-md);
    --page-y: var(--space-lg);
  }

  .card {
    padding: var(--space-sm);
  }
}
```

***

# Vurdering af den eksisterende demo

Den nuværende demo har allerede en ganske god spacing-fornemmelse, men værdierne er lidt blandede:

```css
.7rem
.75rem
.85rem
.9rem
1rem
1.25rem
1.5rem
2rem
clamp(1.4rem, 3vw, 2.4rem)
```

Det fungerer visuelt, men for et designsystem vil jeg rydde det op, så vi får færre tilfældige værdier og bedre konsistens.

Mit forslag er derfor ikke at ændre udtrykket dramatisk — bare gøre spacing mere systematisk og lettere at genbruge i Blazor-komponenter.

## Beslutning

Min anbefaling for spacing er:

> Brug en 4/8px-baseret spacing-skala, med 16px som standard-card-padding, 24px som større layoutluft og 32px mellem hovedsektioner.