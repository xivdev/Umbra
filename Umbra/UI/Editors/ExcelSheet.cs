using System.IO;
using ImGuiNET;
using Lumina;
using Lumina.Excel;
using Umbra.Editor;

namespace Umbra.UI.FileEditors
{
    [RegisterEditor( ".exh" )]
    public class ExcelSheet : IEditorWindow
    {
        private readonly string _path;
        private readonly string _name;
        private ExcelSheetImpl _sheet;

        private string _windowTitle;

        private bool _open = true;

        public ExcelSheet( string path )
        {
            _name = Path.GetFileNameWithoutExtension( path );
            _path = path;
            _windowTitle = $"Sheet: {_name}";
            
            
            var gameData = Service< GameData >.Get();
            _sheet = gameData.Excel.GetSheetRaw( _name );
        }

        public string FilePath => _path;

        public void Render()
        {
            if( !_open )
            {
                Service< EditorManager >.Get().CleanupWindow( this );
                return;
            }
            
            if( !ImGui.Begin( _windowTitle, ref _open, ImGuiWindowFlags.NoSavedSettings ) )
            {
                ImGui.End();
                return;
            }

            ImGui.Text($"rows: {_sheet.Header.RowCount}");

            ImGui.End();
        }
    }
}