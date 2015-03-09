Option Explicit On
Option Strict On

Imports System.Diagnostics
Imports Microsoft.SPOT
Imports Microsoft.SPOT.Platform.Test

'IMPORTANT: Run tests FancyNesting, BreakpointTests, and AppDomainFilterTransitions in a debugger to verify stepping and breakpoints work correctly.

Namespace Microsoft.SPOT.Platform.Tests

    Public Class AppDomainExceptionTest
        Inherits MarshalByRefObject
        Implements IMFTestInterface

        Private appDomainTracker As Integer
        Private appDomainTestSucceeded As Boolean

        <SetUp()> _
        Public Function Initialize() As InitializeResult Implements IMFTestInterface.Initialize

            Log.Comment("AppDomainExceptionTest starting...")
            Return InitializeResult.ReadyToGo
        End Function

        <TearDown()> _
        Public Sub CleanUp() Implements IMFTestInterface.CleanUp
            Log.Comment("AppDomainExceptionTest finished.")
        End Sub

        <TestMethod()> _
        Public Function AppDomainFilterTransitions() As MFTestResults
            'When the option in Tools|Options|Debugging is set, the DevEnv should break twice when the exception tries to cross 2 AD-boundaries
            'to reach it's handler, but it hasn't ever worked yet.
            Const msg As String = "Testing AppDomain Exception Filtering"

            appDomainTestSucceeded = True
            appDomainTracker = 0

            'Initialize AppDomain
            Dim szAssm As String = GetType(AppDomainExceptionTest).Assembly.FullName
            Dim m_appDomain1 As AppDomain = AppDomain.CreateDomain("MidDomain")
            m_appDomain1.Load(szAssm)

            Dim proxyFilter As AppDomainFilter = DirectCast(m_appDomain1.CreateInstanceAndUnwrap(szAssm, GetType(AppDomainFilter).FullName), AppDomainFilter)

            'Actually run the test
            Try
                'A StepIn here should step into the next AppDomain
                proxyFilter.Filter(msg, Me)
                Return MFTestResults.Fail
            Catch ex As Exception When True
                'A StepOver in the filter above should land here
                If appDomainTracker <> 5 Then Return MFTestResults.Fail
                appDomainTracker = 6

                If [String].Compare(msg, ex.Message) <> 0 Then Return MFTestResults.Fail ' Don't use VB String comparison helper, as it's not implemented.
            Finally
                If appDomainTracker <> 6 Then appDomainTestSucceeded = False

                appDomainTracker = 7

                'Terminate AppDomain
                AppDomain.Unload(m_appDomain1)
                m_appDomain1 = Nothing
                proxyFilter = Nothing
            End Try

            If appDomainTestSucceeded Then
                Return MFTestResults.Pass
            End If

            Return MFTestResults.Fail

        End Function

        Friend Sub FailTestIfNotAndIncrement(ByVal x As Integer)
            If appDomainTracker <> x Then
                appDomainTestSucceeded = False
            End If

            appDomainTracker = x + 1
        End Sub

        Friend Sub FailTest()
            appDomainTestSucceeded = False
        End Sub

        Private Class AppDomainThrower
            Inherits MarshalByRefObject

            Public Sub Throws(ByVal msg As String, ByRef et As AppDomainExceptionTest)
                et.FailTestIfNotAndIncrement(1)

                Try
                    'A StepOver should immediately stop in the finally, since there are no handlers (filtered or not) in this AppDomain.
                    Throw New Exception(msg)
                Finally
                    'A StepOver here should land in the filter in the previous AppDomain
                    et.FailTestIfNotAndIncrement(2)
                End Try
            End Sub
        End Class

        Private Class AppDomainFilter 'Was private when nested
            Inherits MarshalByRefObject

            Public Sub Filter(ByVal msg As String, ByRef et As AppDomainExceptionTest)
                'Initialize AppDomain
                Dim szAssm As String = GetType(ExceptionTest).Assembly.FullName
                Dim m_appDomain2 As AppDomain = AppDomain.CreateDomain("ThrowerDomain")
                m_appDomain2.Load(szAssm)
                Dim proxyThrower As AppDomainThrower = CType(m_appDomain2.CreateInstanceAndUnwrap(szAssm, GetType(AppDomainThrower).FullName), AppDomainThrower)

                'Actually run the test
                Try
                    et.FailTestIfNotAndIncrement(0)

                    'A StepIn here should step into the next AppDomain
                    proxyThrower.Throws(msg, et)
                    et.FailTest()
                Catch e As Exception When Check(msg, e.Message, et)
                    et.FailTest()
                Finally
                    et.FailTestIfNotAndIncrement(4)

                    'Terminate AppDomain
                    AppDomain.Unload(m_appDomain2)
                    m_appDomain2 = Nothing
                    proxyThrower = Nothing
                    'A StepOver here should land in the original AppDomain
                End Try
            End Sub

            Private Function Check(ByVal s1 As String, ByVal s2 As String, ByRef et As AppDomainExceptionTest) As Boolean
                et.FailTestIfNotAndIncrement(3)

                If [String].Compare(s1, s2) <> 0 Then
                    et.FailTest()
                End If

                Return False
            End Function
        End Class

    End Class

End Namespace
