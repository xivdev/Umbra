using ImGuiNET;
using Lumina;
using Lumina.Data;
using Umbra.Editor;

namespace Umbra.UI.FileEditors
{
    [RegisterEditor( "*", EditorPriority.Lowest )]
    public class RawFile : IEditorWindow
    {
        private readonly string _path;
        private FileResource? _file;

        private string _windowTitle;

        private bool _open = true;

        public RawFile( string path )
        {
            _path = path;
            _windowTitle = $"Raw: {path}";

            var gameData = Service< GameData >.Get();
            _file = gameData.GetFile( path );
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

            ImGui.Text( $"path: {_path}" );
            if( _file != null )
            {
                ImGui.Text( $"size: {_file.Data.Length:#,###} bytes" );
            }

            ImGui.End();
        }
    }
}