# Simulacija rada hotelskog sistema

[cite_start]Ovaj projekat predstavlja model **digitalizovanog upravljanja hotelom** sa fokusom na automatizaciju rezervacija, praćenje aktivnosti gostiju i koordinaciju hotelskog osoblja u realnom vremenu[cite: 2]. [cite_start]Sistem je zasnovan na klijent-server arhitekturi koja koristi UDP i TCP protokole za komunikaciju[cite: 2, 9].

## 🏗️ Arhitektura sistema

[cite_start]Sistem se sastoji od tri ključne komponente[cite: 2, 9]:

1.  [cite_start]**Centralni server**: Srce sistema koje održava evidenciju o svim apartmanima i gostima, obrađuje rezervacije i dodeljuje zadatke osoblju[cite: 9].
2.  [cite_start]**Gosti (UDP klijenti)**: Korisnici koji šalju zahteve za rezervaciju, unose podatke o boravku i vrše narudžbine[cite: 2, 11].
3.  [cite_start]**Hotelsko osoblje (TCP klijenti)**: Zaposleni koji primaju i potvrđuju zadatke za održavanje hotela putem pouzdane veze[cite: 2, 11].

### Komunikacioni protokoli
* [cite_start]**UDP**: Koristi se za komunikaciju sa gostima radi brze prijave i unosa podataka o rezervaciji[cite: 2].
* [cite_start]**TCP**: Koristi se za komunikaciju sa osobljem radi osiguravanja pouzdane dostave radnih zadataka[cite: 2].

---

## ✨ Ključne funkcionalnosti

### 🏨 Upravljanje apartmanima
[cite_start]Sistem održava listu apartmana sa sledećim podacima[cite: 2, 10]:
* [cite_start]**Osnovni parametri**: Broj apartmana, sprat, klasa (1, 2, 3) i maksimalan broj gostiju[cite: 10].
* [cite_start]**Statusi**: Praćenje da li je apartman prazan, zauzet ili mu je potrebno čišćenje[cite: 10].
* [cite_start]**Opremljenost i bezbednost**: Stanje minibara i praćenje protivpožarnog alarmnog sistema[cite: 2, 10].

### 👤 Funkcije za goste
* [cite_start]**Rezervacija**: Unos broja gostiju, broja noćenja i željene klase apartmana[cite: 2, 11].
* [cite_start]**Tok boravka**: Mogućnost naručivanja hrane i pića, kao i aktivacija alarma u hitnim slučajevima[cite: 11, 12].
* [cite_start]**Naplata**: Automatski obračun troškova noćenja, minibara i dodatnih usluga uz generisanje završnog računa[cite: 2, 12].

### 🧹 Koordinacija osoblja
[cite_start]Osoblje izvršava zadatke dodeljene od strane servera[cite: 9]:
* [cite_start]**Čišćenje**: Održavanje apartmana koji su napušteni ili zahtevaju higijenu[cite: 9].
* [cite_start]**Minibar**: Ažuriranje stanja pića i hrane na osnovu zahteva gostiju[cite: 9].
* [cite_start]**Alarmi**: Hitna sanacija u slučaju aktivacije protivpožarnog alarma[cite: 9].

---

## 🛠️ Tehnička implementacija

* [cite_start]**Neblokirajući server**: Implementiran polling model za istovremenu proveru poruka na UDP i TCP portovima[cite: 11].
* [cite_start]**Serijalizacija**: Podaci o klasama `Apartman`, `Gost` i `Osoblje` prenose se binarnom serijalizacijom pomoću `MemoryStream`-a[cite: 10, 11].
* [cite_start]**Algoritmi**: Automatizovan obračun troškova i dinamička alokacija slobodnih apartmana[cite: 2, 11].

---

## 👥 Projektni tim (Grupa 6)

* [cite_start]**Ivan Ćapara** (PR105-2022) - [capara.ivan359@gmail.com](mailto:capara.ivan359@gmail.com) [cite: 1]
* [cite_start]**Aleksandar Jokanović** (PR106-2022) - [cjokanovic76@gmail.com](mailto:cjokanovic76@gmail.com) [cite: 1]

[cite_start]**GitHub link**: [https://github.com/caparaivan/PRMUIS](https://github.com/caparaivan/PRMUIS) [cite: 1]
