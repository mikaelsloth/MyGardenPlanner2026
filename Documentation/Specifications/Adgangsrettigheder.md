##### Andet:

Der tjekkes på kredit og forbrug ved login (token-baseret adgang), samt ved brug af rettigheder.

Forskellige rettigheder på forskellige niveauer kan kombineres.



### Adgangsmatrix level 1:

C = Oprette

R = Læse

U = Opdatere

D = Slette

A = Arkivere / Re-aktivere

\-E = kun records man selv har oprettet

\-A = alle records i en have

\-M = fælles records for system



##### Adresse

|Bruger rolle|Brugt i model|Rettighed|
|-|-|-|
|Alle|UserProfile|CRUD -E|
|Have Arkitekt\\Administrator|Have|CRUD -A|
|Have Arkitekt\\Editor|Have|CRU -A|
|Have Arkitekt\\Viewer|Have|R -A|
|Have Arkitekt\\Administrator|Butik|CRUD -A<br />R -M|
|Editor|Butik|CRUD -E<br />CRU -A<br />R -M|
|Viewer|Butik|CRUD -E<br />R -A<br />R -M|



##### Bed

|Bruger rolle|Brugt i model|Rettighed|
|-|-|-|
|Have Arkitekt\\Administrator|Bed|CRUD -A|
|Have Arkitekt\\Editor|Bed|CRU -A<br />CRUD -E|
|Have Arkitekt\\Viewer+|Bed|R -A|
|Have Arkitekt\\Viewer|Bed|R -A<br />Kun adgang til følgende dokumenter: Primær bed skitse og Primær planteliste|
|Have Arkitekt\\Alle|Alle andre|Følger ovenstående|



##### Butik

|Bruger rolle|Brugt i model|Rettighed|
|-|-|-|
|Have Arkitekt\\Administrator|Butik|CRUD -A<br />R -M|
|Have Arkitekt\\Editor|Butik|CRU -A<br />CRUD -E<br />R -M|
|Have Arkitekt\\Viewer+|Butik|R -A<br />R -M|
|Have Arkitekt\\Viewer|Butik|R -A<br />R -M<br />Kun adgang til følgende dokumenter: Primær logo|
|Have Arkitekt\\Alle|Alle andre|Følger ovenstående|



##### Have

|Bruger rolle|Brugt i model|Rettighed|
|-|-|-|
|Have Arkitekt\\Administrator|Have|CRUDA -A|
|Have Arkitekt\\Editor|Have|CRU -A<br />CRUDA -E|
|Have Arkitekt\\Viewer+|Have|R -A|
|Have Arkitekt\\Viewer|Have|R -A<br />Kun adgang til følgende dokumenter: Primær haveplan, Introduktion og Primær planteliste|
|Have Arkitekt\\Alle|Alle andre|Følger ovenstående|



##### HaveInvitation

|Bruger rolle|Brugt i model|Rettighed|
|-|-|-|
|Have Arkitekt\\Administrator|HaveInvitation|CRUDA -A|
|Alle|HaveInvitation|CRUDA -E|



##### Kontakt

|Bruger rolle|Brugt i model|Rettighed|
|-|-|-|
|Alle|UserProfile|CRUD -E|
|Have Arkitekt\\Administrator|Have|CRUD -A|
|Have Arkitekt\\Editor|Have|CRU -A|
|Have Arkitekt\\Viewer|Have|R -A|
|Have Arkitekt\\Administrator|Butik|CRUD -A<br />R -M|
|Editor|Butik|CRUD -E<br />CRU -A<br />R -M|
|Viewer|Butik|CRUD -E<br />R -A<br />R -M|



##### Materiale

|Bruger rolle|Brugt i model|Rettighed|
|-|-|-|
|Have Arkitekt\\Administrator|Materiale|CRUD -A<br />R -M|
|Have Arkitekt\\Editor|Materiale|CRU -A<br />CRUD -E<br />R -M|
|Have Arkitekt\\Viewer+|Materiale|R -A<br />R -M|
|Have Arkitekt\\Viewer|Materiale|R -A<br />R -M<br />Kun adgang til følgende dokumenter: Primær billede og Sekundær billede|
|Have Arkitekt\\Alle|Alle andre|Følger ovenstående|



##### Plante

|Bruger rolle|Brugt i model|Rettighed|
|-|-|-|
|Have Arkitekt\\Administrator|Plante|CRUD -A<br />R -M|
|Have Arkitekt\\Editor|Plante|CRU -A<br />CRUD -E<br />R -M|
|Have Arkitekt\\Viewer+|Plante|R -A<br />R -M|
|Have Arkitekt\\Viewer|Plante|R -A<br />R -M<br />Kun adgang til følgende dokumenter: Primær billede og Sekundær billede|
|Have Arkitekt\\Alle|Alle andre|Følger ovenstående|



##### Userprofile

|Bruger rolle|Brugt i model|Rettighed|
|-|-|-|
|Alle|Alle|CRUD -E|



##### Vedhaeftet\*

|Bruger rolle|Brugt i model|Rettighed|
|-|-|-|
|Have Arkitekt\\Administrator|Vedhaeftet\*|CRUD -A<br />R -M|
|Have Arkitekt\\Editor|Vedhaeftet\*|CRU -A<br />CRUD -E<br />R -M|
|Have Arkitekt\\Viewer|Vedhaeftet\*|R -A<br />R -M|
|Have Arkitekt\\Alle|Alle andre|Følger ovenstående|

------

### Adgangsmatrix admin-området (system)

Admin-området er adskilt fra have-rollerne ovenfor (Have Arkitekt\\...). Det styres af fire systemroller:
SystemAdmin, DataAdmin, PolicyAdmin og AuditViewer.

Adgang til en admin-side kræver begge dele:

* En af de krævede roller. Rollen kan være tildelt direkte eller være aktiv via en godkendt JIT-eskalering.
* Totrinsbekræftelse (2FA) aktiveret på kontoen.

Uden 2FA sendes brugeren til /Account/Manage/EnableAuthenticator. Uden rolle sendes brugeren til /Account/AccessDenied. Anonyme brugere sendes til /Account/Login med relativ ReturnUrl.



##### Sider

|Rute|Overskrift|Policy|Roller med adgang|
|-|-|-|-|
|/admin/subscriptions|Administrer abonnementer|RequireGlobalAdmin|SystemAdmin|
|/admin/security-policies|Sikkerhedspolicies|RequirePolicyAdmin|PolicyAdmin|
|/admin/jit-requests|JIT-adgang|RequireAnyAdminRole|SystemAdmin, DataAdmin, PolicyAdmin, AuditViewer|
|/admin/audit-log|AuditLog|RequireAuditViewer|AuditViewer|

RequireDataAdmin er reserveret til kommende stamdata-admin-sider (Layer 1). Ingen side bruger den endnu. DataAdmin har i dag kun adgang til /admin/jit-requests.



##### Handlinger med ekstra krav

Step-up = ny bekræftelse med adgangskode og TOTP. Rate limit = admin-API-grænsen pr. bruger (glidende vindue, konfigurerbar under Sikkerhedspolicies).

|Side|Handling|Step-up|Rate limit|
|-|-|-|-|
|/admin/subscriptions|Gem basispris|Ja|Ja|
|/admin/subscriptions|Gem, tilføj og slet volumenrabat-trin|Ja|Ja|
|/admin/subscriptions|Gem, tilføj og slet tilkøb|Ja|Ja|
|/admin/security-policies|Gem i alle seks faner|Ja|Ja|
|/admin/jit-requests|Send anmodning|Nej|Ja|
|/admin/jit-requests|Træk egen afventende anmodning tilbage|Nej|Ikke dokumenteret|
|/admin/jit-requests|Godkend eller afvis anmodning|Ja|Ja|
|/admin/audit-log|Eksportér og Send til baggrundseksport|Ja|Ja|
|/admin/audit-log|Filtrér, sortér og se detaljer|Nej|Nej|

Step-up-vinduet er konfigurerbart (standard 15 minutter, tilladt 1-60). Inden for vinduet i samme session vises modalen ikke igen. En ny sideindlæsning kræver ny bekræftelse.

Login-forsøg (POST til login, 2FA-login og gendannelseskode-login) har desuden en separat grænse pr. IP-adresse. Ved overskridelse svarer serveren 429 med dansk besked og Retry-After.



##### JIT-eskalering

|Regel|Beskrivelse|
|-|-|
|Hvem kan anmode|Brugere med mindst én admin-rolle (adgang til /admin/jit-requests).|
|Anmodning|Rolle, antal minutter og en årsag. Minutter skal ligge inden for policyen (standard 30-90). Årsag er påkrævet.|
|Afvises|Hvis ansøgeren allerede er medlem af rollen, eller allerede har en afventende eller aktiv anmodning om samme rolle.|
|Godkendelse|Godkender må ikke være ansøgeren selv og skal være direkte medlem af den ansøgte rolle. Aktiv JIT-adgang tæller ikke.|
|Tilbagetrækning|Ansøgeren kan trække en afventende anmodning tilbage (status Annulleret).|
|Udløb|Godkendte anmodninger bliver Udløbet, når tiden er gået. Afventende anmodninger annulleres efter PendingExpiryHours (standard 48 timer).|
|Adgang|Vurderes ved hvert kald ud fra udløbstidspunktet, uden cache.|
|Sikkerhedsalarm|Sendes til administratorlisten ved anmodning og godkendelse. Ikke ved afvisning eller tilbagetrækning.|



##### Navigation

Menuen Administration viser kun de links, brugeren har adgang til. Menuen er statisk: den opdateres først ved en ny sideindlæsning efter en ændring i adgang, fx en godkendt JIT-eskalering. Det er tilsigtet.

