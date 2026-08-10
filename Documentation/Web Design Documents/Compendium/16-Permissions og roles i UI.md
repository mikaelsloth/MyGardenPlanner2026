### 📄 16-Permissions og roles i UI.md

# 16 - Permissions og roles i UI

## 📘 Grundregel
- UI’et skal afspejle brugerens faktiske adgang uden at skabe blindgyder. Backend håndhæver sikkerhedsreglerne, mens frontend forklarer dem roligt og gennemskueligt: Handlinger, brugeren aldrig kan udføre, skjules; forventede men aktuelt låste handlinger deaktiveres med forklaring (`.permission-hint`); og formularer uden skriveadgang vises som rene læsevisninger (`MgpReadOnlySection`) frem for rækker af deaktiverede feltinput.

---

## 🔍 Anvendelse / varianter / typer

### Adgangsdimensioner og UI-håndtering

| Dimension | UI-manifestation | Eksempel |
| :--- | :--- | :--- |
| **Visibility** | Må brugeren se indholdet/objektet? | Skjul fortrolige bilag helt ved manglende adgang. |
| **Actionability** | Må brugeren udføre handlingen? | Deaktivér download for Læser-rolle; skjul slet-knap helt. |
| **Explanation** | Skal UI’et forklare adgangsbegrænsningen? | Vis "Kun haveejere kan gøre midlertidige filer permanente". |

- **Filter Visibility:** Filtermuligheder og resultattællinger skal tilpasses brugerens adgang. UI'et må aldrig præsentere filterværdier i dropdowns eller filterbars, som brugeren pga. manglende permissions ikke har adgang til at se indholdet af.

- **Detail Page Modes:** Detail-sider skal eksplicit understøtte tre varianter baseret på rolle:
    1. **Owner / Editor:** Fuld adgang til redigering, fil-upload og administrationshandlinger.
    2. **Viewer / Read-only:** Alle handlinger fjernes eller deaktiveres med `.permission-hint`; formulardata vises som ren `MgpReadOnlySection`.
    3. **Restricted:** Adgang afvises med en rolig `status-restricted` besked uden at afsløre uautoriserede data.

### Strategi for handlinger: Skjul, Disable eller Restricted State

| UI-mønster | Situation | UI-udformning |
| :--- | :--- | :--- |
| **Skjul handling** | Handlingen er aldrig relevant for brugerens rolle. | Handlingen fjernes fuldstændigt fra DOM/UI (f.eks. slet- og administrationsknapper). |
| **Disable med forklaring** | Handlingen er forventet i konteksten, men midlertidigt låst, og forklaring giver værdi. | Knappen deaktiveres (`disabled`) og ledsages af en `.permission-hint` (f.eks. *"Download: Ikke tilgængelig for din rolle"*). |
| **Restricted State (No Access)** | Hele sektionen, siden eller objektet er utilgængeligt pga. manglende adgang. | Vis roligt `MgpRestrictedState` / `.empty-restricted` panel med klar forklaring og næste handling. |
| **Security-Sensitive Hiding** | Objektets eksistens er fortrolig og må ikke afsløres. | Skjul objektet helt eller vis generisk uafklaret no-access besked uden at bekræfte entitetens navngivne eksistens. |

> **Attention Note (No-Access & Restricted States):** Manglende adgang (no-access) udgør typisk en **Level 3 Attention State**, da det forhindrer brugeren i at udføre en forventet handling. Tilstanden skal dog forklares roligt som en klar rettighedstilstand (`.status-restricted`) med anvisning om næste trin (fx *"Kontakt haveejeren"*), og må ikke fremstå som en dramatisk systemfejl.

### Rolle- & Status-labels (`MgpRoleBadge`)

Menneskeligt forståelige labels skal altid erstatte interne enums eller tekniske navne i UI'et:

| Teknisk Enum / Idé | Menneskeligt UI-label | Badge Modifier |
| :--- | :--- | :--- |
| `HaveOwner` / `Owner` | Ejer | `.badge-primary` |
| `Editor` | Redaktør | `.badge-secondary` |
| `Viewer` | Læser | `.badge-muted` |
| `Customer` | Kunde | `.badge-muted` |
| `PendingInvitation` | Afventer invitation | `.badge-accent` |
| `RevokedInvitation` | Tilbagekaldt | `.badge-muted` |
| `ExpiredInvitation` | Udløbet | `.badge-danger-soft` |

---

## 🚫 Regler (Do / Don't)
- **Do:** Oversæt altid tekniske enum-navne og koder til menneskeligt forståelige sprog-labels i UI'et.
- **Do:** Gennemfør altid permission-checks *før* en confirmation-dialog åbnes (vis aldrig en bekræftelsesmodal for blot at afvise handlingen bagefter).
- **Do:** Vis formularer og sektioner uden redigeringsadgang som rene læsevisninger (`MgpReadOnlySection`) frem for lange rækker af deaktiverede inputfelter.
- **Do:** Tilbyd kun knapper som *"Anmod om adgang"*, hvis systemet og workflowet reelt understøtter anmodningsprocessen – ellers benyt *"Kontakt haveejer"* eller ren forklaring.
- **Do:** Formidl altid adgangsbegrænsninger og rollebetingede afslag i direkte kontekst – enten som en inline statusbesked (`.status-restricted`), en `.permission-hint` ved en deaktiveret knap eller via en `MgpReadOnlySection`. Permission-beskeder må **aldrig** vises som toasts, da brugeren skal kunne genlæse årsagen til sin begrænsede adgang.
- **Do:** Sikr at tilgængelige filtermuligheder og tællinger altid afspejler den data, brugeren reelt har rettigheder til at se, så blindgyder med 0 resultater pga. adgangsbegrænsning undgås.
- **Don't:** Brug ikke fejl- eller advarselsfarver (rød/danger) til almindelige rollebetingede begrænsninger.
- **Don't:** Vis ikke disabled slette- eller medlemsadministrationsknapper for brugere, der aldrig kan få adgang til handlingen – skjul dem helt.
- **Edge cases:** Ved sikkerhedsfølsomme dokumenter eller tilbud (hvor brugeren ikke må vide, at filen eksisterer) må kortet ikke vises med lås; entiteten udelades fuldstændigt fra listen.

---

## 🧩 Komponentpåvirkning
- **Type:** Architectural Rule / Pattern Component
- **Nye Razor-komponenter:**
  - `MgpPermissionGate.razor` (Dekorativ wrapper til betinget visning/deaktivering af UI-sektioner baseret på permissions).
  - `MgpRoleBadge.razor` (Badge til rolle- og adgangsstatusser med menneskelige UI-labels).
  - `MgpPermissionHint.razor` (Kompakt forklarende hjælpetekst ved deaktiverede handlinger).
  - `MgpReadOnlySection.razor` (Præsentationskomponent til læsevisning af formulardata uden brug af disabled kontrolelementer).
  - `MgpInvitationStatusCard.razor` (Kort/række til visning af adgangsinvitationer med e-mail, rolle, udløbsstatus og tilbagekaldelsesmulighed).
- **Ændrede Razor-komponenter:**
  - `MgpButton.razor` (Understøttelse af `PermissionHint` og integreret adgangskontrol).
  - `MgpEmptyState.razor` (Tilpasset med eksplicit `Restricted` variant til adgangsbegrænsede flader).
  - `MgpMediaCard.razor` (Udvidet med support for `.media-card-restricted` med synlig metadata og låst download).

---

## 🪙 Tokenpåvirkning
- `--mgp-surface`: Baggrund for læsevisningssektioner (`.read-only-section`) og restricted cards.
- `--mgp-surface-muted`: Baggrund for rollebadges og deaktiverede visningselementer.
- `--mgp-text-muted`: Farve til forklarende adgangstekster og hints.
- `--mgp-border`: Kant om read-only og adgangsbegrænsede paneler.

---

## 💻 CSS & Bootstrap

```css
/* ==========================================================================
   Permissions & Roles UI Styles
   ========================================================================== */

/* Permission Hint (Forklaring ved deaktiverede handlinger) */
.permission-hint {
  font-size: var(--font-size-xs);
  color: var(--mgp-text-muted);
  margin-top: var(--space-xs);
  display: block;
}

/* Read-Only Form Section (Erstatter deaktiverede felter) */
.read-only-section {
  display: grid;
  gap: var(--space-md);
  padding: var(--space-md);
  background: var(--mgp-surface);
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-md);
}

.read-only-field {
  display: grid;
  gap: var(--space-xs);
}

.read-only-label {
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-bold);
  color: var(--mgp-text-muted);
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.read-only-value {
  font-size: var(--font-size-base);
  color: var(--mgp-text);
  font-weight: var(--font-weight-medium);
}

/* Rolle & Adgangsbadges */
.badge-role {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  padding: 0.2em 0.65em;
  font-size: var(--font-size-xs);
  font-weight: var(--font-weight-bold);
  border-radius: 999px;
  background: var(--mgp-surface-muted);
  color: var(--mgp-text);
}

.badge-role.badge-primary {
  background: var(--mgp-primary-soft);
  color: var(--mgp-primary-dark);
}

.badge-role.badge-accent {
  background: var(--mgp-accent-soft, #FFF3E0);
  color: var(--mgp-accent-dark, #E65100);
}

.badge-role.badge-danger-soft {
  background: var(--mgp-danger-bg);
  color: var(--mgp-danger);
}
```