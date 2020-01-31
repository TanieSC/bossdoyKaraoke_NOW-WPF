using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Enums
{
    public class ConnectionEnum
    {
        private static ConnectionType _currentConnection;

        public enum ConnectionType
        {
            BlueTooth,
            LanCable,
            WiFi,
            WiFiDirect
        }

        public static ConnectionType CurrentConnection
        {
            get
            {
                return _currentConnection;
            }
            set
            {
                _currentConnection = value;
            }

        }
    }
}
