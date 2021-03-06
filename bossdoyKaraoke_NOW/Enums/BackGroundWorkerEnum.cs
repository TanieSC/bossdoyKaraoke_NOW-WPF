﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bossdoyKaraoke_NOW.Enums
{
    public class BackGroundWorkerEnum
    {
        private static NewTask _currentTask;

        /// <summary>
        /// The enum task used for running method in the background
        /// </summary>
        public enum NewTask
        {
            ADD_NEW_FAVORITES,
            ADD_NEW_SONGS,
            ADD_FAVORITES_TO_QUEUE,
            ADD_TO_QUEUE,
            ADD_TO_QUEUE_AS_NEXT,
            ADD_PLAYEDSONGS_TO_FAVORITES,
            EMPTY_QUEUE_LIST,
            LOAD_FAVORITES,
            LOAD_SONGS,
            LOAD_QUEUE_SONGS,
            REMOVE_FROM_QUEUE,
            REMOVE_SELECTED_FAVORITE,
            REMOVE_FAVORITES,
            REMOVE_SELECTED_SONG,
            REMOVE_SONGS,
            SEARCH_DIRECTORY,
            SEARCH_TEXTFILE,
            SEARCH_LISTVIEW,
            SHUFFLE_SONGS,
            SORT_SONGS,
            WRITE_TO_QUEUE_LIST,
            EQ_ENABLED,
            LOAD_EQ_PRESET,
            SAVE_EQ_SETTINGS,
            UPDATE_EQ_SETTINGS,
            UPDATE_EQ_PRESET,
            UPDATE_EQ_PREAMP
        }

        /// <summary>
        /// Get or Set current task for backgroung process
        /// </summary>
        public static NewTask CurrentTask
        {
            get
            {
                return _currentTask;
            }
            set
            {
                _currentTask = value;
            }

        }
      
    }
        
}
