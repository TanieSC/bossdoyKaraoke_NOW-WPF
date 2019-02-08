using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using bossdoyKaraoke_NOW.BackGroundWorker;
using bossdoyKaraoke_NOW.Model;

namespace bossdoyKaraoke_NOW.Media
{
    public class SongsSourceListView : ISongsSourceListView //IListViewItemProvider<TrackInfo>
    {
        public List<TrackInfo> Items { get; private set; }

        public SongsSourceListView()
        {
            Items = new List<TrackInfo>();
        }

        public void AddRange(List<TrackInfo> items)
        {
            Items.AddRange(items);
        }

        public void Add(TrackInfo item)
        {
            Items.Add(item);
        }
    }

    public class SongsSourceListViewItems : ISongsSourceListViewItems
    {
        private readonly SongsSourceListView _songsSourceListView;

        public List<TrackInfo> Items
        {
            get
            {
                return _songsSourceListView.Items;
            }
        }

        public SongsSourceListViewItems(List<TrackInfo> selectedSongs)
        {
            _songsSourceListView = new SongsSourceListView();

            for (int i = 0; i < selectedSongs.Count; i++)
            {
                TrackInfo trackInfo = new TrackInfo { ID = (i + 1).ToString(), Name = selectedSongs[i].Name, Artist = selectedSongs[i].Artist, Duration = selectedSongs[i].Duration, FilePath = selectedSongs[0].FilePath };
                _songsSourceListView.Add(trackInfo);
            }
        }

        //private readonly List<TrackInfo> _items;
        //private readonly int _count;
        //private readonly int _fetchDelay;

        ///// <summary>
        ///// Initializes a new instance of the <see cref="DemoCustomerProvider"/> class.
        ///// </summary>
        ///// <param name="items">The songs.</param>
        ///// <param name="count">The count.</param>
        ///// <param name="fetchDelay">The fetch delay.</param>
        /////
        //public SongsSourceListView(List<TrackInfo> items, int count, int fetchDelay)
        //{
        //    _items = items;
        //    _count = count;
        //    _fetchDelay = fetchDelay;
        //}

        ///// <summary>
        ///// Fetches the total number of items available.
        ///// </summary>
        ///// <returns></returns>
        //public int FetchCount()
        //{
        //    Trace.WriteLine("FetchCount");
        //    Thread.Sleep(_fetchDelay);
        //    return _count;
        //}

        ///// <summary>
        ///// Fetches a range of items.
        ///// </summary>
        ///// <param name="startIndex">The start index.</param>
        ///// <param name="count">The number of items to fetch.</param>
        ///// <returns></returns>
        //public IList<TrackInfo> FetchRange(int startIndex, int count)
        //{
        //    Trace.WriteLine("FetchRange: " + startIndex + "," + count);
        //    Thread.Sleep(_fetchDelay);

        //    List<TrackInfo> list = new List<TrackInfo>();

        //    for (int i = startIndex; i < startIndex + count; i++)
        //    {
        //        if (_items.Count == i) break;
        //        TrackInfo trackInfo = new TrackInfo { ID = (i + 1).ToString(), Name = _items[i].Name, Artist = _items[i].Artist, Duration = _items[i].Duration, FilePath = _items[0].FilePath };
        //        list.Add(trackInfo);
        //    }

        //    return list;
        //}
    }
}
