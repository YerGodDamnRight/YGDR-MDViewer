using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YGDR.MDV
{
    public static class MarkdownPreferences
    {
        private static readonly string KeyJIRA                              = "YGDR/MDV/JIRA";
        private static readonly string KeyPipedTables                       = "YGDR/MDV/PIPED";
        private static readonly string KeyPipedTablesRequireHeaderSeparator = "YGDR/MDV/PIPED/USEHSEP";
        private static readonly string KeyHTML                              = "YGDR/MDV/HTML";
        private static readonly string KeyDarkSkin                          = "YGDR/MDV/DarkSkin";

        private static string mJIRA                               = string.Empty;
        private static bool   mPipedTables                        = true;
        private static bool   mPipedTablesRequireHeaderSeparator  = true;
        private static bool   mStripHTML                          = true;
        private static bool   mPrefsLoaded                        = false;
        private static bool   mDarkSkin                           = EditorGUIUtility.isProSkin;

        public static string JIRA                              { get { LoadPrefs(); return mJIRA; } }
        public static bool   StripHTML                         { get { LoadPrefs(); return mStripHTML; } }
        public static bool   DarkSkin                          { get { LoadPrefs(); return mDarkSkin; } }
        public static bool   PipedTables                       { get { LoadPrefs(); return mPipedTables; } }
        public static bool   PipedTablesRequireHeaderSeparator { get { LoadPrefs(); return mPipedTablesRequireHeaderSeparator; } }

        private static void LoadPrefs()
        {
            if( mPrefsLoaded ) return;

            mJIRA                              = EditorPrefs.GetString( KeyJIRA, "" );
            mStripHTML                         = EditorPrefs.GetBool( KeyHTML, true );
            mPipedTables                       = EditorPrefs.GetBool( KeyPipedTables, true );
            mPipedTablesRequireHeaderSeparator = EditorPrefs.GetBool( KeyPipedTablesRequireHeaderSeparator, true );
            mDarkSkin                          = EditorPrefs.GetBool( KeyDarkSkin, EditorGUIUtility.isProSkin );
            mPrefsLoaded = true;
        }

        public class MarkdownSettings : SettingsProvider
        {
            public MarkdownSettings( string path, SettingsScope scopes = SettingsScope.User, IEnumerable<string> keywords = null )
                : base( path, scopes, keywords ) { }

            public override void OnGUI( string searchContext ) => DrawPreferences();
        }

        [SettingsProvider]
        static SettingsProvider CreateMarkdownPreferences() => new MarkdownSettings( "Preferences/YGDR MDV" );


        private static void DrawPreferences()
        {
            LoadPrefs();

            EditorGUI.BeginChangeCheck();

            mJIRA      = EditorGUILayout.TextField( "JIRA URL", mJIRA );
            mStripHTML = EditorGUILayout.Toggle( "Strip HTML", mStripHTML );
            mDarkSkin  = EditorGUILayout.Toggle( "Dark Skin", mDarkSkin );

            EditorGUI.EndChangeCheck();

            if( GUI.changed )
            {
                EditorPrefs.SetString( KeyJIRA, mJIRA );
                EditorPrefs.SetBool( KeyHTML, mStripHTML );
                EditorPrefs.SetBool( KeyDarkSkin, mDarkSkin );
            }
        }
    }
}
