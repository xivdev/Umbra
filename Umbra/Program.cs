using System;
using Lumina;
using Serilog;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Umbra
{
    class Program
    {
        static int Main( string[] args )
        {
            var log = new LoggerConfiguration()
                .WriteTo.Console()
#if DEBUG
                .MinimumLevel.Verbose()
#else
                .MinimumLevel.Information()
#endif
                .CreateLogger();

            Log.Logger = log;

            Log.Information( "Umbra is warming up..." );

            if( args.Length > 0 )
            {
                var dataPath = args[ 0 ];
                Service< GameData >.Set( dataPath, new LuminaOptions
                {
                    CacheFileResources = true,
                    ExcelSheetStrictCastingEnabled = false,
                } );

                Log.Information( "created lumina instance for path: {DataPath}", dataPath );
            }

            var boot = Service< UmbraContext >.Set();
            if( !boot.Init() )
            {
                Log.Error( "failed to init UmbraBoot!" );
                return 1;
            }

            boot.Run();

            return 0;
        }
    }
}