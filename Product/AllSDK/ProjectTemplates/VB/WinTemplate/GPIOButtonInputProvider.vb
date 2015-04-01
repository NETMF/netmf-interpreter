Imports System
Imports Microsoft.SPOT
Imports Microsoft.SPOT.Input
Imports Microsoft.SPOT.Hardware
Imports Microsoft.SPOT.Presentation

Namespace $safeprojectname$
    ''' <summary>
    ''' Uses the hardware provider to get the pins for handling button input.
    ''' </summary>
    Public NotInheritable Class GPIOButtonInputProvider
        Public ReadOnly Dispatcher As Dispatcher

        Private buttons As ButtonPad()
        Private callback As DispatcherOperationCallback
        Private site As InputProviderSite
        Private source As PresentationSource

        ''' <summary>
        ''' Maps GPIOs to Buttons that can be processed by 
        ''' Microsoft.SPOT.Presentation.
        ''' </summary>
        ''' <param name="source"></param>
        Public Sub New(source As PresentationSource)
            ' Set the input source.
            Me.source = source

            ' Register our object as an input source with the input manager and 
            ' get back an InputProviderSite object which forwards the input 
            ' report to the input manager, which then places the input in the 
            ' staging area.
            site = InputManager.CurrentInputManager.RegisterInputProvider(Me)

            ' Create a delegate that refers to the InputProviderSite object's 
            '     callback = New DispatcherOperationCallback(Function(report As Object)
            callback = New DispatcherOperationCallback(Function(report As Object)
                                                           Dim args As InputReportArgs = DirectCast(report, InputReportArgs)
                                                           Return site.ReportInput(args.Device, args.Report)
                                                       End Function)

            Dispatcher = Dispatcher.CurrentDispatcher

            ' Create a hardware provider.
            Dim hwProvider As New HardwareProvider()

            ' Create the pins that are needed for the buttons.  Default their 
            ' values for the emulator.
            Dim pinLeft As Cpu.Pin = Cpu.Pin.GPIO_Pin0
            Dim pinRight As Cpu.Pin = Cpu.Pin.GPIO_Pin1
            Dim pinUp As Cpu.Pin = Cpu.Pin.GPIO_Pin2
            Dim pinSelect As Cpu.Pin = Cpu.Pin.GPIO_Pin3
            Dim pinDown As Cpu.Pin = Cpu.Pin.GPIO_Pin4

            '' Use the hardware provider to get the pins.  If the left pin is 
            '' not set, assume none of the pins are set, and set the left pin 
            '' back to the default emulator value.
            pinLeft = hwProvider.GetButtonPins(Button.VK_LEFT)
            If (pinLeft = Cpu.Pin.GPIO_NONE) Then
                pinLeft = Cpu.Pin.GPIO_Pin0
            Else
                pinRight = hwProvider.GetButtonPins(Button.VK_RIGHT)
                pinUp = hwProvider.GetButtonPins(Button.VK_UP)
                pinSelect = hwProvider.GetButtonPins(Button.VK_SELECT)
                pinDown = hwProvider.GetButtonPins(Button.VK_DOWN)
            End If

            '' Allocate button pads and assign the (emulated) hardware pins as 
            '' input from specific buttons.
            '' Associate the buttons to the pins as discovered or set above
            Dim buttons As ButtonPad()= {New ButtonPad(Me, Button.VK_LEFT, pinLeft), New ButtonPad(Me, Button.VK_RIGHT, pinRight), New ButtonPad(Me, Button.VK_UP, pinUp), New ButtonPad(Me, Button.VK_SELECT, pinSelect), New ButtonPad(Me, Button.VK_DOWN, pinDown)}
            Me.buttons = buttons

        End Sub

        ''' <summary>
        ''' Represents a button pad on the emulated device, containing five 
        ''' buttons for user input. 
        ''' </summary>
        Friend Class ButtonPad
            Implements IDisposable
            Private button As Button
            Private port As InterruptPort
            Private sink As GPIOButtonInputProvider
            Private buttonDevice As ButtonDevice

            ''' <summary>
            ''' Constructs a ButtonPad object that handles the emulated 
            ''' hardware's button interrupts.
            ''' </summary>
            ''' <param name="sink"></param>
            ''' <param name="button"></param>
            ''' <param name="pin"></param>
            Public Sub New(sink As GPIOButtonInputProvider, button As Button, pin As Cpu.Pin)
                Me.sink = sink
                Me.button = button
                Me.buttonDevice = InputManager.CurrentInputManager.ButtonDevice

                ''' Do not set an InterruptPort with GPIO_NONE.
                If pin <> Cpu.Pin.GPIO_NONE Then
                    ' When this GPIO pin is true, call the Interrupt method.
                    port = New InterruptPort(pin, True, Microsoft.SPOT.Hardware.Port.ResistorMode.PullUp, Microsoft.SPOT.Hardware.Port.InterruptMode.InterruptEdgeBoth)
                    AddHandler port.OnInterrupt, AddressOf Interrupt
                End If
            End Sub

            Protected Overridable Sub Dispose(disposing As Boolean)
                If disposing Then
                    ' dispose managed resources
                    If port IsNot Nothing Then
                        port.Dispose()
                        port = Nothing
                    End If
                End If
                ' free native resources
            End Sub

            Public Sub Dispose() Implements System.IDisposable.Dispose
                Dispose(True)
                GC.SuppressFinalize(Me)
            End Sub

            ''' <summary>
            ''' Handles an interrupt.
            ''' </summary>
            ''' <param name="data1"></param>
            ''' <param name="data2"></param>
            ''' <param name="time"></param>
            Private Sub Interrupt(data1 As UInteger, data2 As UInteger, time As DateTime)
                Dim action As RawButtonActions = If((data2 <> 0), RawButtonActions.ButtonUp, RawButtonActions.ButtonDown)

                Dim report As New RawButtonInputReport(sink.source, time, button, action)

                ' Queue the button press to the input provider site.
                sink.Dispatcher.BeginInvoke(sink.callback, New InputReportArgs(buttonDevice, report))
            End Sub

        End Class

    End Class
End Namespace
