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
