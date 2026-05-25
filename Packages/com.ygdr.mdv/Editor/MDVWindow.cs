#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace YGDR.MDV
{
    public class MDVWindow : EditorWindow
    {
        const string SkinBasePath = "Packages/com.ygdr.mdv/Editor/Skin/";

        private GUISkin        mSkinLight;
        private GUISkin        mSkinDark;
        private MarkdownViewer mViewer;
        private Vector2        mScrollPos;

        private string mPath;
        private string mAnchor;
        private int    mLineMin;
        private int    mLineMax;
        private bool   mEditable;

        private bool mPendingAnchor;
        private bool mScheduledAnchor;

        // ── Inline raw edit state ─────────────────────────────────────────────
        private const int MaxRawChunkChars = 5000;
        private string[]  mRawAllLines;
        private int       mRawChunkStart;
        private int       mRawChunkEnd;   // exclusive
        private string    mRawChunkText;
        private bool      mRawInitialized;
        private GUIStyle  mRawStyle;

        public static void Open( string path, string anchor = null, string title = null, int lineMin = -1, int lineMax = -1, bool editable = true )
        {
            var window      = GetWindow<MDVWindow>();
            window.minSize  = new Vector2( 450, 430 );
            window.mPath     = path;
            window.mAnchor   = anchor;
            window.mLineMin  = lineMin;
            window.mLineMax  = lineMax;
            window.mEditable = editable;

            var windowTitle     = string.IsNullOrEmpty( title ) ? Path.GetFileName( path ) : title;
            window.titleContent = new GUIContent( windowTitle );

            window.BuildViewer();
            window.Show();
        }

        void BuildViewer()
        {
            if( mViewer != null )
                EditorApplication.update -= UpdateRequests;

            mRawInitialized = false;
            mRawAllLines    = null;

            mSkinLight = AssetDatabase.LoadAssetAtPath<GUISkin>( SkinBasePath + "MarkdownSkinLight.guiskin" );
            mSkinDark  = AssetDatabase.LoadAssetAtPath<GUISkin>( SkinBasePath + "MarkdownSkinDark.guiskin" );

            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>( mPath );
            if( asset == null )
            {
                Debug.LogError( $"[MDV] Could not load asset at path: {mPath}" );
                return;
            }

            var rawContent = asset.text;

            if( mLineMin >= 0 || mLineMax >= 0 )
            {
                var lines     = rawContent.Split( '\n' );
                var startLine = Mathf.Clamp( mLineMin >= 0 ? mLineMin : 0, 0, lines.Length - 1 );
                var endLine   = Mathf.Clamp( mLineMax >= 0 ? mLineMax : lines.Length - 1, startLine, lines.Length - 1 );
                rawContent    = string.Join( "\n", lines, startLine, endLine - startLine + 1 );
            }

            var skin = MarkdownPreferences.DarkSkin ? mSkinDark : mSkinLight;
            mViewer  = new MarkdownViewer( skin, mPath, rawContent ) { Editable = mEditable };

            mPendingAnchor   = !string.IsNullOrEmpty( mAnchor );
            mScheduledAnchor = false;

            EditorApplication.update += UpdateRequests;
        }

        void UpdateRequests()
        {
            if( mViewer != null && ( mViewer.Update() || mViewer.AnchorScrolling ) )
                Repaint();
        }

        void OnDisable()
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
            }
            mRawInitialized = false;
            mRawAllLines    = null;
            mViewer         = null;
        }

        void OnGUI()
        {
            if( mViewer == null ) return;

            if( mPendingAnchor && !mScheduledAnchor && Event.current.type == EventType.Layout )
            {
                mScheduledAnchor = true;
                var anchorToApply = mAnchor;
                EditorApplication.delayCall += () =>
                {
                    mPendingAnchor = false;
                    mViewer?.ScrollToAnchor( anchorToApply );
                    Repaint();
                };
            }

            if( mViewer.AnchorScrolling )
            {
                if( Event.current.type == EventType.Repaint )
                    mViewer.TickAnchorScroll();
                else if( Event.current.isMouse || Event.current.type == EventType.ScrollWheel )
                {
                    Event.current.Use();
                    return;
                }
            }

            if( mViewer.PendingScrollY.HasValue )
            {
                mScrollPos.y           = Mathf.Max( 0f, mViewer.PendingScrollY.Value );
                mViewer.PendingScrollY = null;
                Repaint();
            }

            mViewer.ViewportHeight = position.height;
            mScrollPos = EditorGUILayout.BeginScrollView( mScrollPos );

            if( mViewer.IsRaw )
                DrawRawEditor();
            else
                mViewer.Draw();

            EditorGUILayout.EndScrollView();
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
    }
}
#endif
