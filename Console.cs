using System;
using BBP_Gen.Elements;
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

internal static class LdStorage
{
	public readonly static LevelObject Floor1 = new(
		new MinMax<IntVector2>(new IntVector2(18, 23), new IntVector2(25, 30)),
		5,
		new MinMax<int>(3, 5),
		new MinMax<int>(0, 2),
		new MinMax<int>(1, 2),
		new MinMax<int>(1, 1),
		false,
		new MinMax<int>(1, 1),
		true,
		3,
		new MinMax<IntVector2>(new IntVector2(4, 5), new IntVector2(6, 7)),
		25f,
		4f,
		[new WeightedSelection<RandomEvent>(new GenericEvent("Fog Event"), 100), new WeightedSelection<RandomEvent>(new PartyEvent(), 50)],
		[new(new(new(new(10, 10), new(15, 15)), "Cafeteria"), 100), new(new(new(new(12, 12),new(18,18)),"Playground"), 100)] // Lots of news lmfao
		);
}