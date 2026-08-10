### 📄 03-Spacing.md

# 03 - Spacing

## 📘 Grundregel
Anvend et strengt 4px/8px baseret spacing-system til alt layout, padding, margin og gap. 16px (`--mgp-space-md`) er standarden for kort og formularfelter, 24px (`--mgp-space-lg`) for overordnede sektioner og 32px (`--mgp-space-xl`) for hovedsektionsadskillelse. Vilkårlige værdier (f.eks. `0.7rem`, `1.25rem`) er forbudt.

---

## 🔍 Anvendelse / varianter / typer

### Spacing Skala (4px / 8px Rytme)

| Spacing Token | Pixels | Rem | Primær Anvendelse |
| :--- | :--- | :--- | :--- |
| `--mgp-space-2xs` | 4px | `0.25rem` | Kritiske mikro-afstande, `.stack-2xs`, tætte elementer. |
| `--mgp-space-xs` | 8px | `0.5rem` | Badge-rækker, tætte knap-grupper, `.stack-xs`. |
| `--mgp-space-sm` | 12px | `0.75rem` | Kompakte cards (`.card-sm`), header vertical padding. |
| `--mgp-space-md` | 16px | `1.00rem` | Standard card-padding, grid gap, feltmarginer, `.stack-md`. |
| `--mgp-space-lg` | 24px | `1.50rem` | Store summary cards (`.card-lg`), sidebar padding, sektionsgaps. |
| `--mgp-space-xl` | 32px | `2.00rem` | Mellem hovedsektioner på siden, side-y padding på desktop. |
| `--mgp-space-2xl` | 48px | `3.00rem` | Mellem store arbejdsblokke / domæneområder. |
| `--mgp-space-3xl` | 64px | `4.00rem` | Maksimal adskillelse i brede viewports. |

### Data Density & Informationstæthed

| Density Level | Token | Container Padding | Anvendelse |
| :--- | :--- | :--- | :--- |
| **Comfortable** | `--mgp-density-comfortable` | `16px` (`1rem`) | Landing pages, dashboards og visuel browsing. |
| **Default** | `--mgp-density-default` | `12px` (`0.75rem`) | Standard entity cards og formularsektioner. |
| **Compact** | `--mgp-density-compact` | `6px 12px` (`0.375rem 0.75rem`) | Compact rows, datatabeller, søge- og filterresultater. |

> **Bemærkning til density:** Spacing-skalaen (`--mgp-space-*`) udgør den faste design-foundation. Konkret komponent-spacing styrles dynamisk via density-tokens (`[data-density="comfortable|default|compact"]`), som overstyrer `--mgp-card-padding`, `--mgp-row-padding-y` og `--mgp-section-gap` i den givne kontekst (jf. `23-Data density og compact mode.md`).

### Layout & Sektioner

| Kontekst | Desktop Værdi | Mobil Værdi (`< 640px`) |
| :--- | :--- | :--- |
| **Main Content Padding** | `32px 24px 48px` | `24px 16px 32px` |
| **Standard Card Padding** | `16px` (`--mgp-space-md`) | `12px` (`--mgp-space-sm`) |
| **Large / Hero Card Padding** | `24px` (`--mgp-space-lg`) | `16px` (`--mgp-space-md`) |
| **Grid Gap (Standard)** | `16px` (`--mgp-space-md`) | `16px` (`--mgp-space-md`) |
| **Form Felt Separation** | `16px` margin-bottom | `16px` margin-bottom |

---

## 🚫 Regler (Do / Don't)
- **Do:** Brug layout-utility klasserne `.stack-xs`, `.stack-sm`, `.stack-md`, `.stack-lg` i Blazor-komponenter til at håndtere vertikal spacing i stedet for tilfældige marginer.
- **Do:** Brug de responsive sidemarkører `--mgp-page-x` og `--mgp-page-y` til at sikre ensartet padding omkring hovedindhold.
- **Don't:** Brug ikke tilfældige eller vilkårlige rem/px-størrelser (f.eks. `0.7rem`, `0.85rem` eller `2.4rem`).
- **Don't:** Gør ikke formularer for kompakte. Oprethold minimum 16px mellem felter og 32px mellem formularsektioner.
- **Edge cases:** På mobilskærme (`< 640px`) nedskaleres standard card-padding fra 16px til 12px for at maksimere læsearealet.

---

## 🧩 Komponentpåvirkning
- **Type:** Foundation / Pattern
- **Nye Razor-komponenter:**
  - `<MgpStack>`: Hjælpekomponent til opbygning af vertikale layouts med ensartet spacing.
- **Ændrede komponenter:** Existerende layouts og kort-komponenter opdateres med de nye spacing-tokens.

```razor
@* Eksempel på MgpStack.razor *@
<div class="stack-@Gap.ToString().ToLower() @CssClass">
    @ChildContent
</div>

@code {
    public enum StackGap { TwoXs, Xs, Sm, Md, Lg, Xl }

    [Parameter] public StackGap Gap { get; set; } = StackGap.Md;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string CssClass { get; set; } = string.Empty;
}
```

---

## 🪙 Tokenpåvirkning
Etablering af spacing-tokens i `:root`:
- `--mgp-space-2xs` (4px), `--mgp-space-xs` (8px), `--mgp-space-sm` (12px), `--mgp-space-md` (16px), `--mgp-space-lg` (24px), `--mgp-space-xl` (32px), `--mgp-space-2xl` (48px), `--mgp-space-3xl` (64px)
- Dynamic page margin tokens: `--mgp-page-x`, `--mgp-page-y`

---

## 💻 CSS & Bootstrap

```css
:root {
  /* Spacing Scale (4/8px rhythm) */
  --mgp-space-2xs: 0.25rem; /* 4px */
  --mgp-space-xs:  0.5rem;  /* 8px */
  --mgp-space-sm:  0.75rem; /* 12px */
  --mgp-space-md:  1rem;    /* 16px */
  --mgp-space-lg:  1.5rem;  /* 24px */
  --mgp-space-xl:  2rem;    /* 32px */
  --mgp-space-2xl: 3rem;    /* 48px */
  --mgp-space-3xl: 4rem;    /* 64px */

  /* Responsive Page Margins */
  --mgp-page-x: var(--mgp-space-lg);
  --mgp-page-y: var(--mgp-space-xl);

  /* Data Density Tokens */
  --mgp-density-comfortable: var(--mgp-space-md);
  --mgp-density-default:     var(--mgp-space-sm);
  --mgp-density-compact:     0.375rem var(--mgp-space-sm);
}

@media (max-width: 640px) {
  :root {
    --mgp-page-x: var(--mgp-space-md);
    --mgp-page-y: var(--mgp-space-lg);
  }
}

/* Page Layout */
main {
  padding: var(--mgp-page-y) var(--mgp-page-x) var(--mgp-space-2xl);
}

.sidebar {
  padding: var(--mgp-space-lg);
}

.header-inner {
  padding: var(--mgp-space-sm) var(--mgp-page-x);
}

/* Element & Component Spacing */
.section {
  margin-top: var(--mgp-space-xl);
}

.card {
  padding: var(--mgp-space-md);
}

.card-lg {
  padding: var(--mgp-space-lg);
}

.card-sm {
  padding: var(--mgp-space-sm);
}

@media (max-width: 640px) {
  .card {
    padding: var(--mgp-space-sm);
  }
}

.grid {
  display: grid;
  gap: var(--mgp-space-md);
}

.field {
  margin-bottom: var(--mgp-space-md);
}

/* Stack Layout Utilities */
.stack-2xs { display: grid; gap: var(--mgp-space-2xs); }
.stack-xs  { display: grid; gap: var(--mgp-space-xs); }
.stack-sm  { display: grid; gap: var(--mgp-space-sm); }
.stack-md  { display: grid; gap: var(--mgp-space-md); }
.stack-lg  { display: grid; gap: var(--mgp-space-lg); }
.stack-xl  { display: grid; gap: var(--mgp-space-xl); }
```