# 2. Verifikationsproces pr. modul
### C. Kontoadministration
- [ ] Skift e-mail (bekræftelseslink)
- [ ] Skift password
- [ ] Download/slet personlige data
### C1. Skift e-mail (bekræftelseslink)
- Pre-requisite: Ny bruger oprettet og bekræftet via UI (RegisteredUserFlow).
- Route: `/` --> `/account/Login` -->  ? --> `/Account/Manage/Email`
- Bruger: ny bruger
- Handling:
  a. Gå til `/Account/Manage/Email` og skift email
  b. Log ud og log ind igen med den nye email
- Forventet:
  a. Den nye email går i bekræftelses flow
  b. Efter bekræftelse skal den nye email fungere ved login - den gamle skal ikke.
- Skærmbillede: ja `/Account/Manage/Email` (375, 768, 939, 940, 1280)
- Filer: Components\Account\Pages\Manage\Email(.cs), Components\Account\Pages\ConfirmEmailChange.razor(.cs)
### C2. Skift password
- Pre-requisite: Ny bruger oprettet og bekræftet via UI (RegisteredUserFlow)
- Route: `/` --> `/account/Login` --> ? --> `/account/manage/changepassword`
- Bruger: ny bruger
- Handling:
  a. Gå til ChangePassword og ændre password
  b. Log ud og log derefter ind igen med det nye password
- Forventet: 
  a. Ved gyldigt nyt password, redirectes tilbage til siden man kom fra med bekræftelse på ændringen
  b. Login med det nye password skal fungere - login med det gamle skal ikke fungere
- Skærmbillede: ja: `/account/manage/changepassword (375, 768, 939, 940, 1280)
- Filer: Components\Account\Pages\Manage\ChangePassword.razor(.cs), Components\Account\Pages\SetPassword.razor(.cs)
### C3. Download/slet personlige data
- Pre-requisite: Ny bruger oprettet og bekræftet via UI (
- Route: `/` --> `/account/Login` --> ? --> `/account/manage/PersonalData`
- Bruger: ny bruger 
- Handling: 
  a. Download personlige data
  b. Slet personlige data
- Forventet:
  a.
- Skærmbillede: ja (375, 768, 939, 940, 1280: account/manage/PersonalData
- Filer: Components\Account\Pages\Manage\PersonalData.razor(.cs), Components\Account\Pages\Manage\DeletePersonalData.razor(.cs)