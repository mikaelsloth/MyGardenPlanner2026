### 📄 08-Printprincipper.md

# 08 - Printprincipper

## 📘 Grundregel
- Print fokuserer udelukkende på dokumentation og overblik ved at fjerne alle interaktive UI-elementer, navigation og handlinger. Layoutet transformationeres til en ren, flad, 1-kolonne læsevisning baseret på A4-format, hvor informationstæthed prioriteres uden afhængighed af farver eller skygger.

---

## 🔍 Anvendelse / varianter / typer

| Printtype | Formål / Indhold | Format & Adfærd |
| :--- | :--- | :--- |
| **Dokumentationsprint** | Haveoverblik, bedoversigter, planteplaner og tilbud. | Kundevenligt layout, luftig struktur, fuldt overblik over sektioner og cards. |
| **Arbejdsprint** | Intern plante-/materialeliste, tjeklister til havebesøg, målskitser. | Kompakt og praktisk tabellayout (`.print-table`) for høj datatæthed. |
| **Formular- / Læsevisning** | Udskrift af oprettede data fra redigeringssider. | Inputfelter, dropdowns og textareas vises som ren råtekst uden rammestreger, skygger eller knapper. |
| **Detail Page Print** | Dokumentationsudskrift af enkelte entiteter (Haveoverblik, Bedoverblik, Materialeliste, Planteliste, Filoversigt, Kundeoverblik). | Navigation, context tabs, header-actions og Danger Zone skjules. Indholdet udskrives som en ren 1-kolonne dokumentrapport. |

> **Transformering fra skærm til print:**  
> Interaktive cards, media cards og række-grids på skærmen transformerer ved udskrift automatisk til en flad `PrintTable` (`.print-table`). Dette sikrer maksimal informationstæthed, minimerer antaller af papirsider og fjerner unødvendige visuelle rammer/skygger i monokromt tryk.

> **Print Density:** Udskrifter og print-rapporter anvender altid **Compact / Document Mode** (`[data-density="compact"]`). Dette maksimerer datamængden pr. papirside, minimerer padding og konverterer visuelle skærmkort til flade, papiroptimerede tabeller (`.print-table`).

---

## 🚫 Regler (Do / Don't)
- **Do:** Anvend `.print-only` og `.screen-only` utilities til at skræddersy overskrifter og metadata specifikt til papir.
- **Do:** Ledsag altid statusser og badges med tekstforklaring, da print skal fungere uden farve (monokrom print).
- **Do:** Styr sideskift aktivt med `.print-avoid-break` på cards/sektioner og `.print-page-break` før nye hovedafsnit.
- **Do:** Tillad at arkiveret status og tilhørende badges (`.badge-archived`) udskrives på dokumentationsprint for at bevare det historiske overblik, men skjul altid interaktive handlinger som `[Gendan]`, `[Vis arkiverede]` og `[Slet permanent]`.
- **Do:** Skjul altid transient notifikations-UI (`.toast-stack`, `.toast`) ved udskrift. Inline statusbeskeder (`.status-message`, `.status-banner`) udskrives kun, hvis de har direkte dokumentationsværdi for den udskrevne rapport.
- **Do:** Konverter cards og visuelle lister på skærmen til en flad `PrintTable` (`.print-table`) ved udskrift, så visningen matcher papirets dokumentationsbehov fremfor skærmens browsing-behov.
- **Do:** Sikr at printudskrifter ikke fjerner meningsbærende tekst eller statusser, der har dokumentationsværdi. Farve må aldrig være eneste signal i print (badges og statusser skal altid ledsages af klar tekst).
- **Don't:** Vis aldrig navigationskomponenter (sidebar, header, drawers, tabs), handlingsknapper, filterbarer eller upload-zoner på print.
- **Don't:** Vis ikke automatisk URL-adresser efter links (`a::after { content: ""; }`), medmindre der er tale om ren teknisk dokumentation.
- **Edge cases:** Ved udskrift af komplekse datalister omdannes skærmens kort-grid til en klassisk HTML-tabel (`.print-table`) med faste grænser for at forhindre unødvendigt papirforbrug. Tilstande for Empty, Error og No Access skjules automatisk ved print, medmindre de eksplicit er placeret i en `.print-only` container som dokumentation for manglende rapporteringsdata.

---

## 🧩 Komponentpåvirkning
- **Type:** Foundation / Pattern
- **Nye Razor-komponenter:**
  - `MgpPrintScope.razor` (Wrapper-komponent til at isolere og printe specifikke del-sektioner på siden).
  - `MgpPrintFooter.razor` (Standardiseret print-footer med udskriftsdato og projekttitel, kun synlig ved print).
  - `MgpPrintTable.razor` (Specifik printtabel-komponent der konverterer skærm-cards og lister til en flad, papiroptimeret tabelvisning).
- **Ændrede Razor-komponenter:**
  - Skjulning i print-mode tilføjes på tværs af interaktive komponenter (`MgpCard`, `MgpFormField`, `MgpFilterBar`, `MgpUploadZone`, `MgpHeader`, `MgpSidebar`).

---

## 🪙 Tokenpåvirkning
- Ved print overstyres skærmens tokens globalt i `@media print`:
  - `--mgp-surface`: Tvinges til `#ffffff`.
  - `--mgp-text`: Tvinges til `#111111`.
  - `--mgp-border`: Tvinges til `#cccccc`.
  - Skygger (`var(--shadow-*)`) nulstilles.

---

## 💻 CSS & Bootstrap

```css
/* Print Utilities (Generelle) */
.print-only {
  display: none !important;
}

/* Global Print-pakke */
@media print {
  @page {
    size: A4;
    margin: 16mm;
  }

  body {
    background: #ffffff !important;
    color: #111111 !important;
    font-size: 10.5pt;
    line-height: 1.4;
  }

  /* Skjul interaktive, nav- og tilstandselementer */
  .screen-only,
  .sidebar,
  .header,
  .footer,
  .drawer,
  .drawer-backdrop,
  .btn,
  .btn-row,
  .card-actions,
  .form-actions,
  .filter-bar,
  .upload-zone,
  .nav-drawer,
  .mobile-header,
  .skeleton-card,
  .skeleton-row,
  .skeleton-media,
  .inline-loading,
  .btn-spinner,
  .empty-state,
  .archive-banner,
  .btn-restore,
  .context-tabs.no-print 
  .toast-stack,
  .toast {
    display: none !important;
  }


  .print-only {
    display: block !important;
  }

  .app-shell {
    display: block !important;
  }

  main {
    max-width: none !important;
    padding: 0 !important;
  }

  /* Nulstil Grid til 1-kolonne */
  .grid,
  .grid-2,
  .grid-3,
  .grid-4,
  .form-grid,
  .form-grid-2,
  .form-grid-3,
  .form-grid-4 {
    display: block !important;
  }

  .grid > *,
  .form-grid > * {
    margin-bottom: 12pt;
  }

  /* Cards og Billeder i Print */
  .card,
  .hero,
  .status-message {
    box-shadow: none !important;
    border: 1px solid #cccccc !important;
    background: #ffffff !important;
    break-inside: avoid;
  }

  /* Badges */
  .badge {
    background: #ffffff !important;
    color: #111111 !important;
    border: 1px solid #999999 !important;
  }

  /* Formularer som Læsevisning */
  input,
  textarea,
  select {
    border: 0 !important;
    background: transparent !important;
    padding: 0 !important;
    box-shadow: none !important;
    appearance: none;
  }

  .help,
  .help-empty,
  .field-message {
    display: none !important;
  }

  /* Links uden URL-støj */
  a::after {
    content: "";
  }

  /* Sideskift Styring */
  h1, h2, h3 {
    break-after: avoid;
  }

  .print-avoid-break {
    break-inside: avoid;
  }

  .print-page-break {
    break-before: page;
  }

  /* Print-specifik Tabel */
  .print-table {
    width: 100%;
    border-collapse: collapse;
  }

  .print-table th,
  .print-table td {
    text-align: left;
    border-bottom: 1px solid #dddddd;
    padding: 6pt 4pt;
    vertical-align: top;
  }

  .print-table th {
    font-weight: 700;
  }

  /* Print Footer */
  .print-footer {
    display: block !important;
    margin-top: 24pt;
    padding-top: 8pt;
    border-top: 1px solid #cccccc;
    font-size: 9pt;
    color: #555555;
  }
}
```