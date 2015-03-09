Option Explicit On
Option Strict On

Imports Microsoft.SPOT
Imports Microsoft.SPOT.Input
Imports Microsoft.SPOT.Presentation
Imports Microsoft.SPOT.Presentation.Controls

Namespace $safeprojectname$

    Public Module Module1
        Private mainWindow As Window

        Public Sub Main()
            Dim myApplication As New Program()

            Dim mainWindow As Window = myApplication.CreateWindow()

            ' Create the object that configures the GPIO pins to buttons.
            Dim inputProvider As New GPIOButtonInputProvider(Nothing)

            ' Start the application
            myApplication.Run(mainWindow)
        End Sub
    End Module


    Public Class Program
        Inherits Microsoft.SPOT.Application
        Public Function CreateWindow() As Window
            ' Create a window object and set its size to the
            ' size of the display.
            mainWindow = New Window()
            mainWindow.Height = SystemMetrics.ScreenHeight
            mainWindow.Width = SystemMetrics.ScreenWidth

            ' Create a single text control.
            Dim text As New Text()

            text.Font = Resources.GetFont(Resources.FontResources.small)
            text.TextContent = Resources.GetString(Resources.StringResources.String1)
            text.HorizontalAlignment = Microsoft.SPOT.Presentation.HorizontalAlignment.Center
            text.VerticalAlignment = Microsoft.SPOT.Presentation.VerticalAlignment.Center

            ' Add the text control to the window.
            mainWindow.Child = text

            ' Connect the button handler to all of the buttons.
            mainWindow.[AddHandler](Buttons.ButtonUpEvent, New RoutedEventHandler(AddressOf OnButtonUp), False)

            ' Set the window visibility to visible.
            mainWindow.Visibility = Visibility.Visible

            ' Attach the button focus to the window.
            Buttons.Focus(mainWindow)

            Return mainWindow
        End Function

        Private Sub OnButtonUp(sender As Object, evt As RoutedEventArgs)
            Dim e As ButtonEventArgs = DirectCast(evt, ButtonEventArgs)

            ' Print the button code to the Visual Studio output window.
            Debug.Print(e.Button.ToString())
        End Sub
    End Class

End Namespace