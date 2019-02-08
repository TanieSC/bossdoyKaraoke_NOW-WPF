using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Credits to Andy : https://www.technical-recipes.com/2017/how-to-use-interaction-triggers-to-handle-user-initiated-events-in-wpf-mvvm/
namespace bossdoyKaraoke_NOW.Interactivity
{
    public class EventArgs<T> : EventArgs
    {
        public EventArgs(T value)
        {
            Value = value;
        }

        public T Value { get; private set; }
    }
}
