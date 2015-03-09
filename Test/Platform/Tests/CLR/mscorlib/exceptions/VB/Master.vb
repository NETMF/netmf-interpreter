Option Explicit On
Option Strict On

Imports System.Diagnostics
Imports Microsoft.SPOT.Platform.Test

Namespace Microsoft.SPOT.Platform.Tests

    Public Class Master
        Shared Sub Main()

            Dim args() As String = { "ExceptionTest", "AppDomainExceptionTest" }

            Dim runner As MFTestRunner = New MFTestRunner(args)

        End Sub

    End Class

End Namespace