using System;

public class Test
{

	public static int Main(string[] args)
	{
		int year, month, day, hour, min, sec;

		if (6 > args.Length)
		{
			Console.WriteLine("Usage: GetUTCTicks.exe Year Month Day Hour Min Sec");
			Console.WriteLine("     Year - 4 digit year (ie. 2000)");
			Console.WriteLine("");
			Console.WriteLine("This tool will convert a UTC date into ticks, which");
			Console.WriteLine("   is useful for the X509 tests. ");
			Console.WriteLine("");
			Console.WriteLine("Here is a quick tutorial on how to get the date's from");
			Console.WriteLine(" X509 Certificates.");
			Console.WriteLine("   1. Open the certificiate in a hex editor.");
			Console.WriteLine("   2. Locate the Valid From and Valid To dates:");
			Console.WriteLine("       Close to the top of the file you will see dates in the following");
			Console.WriteLine("        formats: YYMMDDHHMMSSZ or YYYYMMDDHHSSMMZ");
			Console.WriteLine("         6f 6d 31 11 30 0f 06 03 55 04 03 13 08 69 61 36 | om1 0   U    ia6");
			Console.WriteLine("         34 20 65 6e 74 30 1e 17 0d 30 31 30 32 30 38 31 | 4 ent0   0102081");
			Console.WriteLine("         38 34 37 31 36 5a 17 0d 30 33 30 32 30 38 31 38 | 84716Z  03020818");
			Console.WriteLine("         35 37 31 36 5a 30 7a 31 13 30 11 06 0a 09 92 26 | 5716Z0z1 0     &");
			Console.WriteLine("   3. Extract the Valid From date: (the first date in the certificate)");
			Console.WriteLine("        GetUTCTicks.exe 2001 2 8 18 47 16");
			Console.WriteLine("   4. Extract the Valid To date: (the second date in the certificate)");
			Console.WriteLine("        GetUTCTicks.exe 2003 2 8 18 57 16");
			return -1;
		}

		year  = Convert.ToInt32(args[0]);
		month = Convert.ToInt32(args[1]);
		day   = Convert.ToInt32(args[2]);
		hour  = Convert.ToInt32(args[3]);
		min   = Convert.ToInt32(args[4]);
		sec   = Convert.ToInt32(args[5]);
		DateTime date;

		Console.WriteLine("Date string: y:{0} m:{1} d:{2} h:{3} m:{4} s:{5}", year, month, day, hour, min, sec);

		date = new DateTime(year, month, day, hour, min, sec, DateTimeKind.Utc);

		Console.WriteLine("Date : {0}", date);
		Console.WriteLine("Ticks: {0}", date.Ticks);

		return 100;
		
	}
}