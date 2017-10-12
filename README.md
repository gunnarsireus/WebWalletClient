# WebWalletClient
Background
You want to create your online wallet service to which everyone can register and keep track of their hard earned money.
 
Assignment
The task is to create a web service with a frontend where you should be able to keep track of your money.
 
The service should meet the following criteria;
•	A user should be able to sign up for the service using his/her email-address and a password
•	A user must log in before he/she can use the service
•	In the service you should be able to:
o	Show the current balance and a list of withdrawals/deposits together with timestamps for each action.
o	Deposit money and provide a comment to a deposit.
o	Withdraw money and provide a comment to a withdrawal.
 
It should not be possible to end up withdrawing more money than your balance.
 
Requirements
The assignment should be written in C# MVC with unit tests where you see fit.
For data storage you should use any portable format, i.e. a flat file or a lightweight database.

**********************************************************************************************


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
köra i debuggläge. Starta först WebWalletAPI. En browser öppnas och Swagger startar.
skrivs ut.

Därefter tryck F5 i WebWalletClient. När programmet har öppnat, registrera dig som användare 
av WebWallet. Nu kan du börja skapa bankkonton och registrera transaktioner på bankkonton. 
Varje användare kan bara se sina egna bankkonton och transaktioner. Man kan lägga till en 
funktion där olika kategorier av användare används, exempelvis ”Admin” och ”External User” 
där ”Admin” tillåts se alla bankkonton och till skillnad från ”External User” som bara kan 
se sina egna konton och transaktioner. Men i denna version av WebWallet är detta inte 
implementerat.
