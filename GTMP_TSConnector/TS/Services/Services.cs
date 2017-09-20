using GTMP_TSConnector.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using anyID = System.UInt16;
using uint64 = System.UInt64;

namespace GTMP_TSConnector
{
    internal class Services
    {
        public static bool IsChannel = false;
        private static IntPtr nickname;
        public static ulong IDChanDefault = 22;
        public static int refresh = 0;
        public static ulong ServerID;
        private static ulong chanId;
        private static string pseudonyme;

        public static void Gestion(string result)
        {
            try
            {
                ServerID = TSPlugin.Instance.Functions.getCurrentServerConnectionHandlerID();
                if (GetMyId(ServerID) == 0) return;
                string[] args = result.Split(new string[] { "/" }, StringSplitOptions.None);
                SoundPlayer audio_on = new SoundPlayer(Resources.on);
                LoadServer(ServerID);
                switch (args[1].ToString())
                {
                    case "login":
                        if (IsChannel == false)
                        {
                            if (args[2] == null) return;
                            IsChannel = true;
                            LoadName(ServerID);
                            TSPlugin.Instance.Functions.getChannelOfClient(ServerID, GetMyId(ServerID), ref chanId);
                            TSPlugin.Instance.Functions.requestClientMove(ServerID, GetMyId(ServerID), IDChanDefault, "dsf84dsf7", null);
                            TSPlugin.Instance.Functions.setClientSelfVariableAsInt(ServerID, (IntPtr)ClientProperties.CLIENT_INPUT_MUTED, 1);
                            pseudonyme = args[2].ToString().Replace("%20", " ");
                            ChangeName(ServerID, pseudonyme);
                            audio_on.Play();

                            IntPtr plugins = IntPtr.Zero;
                            TSPlugin.Instance.Functions.getChannelClientList(ServerID, IDChanDefault, ref plugins);
                            List<ushort> pluginsID = ReadAndFreeUInt16List(plugins);
                            foreach(ushort id in pluginsID) { MuteClient(ServerID, id); }
                            TSPlugin.Instance.Functions.flushClientSelfUpdates(ServerID, null);
                        }
                        break;
                    case "vocal":
                        var pl = args[2].ToString().Replace("%7B%22", "").Replace("%22", "").Replace("%20", " ").Replace("%7D", "");
                        //TSPlugin.Instance.Functions.printMessageToCurrentTab("Vocal : " + pl.ToString());
                        string[] vtc = pl.Split(new string[] { ";" }, StringSplitOptions.None);
                        if (vtc[1] == "true")
                        {
                            TSPlugin.Instance.Functions.setClientSelfVariableAsInt(ServerID, (IntPtr)ClientProperties.CLIENT_INPUT_MUTED, 0);
                        }
                        else
                        {
                            TSPlugin.Instance.Functions.setClientSelfVariableAsInt(ServerID, (IntPtr)ClientProperties.CLIENT_INPUT_MUTED, 1);
                        }
                        break;
                    case "players":
                        refresh = 0;
                        if (IsChannel == false) { Gestion("/login/" + pseudonyme + "/"); }
                        if (args[2] == null) return;
                        if (args[2].ToString() == "0")
                        {
                            //Log.Info("Players!");
                            IntPtr plugins = IntPtr.Zero;
                            TSPlugin.Instance.Functions.getChannelClientList(ServerID, IDChanDefault, ref plugins);
                            var pluginsID = ReadAndFreeUInt16List(plugins);
                            foreach (anyID pls in pluginsID)
                            {
                                MuteClient(ServerID, pls);
                            }
                        }
                        else
                        {
                            List<KeyValuePair<string, string[]>> client = new List<KeyValuePair<string, string[]>>();
                            var clients0 = args[2].ToString().Replace("%20", " ").Replace("%7C", "|");
                            string[] clients = clients0.Split(new string[] { ";" }, StringSplitOptions.None);
                            foreach (string cls in clients)
                            {

                                string[] namedistance = cls.Split('|');
                                string[] testes = new string[] { namedistance[1], namedistance[2] };
                                client.Add(new KeyValuePair<string, string[]>(namedistance[0], testes));
                            }

                            IntPtr plugins = IntPtr.Zero;
                            TSPlugin.Instance.Functions.getChannelClientList(ServerID, IDChanDefault, ref plugins);
                            List<ushort> pluginsID = ReadAndFreeUInt16List(plugins);
                            pluginsID.Remove(GetMyId(ServerID));
                            foreach (anyID pls in pluginsID)
                            {
                                IntPtr name = IntPtr.Zero;
                                TSPlugin.Instance.Functions.getClientVariableAsString(ServerID, pls, (IntPtr)ClientProperties.CLIENT_NICKNAME, ref name);

                                var pair = client.Find(x => x.Key == IntPtrString(name));
                                if (pair.Value != null)
                                {
                                    string[] test = pair.Value;
                                    if (test == null) { Log.Info("ERREUR AVEC PAIR VALUE"); }
                                    UMuteClient(ServerID, pls);
                                    double distance = Convert.ToDouble(test[0].Replace(".", ","));
                                    double range = Convert.ToDouble(test[1].Replace(".", ","));
                                    float coef = (float)((range - distance) / range);
                                    float baseVolume = -30.0f;
                                    float volumeModifier = baseVolume + (coef * 33);
                                    /*
                                    Log.Info("Name: " + pair.Key.ToString());
                                    Log.Info("Distance: " + distance.ToString());
                                    Log.Info("Range: " + range.ToString());
                                    Log.Info("Coef: " + coef.ToString());
                                    Log.Info("VolMod: " + volumeModifier.ToString());
                                    */
                                    var error = TSPlugin.Instance.Functions.setClientVolumeModifier(ServerID, pls, volumeModifier);
                                    if (error != 0) { Log.Info("ERROR"); }

                                }
                                else
                                {
                                    MuteClient(ServerID, pls);
                                }
                            }

                        }

                        break;
                    case "debug":
                        if (args[2] != null)
                        {
                            /*
                            string test = args[2].Replace("%7B%22", "").Replace("%22", "").Replace("%20", " ").Replace("%7D", "").Replace("%7C", "|").ToString();
                            string[] vtcc = test.Split(new string[] { "|" }, StringSplitOptions.None);
                            TS3_VECTOR position = new TS3_VECTOR()
                            {
                                x = float.Parse(vtcc[1], CultureInfo.InvariantCulture.NumberFormat),
                                y = float.Parse(vtcc[2], CultureInfo.InvariantCulture.NumberFormat),
                                z = float.Parse(vtcc[3], CultureInfo.InvariantCulture.NumberFormat)
                            };
                            TS3_VECTOR nullvector = new TS3_VECTOR()
                            {
                                x = 0.0f,
                                y = 0.0f,
                                z = 0.0f
                            };
                            IntPtr plugins = IntPtr.Zero;
                            TSPlugin.Instance.Functions.getChannelClientList(ServerID, IDChanDefault, ref plugins);
                            var pluginsID = ReadAndFreeUInt16List(plugins);
                            TSPlugin.Instance.Functions.systemset3DListenerAttributes(ServerID, position, nullvector, nullvector);

                            Settings3d(ServerID, 1.0f, 10.0f);
                            for (int i = 0; pluginsID[i] != 0; ++i)
                            {
                                var error = ChannelSet3DAttributes(ServerID, pluginsID[i], position);

                            }*/
                        }
                        break;


                    case "copradio":
                        /*
                        if (args[2] != null)
                        {
                            SoundPlayer radio_on = new SoundPlayer(Properties.Resources._70106__justinbw__function_beep);
                            List<string> client = new List<string>();
                            var clients0 = args[2].ToString().Replace("%20", " ");
                            string[] clients = clients0.Split(new string[] { ";" }, StringSplitOptions.None);
                            foreach (string cls in clients)
                            {
                                client.Add(cls);
                            }
                            IntPtr plugins = IntPtr.Zero;
                            TSPlugin.Instance.Functions.getChannelClientList(ServerID, IDChanDefault, ref plugins);
                            var pluginsID = ReadAndFreeUInt16List(plugins);
                            pluginsID.Remove(GetMyId(ServerID));
                            foreach (var pls in pluginsID)
                            {

                                IntPtr name = IntPtr.Zero;
                                TSPlugin.Instance.Functions.getClientVariableAsString(ServerID, pls, (IntPtr)ClientProperties.CLIENT_NICKNAME, ref name);

                                if (client.Contains(IntPtrString(name)))
                                {
                                    //TSPlugin.Instance.Functions.printMessageToCurrentTab("Players : " + IntPtrString(name).ToString() + " - OK (" + pls.ToString() + ")");
                                    UMuteClient(ServerID, pls);
                                }

                                else
                                {
                                    //TSPlugin.Instance.Functions.printMessageToCurrentTab("Players : " + IntPtrString(name).ToString() + " - NOK (" + pls.ToString() + ")");
                                    MuteClient(ServerID, pls);
                                }

                            }
                        }
                         */
                        break;
                }
                TSPlugin.Instance.Functions.flushClientSelfUpdates(ServerID, null);
            }
            catch (Exception e)
            {
                Log.Info(e.ToString());
            }
        }

        /*****************************************************************************************/
        /*                                  FUNCTION ZONE                                        */
        /*****************************************************************************************/

        public static void SetClientVolume(uint64 serverID, anyID ID, int volume)
        {
            TSPlugin.Instance.Functions.setClientVolumeModifier(serverID, ID, volume);
        }

        public static string ServerName()
        {
            IntPtr serverNamePtr = IntPtr.Zero;
            uint64 schID = TSPlugin.Instance.Functions.getCurrentServerConnectionHandlerID();
            TSPlugin.Instance.Functions.getServerVariableAsString(schID, (IntPtr)VirtualServerProperties.VIRTUALSERVER_NAME, ref serverNamePtr);
            return Marshal.PtrToStringAnsi(serverNamePtr);
        }

        public static void Logout()
        {
            uint64 serverID = TSPlugin.Instance.Functions.getCurrentServerConnectionHandlerID();
            SoundPlayer audio_off = new SoundPlayer(GTMP_TSConnector.Properties.Resources.on);
            IsChannel = false;
            TSPlugin.Instance.Functions.requestClientMove(serverID, GetMyId(serverID), chanId, "", null);
            TSPlugin.Instance.Functions.setClientSelfVariableAsInt(serverID, (IntPtr)ClientProperties.CLIENT_INPUT_MUTED, 0);
            ChangeName(serverID, IntPtrString(nickname));

            TSPlugin.Instance.Functions.flushClientSelfUpdates(serverID, null);

            TSPlugin.Instance.chanId = 0;
            audio_off.Play();
            IntPtr plugins = IntPtr.Zero;
            TSPlugin.Instance.Functions.getClientList(serverID, ref plugins);
            var pluginsID = ReadAndFreeUInt16List(plugins);
            for (int i = 0; pluginsID[i] != 0; ++i)
            {
                UMuteClient(serverID, pluginsID[i]);
                SetClientVolume(serverID, pluginsID[i], 0);
            }
            TSPlugin.Instance.Functions.flushClientSelfUpdates(serverID, null);
        }

        public static uint ChannelSet3DAttributes(uint64 scHandlerID, anyID clientID, TS3_VECTOR position)
        {
            uint error = TSPlugin.Instance.Functions.channelset3DAttributes(scHandlerID, clientID, ref position);
            return error;
        }

        private static uint Settings3d(uint64 serverID, float distanceFactor, float rolloffScale = 200f)
        {
            uint error = TSPlugin.Instance.Functions.systemset3DSettings(serverID, distanceFactor, rolloffScale);
            return error;
        }

        public static void MuteClient(uint64 serverID, anyID ID)
        {
            TSPlugin.Instance.Functions.requestMuteClients(serverID, ref ID, null);
        }

        public static void UMuteClient(uint64 serverID, anyID ID)
        {
            TSPlugin.Instance.Functions.requestUnmuteClients(serverID, ref ID, null);
        }

        private static void LoadName(uint64 serverID)
        {
            IntPtr name_client = (IntPtr)ClientProperties.CLIENT_NICKNAME;
            TSPlugin.Instance.Functions.getClientSelfVariableAsString(serverID, name_client, ref nickname);
        }

        private static void LoadServer(uint64 serverID)
        {
            IntPtr name_server = (IntPtr)VirtualServerProperties.VIRTUALSERVER_NAME;
            TSPlugin.Instance.Functions.getServerVariableAsString(serverID, name_server, ref TSPlugin.Instance.servername);
        }

        private static void ChangeName(uint64 serverID, string nickname)
        {
            IntPtr name_client = (IntPtr)ClientProperties.CLIENT_NICKNAME;
            TSPlugin.Instance.Functions.setClientSelfVariableAsString(serverID, name_client, nickname);
        }

        public static anyID GetMyId(uint64 serverConnectionHandlerID)
        {
            anyID myID = 0;
            if (!IsConnected(serverConnectionHandlerID)) return myID;
            TSPlugin.Instance.Functions.getClientID(serverConnectionHandlerID, ref myID);
            return myID;
        }

        public static bool IsConnected(uint64 serverConnectionHandlerID)
        {
            int result = 0;
            TSPlugin.Instance.Functions.getConnectionStatus(serverConnectionHandlerID, ref result);
            return result != 0;
        }

        public static ushort ReadUInt16(IntPtr source, int offset)
        {
            short[] sValue = { 0 };
            ushort[] uValue = { 0 };
            sValue[0] = Marshal.ReadInt16(source, offset);
            Buffer.BlockCopy(sValue, 0, uValue, 0, 2);
            return uValue[0];
        }

        public static unsafe string IntPtrString(IntPtr nativeUtf8)
        {
            byte* bytes = (byte*)nativeUtf8.ToPointer();
            int size = 0;
            while (bytes[size] != 0)
            {
                ++size;
            }
            byte[] buffer = new byte[size];
            Marshal.Copy(nativeUtf8, buffer, 0, size);
            return Encoding.UTF8.GetString(buffer);
        }

        public static List<ushort> ReadAndFreeUInt16List(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero) return null;
            List<ushort> result = new List<ushort>();
            for (int offset = 0; ; offset += sizeof(ushort))
            {
                ushort item = (ushort)Marshal.ReadInt16(pointer, offset);
                if (item == 0) break;
                result.Add(item);
            }
            TSPlugin.Instance.Functions.freeMemory(pointer);
            return result;
        }
    }
}