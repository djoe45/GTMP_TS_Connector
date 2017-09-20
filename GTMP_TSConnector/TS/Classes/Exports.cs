using RGiesecke.DllExport;
using System;
using System.Runtime.InteropServices;
using anyID = System.UInt16;
using uint64 = System.UInt64;

namespace GTMP_TSConnector
{

    public class Exports
    {

        public static string pluginID = null;

        public static string Status;
        public static bool ServerConnected;

        static bool Is64Bit()                                                                                    { return Marshal.SizeOf(typeof(IntPtr)) == 8; }
        [DllExport] public static string ts3plugin_name()                                                        { return TSPlugin.Instance.PluginName;        }
        [DllExport] public static string ts3plugin_version()                                                     { return TSPlugin.Instance.PluginVersion;     }
        [DllExport] public static int ts3plugin_apiVersion()                                                     { return TSPlugin.Instance.ApiVersion;        }
        [DllExport] public static string ts3plugin_author()                                                      { return TSPlugin.Instance.Author;            }
        [DllExport] public static string ts3plugin_description()                                                 { return TSPlugin.Instance.Description;       }
        [DllExport] public static void ts3plugin_setFunctionPointers(TS3Functions funcs)                         { TSPlugin.Instance.Functions = funcs;        }

        [DllExport] public static int ts3plugin_init()
        {
            //TSPlugin.Instance.Functions.setPluginMenuEnabled(pluginID, 0, 1);
            return TSPlugin.Instance.Init();
        }

        [DllExport] public static void ts3plugin_freeMemory(IntPtr data)                                         { Marshal.FreeHGlobal(data);                  }
        [DllExport] public static void ts3plugin_currentServerConnectionChanged(ulong serverConnectionHandlerID) {                                             }

        [DllExport] public static void ts3plugin_registerPluginID(string id)
        {
            var functs = TSPlugin.Instance.Functions;

            //pluginID = id;

            TSPlugin.Instance.PluginID = id;
        }

        [DllExport] public static int ts3plugin_requestAutoload()
        {
            return 1;  /* 1 = request autoloaded, 0 = do not request autoload */
        }

        [DllExport] public static void ts3plugin_shutdown()
        {
            TSPlugin.ws.Stop();
            TSPlugin.timer.Stop();

            //TSPlugin.Instance.Functions.setPluginMenuEnabled(pluginID, 0, 0);
            //ts3Functions.setPluginMenuEnabled(pluginID, Rubberband_Disabled, 0);


        }

        [DllExport]
        public static void ts3plugin_onConnectStatusChangeEvent(UInt64 serverConnectionHandlerID, int newStatus, int errorNumber)
        {
            if (newStatus == 4)
            {
                ServerConnected = true;
            } else
            {
                ServerConnected = false;
            }
            //Log.Info(newStatus.ToString());
        }

        [DllExport]
        public static void onClientMoveEvent(UInt64 serverConnectionHandlerID, anyID clientID, uint64 oldChannelID, uint64 newChannelID,int visibility,char moveMessage)
        {
            if (clientID != (Services.GetMyId(Services.ServerID))) return;
            //if (Services.IsChannel == false) { Services.Gestion("/login/" + Services.pseudonyme + "/"); }

        }

    }
}