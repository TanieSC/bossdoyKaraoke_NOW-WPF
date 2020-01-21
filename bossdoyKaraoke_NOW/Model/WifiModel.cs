using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Model
{
    class WifiModel
    {
        string _displayName;
        string _profileName;
        //bool _isConnected;
        string _ssid;
        byte[] _ssidBytes;
        string _ssidHex;
        string _key;

        public string DisplayName { get; set; }
        public string ProfileName { get; set; }
        public string SSID { get; set; }
        public byte[] SSIDBytes { get; set; }
        public string SSIDHex { get; set; }
        public string Key { get; set; }

    }
}
