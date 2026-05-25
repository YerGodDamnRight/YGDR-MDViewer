using UnityEngine;

namespace YGDR.MDV
{
    public enum TableAlign { Left, Center, Right }

    public interface IActions
    {
        Texture FetchImage( string url );
        void    SelectPage( string url );
    }

    public interface IBuilder
    {
        void Text( string text, Style style, string link, string tooltip );
        void Image( string url, string alt, string tooltip, float overrideWidth = 0f, float overrideHeight = 0f );

        void NewLine();
        void Space();
        void HorizontalLine();

        void Indent();
        void Outdent();
        void Prefix( string text, Style style );

        void StartBlock( bool quoted, AlertType alertType = AlertType.None );
        void EndBlock();

        void StartCollapsible( string summary );
        void EndCollapsible();

        void StartTable( TableAlign[] columnAlignments );
        void EndTable();

        void StartTableRow( bool isHeader );
        void EndTableRow();
    }
}
