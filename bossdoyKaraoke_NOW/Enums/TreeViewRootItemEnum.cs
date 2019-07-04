using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace bossdoyKaraoke_NOW.Enums
{
    class TreeViewRootItemEnum
    {
        /// <summary>
        /// Enums to Set root node of the treeview
        /// </summary>
        public enum RootNode
        {
            SONG_QUEUE,
            MY_FAVORITES,
            ADD_FAVORITES,
            MY_COMPUTER,
            ADD_FOLDER

        }

        /// <summary>
        /// Enums to get the connections of node on the treeview and yield return
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
