Projekt u kterého jsem se naučil práci s C# Razor Pages (MVVM), v průběhu vývoje jsem se zlepšoval a spousta kodu se dala napsat čistěji (jako například SqlClient dependency injection).
Přesto si myslím, že je projekt poměrně rozsáhlý a dal mi mnoho.

Použité technologie: C#, Razor pages, HTML, CSS, JS, Chart,js, ADO.NET, Bootstrap, JSON

Pro stažení a funkčnost je potřeba vytvořit na localhostu databázi certainty s dvěmi tabulky recordTable a userTable podle předlohy:

CREATE DATABASE certainty;
USE certainty;

CREATE TABLE userTable(
    username VARCHAR(100),
    password VARCHAR(100),
    email VARCHAR(70),
    salt VARCHAR(100),
    currency VARCHAR(5),
    PRIMARY KEY (username)
);

CREATE TABLE recordTable(
    recordID INT PRIMARY KEY,
    category VARCHAR(100),
    userID VARCHAR(100),
    value FLOAT,
    recordDate DATE,
    FOREIGN KEY (userID) REFERENCES userTable(username)
);

Tabulky nebo databáze může být pojmenována jinak, ale je nutno následně upravit appsettings.json a musí být zachovaná struktura daných tabulek.
