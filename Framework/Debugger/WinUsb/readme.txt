/// <summary>
/// Project: WinUsb_cs
/// 
/// ***********************************************************************
/// Software License Agreement
///
/// Licensor grants any person obtaining a copy of this software ("You") 
/// a worldwide, royalty-free, non-exclusive license, for the duration of 
/// the copyright, free of charge, to store and execute the Software in a 
/// computer system and to incorporate the Software or any portion of it 
/// in computer programs You write.   
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
/// THE SOFTWARE.
/// ***********************************************************************
/// 
/// Author               Date        Version
/// Jan Axelson          6/3/08     1.3
///                      8/8/08     1.3.1 Because WinUsb_QueryDeviceInformation appears
///                                       unreliable for detecting device speed, I
///                                       commented out the call to obtain device speed. 
///                      10/1/08    1.4   Minor edits     
///                      10/29/08   1.5   Minor edits mainly for 64-bit compatibility
///                      11/1/08    1.6   Minor edits  
///                      11/9/08    1.7   Minor edits    
///                      2/10/09    1.8   Changes to WinUsb_ReadPipe parameters.   
///                      2/11/09    1.81  Moved Free_ and similar to Finally blocks
///                                       to ensure they execute.   
/// 
/// 
/// This software was created using Visual Studio 2008 Standard Edition with .NET Framework 2.0.
/// 
/// Purpose: 
/// Demonstrates USB communications using the Microsoft WinUSB driver.
/// 
/// Requirements:
/// Windows XP or later and an attached USB device that uses the WinUSB driver.
/// 
/// Description:
/// Finds an attached device whose INF file contains a specific device interface GUID.
/// Enables sending and receiving data via bulk, interrupt, and control transfers.
/// 
/// Uses RegisterDeviceNotification() and WM_DEVICE_CHANGE messages
/// to detect when a device is attached or removed.
/// 
/// For bulk and interrupt transfers, the application uses a Delegate and the BeginInvoke 
/// and EndInvoke methods to read data asynchronously, so the application's main thread 
/// doesn't have to wait for the device to return data. A callback routine uses 
/// marshaling to send data to the form, whose code runs in a different thread. 
///  
/// This software, an example INF file, and companion device firmware are available from 
/// www.Lvr.com
/// 
/// Send comments, bug reports, etc. to jan@Lvr.com 
/// This application has been tested under Windows XP and Windows Vista.
/// </summary>