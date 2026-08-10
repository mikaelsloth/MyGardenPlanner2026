# 14. Confirmation og destructive actions

## Overordnet princip

> **Destruktive handlinger skal være tydelige, bevidste og proportionale med konsekvensen. Brugeren skal forstå, hvad der sker, hvad der påvirkes, og om handlingen kan fortrydes.**

Det betyder, at vi ikke skal behandle alle “danger actions” ens. At arkivere et bed er ikke det samme som at slette en fil permanent. At fjerne en plante fra et bed er ikke det samme som at slette planten fra masterdata.

***

## 14.1 Skel mellem handlingstyper

Jeg vil opdele dem i fem kategorier:

| Type                            | Eksempel                                          |     Risiko | UI-mønster                                |
| ------------------------------- | ------------------------------------------------- | ---------: | ----------------------------------------- |
| **Lav risiko / reversibel**     | Arkivér have, arkivér bed                         | Lav-middel | Bekræftelse eller inline confirm          |
| **Fjern relation**              | Fjern plante fra bed, fjern materiale fra projekt |     Middel | Dialog med kontekst                       |
| **Tilbagekald adgang**          | Fjern medlem, tilbagekald invitation              | Middel-høj | Dialog med tydelig konsekvens             |
| **Slet data**                   | Slet fil, slet plante, slet materiale             |        Høj | Modal confirmation                        |
| **Permanent / ikke reversibel** | Permanent sletning af have/fil/data               |  Meget høj | Stærk confirmation, evt. tekstbekræftelse |

Designmæssigt bør vi undgå én generisk “Er du sikker?”-dialog til alt.

***

## 14.2 Arkivér er ikke det samme som slet

“Arkivering vs. sletning” er også udpeget som et særskilt guideline-emne, og dokumentet nævner, at arkivering bør have egne UI-principper som arkiveret badge, dæmpet card state, filteret “Vis arkiverede”, gendan-handling og tydelig forskel på “Arkivér” og “Slet”.

Jeg vil allerede her bruge den skelnen:

## Arkivér

Bruges når objektet ikke længere er aktivt, men stadig bør kunne findes historisk.

Eksempler:

```text
Arkivér have
Arkivér bed
```

Tone:

```text
Arkivér Villa Solbakken?
Haven skjules fra aktive oversigter, men kan findes igen under arkiverede haver.

[Annullér] [Arkivér have]
```

## Slet

Bruges når data faktisk fjernes eller gøres utilgængelig.

Eksempel:

```text
Slet fil?
Filen “Haveskitse maj.pdf” fjernes fra Villa Solbakken.

[Annullér] [Slet fil]
```

## Designregel

> **Hvis handlingen kan løses med arkivering, bør arkivering foretrækkes frem for sletning.**

***

## 14.3 Bekræftelse skal være proportional

Ikke alle handlinger kræver modal.

## Ingen confirmation

Bruges til ufarlige eller let reversible UI-handlinger:

* luk preview
* ryd søgning
* fjern filterchip
* collapse sidebar
* skift view-mode

## Inline confirmation

Bruges til mindre, reversible handlinger i en liste:

```text
Fjern plante fra bed?
[Annullér] [Fjern]
```

Kan vises direkte i kortet/rækken.

## Modal confirmation

Bruges når handlingen påvirker data eller adgang:

* slet fil
* fjern medlem
* tilbagekald invitation
* arkivér have
* arkivér bed

## Strong confirmation

Bruges ved permanent eller meget risikabel handling:

* permanent sletning af have
* permanent sletning af alle filer
* slet objekt med relationer

Kan kræve, at brugeren skriver navn:

```text
Skriv “Villa Solbakken” for at bekræfte.
```

## Designregel

> **Jo større konsekvens, jo stærkere confirmation.**

***

## 14.4 Confirmation-dialogens indhold

En god confirmation skal svare på fire ting:

1. **Hvad er handlingen?**
2. **Hvad påvirkes?**
3. **Kan det fortrydes?**
4. **Hvad er den præcise bekræftelseshandling?**

Eksempel:

```text
Slet fil?
Filen “Haveskitse maj.pdf” fjernes fra Villa Solbakken.
Denne handling kan ikke fortrydes.

[Annullér] [Slet fil]
```

Bedre end:

```text
Er du sikker?
[OK] [Cancel]
```

***

## 14.5 Button hierarchy

Vi har allerede etableret danger-knapper. Her bør de bruges sådan:

```html
<div class="modal-actions">
  <button class="btn btn-secondary" type="button">Annullér</button>
  <button class="btn btn-danger" type="button">Slet fil</button>
</div>
```

## Regler

* Cancel/Annullér skal være tydelig og let.
* Destruktiv handling skal være specifik: “Slet fil”, ikke bare “Slet”.
* Danger-knap bør som hovedregel stå sidst.
* Primary-knap bør ikke bruges til destructive action.
* Destruktive handlinger må ikke være default submit ved Enter uden omtanke.

## Designregel

> **Danger-knapper skal være konkrete og handlingsspecifikke.**

***

## 14.6 Tekster: brug objektets navn

Confirmation skal bruge objektets navn, hvis muligt.

## Godt

```text
Slet “Haveskitse maj.pdf”?
Filen fjernes fra Villa Solbakken.
```

```text
Fjern Anne Solbakken?
Anne mister adgang til Villa Solbakken.
```

```text
Tilbagekald invitation?
Invitationen til kunde@example.dk kan ikke længere bruges.
```

## Dårligt

```text
Slet element?
Er du sikker?
```

## Designregel

> **Brug objektets navn i titlen eller brødteksten, så brugeren ved præcis hvad de bekræfter.**

***

## 14.7 Fjern relation vs. slet objekt

Dette bliver meget vigtigt i MyGardenPlanner.

Eksempler:

* Fjern plante fra bed
* Slet plante fra masterdata
* Fjern materiale fra projekt
* Slet materiale fra materialelisten

Det er forskellige handlinger.

## Fjern relation

```text
Fjern Lavendel fra Staudehaven mod syd?
Planten fjernes kun fra dette bed. Den findes stadig i plantelisten.

[Annullér] [Fjern fra bed]
```

## Slet objekt

```text
Slet Lavendel?
Planten fjernes fra plantelisten. Den kan ikke længere vælges til nye bede.

[Annullér] [Slet plante]
```

## Designregel

> **Hvis handlingen kun fjerner en relation, må UI’et ikke bruge sprog, der lyder som permanent sletning.**

Brug:

```text
Fjern fra bed
Fjern fra projekt
Fjern fra liste
```

Ikke:

```text
Slet
```

medmindre objektet faktisk slettes.

***

## 14.8 Tilbagekald invitation og fjern medlem

Disse handler om adgang, og skal derfor forklares respektfuldt.

## Tilbagekald invitation

```text
Tilbagekald invitation?
Invitationen til kunde@example.dk kan ikke længere bruges.
Personen får ikke adgang til Villa Solbakken via denne invitation.

[Annullér] [Tilbagekald invitation]
```

## Fjern medlem

```text
Fjern medlem?
Anne Solbakken mister adgang til Villa Solbakken.
Eksisterende data i haven slettes ikke.

[Annullér] [Fjern adgang]
```

## Designregel

> **Adgangsændringer skal forklare konsekvensen for personen og for data.**

***

## 14.9 Slet fil

Filer og dokumenter bør have et præcist flow.

```text
Slet fil?
Filen “Haveskitse maj.pdf” fjernes fra Villa Solbakken.
Denne handling kan ikke fortrydes.

[Annullér] [Slet fil]
```

Hvis filen er midlertidig:

```text
Fjern midlertidig fil?
Filen “Ny skitse.png” fjernes fra midlertidige filer.

[Annullér] [Fjern fil]
```

Hvis den kan gendannes:

```text
Filen flyttes til slettede filer og kan gendannes senere.
```

Men hvis der ikke findes en gendan-funktion endnu, bør vi ikke skrive det.

## Designregel

> **UI’et må aldrig love fortryd/gendan, hvis funktionen ikke findes.**

***

## 14.10 Slet have eller bed

Dette er højrisiko, fordi de kan have relationer.

Jeg ville som udgangspunkt foretrække **Arkivér** først.

## Arkivér have

```text
Arkivér Villa Solbakken?
Haven skjules fra aktive oversigter, men bevares med bede, filer og medlemmer.

[Annullér] [Arkivér have]
```

## Slet have permanent

```text
Slet Villa Solbakken permanent?
Dette fjerner haven og relaterede data, hvis sletning er tilladt.
Denne handling kan ikke fortrydes.

Skriv “Villa Solbakken” for at bekræfte.

[Annullér] [Slet permanent]
```

Jeg ville dog ikke designe permanent sletning som en let tilgængelig standardhandling. Den bør ligge sekundært, evt. under “Flere handlinger” eller en farlig zone i indstillinger.

## Designregel

> **Permanent sletning af store objekter bør være gemt i en tydelig “Farlig zone”, ikke være en primær handling.**

***

## 14.11 “Danger zone” på detail-/settings-sider

For objekter som Have, Bed, Plante og Materiale kan en detail-/settings-side have en separat sektion:

```text
Farlig zone
Handlinger her kan påvirke adgang eller fjerne data.

[Arkivér have]
[Slet permanent]
```

Visuelt:

* separat card
* danger/attention border
* forklarende tekst
* handlinger klart adskilt fra normal redigering

Men vi skal passe på med at overdramatisere. “Farlig zone” kan evt. hedde:

```text
Avancerede handlinger
```

eller:

```text
Arkivering og sletning
```

Det passer bedre til den rolige MyGardenPlanner-tone.

## Designregel

> **Destruktive handlinger skal adskilles visuelt fra almindelige gem/rediger-handlinger.**

***

## 14.12 Confirmation og loading state

Når brugeren bekræfter en destruktiv handling, skal knappen skifte til loading.

```html
<button class="btn btn-danger" disabled>
  <span class="btn-spinner"></span>
  Sletter…
</button>
```

Eksempler:

```text
Arkiverer…
Sletter…
Fjerner adgang…
Tilbagekalder invitation…
Fjerner fra bed…
```

## Designregel

> **Efter confirmation skal selve destruktive handling vise loading state direkte på danger-knappen.**

***

## 14.13 Efter handling: status og return-flow

Når handlingen er udført, skal brugeren få en rolig statusbesked.

## Efter slet fil

```text
Filen er slettet
“Haveskitse maj.pdf” blev fjernet fra Villa Solbakken.
```

## Efter arkivér have

```text
Haven er arkiveret
Villa Solbakken findes nu under arkiverede haver.
```

## Efter fjern plante fra bed

```text
Planten er fjernet fra beddet
Lavendel findes stadig i plantelisten.
```

Dette hænger direkte sammen med vores navigation state-principper: brugeren skal blive i eller returnere til relevant kontekst.

## Designregel

> **Efter destructive action skal UI’et bekræfte resultatet og bevare brugerens kontekst.**

***

## 14.14 Undo — hvornår giver det mening?

Hvis handlingen er let reversibel, kan “Fortryd” være bedre end tung confirmation.

Eksempel:

```text
Planten blev fjernet fra beddet.
[Fortryd]
```

Godt til:

* fjern relation
* fjern filter
* arkivér, hvis gendan er simpelt
* fjern plante fra bed
* fjern materiale fra projekt

Ikke godt til:

* permanent sletning
* download/adgang
* invitation tilbagekaldt, hvis token allerede er ugyldiggjort
* fil sletning uden gendan

## Designregel

> **Brug undo til reversible handlinger. Brug confirmation til irreversible eller højrisiko-handlinger.**

***

## 14.15 Soft delete, archive, hard delete

Jeg ville dokumentere tre begreber:

## Arkivér

Objektet bevares, men skjules fra aktive oversigter.

```text
Arkivér have
Gendan have
```

## Fjern

Relationen fjernes, men objektet bevares.

```text
Fjern plante fra bed
Fjern materiale fra projekt
```

## Slet

Objektet slettes eller fjernes permanent.

```text
Slet fil
Slet plante
Slet permanent
```

## Designregel

> **Brug ordene “Arkivér”, “Fjern” og “Slet” konsekvent.**

***

## 14.16 Mobile confirmation

På mobil bør confirmation være:

* fuld bredde knapper
* tydelig titel
* kort tekst
* handling nederst
* cancel let tilgængelig
* ikke for lille klikområde

Eksempel:

```text
Slet fil?
“Haveskitse maj.pdf” fjernes fra Villa Solbakken.

[Annullér]
[Slet fil]
```

På mobil kan bottom sheet være bedre end klassisk modal, men kun hvis den implementeres ordentligt med fokusstyring.

***

## 14.17 Accessibility

Confirmation-dialoger skal være tilgængelige.

Guidelines:

* Dialog skal have tydelig titel.
* Fokus skal flyttes til dialogen.
* Escape/luk skal være muligt, hvis det ikke skaber risiko.
* Fokus skal returnere til den udløsende knap efter cancel.
* Danger-knap skal ikke nødvendigvis have autofocus.
* Knapper skal have konkrete labels.
* Farve må ikke være eneste signal.

## Designregel

> **Confirmation skal kunne forstås og gennemføres med tastatur og skærmlæser.**

***

## 14.18 Konkrete MyGardenPlanner-eksempler

### Arkivér bed

```text
Arkivér Staudehaven mod syd?
Beddet skjules fra aktive oversigter, men kan vises igen under arkiverede bede.

[Annullér] [Arkivér bed]
```

### Fjern plante fra bed

```text
Fjern Lavendel fra Staudehaven mod syd?
Planten fjernes kun fra dette bed. Den findes stadig i plantelisten.

[Annullér] [Fjern fra bed]
```

### Slet fil

```text
Slet Haveskitse maj.pdf?
Filen fjernes fra Villa Solbakken.
Denne handling kan ikke fortrydes.

[Annullér] [Slet fil]
```

### Tilbagekald invitation

```text
Tilbagekald invitation?
Invitationen til kunde@example.dk kan ikke længere bruges.

[Annullér] [Tilbagekald invitation]
```

### Fjern medlem

```text
Fjern adgang?
Anne Solbakken mister adgang til Villa Solbakken.
Eksisterende haveindhold slettes ikke.

[Annullér] [Fjern adgang]
```

***

## 14.19 Beslutningstabel

| Handling                    | Type            | Confirmation        | Primær knap            | Efter handling                |
| --------------------------- | --------------- | ------------------- | ---------------------- | ----------------------------- |
| Arkivér have                | Reversibel/soft | Modal/inline        | Arkivér have           | Status + flyt til arkiverede  |
| Arkivér bed                 | Reversibel/soft | Modal/inline        | Arkivér bed            | Status + dæmpet state         |
| Fjern plante fra bed        | Fjern relation  | Inline/modal        | Fjern fra bed          | Status + evt. undo            |
| Fjern materiale fra projekt | Fjern relation  | Inline/modal        | Fjern fra projekt      | Status + evt. undo            |
| Slet fil                    | Slet data       | Modal               | Slet fil               | Status + fjern fra liste      |
| Fjern medlem                | Adgang          | Modal               | Fjern adgang           | Status + opdater medlemeliste |
| Tilbagekald invitation      | Adgang/token    | Modal               | Tilbagekald invitation | Status + opdater invitation   |
| Permanent slet have         | Høj risiko      | Strong confirmation | Slet permanent         | Navigér væk/status            |

***

## 14.20 Komponenter/patterns

Jeg ville definere disse:

### `ConfirmDialog`

Til almindelige confirmations.

```text
title
description
impact
cancelLabel
confirmLabel
variant
```

### `InlineConfirm`

Til små listehandlinger.

```text
Fjern fra bed?
[Annullér] [Fjern]
```

### `DangerZone`

Til detail/settings-sider.

```text
Arkivering og sletning
[Arkivér] [Slet permanent]
```

### `UndoStatus`

Til reversible handlinger.

```text
Planten blev fjernet fra beddet.
[Fortryd]
```

***

# Anbefalet designbeslutning

Jeg ville låse principperne sådan:

1. **Destruktive handlinger skal være proportionale med konsekvensen.**
2. **Arkivér, fjern og slet skal bruges som tre forskellige begreber.**
3. **Arkivering foretrækkes frem for sletning, når data bør bevares.**
4. **Fjern relation må ikke formuleres som permanent sletning.**
5. **Confirmation skal nævne objektets navn og konsekvensen.**
6. **Danger-knapper skal have specifikke labels.**
7. **Permanent sletning kræver stærkere confirmation end almindelig arkivering.**
8. **Destruktive handlinger skal adskilles visuelt fra normale handlinger.**
9. **Efter handling skal brugeren få statusbesked og bevare kontekst.**
10. **Undo bruges kun når handlingen faktisk er reversibel.**
11. **Confirmation-dialoger skal være keyboard- og screenreader-venlige.**
12. **UI’et må ikke love gendannelse, hvis funktionen ikke findes.**

***

# Kort dokumentationstekst

Du kan bruge denne direkte:

> **Confirmation og destructive actions:** Destruktive handlinger skal være bevidste, kontekstuelle og proportionale med konsekvensen. UI’et skal skelne tydeligt mellem at arkivere, fjerne en relation og slette data. Confirmation skal nævne det konkrete objekt, forklare konsekvensen og bruge en handlingsspecifik danger-knap som “Slet fil”, “Fjern adgang” eller “Tilbagekald invitation”. Reversible handlinger kan bruge undo eller lettere confirmation, mens permanent sletning kræver stærkere confirmation. Efter handlingen skal brugeren få en rolig statusbesked og blive i eller returnere til relevant kontekst.