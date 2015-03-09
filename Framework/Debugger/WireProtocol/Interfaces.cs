////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;


namespace Microsoft.SPOT.Debugger.WireProtocol
{
    public interface IControllerHost
    {
        void SpuriousCharacters( byte[ ] buf, int offset, int count );

        void ProcessExited( );
    }

    public interface IController
    {
        DateTime LastActivity { get; }

        bool IsPortConnected { get; }

        Packet NewPacket( );

        bool QueueOutput( MessageRaw raw );

        void SendRawBuffer( byte[ ] buf );

        void ClosePort( );

        void Start( );
        void StopProcessing( );
        void ResumeProcessing( );
        void Stop( );

        uint GetUniqueEndpointId( );

        CLRCapabilities Capabilities { get; set; }
    }

    public interface IControllerHostLocal : IControllerHost
    {
        Stream OpenConnection( );

        bool ProcessMessage( IncomingMessage msg, bool fReply );
    }

    public interface IControllerLocal : IController
    {
        Stream OpenPort( );
    }

    public interface IStreamAvailableCharacters
    {
        int AvailableCharacters { get; }
    }

    public interface IControllerHostRemote : IControllerHost
    {
        bool ProcessMessage( byte[ ] header, byte[ ] payload, bool fReply );
    }

    public interface IControllerRemote : IController
    {
        bool RegisterEndpoint( uint epType, uint epId );
        void DeregisterEndpoint( uint epType, uint epId );
    }
}