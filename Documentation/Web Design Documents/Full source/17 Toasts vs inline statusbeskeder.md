# 17. Toasts vs. inline statusbeskeder

## Overordnet princip

> **Inline statusbeskeder bruges til vigtig, kontekstuel eller handlingskrævende information. Toasts bruges kun til korte, ikke-kritiske bekræftelser.**

Det passer godt til MyGardenPlanner, fordi appen skal føles rolig, struktureret og pålidelig — ikke som en app hvor beskeder popper ind og ud hele tiden.

***

## 17.1 Hvad er forskellen?

| Type                    | Brug                                 | Levetid               | Placering             |
| ----------------------- | ------------------------------------ | --------------------- | --------------------- |
| **Inline statusbesked** | Vigtig eller kontekstuel information | Bliver stående        | I relevant sektion    |
| **Toast**               | Kort global bekræftelse              | Forsvinder automatisk | Typisk hjørne/top     |
| **Banner**              | Sidebred vigtig information          | Bliver stående        | Øverst i side/sektion |
| **Badge**               | Kort status på objekt                | Bliver på objektet    | På card/række/nav     |
| **Validation message**  | Felt-/formularfejl                   | Bliver til rettet     | Ved felt/formular     |

Vi har allerede brugt inline status messages flere steder i demoerne, fx efter arkivering, sletning, gendannelse, ændret rolle og no-access forklaring.

***

# 17.2 Inline statusbeskeder

Inline statusbeskeder bør være standardvalget i MyGardenPlanner.

## Brug inline status når beskeden

* hører til et bestemt objekt
* hører til en bestemt sektion
* skal kunne læses mere end én gang
* kræver handling
* forklarer hvorfor noget skete
* forklarer hvorfor noget ikke kan ske
* er resultatet af en destruktiv handling
* handler om adgang eller permissions
* handler om filter, empty/error/no-access
* handler om upload/preview/processing

## Eksempler

### Efter arkivering

```text
Haven er arkiveret
Villa Solbakken findes nu under arkiverede haver.

[Vis arkiverede]
```

### Efter sletning

```text
Filen er slettet
Haveskitse maj.pdf blev fjernet fra Villa Solbakken.
```

### Efter oprettelse skjult af filter

```text
Planten er oprettet
Den vises ikke i listen, fordi det aktive filter er “Sol”.

[Nulstil filter]
```

### Permission

```text
Du har ikke adgang til filerne
Kontakt haveejeren, hvis du skal kunne se dem.
```

## Designregel

> **Hvis beskeden forklarer en tilstand eller kræver handling, skal den være inline.**

***

# 17.3 Toasts

Toasts kan stadig være nyttige, men de bør bruges meget sparsomt.

## Brug toast når beskeden

* er kort
* ikke kræver handling
* ikke er kritisk
* blot bekræfter en handling
* ikke behøver at kunne genlæses
* ikke påvirker brugerens kontekst

## Eksempler

```text
Filen er uploadet.
```

```text
Ændringer gemt.
```

```text
Invitation sendt.
```

```text
Kopieret til udklipsholder.
```

## Designregel

> **Toast må kun bruges til beskeder, som brugeren trygt kan overse.**

Hvis brugeren skal vide det for at forstå appens tilstand, er det ikke en toast — så er det inline.

***

# 17.4 Toasts må ikke bære kritisk information

Dette er den vigtigste regel.

Dårligt:

```text
Toast:
Filen blev ikke uploadet, fordi den er for stor.
```

Hvis beskeden forsvinder, mister brugeren årsagen.

Bedre:

```text
Inline error:
Upload mislykkedes
Filen er for stor. Vælg en mindre fil.

[Vælg anden fil]
```

Dårligt:

```text
Toast:
Du har ikke adgang til filen.
```

Bedre:

```text
Inline no-access:
Du har ikke adgang til filen
Din rolle giver ikke adgang til at åbne eller downloade denne fil.
```

## Designregel

> **Fejl, adgangsbegrænsninger, validering og destruktive resultater må ikke kun vises som toast.**

***

# 17.5 Status message variants

Vi bør definere faste varianter for inline statusbeskeder.

| Variant             | Brug                            |
| ------------------- | ------------------------------- |
| `status-success`    | Handling lykkedes               |
| `status-warning`    | Noget kræver opmærksomhed       |
| `status-danger`     | Fejl eller destruktivt resultat |
| `status-info`       | Neutral information             |
| `status-processing` | Systemet arbejder               |
| `status-restricted` | Adgang begrænset                |

Eksempel:

```html
<div class="status-message status-success">
  <span class="status-dot"></span>
  <div>
    <strong>Filen er uploadet</strong>
    <p class="meta">Haveskitse maj.pdf er tilføjet til Villa Solbakken.</p>
  </div>
</div>
```

***

# 17.6 Hvor placeres inline status?

## Side-level status

Bruges øverst i main content, lige under page header/context tabs.

Eksempel:

```text
Haven er arkiveret
Villa Solbakken findes nu under arkiverede haver.
```

## Section-level status

Bruges inde i en sektion.

Eksempel:

```text
Filer kunne ikke hentes
Prøv igen.
```

## Object-level status

Bruges på et card eller en række.

Eksempel:

```text
Thumbnail oprettes…
```

## Form-level status

Bruges øverst i formularen eller under form section.

Eksempel:

```text
Gemte ændringer
Havens oplysninger er opdateret.
```

## Field-level status

Bruges ved feltet.

Eksempel:

```text
Navn er påkrævet.
```

## Designregel

> **Placér beskeden så tæt på årsagen som muligt.**

***

# 17.7 Toast-position og adfærd

Hvis vi indfører toasts, bør de være diskrete og standardiserede.

## Placering

Jeg vil anbefale:

* desktop: nederst til højre eller øverst til højre
* mobil: nederst, men ikke oven på primære handlinger

I MyGardenPlanner ville jeg hælde mod **nederst til højre på desktop** og **nederst på mobil**, fordi header/navigation allerede har meget kontekst.

## Adfærd

* vises kortvarigt
* kan lukkes manuelt
* må ikke blokere UI
* maks. 1–3 synlige ad gangen
* nyere toasts må ikke skubbe vigtig inline status væk
* fejl bør ikke auto-forsvinde, hvis de er kritiske — men så bør de nok ikke være toast

## Designregel

> **Toast må ikke dække primære knapper, formularhandlinger eller mobilnavigation.**

***

# 17.8 Toast-indhold

Toasts skal være korte.

## God toast

```text
Ændringer gemt.
```

```text
Invitation sendt.
```

```text
Link kopieret.
```

## For lang toast

```text
Invitationen til kunde@example.dk er sendt med rollen Læser og udløber om ...
```

Det bør være inline eller detaljekort.

## Designregel

> **Toast copy bør være én kort sætning.**

***

# 17.9 Toast vs. inline efter konkrete handlinger

| Handling             | Anbefalet feedback                                                         |
| -------------------- | -------------------------------------------------------------------------- |
| Gem formular         | Toast eller inline success                                                 |
| Opret plante         | Inline success + highlight                                                 |
| Upload fil           | Inline success ved filsektion; evt. toast hvis bruger forbliver samme sted |
| Slet fil             | Inline status                                                              |
| Arkivér have         | Inline status                                                              |
| Gendan have          | Inline status                                                              |
| Fjern plante fra bed | Inline status + evt. undo                                                  |
| Kopiér link          | Toast                                                                      |
| Send invitation      | Toast + invitation vises i liste                                           |
| Invitation fejlede   | Inline error                                                               |
| Download startet     | Toast kan bruges                                                           |
| No access            | Inline no-access                                                           |
| Preview ikke klar    | Inline processing                                                          |
| Thumbnail oprettes   | Object-level processing                                                    |

***

# 17.10 Toast og undo

Undo kan vises i toast i nogle apps, men jeg ville være forsigtig.

For MyGardenPlanner vil jeg anbefale:

* Undo for relationer vises inline tæt på listen.
* Toast undo kan bruges senere, hvis det er et lille globalt UI pattern.

Eksempel inline:

```text
Planten blev fjernet fra beddet.
[Fortryd]
```

Ikke nødvendigvis toast.

## Designregel

> **Undo bør placeres tæt på det objekt eller den liste, handlingen påvirker.**

***

# 17.11 Toasts og navigation state

Hvis brugeren navigerer væk, kan toast forsvinde eller blive irrelevant.

Derfor:

* return-flow beskeder bør være inline
* beskeder efter opret/rediger bør vises på destinationen
* toast må ikke være eneste spor efter handlingen

Eksempel:

```text
Tilføj plante → returnér til planteliste
Inline:
Planten er oprettet
Lavendel er markeret i listen.
```

Ikke kun:

```text
Toast:
Plante oprettet
```

## Designregel

> **Beskeder der forklarer return-flow skal være inline på destinationen.**

***

# 17.12 Accessibility

Toasts kan være svære for tilgængelighed, fordi de forsvinder og kan blive overset.

Guidelines:

* Toasts bør annonceres høfligt, ikke aggressivt.
* Kritiske fejl bør ikke auto-forsvinde.
* Toasts skal kunne lukkes.
* Inline status er bedre til beskeder, brugeren skal kunne læse i eget tempo.
* Statusbeskeder skal ikke kun bruge farve.

## Designregel

> **Vigtig information skal være persistent og læsbar i brugerens eget tempo.**

***

# 17.13 Attention management

Dette hænger sammen med punkt #14, men vi kan allerede formulere en regel:

> **Jo vigtigere beskeden er, desto mere persistent og kontekstuel skal den være.**

| Vigtighed     | Pattern                      |
| ------------- | ---------------------------- |
| Lav           | Toast                        |
| Middel        | Object/section inline        |
| Høj           | Page-level inline            |
| Kritisk       | Persistent inline + handling |
| System/global | Banner                       |

Eksempel:

* “Link kopieret” → toast
* “Filen er uploadet” → inline i filsektion eller toast, afhængigt af kontekst
* “Upload fejlede” → inline error
* “Du har ikke adgang” → inline no-access
* “Betaling kræver handling” → persistent banner/status

***

# 17.14 Komponenter/patterns

Jeg ville definere disse:

## `StatusMessage`

Persistent inline besked.

```text
variant: success | warning | danger | info | processing | restricted
scope: page | section | object | form
```

## `Toast`

Kort global besked.

```text
variant: success | info
autoDismiss: true
```

Jeg ville undgå danger-toasts i første omgang.

## `StatusBanner`

Hvis noget påvirker hele siden eller kontoen.

```text
Din adgang udløber snart
```

## `FieldMessage`

Feltvalidering.

## `ObjectStatus`

Status på card/række.

***

# 17.15 CSS-retning

## Inline status

```css
.status-message {
  display: flex;
  gap: var(--space-sm);
  align-items: start;
  padding: var(--space-sm);
  border-radius: var(--radius-md);
  border: 1px solid var(--mgp-border);
  background: var(--mgp-surface);
}

.status-success .status-dot {
  background: var(--mgp-primary);
}

.status-warning .status-dot {
  background: var(--mgp-accent);
}

.status-danger .status-dot {
  background: var(--mgp-danger);
}
```

## Toast

```css
.toast-stack {
  position: fixed;
  right: var(--space-lg);
  bottom: var(--space-lg);
  display: grid;
  gap: var(--space-sm);
  z-index: 50;
  max-width: min(360px, calc(100vw - 2rem));
}

.toast {
  background: var(--mgp-surface);
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-md);
  padding: var(--space-sm) var(--space-md);
}
```

Mobil:

```css
@media (max-width: 640px) {
  .toast-stack {
    left: var(--space-md);
    right: var(--space-md);
    bottom: var(--space-md);
  }
}
```

***

# 17.16 Beslutningstabel

| Situation                    |                Toast |          Inline |
| ---------------------------- | -------------------: | --------------: |
| Link kopieret                |                   Ja |             Nej |
| Små ændringer gemt           |     Ja / evt. inline |  Ja hvis vigtig |
| Fil uploadet                 |                 Evt. | Ja i filsektion |
| Upload fejlede               |                  Nej |              Ja |
| Slet fil                     |                  Nej |              Ja |
| Arkivér have                 |                  Nej |              Ja |
| No access                    |                  Nej |              Ja |
| Validation error             |                  Nej |              Ja |
| Invitation sendt             | Ja + listeopdatering |            Evt. |
| Invitation fejlede           |                  Nej |              Ja |
| Thumbnail processing         |                  Nej |    Ja på objekt |
| Return-flow efter oprettelse |                  Nej |              Ja |

***

# Anbefalet designbeslutning

Jeg ville låse principperne sådan:

1. **Inline statusbeskeder er standard i MyGardenPlanner.**
2. **Toasts bruges kun til korte, ikke-kritiske bekræftelser.**
3. **Vigtige beskeder må ikke kun vises som toast.**
4. **Fejl, no-access, validering, uploadfejl og destructive results skal være inline.**
5. **Toasts skal være korte, diskrete og kunne lukkes.**
6. **Toast må ikke dække primære handlinger, især på mobil.**
7. **Status placeres så tæt på årsagen som muligt.**
8. **Return-flow beskeder skal være inline på destinationen.**
9. **Undo bør være inline tæt på objektet/listen.**
10. **Farve understøtter status, men teksten bærer betydningen.**
11. **Start med inline statusbeskeder; tilføj toast-komponent senere hvis behovet viser sig.**

***

# Kort dokumentationstekst

Du kan bruge denne direkte:

> **Toasts vs. inline statusbeskeder:** Inline statusbeskeder er standard for vigtig, kontekstuel eller handlingskrævende feedback. De bruges til fejl, no-access, uploadstatus, destructive actions, return-flow og beskeder, brugeren skal kunne genlæse. Toasts bruges kun til korte, ikke-kritiske bekræftelser som “Ændringer gemt” eller “Link kopieret”. Vigtig information må ikke kun vises som toast, fordi toasts forsvinder. Statusbeskeder placeres så tæt på årsagen som muligt, og undo-handlinger bør vises inline ved det objekt eller den liste, de påvirker.