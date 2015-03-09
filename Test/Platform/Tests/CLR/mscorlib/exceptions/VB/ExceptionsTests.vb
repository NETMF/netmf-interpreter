Option Explicit On
Option Strict On

Imports System.Diagnostics
Imports Microsoft.SPOT
Imports Microsoft.SPOT.Platform.Test

'IMPORTANT: Run tests FancyNesting, BreakpointTests, and AppDomainFilterTransitions in a debugger to verify stepping and breakpoints work correctly.

Namespace Microsoft.SPOT.Platform.Tests

    Public Class ExceptionTest
        Implements IMFTestInterface

        Dim fTestFailed As Boolean = False

        <SetUp()> _
        Public Function Initialize() As InitializeResult Implements IMFTestInterface.Initialize

            Log.Comment("ExceptionTest starting...")
            Return InitializeResult.ReadyToGo
        End Function

        <TearDown()> _
        Public Sub CleanUp() Implements IMFTestInterface.CleanUp
            Log.Comment("ExceptionTest finished.")
        End Sub

        '--

        Private Sub Throws(ByVal e As Exception)
            Throw e
        End Sub

        Private Function CreateException() As Exception
            Return New Exception()
        End Function

        '--

        <TestMethod()> _
        Public Function FilterFalse() As MFTestResults
            Try

            Try
                Throws(CreateException())
            Catch When False
                Return MFTestResults.Fail
            End Try

            Catch ex As Exception
                Return MFTestResults.Pass
            End Try

            Return MFTestResults.Fail

        End Function

        <TestMethod()> _
        Public Function FilterTrue() As MFTestResults

            Try
                Throw New Exception
            Catch When True
                Return MFTestResults.Pass
            End Try

            Return MFTestResults.Fail

        End Function

        <TestMethod()> _
        Public Function UnwindFilterTrue() As MFTestResults
            'This test should cause a user-unhandled box to pop up in DevEnv
            Try
                FilterFalse()
            Catch When True
                Return MFTestResults.Fail
            End Try
                Return MFTestResults.Pass
        End Function

        <TestMethod()> _
        Public Function UnwindFilterFalse() As MFTestResults
            Try
                FilterFalse()
            Catch When False
                Return MFTestResults.Fail
            End Try
                Return MFTestResults.Pass
        End Function

        Private Shared Function ThrowException() As Boolean
            Throw New Exception
        End Function

        '<TestMethod()> _
        'Public Function UnwindFilterThrowsException() As MFTestResults
        '    Try
        '        Try
        '           Throw New ApplicationException
        '        Catch When ThrowException()
        '            Return MFTestResults.Fail
        '        End Try
        '    Catch
        '        Log.Comment("outer filter caught the exception")
        '    End Try

        '    Return MFTestResults.Pass
        'End Function

        '<TestMethod()> _
        'Public Function UnwindFilterThrowsExceptionWithFinally() As MFTestResults
        '    Try
        '        Try
        '            Throw New ApplicationException
        '        Catch When ThrowException()
        '                Return MFTestResults.Fail
        '        Finally
        '            'This will be run either if the test succeeds or fails.
        '            'The difference is that the finally block encloses the second exception.
        '            'If FindEhBlock returns true on the finally block, then the exception will not get replaced with the correct one.
        '        End Try
        '    Catch
        '        Log.Comment("outer filter caught the exception")
        '    End Try

        '   Return MFTestResults.Pass

        'End Function

        <TestMethod()> _
        Public Function NestedFilterTrue() As MFTestResults
            Try
                Throw New Exception("123")
            Catch
                Try
                    Throw New Exception("234")
                Catch When True
                    Return MFTestResults.Pass
                End Try
            End Try
                Return MFTestResults.Fail
        End Function

        <TestMethod()> _
        Public Function NestedFilterFalse() As MFTestResults
            Try

                Try
                    Throw New Exception("123")
                Catch
                    Try
                        Throw New Exception("234")
                    Catch When False
                        Return MFTestResults.Fail
                    End Try
                End Try

            Catch ex As Exception
                Return MFTestResults.Pass
            End Try

            Return MFTestResults.Fail

        End Function

        <TestMethod()> _
        Public Function MultiCatchFilterUT() As MFTestResults
            Try
                Throw New Exception
            Catch When False
                Return MFTestResults.Fail
            Catch e As Exception
                Return MFTestResults.Pass
            End Try
                Return MFTestResults.Fail
        End Function

        <TestMethod()> _
        Public Function MultiCatchFilterUTU() As MFTestResults
            Try
                Throw New Exception
            Catch When False
                Return MFTestResults.Fail
            Catch e As ApplicationException
                Return MFTestResults.Fail
            Catch When True
                Return MFTestResults.Pass
            End Try
                Return MFTestResults.Fail
        End Function

        <TestMethod()> _
        Public Function CatchAll() As MFTestResults
            Try
                Throw New Exception
            Catch
                Return MFTestResults.Pass
            End Try
                Return MFTestResults.Fail
        End Function

        <TestMethod()> _
        Public Function FancyNesting() As MFTestResults
            Dim check As Integer = 0
            Dim ret As MFTestResults = MFTestResults.Pass

            fTestFailed = False

            Try
                FilterChecker(check, 0)
                Try
                    FilterChecker(check, 1)
                    ret = FancyNesting2(1) 'Do a StepOver here, and the step should complete on the 'Catch When False' block below.
                Finally
                    'After running the finally's in FancyNesting2, the stepper should complete here.
                    'A StepOver should complete inside the catch block.
                    FilterChecker(check, 2)
                End Try
            Catch ex As ApplicationException When False 'StepOver/In here and be whisked back inside FancyNesting2's outer finally block
            Catch ex As ApplicationException
                FilterChecker(check, 3)
            Finally
                FilterChecker(check, 4) 'A StepOver here should complete on the last line of this function.
            End Try

            FilterChecker(check, 5)

            If fTestFailed Then
                Return MFTestResults.Fail
            End If

            Return ret

        End Function

        Private Function FancyNesting2(ByVal x As Integer) As MFTestResults
            Dim check As Integer = 0
            Dim fFailed As Boolean = False
            Dim ret As MFTestResults = MFTestResults.Pass

            Try
                If x = 3 Then Throw New ApplicationException()
                ret = FancyNesting2(x + 1)
            Finally
                'The step from the filter in FancyNesting should complete here.
                FilterChecker(check, 0)
                Try
                    FilterChecker(check, 1)
                    If x = 2 Then Throw New ApplicationException()
                Catch ex As ApplicationException When True 'A StepIn/Out here should go into the catch block
                    FilterChecker(check, 2)
                Finally
                    'Stepping out here will "return" to the beginning of this finally block because of recursive calls.
                    'After all three copies of this function are popped off the stack, the step should complete in the inner finally in FancyNesting.
                    If (x = 2 And check <> 3) Or (x <> 2 And check <> 2) Then fFailed = True
                End Try
            End Try

            If fFailed Or fTestFailed Then
                Return MFTestResults.Fail
            End If

            Return ret

        End Function

        Private Shared Function VoidThrowException() As MFTestResults

            Dim ranFinally As Boolean = False
            Dim fFailed As Boolean = False

            Try
                Throw New Exception
            Catch
            Finally
                'If this finally gets ran twice then there is a problem with returning CLR_E_PROCCESS_EXCEPTION for an
                'unhandled exception from inside ExecuteIL.
                If ranFinally Then fFailed = True

                ranFinally = True
            End Try
            'Gurantee that the lack of a handler caused the thread to exit, not the end of the function:
            System.Threading.Thread.Sleep(1000)

            If fFailed Then
                Return MFTestResults.Fail
            End If

            Return MFTestResults.Pass

        End Function

        <TestMethod()> _
        Public Function UnhandledException() As MFTestResults
            Dim myThread As System.Threading.Thread
            myThread = New System.Threading.Thread(AddressOf VoidThrowException)
            myThread.Start()
            If Not myThread.Join(5000) Then Return MFTestResults.Fail

            Return MFTestResults.Pass

        End Function

        Private Function FilterChecker(ByRef value As Integer, ByVal expected As Integer, Optional ByVal ret As Boolean = False) As Boolean
            If value <> expected Then
                fTestFailed = True
            End If

            value = expected + 1

            Return ret
        End Function

        <TestMethod()> _
        Public Function FilterContinuityCheck() As MFTestResults
            Dim x As Integer = 1
            Dim check As Integer = 0

            fTestFailed = False

            Try
                'We don't want the first instruction in the protected block to be within the second proctected block
                x = x + 5
                Try
                    check = 1
                    Throw New Exception
                Finally
                    FilterChecker(check, 3)
                End Try
                x = x + 10  'Same for the last instruction
            Catch When FilterChecker(check, 1, False)
            Catch When FilterChecker(check, 2, True)
                FilterChecker(check, 4)
            End Try

            If fTestFailed Then
                Return MFTestResults.Fail
            End If

            Return MFTestResults.Pass

        End Function

        <TestMethod()> _
        Public Function WeirdFiltersTest() As MFTestResults
            Dim check As Integer = 0

            fTestFailed = False

            WeirdFilters(1, check)
            FilterChecker(check, 4)

            If fTestFailed Then
                Return MFTestResults.Fail
            End If

            Return MFTestResults.Pass

        End Function

        Private Sub WeirdFilters(ByVal x As Integer, ByRef check As Integer)
            Try
                If (x = 3) Then Throw New Exception
                WeirdFilters(x + 1, check)
            Catch When x = 1
                FilterChecker(check, 2)
            Finally
                If x = 3 Then FilterChecker(check, 0)
                If x = 2 Then FilterChecker(check, 1)
                If x = 1 Then FilterChecker(check, 3)
            End Try
        End Sub

    End Class

End Namespace
