namespace BBP_Gen.Main;

public class ConsoleLogger(string id) // Just a simple logger to include IDs and different colors, maybe there was some faster way, but I don't care
{
	public void Log(object msg, ConsoleColor color, bool write = false)
	{
		Console.BackgroundColor = color;
		if (!write)
			Console.WriteLine($"[{ID}] {msg}");
		else
			Console.Write($"[{ID}] {msg}");

		Console.ResetColor();
	}

	public void Log(object msg, bool write = false)
	{
		if (!write)
			Console.WriteLine($"[{ID}] {msg}");
		else
			Console.Write($"[{ID}] {msg}");
	}

	public static void DirectLog(object msg, ConsoleColor color = ConsoleColor.Black, bool write = false)
	{
		Console.BackgroundColor = color;
		if (!write)
			Console.WriteLine(msg);
		else
			Console.Write(msg);

		Console.ResetColor();
	}

	private readonly string ID = id;
}

