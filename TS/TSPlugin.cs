using System;
using System.Linq;
using System.Net;
using System.Timers;

using uint64 = System.UInt64;

namespace GTMP_TSConnector
{
    public class TSPlugin
    {

        #region initplugin
        private readonly static Lazy<TSPlugin> _instance = new Lazy<TSPlugin>(() => new TSPlugin());

        private TSPlugin()
        {

        }

        public static TSPlugin Instance
        {
            get
            {
                return _instance.Value;
            }
        }
        #endregion

        public TS3Functions Functions { get; set; }
        public string PluginID { get; set; }

        public string PluginName    = "GTMP Task Force";
        public string PluginVersion = "3.3";
        public int ApiVersion       = 22;
        public string Author        = "Copyright (c) 2017 - Production par Djoe45";
        public string Description   = "";
        public uint64 chanId        = 0;
        public ulong[] ChannelAllowed = { 22, 7, 8, 10, 11, 13, 14, 27, 136 };
        public IntPtr nickname;
        public IntPtr servername;

        public delegate void NewMessageDelegate(string NewMessage);
        public static DateTime login_now;

        public static WebServer ws = new WebServer(SendResponse, "http://localhost:25984/");
        public static Timer timer = new Timer(1000);

        public int Init()
        {
            login_now = DateTime.Now;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            ws.Run();
            return 0;
        }

        

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!Exports.ServerConnected) return;
            if (Services.IsChannel)
            {
                Services.refresh += 1;
                if (Services.refresh > 7)
                {
                    Services.Logout();
                }

                ulong idchannelactuel = 0;
                Instance.Functions.getChannelOfClient(Services.ServerID, Services.GetMyId(Services.ServerID), ref idchannelactuel);
                if (!ChannelAllowed.Contains(idchannelactuel))
                {
                    Instance.Functions.requestClientMove(Services.ServerID, Services.GetMyId(Services.ServerID), Services.IDChanDefault, "dsf84dsf7", null);
                }
            }
        }

        public static string SendResponse(HttpListenerRequest request)
        {
            string url   = request.RawUrl.ToString();
            string agent = request.Headers.ToString();

            Services.Gestion(url.ToString());
            //if (url != "/update/")  //Instance.Functions.printMessageToCurrentTab("Debug1 : " + url.ToString());

            //if (!agent.Contains("sdch")) if (url != "/favicon.ico") Services.gestion(url.ToString());
            return string.Format("<HTML><BODY>{0} - {1} - {2}</BODY></HTML>", Instance.PluginName, Instance.Author, DateTime.Now);
        }

    }
}