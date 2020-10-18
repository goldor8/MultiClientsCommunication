using System;
using System.Net;
using System.Net.Sockets;

namespace MultiServerBasic
{
    public class Client {
        public int ID; //ID assign to this client.
        public TCP Tcp; //Tcp connection.
        
        private Server _server; //Server where is this client.

        /// <summary>Initialise the client.</summary>
        /// <param name="ID">ID linked to the client.</param>
        /// <param name="server">Server linked to the client.</param>
        public Client(int ID,Server server) {
            this._server = server; //Set the server where is this client.
            this.ID = ID; //Set the id.
        }

        /// <summary>Connect the client with the tcp and udp protocol.</summary>
        /// <param name="tcpSocket">Socket to link client and server in tcp.</param>
        /// <param name="udpSocket">Socket to link client and server in udp.</param>
        public void Connect(TcpClient tcpSocket,UdpClient udpSocket) {
            //Connect the tcp
            Tcp = new TCP(this);
            Tcp.Connect(tcpSocket);
            //todo: connect the udp
        }

        /// <summary>Disconnect the client (tcp and udp).</summary>
        private void Disconnect() {
            Tcp.Disconnect();
            //Udp.Disconnect();
            _server.GetClients().Remove(ID); //Remove the client frome the server client list.
        }
        
        public class TCP {
            private readonly Client _client; //Client of the tcp.
            private TcpClient _socket; //Socket that link the client to the server.
            private NetworkStream _stream; //The stream of tcp data.
            private Byte[] _receiveBuffer;

            /// <summary>Initialise the tcp protocol to be used.</summary>
            /// <param name="client">Client to be linked to the tcp protocol.</param>
            public TCP(Client client) {
                this._client = client; //Set the client.
                _receiveBuffer = new byte[4096]; //Set the max receive Buffer.
            }
            
            /// <summary>Disconnect the tcp protocol.</summary>
            public void Disconnect() {
                Console.WriteLine("Disconnecting client with ID : "+_client.ID);
                
                _socket.Close();
                _socket = null;
                _stream = null;
                _receiveBuffer = null;
            }

            /// <summary>Connect the tcp protocol.</summary>
            /// <param name="tcpSocket">tcp socket connected to the client.</param>
            public void Connect(TcpClient tcpSocket) {
                _socket = tcpSocket; //Set the socket (link between client and server).
                
                if (!_socket.Connected) {
                    Console.WriteLine("Error the connection as failed ! (unconnected after the connection request)");
                    return;
                }
                
                Console.WriteLine("Client ID : "+_client.ID+" successfully connected");

                _stream = _socket.GetStream(); //Get the tcp data stream.

                _stream.BeginRead(_receiveBuffer, 0, 4096, ReceiveCallback, null); //Listen incoming tcp data.
            }

            //Function call after receive data from client.
            private void ReceiveCallback(IAsyncResult result) {
                int packetLenght = _stream.EndRead(result); //Stop listen incoming tcp data.

                if (packetLenght <= 0) {
                    //I dont know what that mean but that seem to be call when the connection is interrupted between client and server.
                    Console.WriteLine("Error cause by the lenght of the packet <= 0 the client while be disconnected");
                    _client.Disconnect(); //Disconnect safely the client.
                    
                    return;
                }
                
                Byte[] data = new byte[packetLenght]; //Prepare to store the incoming data.
                Array.Copy(_receiveBuffer,data,packetLenght); //Free the receive buffer to be used by the listener.
                
                _stream.BeginRead(_receiveBuffer, 0, 4096, ReceiveCallback, null); //Restart listen incoming tcp data.

                _client.HandlePacket(new Packet(data)); //Convert into a packet and handle it.
            }

            /// <summary>Send data from server to client.</summary>
            /// <param name="packet">Packet to send.</param>
            public void SendPacket(Packet packet) {
                //If connection always exist.
                if (_socket != null) {
                    _stream.BeginWrite(packet.ReadAllBytes(), 0, packet.GetLenght(), null, null); //Send data to the client.
                }
                else {
                    Console.WriteLine("TCP client with client ID : "+_client.ID+" isn't connected, cannot send data");
                }
            }
            
        }

        public class UDP
        {
            public IPEndPoint EndPoint;

            public void Connect()
            {
                
            }
        }
        
        /// <summary>Handle data to be readied.</summary>
        /// <param name="packet">Packet to read.</param>
        public void HandlePacket(Packet packet) {
            //make sure the action will be executed in the right order.
            _server.GetThreadManager().ExecuteOnMainThread(() => {
                    
                int packetID = packet.ReadInt(true); //Read the packet ID.
                _server.PacketHandlers[packetID](packet,ID); //Execute the function assign to this packet ID.
                    
            });
        }
        
    }
}