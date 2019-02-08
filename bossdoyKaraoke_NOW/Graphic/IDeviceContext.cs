using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;

namespace bossdoyKaraoke_NOW.Graphic
{
    public interface IDeviceContext : IDisposable
    {
        D2D.DeviceContext VideoContext { get;  }
        D2D.DeviceContext CdgContext { get; }
        DW.Factory DWFactory { get; }
        void UpdateScreen();
        void Present();
    }
}
