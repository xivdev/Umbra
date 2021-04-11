using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Serilog;
using Umbra.Editor;
using Umbra.Rendering;
using Umbra.UI;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using ImGuiRenderer = Umbra.Rendering.ImGui.ImGuiRenderer;

namespace Umbra
{
    public class UmbraContext
    {
        private Sdl2Window _window = null!;
        private GraphicsDevice _gd = null!;

        private ImGuiRenderer _imGuiRenderer = null!;
        private ImFontPtr _font = null!;

        private static CommandList _cl = null!;

        private ExcelSheetList _excelSheetList = null!;

        private bool _resized;

        private bool _exiting;

        private bool _drawImguiDemo;

        public UmbraContext()
        {
        }

        public bool Init()
        {
            SetupWindow();

            Service< EditorManager >.Set();

            _excelSheetList = new ExcelSheetList();

            return true;
        }

        private void SetupWindow()
        {
            var windowCreateInfo = new WindowCreateInfo
            {
                X = 100,
                Y = 100,
                WindowWidth = 1280,
                WindowHeight = 720,
#if DEBUG
                WindowTitle = "Umbra (Debug)",
#else
                WindowTitle = "Umbra",
#endif
            };
            _window = VeldridStartup.CreateWindow( ref windowCreateInfo );
            _window.Resized += WindowOnResized;

            var options = new GraphicsDeviceOptions
            {
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true,
            };
            _gd = VeldridStartup.CreateGraphicsDevice( _window, options, GraphicsBackend.Vulkan );
            _gd.SyncToVerticalBlank = true;

            CreateResources();

            _imGuiRenderer = new ImGuiRenderer(
                _gd,
                _gd.MainSwapchain.Framebuffer.OutputDescription,
                _window.Width,
                _window.Height
            );

            var io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            var style = ImGui.GetStyle();

            // _font = io.Fonts.AddFontFromFileTTF( "Assets/Fonts/OpenSans-SemiBold.ttf", 15.0f );
            // io.Fonts.Build();

            style.FrameRounding = 4.0f;
            style.GrabRounding = 4.0f;

            var colours = style.Colors;
            colours[ ( int )ImGuiCol.Text ] = new Vector4( 0.95f, 0.96f, 0.98f, 1.00f );
            colours[ ( int )ImGuiCol.TextDisabled ] = new Vector4( 0.36f, 0.42f, 0.47f, 1.00f );
            colours[ ( int )ImGuiCol.WindowBg ] = new Vector4( 0.11f, 0.15f, 0.17f, 1.00f );
            colours[ ( int )ImGuiCol.ChildBg ] = new Vector4( 0.15f, 0.18f, 0.22f, 1.00f );
            colours[ ( int )ImGuiCol.PopupBg ] = new Vector4( 0.08f, 0.08f, 0.08f, 0.94f );
            colours[ ( int )ImGuiCol.Border ] = new Vector4( 0.08f, 0.10f, 0.12f, 1.00f );
            colours[ ( int )ImGuiCol.BorderShadow ] = new Vector4( 0.00f, 0.00f, 0.00f, 0.00f );
            colours[ ( int )ImGuiCol.FrameBg ] = new Vector4( 0.20f, 0.25f, 0.29f, 1.00f );
            colours[ ( int )ImGuiCol.FrameBgHovered ] = new Vector4( 0.12f, 0.20f, 0.28f, 1.00f );
            colours[ ( int )ImGuiCol.FrameBgActive ] = new Vector4( 0.09f, 0.12f, 0.14f, 1.00f );
            colours[ ( int )ImGuiCol.TitleBg ] = new Vector4( 0.09f, 0.12f, 0.14f, 0.65f );
            colours[ ( int )ImGuiCol.TitleBgActive ] = new Vector4( 0.08f, 0.10f, 0.12f, 1.00f );
            colours[ ( int )ImGuiCol.TitleBgCollapsed ] = new Vector4( 0.00f, 0.00f, 0.00f, 0.51f );
            colours[ ( int )ImGuiCol.MenuBarBg ] = new Vector4( 0.15f, 0.18f, 0.22f, 1.00f );
            colours[ ( int )ImGuiCol.ScrollbarBg ] = new Vector4( 0.02f, 0.02f, 0.02f, 0.39f );
            colours[ ( int )ImGuiCol.ScrollbarGrab ] = new Vector4( 0.20f, 0.25f, 0.29f, 1.00f );
            colours[ ( int )ImGuiCol.ScrollbarGrabHovered ] = new Vector4( 0.18f, 0.22f, 0.25f, 1.00f );
            colours[ ( int )ImGuiCol.ScrollbarGrabActive ] = new Vector4( 0.09f, 0.21f, 0.31f, 1.00f );
            colours[ ( int )ImGuiCol.CheckMark ] = new Vector4( 0.28f, 0.56f, 1.00f, 1.00f );
            colours[ ( int )ImGuiCol.SliderGrab ] = new Vector4( 0.28f, 0.56f, 1.00f, 1.00f );
            colours[ ( int )ImGuiCol.SliderGrabActive ] = new Vector4( 0.37f, 0.61f, 1.00f, 1.00f );
            colours[ ( int )ImGuiCol.Button ] = new Vector4( 0.20f, 0.25f, 0.29f, 1.00f );
            colours[ ( int )ImGuiCol.ButtonHovered ] = new Vector4( 0.28f, 0.56f, 1.00f, 1.00f );
            colours[ ( int )ImGuiCol.ButtonActive ] = new Vector4( 0.06f, 0.53f, 0.98f, 1.00f );
            colours[ ( int )ImGuiCol.Header ] = new Vector4( 0.20f, 0.25f, 0.29f, 0.55f );
            colours[ ( int )ImGuiCol.HeaderHovered ] = new Vector4( 0.26f, 0.59f, 0.98f, 0.80f );
            colours[ ( int )ImGuiCol.HeaderActive ] = new Vector4( 0.26f, 0.59f, 0.98f, 1.00f );
            colours[ ( int )ImGuiCol.Separator ] = new Vector4( 0.20f, 0.25f, 0.29f, 1.00f );
            colours[ ( int )ImGuiCol.SeparatorHovered ] = new Vector4( 0.10f, 0.40f, 0.75f, 0.78f );
            colours[ ( int )ImGuiCol.SeparatorActive ] = new Vector4( 0.10f, 0.40f, 0.75f, 1.00f );
            colours[ ( int )ImGuiCol.ResizeGrip ] = new Vector4( 0.26f, 0.59f, 0.98f, 0.25f );
            colours[ ( int )ImGuiCol.ResizeGripHovered ] = new Vector4( 0.26f, 0.59f, 0.98f, 0.67f );
            colours[ ( int )ImGuiCol.ResizeGripActive ] = new Vector4( 0.26f, 0.59f, 0.98f, 0.95f );
            colours[ ( int )ImGuiCol.Tab ] = new Vector4( 0.11f, 0.15f, 0.17f, 1.00f );
            colours[ ( int )ImGuiCol.TabHovered ] = new Vector4( 0.26f, 0.59f, 0.98f, 0.80f );
            colours[ ( int )ImGuiCol.TabActive ] = new Vector4( 0.20f, 0.25f, 0.29f, 1.00f );
            colours[ ( int )ImGuiCol.TabUnfocused ] = new Vector4( 0.11f, 0.15f, 0.17f, 1.00f );
            colours[ ( int )ImGuiCol.TabUnfocusedActive ] = new Vector4( 0.11f, 0.15f, 0.17f, 1.00f );
            colours[ ( int )ImGuiCol.PlotLines ] = new Vector4( 0.61f, 0.61f, 0.61f, 1.00f );
            colours[ ( int )ImGuiCol.PlotLinesHovered ] = new Vector4( 1.00f, 0.43f, 0.35f, 1.00f );
            colours[ ( int )ImGuiCol.PlotHistogram ] = new Vector4( 0.90f, 0.70f, 0.00f, 1.00f );
            colours[ ( int )ImGuiCol.PlotHistogramHovered ] = new Vector4( 1.00f, 0.60f, 0.00f, 1.00f );
            colours[ ( int )ImGuiCol.TextSelectedBg ] = new Vector4( 0.26f, 0.59f, 0.98f, 0.35f );
            colours[ ( int )ImGuiCol.DragDropTarget ] = new Vector4( 1.00f, 1.00f, 0.00f, 0.90f );
            colours[ ( int )ImGuiCol.NavHighlight ] = new Vector4( 0.26f, 0.59f, 0.98f, 1.00f );
            colours[ ( int )ImGuiCol.NavWindowingHighlight ] = new Vector4( 1.00f, 1.00f, 1.00f, 0.70f );
            colours[ ( int )ImGuiCol.NavWindowingDimBg ] = new Vector4( 0.80f, 0.80f, 0.80f, 0.20f );
            colours[ ( int )ImGuiCol.ModalWindowDimBg ] = new Vector4( 0.80f, 0.80f, 0.80f, 0.35f );
        }

        private void WindowOnResized()
        {
            _resized = true;
        }

        private void CreateResources()
        {
            _cl = _gd.ResourceFactory.CreateCommandList();
        }

        public void Run()
        {
            long previousFrameTicks = 0;
            var sw = new Stopwatch();
            sw.Start();

            while( _window.Exists && !_exiting )
            {
                var currentFrameTicks = sw.ElapsedTicks;
                var deltaTime = ( currentFrameTicks - previousFrameTicks ) / ( float )Stopwatch.Frequency;

                var snapshot = _window.PumpEvents();
                if( !_window.Exists )
                {
                    break;
                }

                HandleWindowResize();

                Draw( deltaTime, snapshot );

                previousFrameTicks = currentFrameTicks;
            }

            _gd.WaitForIdle();
            _imGuiRenderer.Dispose();
            _cl.Dispose();
            _gd.Dispose();
        }

        private void HandleWindowResize()
        {
            if( !_resized )
            {
                return;
            }

            _gd.ResizeMainWindow( ( uint )_window.Width, ( uint )_window.Height );
            _imGuiRenderer.WindowResized( _window.Width, _window.Height );
            _resized = false;
        }

        private void SetupDockspace()
        {
            var viewport = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos( viewport.Pos );
            ImGui.SetNextWindowSize( viewport.Size );
            ImGui.SetNextWindowViewport( viewport.ID );
            ImGui.PushStyleVar( ImGuiStyleVar.WindowRounding, 0.0f );
            ImGui.PushStyleVar( ImGuiStyleVar.WindowBorderSize, 0.0f );
            ImGui.PushStyleVar( ImGuiStyleVar.WindowPadding, new Vector2( 0.0f, 0.0f ) );

            var flags =
                ImGuiWindowFlags.MenuBar |
                ImGuiWindowFlags.NoDocking |
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoNavFocus |
                ImGuiWindowFlags.NoBackground;

            bool open = true;
            ImGui.Begin( "UmbraDockSpace", ref open, flags );
            ImGui.PopStyleVar( 3 );

            var io = ImGui.GetIO();
            if( io.ConfigFlags.HasFlag( ImGuiConfigFlags.DockingEnable ) )
            {
                var dockspaceId = ImGui.GetID( "UmbraDockSpace" );
                ImGui.DockSpace( dockspaceId, new Vector2( 0.0f, 0.0f ), ImGuiDockNodeFlags.PassthruCentralNode );
            }

            ImGui.End();
        }

        private void Draw( float deltaTime, InputSnapshot snapshot )
        {
            _cl.Begin();
            _cl.SetFramebuffer( _gd.MainSwapchain.Framebuffer );
            _cl.ClearColorTarget( 0, new RgbaFloat( 0.04f, 0.04f, 0.04f, 1.0f ) );

            _imGuiRenderer.NewFrame( deltaTime, snapshot );

            SetupDockspace();

            ImGui.BeginMainMenuBar();

            if( ImGui.BeginMenu( "Umbra" ) )
            {
                if( ImGui.MenuItem( "Exit" ) )
                {
                    Log.Debug( "exiting umbra..." );
                    _exiting = true;
                }

                ImGui.EndMenu();
            }

#if DEBUG
            if( ImGui.BeginMenu( "Debug" ) )
            {
                ImGui.MenuItem( "Draw ImGui Demo", null, ref _drawImguiDemo );
                
                var vsync = _gd.SyncToVerticalBlank;
                if( ImGui.MenuItem( "VSync", null, ref vsync ) )
                {
                    _gd.SyncToVerticalBlank = vsync;
                }
                
                ImGui.EndMenu();
            }
#endif

            ImGui.PushStyleColor( 0, new Vector4( 0.7f, 0.7f, 0.7f, 1.0f ) );
            var fps = ImGui.GetIO().Framerate;
            ImGui.Text( $"{fps:F1} FPS ({1000.0f / fps:F} ms)" );
            ImGui.PopStyleColor();

            ImGui.EndMainMenuBar();

            _excelSheetList.Render();

            Service< EditorManager >.Get().Render();
            
            #if DEBUG
            if( _drawImguiDemo )
            {
                ImGui.ShowDemoWindow(ref _drawImguiDemo);
            }
            #endif

            _imGuiRenderer.Render( _gd, _cl );

            _cl.End();
            _gd.SubmitCommands( _cl );
            _gd.SwapBuffers( _gd.MainSwapchain );
        }
    }
}