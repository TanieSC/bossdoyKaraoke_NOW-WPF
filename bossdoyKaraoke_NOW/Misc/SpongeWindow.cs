using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//FROM: Alexey Golub https://tyrrrz.me/blog/wndproc-in-wpf
namespace bossdoyKaraoke_NOW.Misc
{
    public sealed class SpongeWindow : NativeWindow
    {
        public event EventHandler<Message> WndProcCalled;

        public SpongeWindow()
        {
            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m)
        {
            WndProcCalled?.Invoke(this, m);
            base.WndProc(ref m); // don't forget this line
        }
    }
}
