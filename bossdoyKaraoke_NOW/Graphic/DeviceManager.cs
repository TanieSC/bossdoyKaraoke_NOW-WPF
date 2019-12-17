using System;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;

namespace bossdoyKaraoke_NOW.Graphic
{
    class DeviceManager : IDeviceManager
    {
        private static DeviceManager _instance;
        private D3D.FeatureLevel[] _featureLevels = {
             D3D.FeatureLevel.Level_11_1,
             D3D.FeatureLevel.Level_11_0,
             D3D.FeatureLevel.Level_10_1,
             D3D.FeatureLevel.Level_10_0,
             D3D.FeatureLevel.Level_9_3,
             D3D.FeatureLevel.Level_9_2,
             D3D.FeatureLevel.Level_9_1
            };

        public static DeviceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DeviceManager();
                }
                return _instance;
            }
        }

        public D3D11.Device D3d11Device { get; private set; }
        public D2D.Device D2D1device { get; private set; }
        public DW.Factory DWFactory { get; private set; }
        public DXGI.Device DxgiDevice { get; private set; }

        public DeviceManager()
        {
            D3d11Device = new D3D11.Device(D3D.DriverType.Hardware,
#if DEBUG
               D3D11.DeviceCreationFlags.Debug |
#endif
               D3D11.DeviceCreationFlags.BgraSupport, _featureLevels);

            DxgiDevice = D3d11Device.QueryInterface<D3D11.Device>().QueryInterface<DXGI.Device>();

            DWFactory = new DW.Factory();

            D2D1device = new D2D.Device(DxgiDevice);

        }


        ~DeviceManager()
        {
            Dispose(false);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    D3d11Device.Dispose();
                    DxgiDevice.Dispose();
                    D2D1device.Dispose();
                    DWFactory.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DeviceManager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion


    }
}
