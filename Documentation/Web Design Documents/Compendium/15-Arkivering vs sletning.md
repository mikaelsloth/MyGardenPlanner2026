### 📄 15-Arkivering vs sletning.md

# 15 - Arkivering vs. sletning

## 📘 Grundregel
- Arkivering skjuler uvirksomme eller historiske objekter fra det aktive arbejdsrum uden at ødelægge historikken. Sletning er en destruktiv handling, der kun anvendes, når data reelt skal fjernes permanent. UI'et skal anvende begreberne entydigt: "Arkivér" (inaktiver med gendannelsesmulighed), "Gendan" (reaktiver), "Fjern" (afkobl relation uden at slette masterdata) og "Slet" (permanent fjernelse af data).

---

## 🔍 Anvendelse / varianter / typer

### Begrebsafklaring og Action Matrix

| Begreb | Betydning & Konsekvens | Eksempel | Primær UI-Handling | Confirmation-niveau |
| :--- | :--- | :--- | :--- | :--- |
| **Arkivér** | Skjul fra aktive visninger, bevar data historisk. | Arkivér have / bed | `[Arkivér have]` | Mild modal confirmation |
| **Gendan** | Flyt arkiveret objekt tilbage til aktiv visning. | Gendan bed | `[Gendan bed]` | Let confirmation / Direkte med statusbesked |
| **Fjern** | Fjern en tilknytning/relation, bevar masterdata. | Fjern plante fra bed | `[Fjern fra bed]` | Inline confirmation / Undo status |
| **Slet** | Permanent sletning af en datatransaktion/fil. | Slet fil | `[Slet fil]` | Modal confirmation (klare konsekvenser) |
| **Slet permanent** | Hard delete af samleenhed og alle underdata. | Permanent sletning af have | `[Slet permanent]` | Strong confirmation (skriv navne-match) i `MgpDangerZone` |

- `MgpDangerZone` på detail pages kan være sammenfoldet (collapsed) som default for at reducere støj, men skal have en krystalklar og overskuelig overskrift (*"Arkivering og sletning"*), så brugeren aldrig skal gætte sig til placeringen.

---

### Visning og Filter-mønster for Arkiverede Objekter
- **Standardadfærd:** Aktive oversigter viser kun aktive enheder som standard.
- **Filter-adfærd:** Arkiverede enheder håndteres som et eksplicit scope-filter (`MgpArchiveFilter` eller *Status: Aktive / Arkiverede / Alle*) i filterbaren. Som standard vises kun aktive enheder. Skift af scope gemmes i URL-state (`?status=archived`).
- **Visuel fremtræden:** Arkiverede kort (`.card-archived`) dæmpes visuelt, får en neutral status-badge (`.badge-archived`) og fremhæver "Gendan" som den primære interaktion.

> **Attention Note (Arkiveret tilstand):** Arkiverede entiteter udgør en **Level 1 Attention State** (passiv/dæmpet information). De skal vises med et muted badge (`.badge-archived`) og dæmpet visning, men må **aldrig** markeres med advarsels- (orange) eller fejlfarver (rød).

---

### Særlige regler for Filer (v1 Livscyklus)
- Filer styres primært af deres **lifetime/status** (midlertidig, permanent, udløber) og **ikke** af en "Arkivér fil"-funktion.
- Standardhandlinger for filer i v1 er: `Gør permanent`, `Fjern fil` (midlertidig) og `Slet fil` (permanent). Arkivering af filer indføres først, hvis der opstår et eksplicit dokumentarkiv-behov.

---

## 🚫 Regler (Do / Don't)
- **Do:** Brug altid "Fjern", hvis handlingen blot afkobler et objekt fra en relation (fx en plante fra et bed) – masterdata forbliver i systemet.
- **Do:** Skjul arkiverede objekter i aktive oversigter som standard og kræv et aktivt filter for at vise dem.
- **Do:** Placer altid permanent sletning af store enheder (fx en hel have) i en dedikeret `MgpDangerZone` under indstillinger.
- **Do:** Ledsag enhver gennemført arkivering, gendannelse, sletning eller relation-fjernelse af en inline `MgpStatusMessage` på listen/siden. Beskeden skal benytte det præcise begreb for handlingen (fx *"Haven 'Villa Solbakken' blev arkiveret. [Vis arkiverede]"* eller *"Planten blev fjernet fra bedet. [Fortryd]"*).
- **Don't:** Brug aldrig ordet "Slet", når noget blot inaktiveres, arkiveres eller afkobles.
- **Don't:** Brug ikke "Deaktivér" eller "Slå fra", hvis der reelt menes "Arkivér".
- **Don't:** Lov aldrig i UI-teksten, at et objekt kan "Gendannes", medmindre funktionen reelt er bygget og understøttet i backend.
- **Edge cases:** 
  - **Direkte link til arkiveret objekt:** Åbnes et arkiveret objekt direkte via URL, vises en fremtrædende arkiv-banner øverst (`.archive-banner`) med status forklaring og en direkte `[Gendan]` handling.
  - **Arkivering med aktive relationer:** Hvis en have arkiveres, bevares tilknyttede medlemmer og filer historisk, men inaktiveres i aktive arbejdsrum.

---

## 🧩 Komponentpåvirkning
- **Type:** Pattern Component / Architectural Rule
- **Nye Razor-komponenter:**
  - `MgpArchivedBadge.razor` (Badge til indikering af arkiveret tilstand).
  - `MgpArchiveFilter.razor` (Filterchip/toggle til at skifte mellem Aktive / Arkiverede / Alle).
  - `MgpArchiveBanner.razor` (Kontekstuel advarsels- og gendannelsesbanner ved direkte visning af et arkiveret objekt).
- **Ændrede Razor-komponenter:**
  - `MgpCard.razor` (Opdateret med support for varianten `.card-archived`).
  - `MgpConfirmDialog.razor` (Support for arkiverings- og gendannelsestekster samt konsekvensforklaringer).
  - `MgpDangerZone.razor` (Skarp adskillelse mellem arkiverings- og permanente slettehandlinger).

---

## 🪙 Tokenpåvirkning
- `--mgp-state-archived`: Anvendes til tekst, ikoner og kantoverskridelse på arkiverede badges og banners (`#8A8F86`).
- `--mgp-surface-muted`: Baggrundsfarve på arkiverede kort og dæmpede badges.
- `--mgp-text-muted`: Muted tekstfarve på inaktive/arkiverede elementer.

---

## 💻 CSS & Bootstrap

```css
/* ==========================================================================
   Archived State & UI Component Styles
   ========================================================================== */

/* Card Archived State */
.card-archived {
  background: var(--mgp-surface-muted);
  color: var(--mgp-text-muted);
  border-color: var(--mgp-border);
  opacity: 0.90;
}

.card-archived .card-title {
  color: var(--mgp-text-muted);
}

/* Badge Archived State */
.badge-archived {
  color: var(--mgp-state-archived);
  background: var(--mgp-surface);
  border: 1px solid var(--mgp-border);
  font-weight: 600;
}

/* Context Banner ved direkte visning af arkiverede objekter */
.archive-banner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-md);
  padding: var(--space-sm) var(--space-md);
  background: var(--mgp-surface-muted);
  border: 1px solid var(--mgp-border);
  border-left: 4px solid var(--mgp-state-archived);
  border-radius: var(--radius-md);
  margin-bottom: var(--space-md);
  color: var(--mgp-text);
}

.archive-banner-text {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
  font-size: var(--font-size-sm);
}

@media (max-width: 640px) {
  .archive-banner {
    flex-direction: column;
    align-items: flex-start;
  }
}
```