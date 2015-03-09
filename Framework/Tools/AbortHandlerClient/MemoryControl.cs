////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

using _DBG = Microsoft.SPOT.Debugger;

namespace AbortHandlerClient
{
	/// <summary>
	/// Summary description for MemoryControl.
	/// </summary>
	public class MemoryControl : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;


        public enum ViewMode
        {
            Bytes      ,
            Shorts     ,
            Words      ,
            Disassembly,
            HeapBlocks ,
        }

        const uint DEFAULT_BACK_WINDOW = 4;


        MemoryHandler m_mh;

        Hashtable     m_symbols_AddressToString;
        Hashtable     m_symbols_StringToAddress;

        ViewMode      m_viewMode = ViewMode.Disassembly;
        uint          m_address;
        int           m_addressIncrement;
        ElementGroup  m_elements;
        Hashtable     m_elementsMemory = new Hashtable();
        Element       m_selected;
        uint          m_selectedAddress;

        Bitmap        m_bm;

        ArrayList     m_history = new ArrayList();
        int           m_historyPos;


		public MemoryControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// Need to add any initialization after the InitializeComponent call
		}

        public void Link( MemoryHandler mh, Hashtable symbols_AddressToString, Hashtable symbols_StringToAddress )
        {
            m_mh                      = mh;
            m_symbols_AddressToString = symbols_AddressToString;
            m_symbols_StringToAddress = symbols_StringToAddress;
        }


        public void Reload()
        {
            SetViewMode( m_viewMode );
        }

        public void ShowMemory( uint address )
        {
            UpdateAddressIncrement();

            UnselectElement();

            m_selectedAddress = RoundAddress( address );

            address -= (uint)(DEFAULT_BACK_WINDOW * m_addressIncrement);

            AddToHistory( address );
            GoToAddress ( address );
        }

        private void GoToAddress( uint address )
        {
            m_address  = RoundAddress( address );
            m_elements = null;
            m_bm       = null;

            this.Invalidate();

            this.Focus();
        }

        private void AddToHistory( uint address )
        {
            CutHistory();

            m_historyPos = m_history.Count;
            m_history.Add( address );
        }

        private void UnselectElement()
        {
            if(m_selected != null) 
            {
                m_selected.Selected = false;

                m_selected = null;
            }
        }

        private void MoveBackwardInHistory()
        {
            if(m_historyPos > 0)
            {
                m_historyPos--;

                UnselectElement();

                m_selectedAddress = (uint)m_history[m_historyPos];

                GoToAddress( m_selectedAddress );
            }
        }

        private void MoveForwardInHistory()
        {
            if(m_historyPos < (m_history.Count-1))
            {
                m_historyPos++;

                UnselectElement();

                m_selectedAddress = (uint)m_history[m_historyPos];

                GoToAddress( m_selectedAddress );
            }
        }

        private void CutHistory()
        {
            int count = m_history.Count;
            int last  = m_historyPos + 1;
            int diff  = count - last;

            if(diff > 0)
            {
                m_history.RemoveRange( last, diff );
            }
        }

        private uint RoundAddress( uint address )
        {
            switch(m_viewMode)
            {
                case ViewMode.Bytes      :                                   break;
                case ViewMode.Shorts     : address = address & ~(uint)(2-1); break;
                case ViewMode.Words      : address = address & ~(uint)(4-1); break;
                case ViewMode.Disassembly: address = address & ~(uint)(4-1); break;
            }

            return address;
        }


        public new void Scroll( int diff )
        {
            GoToAddress( (uint)(m_address + diff * m_addressIncrement) );
        }

        public void SetViewMode( ViewMode vm )
        {
            m_viewMode = vm; UpdateAddressIncrement();

            UnselectElement();

            m_elementsMemory.Clear();

            GoToAddress( m_address );
        }

        private void UpdateAddressIncrement()
        {
            switch(m_viewMode)
            {
                case ViewMode.Disassembly:
                    m_addressIncrement = 4;
                    break;

                case ViewMode.HeapBlocks:
                    m_addressIncrement = 12;
                    break;

                default:
                    m_addressIncrement = 16;
                    break;
            }
        }


		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

        protected override void OnPaint( PaintEventArgs pe )
        {
            if(m_mh == null) return;

            int width  = this.Width;
            int height = this.Height;

            if(m_bm == null || m_bm.Width != width || m_bm.Height != height)
            {
                m_bm = new Bitmap( width, height );

                Graphics             gfx     = Graphics.FromImage( m_bm );
                Font                 f       = this.Font;
                int                  rows    = (height + f.Height - 1) / f.Height;
                uint                 address = m_address;
                MemoryHandler.Region data    = m_mh.ReadRegion( address, m_addressIncrement * rows );
                int                  pos;

                m_elements = new ElementGroup();

                gfx.Clear( Color.White );

                for(float y = 0; y < height; address += (uint)m_addressIncrement)
                {
                    Element el = (Element)m_elementsMemory[ address ];

                    if(el == null)
                    {
                        ElementGroup line = new ElementGroup();
                        PointF       pt   = new PointF( 0, 0 );
                        string       symbol;
                        uint         val32;
                        ushort       val16;
                        byte         val8;

                        symbol = m_symbols_AddressToString[ address ] as string;
                        if(symbol != null)
                        {
                            el = new TextElement( f, Brushes.Purple, String.Format( "{0}:", symbol ) ); line.Add( el );

                            pt.Y += 2;

                            pt = el.Prepare( gfx, pt );

                            pt.X  = 0;
                            pt.Y += f.Height;
                        }

                        el = new AddressElement( f, Brushes.Red, address );

                        InsertElement( line, el, gfx, ref pt, 0, 6 );

                        switch(m_viewMode)
                        {
                            case ViewMode.Words:
                            case ViewMode.Disassembly:
                                for(pos=0; pos<m_addressIncrement; pos+=4)
                                {
                                    if(data.GetWord( (uint)(address + pos), out val32 ))
                                    {
                                        el = new WordElement( f, Brushes.Black, val32 );
                                    }
                                    else
                                    {
                                        el = new TextElement( f, Brushes.Black, "XXXXXXXX" );
                                    }
                                    
                                    InsertElement( line, el, gfx, ref pt, 0, 6 );
                                }
                                break;

                            case ViewMode.Shorts:
                                for(pos=0; pos<m_addressIncrement; pos+=2)
                                {
                                    if(data.GetShort( (uint)(address + pos), out val16 ))
                                    {
                                        el = new ShortElement( f, Brushes.Black, val16 );
                                    }
                                    else
                                    {
                                        el = new TextElement( f, Brushes.Black, "XXXX" );
                                    }

                                    InsertElement( line, el, gfx, ref pt, 0, 6 );
                                }
                                break;

                            case ViewMode.Bytes:
                                for(pos=0; pos<m_addressIncrement; pos+=1)
                                {
                                    if(data.GetByte( (uint)(address + pos), out val8 ))
                                    {
                                        el = new ByteElement( f, Brushes.Black, val8 );
                                    }
                                    else
                                    {
                                        el = new TextElement( f, Brushes.Black, "XX" );
                                    }

                                    InsertElement( line, el, gfx, ref pt, 0, 6 );
                                }
                                break;

                            case ViewMode.HeapBlocks:
                                for(pos=0; pos<m_addressIncrement; pos+=4)
                                {
                                    if(data.GetWord( (uint)(address + pos), out val32 ))
                                    {
                                        el = new WordElement( f, Brushes.Black, val32 );
                                    }
                                    else
                                    {
                                        el = new TextElement( f, Brushes.Black, "XXXXXXXX" );
                                    }

                                    InsertElement( line, el, gfx, ref pt, 0, 6 );
                                }
                                break;
                        }

                        el = new CharsElement( f, Brushes.Blue, data, address, m_addressIncrement );
                       
                        InsertElement( line, el, gfx, ref pt, 10, 10 );

                        if(m_viewMode == ViewMode.Disassembly)
                        {
                            uint res;

                            if(data.GetWord( address, out res ))
                            {
                                string str;
                                uint   target       = 0;
                                bool   targetIsCode = false;

                                try
                                {
                                    str = _DBG.ArmDisassembler.DecodeAndPrint( address, res, ref target, ref targetIsCode );
                                }
                                catch
                                {
                                    str = null;
                                }

                                el = new TextElement( f, Brushes.Maroon, str );

                                if(target != 0xFFFFFFFF)
                                {
                                    el.Value = target;
                                }

                                InsertElement( line, el, gfx, ref pt, 0, 6 );

                                symbol = m_symbols_AddressToString[ target ] as string;
                                if(symbol != null)
                                {
                                    el = new TextElement( f, Brushes.Purple, String.Format( "{0}", symbol ) );
                                    
                                    el.Value = target;

                                    InsertElement( line, el, gfx, ref pt, 0, 0 );
                                }
                                else if(target != 0xFFFFFFFF && targetIsCode == false)
                                {
                                    MemoryHandler.Region data2 = m_mh.ReadRegion( target, 4 );

                                    if(data2.GetWord( target, out res ))
                                    {
                                        el = new TextElement( f, Brushes.Purple, String.Format( "= #0x{0,8:X8}", res ) );
                                        
                                        el.Value = res;

                                        InsertElement( line, el, gfx, ref pt, 0, 0 );
                                    }
                                }
                            }
                        }
                        else if(m_viewMode == ViewMode.HeapBlocks)
                        {
                            if(data.GetWord( address, out val32 ))
                            {
                                uint dt    = (val32 >>  0) & 0x00FF;
                                uint flags = (val32 >>  8) & 0x00FF;
                                uint size  = (val32 >> 16) & 0xFFFF;
      
                                if(dt > (uint)_DBG.RuntimeDataType.DATATYPE_STACK_FRAME) dt++;

                                if(dt >= (uint)_DBG.RuntimeDataType.DATATYPE_FIRST_INVALID)
                                {
                                    el = new TextElement( f, Brushes.Red, String.Format( "Invalid data type: {0}", dt ) );
                                }
                                else
                                {
                                    el = new TextElement( f, Brushes.DarkGreen, String.Format( "{0} {1} Size:{2} Next:{3,8:X8}", (_DBG.RuntimeDataType)dt, flags, size, address + size * 12 ) );

                                    el.Value = (uint)(address + size * 12);
                                }

                                InsertElement( line, el, gfx, ref pt, 10, 6 );
                            }
                        }

                        m_elementsMemory[ address ] = line;

                        el = line;
                    }

                    el.SetOrigin( 0, y );

                    m_elements.Add( el );

                    y += el.Bounds.Bottom;

                    if(m_selected == null)
                    {
                        foreach(Element el2 in el)
                        {
                            if(el2 is AddressElement && el2.Value.Equals( m_selectedAddress ))
                            {
                                m_selected = el2;
                                m_selected.Selected = true;
                                break;
                            }
                        }
                    }
                }

                m_elements.Draw( gfx );
            }

            pe.Graphics.DrawImage( m_bm, 0, 0 );
        }

        private static void InsertElement( ElementGroup line, Element el, Graphics gfx, ref PointF pt, float preX, float postX )
        {
            line.Add( el );

            pt.X += preX;

            pt = el.Prepare( gfx, pt );
            
            pt.X += postX;
        }

        protected override void OnPaintBackground( PaintEventArgs pevent )
        {
            if(m_mh == null)
            {
                base.OnPaintBackground( pevent );
            }
        }

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            // 
            // MemoryControl
            // 
            this.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.Name = "MemoryControl";
            this.Size = new System.Drawing.Size(200, 200);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MemoryControl_MouseDown);
            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.MemoryControl_MouseWheel);

        }
		#endregion

        private void MemoryControl_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            PointF pt = new PointF( e.X, e.Y );

            switch(e.Button)
            {
                case MouseButtons.Left:
                    UnselectElement();

                    m_bm       = null;
                    m_selected = m_elements.Contains( pt );

                    if(m_selected != null)
                    {
                        m_selected.Selected = true;

                        if(e.Clicks == 2)
                        {
                            if(m_selected.Value is uint)
                            {
                                ShowMemory( (uint)m_selected.Value );
                            }
                        }
                    }

                    this.Invalidate();
                    break;

                case MouseButtons.XButton1:
                    MoveBackwardInHistory();
                    break;

                case MouseButtons.XButton2:
                    MoveForwardInHistory();
                    break;
            }
        }

        private void MemoryControl_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Scroll( -SystemInformation.MouseWheelScrollLines * e.Delta / 120 );
        }
	}
}

