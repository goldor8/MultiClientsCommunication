using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client instance;

    public static string IP;
    public static int Port;
    public static int ID;
    public static bool IsConnected = false;
    
    public static TCP Tcp;
    
    private static Dictionary<int, ActionHandler> _packetHandlers;
    private delegate void ActionHandler(Packet packet);

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("An instance already exists, destroying this instance");
            Destroy(this);
        }
        InitialisePacketsIDReader();
    }

    public void OnApplicationQuit()
    {
        Disconnect();
    }

    public void Start()
    {
        instance.Connect("127.0.0.1",28707);
        NetworkAction.AskToResendMesage("hello");
    }

    public void Connect(string ipPARAM,int portPARAM)
    {
        IP = ipPARAM;
        Port = portPARAM;
        
        Tcp = new TCP();
        Tcp.Connect();
    }

    private void Disconnect()
    {
        if (IsConnected)
        {
            IsConnected = false;
            
            Tcp.DisConnect();
            //udp.socket.Close();
        }
    }
    
    public class TCP
    {
        public TcpClient socket;
        public NetworkStream stream;
        public Packet receiveData;
        private Byte[] receiveBuffer;

        public void Connect()
        {
            Debug.Log("Connection to "+IP+":"+Port+"...");
            socket = new TcpClient {ReceiveBufferSize = 4096, SendBufferSize = 4096};
            socket.BeginConnect(IP,Port,ConnectCallback,socket);
            receiveBuffer = new byte[4096];
        }

        public void DisConnect()
        {
            Debug.Log("Disconnecting...");
            socket.Close();
            stream = null;
            receiveData.Dispose();
            receiveBuffer = null;
        }

        public void ConnectCallback(IAsyncResult result)
        {
            socket.EndConnect(result);
            if (!socket.Connected)
            {
                Debug.Log("Error the connection as failed ! (unconnected after the connection request)");
                return;
            }
            
            Debug.Log("Successfully connected to the server : "+IP);

            stream = socket.GetStream();
            receiveData = new Packet();

            stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);
        }

        public void ReceiveCallback(IAsyncResult result)
        {
            int packetLenght = stream.EndRead(result);
            if (packetLenght <= 0)
            {
                
                Debug.Log("Error cause by the lenght of the packet < 1 the client while be disconnected");
                instance.Disconnect();
                return;
            }
            
            Byte[] data = new byte[packetLenght];
            Array.Copy(receiveBuffer,data,packetLenght);

            HandlePacket(new Packet(data));
            stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);
        }

        public void HandlePacket(Packet packet)
        {
            Debug.Log("Incomming packet with Id : "+packet.ReadInt(false));
            
            int packetID = packet.ReadInt(true);
            _packetHandlers[packetID](packet);
        }
        
        public void SendPacket(Packet packet)
        {
            if (socket != null)
            {
                    stream.BeginWrite(packet.ReadAllBytes(), 0, packet.GetLenght(), null, null);
            }
            else
            {
                Debug.Log("TCP isn't connected cannot send data");
            }
        }
    }

    private void InitialisePacketsIDReader()
    {
        _packetHandlers = new Dictionary<int, ActionHandler>()
        {
            {(int) Packet.ServerPacketIDReference.Connected, NetworkAction.Connnected},
            {(int) Packet.ServerPacketIDReference.DebugMessage, NetworkAction.DebugMessage}
        };
    }

}
