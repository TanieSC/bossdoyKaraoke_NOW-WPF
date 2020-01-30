using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using bossdoyKaraoke_NOW.Enums;
using bossdoyKaraoke_NOW.Interactivity;
using bossdoyKaraoke_NOW.Model;
using MaterialDesignThemes.Wpf;
using NativeWifi;
using static NativeWifi.WlanClient;
using static bossdoyKaraoke_NOW.Enums.ConnectionEnum;

namespace bossdoyKaraoke_NOW.ViewModel
{
    class ConnectionVModel : IConnectionVModel, INotifyPropertyChanged
    {
        private WlanClient _client;
        private WlanInterface _wlanIface;
        private ListBox _device_list;
        public ObservableCollection<string> Items { get; } = new ObservableCollection<string>();

        private ICommand _loadedCommand;
        private ICommand _selectedItemCommand;
        private ICommand _connectCommand;
        private ICommand _clientConnectCommand;
        private PackIconKind _iconClientConnect = PackIconKind.EthernetCableOff;

        public ConnectionVModel()
        {
            _client = new WlanClient();
            _wlanIface = _client.Interfaces.FirstOrDefault();

            var gg = NetworkInterface.GetAllNetworkInterfaces();
        }

        public PackIconKind IconClientConnect
        {
            get
            {
                return _iconClientConnect;
            }
            set
            {
                _iconClientConnect = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadedCommand
        {
            get
            {
                return _loadedCommand ?? (_loadedCommand = new RelayCommand(x =>
                {
                    _device_list = x as ListBox;

                   


                    if (_wlanIface == null)
                    {
                        Console.WriteLine("No Wifi Interface available!");
                        throw new Exception("No Wifi Interface available!");
                    }
                    else
                    {
                        if (ConnectionEnum.CurrentConnection == ConnectionEnum.ConnectionType.WiFi)
                        {
                            // Lists all available networks
                            Wlan.WlanAvailableNetwork[] networks = _wlanIface.GetAvailableNetworkList(0);
                            
                            foreach (Wlan.WlanAvailableNetwork network in networks)
                            {
                                var name = GetStringForSSID(network.dot11Ssid);
                                var ssidBytes = Encoding.Default.GetBytes(name);

                                if (network.flags == Wlan.WlanAvailableNetworkFlags.HasProfile)
                                {
                                }
                              //  var rrr = Wlan.WlanAvailableNetworkFlags.Connected;

                                WifiModel wifi = new WifiModel
                                {
                                    DisplayName = name,
                                    ProfileName = network.profileName,
                                    SSID = name,
                                    SSIDBytes = ssidBytes,
                                   // SSIDHex = StringToHex(ssidBytes),
                                    Key = ""
                                };

                                if (network.flags == Wlan.WlanAvailableNetworkFlags.HasProfile)
                                {
                                    Console.WriteLine("WIFI " + wifi.DisplayName);
                                }

                                Items.Add(wifi.DisplayName);
                            }

                            // device_list.ItemsSource = Items; //planning to move this to backgrond worker
                        }
                        else if (ConnectionEnum.CurrentConnection == ConnectionEnum.ConnectionType.WiFiDirect)
                        {
                        }
                        else
                        {
                        }
                    }

                }));
            }
        }


        public ICommand ClientConnectCommand
        {
            get
            {
                return _clientConnectCommand ?? (_clientConnectCommand = new RelayCommand(x =>
                {
                    CurrentConnection = (ConnectionType)x;

                    if (CurrentConnection == ConnectionType.WiFi)
                    {
                        IconClientConnect = PackIconKind.Wifi;
                    }
                    if (CurrentConnection == ConnectionType.WiFiDirect)
                    {
                        IconClientConnect = PackIconKind.WifiFavourite;
                    }
                    if (CurrentConnection == ConnectionType.BlueTooth)
                    {
                        IconClientConnect = PackIconKind.Bluetooth;
                    }
                }));
            }
        }

        public ICommand SelectedItemCommand
        {
            get
            {
                return _selectedItemCommand ?? (_selectedItemCommand = new RelayCommand(x =>
                {
                    Connect(x as string);
                }));
            }
        }

        public ICommand ConnectCommand
        {
            get
            {
                return _connectCommand ?? (_connectCommand = new RelayCommand(x =>
                {

                }));
            }
        }

        public void Connect(string ssid)
        {
            string profileName = ssid; // this is also the SSID
            string mac = StringToHex(profileName);
            //string profileXml = string.Format("<?xml version=\"1.0\"?>\r\n<WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\">\r\n\t<name>{0}</name>\r\n\t<SSIDConfig>\r\n\t\t<SSID>\r\n\t\t\t<hex>{1}</hex>\r\n\t\t\t<name>{0}</name>\r\n\t\t</SSID>\r\n\t</SSIDConfig>\r\n\t<connectionType>ESS</connectionType>\r\n\t<connectionMode>auto</connectionMode>\r\n\t<MSM>\r\n\t\t<security>\r\n\t\t\t<authEncryption>\r\n\t\t\t\t<authentication>WPA2PSK</authentication>\r\n\t\t\t\t<encryption>AES</encryption>\r\n\t\t\t\t<useOneX>false</useOneX>\r\n\t\t\t</authEncryption>\r\n\t\t\t<sharedKey>\r\n\t\t\t\t<keyType>passPhrase</keyType>\r\n\t\t\t\t<protected>true</protected>\r\n\t\t\t\t<keyMaterial>{2}</keyMaterial>\r\n\t\t\t</sharedKey>\r\n\t\t</security>\r\n\t</MSM>\r\n</WLANProfile>\r\n", ssid, mac, "BX_744ky19");

            string profileXml = string.Format("<?xml version=\"1.0\"?><WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\"><name>{0}</name><SSIDConfig><SSID><hex>{1}</hex><name>{0}</name></SSID></SSIDConfig><connectionType>ESS</connectionType><connectionMode>auto</connectionMode><MSM><security><authEncryption><authentication>WPA2PSK</authentication><encryption>AES</encryption><useOneX>false</useOneX></authEncryption><sharedKey><keyType>passPhrase</keyType><protected>true</protected><keyMaterial>{2}</keyMaterial></sharedKey></security></MSM></WLANProfile>", ssid, mac, "BX_744ky19");

            try
            {
               // var f = _wlanIface.GetProfileXml(profileName);
               // _wlanIface.SetProfile(Wlan.WlanProfileFlags.AllUser, profileXml, true);
                _wlanIface.Connect(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profileName);                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Occured!");
                //throw;
            }
        }


        public static bool IsNetworkAvailable(long minimumSpeed)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return false;

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // discard because of standard reasons
                if ((ni.OperationalStatus != OperationalStatus.Up) ||
                    (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) ||
                    (ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel))
                    continue;

                // this allow to filter modems, serial, etc.
                // I use 10000000 as a minimum speed for most cases
                if (ni.Speed < minimumSpeed)
                    continue;

                // discard virtual cards (virtual box, virtual pc, etc.)
                if ((ni.Description.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (ni.Name.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0))
                    continue;

                // discard "Microsoft Loopback Adapter", it will not show as NetworkInterfaceType.Loopback but as Ethernet Card.
                if (ni.Description.Equals("Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase))
                    continue;

                

                return true;
            }
            return false;
        }

        static string GetStringForSSID(Wlan.Dot11Ssid ssid)
        {
            return Encoding.UTF8.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }

        string StringToHex(string ssidStr)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = Encoding.Default.GetBytes(ssidStr);

            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(Convert.ToString(byStr[i], 16));
            }
            return (sb.ToString().ToUpper());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
