////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.Collections;
using System.Runtime.CompilerServices;

using Microsoft.SPOT;
using Microsoft.SPOT.TestFramework;

namespace Microsoft.SPOT.Test.LCD
{
    public class GifArray :            Microsoft.SPOT.TestFramework.Test
    {
        public GifArray( string comment ):base( comment ){}

        public override void Run()
        {
            Bitmap bitmap = new Bitmap( Dimensions.Width, Dimensions.Height );
                
            try
            {
                bitmap.Clear();

                byte[] byteArr = Resources.GetBytes( Resources.BinaryResources.GifNormal );
                Bitmap bmp = new Bitmap( byteArr, Bitmap.BitmapImageType.Gif );

                bitmap.DrawImage( 0, 0, bmp, 0, 0, bmp.Width, bmp.Height );

                bitmap.Flush();
                Thread.Sleep(2000);
                Pass = true;
            }
            catch(Exception e)
            {
                UnexpectedException( e );
            }
            
        }
    }
    public class JpegArray :           Microsoft.SPOT.TestFramework.Test
    {
        public JpegArray( string comment ):base( comment ){}

        public override void Run()
        {
            Bitmap bitmap = new Bitmap( Dimensions.Width, Dimensions.Height );
                
            try
            {
                bitmap.Clear();

                byte[] byteArr = Resources.GetBytes( Resources.BinaryResources.JpegNormal );
                Bitmap bmp = new Bitmap( byteArr, Bitmap.BitmapImageType.Jpeg );

                bitmap.DrawImage( 0, 0, bmp, 0, 0, bmp.Width, bmp.Height );

                bitmap.Flush();
                Thread.Sleep(2000);
                Pass = true;
            }
            catch(Exception e)
            {
                UnexpectedException( e );
            }
            
        }
    }

    public class Orientation :         Microsoft.SPOT.TestFramework.Test
    {
        public Orientation( string comment ):base( comment ){}

        public override void Run()
        {
            try
            {
                Bitmap bmp = new Bitmap( Dimensions.Width, Dimensions.Height );

                bmp.DrawLine( Utility.FromRGB( 255, 0, 0 ),
                    1,
                    Dimensions.Width / 2, Dimensions.Height / 2, Dimensions.Width, Dimensions.Height / 2 );

                bmp.DrawLine( Utility.FromRGB( 0, 255, 0 ),
                    1,
                    Dimensions.Width / 2, Dimensions.Height / 2, Dimensions.Width / 2, 0 );

                bmp.Flush();

                Thread.Sleep( 3000 );
                Pass = true;
            }
            catch(Exception e)
            {
                UnexpectedException( e );
            }
        }
    }

    public class GreenBlueX :          Microsoft.SPOT.TestFramework.Test
	{
		public GreenBlueX( string comment ):base( comment ){}
		public override void Run()
		{
			try
			{
				Bitmap bmp = new Bitmap( Dimensions.Width, Dimensions.Height );

				int size = System.Math.Min( Dimensions.Width, Dimensions.Height );

				for(int i = 0; i < size; ++i)
				{
					bmp.SetPixel( i, i, (Microsoft.SPOT.Presentation.Media.Color)((255 - i) << 16) );
				}
				for(int i = 0; i < size; i += 2)
				{
					bmp.SetPixel( size - i, i, (Microsoft.SPOT.Presentation.Media.Color)(i << 8) );
				}
				bmp.Flush();

				Thread.Sleep( 3000 );
				Pass = true;
			}
			catch(Exception e)
			{
				UnexpectedException( e );
			}
		}
	}

	public class DrawTextTest :        Microsoft.SPOT.TestFramework.Test
	{
		public DrawTextTest( string comment ):base( comment ){}
		public override void Run()
		{
			try
			{
				Bitmap bmp = new Bitmap( Dimensions.Width, Dimensions.Height );
				Font font = Resources.GetFont( Resources.FontResources.GLANCEABLE );

                Random rand = new Random();

				for(int x = 0; x < 10; x++)
				{
					int baseColor = rand.Next( 0xffff );
					for(int i = 0; i < Dimensions.Height; i++)
					{
						bmp.Clear();
						bmp.DrawText( "Hello World :) 12345", font, (Presentation.Media.Color)(((255 - i) << 16) | baseColor), 0, i );
						bmp.Flush();
					}
				}
				Pass = true;
			}
			catch(Exception e)
			{
				UnexpectedException( e );
			}
		}
	}

	public class RandomSetPixel :      Microsoft.SPOT.TestFramework.Test
	{
		public RandomSetPixel( string comment ):base( comment ){}
		public override void Run()
		{
			try
			{
				Bitmap bmp = new Bitmap( Dimensions.Width, Dimensions.Height );

                Random rand = new Random();
				for(int i = 0; i < 1000; i++)
				{
                    bmp.SetPixel(rand.Next(Dimensions.Width), rand.Next(Dimensions.Height), (Presentation.Media.Color)rand.Next(0xFFFFFF));
					bmp.Flush();
				}
				Pass = true;
			}
			catch(Exception e)
			{
				UnexpectedException( e );
			}
		}
	}

	public class RandomDrawLine :      Microsoft.SPOT.TestFramework.Test
	{
		public RandomDrawLine( string comment ):base( comment ){}
		public override void Run()
		{
			try
			{
				Bitmap bmp = new Bitmap( Dimensions.Width, Dimensions.Height );
                Random rand = new Random();

				for(int i = 0; i < 1000; i++)
				{
					bmp.DrawLine( (Presentation.Media.Color)rand.Next( 0xFFFFFF ), 1,
                        rand.Next(Dimensions.Width), rand.Next(Dimensions.Height), rand.Next(Dimensions.Width), rand.Next(Dimensions.Height));
					bmp.Flush();
				}
				Pass = true;
			}
			catch(Exception e)
			{
				UnexpectedException( e );
			}
		}
	}

	public class RandomDrawCircle :    Microsoft.SPOT.TestFramework.Test
	{
		public RandomDrawCircle( string comment ):base( comment ){}
		public override void Run()
		{
			try
			{
				Bitmap bmp = new Bitmap( Dimensions.Width, Dimensions.Height );
                Random rand = new Random();

				for(int i = 0; i < 1000; i++)
				{
                    int radius = rand.Next(100);
                    bmp.DrawEllipse((Presentation.Media.Color)rand.Next(0xFFFFFF), 1,
                        rand.Next(Dimensions.Width), rand.Next(Dimensions.Height), radius, radius, 0, 0, 0, 0, 0, 0, 0);
					bmp.Flush();
				}
				Pass = true;
			}
			catch(Exception e)
			{
				UnexpectedException( e );
			}
		}
	}

	public class RandomDrawRectangle : Microsoft.SPOT.TestFramework.Test
	{
		public RandomDrawRectangle( string comment ):base( comment ){}
		public override void Run()
		{
			try
			{
				Bitmap bmp = new Bitmap( Dimensions.Width, Dimensions.Height );
                Random rand = new Random();

				for (int i = 0; i < 1000; i++)
				{
                    Presentation.Media.Color fillColor = (Presentation.Media.Color)rand.Next(0xFFFFFF);
                    bmp.DrawRectangle((Presentation.Media.Color)rand.Next(0xFFFFFF), rand.Next(1),
                        rand.Next(Dimensions.Width), rand.Next(Dimensions.Height), rand.Next(Dimensions.Width), rand.Next(Dimensions.Height), 0, 0, fillColor, 0, 0, fillColor, 0, 0, (ushort)rand.Next(256));
					bmp.Flush();
				}
				Pass = true;
			}
			catch(Exception e)
			{
				UnexpectedException( e );
			}
		}
	}

	public class RandomDrawImage :     Microsoft.SPOT.TestFramework.Test
	{
		public RandomDrawImage( string comment ):base( comment ){}
		public override void Run()
		{
			try
			{
				Bitmap scratch = new Bitmap( 64, 32 );

				for(int x = 0; x < scratch.Width; x++)
				{
					for(int y = 0; y < scratch.Height; y++)
					{
						scratch.SetPixel( x, y, Utility.FromRGB( x * 255 / (scratch.Width), y * 255 / (scratch.Height), 255 - x * 255 / (scratch.Width) ) );
					}
				}

				Bitmap bmp = new Bitmap( Dimensions.Width, Dimensions.Height );
                Random rand = new Random();

				for(int i = 0; i < 1000; i++)
				{
					bmp.DrawImage( rand.Next( Dimensions.Width ), rand.Next( Dimensions.Height ), scratch, 0, 0, scratch.Width, scratch.Height );
					bmp.Flush();
				}
				Pass = true;
			}
			catch(Exception e)
			{
				UnexpectedException( e );
			}
		}
	}

	public class DrawImageParameters : Microsoft.SPOT.TestFramework.Test
	{
		public DrawImageParameters( string comment ):base( comment ){}
		public override void Run()
		{
			try
			{
				Bitmap bmp = new Bitmap( 20, 20 );

				bmp.DrawImage( 0, 0, bmp, 0, 0, bmp.Width, bmp.Height );
			}
			catch
			{
				Pass = true;
			}
		}
	}

	public class StretchImage :        Microsoft.SPOT.TestFramework.Test
	{
		public StretchImage( string comment ):base( comment ){}
		public override void Run()
		{
			try
			{
				Bitmap bmp = new Bitmap( Dimensions.Width, Dimensions.Height );

				Resources.BitmapResources[] bitmaps = new Resources.BitmapResources[]
				{
					Resources.BitmapResources.Outlook0,
					Resources.BitmapResources.Outlook1,
					Resources.BitmapResources.Outlook2,
				};

				for(int i = 0; i < bitmaps.Length; i++ )
				{
					Bitmap src = Resources.GetBitmap( bitmaps[i] );

					for(int s = 0; s <= 800; s++)
					{
						int w = s * Dimensions.Width / 200;
						int h = s * Dimensions.Height / 200;
						int x = (Dimensions.Width - w) / 2;
						int y = (Dimensions.Height - h) / 2;

						bmp.StretchImage( x, y, src, w, h, 256 );
						bmp.Flush();
					}

					Thread.Sleep( 3000 );
				}
				Pass = true;
			}
			catch (Exception e)
			{
				UnexpectedException( e );
			}
		}
	}

	public class SlideShow :           Microsoft.SPOT.TestFramework.Test
	{
		public SlideShow( string comment ):base( comment ){}
		public override void Run()
		{
			try
			{                
				Bitmap bmp = new Bitmap( Dimensions.Width, Dimensions.Height );
                                
				Resources.BitmapResources[] bitmaps = new Resources.BitmapResources[]
				{
					Resources.BitmapResources.Outlook0,
					Resources.BitmapResources.Outlook1,
					Resources.BitmapResources.Outlook2,
				};

				for(int i = 0; i < bitmaps.Length; i++ )
				{
					Bitmap src = Resources.GetBitmap( bitmaps[i] );

					bmp.DrawImage( 0, 0, src, 0, 0, src.Width, src.Height );                    
					bmp.Flush();                    
                    
					Thread.Sleep( 3000 );
				}
				Pass = true;
			}
			catch(Exception e)
			{
				UnexpectedException( e );
			}
		}
	}

	public class Bouncy :              Microsoft.SPOT.TestFramework.Test
	{
		public Bouncy( string comment ):base( comment ){}
		public override void Run()
		{
			try
			{
				Bitmap bmp = new Bitmap( Dimensions.Width, Dimensions.Height );

				Bitmap src = Resources.GetBitmap( Resources.BitmapResources.SPOTLOGO );
                Random rand = new Random();

				int xPos = rand.Next( Dimensions.Width - src.Width );
				int yPos = rand.Next( Dimensions.Height - src.Height );

				int xDir = 3;
				int yDir = 4;

				for(int animcount = 0; animcount < 1000; animcount++)
				{
					bmp.Clear();
					bmp.DrawImage( xPos, yPos, src, 0, 0, src.Width, src.Height );
					bmp.Flush();

					xPos = xPos + xDir;
					yPos = yPos + yDir;

					if(xPos + src.Width > bmp.Width || xPos < 0)
					{
						xDir = -xDir;
						xPos += xDir;
					}
					if(yPos + src.Height > bmp.Height || yPos < 0)
					{
						yDir = -yDir;
						yPos += yDir;
					}

					Thread.Sleep( 10 );
				}
				Pass = true;
			}
			catch(Exception e)
			{
				UnexpectedException( e );
			}
		}
	}

	public class DrawTextInRect :      Microsoft.SPOT.TestFramework.Test
	{
		public DrawTextInRect( string comment ):base( comment ){}
		public override void Run()
		{
			try
			{
				Font font = Resources.GetFont( Resources.FontResources.GLANCEABLE );
				Bitmap bmp = new Bitmap( Dimensions.Width, Dimensions.Height );

				uint[] flags = { 
								   Bitmap.DT_WordWrap | Bitmap.DT_AlignmentLeft,
								   Bitmap.DT_WordWrap | Bitmap.DT_AlignmentCenter,
								   Bitmap.DT_WordWrap | Bitmap.DT_AlignmentRight,
				};


				string s = "Alas! poor Horatio. I knew him, my friend; a fellow of infinite jest, of most excellent fancy; he hath borne me on his back a thousand times; and now, where be your gibes now? your gambols? your songs? your flashes of merriment, that were wont to set the table on a roar? Not one now, to mock your own grinning? quite chapfallen? Now get you to my lady?s chamber, and tell her, let her paint an inch thick, to this favour she must come; make her laugh at that. Prithee, what say you?";

				for(int i = 0; i < flags.Length; i++)
				{
					bmp.DrawRectangle( (Presentation.Media.Color)0xFF0000, 0, 0, 0, Dimensions.Width, Dimensions.Height, 0, 0, (Presentation.Media.Color)0xFF0000, 0, 0, (Presentation.Media.Color)0xFF0000, 0, 0, Bitmap.OpacityOpaque );

					Presentation.Media.Color color = (Presentation.Media.Color)0x0000FF;
					bmp.DrawTextInRect( s, 0, 0, Dimensions.Width, Dimensions.Height, flags[i], color, font );

					bmp.Flush();

					Thread.Sleep( 2000 );
				}
				Pass = true;
			}
			catch(Exception e)
			{
				UnexpectedException( e );
			}
		}
	}
 
    public class Dimensions
    {
        public static int Width;
        public static int Height;

        static Dimensions()
        {

            Width = Microsoft.SPOT.Presentation.SystemMetrics.ScreenWidth;
            Height = Microsoft.SPOT.Presentation.SystemMetrics.ScreenHeight;
        }
    }

    public abstract class Utility
    {
        public static Presentation.Media.Color FromRGB( int r, int g, int b )
        {
            return (Presentation.Media.Color)(((byte)b) << 16 | ((byte)g) << 8 | ((byte)r));
        }
    }

	public class MainEntryPoint
	{
		public static void Main()
		{
			TestSuite suite = new TestSuite();

            suite.RunTest( new GifArray           ("Verify use of gif array results in display of eiffle tower") );
            suite.RunTest( new JpegArray          ("Verify use of jpeg array results in display of waterfall") );
            suite.RunTest( new Orientation        ("Verify red is right, and green is up") );
            suite.RunTest( new GreenBlueX         ("Verify green / blue X"                ) );
			suite.RunTest( new DrawTextTest       ("Verify Hello World appears in Text") );
			suite.RunTest( new RandomSetPixel     ("Verify Random SetPixel() calls") );
			suite.RunTest( new RandomDrawLine     ("Verify Random DrawLine() calls with thickness = 1") );
			suite.RunTest( new RandomDrawCircle   ("Verify Random DrawEllipse() calls: basic circles, thinkness = 1, no fill") );
			suite.RunTest( new RandomDrawRectangle("Verify Random DrawRectangle() calls: basic rectangles, thinkness = 1") );
			suite.RunTest( new RandomDrawImage    ("Verify Random DrawImage() calls: gradient image, constant size, entire image. dark blue in upper left") );
			suite.RunTest( new DrawImageParameters("Verify drawimage parameter correctly throws exception on different src/dst parameters") );
			suite.RunTest( new StretchImage       ("Verify images stretched equidistant across screen") );            
			suite.RunTest( new SlideShow          ("Verify 3 bmps blit ed over one another") );
			suite.RunTest( new Bouncy             ("Verify bmp bounced within screen") );
			suite.RunTest( new DrawTextInRect     ("Verify left, center and right justified text") );

			suite.Finished();
		}
	}
}
