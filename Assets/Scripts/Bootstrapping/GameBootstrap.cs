using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Scripting;

namespace Bootstrapping
{
    /// <summary>
    /// startup point of application,
    /// set run in backround to preserve lagging on alt tabbing
    /// check if we are client or server, for client we have to create client world
    /// </summary>
    [Preserve] // without it il2cpp strips the class, netcode only finds it by reflection
    public class GameBootstrap : ClientServerBootstrap
    {
        public const int Port = 7979; // used both as the listen port and the connect port

        // called by unity before the first scene loads, decides which worlds get created
        public override bool Initialize(string defaultWorldName)
        {
            UnityEngine.Application.runInBackground = true;
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
}
