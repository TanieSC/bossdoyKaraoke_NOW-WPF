using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using bossdoyKaraoke_NOW.Interactivity;
using bossdoyKaraoke_NOW.Misc;
using static bossdoyKaraoke_NOW.Misc.GlobalHotkeyService;

namespace bossdoyKaraoke_NOW.ViewModel
{
    class KeyGestureVModel : IKeyGestureVModel
    {
        private GlobalHotkeyService _ctlO;
        private GlobalHotkeyService _ctlA;
        private GlobalHotkeyService _ctlP;
        private GlobalHotkeyService _ctlE;
        private ICommand _keyGestureCommand;

        public ICommand KeyGestureCommand
        {
            get
            {
                return _keyGestureCommand ?? (_keyGestureCommand = new RelayCommand(x => ChangeGroup()));
            }
        }

        public KeyGestureVModel()
        {
            _ctlO = new GlobalHotkeyService(Key.O, KeyModifier.Ctrl, OnHotKeyHandler);
            _ctlA = new GlobalHotkeyService(Key.A, KeyModifier.Ctrl, OnHotKeyHandler);
            _ctlP = new GlobalHotkeyService(Key.P, KeyModifier.Ctrl, OnHotKeyHandler);
            _ctlE = new GlobalHotkeyService(Key.E, KeyModifier.Ctrl, OnHotKeyHandler);
        }

        private void OnHotKeyHandler(GlobalHotkeyService hotKey)
        {
            if (hotKey.KeyModifiers == KeyModifier.Ctrl)
            {
                switch (hotKey.Key)
                {
                    case Key.O:

                        break;
                    case Key.A:

                        break;
                    case Key.P:

                        break;
                    case Key.E:

                        break;
                }
            }
        }
    }
}
