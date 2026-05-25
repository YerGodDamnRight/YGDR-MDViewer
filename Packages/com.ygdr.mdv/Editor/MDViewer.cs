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
