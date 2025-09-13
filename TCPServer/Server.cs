using HotelskiSistem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace TCPServer
{
    public class Server
    {
        private const int Port = 50001;
        private static int BrojApartmana = 20;
        private static int MaxOsoblje = 3;

        private static List<Apartman> apartmani = new List<Apartman>();
        private static Dictionary<IPEndPoint, Apartman> gostiIP = new Dictionary<IPEndPoint, Apartman>(); //mapira IP adrese gostiju na njihove apartmane

        private static List<Socket> povezanoOsoblje = new List<Socket>(); //lista soketa povezanog osoblja sa serverom
        private static bool ServerRadi = true;

        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Server: Pokretanje hotelskog sistema.");

            // Inicijalizacija apartmana
            for (int i = 1; i <= BrojApartmana; i++)
            {
                apartmani.Add(new Apartman
                {
                    BrojApartmana = i,
                    Sprat = (i - 1) / 5 + 1,
                    Klasa = (i % 3) + 1,
                    MaksimalanBrojGostiju = (i % 3) + 1,
                    StanjeApartmana = StanjeApartmana.Prazan,
                    StanjeAlarma = StanjeAlarma.Normalno,
                    ListaGostiju = new List<Gost>(),
                    UkupniRacun = 0,
                    PreostaloNoci = 0
                });
            }

            // Inicijalizacija TCP i UDP soketa
            Socket tcpServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket udpServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // TCP server (za osoblje)
            IPEndPoint tcpServerEP = new IPEndPoint(IPAddress.Any, Port); // slusa na svim adresama
            tcpServerSocket.Bind(tcpServerEP); //rezervise port za tcp server
            tcpServerSocket.Blocking = false;
            tcpServerSocket.Listen(MaxOsoblje);
            Console.WriteLine($"Server: TCP server sluša na portu {Port} za osoblje.");

            // UDP server (za goste)
            IPEndPoint udpServerEP = new IPEndPoint(IPAddress.Any, Port);
            udpServerSocket.Bind(udpServerEP);
            udpServerSocket.Blocking = false;
            Console.WriteLine($"Server: UDP server sluša na portu {Port} za goste.");

            Console.WriteLine("\nServer je pokrenut. Pritisnite ESC za izlazak.");
            // Pokretanje pozadinske niti za auriranje boravaka
            Thread azuriranjeThread = new Thread(() => AzurirajBoravke(udpServerSocket));
            azuriranjeThread.Start();

            while (ServerRadi)
            {
                // Priprema listi za Select
                List<Socket> checkRead = new List<Socket>();
                List<Socket> checkError = new List<Socket>();

                checkRead.Add(udpServerSocket);
                checkError.Add(udpServerSocket);

                if (povezanoOsoblje.Count < MaxOsoblje)
                {
                    checkRead.Add(tcpServerSocket);
                    checkError.Add(tcpServerSocket);
                }

                foreach (var staffSocket in povezanoOsoblje)
                {
                    checkRead.Add(staffSocket);
                    checkError.Add(staffSocket);
                }

                try
                {
                    Socket.Select(checkRead, null, checkError, 100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Server: Greška u Socket.Select: {ex.Message}");
                    break;
                }

                // Provera za unos sa tastature
                if (Console.KeyAvailable)
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine("\nServer: Pritisnut je ESC. Gašenje servera...");
                        ServerRadi = false;
                        break;
                    }
                }

                if (checkRead.Count > 0)
                {
                    foreach (Socket s in checkRead.ToList())
                    {
                        if (s == tcpServerSocket)
                        {
                            try
                            {
                                Socket clientOsoblje = tcpServerSocket.Accept();
                                clientOsoblje.Blocking = false;
                                povezanoOsoblje.Add(clientOsoblje);
                                Console.WriteLine($"Server: Novo osoblje povezano sa: {clientOsoblje.RemoteEndPoint}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Server: Greška pri prihvatanju TCP konekcije: {ex.Message}");
                            }
                        }
                        else if (s == udpServerSocket)
                        {
                            byte[] buffer = new byte[1024];
                            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                            try
                            {
                                int bytesRead = udpServerSocket.ReceiveFrom(buffer, ref remoteEP);
                                string poruka = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                ObradiPorukuGosta(poruka, remoteEP, udpServerSocket);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Server: Greška pri prijemu UDP poruke: {ex.Message}");
                            }
                        }
                        else if (povezanoOsoblje.Contains(s))
                        {
                            byte[] buffer = new byte[1024];
                            try
                            {
                                int bytesRead = s.Receive(buffer);
                                if (bytesRead == 0)
                                {
                                    Console.WriteLine($"Server: Klijent se isključio: {s.RemoteEndPoint}");
                                    s.Close();
                                    povezanoOsoblje.Remove(s);
                                    continue;
                                }
                                var poruka = Deserijalizuj<Poruka>(buffer);
                                ObradiPotvrduOsoblja(poruka, s);
                            }
                            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.WouldBlock || ex.SocketErrorCode == SocketError.IOPending)
                            {
                                // Nema podataka, preskoci
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Server: Greška pri prijemu TCP poruke od osoblja: {ex.Message}");
                                s.Close();
                                povezanoOsoblje.Remove(s);
                            }
                        }
                    }
                }
            }

            // ciscenje i zatvaranje socketa
            foreach (var staffSocket in povezanoOsoblje)
            {
                staffSocket.Close();
            }
            tcpServerSocket.Close();
            udpServerSocket.Close();
            Console.WriteLine("Server: Sistem je ugašen.");
        }

        private static void ObradiPorukuGosta(string poruka, EndPoint remoteEP, Socket udpSocket)
        {
            var delovi = poruka.Split(';');
            var tipPoruke = delovi[0].ToUpper();
            var ipEP = remoteEP as IPEndPoint;

            Console.WriteLine($"Server: Primljena UDP poruka od {remoteEP}: '{poruka}'");
            switch (tipPoruke)
            {
                case "PROVERI_DOSTUPNOST":
                    var slobodni = apartmani.Where(a => a.StanjeApartmana == StanjeApartmana.Prazan).GroupBy(a => a.Klasa).OrderBy(g => g.Key);
                    var sb = new StringBuilder();
                    bool prvaKlasa = true;
                    foreach (var grupa in slobodni)
                    {
                        if (!prvaKlasa)
                        {
                            sb.Append(Environment.NewLine);
                        }
                        sb.Append($"\nklasa {grupa.Key}:");
                        var brojevi = string.Join(", ", grupa.Select(a => a.BrojApartmana));
                        sb.Append($" {brojevi}");
                        prvaKlasa = false;
                    }
                    PosaljiPorukuUDP($"LISTA_SOBA;{sb.ToString()}", ipEP, udpSocket);
                    break;

                case "REZERVACIJA":
                    // REZERVACIJA;BrojApartmana;BrojGostiju;BrojNoci
                    if (delovi.Length == 4
                        && int.TryParse(delovi[1], out int brApt)
                        && int.TryParse(delovi[2], out int brojGostiju)
                        && int.TryParse(delovi[3], out int brojNoci))
                    {
                        var apt = apartmani
                            .FirstOrDefault(a => a.BrojApartmana == brApt
                                              && a.StanjeApartmana == StanjeApartmana.Prazan
                                              && a.MaksimalanBrojGostiju >= brojGostiju);
                        if (apt != null)
                        {
                            apt.StanjeApartmana = StanjeApartmana.Zauzet;
                            apt.TrenutniBrojGostiju = brojGostiju;
                            apt.PreostaloNoci = brojNoci;
                            apt.ListaGostiju.Clear();
                            gostiIP[ipEP] = apt;

                            PosaljiPorukuUDP($"POTVRDA_REZERVACIJE;{apt.BrojApartmana};{apt.PreostaloNoci}", ipEP, udpSocket);
                            Console.WriteLine($"Server: Apartman {apt.BrojApartmana} rezervisan na {brojNoci} noći.");
                        }
                        else
                        {
                            PosaljiPorukuUDP("ODBIJANJE_REZERVACIJE;Traženi apartman nije dostupan ili kapacitet ne odgovara.", ipEP, udpSocket);
                        }
                    }
                    else
                    {
                        PosaljiPorukuUDP("GRESKA;Format rezervacije: REZERVACIJA;BrojApt;BrojGostiju;BrojNoci", ipEP, udpSocket);
                    }
                    break;

                case "PLATI":
                    if (gostiIP.ContainsKey(ipEP) && delovi.Length == 2)
                    {
                        var apt = gostiIP[ipEP];
                        PosaljiPorukuUDP(
                            $"POTVRDA_PLATE;Uplaćeno {apt.UkupniRacun} dinara. Hvala!",
                            ipEP, udpSocket);
                        Console.WriteLine($"Server: Gost u apartmanu {apt.BrojApartmana} platio {apt.UkupniRacun} din.");

                        // Oslobadjanje apartmana i zadatak za ciscenje
                        apt.StanjeApartmana = StanjeApartmana.PotrebnoCiscenje;
                        apt.UkupniRacun = 0;
                        apt.ListaGostiju.Clear();
                        gostiIP.Remove(ipEP);
                        PosaljiZadatakOsoblju(new ZadatakOsoblju
                        {
                            TipZadatka = "ciscenje",
                            BrojApartmana = apt.BrojApartmana,
                            Opis = $"Očistite apartman {apt.BrojApartmana}."
                        });
                    }
                    break;

                case "NARUDZBINA":
                    if (gostiIP.ContainsKey(ipEP) && delovi.Length >= 2)
                    {
                        var apartman = gostiIP[ipEP];
                        string predmet = delovi[1];
                        double cena = 0;
                        if (predmet.ToLower() == "minibar") cena = 50;
                        apartman.UkupniRacun += cena;
                        PosaljiPorukuUDP($"POTVRDA_NARUDZBINE;{predmet} je narucen. Dodato na vas racun.", ipEP, udpSocket);
                        Console.WriteLine($"Server: Gosti u apartmanu {apartman.BrojApartmana} su naručili {predmet}.");
                    }
                    else
                    {
                        PosaljiPorukuUDP("GRESKA;Morate biti prijavljeni da biste naručili.", ipEP, udpSocket);
                    }
                    break;

                case "ALARM":
                    if (gostiIP.ContainsKey(ipEP))
                    {
                        var apartman = gostiIP[ipEP];
                        apartman.StanjeAlarma = StanjeAlarma.Aktivirano;
                        apartman.UkupniRacun += 100;
                        PosaljiPorukuUDP("ALARM_AKTIVIRAN;Server je obavesten. Osoblje je na putu.", ipEP, udpSocket);
                        Console.WriteLine($"Server: ALARM aktiviran u apartmanu {apartman.BrojApartmana}.");
                        PosaljiZadatakOsoblju(new ZadatakOsoblju { TipZadatka = "sanacija_alarma", BrojApartmana = apartman.BrojApartmana, Opis = $"Sanirajte alarm u apartmanu {apartman.BrojApartmana}." });
                    }
                    break;

                default:
                    PosaljiPorukuUDP("GRESKA;Nepoznata komanda.", ipEP, udpSocket);
                    break;
            }
        }

        private static void AzurirajBoravke(Socket udpSocket)
        {
            while (ServerRadi)
            {
                Thread.Sleep(30000); // Similacija protoka vremena (svakih 30s)
                Console.WriteLine("Server: Ažuriranje statusa boravaka...");
                var aktivniBoravci = gostiIP.ToList();
                foreach (var entry in aktivniBoravci)
                {
                    var apartman = entry.Value;
                    if (apartman.PreostaloNoci > 0)
                    {
                        apartman.PreostaloNoci--;
                    }
                    if (apartman.PreostaloNoci == 0)
                    {
                        string poruka = $"ZAVRSETAK_BORAVKA;Vaš boravak u apartmanu {apartman.BrojApartmana} je završen. Ukupni račun: {apartman.UkupniRacun} dinara.";
                        PosaljiPorukuUDP(poruka, entry.Key, udpSocket);
                        Console.WriteLine($"Server: Obavešten gost u apartmanu {apartman.BrojApartmana} o završetku boravka.");
                    }
                }
            }
        }

        private static void ObradiPotvrduOsoblja(Poruka poruka, Socket staffSocket)
        {
            var zadatak = poruka.Sadrzaj as ZadatakOsoblju;
            Console.WriteLine($"Server: Primljena potvrda od osoblja za zadatak '{zadatak.TipZadatka}' u apartmanu {zadatak.BrojApartmana}.");
            var apartman = apartmani.FirstOrDefault(a => a.BrojApartmana == zadatak.BrojApartmana);
            if (apartman != null)
            {
                if (zadatak.TipZadatka == "ciscenje")
                {
                    apartman.StanjeApartmana = StanjeApartmana.Prazan;
                    Console.WriteLine($"Server: Apartman {apartman.BrojApartmana} je sada prazan i spreman za nove goste.");
                }
                else if (zadatak.TipZadatka == "sanacija_alarma")
                {
                    apartman.StanjeAlarma = StanjeAlarma.Normalno;
                    Console.WriteLine($"Server: Alarm u apartmanu {apartman.BrojApartmana} je saniran.");
                }
            }
        }

        private static void PosaljiPorukuUDP(string poruka, IPEndPoint ep, Socket udpSocket)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(poruka);
            udpSocket.SendTo(buffer, ep);
        }

        private static void PosaljiPoruku(Socket s, Poruka poruka)
        {
            byte[] buffer = Serijalizuj(poruka);
            s.Send(buffer);
        }

        private static void PosaljiZadatakOsoblju(ZadatakOsoblju zadatak)
        {
            if (povezanoOsoblje.Count > 0)
            {
                var poruka = new Poruka { Tip = "ZADATAK", Sadrzaj = zadatak };
                PosaljiPoruku(povezanoOsoblje.First(), poruka);
            }
        }

        private static byte[] Serijalizuj<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private static T Deserijalizuj<T>(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                var bf = new BinaryFormatter();
                return (T)bf.Deserialize(ms);
            }
        }
    }
}