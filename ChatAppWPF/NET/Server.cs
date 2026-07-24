using ChatClient.NET.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.NET
{
    public class Server
    {
        TcpClient _client;
        public PacketReader PacketReader;

        
        public event Action connectedEvent;
        public event Action messageReceivedEvent;
        public event Action UserDisconnectEvent;

        public Server()
        {
            _client = new TcpClient();
        }

        public void ConnectToServer(string username)
        {
            if(!_client.Connected)
            {
                _client.Connect("127.0.0.1", 7891);
                PacketReader = new PacketReader(_client.GetStream());

                if(!string.IsNullOrEmpty(username))
                {
                    var connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteString(username);
                    _client.Client.Send(connectPacket.GetPacketBytes());
                }
                ReadPackets();                                                                         
            }
        }

        private void ReadPackets()
        {
            Task.Run(() =>
            {
                while(true)
                {
                    var opCode = PacketReader.ReadByte();
                    switch(opCode)
                    {
                        case 1: // Connection message
                            connectedEvent?.Invoke();
                            break;

                        case 5: // Message message
                            messageReceivedEvent?.Invoke();
                            break;

                        case 10: // Disconnection message
                            UserDisconnectEvent?.Invoke();
                            break;
                        default:
                            Console.WriteLine("ah yess....");
                            break;
                    }
                }
            });
        }

        public void SendMessageToServer(string message)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(5); // Message opcode
            messagePacket.WriteString(message);
            _client.Client.Send(messagePacket.GetPacketBytes());
        }
    }
}
