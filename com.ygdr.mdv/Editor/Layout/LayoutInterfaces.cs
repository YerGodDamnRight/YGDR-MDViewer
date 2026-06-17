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
