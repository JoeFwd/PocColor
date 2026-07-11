using System;
using System.IO;
using System.Reflection;

namespace PocColor;

internal class Log
{
	private static StreamWriter Writer;

	static Log()
	{
		try
		{
			Writer = new StreamWriter(new FileStream(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(Assembly.GetExecutingAssembly().Location)), "../../log.txt"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite));
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}

	public static void write(string messageString)
	{
		try
		{
			if (Writer != null)
			{
				Writer.WriteLine(messageString);
				Writer.Flush();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}
}
