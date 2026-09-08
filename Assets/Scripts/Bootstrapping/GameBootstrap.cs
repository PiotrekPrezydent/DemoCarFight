using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class GameBootstrap : ClientServerBootstrap
{
    public const int Port = 7979;

    public override bool Initialize(string defaultWorldName)
    {
        var address = "127.0.0.1";
        var clientOnly = false;
        
        var args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] != "-client")
                continue;

            clientOnly = true;
            if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                address = args[i + 1];
        }

        AutoConnectPort = Port;
        
        DefaultConnectAddress = NetworkEndpoint.Parse(address,Port);
        
        if (clientOnly)
        {
            CreateClientWorld("ClientWorld");
            return true;
        }
        
        return base.Initialize(defaultWorldName);
    }
}
