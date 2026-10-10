# 2. Verifikationsproces pr. modul
### C. Kontoadministration
- [ ] Skift e-mail (bekræftelseslink)
- [ ] Skift password
- [ ] Download/slet personlige data
### C1. Skift e-mail (bekræftelseslink)
- Pre-requisite: Ny bruger oprettet og bekræftet via UI (RegisteredUserFlow) og logget ind (LoginFlow).
- Route: `/` --> `/Account/Login` --> (direkte URL, intet link i UI) --> `/Account/Manage/Email` --> `/Account/ConfirmEmailChange?...` (linket vises på Email-siden, fordi appen bruger IdentityNoOpEmailSender)
- Bruger: ny bruger. Ny e-mail genereres med RegisteredUserFlow.NewEmail() (ryddes af ResetSmokeTestUsersInDB.sql).
- Handling:
  a. Gå til `/Account/Manage/Email`, skift e-mail og følg bekræftelseslinket
  b. Log ud (ryd cookies) og log ind igen med den nye e-mail
  c. Log ud og forsøg at logge ind med den gamle e-mail
- Forventet:
  a. "Confirmation link to change email sent. Please check your email." og linket "Click here to confirm your new email". Linket åbner ConfirmEmailChange: "Tak, fordi du bekræftede skiftet af din e-mail."
  b. Login med den nye e-mail lykkes.
  c. Login med den gamle e-mail afvises ("Error: Invalid login attempt.").
- Negative tests:
  - Uændret e-mail: "Your email is unchanged." og intet link.
  - Ugyldig e-mail: valideringsfejl, ingen besked om sendt link.
  - Ugyldigt link (forkert token): "Error: Der opstod en fejl ved skift af e-mail."; den nye e-mail virker ikke og den gamle virker stadig.
- Skærmbillede: ja `/Account/Manage/Email` ved sideindlæsning og efter submit (375, 768, 939, 940, 1280)
- Filer: Components\Account\Pages\Manage\Email.razor(.cs), Components\Account\Pages\ConfirmEmailChange.razor(.cs)
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