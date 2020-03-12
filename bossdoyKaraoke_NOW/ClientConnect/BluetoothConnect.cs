using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.ClientConnect
{
    class BluetoothConnect
    {
        public void Start()
        {
            Thread t = new Thread(new ThreadStart(StartBluetooth));
            t.Start();
        }

        private void StartBluetooth()
        {
            try
            {
               // if(BluetoothRadio)
            }
            catch (Exception ex)
            {
            }
        }
    }
}