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
- Bemærk: rabattabellen "Have-abonnement (trapperabat)" på `/pricing` læser standardkataloget og ikke databasen og kan derfor ikke verificeres, før den er afklaret.
- Skærmbilleder (768, 1280): `Volumenrabat_AdminSubscriptions_Gem` og `Volumenrabat_Pricing_Efter`.
- Filer: AdminSubscriptionManagementPage.razor(.cs), VolumeDiscountEditor.razor(.cs), GardenVolumeDiscountAdminService.cs, PricingPage.razor(.cs), PricingCalculatorService.cs, GardenVolumeDiscountTable.razor(.cs), DefaultGardenVolumeDiscountCatalog.cs

### F3. Tilkøb
- Route: `/admin/subscriptions` 
- Bruger: Ny SystemAdmin B
- Handling:
  a. B Rediger 2-4 felter på fanen med Tilkøbs priser
  b. B Gennemfør stepup-modal
  c. A godkender på `/admin/jit-requests`
  d. B gemmer ændringer
  e. B opretter duplikeret linje
  f. B gemmer ændringer
  g. B Logout og tjek `/pricing` siden som anonym bruger
  h. Nulstil ændringer
- Forventet:
  a. Alle processer a-d forløber uden fejl
  b. e-f skal fejle
  c. Pricing siden viser de nye priser
- Skærmbillede: ja, admin side ved Gem og Pricing side efter opdatering (768, 1280)
- Filer: Components\Pages\Admin\AdminSubscriptionManagementPage.razor(.cs), Components\Domain\Admin\AddOnEditor.razor(.cs), Components\Pages\PricingPage.razor(.cs), 
    Components\Domain\Subscriptions\SubscriptionAddOnCard.razor(.cs), Components\Pages\Admin\AdminJitRequestsPage.razor(.cs), PricingCalculatorService.cs
