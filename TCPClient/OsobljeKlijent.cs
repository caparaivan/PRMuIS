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

namespace TCPClient
{
    public class OsobljeKlijent
    {
        private const int Port = 50001;
        private static IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, Port);

        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("Dobrodošli, osoblje!");
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Connect(serverEP);
                Console.WriteLine("Povezano sa serverom. Čekam zadatke...");
                clientSocket.Blocking = false;

                while (true)
                {
                    List<Socket> checkRead = new List<Socket> { clientSocket };
                    List<Socket> checkError = new List<Socket> { clientSocket };
                    Socket.Select(checkRead, null, checkError, 1000);

                    if (checkRead.Count > 0)
                    {
                        byte[] buffer = new byte[1024];
                        try
                        {
                            int bytesRead = clientSocket.Receive(buffer);
                            if (bytesRead == 0)
                            {
                                Console.WriteLine("Server je prekinuo vezu.");
                                break;
                            }
                            var poruka = Deserijalizuj<Poruka>(buffer);
                            if (poruka.Tip == "ZADATAK")
                            {
                                var zadatak = poruka.Sadrzaj as ZadatakOsoblju;
                                Console.WriteLine($"\nPrimljen zadatak od servera: {zadatak.Opis}");
                                Thread.Sleep(3000);
                                Console.WriteLine($"Zadatak '{zadatak.TipZadatka}' u apartmanu {zadatak.BrojApartmana} je završen.");
                                PosaljiPotvrdu(clientSocket, zadatak);
                            }
                        }
                        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.WouldBlock || ex.SocketErrorCode == SocketError.IOPending)
                        {
                            // Nema podataka, nastavi
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Greška tokom komunikacije sa serverom: {ex.Message}");
                            break;
                        }
                    }
                    if (checkError.Count > 0)
                    {
                        Console.WriteLine("Greška na soketu. Prekid veze.");
                        break;
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Greška pri povezivanju sa serverom: {ex.Message}");
            }
            finally
            {
                clientSocket.Close();
                Console.WriteLine("Aplikacija osoblja je završena.");
            }
        }

        private static void PosaljiPotvrdu(Socket s, ZadatakOsoblju zadatak)
        {
            var poruka = new Poruka { Tip = "POTVRDA_ZADATKA", Sadrzaj = zadatak };
            byte[] buffer = Serijalizuj(poruka);
            s.Send(buffer);
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