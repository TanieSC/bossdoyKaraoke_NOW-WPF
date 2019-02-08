using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;

namespace bossdoyKaraoke_NOW.Graphic
{
    class DeviceContext : IDeviceContext
    {


        private Control _control;
        private IDeviceManager _deviceManager;
        private D2D.DeviceContext _videoContext;
        private D2D.DeviceContext _cdgContext;
        private D3D11.Device _d3d11Device;
        private DXGI.Device _dxgiDevice;
        private DXGI.SwapChain _swapChain;
        private DXGI.Surface _backBuffer;
        private D2D.Bitmap1 _targetBitmap;

        public D2D.DeviceContext VideoContext
        {
            get
            {
                return _videoContext;
            }
        }

        public D2D.DeviceContext CdgContext
        {
            get
            {
                return _cdgContext;
            }
        }

        public DW.Factory DWFactory
        {
            get
            {
               return _deviceManager.DWFactory;
            }
        }

        public DeviceContext(IDeviceManager deviceManager, Control control)
        {
            if (deviceManager == null)
            {
                throw new ArgumentNullException("deviceManager", "Device Manager cannot be null");
            }

            if (control == null)
            {
                throw new ArgumentNullException("control", "Control cannot be null");
            }

            _deviceManager = deviceManager;
            _d3d11Device = deviceManager.D3d11Device;
            _videoContext = new D2D.DeviceContext(deviceManager.D2D1device, D2D.DeviceContextOptions.None); //deviceManager.D2dContext;
            _cdgContext = new D2D.DeviceContext(deviceManager.D2D1device, D2D.DeviceContextOptions.None); //deviceManager.D2dContext;

            _dxgiDevice = deviceManager.DxgiDevice;
            _control = control;

            CreateSwapChain();

        }

        public void Present()
        {
            _swapChain.Present(0, DXGI.PresentFlags.None);
        }

        public void UpdateScreen()
        {
            ResizeScreen();
           // CreateSwapChain();
        }

        private void CreateSwapChain()
        {
            var swapChainDesc = new DXGI.SwapChainDescription()
            {
                BufferCount = 1,
                Usage = DXGI.Usage.RenderTargetOutput,
                OutputHandle = _control.Handle,
                IsWindowed = true,
                ModeDescription = new DXGI.ModeDescription(0, 0, new DXGI.Rational(60, 1), DXGI.Format.B8G8R8A8_UNorm),
                SampleDescription = new DXGI.SampleDescription(1, 0),
                SwapEffect = DXGI.SwapEffect.Discard
            };

            _swapChain = new DXGI.SwapChain(_dxgiDevice.GetParent<DXGI.Adapter>().GetParent<DXGI.Factory>(), _d3d11Device, swapChainDesc);
            // BackBuffer
            _backBuffer = DXGI.Surface.FromSwapChain(_swapChain, 0);
            //BackBuffer DeviceContext
            _targetBitmap = new D2D.Bitmap1(_videoContext, _backBuffer);
            _videoContext.Target = _targetBitmap;
        }

        private void ResizeScreen()
        {
            if (_videoContext != null) _videoContext.Target = null;
            if (_backBuffer != null)  _backBuffer.Dispose();
            if (_targetBitmap != null) _targetBitmap.Dispose();
            _swapChain.ResizeBuffers(1, 0, 0, DXGI.Format.B8G8R8A8_UNorm, DXGI.SwapChainFlags.None);
            _backBuffer = DXGI.Surface.FromSwapChain(_swapChain, 0);
            _targetBitmap = new D2D.Bitmap1(_videoContext, _backBuffer);
            _videoContext.Target = _targetBitmap;
        }

        ~DeviceContext()
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
                    _videoContext.Dispose();
                    _cdgContext.Dispose();
                    //_d3d11Device.Dispose();
                    //_dxgiDevice.Dispose();
                    _swapChain.Dispose();
                    _backBuffer.Dispose();
                    _targetBitmap.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DeviceContext() {
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
