﻿using System;
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

namespace bossdoyKaraoke_NOW
{
    /// <summary>
    /// Interaction logic for Preferences.xaml
    /// </summary>
    public partial class Preferences : Window
    {

        public Preferences()
        {
            InitializeComponent();            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            panelPreviewScreen.Dispose();
            bgVideoPreviewScreen.Dispose();
        }
    }
}
