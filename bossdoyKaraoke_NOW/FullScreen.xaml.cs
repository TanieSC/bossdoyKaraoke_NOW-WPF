using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using bossdoyKaraoke_NOW.FormControl;
using bossdoyKaraoke_NOW.Graphic;
using bossdoyKaraoke_NOW.Media;
using SharpDX;
using SharpDX.Mathematics.Interop;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;

namespace bossdoyKaraoke_NOW
{
    /// <summary>
    /// Interaction logic for FullScreen.xaml
    /// </summary>
    public partial class FullScreen : Window
    {
        private VideoImage _videoImage;

        public FullScreen()
        {
            InitializeComponent();
            _videoImage = new VideoImage(true);
            full_video_screen.Child = _videoImage;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _videoImage.Dispose();
            Hide();
        }
    }
}
