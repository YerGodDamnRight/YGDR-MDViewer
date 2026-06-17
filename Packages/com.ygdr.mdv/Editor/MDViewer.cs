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


#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace YGDR.MDV
{
    public static class MDViewer
    {
        public static void Open( string pathOrGuid, string anchor = null, string title = null, int lineMin = -1, int lineMax = -1, bool editable = true )
        {
            var resolvedPath = ResolvePath( pathOrGuid );

            if( resolvedPath == null )
            {
                Debug.LogError( $"[MDV] Could not resolve asset: {pathOrGuid}" );
                return;
            }

            MDVWindow.Open( resolvedPath, anchor, title, lineMin, lineMax, editable );
        }

        static string ResolvePath( string pathOrGuid )
        {
            if( pathOrGuid.Length == 32 && IsAllHex( pathOrGuid ) )
            {
                var guidPath = AssetDatabase.GUIDToAssetPath( pathOrGuid );
                if( !string.IsNullOrEmpty( guidPath ) )
                    return guidPath;
            }

            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>( pathOrGuid );
            return asset != null ? pathOrGuid : null;
        }

        static bool IsAllHex( string value )
        {
            foreach( var character in value )
            {
                bool isDigit  = character >= '0' && character <= '9';
                bool isLower  = character >= 'a' && character <= 'f';
                bool isUpper  = character >= 'A' && character <= 'F';

                if( !isDigit && !isLower && !isUpper ) return false;
            }
            return true;
        }
    }
}
#endif
