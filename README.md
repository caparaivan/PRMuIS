# 🏨 Simulacija rada hotelskog sistema

## 🚀 Korišćene tehnologije
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![TCP](https://img.shields.io/badge/TCP-00457C?style=for-the-badge&logo=socketdotio&logoColor=white)
![UDP](https://img.shields.io/badge/UDP-008000?style=for-the-badge&logo=socketdotio&logoColor=white)
![IP](https://img.shields.io/badge/IP-Networking-blue?style=for-the-badge&logo=internet-explorer&logoColor=white)
![OOP](https://img.shields.io/badge/OOP-Programming-orange?style=for-the-badge&logo=code&logoColor=white)
![Serialization](https://img.shields.io/badge/Binary%20Serialization-gray?style=for-the-badge&logo=databricks&logoColor=white)

---

## 📖 Opis projekta
**Simulacija rada hotelskog sistema** predstavlja model digitalizovanog upravljanja hotelom sa fokusom na:
- Rezervaciju apartmana putem UDP komunikacije sa gostima  
- Praćenje aktivnosti gostiju (noćenja, minibar, alarm)  
- Koordinaciju hotelskog osoblja putem TCP komunikacije  
- Evidenciju i obračun troškova  

Centralni **server** je zadužen za:
- Obradu rezervacija i praćenje statusa apartmana  
- Slanje zadataka osoblju (čišćenje, minibar, alarm)  
- Generisanje završnog računa za gosta  

---

## 🛠️ Funkcionalnosti
- **Gost (UDP klijent)**  
  - Unosi broj apartmana, broj gostiju i broj noći  
  - Prima potvrdu rezervacije i završni račun  

- **Osoblje (TCP klijent)**  
  - Prima zadatke od servera (čišćenje, minibar, alarm)  
  - Vraća potvrdu o izvršenju zadatka  

- **Server**  
  - Održava listu apartmana sa svim podacima  
  - Evidentira goste, njihove troškove i stanje apartmana  
  - Radi u neblokirajućem režimu (polling model)  

---

## 🏗️ Arhitektura sistema
- **Apartman**  
  - Broj apartmana, sprat, klasa (1,2,3), maksimalan broj gostiju  
  - Trenutni broj gostiju, stanje minibara, stanje apartmana (prazan, zauzet, potrebno čišćenje)  
  - Stanje alarma (normalno/aktivirano)  

- **Gost**  
  - Ime, prezime, pol, datum rođenja, broj pasoša  

- **Osoblje**  
  - ID, ime, prezime, pol, funkcija  

- **Server**  
  - UDP komunikacija sa gostima  
  - TCP komunikacija sa osobljem  
  - Serijalizacija podataka pomoću `BinaryFormatter` i `MemoryStream`  

---

## 📌 Tok rada
1. **Prijava gosta** – unos rezervacije putem UDP klijenta  
2. **Boravak gosta** – korišćenje minibara, narudžbine, eventualna aktivacija alarma  
3. **Koordinacija sa osobljem** – server šalje zadatke osoblju putem TCP-a  
4. **Završetak rezervacije** – gost dobija završni račun, apartman prelazi u stanje „potrebno čišćenje“  

---

## 🧪 Primer upotrebe
Gost rezerviše apartman klase 3 na trećem spratu za tri noći. Tokom boravka koristi minibar i aktivira alarm. Server evidentira troškove minibara i alarma. Po završetku rezervacije gost dobija ukupan račun, a apartman prelazi u stanje „potrebno čišćenje“ koje server prosleđuje osoblju.

---

## 📂 Struktura projekta
- **Server aplikacija** – centralna logika, UDP/TCP komunikacija  
- **Klijent (Gost)** – rezervacije i narudžbine  
- **Klijent (Osoblje)** – izvršavanje zadataka i potvrde  

---

## ▶️ Pokretanje
1. Pokrenuti **server aplikaciju**  
2. Pokrenuti **klijentsku aplikaciju za gosta** (UDP)  
3. Pokrenuti **klijentsku aplikaciju za osoblje** (TCP)  
4. Testirati rezervacije, narudžbine i zadatke osoblja  

---
