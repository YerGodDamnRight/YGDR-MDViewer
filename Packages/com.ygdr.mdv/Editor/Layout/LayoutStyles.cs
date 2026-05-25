using UnityEngine;

namespace YGDR.MDV
{
    public struct Style
    {
        public static readonly Style Default = new Style();

        const int FlagBold          = 0x0100;
        const int FlagItalic        = 0x0200;
        const int FlagFixed         = 0x0400;
        const int FlagLink          = 0x0800;
        const int FlagBlock         = 0x1000;
        const int FlagStrikethrough = 0x2000;
        const int FlagKbd           = 0x4000;

        const int MaskSize   = 0x000F;
        const int MaskWeight = 0x0300;

        int   mStyle;
        Color mTokenColor;

        public Color TokenColor
        {
            get => mTokenColor;
            set => mTokenColor = value;
        }

        public static bool operator==( Style a, Style b ) => a.mStyle == b.mStyle && a.mTokenColor == b.mTokenColor;
        public static bool operator!=( Style a, Style b ) => a.mStyle != b.mStyle || a.mTokenColor != b.mTokenColor;

        public override bool Equals( object a ) => a is Style s && s.mStyle == mStyle && s.mTokenColor == mTokenColor;
        public override int  GetHashCode()       => mStyle.GetHashCode();

        public void Clear() { mStyle = 0x0000; mTokenColor = Color.clear; }

        public bool Bold
        {
            get => ( mStyle & FlagBold ) != 0;
            set { if( value ) mStyle |= FlagBold; else mStyle &= ~FlagBold; }
        }

        public bool Italic
        {
            get => ( mStyle & FlagItalic ) != 0;
            set { if( value ) mStyle |= FlagItalic; else mStyle &= ~FlagItalic; }
        }

        public bool Fixed
        {
            get => ( mStyle & FlagFixed ) != 0;
            set { if( value ) mStyle |= FlagFixed; else mStyle &= ~FlagFixed; }
        }

        public bool Link
        {
            get => ( mStyle & FlagLink ) != 0;
            set { if( value ) mStyle |= FlagLink; else mStyle &= ~FlagLink; }
        }

        public bool Block
        {
            get => ( mStyle & FlagBlock ) != 0;
            set { if( value ) mStyle |= FlagBlock; else mStyle &= ~FlagBlock; }
        }

        public bool Strikethrough
        {
            get => ( mStyle & FlagStrikethrough ) != 0;
            set { if( value ) mStyle |= FlagStrikethrough; else mStyle &= ~FlagStrikethrough; }
        }

        public bool Kbd
        {
            get => ( mStyle & FlagKbd ) != 0;
            set { if( value ) mStyle |= FlagKbd; else mStyle &= ~FlagKbd; }
        }

        public int Size
        {
            get => mStyle & MaskSize;
            set => mStyle = ( mStyle & ~MaskSize ) | Mathf.Clamp( value, 0, 6 );
        }

        public FontStyle GetFontStyle()
        {
            switch( mStyle & MaskWeight )
            {
                case FlagBold:              return FontStyle.Bold;
                case FlagItalic:            return FontStyle.Italic;
                case FlagBold | FlagItalic: return FontStyle.BoldAndItalic;
                default:                    return FontStyle.Normal;
            }
        }
    }

    public class StyleConverter
    {
        private Style      mCurrentStyle = Style.Default;
        private GUIStyle[] mWorking;
        private GUIStyle[] mReference;

        Color linkColor       = new Color( 0.41f, 0.71f, 1.0f, 1.0f );
        const int FixedBlock  = 7;
        const int Variable    = 8;
        const int FixedInline = 12;

        static readonly string[] CustomStyles =
        {
            "variable",
            "h1", "h2", "h3", "h4", "h5", "h6",
            "fixed_block",
            "variable",
            "variable_bold",
            "variable_italic",
            "variable_bolditalic",
            "fixed_inline",
            "fixed_inline_bold",
            "fixed_inline_italic",
            "fixed_inline_bolditalic",
        };

        public StyleConverter( GUISkin skin )
        {
            mReference = new GUIStyle[ CustomStyles.Length ];
            mWorking   = new GUIStyle[ CustomStyles.Length ];

            for( var i = 0; i < CustomStyles.Length; i++ )
            {
                mReference[ i ] = skin.GetStyle( CustomStyles[ i ] );
                mWorking[ i ]   = new GUIStyle( mReference[ i ] );
            }
        }

        public GUIStyle Apply( Style src )
        {
            if( src.Block )
            {
                mWorking[ FixedBlock ].normal.textColor = src.TokenColor.a > 0
                    ? src.TokenColor
                    : mReference[ FixedBlock ].normal.textColor;
                return mWorking[ FixedBlock ];
            }

            var style = mWorking[ src.Size ];

            if( mCurrentStyle != src )
            {
                var font = ( src.Fixed || src.Kbd ? FixedInline : Variable ) + ( src.Bold ? 1 : 0 ) + ( src.Italic ? 2 : 0 );

                style.font             = mReference[ font ].font;
                style.fontStyle        = mReference[ font ].fontStyle;
                style.normal.textColor = src.Link        ? linkColor
                                       : src.TokenColor.a > 0 ? src.TokenColor
                                       : mReference[ font ].normal.textColor;

                mCurrentStyle = src;
            }

            return style;
        }
    }
}
