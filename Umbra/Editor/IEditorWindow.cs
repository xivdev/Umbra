namespace Umbra.Editor
{
    public interface IEditorWindow
    {
        public void Render();
        
        public string FilePath { get; }
    }
}