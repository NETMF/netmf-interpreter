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
    internal abstract class Element : IEnumerable
    {
        protected PointF     m_orig;
        protected RectangleF m_bounds;
        protected Font       m_font;
        protected Brush      m_brush;
        protected object     m_value;
        protected bool       m_fSelected;

        protected Element( Font font, Brush brush )
        {
            m_font  = font;
            m_brush = brush;
            m_orig  = new PointF( 0, 0 );
        }

        internal virtual void SetOrigin( float x, float y )
        {
            m_orig.X = x;
            m_orig.Y = y;
        }

        internal virtual PointF Prepare( Graphics gfx, PointF pt )
        {
            return pt;
        }

        internal abstract void Draw( Graphics gfx );


        public virtual IEnumerator GetEnumerator()
        {
            return new ArrayList().GetEnumerator();
        }


        internal virtual bool Selected
        {
            get
            {
                return m_fSelected;
            }

            set
            {
                m_fSelected = value;
            }
        }

        internal object Value
        {
            get
            {
                return m_value;
            }

            set
            {
                m_value = value;
            }
        }

        internal virtual RectangleF Bounds
        {
            get
            {
                return m_bounds;
            }
        }


        protected PointF PrepareText( Graphics gfx, string text, PointF pt )
        {
            SizeF size = gfx.MeasureString( text, m_font );

            m_bounds = new RectangleF( pt.X, pt.Y, size.Width, size.Height );

            pt.X += size.Width;

            return pt;
        }

        internal virtual Element Contains( PointF pt )
        {
            RectangleF bounds = m_bounds;

            bounds.X += m_orig.X;
            bounds.Y += m_orig.Y;

            if(bounds.Contains( pt ))
            {
                return this;
            }

            return null;
        }

        internal virtual Element FindMatch( object o )
        {
            if(o.Equals( m_value ))
            {
                return this;
            }

            return null;
        }
    }


    internal class ElementGroup : Element
    {
        ArrayList m_elements;
        bool      m_fValidBounds;

        internal ElementGroup() : base( null, null )
        {
            m_elements     = new ArrayList();
            m_fValidBounds = false;
        }


        public override IEnumerator GetEnumerator()
        {
            return m_elements.GetEnumerator();
        }


        internal override void SetOrigin( float x, float y )
        {
            base.SetOrigin( x, y );

            foreach(Element el in m_elements)
            {
                el.SetOrigin( x, y );
            }
        }

        internal override void Draw( Graphics gfx )
        {
            foreach(Element el in m_elements)
            {
                el.Draw( gfx );
            }
        }

        internal override Element Contains( PointF pt )
        {
            foreach(Element el in m_elements)
            {
                Element el2 = el.Contains( pt );

                if(el2 != null)
                {
                    return el2;
                }
            }

            return null;
        }

        internal override Element FindMatch( object o )
        {
            foreach(Element el in m_elements)
            {
                Element el2 = el.FindMatch( o );

                if(el2 != null)
                {
                    return el2;
                }
            }

            return null;
        }

        internal override bool Selected
        {
            set
            {
                foreach(Element el in m_elements)
                {
                    el.Selected = value;
                }
            }
        }

        internal override RectangleF Bounds
        {
            get
            {
                if(m_fValidBounds == false)
                {
                    float xMin   = 0;
                    float xMax   = 0;
                    float yMin   = 0;
                    float yMax   = 0;
                    bool  fFirst = true;

                    foreach(Element el in m_elements)
                    {
                        RectangleF bounds = el.Bounds;
                        float      xMin2  = bounds.Left;
                        float      xMax2  = bounds.Right;
                        float      yMin2  = bounds.Top;
                        float      yMax2  = bounds.Bottom;

                        if(fFirst)
                        {
                            xMin = xMin2;
                            xMax = xMax2;
                            yMin = yMin2;
                            yMax = yMax2;

                            fFirst = false;
                        }
                        else
                        {
                            if(xMin > xMin2) xMin = xMin2;
                            if(xMax < xMax2) xMax = xMax2;
                            if(yMin > yMin2) yMin = yMin2;
                            if(yMax < yMax2) yMax = yMax2;
                        }
                    }

                    m_bounds       = new RectangleF( xMin, yMin, xMax - xMin, yMax - yMin );
                    m_fValidBounds = true;
                }

                return m_bounds;
            }
        }


        internal void Add( Element el )
        {
            m_fValidBounds = false;

            m_elements.Add( el );
        }
    }


    internal class TextElement : Element
    {
        internal string m_text;

        protected TextElement( Font font, Brush brush ) : base( font, brush )
        {
            m_text = null;
        }

        internal TextElement( Font font, Brush brush, string text ) : base( font, brush )
        {
            m_text = text;
        }

        internal override PointF Prepare( Graphics gfx, PointF pt )
        {
            if(m_text == null) m_text = this.ToString();

            return PrepareText( gfx, m_text, pt );
        }

        internal override void Draw( Graphics gfx )
        {
            RectangleF bounds = m_bounds;
            Brush      br;

            bounds.X += m_orig.X;
            bounds.Y += m_orig.Y;

            if(m_fSelected)
            {
                gfx.FillRectangle( SystemBrushes.Highlight, bounds );

                br = SystemBrushes.HighlightText;
            }
            else
            {
                br = m_brush;
            }

            gfx.DrawString( m_text, m_font, br, bounds.X, bounds.Y );
        }
    }

    internal class WordElement : TextElement
    {
        internal WordElement( Font font, Brush brush, uint val ) : base( font, brush )
        {
            m_value = val;
        }

        public override string ToString()
        {
            return String.Format( "{0,8:X8}", m_value );
        }
    }

    internal class AddressElement : WordElement
    {
        internal AddressElement( Font font, Brush brush, uint address ) : base( font, brush, address )
        {
        }


        public override string ToString()
        {
            return String.Format( "[{0,8:X8}]", m_value );
        }
    }

    internal class ShortElement : TextElement
    {
        internal ShortElement( Font font, Brush brush, ushort val ) : base( font, brush )
        {
            m_value = val;
        }


        public override string ToString()
        {
            return String.Format( "{0,4:X4}", m_value );
        }
    }

    internal class ByteElement : TextElement
    {
        internal ByteElement( Font font, Brush brush, byte val ) : base( font, brush )
        {
            m_value = val;
        }


        public override string ToString()
        {
            return String.Format( "{0,2:X2}", m_value );
        }
    }

    internal class CharsElement : Element
    {
        internal char[] m_val;

        internal CharsElement( Font font, Brush brush, MemoryHandler.Region val, uint address, int size ) : base( font, brush )
        {
            m_val = new char[size];

            for(int i=0; i<size; i++)
            {
                byte c;

                if(val.GetByte( address++, out c ) == false)
                {
                    m_val[i] = '?';
                }
                else if(c >= 20 && c < 128)
                {
                    m_val[i] = (char)c;
                }
                else
                {
                    m_val[i] = '.';
                }
            }
        }

        internal override PointF Prepare( Graphics gfx, PointF pt )
        {
            m_bounds = new RectangleF( pt.X, pt.Y, m_val.Length * m_font.Size, m_font.Height );

            pt.X += m_bounds.Width;

            return pt;
        }

        internal override void Draw( Graphics gfx )
        {
            float x = m_bounds.X + m_orig.X;
            float y = m_bounds.Y + m_orig.Y;

            for(int i=0; i<m_val.Length; i++)
            {
                gfx.DrawString( m_val[i].ToString(), m_font, m_brush, x, y );

                x += m_font.Size;
            }
        }
    }


    [Serializable]
    public class MemorySnapshot
    {
        [Serializable]
        public class MemoryBlock
        {
            public uint   m_address;
            public byte[] m_data;
        }

        public MemoryBlock m_block_RAM;
        public MemoryBlock m_block_FLASH;

        public uint[]      m_registers = new uint[16];
        public uint        m_cpsr;
        public uint        m_BWA;
        public uint        m_BWC;

        public uint        m_flashBase;
        public uint        m_flashSize;
        public uint        m_ramBase;
        public uint        m_ramSize;
    }


    public class MemoryHandler
    {
        public class Region
        {
            uint    m_address;
            short[] m_data;

            internal Region( uint address, short[] data )
            {
                m_address = address;
                m_data    = data;
            }

            public uint Address
            {
                get
                {
                    return m_address;
                }
            }

            public int Length
            {
                get
                {
                    return m_data.Length;
                }
            }

            public bool GetWord( uint address, out uint val )
            {
                return GetData( address, 4, out val );
            }

            public bool GetShort( uint address, out ushort val )
            {
                uint data;
                bool res;

                res = GetData( address, 2, out data );

                val = (ushort)data;

                return res;
            }

            public bool GetByte( uint address, out byte val )
            {
                uint data;
                bool res;

                res = GetData( address, 1, out data );

                val = (byte)data;

                return res;
            }

            private bool GetData( uint address, int size, out uint val )
            {
                if(address >= m_address)
                {
                    address -= m_address;

                    if(address + size <= m_data.Length)
                    {
                        uint res = 0;
                        
                        while(size-- > 0)
                        {
                            short v = m_data[address+size];

                            if(v < 0)
                            {
                                val = 0xFFFFFFFF;

                                return false;
                            }

                            res <<= 8;
                            res  |= (uint)((byte)v);
                        }

                        val = res;

                        return true;
                    }
                }

                val = 0xFFFFFFFF;

                return false;
            }
        }

        internal class Chunk
        {
            const uint CHUNKSIZE = 512;

            internal uint    m_address;
            internal short[] m_data;

            internal Chunk( MemorySnapshot.MemoryBlock mb )
            {
                int len = mb.m_data.Length;

                m_address = mb.m_address;
                m_data    = new short[len];

                for(int i=0; i<len; i++)
                {
                    m_data[i] = mb.m_data[i];
                }
            }

            internal Chunk( _DBG.AbortHandler ah, uint address )
            {
                byte[] data = new byte[Chunk.CHUNKSIZE];
                int    i;

                m_address = address & ~(Chunk.CHUNKSIZE-1);
                m_data    = new short[Chunk.CHUNKSIZE];

                if(ah != null)
                {
                    for(int tries=0; tries < 5; tries++)
                    {
                        if(ah.ReadMemory( m_address, data, 0, (int)Chunk.CHUNKSIZE ))
                        {
                            for(i=0; i<Chunk.CHUNKSIZE; i++)
                            {
                                m_data[i] = data[i];
                            }

                            return;
                        }
                    }
                }

                for(i=0; i<Chunk.CHUNKSIZE; i++)
                {
                    m_data[i] = -1;
                }
            }
        }


        _DBG.AbortHandler m_ah;
        ArrayList         m_chunks;

        public MemoryHandler( _DBG.AbortHandler ah )
        {
            m_ah     = ah;
            m_chunks = new ArrayList();
        }

        public void PopulateFromSnapshot( MemorySnapshot mem )
        {
            m_chunks.Clear();

            if(mem.m_block_RAM   != null) m_chunks.Add( new Chunk( mem.m_block_RAM   ) );
            if(mem.m_block_FLASH != null) m_chunks.Add( new Chunk( mem.m_block_FLASH ) );
        }

        public Region ReadRegion( uint address, int size )
        {
            short[] data      = new short[size];
            int     dstOffset = 0;
            int     pos       = 0;
            Region  res       = new Region( address, data );

            while(size > 0)
            {
                Chunk chunk;

                if(pos >= m_chunks.Count)
                {
                    chunk = null;
                }
                else
                {
                    chunk = (Chunk)m_chunks[pos];
                }

                if(chunk == null || chunk.m_address > address)
                {
                    chunk = new Chunk( m_ah, address );

                    m_chunks.Insert( pos, chunk );
                }


                int srcSize = chunk.m_data.Length;

                if(chunk.m_address + srcSize >= address)
                {
                    int srcOffset = (int)(address - chunk.m_address);
                    int move;

                    if(srcOffset + size >= srcSize)
                    {
                        move = srcSize - srcOffset;
                    }
                    else
                    {
                        move = size;
                    }

                    Array.Copy( chunk.m_data, srcOffset, data, dstOffset, move );

                    address   += (uint)move;
                    dstOffset +=       move;
                    size      -=       move;

                    //
                    // Deal with wrap-arounds.
                    //
                    if(address == 0)
                    {
                        pos = -1;
                    }
                }

                pos++;
            }

            return res;
        }

        public byte[] ReadRegionAsBuffer( uint address, int size )
        {
            Region data = ReadRegion( address, size );
            byte[] buf  = new byte[size];

            for(int pos=0; pos<size; pos+=1)
            {
                data.GetByte( address++, out buf[pos] );
            }

            return buf;
        }
    }
}

