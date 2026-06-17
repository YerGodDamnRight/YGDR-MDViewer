/*
    YGDR Markdown Viewer - View / Edit rendered markdsown in Unity
    Copyright (C) 2026  YerGodDamnRight

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


using System.Collections.Generic;

namespace YGDR.MDV
{
    public class MarkdownHistory
    {
        private int          mIndex   = -1;
        private List<string> mHistory = new List<string>();

        public bool   IsEmpty    => mHistory.Count == 0;
        public int    Count      => mHistory.Count;
        public string Current    => mIndex >= 0 ? mHistory[ mIndex ] : null;
        public bool   CanBack    => mIndex > 0;
        public bool   CanForward => mIndex != mHistory.Count - 1;

        public void Clear()
        {
            mHistory.Clear();
            mIndex = -1;
        }

        public string Forward()
        {
            if( CanForward ) mIndex++;
            return Current;
        }

        public string Back()
        {
            if( CanBack ) mIndex--;
            return Current;
        }

        public void Add( string url )
        {
            if( Current == url ) return;

            if( mIndex + 1 < mHistory.Count )
            {
                mHistory.RemoveRange( mIndex + 1, mHistory.Count - mIndex - 1 );
            }

            mHistory.Add( url );
            mIndex++;
        }

        public void OnOpen( string url )
        {
            if( Current != url )
            {
                Clear();
                Add( url );
            }
        }
    }
}
