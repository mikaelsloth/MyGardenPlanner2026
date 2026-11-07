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



