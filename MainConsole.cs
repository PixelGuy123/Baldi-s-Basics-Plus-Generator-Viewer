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
			Console.WriteLine("Welcome to the Baldi\'s Basics Plus Generator Viewer!");
			Console.WriteLine("On this tool, you\'ll be able to visualize a seed through the console!");
			Console.WriteLine("It provides various helpful tools to check a seed, iterate through hundreds of seeds until find one and etc.");
			Console.WriteLine("Please type the number to select an option from this menu");
			for (int i = 0; i < descriptions.Length; i++)
			{
				Console.WriteLine($"[{i + 1}] -- {descriptions[i]}");
			}

			Console.WriteLine();
			Console.WriteLine("Type the number here: ");
			if (int.TryParse(Console.ReadLine(), out int res) && options.TryGetValue(res, out Action? value))
			{
				Console.Clear();
				value();
			}
			else
			{
				Console.WriteLine("Invalid number, please try again...");
				Console.ReadKey();
			}
			Console.Clear();
		}

	}


	static void Visualizer()
	{
		while (true)
		{
			Console.Clear();
            Console.WriteLine("Please, type the Floor you wanna begin on (0 - END (not available), 1 - F1, 2 - F2 (not available), 3 - F3)");

			(LevelObject, int) obj;

			if (int.TryParse(Console.ReadLine(), out int s) && s >= 0 && s <= 3)
			{
				obj = s == 1 ? LdStorage.Floor1 : LdStorage.Floor3;
			}
			else
			{
				Console.WriteLine("Invalid floor number, please try again...");
				Console.ReadKey();
				continue;
			}

			s = new Random().Next();

			Console.WriteLine("Please, type the seed you want to visualize (use \'r\' for a random seed): ");
			string? response = Console.ReadLine();
			if (response?.ToLower() == "r" || int.TryParse(response, out s))
			{
				Stopwatch w = new();
				w.Start();
				var gen = new Generator(s, obj.Item2, obj.Item1);
				try
				{
					
					(_, var list) = gen.BeginGeneration();
					w.Stop();
					list.WriteEverythingOnLine();
					gen.DisplayGrid();
				}
				catch (Exception e)
				{
					w.Stop();
					Console.BackgroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
					Console.ResetColor();

                    Console.WriteLine("Still wanna render the seed map? (y/n)");
					if (Console.ReadLine()?.ToLower() == "y")
						gen.DisplayGrid();
				}
				finally
				{
					Console.WriteLine($"Time to generate seed: {w.ElapsedMilliseconds}ms");
				}

                Console.WriteLine();
                Console.WriteLine("Do you want to generate another seed? (y/n)");

				if (Console.ReadLine()?.ToLower() == "y")
					continue;

                break;
			}
			else
			{
                Console.WriteLine("Invalid seed number, please try again...");
				Console.ReadKey();
            }
		}
	}

	static void GlitchedSeedFinder()
	{
		while (true)
		{
			Console.Clear();
			Console.WriteLine("Please, type the Floor you wanna begin on (0 - END (not available), 1 - F1, 2 - F2 (not available), 3 - F3)");

			(LevelObject, int) obj;

			if (int.TryParse(Console.ReadLine(), out int s) && s >= 0 && s <= 3)
			{
				obj = s == 1 ? LdStorage.Floor1 : LdStorage.Floor3;
			}
			else
			{
				Console.WriteLine("Invalid floor number, please try again...");
				Console.ReadKey();
				continue;
			}

			s = new Random().Next();

			Console.WriteLine("Please, type the seed you want to start the search (use \'r\' for a random seed): ");
			string? response = Console.ReadLine();
			if (response?.ToLower() == "r" || int.TryParse(response, out s))
			{
                Console.Clear();

				Stopwatch w = new();
				w.Start();
				while (true)
				{
					Stopwatch bw = new();
					bw.Start();
					var gen = new Generator(s++, obj.Item2, obj.Item1);
					try
					{

						(bool found, var list) = gen.BeginGeneration(true);

						if (found)
						{
							list.WriteEverythingOnLine();
							gen.DisplayGrid();
							bw.Stop();
							break;
						}
					}
					catch { } // Yup, ignore exceptions

					bw.Stop();
					Console.Write("\rCurrent Seed: {0} - Time: {1}ms \t", s, bw.ElapsedMilliseconds);
				}

				w.Stop();
				Console.WriteLine($"Time to find seed: {w.ElapsedMilliseconds}ms");

				Console.WriteLine();
				Console.WriteLine("Do you want to do another search? (y/n)");

				if (Console.ReadLine()?.ToLower() == "y")
					continue;

				break;
			}
			else
			{
				Console.WriteLine("Invalid seed number, please try again...");
				Console.ReadKey();
			}
		}
	}

	static readonly string[] descriptions =
	[
		"Visualize Seed - You can visualize a seed through this tool (a basic map drawn over the console)",
		"Glitched Seed Searcher - It iterate through all seed until it finds 1 glitched seed"
	];

	static readonly Dictionary<int, Action> options = new()
	{
		{1, Visualizer },
		{2, GlitchedSeedFinder }
	};
}