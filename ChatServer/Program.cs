using ChatServer;
using ChatServer.NET.IO;
using System.Net;
using System.Net.Sockets;

class Program
{
    static List<Client> _users;
    static TcpListener _listener;
    static void Main(string[] args)
    {
        _users = new List<Client>();
        _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
        _listener.Start();

        while(true)
        {
            var client = new Client(_listener.AcceptTcpClient());
            _users.Add(client);

            // Broadcast the connection to everyone on the server
            BroadcastConnection();
        }


    }

    static void BroadcastConnection()
    {
        foreach(var user in _users)
        {
            foreach(var usr in _users) // Nested loop to send the connection message to all users
            {
                var broadcastPacket = new PacketBuilder();
                broadcastPacket.WriteOpCode(1); // Assuming 1 is the opcode for connection message
                broadcastPacket.WriteString(usr.Username);
                broadcastPacket.WriteString(usr.UID.ToString());
                user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
            }
        }
    }

    public static void BroadcastMessage(string message)
    {
        foreach(var user in _users)
        {
            var msgPaket = new PacketBuilder();
            msgPaket.WriteOpCode(5);
            msgPaket.WriteString(message);
            user.ClientSocket.Client.Send(msgPaket.GetPacketBytes());
        }
    }

    public static void BroadcastDisconnection(string uid)
    {
        var disconnectedUser = _users.FirstOrDefault(u => u.UID.ToString() == uid);
        _users.Remove(disconnectedUser);

        foreach (var user in _users)
        {
            var broadcastPacket = new PacketBuilder();
            broadcastPacket.WriteOpCode(10);
            broadcastPacket.WriteString(uid);
            user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
        }

        BroadcastMessage($"{disconnectedUser.Username} has disconnected!");
    }
}