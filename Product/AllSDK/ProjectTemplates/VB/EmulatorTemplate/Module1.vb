Imports System
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports System.Threading

Imports Microsoft.SPOT.Emulator

Namespace $safeprojectname$
    Module Module1
        Sub Main()
            Call (New Program).Start()
        End Sub
    End Module

    Class Program
        Inherits Emulator

        Public Overrides Sub SetupComponent()
            MyBase.SetupComponent()
        End Sub

        Public Overrides Sub InitializeComponent()
            MyBase.InitializeComponent()

            ' Start the UI in its own thread.
            Dim uiThread As New Thread(AddressOf StartForm)
            uiThread.SetApartmentState(ApartmentState.STA)
            uiThread.Start()
        End Sub

        Public Overrides Sub UninitializeComponent()
            MyBase.UninitializeComponent()

            ' The emulator is stopped. Close the WinForm UI.
            Application.Exit()
        End Sub

        Sub StartForm()
            ' Some initial setup for the WinForm UI
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)

            ' Start the WinForm UI. Run() returns when the form is closed.
            Application.Run(New Form1)

            ' When the user closes the WinForm UI, stop the emulator.
            Stop
        End Sub
    End Class
End Namespace
