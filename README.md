# WebWalletClient
WebWallet instruktioner
Ambitionen har varit att göra WebWallet-programmet självinstruerande.
 
WebWallet består av två program:
WebWalletAPI är ett ASP.NET Web  API  för läsning och skrivning i Sqlite-databasen WebWallet.db
som ligger på folder ~/App_Data. Programmet baseras på .NET Core 2.0.0 och använder 
ipnummer: http://localhost:54411/. 

Koden finns på: 
https://github.com/gunnarsireus/WebWalletAPI

WebWalletClient är användargränssnittet skrivet i C# MVC baserat på .NET Core 2.0.0. 
Här finns också användardatabasen AspNet.db, (Sqlite) där användarna registreras. 
Här används bl.a. klasser från  Microsoft.AspNetCore.Identity.
Koden finns på 
https://github.com/gunnarsireus/WebWalletClient

Enhetstester
I projektet WebWalletClient.Tests och WebWalletAPI.Test finns tester som testar Controllers. 
Testerna är implementerade med xUnit och Moq. Testerna körs genom att högerklicka i koden 
och välja ”Run tests”. Observera att WebWalletAPI måste vara igång för att testerna skall 
kunna köras felfritt. 

Köra programmet
Hämta koden från github, öppna med Visual Studio 2017. När koden är öppnad tryck F5 för att 
köra i debuggläge. Starta först WebwalletAPI. En browser öppnas och texten ["WebWalletAPI started"] 
skrivs ut.

Därefter tryck F5 i WebWalletClient. När programmet har öppnat, registrera dig som användare 
av WebWallet. Nu kan du börja skapa bankkonton och registrera transaktioner på bankkonton. 
Varje användare kan bara se sina egna bankkonton och transaktioner. Man kan lägga till en 
funktion där olika kategorier av användare används, exempelvis ”Admin” och ”External User” 
där ”Admin” tillåts se alla bankkonton och till skillnad från ”External User” som bara kan 
se sina egna konton och transaktioner. Men i denna version av WebWallet är detta inte 
implementerat.
