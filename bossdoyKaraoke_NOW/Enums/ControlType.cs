using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Enums
{
    public class ControlType
    {
        private static Name _controlTypeName;

        public enum Name
        {
            None,
            TreeView,
            ListView        
        }


        /// <summary>
        /// 
        /// </summary>
        public static Name ControlName
        {
            get
            {
                return _controlTypeName;
            }
            set
            {
                _controlTypeName = value;
            }

        }
    }
}
