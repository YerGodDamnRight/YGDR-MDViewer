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
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace YGDR.MDV
{
    [CustomEditor( typeof( TextAsset ), true )]
    public class MarkdownEditor : Editor
    {
        private GUISkin SkinLight;
        private GUISkin SkinDark;

        static readonly Type      sPropertyEditorType = typeof( Editor ).Assembly.GetType( "UnityEditor.PropertyEditor" );
        static readonly FieldInfo sScrollViewField    = sPropertyEditorType?.GetField( "m_ScrollView", BindingFlags.NonPublic | BindingFlags.Instance );

        MarkdownViewer mViewer;
        EditorWindow   mInspectorWindow;

        private static readonly List<string> mExtensions = new List<string> { ".md", ".markdown" };

        private const string SkinBasePath = "Packages/com.ygdr.mdv/Editor/Skin/";

        // ── Inline raw edit state ─────────────────────────────────────────────
        private const int MaxRawChunkChars = 5000;
        private string[]  mRawAllLines;
        private int       mRawChunkStart;
        private int       mRawChunkEnd;   // exclusive
        private string    mRawChunkText;
        private bool      mRawInitialized;
        private GUIStyle  mRawStyle;

        protected void OnEnable()
        {
            SkinLight = AssetDatabase.LoadAssetAtPath<GUISkin>( SkinBasePath + "MarkdownSkinLight.guiskin" );
            SkinDark  = AssetDatabase.LoadAssetAtPath<GUISkin>( SkinBasePath + "MarkdownSkinDark.guiskin" );

            var path    = AssetDatabase.GetAssetPath( target );
            var content = ( target as TextAsset ).text;
            var isMarkdownAsset = target is MarkdownAsset;
            var isMarkdownExt   = mExtensions.Contains( Path.GetExtension( path ).ToLower() );

            if( isMarkdownAsset || isMarkdownExt )
            {
                mViewer = new MarkdownViewer( MarkdownPreferences.DarkSkin ? SkinDark : SkinLight, path, content );
                EditorApplication.update += UpdateRequests;
            }
        }

        protected void OnDisable()
        {
            if( mViewer != null )
            {
                if( mRawInitialized && mViewer.IsRaw )
                {
                    FlushRawChunk();
                    mViewer.CommitRawEdit( string.Join( "\n", mRawAllLines ) );
                    mViewer.ExitRawMode();
                }
                EditorApplication.update -= UpdateRequests;
                mViewer = null;
            }
            mRawInitialized = false;
            mRawAllLines    = null;
        }

        void UpdateRequests()
        {
            if( mViewer != null && mViewer.Update() )
                Repaint();
        }

        public override bool UseDefaultMargins() => false;

        public override void OnInspectorGUI()
        {
            if( mViewer != null )
            {
                CacheInspectorWindow();
                if( mViewer.IsRaw )
                    DrawRawEditor();
                else
                    mViewer.Draw();
                if( mViewer != null && !mViewer.IsRaw )
                    ApplyPendingScroll();
            }
            else
            {
                DrawDefaultEditor();
            }
        }

        // ── Inline raw editor ─────────────────────────────────────────────────

        void DrawRawEditor()
        {
            if( !mRawInitialized )
            {
                mRawAllLines    = mViewer.CurrentRawText.Replace( "\r\n", "\n" ).Replace( "\r", "\n" ).Split( '\n' );
                mRawChunkStart  = 0;
                mRawChunkText   = GetRawChunkText( 0 );
                mRawInitialized = true;
                GUIUtility.keyboardControl = 0;
            }

            // Ctrl+S — intercept before TextArea consumes the event
            var evt = Event.current;
            if( evt.type == EventType.KeyDown && evt.keyCode == KeyCode.S && ( evt.control || evt.command ) )
            {
                FlushRawChunk();
                mViewer.CommitRawEdit( string.Join( "\n", mRawAllLines ) );
                mViewer.TrySave();
                evt.Use();
            }

            // ── Nav toolbar ───────────────────────────────────────────────────
            EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );

            GUI.enabled = mRawChunkStart > 0;
            if( GUILayout.Button( "◀ Prev", EditorStyles.toolbarButton, GUILayout.Width( 60 ) ) )
                NavigateRawTo( mRawChunkStart - ( mRawChunkEnd - mRawChunkStart ) );
            GUI.enabled = true;

            GUILayout.Label( $"Lines {mRawChunkStart + 1}–{mRawChunkEnd} / {mRawAllLines.Length}",
                EditorStyles.toolbarButton, GUILayout.ExpandWidth( true ) );

            GUI.enabled = mRawChunkEnd < mRawAllLines.Length;
            if( GUILayout.Button( "Next ▶", EditorStyles.toolbarButton, GUILayout.Width( 60 ) ) )
                NavigateRawTo( mRawChunkEnd );
            GUI.enabled = true;

            if( GUILayout.Button( "✕ Close", EditorStyles.toolbarButton, GUILayout.Width( 60 ) ) )
            {
                FlushRawChunk();
                mViewer.CommitRawEdit( string.Join( "\n", mRawAllLines ) );
                mViewer.ExitRawMode();
                mRawInitialized = false;
                mRawAllLines    = null;
                Repaint();
            }

            EditorGUILayout.EndHorizontal();

            // ── Text area ─────────────────────────────────────────────────────
            mRawChunkText = EditorGUILayout.TextArea( mRawChunkText, GetRawStyle() );
        }

        string GetRawChunkText( int startLine )
        {
            int totalChars = 0;
            int endLine    = startLine;
            while( endLine < mRawAllLines.Length )
            {
                int addChars = ( endLine > startLine ? 1 : 0 ) + mRawAllLines[ endLine ].Length;
                if( endLine > startLine && totalChars + addChars > MaxRawChunkChars ) break;
                totalChars += addChars;
                endLine++;
            }
            endLine        = Math.Max( endLine, startLine + 1 );
            endLine        = Math.Min( endLine, mRawAllLines.Length );
            mRawChunkEnd   = endLine;
            return string.Join( "\n", mRawAllLines, startLine, endLine - startLine );
        }

        void FlushRawChunk()
        {
            if( mRawAllLines == null || mRawChunkText == null ) return;
            var editedLines = mRawChunkText.Split( '\n' );
            int suffixLen   = mRawAllLines.Length - mRawChunkEnd;
            var newLines    = new string[ mRawChunkStart + editedLines.Length + suffixLen ];
            Array.Copy( mRawAllLines, 0,            newLines, 0,                                   mRawChunkStart      );
            Array.Copy( editedLines,  0,            newLines, mRawChunkStart,                      editedLines.Length  );
            Array.Copy( mRawAllLines, mRawChunkEnd, newLines, mRawChunkStart + editedLines.Length, suffixLen           );
            mRawAllLines = newLines;
            mRawChunkEnd = mRawChunkStart + editedLines.Length;
        }

        void NavigateRawTo( int newStart )
        {
            FlushRawChunk();
            mViewer.CommitRawEdit( string.Join( "\n", mRawAllLines ) );
            mRawChunkStart = Math.Clamp( newStart, 0, Math.Max( 0, mRawAllLines.Length - 1 ) );
            mRawChunkText  = GetRawChunkText( mRawChunkStart );
            // Clear keyboard focus so IMGUI re-initializes TextArea from mRawChunkText next draw
            GUIUtility.keyboardControl = 0;
            Repaint();
        }

        GUIStyle GetRawStyle()
        {
            if( mRawStyle != null ) return mRawStyle;
            mRawStyle          = new GUIStyle( EditorStyles.textArea );
            mRawStyle.wordWrap = true;
            mRawStyle.fontSize = 14;
            var monoFont = Font.CreateDynamicFontFromOSFont( new[] { "Consolas", "Courier New", "Lucida Console" }, 14 );
            if( monoFont != null ) mRawStyle.font = monoFont;
            return mRawStyle;
        }

        // ── Inspector scroll / window helpers ─────────────────────────────────

        void CacheInspectorWindow()
        {
            if( mInspectorWindow != null ) return;
            if( sPropertyEditorType == null ) return;

            var candidate = EditorWindow.focusedWindow ?? EditorWindow.mouseOverWindow;
            if( candidate != null && sPropertyEditorType.IsInstanceOfType( candidate ) )
            {
                mInspectorWindow = candidate;
                return;
            }

            var all = Resources.FindObjectsOfTypeAll( sPropertyEditorType );
            if( all != null && all.Length > 0 )
                mInspectorWindow = all[ 0 ] as EditorWindow;
        }

        void ApplyPendingScroll()
        {
            if( mViewer == null ) return;
            if( !mViewer.PendingScrollY.HasValue ) return;
            if( sScrollViewField == null ) return;
            if( mInspectorWindow == null ) return;

            var blockY = mViewer.PendingScrollY.Value;
            mViewer.PendingScrollY = null;

            EditorApplication.delayCall += () =>
            {
                var scrollView = sScrollViewField.GetValue( mInspectorWindow ) as ScrollView;
                if( scrollView == null ) return;
                var viewportH  = scrollView.contentViewport.layout.height;
                var target = blockY + 200f - viewportH * 0.5f;
                scrollView.verticalScroller.value = Mathf.Clamp( target, 0f, scrollView.verticalScroller.highValue );
            };
        }

        private Editor mDefaultEditor;

        void DrawDefaultEditor()
        {
            if( mDefaultEditor == null )
                mDefaultEditor = CreateEditor( target, Type.GetType( "UnityEditor.TextAssetInspector, UnityEditor" ) );

            mDefaultEditor?.OnInspectorGUI();
        }
    }
}
