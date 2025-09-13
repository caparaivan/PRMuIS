using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace HotelskiSistem
{
    public enum StanjeApartmana { Prazan, Zauzet, PotrebnoCiscenje }
    public enum StanjeAlarma { Normalno, Aktivirano }
    public enum Pol { Muski, Zenski, Neodredjeno }

    [Serializable]
    public class Poruka
    {
        public string Tip { get; set; }
        public object Sadrzaj { get; set; }
    }

    [Serializable]
    public class Gost
    {
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string BrojPasosa { get; set; }
    }

    //serializable da bi objekat mogao da se spakuje u niz bajtova i posalje preko mreze
    [Serializable]
    public class Apartman
    {
        public int BrojApartmana { get; set; }
        public int Sprat { get; set; }
        public int Klasa { get; set; }
        public int MaksimalanBrojGostiju { get; set; }
        public int TrenutniBrojGostiju { get; set; }
        public StanjeApartmana StanjeApartmana { get; set; }
        public StanjeAlarma StanjeAlarma { get; set; }
        public List<Gost> ListaGostiju { get; set; }
        public double UkupniRacun { get; set; }
        public int PreostaloNoci { get; set; }
    }

    [Serializable]
    public class Osoblje
    {
        public int ID { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public Pol Pol { get; set; }
        public string Funkcija { get; set; }
    }

    [Serializable]
    public class ZadatakOsoblju
    {
        public string TipZadatka { get; set; }
        public int BrojApartmana { get; set; }
        public string Opis { get; set; }
    }

    [Serializable]
    public class PlatniPodaci
    {
        public int BrojApartmana { get; set; }
        public double Iznos { get; set; }
        public string BrojKartice { get; set; }
    }
}