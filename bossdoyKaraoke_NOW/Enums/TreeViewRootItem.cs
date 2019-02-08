using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace bossdoyKaraoke_NOW.Enums
{
    class TreeViewRootItem
    {
        public enum RootNode
        {
            SONG_QUEUE,
            MY_FAVORITES,
            ADD_FAVORITES,
            MY_COMPUTER,
            ADD_FOLDER

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static IEnumerable<TreeViewItem> Collect(ItemCollection nodes)
        {
            foreach (TreeViewItem node in nodes)
            {
                yield return node;

                foreach (var child in Collect(node.Items))
                    yield return child;
            }
        }
    }
}
