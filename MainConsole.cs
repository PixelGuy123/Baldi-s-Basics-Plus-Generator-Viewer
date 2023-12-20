using BBP_Gen.Misc;
using BBP_Gen.PlusGenerator;

namespace BBP_Gen.Main;

public class MainConsole // Program
{
	public static void Main()
	{
		while (true)
		{
			Console.WriteLine("Initializing Generator");
			Console.WriteLine("(default is 0) Set a seed to begin the process: ");
			int seed = 0;

			if (int.TryParse(Console.ReadLine(), out int r))
				seed = r;

			new Generator(seed, 0, LdStorage.Floor1).BeginGeneration(true);

			Console.WriteLine();
			Console.WriteLine("DONE !! -- Press anything to restart");

			Console.ReadKey();
			Console.Clear();

			GC.Collect();
		}
	}
}