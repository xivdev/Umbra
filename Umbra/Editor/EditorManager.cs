using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Lumina;
using Serilog;

namespace Umbra.Editor
{
    public class EditorManager
    {
        private List< IEditorWindow > _openEditors = new();
        private List< IEditorWindow > _scheduledCleanups = new();

        private Dictionary< RegisterEditorAttribute, Type > _availableEditors;

        public EditorManager()
        {
            _availableEditors = GetType()
                .Assembly
                .GetTypes()
                .Where( t => t.GetCustomAttribute< RegisterEditorAttribute >() != null )
                .OrderByDescending( t => t.GetCustomAttribute< RegisterEditorAttribute >()?.Priority )
                .ToDictionary( x => x.GetCustomAttribute< RegisterEditorAttribute >(), x => x );

            foreach( var editors in _availableEditors )
            {
                Log.Debug(
                    "registered editor {EditorFullName} for files {FileTypes} with priority {EditorPriority}",
                    editors.Value.FullName,
                    editors.Key.FileTypes,
                    editors.Key.Priority
                );
            }
        }

        public bool OpenEditor( string path )
        {
            var gameData = Service< GameData >.Get();

            if( !gameData.FileExists( path ) )
            {
                return false;
            }

            var extension = Path.GetExtension( path );

            // find an applicable editor
            // todo: do this in background
            foreach( var editor in _availableEditors.OrderByDescending( x => x.Key.Priority ) )
            {
                var ec = editor.Key;

                if( !ec.FileTypes.Any( x => string.Equals( x, extension, StringComparison.InvariantCultureIgnoreCase ) || x == "*" ) )
                {
                    continue;
                }

                var win = Activator.CreateInstance( editor.Value, path );
                if( win == null )
                {
                    Log.Error( "failed to create instance of {Type}", editor.Value.FullName );
                    continue;
                }

                Log.Debug(
                    "created editor {FullName} for file {FilePath}",
                    editor.Value.FullName,
                    path
                );

                _openEditors.Add( ( IEditorWindow )win );
                return true;
            }

            return false;
        }

        public void CleanupWindow( IEditorWindow window )
        {
            Log.Debug(
                "cleaning up {FullName} for file {FilePath}",
                window.GetType().FullName,
                window.FilePath
            );
            _scheduledCleanups.Add( window );
        }

        public void Render()
        {
            foreach( var editor in _openEditors )
            {
                editor.Render();
            }

            // cleanup any orphans
            foreach( var sc in _scheduledCleanups )
            {
                _openEditors.Remove( sc );
            }

            _scheduledCleanups.Clear();
        }
    }
}