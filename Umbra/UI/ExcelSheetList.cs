using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Lumina;
using Serilog;
using Umbra.Editor;
using Umbra.Util;

namespace Umbra.UI
{
    public class ExcelSheetList
    {
        private GenericTree _tree;

        public ExcelSheetList()
        {
            var gameData = Service< GameData >.Get();

            _tree = new();
            foreach( var c in gameData.Excel.SheetNames )
            {
                _tree.AddNode( c );
            }

            _tree.FolderSort();

            // order it so nodes with children are first
        }

        private void RenderItem( GenericTree.TreeNode node )
        {
            if( !ImGui.Selectable( node.Fragment, false, ImGuiSelectableFlags.AllowDoubleClick ) )
            {
                return;
            }
            
            // only allow the end node to be clicked because the rest are just folders and that's fucking useless
            if( node.Children.Count != 0 || !ImGui.IsMouseDoubleClicked( ImGuiMouseButton.Left ) )
            {
                return;
            }
                
            Log.Debug( "sheet click: {SheetName}", node.FullName );
            Service< EditorManager >.Get().OpenEditor( $"exd/{node.FullName}.exh" );
        }

        private void RenderItems( GenericTree.TreeNode node )
        {
            if( node.Children?.Count == 0 )
            {
                RenderItem( node );
                return;
            }

            if( !ImGui.TreeNode( node.Fragment ) )
            {
                return;
            }

            foreach( var child in node.Children )
            {
                RenderItems( child );
            }

            ImGui.TreePop();
        }

        public void Render()
        {
            if( !ImGui.Begin( "Excel Sheets" ) )
            {
                return;
            }

            ImGui.PushStyleVar( ImGuiStyleVar.FramePadding, new Vector2( 0, 0 ) );
            if( ImGui.ListBoxHeader( "Sheets", new Vector2( -1, -1 ) ) )
            {
                foreach( var n in _tree.Nodes )
                {
                    RenderItems( n );
                }

                ImGui.ListBoxFooter();
            }

            ImGui.PopStyleVar();

            ImGui.End();
        }
    }
}