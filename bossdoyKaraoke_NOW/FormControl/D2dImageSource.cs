using System;
using System.Windows.Forms;
using Rendercontext = bossdoyKaraoke_NOW.Graphic;

namespace bossdoyKaraoke_NOW.FormControl
{
    public partial class D2dImageSource : UserControl
    {
        private Timer _videotimer;

        public Rendercontext.IDeviceContext RenderContext;

        private int _timerInterval = 50;
        public int TimerInterval
        {
            get { return _timerInterval; }
            set { _timerInterval = value; }
        }

        public D2dImageSource()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            RenderContext = new Rendercontext.DeviceContext(Rendercontext.DeviceManager.Instance, this);
        }

        public virtual void Render()
        {

        }

        private void D2dImageSource_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;

            _videotimer = new Timer();
            _videotimer.Interval = _timerInterval;
            _videotimer.Enabled = false;
            _videotimer.Tick += Videotimer_Tick;
            _videotimer.Start();

            Disposed += D2dImageSource_Disposed;

        }

        private void Videotimer_Tick(object sender, EventArgs e)
        {

            if (DesignMode) return;
            if (RenderContext == null) return;

            UpdateRender();

        }

        public virtual void D2dImageSource_Disposed(object sender, EventArgs e)
        {
            if (RenderContext != null)
            {
                RenderContext.Dispose();
                RenderContext = null;
                Rendercontext.DeviceManager.Instance.Dispose();
            }
        }

        private void D2dImageSource_Resize(object sender, EventArgs e)
        {
            if (DesignMode) return;
            if (RenderContext == null) return;

            RenderContext.UpdateScreen();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            BackColor = System.Drawing.Color.Black;
        }

        private void UpdateRender()
        {
            if (DesignMode) return;
            if (RenderContext == null) return;

            Render();
            RenderContext.Present();

        }

    }
}
