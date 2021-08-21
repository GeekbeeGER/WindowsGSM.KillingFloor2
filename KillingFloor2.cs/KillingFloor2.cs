﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Query;
using WindowsGSM.GameServer.Engine;
using System.IO;
using System.Linq;
using System.Net;

namespace WindowsGSM.Plugins
{
    public class KillingFloor2 : SteamCMDAgent
    {
        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.KillingFloor2", // WindowsGSM.XXXX
            author = "Jamie96ITS",
            description = "WindowsGSM plugin for supporting KillingFloor2 Dedicated Server",
            version = "1.0",
            url = "https://github.com/Jamie96ITS/WindowsGSM.KillingFloor2", // Github repository link (Best practice)
            color = "#FF0000" // Color Hex
        };

        // - Settings properties for SteamCMD installer
        public override bool loginAnonymous => true;
        public override string AppId => "232130"; // Game server appId

        // - Standard Constructor and properties
        public KillingFloor2(ServerConfig serverData) : base(serverData) => base.serverData = _serverData = serverData;
        private readonly ServerConfig _serverData;
        public string Error, Notice;


        // - Game server Fixed variables
        public override string StartPath => @"Binaries\win64\kfserver"; // Game server start path
        public string FullName = "Killing Floor 2 Dedicated Server"; // Game server FullName
        public bool AllowsEmbedConsole = true;  // Does this server support output redirect?
        public int PortIncrements = 1; // This tells WindowsGSM how many ports should skip after installation
        public object QueryMethod = new A2S(); // Query method should be use on current server type. Accepted value: null or new A2S() or new FIVEM() or new UT3()


        // - Game server default values
        public string Port = "7777"; // Default port
        public string QueryPort = "27015"; // Default query port
        public string Defaultmap = "kf-bioticslab"; // Default map name
        public string Maxplayers = "6"; // Default maxplayers
        public string Additional = "?adminpassword=admin"; // Additional server start parameter


        // - Create a default cfg for the game server after installation
        public async void CreateServerCFG()
        {
			//Not needed. GUI configurable settings work on command line, and the rest are generated by the server. 
		}

        // - Start server function, return its Process to WindowsGSM
        public async Task<Process> Start()
        {


            // Prepare start parameter
            string param = string.IsNullOrWhiteSpace(_serverData.ServerMap) ? string.Empty : $" {_serverData.ServerMap}";
            param += string.IsNullOrWhiteSpace(_serverData.ServerIP) ? string.Empty : $"?MultiHome={_serverData.ServerIP}";
            param += string.IsNullOrWhiteSpace(_serverData.ServerPort) ? string.Empty : $"?Port={_serverData.ServerPort}";
			param += string.IsNullOrWhiteSpace(_serverData.ServerQueryPort) ? string.Empty : $"?QueryPort={_serverData.ServerQueryPort}";
            param += $"{_serverData.ServerParam}";
			
			// Saw this in another plugin from Kickbut101. Comment on it is right, this was useful. 
            // Output the startupcommands used. Helpful for troubleshooting server commands and testing them out - leaving this in because it's helpful af.	
			var startupCommandsOutputTxtFile = ServerPath.GetServersServerFiles(_serverData.ServerID, "startupCommandsUsed.log");
            File.WriteAllText(startupCommandsOutputTxtFile, $"{param}");
			
            // Prepare Process
            var p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = ServerPath.GetServersServerFiles(_serverData.ServerID),
                    FileName = ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath),
                    Arguments = param,
                    WindowStyle = ProcessWindowStyle.Minimized,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            // Set up Redirect Input and Output to WindowsGSM Console if EmbedConsole is on
            if (AllowsEmbedConsole)
            {
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                var serverConsole = new ServerConsole(_serverData.ServerID);
                p.OutputDataReceived += serverConsole.AddOutput;
                p.ErrorDataReceived += serverConsole.AddOutput;

                // Start Process
                try
                {
                    p.Start();
                }
                catch (Exception e)
                {
                    Error = e.Message;
                    return null; // return null if fail to start
                }

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                return p;
            }

            // Start Process
            try
            {
                p.Start();
                return p;
            }
            catch (Exception e)
            {
                Error = e.Message;
                return null; // return null if fail to start
            }
        }


		// - Stop server function
        public async Task Stop(Process p)
        {
            await Task.Run(() =>
            {
                if (p.StartInfo.CreateNoWindow)
                {
                    Functions.ServerConsole.SetMainWindow(p.MainWindowHandle);
                    Functions.ServerConsole.SendWaitToMainWindow("^c");
					
                }
                else
                {
                    Functions.ServerConsole.SetMainWindow(p.MainWindowHandle);
                    Functions.ServerConsole.SendWaitToMainWindow("^c");
                }
            });
			await Task.Delay(20000);
        }
		
    }
}