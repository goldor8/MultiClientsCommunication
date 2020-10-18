using UnityEngine;

public class NetworkAction
{
    public static void AskToResendMesage(string message)
    {
        Packet packet = new Packet((int) Packet.ClientPacketIDReference.ResendMessage);
        packet.Write(message);
        Client.Tcp.SendPacket(packet);
        packet.Dispose();
    }
    
    public static void Connnected(Packet packet)
    {
        Client.ID = packet.ReadInt(true);
        Debug.Log("Connected with ID : "+Client.ID);
        packet.Dispose();
    }
    
    public static void DebugMessage(Packet packet)
    {
        string message = packet.ReadString(true);
        Debug.Log(message);
        packet.Dispose();
    }
}
