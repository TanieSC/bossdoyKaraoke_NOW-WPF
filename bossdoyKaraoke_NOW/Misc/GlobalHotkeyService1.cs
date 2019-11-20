using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//FROM: Alexey Golub https://tyrrrz.me/blog/wndproc-in-wpf
namespace bossdoyKaraoke_NOW.Misc
{
    class GlobalHotkeyService1 : IDisposable
    {
        [DllImport("user32.dll", EntryPoint = "RegisterHotKey", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        private const int HOTKEY_ID = 9000;

        private readonly SpongeWindow _sponge;

        public GlobalHotkeyService1()
        {
            _sponge = new SpongeWindow();
            _sponge.WndProcCalled += (s, e) => ProcessMessage(e);

            RegisterMessages();
            
        }

        private void RegisterMessages()
        {
            // Register F1 as a global hotkey
            var id = 1;
            RegisterHotKey(_sponge.Handle, id, 0, 0x70);
        }

        private void ProcessMessage(Message message)
        {
            // Only interested in hotkey messages so skip others
            if (message.Msg != 0x0312)
                return;

            // Get hotkey id
            var id = message.WParam.ToInt32();
            int vkey = (((int)message.LParam >> 16) & 0xFFFF);

    
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
