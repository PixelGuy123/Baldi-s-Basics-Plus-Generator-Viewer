using BBP_Gen.Misc;
using BBP_Gen.PlusGenerator;
using System.Diagnostics;

namespace BBP_Gen.Main;

public class MainConsole // Program
{
	public static void Main()
	{
		while (true)
		{
			Console.WriteLine("Initializing Generator");
			Console.WriteLine("(default is 0) Set a seed to begin the process: ");
			int seed = new Random().Next();

			if (int.TryParse(Console.ReadLine(), out int r))
				seed = r;

			Stopwatch w = new();
			w.Start();

			/*for (int i = seed; i < seed + 100; i++)
			{

				new Generator(i, 0, LdStorage.Floor1).BeginGeneration(false);
			}*/

			//var gen = new Generator(seed, 0, LdStorage.Floor1);
			var gen = new Generator(seed, 2, LdStorage.Floor3);

			gen.BeginGeneration(false);

			w.Stop();
			gen.DisplayGrid();

            Console.WriteLine(w.ElapsedMilliseconds + "ms");


            Console.WriteLine();
			Console.WriteLine("DONE !! -- Press anything to restart");

			Console.ReadKey();
			Console.Clear();

            GC.Collect();
		}
	}
}