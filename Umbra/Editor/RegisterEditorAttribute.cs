using System;

namespace Umbra.Editor
{
    [AttributeUsage( AttributeTargets.Class )]
    public class RegisterEditorAttribute : Attribute
    {
        public EditorPriority Priority { get; }
        public string[] FileTypes { get; }

        public RegisterEditorAttribute( string[] fileTypes, EditorPriority priority = EditorPriority.Normal )
        {
            Priority = priority;
            FileTypes = fileTypes;
        }

        public RegisterEditorAttribute( string fileType, EditorPriority priority = EditorPriority.Normal )
        {
            Priority = priority;
            FileTypes = new [] { fileType };
        }
    }
}