using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DebuggerPortConsole
{
    class Program
    {
        static byte[] buffer = new byte[4096];
        static void Main( string[ ] args )
        {
            var watcher = new FileSystemWatcher( LocalPipePrefix );
            watcher.Created += ( s, e ) => Console.WriteLine( "WATCHER: {0}", e.FullPath );
            watcher.EnableRaisingEvents = true;

            var cts = new CancellationTokenSource( );
            Console.CancelKeyPress += ( s, e ) => { cts.Cancel( ); };
            while( !cts.IsCancellationRequested )
            {
                Console.WriteLine( "press Ctr-C or Ctrl-Break to exit" );
                //RunAsPipeServer( @"NETMF\DBG", cts );
                RunAsPipeClient( "TinyClr_*_Port1", cts.Token );
            }
        }

        private static void RunAsPipeClient( string namePattern, CancellationToken ct )
        {
            Console.WriteLine( "Waiting for devices..." );
            var servers = DiscoverServers( namePattern, ct );
            if( servers.Count > 0 )
                Console.WriteLine( "Devices found" );

            for( var i = 0; i < servers.Count; i++ )
                Console.WriteLine( "{0}) {1}", i, servers[ i ] );

            Console.Write( "Select the device to connect to:" );
            
            var serverIndex = int.Parse( Console.ReadLine() );
            using( var pipeClient = new NamedPipeClientStream(".", servers[serverIndex], PipeDirection.InOut, PipeOptions.Asynchronous ) )
            {
                pipeClient.Connect( 10000 );
                if( !ct.IsCancellationRequested )
                {
                    Console.WriteLine( "Target connected" );
                    ProcessInboundData( pipeClient, ct );
                    Console.WriteLine( "Target diconnected" );
                }
            }
        }
        const string LocalPipePrefix = @"\\.\Pipe\";

        private static IReadOnlyList<string> DiscoverServers( string pattern, CancellationToken ct )
        {
            // unfortunately FileSystemWatcher doesn't work on Named pipes...
            var watcherTask = Task.Run( () =>
            {
                do
                {
                    var servers = ( from path in Directory.EnumerateFileSystemEntries( LocalPipePrefix, pattern )
                                    select path.Substring( LocalPipePrefix.Length )
                                  ).ToList( );

                    if( servers.Count > 0 )
                        return servers;

                    Task.Delay( 1000, ct );
                    ct.ThrowIfCancellationRequested( );
                }while(true);
            });
            
            watcherTask.Wait( ct );
            if( watcherTask.IsCanceled )
                return new List<string>( );
            
            return watcherTask.Result;
        }

        private static void RunAsPipeServer( string name, CancellationToken ct )
        {
            Console.WriteLine( "Waiting to connect with target..." );
            using( var pipeServer = new NamedPipeServerStream( name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous ) )
            {
                WaitForClientConnection( pipeServer, ct );
                if( !ct.IsCancellationRequested )
                {
                    Console.WriteLine( "Target connected" );
                    ct = ProcessInboundData( pipeServer, ct );
                    Console.WriteLine( "Target diconnected" );
                }
            }
        }

        private static CancellationToken ProcessInboundData( PipeStream pipeStream, CancellationToken ct )
        {
            using( var reader = new StreamReader( pipeStream ) )
            {
                while( !ct.IsCancellationRequested && pipeStream.IsConnected )
                {
                    var readTask = reader.ReadLineAsync( );
                    readTask.Wait( ct );
                    if( ct.IsCancellationRequested )
                        break;

                    Console.WriteLine( readTask.Result );
                }
            }
            return ct;
        }

        private static void WaitForClientConnection( NamedPipeServerStream pipeServer, CancellationToken ct )
        {
            var factory = new TaskFactory( ct );

            // server stream implementation doesn't support cancel
            // on wait for connection so build a task and use that
            // to do the cancellation.
            var task = factory.FromAsync( pipeServer.BeginWaitForConnection
                                        , pipeServer.EndWaitForConnection
                                        , TaskCreationOptions.LongRunning
                                        );
            task.Wait( ct );
        }
    }
}
