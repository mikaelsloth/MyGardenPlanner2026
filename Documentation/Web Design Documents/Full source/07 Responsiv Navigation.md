## 7. Responsiv navigation

Jeg vil foreslå, at vi tænker navigationen i tre niveauer:

| Skærmtype | Navigation                                                         |
| --------- | ------------------------------------------------------------------ |
| Desktop   | Fast venstre sidebar                                               |
| Tablet    | Kompakt sidebar eller off-canvas                                   |
| Mobil     | Topbar + burger/off-canvas eller bottom nav for primære handlinger |

Min anbefaling er:

> **Desktop bruger permanent sidebar. Mobil bruger topbar med menu-knap og off-canvas navigation.**

Jeg vil ikke starte med bottom navigation som primær løsning, fordi MyGardenPlanner sandsynligvis får flere sektioner end 4-5 faste hovedpunkter. Bottom nav kan dog være relevant senere til få kernehandlinger.

***

# Desktop-navigation

Den nuværende sidebar fungerer godt som desktop-mønster:

```text
MyGardenPlanner

Overblik
- Dashboard
- Mine haver

Arbejde
- Bede
- Filer & tegninger
- Planter
- Materialer
- Butikker

Konto
- Profil
- Medlemmer
```

## Behold på desktop

* fast venstre sidebar
* tydelige gruppeoverskrifter
* aktiv markering
* kompakte, men rolige nav-links
* scroll i sidebar hvis listen bliver lang

CSS-retningen er allerede god:

```css
.sidebar {
  width: 280px;
  background: var(--mgp-surface);
  border-right: 1px solid var(--mgp-border);
}
```

Jeg vil dog senere anbefale en `--sidebar-width` token:

```css
--sidebar-width: 280px;
```

***

# Tablet-navigation

Tablet er lidt sværere. Her har vi typisk to muligheder:

## Mulighed A: Skjul sidebar og brug off-canvas

Det er simpelt og konsekvent med mobil.

Fordel:

* mindre layoutkompleksitet
* mere plads til indhold
* samme mønster som mobil

Ulempe:

* navigationen er mindre synlig

## Mulighed B: Collapsed sidebar

Sidebar bliver til en smal ikonkolonne.

Fordel:

* navigationen er stadig synlig
* god til power users

Ulempe:

* kræver gode ikoner
* kan blive for tidligt komplekst

## Min anbefaling

Start med:

```text
Desktop: sidebar
Tablet/mobil: topbar + off-canvas menu
```

Collapsed sidebar kan komme senere, hvis behovet opstår.

***

# Mobil-navigation

På mobil bør navigationen være:

* topbar med brand/context
* menu-knap
* off-canvas panel
* stor klikflade
* tydelig aktiv side
* ingen hover-afhængighed
* mulighed for at lukke menuen

Struktur:

```html
<header class="mobile-header">
  <button class="btn btn-secondary btn-icon" aria-label="Åbn menu">☰</button>
  <div>
    <p class="context-title">MyGardenPlanner</p>
    <p class="context-subtitle">Villa Solbakken</p>
  </div>
</header>
```

Off-canvas:

```html
<div class="nav-drawer">
  <div class="nav-drawer-header">
    <div class="brand">MyGardenPlanner</div>
    <button class="btn btn-secondary btn-icon" aria-label="Luk menu">×</button>
  </div>

  <nav>
    ...
  </nav>
</div>
```

***

# Vigtig beslutning: navigation vs. context

Jeg synes, vi skal skelne mellem:

## Global navigation

Hvor i appen er jeg?

* Dashboard
* Haver
* Planter
* Materialer
* Filer
* Butikker
* Profil

## Lokal kontekstnavigation

Hvad arbejder jeg med lige nu?

Eksempel: `Villa Solbakken`

* Overblik
* Bede
* Filer
* Medlemmer
* Indstillinger

Det betyder, at vi måske senere skal have to navigationsmønstre:

1. **App navigation** i sidebar/drawer
2. **Context tabs** inde på fx en have-detaljeside

Det er vigtigt, fordi appen ellers hurtigt kan få en sidebar med for mange punkter.

***

# Context tabs

På detaljesider kan tabs være gode:

```html
<nav class="context-tabs">
  <button class="context-tab active">Overblik</button>
  <button class="context-tab">Bede</button>
  <button class="context-tab">Filer</button>
  <button class="context-tab">Medlemmer</button>
</nav>
```

CSS-idé:

```css
.context-tabs {
  display: flex;
  gap: var(--space-xs);
  overflow-x: auto;
  padding-bottom: var(--space-xs);
  border-bottom: 1px solid var(--mgp-border);
}

.context-tab {
  border: 1px solid transparent;
  background: transparent;
  color: var(--mgp-text-muted);
  border-radius: 999px;
  padding: var(--space-xs) var(--space-sm);
  font-weight: var(--font-weight-bold);
  white-space: nowrap;
}

.context-tab.active {
  background: var(--mgp-primary-soft);
  color: var(--mgp-primary-dark);
  border-color: rgba(63,107,74,.18);
}
```

På mobil fungerer tabs godt som horisontal scroll, så længe der ikke er for mange.

***

# Navigationsstates

Vi bør definere disse states:

| State     | Brug                                  |
| --------- | ------------------------------------- |
| Default   | Almindeligt nav-link                  |
| Hover     | Desktop feedback                      |
| Active    | Aktuel side                           |
| Focus     | Tastatur                              |
| Disabled  | Ikke tilgængelig                      |
| Attention | Fx invitationer/filer kræver handling |

Eksempel:

```html
<button class="nav-link active">Dashboard</button>
<button class="nav-link">Mine haver</button>
<button class="nav-link">
  Filer
  <span class="nav-badge">2</span>
</button>


CSS:

```css
.nav-badge {
  margin-left: auto;
  min-width: 1.35rem;
  height: 1.35rem;
  border-radius: 999px;
  display: inline-grid;
  place-items: center;
  background: var(--mgp-warning-bg);
  color: #653521;
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-bold);
}
```

Dette kan være nyttigt til fx:

* filer der udløber snart
* invitationer der afventer
* nye uploads
* manglende oplysninger

Men vi skal bruge badges sparsomt.

***

# Off-canvas pattern

Jeg vil anbefale en CSS-only demo først, hvor drawer-klassen bare vises som eksempel.

Senere i Blazor kan åbne/lukke styres af state.

CSS-retning:

```css
.mobile-nav-toggle {
  display: none;
}

.nav-drawer {
  display: none;
}

.drawer-backdrop {
  display: none;
}

@media (max-width: 940px) {
  .desktop-sidebar {
    display: none;
  }

  .mobile-nav-toggle {
    display: inline-flex;
  }

  .nav-drawer {
    position: fixed;
    inset: 0 auto 0 0;
    width: min(86vw, 340px);
    background: var(--mgp-surface);
    border-right: 1px solid var(--mgp-border);
    padding: var(--space-lg);
    z-index: 30;
    box-shadow: var(--shadow-md);
  }

  .drawer-backdrop {
    position: fixed;
    inset: 0;
    background: rgba(36,49,40,.22);
    z-index: 20;
  }
}
```

I demoen kan vi vise drawer som “åben tilstand”.

***

# Sticky header på mobil

Jeg vil beholde sticky header, men den skal være kompakt.

```css
.mobile-header {
  display: none;
}

@media (max-width: 940px) {
  .mobile-header {
    display: flex;
    align-items: center;
    gap: var(--space-sm);
    padding: var(--space-sm) var(--space-md);
    background: rgba(250,248,242,.92);
    border-bottom: 1px solid var(--mgp-border);
    position: sticky;
    top: 0;
    z-index: 10;
  }
}
```

Vi skal være opmærksomme på, at for meget sticky UI på mobil kan tage plads. Derfor bør headeren holdes lav.

***

# Navigation og formularer

Efter formularpunktet er det værd at tænke på:

* På lange formularer skal navigationen ikke fylde for meget.
* Mobilmenuen skal ikke forstyrre form focus.
* Eventuelle tabs på en formularside bør være sticky med forsigtighed.
* Form actions kan evt. blive sticky senere, men det er et separat mønster.

Jeg ville ikke indføre sticky form actions endnu. Først navigation.

***

### Collapsible desktop og tablet

Vi talte før om:

* desktop: fast sidebar
* tablet/mobil: topbar + drawer

En collapsed desktop sidebar kan være et ekstra breakpoint imellem:

```text
Large desktop: fuld sidebar
Medium desktop/laptop: collapsible eller collapsed sidebar
Tablet/mobil: drawer
```

Det kan give en mere flydende responsiv oplevelse.

***

## Hvad taler imod?

### 1. Mere kompleksitet

Det største modargument er kompleksitet.

En collapsible sidebar kræver beslutninger om:

* hvordan den kollapser
* om state gemmes
* hvad der vises i collapsed tilstand
* hvordan active states vises
* hvordan badges vises
* hvordan tooltips fungerer
* hvordan keyboard navigation fungerer
* hvad der sker ved resize
* hvordan den spiller sammen med mobil drawer

Det er ikke nødvendigvis svært, men det er mere, der skal designes og testes.

***

### 2. Ikonafhængighed

Hvis collapsed sidebar kun viser ikoner, bliver designet afhængigt af, at hvert navigationspunkt har et stærkt ikon.

For nogle punkter er det nemt:

* Dashboard
* Haver
* Filer
* Profil

Men for andre kan det blive uklart:

* Bede
* Materialer
* Butikker
* Medlemmer
* Invitationer

Hvis ikonerne ikke er intuitive, kan collapsed navigation blive mindre brugbar.

***

### 3. Risiko for skjult navigation

En fuld sidebar giver et godt overblik over appens struktur. Hvis den ofte er collapsed, bliver strukturen mindre tydelig.

Det kan især være et problem for nye brugere, der endnu ikke ved hvor tingene ligger.

Derfor bør collapse være **brugerens valg**, ikke noget vi automatisk tvinger på store desktopskærme.

***

### 4. Badges og status kan blive sværere

Vi har talt om badges i navigationen, fx:

* filer der udløber snart
* invitationer der afventer
* opmærksomhedspunkter

I collapsed tilstand er der mindre plads til disse signaler. Vi kan stadig vise små dots eller tal, men betydningen bliver mindre tydelig uden label.

Det skal designes forsigtigt.

***

## Min anbefaling

Jeg vil anbefale, at vi designer tre desktop-/navigationstilstande:

| Tilstand                     | Brug                                     |
| ---------------------------- | ---------------------------------------- |
| **Expanded sidebar**         | Standard på desktop                      |
| **Collapsed sidebar / rail** | Valgfri på desktop, især til tunge sider |
| **Drawer navigation**        | Tablet/mobil                             |

Altså:

```text
Desktop default:
[280px sidebar] + content

Desktop collapsed:
[72px nav rail] + content

Mobile/tablet:
[topbar] + off-canvas drawer
```

***

## Hvordan bør collapsed sidebar se ud?

Jeg ville ikke skjule den helt på desktop. Jeg ville hellere lave en **nav rail**.

### Expanded

```text
✿ MyGardenPlanner

Overblik
Dashboard
Mine haver

Arbejde
Bede
Filer
Planter
Materialer
Butikker
```

### Collapsed

```text
✿
☰

⌂
🌿
▧
✎
⬚
```

Men med vigtigt forbehold:

* ikoner skal vælges bevidst senere
* active state skal stadig være tydelig
* tooltips bør vises ved hover/focus
* collapse-knappen skal være let at finde
* badges skal kunne vises som små dots/tal

***

## Designprincip for collapsed sidebar

Jeg ville formulere det sådan:

> Collapsed sidebar er en pladsbesparende desktop-tilstand til erfarne brugere og indholdstunge sider. Den må aldrig være den eneste måde at forstå navigationen på.

Det betyder:

* expanded er standard
* collapsed er valgfri
* labels findes altid i expanded/drawer
* collapsed skal stadig være keyboard-venlig
* active state skal være tydelig
* tooltips bør understøtte forståelse

***

## Skal vi gemme brugerens valg?

Ja, sandsynligvis.

Hvis brugeren kollapser sidebaren, bør appen huske det — fx via local storage i browseren senere.

Men designmæssigt kan vi bare forberede klasserne:

```css
.app-shell.sidebar-collapsed {
  grid-template-columns: 72px 1fr;
}
```

Og:

```css
.sidebar-collapsed .nav-label,
.sidebar-collapsed .nav-section-title,
.sidebar-collapsed .brand-text {
  display: none;
}
```

***

## Hvad med automatisk collapse på bestemte sider?

Det ville jeg være forsigtig med.

Det kan være irriterende, hvis navigationen ændrer sig automatisk fra side til side.

Bedre:

* brugeren vælger collapsed/expanded
* appen husker valget
* eventuelt kan meget smalle desktop-widths starte collapsed

Men jeg ville ikke sige:

> “På liste-sider kollapser vi automatisk sidebaren.”

Det kan føles uforudsigeligt.

***

# Min anbefalede beslutning for responsiv navigation

Jeg vil låse det sådan:

1. **Desktop:** permanent venstre sidebar.
2. **Tablet/mobil:** sidebar skjules.
3. **Mobil/tablet:** topbar med menu-knap.
4. **Navigation åbnes som off-canvas drawer.**
5. **Aktiv side markeres med samme rolige salvie-baggrund.**
6. **Badges bruges kun til reelle opmærksomhedspunkter.**
7. **Context tabs bruges inde på entity-detail-sider.**
8. **Collapsed icon-sidebar udskydes.**
9. **Drawer skal kunne rumme samme nav-struktur som desktop-sidebar.**
10. **Navigation skal være keyboard- og touch-venlig.**