using System;
using System.Collections.Generic;
using System.Linq;

namespace YGDR.MDV
{
    public static class MarkdownUtils
    {
        static readonly char[] Separators = { '/', '\\' };

        public static string PathCombine( string a, string b, string separator = "/" )
        {
            var partsA   = ( a ?? "" ).Split( Separators, StringSplitOptions.RemoveEmptyEntries );
            var partsB   = ( b ?? "" ).Split( Separators, StringSplitOptions.RemoveEmptyEntries );
            var combined = partsA.Concat( partsB ).Where( el => el != "." );
            var path     = new List<string>();

            foreach( var el in combined )
            {
                if( el != ".." )
                    path.Add( el );
                else if( path.Count > 0 )
                    path.RemoveAt( path.Count - 1 );
            }

            return string.Join( separator, path.ToArray() );
        }

        public static string PathNormalise( string a, string separator = "/" )
            => PathCombine( "", a, separator );
    }
}
