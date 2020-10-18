using System;

namespace MultiServerBasic
{
    public class NetworkServerAction
    {
        private readonly Server _server; //Server linked

        /// <summary>Initialise the server actions.</summary>
        /// <param name="server">Server to link.</param>
        public NetworkServerAction(Server server)
        {
            _server = server;
        }

        #region SendAction

        /// <summary>Send a packet to to attribute the client id.</summary>
        /// <param name="id">ID of the client to send this message.</param>
        public void SendConnected(int id)
        {
            Packet connectedPacket = new Packet((int) Packet.ServerPacketIDReference.Connected);
            connectedPacket.Write(id);
            
            _server.GetClients()[id].Tcp.SendPacket(connectedPacket); //send "connected" Packet to assign an id
            
            connectedPacket.Dispose();
            
            Console.WriteLine("Client connection confirmation sent to client ID : "+_server.GetClients()[id].ID);
        }

        #endregion


        #region ReceiveAction

        /// <summary>Action to do when a client ask to resend a message.</summary>
        /// <param name="packet">Packet receive.</param>
        /// <param name="clientID">Client ID of the sender</param>
        public void ResendMessage(Packet packet,int clientID)
        {
            Packet debugMessage = new Packet((int) Packet.ServerPacketIDReference.DebugMessage);
            debugMessage.Write("you ask to resend this message : "+packet.ReadString(true));
            
            _server.GetClients()[clientID].Tcp.SendPacket(debugMessage);
            
            debugMessage.Dispose();
        }

        #endregion

    }
}