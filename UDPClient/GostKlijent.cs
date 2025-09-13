using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPClient
{
    public class GostKlijent
    {
        private const int Port = 50001;
        private static readonly IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, Port); //localhost 128.0.1
        private static bool UTokuboravka = false;
        private static Socket clientSocket;

        public static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Dobrodošli u hotelski sistem!");

            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) // UDP socket
            {
                Blocking = false
            };

            _ = Task.Run(() => OsluskujServer()); // _ zbog nepotrebnog povratnog podatka

            while (true)
            {
                if (UTokuboravka)
                {
                    IspisiMeniZaBoravak();
                    string opcija = Console.ReadLine()?.Trim();
                    switch (opcija)
                    {
                        case "1":
                            PosaljiPoruku("NARUDZBINA;minibar");
                            break;
                        case "2":
                            PosaljiPoruku("ALARM");
                            break;
                        default:
                            Console.WriteLine("Nepoznata opcija. Pokušajte ponovo.");
                            break;
                    }
                }
                else
                {
                    IspisiMeni();
                    string opcija = Console.ReadLine()?.Trim();
                    switch (opcija)
                    {
                        case "1":
                            ProveriDostupnost();
                            break;
                        case "2":
                            await RezervacijaFlow();
                            break;
                        case "5":
                            Console.WriteLine("Hvala i doviđenja!");
                            return;
                        default:
                            Console.WriteLine("Nepoznata opcija. Pokušajte ponovo.");
                            break;
                    }
                }
            }
        }

        private static async Task OsluskujServer()
        {
            byte[] buffer = new byte[1024];
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0); // cuva adresu servera odakle je stigla poruka

            while (true)
            {
                var checkRead = new List<Socket> { clientSocket };
                Socket.Select(checkRead, null, null, 1000); //provjera ima li sta za citanje

                if (checkRead.Count > 0)
                {
                    try
                    {
                        int len = clientSocket.ReceiveFrom(buffer, ref remoteEP); // popunjava udaljene adrese
                        string poruka = Encoding.UTF8.GetString(buffer, 0, len);
                        ObradiPorukuServera(poruka);
                    }
                    catch (SocketException ex) when (
                        ex.SocketErrorCode == SocketError.WouldBlock || ex.SocketErrorCode == SocketError.IOPending)
                    {
                        // nema podataka
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Greška pri prijemu poruke: {ex.Message}");
                    }
                }

                await Task.Delay(50);
            }
        }

        private static void ObradiPorukuServera(string poruka)
        {
            if (poruka.StartsWith("ZAVRSETAK_BORAVKA"))
            {
                var delovi = poruka.Split(';');
                Console.WriteLine($"\n{delovi[1]}");
                Console.Write("Unesite broj kreditne kartice za plaćanje: ");
                string brKartice = Console.ReadLine()?.Trim();
                PosaljiPoruku($"PLATI;{brKartice}");
                UTokuboravka = false;
            }
            else if (poruka.StartsWith("POTVRDA_REZERVACIJE"))
            {
                UTokuboravka = true;
                Console.WriteLine("\nRezervacija uspešna! Stisnite enter da uđete u meni tokom boravka.");
            }
            else if (poruka.StartsWith("POTVRDA_PLATE"))
            {
                Console.WriteLine($"\n{poruka.Substring("POTVRDA_PLATE;".Length)}");
                Console.WriteLine("Plaćanje završeno. Stistnite enter ako zelite da ponovo rezervisete sobu");
            }
            else if (poruka.StartsWith("LISTA_SOBA"))
            {
                Console.WriteLine($"{poruka.Substring("LISTA_SOBA;".Length)}");
            }
            else
            {
                Console.WriteLine($"\nOdgovor servera: {poruka}");
            }
        }

        private static void IspisiMeni()
        {
            Console.WriteLine("\n=== Glavni meni ===");
            Console.WriteLine("1. Provera dostupnosti soba");
            Console.WriteLine("2. Rezervacija apartmana");
            Console.WriteLine("5. Izlaz");
            Console.Write("Izaberite opciju: ");
        }

        private static void IspisiMeniZaBoravak()
        {
            Console.WriteLine("\n=== Meni tokom boravka ===");
            Console.WriteLine("1. Narudžbina (minibar)");
            Console.WriteLine("2. Aktiviraj alarm");
            Console.Write("Izaberite opciju: ");
        }

        private static void ProveriDostupnost()
        {
            PosaljiPoruku("PROVERI_DOSTUPNOST");
        }

        private static async Task RezervacijaFlow()
        {
            Console.Write("\nUnesite broj apartmana za rezervaciju: ");
            string brApt = Console.ReadLine()?.Trim();
            Console.Write("Unesite broj gostiju: ");
            string brGostiju = Console.ReadLine()?.Trim();
            if (!int.TryParse(brGostiju, out int brGostijuInt) || brGostijuInt <= 0)
            {
                Console.WriteLine("Neispravan broj gostiju. Pokušajte ponovo.");
                return;
            }

            for (int i = 0; i < brGostijuInt; i++)
            {
                Console.WriteLine($"\nGost {i+1}:");
                Console.Write("Ime: ");
                string ime = Console.ReadLine()?.Trim() ?? "";

                Console.Write("Prezime: ");
                string prezime = Console.ReadLine()?.Trim() ?? "";

                Console.Write("Broj lične karte / pasoša: ");
                string dokument = Console.ReadLine()?.Trim() ?? "";
            }


            Console.Write("Unesite broj noći: ");
            string brNoci = Console.ReadLine()?.Trim();

            PosaljiPoruku($"REZERVACIJA;{brApt};{brGostiju};{brNoci}");
        }

        private static void PosaljiPoruku(string tekst)
        {
            byte[] buf = Encoding.UTF8.GetBytes(tekst);
            try
            {
                clientSocket.SendTo(buf, serverEP);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška pri slanju poruke: {ex.Message}");
            }
        }
    }
}
