# 25. Komponentnavngivning og design tokens

## Overordnet princip

> **Komponentnavne skal beskrive formål og brug. Design tokens skal beskrive designbeslutninger — ikke tilfældige visuelle værdier.**

Det betyder, at vi skal undgå navne som:

```text
GreenBox
BigCard
SmallButton
NicePanel
```

og i stedet bruge navne, der siger noget om komponentens rolle:

```text
StatusMessage
EntityCard
DetailHeader
FilterBar
DangerZone
```

Designsystemet skal gøre det let at svare på:

* Hvad hedder komponenten?
* Hvornår bruges den?
* Hvilke varianter har den?
* Hvilke states har den?
* Hvilke tokens bruger den?
* Hvilke accessibility-krav har den?
* Hvordan opfører den sig på mobil og print?

***

# 25.1 Hvorfor komponentordbog?

Efter alle de emner vi har gennemgået, har vi nu mange patterns:

* empty states
* loading states
* status messages
* confirmation dialogs
* archived cards
* permission states
* toast/inline status
* filter bars
* compact rows
* detail headers
* mobile drawers
* accessibility patterns
* density modes
* attention badges

Hvis de ikke får fælles navne, bliver implementationen hurtigt inkonsistent.

## Designregel

> **Alle genbrugte UI-mønstre skal have ét navn, én dokumenteret rolle og ét anbefalet usage-pattern.**

***

# 25.2 Navngivning: principper

Jeg ville bruge disse principper:

## 1. Navngiv efter rolle, ikke udseende

Godt:

```text
StatusMessage
AttentionBadge
EntityCard
SummaryCard
FilterDrawer
```

Mindre godt:

```text
OrangeBox
GreenPill
BigCard
SmallPanel
```

## 2. Brug generiske komponentnavne for patterns

Eksempel:

```text
EntityCard
CompactEntityRow
DetailHeader
StatusMessage
```

## 3. Brug domænespecifikke navne, når komponenten er bundet til domænet

Eksempel:

```text
GardenSummaryCard
PlantListRow
FilePreviewCard
MemberAccessRow
InvitationStatusCard
```

## 4. Brug konsekvent suffix

| Suffix    | Brug                             |
| --------- | -------------------------------- |
| `Card`    | Afgrænset informationsblok       |
| `Row`     | Kompakt listeelement             |
| `Table`   | Tabulære data                    |
| `Dialog`  | Modal beslutning                 |
| `Drawer`  | Side/bottom panel                |
| `Badge`   | Kort status/klassifikation       |
| `Message` | Inline feedback                  |
| `Banner`  | Side/global besked               |
| `Section` | Større indholdsblok              |
| `Header`  | Top/intro for side eller sektion |
| `Toolbar` | Samling af handlinger/kontroller |

## Designregel

> **Komponentnavne skal kunne læses uden at kende CSS’en.**

***

# 25.3 Foreslået komponentordbog

## Foundation components

Disse er basale byggesten.

```text
Button
IconButton
Badge
StatusDot
Link
FormField
TextInput
SelectInput
CheckboxGroup
RadioGroup
```

## Layout components

```text
AppShell
PageHeader
DetailHeader
SectionHeader
SummaryGrid
TwoColumnLayout
SidebarPanel
PrintSection
```

## Feedback components

```text
StatusMessage
Toast
GlobalBanner
InlineValidationMessage
ProcessingStatus
NoAccessState
ErrorState
EmptyState
FilteredEmptyState
```

## Data display components

```text
EntityCard
CompactEntityRow
DataTable
ThumbnailGrid
MediaCard
SummaryCard
MetadataList
ActivityList
```

## Interaction components

```text
ConfirmDialog
DangerZone
FilterBar
FilterDrawer
SortSelect
ViewModeToggle
ActiveFilterChips
CollapsibleSectionCard
ContextTabs
Pagination
```

## Domain components

```text
GardenCard
GardenDetailHeader
BedCard
PlantCard
PlantRow
MaterialRow
FilePreviewCard
FileUploadPanel
MemberAccessRow
InvitationStatusCard
```

## Designregel

> **Start med generiske komponenter. Lav domænekomponenter, når data og adfærd er specifik for MyGardenPlanner.**

***

# 25.4 Component naming hierarchy

Jeg ville strukturere komponenter i tre niveauer:

## Level 1 — primitives

Små komponenter uden domæneviden.

```text
Button
Badge
FormField
StatusMessage
```

## Level 2 — patterns

Komponenter der samler primitives til et UI-mønster.

```text
EntityCard
FilterBar
ConfirmDialog
CollapsibleSectionCard
```

## Level 3 — domain components

Komponenter med MyGardenPlanner-domæne.

```text
GardenCard
FilePreviewCard
InvitationStatusCard
PlantSearchResultRow
```

## Designregel

> **Domænekomponenter må gerne bruge generiske pattern-komponenter internt, men pattern-komponenter må ikke kende domænedata.**

Eksempel:

```text
GardenCard bruger EntityCard
InvitationStatusCard bruger Badge + CompactEntityRow
FilePreviewCard bruger MediaCard
```

***

# 25.5 Variant naming

Komponenter skal have få og tydelige variants.

## Button variants

```text
primary
secondary
danger
ghost
```

## Badge variants

```text
neutral
primary
warning
danger
info
restricted
```

## StatusMessage variants

```text
success
info
warning
danger
processing
restricted
```

## Card variants

```text
default
highlight
archived
restricted
dangerZone
```

## Density variants

```text
comfortable
default
compact
print
```

## Designregel

> **Variants skal beskrive intention — ikke farve. Brug `warning`, ikke `orange`. Brug `danger`, ikke `red`.**

***

# 25.6 State naming

States bør være fælles på tværs af komponenter.

```text
default
hover
focus
active
disabled
loading
selected
expanded
collapsed
empty
error
restricted
archived
processing
```

## Eksempel

En fil kan have:

```text
FilePreviewCard
state: processing
badge: Thumbnail oprettes
action: Vis metadata
```

En have kan have:

```text
GardenCard
state: archived
badge: Arkiveret
action: Gendan
```

## Designregel

> **State er ikke det samme som variant. Variant er typen af komponent. State er komponentens aktuelle tilstand.**

***

# 25.7 Design tokens: hvad skal tokens bruges til?

Design tokens skal gøre designet konsistent og lettere at ændre.

De bør dække:

* farver
* typografi
* spacing
* radius
* shadow
* border
* z-index
* breakpoints
* density
* motion
* focus
* print
* component-specific tokens

## Designregel

> **Tokens skal repræsentere designbeslutninger, ikke bare rå værdier.**

Eksempel:

Godt:

```css
--color-surface
--color-text-muted
--space-md
--radius-card
--shadow-card
```

Mindre godt:

```css
--green1
--gray7
--12px
--boxshadow2
```

***

# 25.8 Token taxonomy

Jeg ville opdele tokens i lag.

## Global tokens

Rå fundamentværdier.

```css
--color-green-700: #2F5138;
--color-green-600: #3F6B4A;
--color-sand-50: #FAF8F2;
--space-4: 1rem;
--radius-3: .75rem;
```

## Semantic tokens

Betydning i UI’et.

```css
--color-primary: var(--color-green-600);
--color-primary-dark: var(--color-green-700);
--color-bg: var(--color-sand-50);
--color-surface: #FFFFFF;
--color-text: #243128;
--color-text-muted: #6F766D;
--color-danger: #9F3A38;
```

## Component tokens

Specifikke komponentbeslutninger.

```css
--card-padding: var(--space-md);
--card-radius: var(--radius-md);
--button-height: 44px;
--status-border-color: var(--color-border);
--focus-ring-color: var(--color-primary-dark);
```

## Designregel

> **Brug semantic tokens i komponenter. Global tokens bruges til at definere systemet, ikke direkte i hver komponent.**

***

# 25.9 Foreslået token-navngivning

Jeg ville bruge en enkel, stabil navngivning.

## Color tokens

```css
--color-bg
--color-surface
--color-surface-muted
--color-border
--color-text
--color-text-muted

--color-primary
--color-primary-dark
--color-primary-soft

--color-accent
--color-warning-bg

--color-danger
--color-danger-soft

--color-info
--color-info-soft
```

## Spacing tokens

```css
--space-2xs
--space-xs
--space-sm
--space-md
--space-lg
--space-xl
--space-2xl
```

## Radius tokens

```css
--radius-sm
--radius-md
--radius-lg
--radius-pill
```

## Shadow tokens

```css
--shadow-sm
--shadow-md
--shadow-none
```

## Typography tokens

```css
--font-sans
--font-mono

--font-size-xs
--font-size-sm
--font-size-base
--font-size-md
--font-size-xl
--font-size-xxl

--line-height-tight
--line-height-base

--font-weight-medium
--font-weight-bold
```

## Layout tokens

```css
--content-max
--sidebar-width
--page-padding-x
--page-padding-y
--section-gap
```

## Interaction tokens

```css
--focus-ring-color
--focus-ring-width
--focus-ring-offset
--control-height
--touch-target-min
```

## Density tokens

```css
--density-card-padding
--density-row-padding-y
--density-row-padding-x
--density-table-cell-padding
--density-section-gap
```

***

# 25.10 MyGardenPlanner token prefix

Vi har brugt `--mgp-*` i demoerne:

```css
--mgp-primary
--mgp-bg
--mgp-surface
--mgp-border
```

Det er fint, især hvis tokens lever sammen med andre CSS-systemer.

Jeg ville dog vælge én af to strategier:

## Strategi A — kortere semantic tokens

```css
--color-primary
--color-bg
--space-md
```

Fordel: lettere at læse.

## Strategi B — namespaced tokens

```css
--mgp-color-primary
--mgp-color-bg
--mgp-space-md
```

Fordel: mindre risiko for konflikt.

## Min anbefaling

For MyGardenPlanner ville jeg bruge namespaced tokens i det egentlige projekt:

```css
--mgp-color-primary
--mgp-color-primary-dark
--mgp-color-bg
--mgp-color-surface
--mgp-space-md
--mgp-radius-md
```

Og eventuelt holde demoerne lidt kortere.

## Designregel

> **Vælg én token-prefix-strategi og brug den konsekvent.**

***

# 25.11 Tokens og density

Fra #13 bør density ikke laves som separate komponenter, men som token overrides.

Eksempel:

```css
:root {
  --mgp-density-card-padding: var(--mgp-space-md);
  --mgp-density-row-padding-y: var(--mgp-space-sm);
  --mgp-density-table-cell-padding: var(--mgp-space-sm);
}

[data-density="comfortable"] {
  --mgp-density-card-padding: var(--mgp-space-lg);
  --mgp-density-row-padding-y: var(--mgp-space-md);
  --mgp-density-table-cell-padding: var(--mgp-space-md);
}

[data-density="compact"] {
  --mgp-density-card-padding: var(--mgp-space-sm);
  --mgp-density-row-padding-y: var(--mgp-space-xs);
  --mgp-density-table-cell-padding: var(--mgp-space-xs);
}
```

## Designregel

> **Density skal ændre spacing og informationsmængde — ikke accessibility-baseline.**

***

# 25.12 Tokens og accessibility

Accessibility bør også have tokens.

```css
--mgp-focus-ring-color: var(--mgp-color-primary-dark);
--mgp-focus-ring-width: 3px;
--mgp-focus-ring-offset: 3px;
--mgp-touch-target-min: 44px;
```

Komponenter bør bruge dem:

```css
:focus-visible {
  outline: var(--mgp-focus-ring-width) solid var(--mgp-focus-ring-color);
  outline-offset: var(--mgp-focus-ring-offset);
}

.btn,
.icon-button {
  min-height: var(--mgp-touch-target-min);
}
```

## Designregel

> **Accessibility-værdier skal være tokens, så de ikke bliver tilfældige pr. komponent.**

***

# 25.13 Tokens og attention

Attention-systemet bør have tokens, men stadig bruge tekst som primær betydning.

```css
--mgp-attention-neutral-bg
--mgp-attention-info-bg
--mgp-attention-warning-bg
--mgp-attention-danger-bg

--mgp-attention-warning-border
--mgp-attention-danger-border
```

Eller simplere:

```css
--mgp-color-warning-bg
--mgp-color-danger-soft
--mgp-color-danger
```

## Designregel

> **Attention tokens skal understøtte systemet, men komponentens tekst skal bære betydningen.**

***

# 25.14 Component documentation format

Hver komponent i komponentordbogen bør dokumenteres ens.

Jeg foreslår denne skabelon:

```text
Komponentnavn
Formål
Brug når
Brug ikke når
Varianter
States
Density support
Accessibility notes
Mobile behavior
Print behavior
Eksempeltekst
Relaterede komponenter
```

## Eksempel: `StatusMessage`

```text
StatusMessage

Formål:
Viser inline feedback tæt på årsagen.

Brug når:
- En handling lykkedes eller fejlede
- Brugeren skal forstå en tilstand
- No-access, warning eller processing skal forklares

Brug ikke når:
- Beskeden er en kort ikke-kritisk bekræftelse, fx “Link kopieret”

Varianter:
success, info, warning, danger, processing, restricted

States:
default, loading, dismissed

Accessibility:
- Skal have tekst
- Farve må ikke stå alene
- role="alert" kun ved vigtige fejl

Mobile:
- Fuld bredde
- Handlinger stackes

Print:
- Vises kun hvis relevant for dokumentation
```

***

# 25.15 Eksempel: komponentordbog

## `EntityCard`

Bruges til generiske objekter som haver, bede, planter eller materialer.

Variants:

```text
default
highlight
archived
restricted
attention
```

States:

```text
default
selected
loading
disabled
```

Accessibility:

```text
- Heading skal være semantisk
- Hvis hele card er klikbart, skal focus state være tydelig
- Handlinger skal være buttons/links
```

***

## `CompactEntityRow`

Bruges til lange lister og søgeresultater.

Variants:

```text
default
attention
restricted
archived
```

States:

```text
selected
loading
disabled
```

Density:

```text
default
compact
```

Accessibility:

```text
- Row actions skal være tastaturtilgængelige
- Ikke kun hover-actions
```

***

## `StatusMessage`

Bruges til inline status.

Variants:

```text
success
info
warning
danger
processing
restricted
```

Accessibility:

```text
- Tekst kræves
- Ikon/farve er supplement
```

***

## `ConfirmDialog`

Bruges til bekræftelser.

Variants:

```text
standard
danger
strongConfirmation
```

Accessibility:

```text
- Dialog title
- Focus management
- Cancel + confirm
- Specific action label
```

***

## `FilterDrawer`

Bruges på mobil til filtre.

Variants:

```text
default
fullScreen
bottomSheet
```

Accessibility:

```text
- Fokus flyttes ind
- Lukkevej
- Apply/reset buttons
```

***

## `DangerZone`

Bruges til destruktive handlinger.

Variants:

```text
archive
delete
accessRemoval
```

Accessibility:

```text
- Tydelig tekst
- Ikke kun rødt
- Confirmation før destruktiv handling
```

***

# 25.16 File/folder organization

Hvis I bygger i Blazor/Razor, kunne strukturen fx være:

```text
/Components
  /Foundation
    Button.razor
    IconButton.razor
    Badge.razor
    FormField.razor

  /Feedback
    StatusMessage.razor
    Toast.razor
    GlobalBanner.razor
    EmptyState.razor
    ErrorState.razor
    NoAccessState.razor

  /DataDisplay
    EntityCard.razor
    CompactEntityRow.razor
    DataTable.razor
    MediaCard.razor
    SummaryCard.razor

  /Navigation
    ContextTabs.razor
    Pagination.razor
    ViewModeToggle.razor

  /Overlays
    ConfirmDialog.razor
    FilterDrawer.razor

  /Domain
    /Gardens
      GardenCard.razor
      GardenDetailHeader.razor
    /Plants
      PlantCard.razor
      PlantRow.razor
    /Files
      FilePreviewCard.razor
      FileUploadPanel.razor
```

## Designregel

> **Strukturen skal gøre forskel på foundations, patterns og domænekomponenter.**

***

# 25.17 CSS/token file organization

Eksempel:

```text
/wwwroot/css
  tokens.css
  base.css
  typography.css
  layout.css
  components.css
  utilities.css
  print.css
```

Eller mere opdelt:

```text
/styles
  /tokens
    colors.css
    spacing.css
    typography.css
    radius.css
    shadow.css
    motion.css
    density.css

  /components
    button.css
    badge.css
    card.css
    status-message.css
    table.css
    drawer.css
```

## Min anbefaling

Start simpelt:

```text
tokens.css
base.css
components.css
print.css
```

Split først, når filerne bliver for store.

***

# 25.18 Naming conventions for CSS classes

Jeg ville bruge en enkel komponentorienteret klasseform.

Eksempel:

```css
.status-message
.status-message--success
.status-message--warning
.status-message--danger

.entity-card
.entity-card--archived
.entity-card--restricted

.badge
.badge--warning
.badge--danger
```

Hvis I foretrækker den stil fra demoerne:

```css
.status-message.status-warning
.card.card-archived
.badge.badge-danger
```

Det er også fint. Det vigtigste er konsistens.

## Designregel

> **Vælg én class naming-stil og brug den konsekvent.**

***

# 25.19 Avoid component explosion

Risikoen med komponentordbog er, at alt bliver sin egen komponent.

Undgå dette:

```text
GardenGreenCard
GardenArchivedGreenCard
GardenArchivedSmallCard
GardenArchivedCompactCard
```

Brug hellere:

```text
GardenCard
variant="archived"
density="compact"
```

eller:

```text
EntityCard
entityType="garden"
state="archived"
density="compact"
```

## Designregel

> **Lav variants og states før du laver nye komponenter.**

***

# 25.20 Hvornår skal en komponent være domain-specific?

Lav en domænekomponent, når den har:

* domænespecifik data
* domænespecifik handling
* domænespecifik tekst
* domænespecifik permission logic
* domænespecifik relation

Eksempel:

`StatusMessage` er generisk.

`InvitationStatusCard` er domænespecifik, fordi den har:

```text
Email
Rolle
Status
ExpiresUtc
Tilbagekald invitation
Send igen
```

## Designregel

> **Domænekomponenter er tilladt, når domænet styrer indhold, handlinger eller regler.**

***

# 25.21 Komponentnavne på engelsk eller dansk?

Jeg ville vælge:

* **engelsk til kode og komponenter**
* **dansk til bruger-facing tekst**

Eksempel:

```text
Component: ArchiveConfirmDialog
Button text: Arkivér have
```

Hvorfor?

* kodekonventioner er ofte engelske
* framework/community bruger engelske navne
* domænetekst kan stadig være dansk
* komponentnavne bliver kortere og mere standardiserede

## Designregel

> **Kodekomponenter navngives på engelsk. UI-copy navngives på dansk.**

Hvis du foretrækker dansk i kodebasen, kan det også lade sig gøre — men så skal det vælges konsekvent. Min anbefaling er engelsk i komponentnavne.

***

# 25.22 Design tokens i praksis

Et samlet eksempel:

```css
:root {
  /* Color */
  --mgp-color-primary: #3F6B4A;
  --mgp-color-primary-dark: #2F5138;
  --mgp-color-primary-soft: #DDE8D8;

  --mgp-color-bg: #FAF8F2;
  --mgp-color-surface: #FFFFFF;
  --mgp-color-surface-muted: #EFEAE0;
  --mgp-color-border: #D8D2C7;

  --mgp-color-text: #243128;
  --mgp-color-text-muted: #6F766D;

  --mgp-color-danger: #9F3A38;
  --mgp-color-danger-soft: #FFF4F3;
  --mgp-color-warning-bg: #F8E8D8;

  /* Spacing */
  --mgp-space-2xs: .25rem;
  --mgp-space-xs: .5rem;
  --mgp-space-sm: .75rem;
  --mgp-space-md: 1rem;
  --mgp-space-lg: 1.5rem;
  --mgp-space-xl: 2rem;
  --mgp-space-2xl: 3rem;

  /* Radius */
  --mgp-radius-sm: .45rem;
  --mgp-radius-md: .75rem;
  --mgp-radius-lg: 1.1rem;
  --mgp-radius-pill: 999px;

  /* Shadow */
  --mgp-shadow-sm: 0 1px 2px rgba(36, 49, 40, .08);
  --mgp-shadow-md: 0 12px 28px rgba(36, 49, 40, .10);

  /* Accessibility */
  --mgp-focus-ring-color: var(--mgp-color-primary-dark);
  --mgp-focus-ring-width: 3px;
  --mgp-focus-ring-offset: 3px;
  --mgp-touch-target-min: 44px;

  /* Layout */
  --mgp-content-max: 1220px;
  --mgp-sidebar-width: 280px;

  /* Density */
  --mgp-density-card-padding: var(--mgp-space-md);
  --mgp-density-row-padding-y: var(--mgp-space-sm);
  --mgp-density-row-padding-x: var(--mgp-space-md);
  --mgp-density-table-cell-padding: var(--mgp-space-sm);
}

[data-density="comfortable"] {
  --mgp-density-card-padding: var(--mgp-space-lg);
  --mgp-density-row-padding-y: var(--mgp-space-md);
  --mgp-density-row-padding-x: var(--mgp-space-lg);
  --mgp-density-table-cell-padding: var(--mgp-space-md);
}

[data-density="compact"] {
  --mgp-density-card-padding: var(--mgp-space-sm);
  --mgp-density-row-padding-y: var(--mgp-space-xs);
  --mgp-density-row-padding-x: var(--mgp-space-sm);
  --mgp-density-table-cell-padding: var(--mgp-space-xs);
}
```

***

# 25.23 Anbefalet designbeslutning

Jeg ville låse principperne sådan:

1. **Alle gentagne UI-mønstre skal have et dokumenteret komponentnavn.**
2. **Komponentnavne beskriver formål, ikke udseende.**
3. **Komponenter opdeles i primitives, patterns og domain components.**
4. **Variants beskriver intention, fx `warning`, `danger`, `restricted` — ikke farve.**
5. **States beskriver aktuel tilstand, fx `loading`, `archived`, `processing`.**
6. **Lav variants/states før nye komponenter for at undgå component explosion.**
7. **Kodekomponenter navngives helst på engelsk; UI-copy er dansk.**
8. **Design tokens opdeles i global, semantic og component tokens.**
9. **Komponenter bør bruge semantic/component tokens, ikke rå farveværdier.**
10. **Accessibility-værdier som focus ring og touch target er tokens.**
11. **Density styres via token overrides.**
12. **Hver komponent i komponentordbogen får usage, variants, states, accessibility, mobile og print notes.**
13. **Start med få tokenfiler og split først, når systemet vokser.**

***

# Kort dokumentationstekst

Du kan bruge denne direkte:

> **Komponentnavngivning og design tokens:** MyGardenPlanner bruger en komponentordbog, hvor alle gentagne UI-mønstre har et tydeligt navn, formål, variants, states og accessibility-notes. Komponentnavne beskriver rolle frem for udseende, fx `StatusMessage`, `EntityCard`, `FilterDrawer` og `DangerZone`. Komponenter opdeles i primitives, patterns og domain components. Variants beskriver intention som `warning`, `danger` og `restricted`, mens states beskriver tilstande som `loading`, `archived` og `processing`. Design tokens opdeles i global, semantic og component tokens og bruges til farver, spacing, radius, shadow, typography, density, focus og touch targets. Density og accessibility styres via tokens, så designet forbliver konsistent på tværs af komponenter.