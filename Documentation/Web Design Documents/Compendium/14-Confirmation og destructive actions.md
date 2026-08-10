### 📄 14-Confirmation og destructive actions.md

# 14 - Confirmation og destructive actions

## 📘 Grundregel
- Destruktive handlinger skal være bevidste, kontekstuelle og proportionale med risikoniveauet. UI'et skal skelne entydigt mellem at arkivere, fjerne en relation og slette data. Confirmation-dialoger skal eksplicit navngive det berørte objekt, forklare konsekvensen, benytte handlingsspecifikke danger-labels (`.btn-danger`) placeret til højre/sidst og må aldrig love gendannelse, medmindre funktionen reelt eksisterer.

---

## 🔍 Anvendelse / varianter / typer

### Handlingstyper og Risikoniveauer

| Handlingstype | Eksempel | Risikoniveau | UI-mønster & Knaptekst | Konsekvens & Teksttone |
| :--- | :--- | :--- | :--- | :--- |
| **Arkivér (Soft delete)** | Arkivér have, arkivér bed | Lav-Middel | `MgpConfirmDialog` / `InlineConfirm` <br> `[Arkivér bed]` | Objekter skjules fra aktive oversigter, men bevares historisk og kan gendannes. |
| **Gendan** | Gendan bed, gendan have | Lav | `MgpConfirmDialog` / Direct action <br> `[Gendan bed]` | Reaktiverer et arkiveret objekt og flytter det tilbage til den aktive liste. |
| **Fjern relation** | Fjern plante fra bed, fjern materiale fra projekt | Middel | `MgpInlineConfirm` / `MgpUndoStatus` <br> `[Fjern fra bed]` | Fjerner kun tilknytningen. Masterdata forbliver uberørt. Ordet "Slet" må **ikke** anvendes. |
| **Adgang & Tokens** | Fjern medlem, tilbagekald invitation | Middel-Høj | `MgpConfirmDialog` <br> `[Fjern adgang]` / `[Tilbagekald invitation]` | Tilbagekalder rettigheder eller tokens. Forklarer præcist konsekvensen for personen og data. |
| **Slet data** | Slet fil, slet plante fra masterdata | Høj | `MgpConfirmDialog` <br> `[Slet fil]` | Fjerner data permanent. Udføres med tydelig advarsel om manglende fortrydelse. |
| **Permanent / Hard delete** | Permanent sletning af have med alle relaterede data | Meget Høj | Strong Confirmation (skriv navn) i `MgpDangerZone` <br> `[Slet permanent]` | Uafvendelig sletning af store enheder. Kræver at brugeren indtaster enhedens navn for at bekræfte. |

### Confirmation-mønstre

| UI-mønster | Brugerkontekst | Implementeringsretningslinje |
| :--- | :--- | :--- |
| **Ingen confirmation** | Ufarlige, reversible UI-handlinger (f.eks. luk preview, ryd søgning, collapse panel). | Udføres prompte uden afbrydelse. |
| **Inline confirmation (`MgpInlineConfirm`)** | Mindre, direkte handlinger i kort eller tabelrækker. | Vises direkte i rækken/kortet uden at åbne et modal-overlay. |
| **Modal confirmation (`MgpConfirmDialog`)** | Standard ved sletning af filer, fjernelse af medlemmer og arkivering. | Åbner fokuseret dialog. Kræver eksplicit valg mellem Annullér og Danger-handling. |
| **Strong confirmation** | Uafvendelig eller kritisk permanent sletning. | Kræver manuel indtastning af objektets navn (fx *"Villa Solbakken"*) før danger-knappen aktiveres. Placeres i en `MgpDangerZone`. |
| **Undo status (`MgpUndoStatus`)** | Let reversible relationshandlinger (f.eks. fjern plante fra bed). | Erstattet af transient toast/statusbesked med "Fortryd"-knap framfor en forudgående spærrende modal. |

---

## 🚫 Regler (Do / Don't)
- **Do:** Anvend altid det konkrete objektnavn i overskrift eller brødtekst (f.eks. *"Slet 'Haveskitse maj.pdf'?"* fremfor *"Slet fil?"*).
- **Do:** Brug altid specifikke, handlingsorienterede labels på bekræftelsesknappen (f.eks. *"Slet fil"*, *"Fjern adgang"*, *"Arkivér have"*) fremfor generiske *"OK"* eller *"Ja"*.
- **Do:** Placér altid afbrydelseshandlingen (`.btn-secondary`) først og den destruktive handling (`.btn-danger`) sidst/til højre.
- **Do:** Vis loading state med spinner (`.btn-spinner`) og aktiv bydeform (f.eks. *"Sletter..."*, *"Arkiverer..."*) direkte på danger-knappen under afsendelse.
- **Do:** Sikr at statusbeskeder (`MgpStatusMessage`) efter udførte handlinger altid benytter det nøjagtige samme begreb som den udførte handling (f.eks. *"arkiveret"*, *"gendannet"*, *"fjernet"* eller *"slettet"*).
- **Do:** Permission-checks skal altid udføres *før* en confirmation-dialog aktiveres. Brugeren må aldrig mødes af en bekræftelsesmodal for derefter at få afvist handlingen ved klik på bekræft. Knappen til at åbne modalen skal i stedet skjules eller deaktiveres med en forklaring.
- **Do:** Bekræft gennemførte destruktive eller irreversible handlinger med en klar, rolig inline `MgpStatusMessage` (fx *"Haven 'Villa Solbakken' blev arkiveret"*). Handlingen udgør en historisk bekræftelse og skal **ikke** permanent efterlade fladen i en rød alarmtilstand.
- **Do:** Placer destruktive handlinger på detail pages i en særskilt `MgpDangerZone` længere nede på siden frem for som primære knapper i detail headeren.
- **Do:** Mobile confirmation-dialoger skal holdes korte og konkrete med eksplicitte objektnavne, og handlingsknapper (.confirm-dialog-actions .btn) skal opstilles vertikalt i fuld bredde (width: 100%) med minimum 44px touch target for at undgå fejl-taps.
- **Do:** Bekræftelsesdialoger (`MgpConfirmDialog`) skal altid indeholde eksplicit fokusstyring: Fokus flyttes automatisk ind i dialogen ved åbning, fanges inde i dialogen (focus trap), og returneres til den udløsende knap ved annullering eller lukning.
- **Don't:** Anvend **aldrig** compact density på bekræftelsesdialoger (`MgpConfirmDialog`), danger zones eller inline slette-bekræftelser (`MgpInlineConfirm`), uanset om den omkringliggende side er i compact mode. Confirmations skal altid vises i Default eller Comfortable density med god luft for at forhindre fejlklik.
- **Don't:** Brug aldrig `.btn-primary` til destruktive handlinger, og gør ikke destruktive knapper til default submit ved Enter-tryk.
- **Don't:** Brug aldrig ordet "Slet", hvis der blot fjernes en relation (brug *"Fjern fra bed"*) eller arkiveres (brug *"Arkivér"*).
- **Don't:** Lov aldrig i UI-teksten, at en handling kan fortrydes eller gendannes, medmindre funktionen reelt findes i systemet.
- **Edge cases:** Ved midlertidige filer anvendes *"Fjern fil"* fremfor *"Slet fil"*. Permanent sletning af store samleenheder (f.eks. hele haver) må aldrig placeres direkte i oversigtskort, men skal isoleres i en dedikeret `MgpDangerZone` på indstillingssiden.

---

## 🧩 Komponentpåvirkning
- **Type:** Pattern / Domain Component
- **Nye Razor-komponenter:**
  - `MgpConfirmDialog.razor` (Modal-komponent til bekræftelsesdialoger med support for strong text-confirmation).
  - `MgpInlineConfirm.razor` (Kompakt bekræftelses-popover til lister og kort).
  - `MgpDangerZone.razor` (Afgrænset sektions-card til farlige/permanente handlinger på detalje-/indstillingssider).
  - `MgpUndoStatus.razor` (Transient besked med integreret "Fortryd"-knap til reversible relationshandlinger).
- **Ændrede Razor-komponenter:**
  - `MgpButton.razor` (Sikret support for danger-outline variant og loading state med bydeformstekst).
  - `MgpStatusMessage.razor` (Anvendes til bekræftelse af gennemført destruktiv handling i return-flowet).

---

## 🪙 Tokenpåvirkning
- `--mgp-danger`: Tekst-, ikon- og kantfarve for danger-knapper (`.btn-danger`) og advarselsikoner.
- `--mgp-danger-bg`: Blød baggrund ved hover på destruktive knapper og i `MgpDangerZone`.
- `--mgp-danger-border`: Kantfarve for modal-advarsler og destruktive sektioner.
- `--mgp-surface`: Baggrundsfarve for modal-dialoger og inline confirmation popovers.
- `--mgp-surface-muted`: Baggrund for annullér-handlinger og inaktive felter.

---

## 💻 CSS & Bootstrap

```css
/* ==========================================================================
   Confirmation & Destructive Actions Styles
   ========================================================================== */

/* Confirmation Modal Overlay & Structure */
.confirm-dialog-backdrop {
  position: fixed;
  inset: 0;
  background-color: rgba(0, 0, 0, 0.4);
  display: grid;
  place-items: center;
  z-index: 1060;
}

.confirm-dialog {
  background: var(--mgp-surface);
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-lg);
  width: 100%;
  max-width: 480px;
  padding: var(--space-md);
  display: grid;
  gap: var(--space-md);
}

.confirm-dialog-header {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.confirm-dialog-title {
  font-size: var(--font-size-lg);
  font-weight: var(--font-weight-bold);
  margin: 0;
  color: var(--mgp-text);
}

.confirm-dialog-body {
  font-size: var(--font-size-sm);
  color: var(--mgp-text-muted);
  line-height: 1.5;
}

.confirm-dialog-impact {
  margin-top: var(--space-xs);
  padding: var(--space-xs) var(--space-sm);
  background: var(--mgp-surface-muted);
  border-left: 3px solid var(--mgp-danger);
  border-radius: var(--radius-sm);
  color: var(--mgp-text);
}

.confirm-dialog-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-sm);
}

/* Inline Confirm Popover */
.inline-confirm {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  padding: var(--space-xs) var(--space-sm);
  background: var(--mgp-surface);
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-sm);
  box-shadow: var(--shadow-sm);
}

/* Danger Zone (Indstillingssider) */
.danger-zone {
  border: 1px solid var(--mgp-danger-border);
  background: var(--mgp-surface);
  border-radius: var(--radius-md);
  padding: var(--space-md);
  display: grid;
  gap: var(--space-sm);
}

.danger-zone-header {
  color: var(--mgp-danger);
  font-weight: var(--font-weight-bold);
  font-size: var(--font-size-base);
}

/* Responsiv tilpasning for Mobil confirmation */
@media (max-width: 640px) {
  .confirm-dialog {
    width: calc(100% - (2 * var(--space-md)));
    margin: var(--space-md);
  }

  .confirm-dialog-actions {
    flex-direction: column-reverse;
    width: 100%;
  }

  .confirm-dialog-actions .btn {
    width: 100%;
    justify-content: center;
  }
}
```