using System;

namespace MultiServerBasic
{
    internal class Program
    {
        

        public static void Main(string[] args)
        {
            int i = 0;
            var server = new Server(2,20,28707);
            string test = Console.ReadLine();
            
            if (test == "start") {
                packettest(100);
            }
            
            void packettest(int maxI) {
                server.GetThreadManager().ExecuteOnMainThread(() => {
                    
                    var packet = new Packet((int) Packet.ServerPacketIDReference.DebugMessage);
                    packet.Write("hello " + i);
                    server.SendTCPMessageToAllConnection(packet);
                    packet.Dispose();
                    i++;
                    if (i < maxI)
                    {
                        packettest(maxI);
                    }
                });
            }
            
        }
    }
}