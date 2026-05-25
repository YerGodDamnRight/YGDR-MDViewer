#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace YGDR.MDV
{
    public class MDVTestWindow : EditorWindow
    {
        [MenuItem( "YGDR/MDV Generator" )]
        static void OpenWindow()
        {
            var window = GetWindow<MDVTestWindow>( "MDV Generator" );
            window.minSize = new Vector2( 380, 430 );
            window.Show();
        }

        private string  mPathOrGuid = "Assets/";
        private string  mAnchor    = "";
        private string  mTitle     = "";
        private int     mLineMin   = -1;
        private int     mLineMax   = -1;
        private bool    mEditable  = true;
        private Object  mAsset     = null;
        private string  mAssetGuid = "";

        void OnGUI()
        {
            GUILayout.Label( "MDViewer.Open() Generator", EditorStyles.boldLabel );
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            mAsset = EditorGUILayout.ObjectField( "Asset (.md, drag here)", mAsset, typeof( TextAsset ), false );
            if( EditorGUI.EndChangeCheck() && mAsset != null )
            {
                var path = AssetDatabase.GetAssetPath( mAsset );
                var ext  = System.IO.Path.GetExtension( path ).ToLower();

                if( ext != ".md" && ext != ".markdown" )
                {
                    Debug.LogWarning( "MDV Generator: only .md / .markdown files accepted." );
                    mAsset = null;
                }
                else
                {
                    mAssetGuid  = AssetDatabase.AssetPathToGUID( path );
                    mPathOrGuid = path;
                }
            }

            if( !string.IsNullOrEmpty( mAssetGuid ) )
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField( "GUID", mAssetGuid, EditorStyles.textField );
                if( GUILayout.Button( "Copy", GUILayout.Width( 50 ) ) )
                    EditorGUIUtility.systemCopyBuffer = mAssetGuid;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            mPathOrGuid = EditorGUILayout.TextField( "Path or GUID", mPathOrGuid );
            mAnchor     = EditorGUILayout.TextField( "Anchor (#section)", mAnchor );
            mTitle      = EditorGUILayout.TextField( "Title (optional)", mTitle );

            EditorGUILayout.Space();
            GUILayout.Label( "Line Range (-1 = disabled)", EditorStyles.miniLabel );
            mLineMin  = EditorGUILayout.IntField( "Line Min", mLineMin );
            mLineMax  = EditorGUILayout.IntField( "Line Max", mLineMax );
            mEditable = EditorGUILayout.Toggle( "Editable", mEditable );

            EditorGUILayout.Space();

            if( GUILayout.Button( "Open", GUILayout.Height( 30 ) ) )
            {
                MDViewer.Open(
                    mPathOrGuid,
                    string.IsNullOrEmpty( mAnchor ) ? null : mAnchor,
                    string.IsNullOrEmpty( mTitle )  ? null : mTitle,
                    mLineMin,
                    mLineMax,
                    mEditable
                );
            }

            EditorGUILayout.Space();
            GUILayout.Label( "Generated Call", EditorStyles.boldLabel );

            var call = BuildCall();
            EditorGUILayout.SelectableLabel( call, EditorStyles.textArea, GUILayout.Height( 110 ) );

            if( GUILayout.Button( "Copy Call" ) )
                EditorGUIUtility.systemCopyBuffer = call;
        }

        string BuildCall()
        {
            var args = new System.Collections.Generic.List<string> { "\"" + mPathOrGuid + "\"" };

            if( !string.IsNullOrEmpty( mAnchor ) ) args.Add( "anchor: \"" + mAnchor + "\"" );
            if( !string.IsNullOrEmpty( mTitle ) )  args.Add( "title: \"" + mTitle + "\"" );
            if( mLineMin != -1 )                   args.Add( "lineMin: " + mLineMin );
            if( mLineMax != -1 )                   args.Add( "lineMax: " + mLineMax );
            if( !mEditable )                       args.Add( "editable: false" );

            return "MDViewer.Open(\n    " + string.Join( ",\n    ", args ) + "\n);";
        }
    }
}
#endif
