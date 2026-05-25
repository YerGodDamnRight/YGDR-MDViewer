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
