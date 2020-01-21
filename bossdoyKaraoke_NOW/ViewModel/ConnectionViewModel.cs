using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using bossdoyKaraoke_NOW.Interactivity;
using bossdoyKaraoke_NOW.Model;
using NativeWifi;
using static NativeWifi.WlanClient;

namespace bossdoyKaraoke_NOW.ViewModel
{
    class ConnectionViewModel
    {
        WlanClient _client;
        WlanInterface _wlanIface;

        private ICommand _loaded;

        public ConnectionViewModel() {
            _client = new WlanClient();
            _wlanIface = _client.Interfaces.FirstOrDefault();
        }

        public ICommand Loaded
        {
            get
            {
                return _loaded ?? (_loaded = new RelayCommand(x =>
                {
                    if (_wlanIface == null)
                    {
                        Console.WriteLine("No Wifi Interface available!");
                        throw new Exception("No Wifi Interface available!");
                    }
                    else
                    {
                        // Lists all available networks
                        Wlan.WlanAvailableNetwork[] networks = _wlanIface.GetAvailableNetworkList(0);
                        foreach (Wlan.WlanAvailableNetwork network in networks)
                        {
                            var name = GetStringForSSID(network.dot11Ssid);
                            var ssidBytes = Encoding.Default.GetBytes(name);

                            WifiModel wifi = new WifiModel
                            {
                                DisplayName = name,
                                ProfileName = network.profileName,
                                SSID = name,
                                SSIDBytes = ssidBytes,
                                SSIDHex = StringToHex(ssidBytes),
                                Key = ""
                        };
                        }
                    }

                }));
            }
        }

        static string GetStringForSSID(Wlan.Dot11Ssid ssid)
        {
            return Encoding.UTF8.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }

        string StringToHex(byte[] byStr)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(Convert.ToString(byStr[i], 16));
            }
            return (sb.ToString().ToUpper());
        }
    }
}
