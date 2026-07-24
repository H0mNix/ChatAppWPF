using ChatServer.NET.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class Client
    {
        public string Username { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }

        PacketReader _packetReader;

        public Client(TcpClient client)
        {
            ClientSocket = client;
            UID = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());

            var opcode = _packetReader.ReadByte(); // Read the opcode
            Username = _packetReader.ReadMessage();

            Console.WriteLine($"[{DateTime.Now}] : Client has connected with the username : {Username}");

            Task.Run(() => Process());
        }

        void Process()
        {
            while(true)
            {
                try
                {
                    var opcode = _packetReader.ReadByte();
                    switch(opcode)
                    {
                        case 5: // Message opcode
                            var message = _packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}] : {Username} : {message}");
                            Program.BroadcastMessage($"[{DateTime.Now}] : {Username} : {message}");
                            break;
                        default:
                            Console.WriteLine($"[{DateTime.Now}] : Unknown opcode received from {Username} with UID {UID}");
                            break;
                    }
                }
                catch(Exception)
                {
                    Console.WriteLine($"{UID.ToString()} has disconnected!");
                    Program.BroadcastDisconnection(UID.ToString());
                    ClientSocket.Close();
                    break;
                }
            }
        }
    }
}
