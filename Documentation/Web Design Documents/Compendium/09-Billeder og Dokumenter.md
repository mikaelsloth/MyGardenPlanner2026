### 📄 09-Billeder og Dokumenter.md

# 09 - Billeder og Dokumenter

## 📘 Grundregel
- Lister og oversigter henter og viser udelukkende metadata og lette thumbnails. Originalfiler, previews og tunge binære data indhentes først ved en bevidst, eksplicit brugerhandling. Filter-, søge- og sorteringshandlinger må aldrig udløse hentning af tunge originalressourcer. Drag-and-drop er altid et supplerende supplement til en tilgængelig, synlig filvælger-knap.

---

## 🔍 Anvendelse / varianter / typer

| Visningsniveau / Variant | CSS & Bootstrap basis | Brugerkontekst & Adfærd |
| :--- | :--- | :--- |
| **Thumbnail Small / Medium** | `.thumbnail` + `.thumb-sm` / `.thumb-md` + `MgpSkeletonMediaCard` | Lister, compact cards og grids. Bruges til hurtig visuel genkendelse uden stor netværksbelastning. Metadata (filnavn, filstørrelse, rettigheder) skal kunne vises og forblive tilgængelige før billedet/forhåndsvisningen er færdigindlæst. |
| **Thumbnail Large / Preview** | `.thumbnail` + `.thumb-lg` / `.media-preview` + `MgpSkeletonMediaCard` | Sidepaneler eller modaler. Bruges til at vurdere dokumentindhold eller billeddetaljer uden at hente originalfilen. Metadata (filnavn, filstørrelse, rettigheder) skal kunne vises og forblive tilgængelige før billedet/forhåndsvisningen er færdigindlæst. |
| **Media Card (Previewable)** | `.card-media` + `.media-card-previewable` + `MgpSkeletonMediaCard` | Interaktivt filkort, hvor klik på thumbnail eller titellink åbner fokuseret preview. Metadata (filnavn, filstørrelse, rettigheder) skal kunne vises og forblive tilgængelige før billedet/forhåndsvisningen er færdigindlæst. |
| **Media Card (Actions Only)** | `.card-media` + `.media-card-actions-only` | Filkort hvor kun eksplicitte knapper (f.eks. "Download" eller "Slet") kan aktiveres. |
| **Media Card (Restricted)** | `.card-media` + `.media-card-restricted` | Kort til låste eller begrænsede filer. Viser metadata, men skjuler/deaktiverer preview og download med forklarende status. |
| **Full Page / Original Viewer** | `.full-viewer` | Fuldskærmsvisning af store tegninger, PDF'er eller originale billeder ved eksplicit valg. |
| **Files Section (Detail Page)** | `FilesSection` / `.detail-files-section` | Dedikeret fil-sektion på entiteters detaljesider. Følger samme thumbnail-, preview-, restricted- og download-principper som filoversigten. |

> - **Restricted State (`.media-card-restricted`):** Ved manglende adgang til at downloade eller afspille et dokument, holdes metadata (filnavn, type, størrelse) synlige, mens download-knappen deaktiveres med en forklarende status. Hvis filens eksistens er sikkerhedsfølsom, skal kortet skjules helt fra oversigten.
> - **Processing State (`.empty-processing`):** Når mediefiler er uploadet, men thumbnail eller preview endnu ikke er færdiggenereret, benyttes tilstanden `empty-processing`. Brugeren skal fortsat have adgang til metadata og mulighed for at downloade originalfilen.

- **Valg af visningsmønster for filer:** Standard fil- og dokumentoversigter benytter `CompactEntityRow` (`.card-compact`). `ThumbnailGrid` (`.thumbnail-grid`) benyttes udelukkende, når den visuelle forhåndsvisning (skitse, foto, tegning) er den primære faktor for brugerens valg.

### File Attention Matrix

| Fil-situation | Attention Level | UI Præsentation |
| :--- | :--- | :--- |
| **Thumbnail oprettes** | **Level 1** | Object-level processing indicator / badge (`.status-processing`). |
| **Fil uploadet** | **Level 1** | Inline success status eller transient toast (afhængigt af kontekst). |
| **Fil udløber snart** | **Level 2** | Accent badge på filkort (`.attention-badge-level-2`) + `[Gør permanent]` handling. |
| **Upload fejlede** | **Level 3** | Persistent inline error (`.status-danger`) + `[Vælg anden fil]` handling. |

---

## 🚫 Regler (Do / Don't)
- **Do:** Tilbyd altid en synlig, fokusérbar "Vælg fil"-knap ved siden af drag-and-drop upload-zoner for at sikre tilgængelighed og mobilvenlighed.
- **Do:** Tydeliggør midlertidige filer (`AttachmentLifetime`) og udløbsdatoer (`ExpiresAt`) med synlige status-badges på filkortet.
- **Do:** Anvend faste aspect-ratios og reserveret plads til thumbnails (f.eks. via CSS `aspect-ratio`) samt native `loading="lazy"` for at forhindre layout-hop ved indlæsning.
- **Do:** Formuler download-handlinger eksplicit i tekst (f.eks. "Download PDF" eller "Download original") framfor kun at benytte et ikon.
- **Do:** Anvend termen *"Fjern fil"* for midlertidige/ikke-gemte bilag og *"Slet fil"* for permanente dokumenter. UI'et må aldrig love gendannelse eller undo for fil-sletning, medmindre en papirkurvsfunktion reelt er etableret.
- **Do:** Styr filer i v1 udelukkende via deres levetid/status (`Gør permanent`, `Fjern fil`, `Slet fil`) frem for et "Arkivér fil"-mønster. Arkivering reserveres til samlede dokumentarkiver.
- **Do:** Informative billeder og thumbnails skal altid have en beskrivende `alt`-tekst ud fra konteksten, mens rent dekorative billeder forsynes med tom alt-tekst (`alt=""`).
- **Do:** Upload-flows skal altid kunne gennemføres fuldstændigt via tastatur og synlig filvælgerknap uden krav om drag-and-drop eller visuel thumbnail.
- **Don't:** Hent eller generér aldrig tunge originalfiler automatisk ved indlæsning af lister eller kortoversigter.
- **Don't:** Brug ikke download som den primære eller implicitte klikhandling på et thumbnail eller filkort. Preview og download skal adskilles.
- **Don't:** Tilbyd ikke preview på filtyper, der ikke understøttes i browseren (vis i stedet et tydeligt filtype-ikon og tilbud download).
- **Don't:** Lad aldrig fritekstsøgning eller filtrering genhente eller generere tunge originalfiler (PDF'er eller højtopløselige billeder). Filtrering og søgning skal udelukkende operere på metadata og cachede thumbnails.
- **Edge cases:** Hvis oprettelse af thumbnail fejler eller understøttes ikke, benyttes en standardiseret fallback med filtype-ikon (PDF, DOC, IMG). Ved manglende rettigheder til filindhold (restricted) vises metadata med dæmpet/deaktiveret download-knap fremfor at skjule kortet. På mobilskærme (<= 640px) skal sidepaneler med preview automatisk åbne i en fokuseret full-screen visning (.full-viewer / full-screen modal) med tydelige "Luk" og "Download" handlinger. Ved print skjules interaktive preview-knapper, upload-zoner og download-handlinger.

---

## 🧩 Komponentpåvirkning
- **Type:** Foundation / Pattern
- **Nye Razor-komponenter:**
  - `MgpThumbnail.razor` (Visning af billed-thumbnails med automatisk størrelsesvalg: Small/Medium/Large, lazy loading og filtype-fallback).
  - `MgpMediaPreview.razor` (Sidepanel/modal til fokuseret dokument- og billedvisning uden hentning af originalfil).
  - `MgpFullViewer.razor` (Dedikeret viewer-komponent til visning af store PDF'er og højtopløselige tegninger).
  - `MgpFileBadge.razor` (Badge til indikering af levetid, midlertidig status og adgangsbegrænsninger).
- **Ændrede Razor-komponenter:**
  - `MgpMediaCard.razor` (Opdateret med support for levetidsbadges, preview-udløsere og tilstande for `restricted`/`actions-only`).
  - `MgpUploadZone.razor` (Opdateret med udvidede tilstande for empty, selected, uploading, success og error samt eksplicit knap-trigger).

---

## 🪙 Tokenpåvirkning
- `--mgp-surface`: Baggrund for thumbnails, preview-modaler og upload-zoner.
- `--mgp-surface-muted`: Baggrund for filtype-fallbacks og låste preview-flader.
- `--mgp-border`: Kant om thumbnails, billedrammer og viewer-paneler.
- `--mgp-primary-soft`: Baggrund for filtype-ikoner og upload-fremhævelse.
- `--mgp-primary-dark`: Tekst- og ikonfarve i fil-overdel og badges.
- `--mgp-accent`: Visuel indikator for midlertidige filer og filer, der udløber snart.
- `--mgp-danger`: Indikator for fejlede uploads eller slettehandlinger.

---

## 💻 CSS & Bootstrap

```css
/* Thumbnail Basisskabelon med faste stak-størrelser */
.thumbnail {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: var(--mgp-surface-muted);
  border: 1px solid var(--mgp-border);
  border-radius: var(--radius-md);
  overflow: hidden;
  position: relative;
  flex-shrink: 0;
}

.thumbnail img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

/* Størrelsesvarianter til Thumbnail-service integration */
.thumb-sm { width: 48px; height: 48px; }
.thumb-md { width: 96px; height: 96px; }
.thumb-lg { width: 100%; aspect-ratio: 16 / 9; max-height: 240px; }

/* Fallback ikon ved manglende thumbnail */
.thumbnail-fallback {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-xs);
  color: var(--mgp-primary-dark);
  font-weight: var(--font-weight-bold);
  font-size: var(--font-size-xs);
}

/* Media Card Interaktionsvarianter */
.media-card-previewable .thumbnail {
  cursor: pointer;
  transition: opacity .15s ease, transform .15s ease;
}

.media-card-previewable .thumbnail:hover {
  opacity: .9;
  transform: scale(1.02);
}

.media-card-restricted {
  opacity: .75;
  background: var(--mgp-surface-muted);
}

/* Preview Modal & Sidepanel Overlay */
.media-preview-panel {
  position: fixed;
  top: 0;
  right: 0;
  bottom: 0;
  width: 480px;
  max-width: 100vw;
  background: var(--mgp-surface);
  border-left: 1px solid var(--mgp-border);
  box-shadow: var(--shadow-lg);
  z-index: 1050;
  display: grid;
  grid-template-rows: auto 1fr auto;
  padding: var(--space-md);
  gap: var(--space-md);
}

.media-preview-body {
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: var(--mgp-surface-muted);
  border-radius: var(--radius-md);
  padding: var(--space-md);
}

.media-preview-body img {
  max-width: 100%;
  height: auto;
  border-radius: var(--radius-sm);
  box-shadow: var(--shadow-sm);
}

/* Print Overstyring */
@media print {
  .media-preview-panel,
  .upload-zone,
  .btn-download {
    display: none !important;
  }
}
```