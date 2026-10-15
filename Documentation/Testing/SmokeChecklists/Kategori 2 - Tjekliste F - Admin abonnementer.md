# 2. Verifikationsproces pr. modul
## F. Admin – Abonnementer (`/admin/subscriptions`, kræver SystemAdmin)
- [ ] Rediger basispris → step-up-modal → gemmes, ny pris ses på `/pricing`
- [ ] Volumenrabat: tilføj/rediger/slet trin, nulstil til standard
- [ ] Tilkøb: samme CRUD-cyklus + duplikat-type afvises

### Forudsætninger
- 

### F1. Rediger basispris
- Route: `/admin/subscriptions`, fane "Basispriser"
- Bruger: Ny SystemAdmin B (2FA). Ingen godkender.
- Handling:
  a. B læser seed-priserne (kontrol, at tidligere kørsel er gendannet)
  b. B redigerer og gemmer én række ad gangen (Gem er pr. række; step-up ved første gem)
  c. En anonym bruger åbner `/pricing` og vælger hver cyklus
  d. B gendanner i en ny circuit (ny step-up) og kontrollerer seed-priserne
- Ændringer:

| Lag | Kategori | Cyklus | Før | Efter |
| ------ | ------ | ------ | ------ | ------ |
| HaveArkitekt | Editor | Annual | 168,00 | 175,00 |
| BedDesigner | Administrator | Monthly | 20,00 | 22,50 |
| Planlaegger | Viewer | Perpetual | 60,00 | 65,00 |

- Forventet: succesbesked "Basispris for '<navn>' er opdateret." efter hver Gem; `/pricing` viser de nye beløb under "Årligt", "Månedligt" og "Engangsbeløb"; gendannelse lykkes.
- Skærmbilleder (768, 1280): `Basispris_AdminSubscriptions_Gem` (efter sidste gem) og `Basispris_Pricing_Efter`.
- Filer: AdminSubscriptionManagementPage.razor(.cs), BasePriceMatrixEditor.razor(.cs), PricingPage.razor(.cs), PricingMatrixTable.razor(.cs), SubscriptionTierAdminService.cs, SubscriptionPricingService.cs, StepUpGuard.cs, AdminActionGuard.cs, DefaultSubscriptionTierCatalog.cs

### F2. Volumenrabat
- Route: `/admin/subscriptions`, fane "Volumen-rabatter"
- Bruger: Ny SystemAdmin B (2FA). Ingen godkender.
- Handling:
  a. B læser Min/Max/faktor på de to rækker (kontrol af seed)
  b. B redigerer og gemmer én række ad gangen (Gem er pr. række; step-up ved første gem)
  c. En anonym bruger beregner i pricing-calculatoren på `/pricing`
  d. B gendanner i en ny circuit (ny step-up) og kontrollerer seed-faktorerne
- Ændringer (Rabat-multiplikator):

| Trin | Før | Efter |
| ------ | ------ | ------ |
| 2-5 haver | 0,90 | 0,85 |
| 6-10 haver | 0,80 | 0,75 |

- Beregning (HaveArkitekt · Editor · Annual = 168,00, 0 arkiverede, ingen tilkøb):
  - 5 aktive haver: rabat-trappe 85, subtotal og total 714,00
  - 6 aktive haver: rabat-trappe 75, subtotal og total 756,00
- Forventet: succesbesked "Volumenrabat-trappe fra <n> haver er opdateret." efter hver Gem; calculatoren viser de nye beløb; gendannelse lykkes.
- Bemærk: Rabattabellen på /pricing viser de nye trin: 2-5 haver 85 % og 6-10 haver 75 % af basispris.
- Skærmbilleder (768, 1280): `Volumenrabat_AdminSubscriptions_Gem` og `Volumenrabat_Pricing_Efter`.
- Filer: AdminSubscriptionManagementPage.razor(.cs), VolumeDiscountEditor.razor(.cs), GardenVolumeDiscountAdminService.cs, PricingPage.razor(.cs), PricingCalculatorService.cs, GardenVolumeDiscountTable.razor(.cs), DefaultGardenVolumeDiscountCatalog.cs, GardenVolumeDiscountService.cs

### F3. Tilkøb
- Route: `/admin/subscriptions`, fane "Tilkøbsmoduler"
- Bruger: Ny SystemAdmin B (2FA). Ingen godkender.
- Handling:
  a. B læser navn og pris på de to rækker (kontrol af seed)
  b. B redigerer og gemmer én række ad gangen (Gem er pr. række; step-up ved første gem)
  c. B forsøger at oprette et tilkøb med en type, der allerede findes ("Tilføj tilkøb")
  d. En anonym bruger åbner `/pricing` og kontrollerer tilkøbskortene
  e. B gendanner i en ny circuit (ny step-up); en evt. ekstra række slettes først
- Ændringer:

| Tilkøb (type) | Cyklus | Før | Efter |
| ------ | ------ | ------ | ------ |
| Bedforslag (Niveau 2) (BedforslagNiveau2) | Monthly | 15,00 | 17,50 |
| Artefaktpakke A (ArtefaktpakkeA) | Annual | 48,00 | 55,00 |

- Forventet:
  - a-b: succesbesked "Tilkøb '<navn>' er opdateret." efter hver Gem
  - c: fejlen "Der findes allerede et tilkøb med typen 'BedforslagNiveau2'."; stadig 5 rækker; ingen række "e2e-duplikat"
  - d: de nye priser på kortene (17,50 kr. /md. og 55,00 kr. /år); stadig 5 kort; intet kort "e2e-duplikat"
  - e: gendannelse lykkes
- Skærmbilleder (768, 1280): `Tilkoeb_AdminSubscriptions_Gem` (efter sidste gem), `Tilkoeb_AdminSubscriptions_Duplikat` (fejlen) og `Tilkoeb_Pricing_Efter`.
- Filer: AdminSubscriptionManagementPage.razor(.cs), AddOnEditor.razor(.cs), SubscriptionAddOnAdminService.cs, PricingPage.razor(.cs), SubscriptionAddOnCard.razor(.cs), SubscriptionAddOnService.cs, DefaultSubscriptionAddOnCatalog.cs
 