# 15. Arkivering vs. sletning

## Overordnet princip

> **Arkivering er en rolig måde at fjerne noget fra det aktive arbejdsrum uden at ødelægge historikken. Sletning er en destruktiv handling, der kun bør bruges, når data reelt skal fjernes.**

Det betyder, at brugeren som udgangspunkt skal møde **Arkivér** før **Slet**, når objektet stadig kan have historisk eller dokumentationsmæssig værdi.

I MyGardenPlanner er dette især relevant for:

* haver
* bede
* projekter
* filer
* invitationer
* måske planter/materialer, afhængigt af om de er masterdata eller kun relationer

***

## 15.1 Begrebsafklaring

Jeg vil skelne mellem fire beslægtede begreber:

| Begreb      | Betydning                                       | Eksempel             |
| ----------- | ----------------------------------------------- | -------------------- |
| **Arkivér** | Skjul fra aktive visninger, men bevar data      | Arkivér have         |
| **Gendan**  | Flyt arkiveret objekt tilbage til aktiv visning | Gendan bed           |
| **Fjern**   | Fjern en relation, men bevar objektet           | Fjern plante fra bed |
| **Slet**    | Fjern data eller gør data utilgængelig          | Slet fil             |

Det vigtigste er, at ordene bruges konsekvent.

## Designregel

> **Brug aldrig “Slet”, hvis handlingen kun fjerner noget fra en kontekst. Brug “Fjern”. Brug “Arkivér”, hvis objektet bevares, men ikke længere skal være aktivt.**

***

# 15.2 Hvornår skal vi arkivere?

Arkivering giver mening, når objektet kan være historisk relevant eller indgå i tidligere arbejde.

## Typiske arkiveringskandidater

### Haver

En have kan være færdig, sat på pause eller ikke længere aktiv, men stadig have værdi som reference.

```text
Arkivér Villa Solbakken?
Haven skjules fra aktive oversigter, men bevares med bede, filer og medlemmer.
```

### Bede

Et bed kan være udfaset, omdesignet eller ikke længere en del af den aktive plan.

```text
Arkivér Staudehaven mod syd?
Beddet skjules fra aktive bede, men kan vises igen under arkiverede bede.
```

### Projekter eller arbejdsforløb

Hvis projektlaget senere får egne arkiveringsstates, giver samme princip mening.

### Invitationer

Her vil jeg dog være forsigtig: invitationer bør ofte have statusser som aktiv, udløbet, accepteret eller tilbagekaldt — ikke nødvendigvis “arkiveret”. Men gamle invitationer kan skjules fra aktiv visning.

***

# 15.3 Hvornår skal vi slette?

Sletning bør være reserveret til situationer, hvor data faktisk skal fjernes.

## Sletning giver mening når

* filen er uploadet ved en fejl
* dokumentet ikke må ligge i haven
* midlertidig fil skal fjernes
* objektet er testdata
* brugeren bevidst ønsker permanent fjernelse
* data ikke længere må opbevares

## Eksempel

```text
Slet Haveskitse maj.pdf?
Filen fjernes fra Villa Solbakken.
Denne handling kan ikke fortrydes.

[Annullér] [Slet fil]
```

## Designregel

> **Sletning skal være specifik, bekræftet og konsekvensforklarende.**

***

# 15.4 Arkiveret UI-state

Arkiverede objekter skal stadig kunne genkendes tydeligt, men ikke fremstå som aktive.

## Visuelle kendetegn

* Dæmpet card/række
* Badge: `Arkiveret`
* Mindre fremtrædende primære handlinger
* Tydelig “Gendan”-handling
* Eventuelt metadata: “Arkiveret”

Eksempel:

```text
Villa Solbakken
Kundehave · Aarhus N

[Arkiveret]

[Gendan] [Vis detaljer]
```

## CSS-retning

```css
.card-archived {
  background: var(--mgp-surface-muted);
  color: var(--mgp-text-muted);
  opacity: .92;
}

.badge-archived {
  color: var(--mgp-text-muted);
  background: var(--mgp-surface-muted);
  border-color: var(--mgp-border);
}
```

## Designregel

> **Arkiverede objekter skal være synlige som historiske/inaktive — ikke som fejl, og ikke som aktive objekter.**

***

# 15.5 Listevisninger og filter

Arkiverede objekter bør som udgangspunkt **ikke** vises i aktive lister, men brugeren skal kunne finde dem.

## Standardadfærd

```text
Aktive haver
- Villa Solbakken
- Skovhaven
```

Arkiverede skjules som standard.

## Filter

```text
[ ] Vis arkiverede
```

eller som filterchip:

```text
Status: Aktive
Status: Arkiverede
Status: Alle
```

## Når arkiverede vises

```text
[Arkiveret] Villa Solbakken
```

## Designregel

> **Aktive lister viser aktive objekter som standard. Arkiverede objekter vises via tydeligt filter.**

Dette er vigtigt for rolig UI, fordi arkiverede data ellers kan støje i den daglige arbejdsliste.

***

# 15.6 Arkivér-flow

Arkivering er mindre farlig end sletning, men stadig en tilstandsændring. Den bør normalt bekræftes.

## Eksempel: arkivér have

```text
Arkivér Villa Solbakken?
Haven skjules fra aktive oversigter, men bevares med bede, filer og medlemmer.

[Annullér] [Arkivér have]
```

## Efter arkivering

```text
Haven er arkiveret
Villa Solbakken findes nu under arkiverede haver.

[Vis arkiverede]
```

## Designregel

> **Arkivering skal forklare, at objektet bevares, men flyttes ud af aktiv visning.**

***

# 15.7 Gendan-flow

Arkivering bør have en modsatrettet handling: **Gendan**.

## Eksempel

```text
Gendan Villa Solbakken?
Haven vises igen under aktive haver.

[Annullér] [Gendan have]
```

Efter handling:

```text
Haven er gendannet
Villa Solbakken vises igen under aktive haver.
```

## Designregel

> **Hvis vi tilbyder arkivering, bør vi også tilbyde en tydelig gendan-handling.**

Hvis gendan ikke findes endnu i implementationen, skal UI’et ikke love det.

***

# 15.8 Arkivér vs. fjern relation

Dette er en vigtig forskel.

## Arkivér objekt

Objektet bliver inaktivt globalt i sin kontekst.

```text
Arkivér bed
```

## Fjern relation

Objektet fjernes kun fra en bestemt sammenhæng.

```text
Fjern Lavendel fra Staudehaven mod syd
```

Tekst:

```text
Planten fjernes kun fra dette bed. Den findes stadig i plantelisten.
```

## Designregel

> **Hvis objektet stadig findes i systemet og kun fjernes fra en relation, skal handlingen hedde “Fjern”, ikke “Arkivér” eller “Slet”.**

***

# 15.9 Arkivér vs. slet på filer

Filer er lidt anderledes.

En fil kan være:

* midlertidig
* permanent
* udløber snart
* slettet/fjernet
* eventuelt arkiveret, hvis filen er historisk relevant

Men jeg ville ikke starte med “arkivér fil” som hovedmønster. For filer giver disse handlinger ofte mere mening:

| Filstatus          | Handling                              |
| ------------------ | ------------------------------------- |
| Midlertidig fil    | Gør permanent / fjern                 |
| Permanent fil      | Slet fil / evt. flyt til arkiv senere |
| Udløber snart      | Gør permanent / fjern                 |
| Historisk dokument | Behold eller kategorisér som arkiv    |

Hvis der senere kommer mange historiske dokumenter, kan “Arkivér fil” give mening. Men i første omgang ville jeg bruge:

```text
Gør permanent
Slet fil
```

for filens livscyklus.

## Designregel

> **Filer bør først og fremmest styres af lifetime/status. Arkivér fil bør kun indføres, hvis der er et tydeligt historisk dokumentbehov.**

Filmodellen arbejder med vedhæftede filer og filmetadata, herunder `VedhaeftetFil`, `VedhaeftetFilData` og thumbnails, hvilket understøtter at filer har en særskilt livscyklus i UI’et.

***

# 15.10 Slet permanent bør være gemt væk

Permanent sletning af større objekter bør ikke være en almindelig knap i hovedvisningen.

## Placering

* Under indstillinger
* Under “Arkivering og sletning”
* I en danger zone
* Ikke som primær handling på oversigt/card

Eksempel:

```text
Arkivering og sletning
Handlinger her kan skjule eller fjerne data.

[Arkivér have]
[Slet permanent]
```

## Designregel

> **Permanent sletning bør være en sekundær, tydeligt adskilt handling — ikke en primær workflow-handling.**

***

# 15.11 Confirmation-niveauer

## Arkivér

Mild confirmation:

```text
Arkivér have?
Haven skjules fra aktive oversigter, men data bevares.

[Annullér] [Arkivér have]
```

## Gendan

Let confirmation eller direkte handling med status:

```text
Haven er gendannet
```

Hvis handlingen er let reversibel, kan vi nøjes med direkte handling + status.

## Slet

Stærkere confirmation:

```text
Slet fil?
Denne handling kan ikke fortrydes.
```

## Permanent sletning

Strong confirmation:

```text
Skriv “Villa Solbakken” for at bekræfte.
```

***

# 15.12 Status efter handling

Efter arkivering/sletning skal UI’et tydeligt fortælle hvad der skete.

## Arkiveret

```text
Haven er arkiveret
Villa Solbakken findes nu under arkiverede haver.

[Vis arkiverede]
```

## Gendannet

```text
Haven er gendannet
Villa Solbakken vises igen under aktive haver.
```

## Slettet

```text
Filen er slettet
Haveskitse maj.pdf blev fjernet fra Villa Solbakken.
```

## Fjernet relation

```text
Planten er fjernet fra beddet
Lavendel findes stadig i plantelisten.

[Fortryd]
```

## Designregel

> **Statusbeskeden skal bruge samme begreb som handlingen: arkiveret, gendannet, fjernet eller slettet.**

***

# 15.13 Sprogprincipper

## Brug

* Arkivér
* Gendan
* Fjern fra bed
* Fjern adgang
* Tilbagekald invitation
* Slet fil
* Slet permanent

## Undgå

* Deaktivér, hvis der menes arkivér
* Slet, hvis der menes fjern relation
* Fjern, hvis data faktisk slettes
* Deaktiver bruger, hvis det kun er adgang til haven
* “OK” i confirmations

## Designregel

> **Knaptekster skal beskrive den konkrete handling, ikke bare sige “OK”.**

***

# 15.14 Edge cases

## Arkiveret objekt med aktive relationer

Eksempel: En have har aktive medlemmer og filer.

UI bør forklare:

```text
Haven arkiveres, men medlemmer og filer bevares.
```

## Arkiveret bed med projektdata

Hvis senere relevant:

```text
Beddet arkiveres, men historiske projektdata bevares.
```

## Sletning blokeres

Hvis objektet ikke kan slettes pga. relationer:

```text
Haven kan ikke slettes
Arkivér haven i stedet, eller fjern relaterede data først.
```

## Arkiveret objekt vises via direkte link

Hvis brugeren åbner et arkiveret objekt direkte:

```text
Denne have er arkiveret
Den vises ikke i aktive oversigter.

[Gendan have]
```

***

# 15.15 UI-komponenter

Jeg ville definere disse komponent-/state-patterns:

## `ArchivedBadge`

```html
<span class="badge badge-muted">Arkiveret</span>
```

## `ArchivedCard`

Dæmpet card med gendan-handling.

```html
<article class="card card-archived">
  ...
</article>
```

## `ArchiveConfirmDialog`

Confirmation til arkivering.

## `RestoreAction`

Gendan-handling i arkiverede lister.

## `DangerZone`

Sektion til arkivering og permanent sletning.

## `ArchiveFilter`

Filter til at vise arkiverede.

```text
[ ] Vis arkiverede
```

eller:

```text
Status: Aktive / Arkiverede / Alle
```

***

# 15.16 Beslutningstabel

| Situation                     | Brug           | Primær handling   | Confirmation        |
| ----------------------------- | -------------- | ----------------- | ------------------- |
| Have ikke længere aktiv       | Arkivér        | Arkivér have      | Ja                  |
| Bed ikke længere aktivt       | Arkivér        | Arkivér bed       | Ja                  |
| Brugeren vil se historik      | Vis arkiverede | Vis arkiverede    | Nej                 |
| Brugeren vil aktivere igen    | Gendan         | Gendan have/bed   | Let / status        |
| Plante fjernes fra bed        | Fjern relation | Fjern fra bed     | Inline/modal        |
| Materiale fjernes fra projekt | Fjern relation | Fjern fra projekt | Inline/modal        |
| Fil uploadet forkert          | Slet           | Slet fil          | Ja                  |
| Permanent slet have           | Slet permanent | Slet permanent    | Strong confirmation |

***

# 15.17 Anbefalet designbeslutning

Jeg ville låse principperne sådan:

1. **Arkivér bruges til inaktive/historiske objekter, hvor data bør bevares.**
2. **Slet bruges kun, når data faktisk fjernes.**
3. **Fjern bruges, når en relation fjernes, men objektet bevares.**
4. **Arkiverede objekter skjules fra aktive lister som standard.**
5. **Arkiverede objekter kan vises via filteret “Vis arkiverede” eller statusfilter.**
6. **Arkiverede objekter får badge og dæmpet visuel state.**
7. **Arkivering bør have gendan-handling.**
8. **Permanent sletning placeres i danger zone/settings, ikke som primær handling.**
9. **Confirmation-tekster skal forklare, om data bevares eller fjernes.**
10. **Statusbeskeder skal bruge samme begreb som handlingen: arkiveret, gendannet, fjernet eller slettet.**
11. **UI’et må ikke love gendan, hvis funktionen ikke findes.**

***

# Kort dokumentationstekst

Du kan bruge denne direkte:

> **Arkivering vs. sletning:** Arkivering bruges til objekter, der ikke længere er aktive, men stadig har historisk eller dokumentationsmæssig værdi. Arkiverede objekter skjules fra aktive lister som standard, men kan vises via filter og gendannes, hvis funktionen understøttes. Sletning bruges kun, når data faktisk skal fjernes, og kræver tydelig confirmation. Hvis en handling kun fjerner en relation — fx en plante fra et bed — skal handlingen hedde “Fjern”, ikke “Slet”. Permanent sletning placeres separat fra almindelige handlinger og kræver stærkere confirmation.