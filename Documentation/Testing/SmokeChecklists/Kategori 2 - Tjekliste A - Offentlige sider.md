# 2. Verifikationsproces pr. modul
## Tjekliste A. Offentlige sider (ingen login)
- [ ] `/`, `/pricing`, `/about`, `/terms`, `/privacy` loader uden fejl
- [ ] Pricing-calculator beregner korrekt (stik af mod Prismatrix-tal)
- [ ] Mobilvisning < 940px: hamburger-menu + `NavDrawer` åbner/lukker, fokus-trap virker
### A1. Load af sider
- Route: `/`, `/pricing`, `/about`, `/terms`, `/privacy`
- Bruger: ingen login
- Handling: Load side
- Forventet: Siden vises uden fejl
- Skærmbillede: ja (375, 768, 939, 940, 1280)
- Filer: Components\Pages\LandingPage.razor, Components\Pages\PricingPage.razor, Components\Pages\AboutPage.razor, Components\Pages\TermsPage.razor, Components\Pages\PrivacyPage.razor
### A2. Pricing-calculator beregner korrekt
- Route: `/pricing`
- Bruger: ingen login
- Handling: Pricing-calculator udfyldes med 5 forskellige scenarier
- Forventet: Pricing-calculator beregner korrekt jf. seeded data
- Skærmbillede: ja (375, 768, 1280)
- Filer: Infrastructure\Data\Seed\DefaultSubscriptionTierCatalog.cs, Infrastructure\Data\Seed\DefaultSubscriptionAddOnCatalog.cs, Infrastructure\Data\Seed\DefaultGardenVolumeDiscountCatalog.cs
### A3. Mobilvisning 
- Route: `/`
- Bruger: ingen login
- Handling: Siden åbnes i 768 + 939 px. hamburger-menu åbnes/lukkes med klik og ved hjælp af tastatur
- Forventet: hamburger-menu + `NavDrawer` åbner/lukker, fokus-trap virker
- Skærmbillede: nej
- Filer: Components\Layout\NavDrawer.razor, Components\Layout\PublicHeader.razor