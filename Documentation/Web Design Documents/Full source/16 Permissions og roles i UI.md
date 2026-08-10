# 16. Permissions og roles i UI

## Overordnet princip

> **UI’et skal vise brugeren det, de har brug for — og kun de handlinger, de meningsfuldt kan udføre. Manglende adgang skal forklares roligt, når forklaringen hjælper brugeren.**

Det betyder, at permissions ikke kun er backend-sikkerhed. Det er også et UI-designspørgsmål.

Backend skal naturligvis håndhæve reglerne, men frontend skal gøre brugeroplevelsen forståelig.

***

## 16.1 Permissions handler om tre ting

Jeg vil skelne mellem:

1. **Visibility** — må brugeren se indholdet?
2. **Actionability** — må brugeren udføre handlingen?
3. **Explanation** — skal UI’et forklare hvorfor noget ikke kan gøres?

Eksempel:

```text
Brugeren kan se filens metadata,
men må ikke downloade originalfilen.
```

Det kræver et andet UI end:

```text
Brugeren må slet ikke vide, at filen findes.
```

***

# 16.2 Skjul, disable eller forklar?

Det vigtigste princip er at vælge rigtigt mellem **skjult**, **disabled** og **forklaret**.

## Skjul handlingen når

Handling aldrig er relevant for brugerens rolle.

Eksempler:

* En viewer/kunde skal måske ikke se “Slet have”.
* En bruger uden medlemsadministration skal måske ikke se “Fjern medlem”.
* En bruger uden filrettigheder skal måske ikke se “Slet fil”.

```text
Handling findes ikke i UI’et.
```

## Disable handlingen når

Handlingen normalt findes i konteksten, men ikke kan bruges lige nu — og forklaringen hjælper.

Eksempler:

```text
Download
Ikke tilgængelig for din rolle
```

```text
Gør permanent
Kun haveejere kan gøre midlertidige filer permanente.
```

## Forklar som state når

Hele sektionen eller objektet ikke kan vises pga. adgang.

Eksempel:

```text
Du har ikke adgang til filerne
Din rolle giver ikke adgang til at se eller downloade filer i denne have.

[Kontakt haveejer]
```

## Designregel

> **Skjul irrelevante handlinger. Disable forventede handlinger, hvis forklaring skaber værdi. Brug no-access state, når hele indholdsområdet er utilgængeligt.**

***

# 16.3 No access er ikke en fejl

Dette hænger direkte sammen med Demo12.

Manglende adgang er ikke:

```text
Fejl
```

Det er heller ikke:

```text
Ingen data
```

Det er:

```text
Du har ikke adgang
```

eller i en mere diskret/sikker variant:

```text
Ingen filer tilgængelige
Der er ingen filer, du kan se med den aktuelle adgang.
```

## Designregel

> **No access må ikke ligne error eller empty.**

No access skal være rolig, respektfuld og tydelig.

***

# 16.4 Rollesprog skal være brugerforståeligt

Interne enum-navne og tekniske roller bør ikke nødvendigvis vises direkte.

Hvis der findes tekniske roller som fx `HaveOwner`, bør UI’et oversætte det til et menneskeligt label.

Eksempler:

| Teknisk idé        | UI-label      |
| ------------------ | ------------- |
| Owner              | Ejer          |
| Editor             | Redaktør      |
| Viewer             | Læser         |
| Customer           | Kunde         |
| Pending invitation | Afventer svar |
| Revoked invitation | Tilbagekaldt  |
| Expired invitation | Udløbet       |

Jeg kender ikke den endelige liste over `HaveRolle`-værdier fra de viste snippets, så ovenstående er et **UI-forslag**, ikke en konstatering af dine faktiske enums.

## Designregel

> **UI’et skal bruge rollelabels, som brugeren forstår — ikke tekniske enum-navne.**

***

# 16.5 Rolle-badges

Roller bør vises som rolige badges.

Eksempel:

```html
<span class="badge badge-primary">Ejer</span>
<span class="badge badge-muted">Læser</span>
<span class="badge badge-accent">Afventer invitation</span>
```

## Brug badges til

* rolle
* invitationstatus
* adgangsniveau
* midlertidig/permanent filstatus
* begrænset adgang

## Undgå

* mange farver
* rødt til almindelige rollebegrænsninger
* badges uden tekst

## Designregel

> **Rolle og adgang bør vises med tekstbaserede badges — ikke kun ikoner eller farver.**

***

# 16.6 Permissions i navigation

Navigation bør også være permission-aware.

## Skjul hele nav-punkter når

Brugeren aldrig kan bruge området.

Eksempel:

* Hvis brugeren ikke må administrere medlemmer, kan “Medlemmer” måske stadig være synligt som læsevisning — men “Invitér” skjules.
* Hvis brugeren ikke må se filer, kan “Filer” enten skjules eller vises med no-access state afhængigt af produktbeslutning.

## Vis nav-punkt men begræns handlinger når

Brugeren godt må se sektionen, men ikke administrere den.

Eksempel:

```text
Filer
- kan se metadata
- kan ikke downloade original
```

## Designregel

> **Navigation skal afspejle adgang uden at skabe blindgyder. Hvis et område vises, skal der være en forståelig tilstand inde i området.**

***

# 16.7 Permissions i lister

I lister bør handlinger tilpasses rolle og kontekst.

## Eksempel: filrække

```text
Haveskitse maj.pdf
PDF · 2,4 MB

[Vis] [Download] [Slet]
```

For en bruger uden download-ret:

```text
Haveskitse maj.pdf
PDF · 2,4 MB

[Vis] [Download disabled: Ikke tilgængelig for din rolle]
```

For en bruger uden indholdsadgang:

```text
Haveskitse maj.pdf
PDF · Metadata

[Adgang begrænset]
```

eller:

```text
Ingen filer tilgængelige
```

hvis filens eksistens ikke bør afsløres.

## Designregel

> **Objektkort og lister skal kunne vise fuld adgang, begrænset adgang og ingen adgang uden at layoutet bryder sammen.**

***

# 16.8 Permissions i formularer

I formularer kan brugeren måske se data, men ikke redigere.

Der bør være tre mulige modes:

| Mode            | Brug                                          |
| --------------- | --------------------------------------------- |
| Edit mode       | Brugeren kan redigere                         |
| Read-only mode  | Brugeren kan se, men ikke ændre               |
| Restricted mode | Brugeren kan ikke se sektionen eller felterne |

## Read-only mode

```text
Havens navn
Villa Solbakken
```

Ikke disabled inputs overalt, hvis formularen slet ikke kan redigeres.

## Disabled field

Bruges hvis feltet normalt kan redigeres, men lige nu ikke kan.

```text
Rolle
[Ejer]
Kun haveejeren kan ændre ejerrollen.
```

## Designregel

> **Hvis hele formularen ikke kan redigeres, vis den som læsevisning. Brug disabled felter kun, når forklaring på feltniveau giver værdi.**

***

# 16.9 Permissions i actions

Vi bør definere action-visibility sådan:

| Handling               | Hvis tilladt       | Hvis ikke tilladt                      |
| ---------------------- | ------------------ | -------------------------------------- |
| Opret                  | Vis primary action | Skjul eller vis no-access info         |
| Redigér                | Vis knap           | Skjul hvis aldrig relevant             |
| Slet                   | Vis danger action  | Skjul for roller uden sletteret        |
| Download               | Vis knap           | Disable hvis forventet, ellers skjul   |
| Gør permanent          | Vis knap           | Disable med forklaring hvis relevant   |
| Invitér                | Vis knap           | Skjul eller vis “Du kan ikke invitere” |
| Fjern medlem           | Vis danger action  | Skjul                                  |
| Tilbagekald invitation | Vis danger action  | Skjul                                  |

## Designregel

> **Destruktive og administrative handlinger bør som hovedregel skjules for brugere, der ikke kan udføre dem.**

***

# 16.10 Request access / kontakt ejer

Når brugeren mangler adgang, kan UI’et tilbyde næste handling — men kun hvis workflowet findes.

Muligheder:

```text
[Kontakt haveejer]
```

```text
[Anmod om adgang]
```

```text
[Se dine roller]
```

Men hvis der ikke findes en faktisk anmodningsfunktion, bør UI’et ikke love den.

## Designregel

> **Tilbyd kun “Anmod om adgang”, hvis produktet faktisk understøtter det. Ellers brug forklarende tekst eller “Kontakt haveejer”.**

***

# 16.11 Invitationer

Invitationer har deres egen UI-logik.

Layer 1 viser, at invitationer har bl.a. `Email`, `Rolle`, `Status`, `CreatedUtc`, `ExpiresUtc`, `AcceptedByUserId` og `AcceptedUtc`. [\[DesignLayer1 | Txt\]](https://onedrive.live.com/?id=ce72053c-4468-4bab-bac8-26e82ca45611\&cid=64031c85d39bc2d5\&web=1)

Det betyder UI’et bør kunne vise:

* hvem invitationen er sendt til
* hvilken rolle invitationen giver
* status
* udløb
* hvem der har accepteret, hvis relevant
* mulighed for at tilbagekalde, hvis tilladt

## Eksempel

```text
kunde@example.dk
Rolle: Læser
Status: Afventer

[Tilbagekald invitation]
```

## Designregel

> **Invitationer skal vises som adgangsobjekter med rolle og status — ikke bare som emailadresser.**

***

# 16.12 Medlemsliste

For medlemmer bør UI’et vise:

* navn/email
* rolle
* status
* handlinger afhængigt af brugerens egen rolle

Eksempel:

```text
Anne Solbakken
Kunde · Aktiv

[Skift rolle] [Fjern adgang]
```

For bruger uden administrationsret:

```text
Anne Solbakken
Kunde · Aktiv
```

Ingen administrative handlinger.

## Designregel

> **Medlemslisten skal være læsbar for relevante brugere, men administrationshandlinger skal være rollebegrænsede.**

***

# 16.13 Permissions og destructive actions

Dette hænger sammen med Demo13.

Hvis brugeren ikke må slette, skal de ikke lokkes hen til en confirmation-dialog.

Dårligt:

```text
[Slet fil]
→ Du har ikke adgang
```

Bedre:

```text
Slet fil-knappen vises ikke
```

eller hvis handlingen er forventet:

```text
[Slet fil disabled]
Kun haveejere kan slette filer.
```

## Designregel

> **Permission-checks skal ske før confirmation, ikke efter brugeren har forsøgt at bekræfte.**

***

# 16.14 Permissions og arkivering

Arkivering bør også være rollebegrænset.

Eksempel:

* Ejer kan arkivere have
* Editor kan måske arkivere bed
* Viewer/kunde kan ikke arkivere

Jeg kender ikke dine endelige rolle-regler, så dette er et **forslag til beslutningsområde**, ikke en konstatering.

## UI-princip

Hvis brugeren ikke må arkivere:

* Skjul “Arkivér” i almindelige actions.
* Vis evt. forklaring i settings/danger zone, hvis brugeren forventer handlingen.

***

# 16.15 Security-sensitive visibility

Nogle gange skal UI’et ikke afsløre, at noget findes.

Eksempel:

```text
Privat tilbud.pdf
```

Hvis en bruger ikke må vide, at tilbuddet findes, skal filen ikke vises som locked card.

I stedet:

```text
Ingen filer tilgængelige
```

## Designregel

> **No-access UI skal skelne mellem “må ikke åbne” og “må ikke vide at det findes”.**

Dette er en vigtig produkt-/sikkerhedsbeslutning.

***

# 16.16 Komponenter/patterns

Jeg ville definere følgende komponentmønstre:

## `PermissionGate`

Konceptuelt wrapper til at afgøre om indhold/action vises.

```text
show
hide
disabled-with-reason
restricted-state
```

## `RestrictedState`

No-access panel.

```text
Du har ikke adgang til filerne
Kontakt haveejeren, hvis du skal kunne se dem.
```

## `RoleBadge`

```text
Ejer
Redaktør
Læser
Kunde
```

## `PermissionHint`

Kort forklaring ved disabled action.

```text
Kun haveejere kan slette filer.
```

## `ReadOnlySection`

Sektion hvor data vises, men ikke kan redigeres.

## `InvitationStatusCard`

Viser email, rolle, status og handlinger.

***

# 16.17 Beslutningstabel

| Situation                                | UI                                     |
| ---------------------------------------- | -------------------------------------- |
| Bruger kan se og redigere                | Vis normal edit UI                     |
| Bruger kan se, men ikke redigere         | Vis read-only UI                       |
| Bruger kan se objekt, men ikke handling  | Skjul eller disable action med grund   |
| Bruger kan se metadata, men ikke indhold | Vis restricted card                    |
| Bruger må ikke kende objektet            | Skjul objektet / generisk no-access    |
| Bruger kan administrere medlemmer        | Vis rolle- og adgangshandlinger        |
| Bruger kan ikke administrere medlemmer   | Vis medlemsdata uden actions           |
| Invitation afventer                      | Vis statusbadge og evt. tilbagekald    |
| Invitation udløbet/tilbagekaldt          | Dæmpet status, ingen accepter-handling |

***

# 16.18 Anbefalet designbeslutning

Jeg ville låse principperne sådan:

1. **Permissions skal påvirke både indhold, navigation og handlinger.**
2. **Backend håndhæver adgang; frontend forklarer den forståeligt.**
3. **Skjul handlinger, brugeren aldrig kan udføre.**
4. **Disable handlinger kun når forklaring giver værdi.**
5. **No access er ikke en error-state.**
6. **Read-only data bør vises som læsevisning, ikke som disabled formular overalt.**
7. **Administrative og destruktive actions skal permission-checkes før confirmation.**
8. **Invitationer skal vise rolle og status tydeligt.**
9. **Rollelabels skal være brugerforståelige, ikke tekniske.**
10. **No-access UI må ikke afsløre følsomme objekter, hvis brugeren ikke må vide de findes.**
11. **Permission states skal kunne fungere i cards, rows, detail pages, forms og media previews.**
12. **UI’et må ikke tilbyde “Anmod om adgang”, hvis workflowet ikke findes.**

***

# Kort dokumentationstekst

Du kan bruge denne direkte:

> **Permissions og roles i UI:** UI’et skal afspejle brugerens adgang uden at skabe blindgyder. Handlinger, som brugeren aldrig kan udføre, skjules som udgangspunkt. Handlinger kan vises disabled med forklaring, hvis brugeren forventer dem og forklaringen skaber værdi. No-access states skal være tydeligt adskilt fra empty og error states. Hvis brugeren kan se data, men ikke redigere, bør UI’et bruge læsevisning frem for en disabled formular. Rolle- og invitationstatus vises med tydelige, brugerforståelige labels og badges. Følsomme objekter må ikke afsløres i UI’et, hvis brugerens rolle ikke bør kende til dem.