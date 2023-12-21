using BBP_Gen.Misc;
using BBP_Gen.PlusGenerator;
using System.Diagnostics;

namespace BBP_Gen.Main;

public class MainConsole // Program
{
	public static void Main()
	{

		/*for (int i = seed; i < seed + 100; i++)
		{

			new Generator(i, 0, LdStorage.Floor1).BeginGeneration(false);
		}*/

		bool setFloor = true;
		int floor = 1;

		int r;

		while (true)
		{
			if (setFloor) 
			{
				Console.WriteLine("Type a floor you wanna begin with: ");
				if (int.TryParse(Console.ReadLine(), out r))
					floor = r;
			}

			Console.WriteLine("(default is random) Set a seed to begin the process: ");
			int seed = new Random().Next();

			if (int.TryParse(Console.ReadLine(), out r))
				seed = r;

			Stopwatch w = new();
			w.Start();

			Generator gen;

			if (floor == 1)
				gen = new Generator(seed, 0, LdStorage.Floor1);

			else
			{
				floor = 3;
				gen = new Generator(seed, 2, LdStorage.Floor3);
			}

			Console.WriteLine($"Initializing Generator with floor: F{floor}");

			try
			{
				gen.BeginGeneration(false);
				gen.DisplayGrid();

				Console.WriteLine();
				Console.WriteLine("DONE !! -- Press anything to restart");
			}
			catch (SeedCrashException e)
			{
				w.Stop();

				Console.WriteLine(e.Message);
				Console.WriteLine("Do you wanna still see the map generated? (y/n)");
				if (Console.ReadLine()?.ToLower() == "y")
					gen.DisplayGrid();
            }
			finally
			{
				w.Stop();
				Console.WriteLine(w.ElapsedMilliseconds + "ms");
			}


            Console.WriteLine("Press R to re-set the floor number");
            setFloor = Console.ReadKey().Key == ConsoleKey.R;
			Console.Clear();

            GC.Collect();
		}
	}
}