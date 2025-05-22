##Driving License Quiz App

##Indítási útmutató

Navigáljon az alkalmazás backend könyvtárába:
 - cd DrivingLicenseQuiz.API

 Indítsa el a szervert:
 - dotnet run

 - Az adatbázis automatikusan feltöltődik 10 tesztkérdéssel az Entity Framework migrációknak köszönhetően.

 - Az adatbázis megtekintéséhez használható például a DB Browser for SQLite, amelyben a szerver elindítása után az adatállomány azonnal elérhető.

 - A felhasználók regisztráció után be tudnak jelentkezni. A rendszer a bejelentkezést követően a munkamenet azonosítót (Session ID) sütikben (cooki -   e-kban) tárolja.

 - A kvíz 10 kérdésből áll, mindegyikhez egy helyes válasz tartozik. A teszt kitöltése után a felhasználó megtekintheti az eredményeit a „History” felületen, beleértve a helyes és helytelen válaszokat is.

Elérhetőség
A webes felhasználói felület elérhető a következő címen:
http://localhost:5073/index.html

Az API endpointok tesztelhetők Swagger felületen keresztül:
http://localhost:5073/swagger
