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
    public interface IDeviceManager : IDisposable
    {
        D3D11.Device D3d11Device { get; }
        D2D.Device D2D1device { get; }
        DXGI.Device DxgiDevice { get; }
        DW.Factory DWFactory { get; }
    }
}
