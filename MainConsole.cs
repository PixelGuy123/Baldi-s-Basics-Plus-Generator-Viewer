using BBP_Gen.Misc;
using BBP_Gen.PlusGenerator;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace BBP_Gen.Main;

public class MainConsole // Program
{
	const string version = "v1.0.2";
	public static void Main()
	{
		Console.WindowWidth += 20; // Idk how to change the console width by default, so I'll manually do it
		LoadSettings();

		while (true)
		{
			Console.WriteLine($"Welcome to the Baldi\'s Basics Plus Generator Viewer! {version} - Made by PixelGuy");
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
				GC.Collect();
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
			Console.WriteLine("Please, type the Floor you wanna begin on (0 - END, 1 - F1, 2 - F2, 3 - F3)");

			(LevelObject, int) obj;

			if (int.TryParse(Console.ReadLine(), out int s) && s >= 0 && s <= 3)
			{
				obj = s == 0 ? LdStorage.END : s == 1 ? LdStorage.Floor1 : s == 2 ? LdStorage.Floor2 : LdStorage.Floor3;
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

					var token = gen.BeginGeneration();
					w.Stop();
					token.Data?.WriteEverythingOnLine();
					gen.DisplayGrid();
				}
				catch (Exception e)
				{
					w.Stop();
					Console.BackgroundColor = ConsoleColor.Red;
					Console.WriteLine("Seed has crashed, type of crash: " + e.Message);
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

	static void HybridSeedFinder()
	{
		var time = DateTime.Now;
		var dirName = $"HybridDumpFolder-{time.Month}-{time.Day}-{time.Year}_{time.Hour}-{time.Minute}-{time.Second}";

		void Generate(int s, string path, bool allowmirror = true)
		{
			try
			{
				SeedToken[] tokens =
				[
					new Generator(s, LdStorage.Floor1.Item2, LdStorage.Floor1.Item1).BeginGeneration(true, "F1"), // Generate all three floors and conclude the experiment :)
					new Generator(s, LdStorage.Floor2.Item2, LdStorage.Floor2.Item1).BeginGeneration(true, "F2"),
					new Generator(s, LdStorage.Floor3.Item2, LdStorage.Floor3.Item1).BeginGeneration(true, "F3")
				];

				bool[] flags = [tokens[0].Type.HasFlag(SeedType.Glitched), tokens[1].Type.HasFlag(SeedType.Glitched), tokens[2].Type.HasFlag(SeedType.Glitched)]; // Yeah, just so it doesn't run this HasFlag twice (or more)

				if (flags.Count(x => x) <= 1)
					return; // Needs at least 2 glitched floors to be hybrid

				StringBuilder ntbs = new();

				StringBuilder sb = new();
				sb.Append($"{s}\t"); // First comes the seed

				if (flags[0])
				{
					sb.Append("F1"); // A little weird way to do it, but it does work
					ntbs.Append($"0/{tokens[0].AmountOfNotebooks}");
					if (flags[1])
					{
						sb.Append(" & F2");
						ntbs.Append($" & 0/{tokens[1].AmountOfNotebooks}");
					}
					if (flags[2])
					{
						sb.Append(" & F3");
						ntbs.Append($" & 0/{tokens[2].AmountOfNotebooks}");
					}
				}
				else if (flags[1])
				{
					sb.Append("F2");
					ntbs.Append($" & 0/{tokens[1].AmountOfNotebooks}");
					if (flags[2])
					{
						sb.Append(" & F3");
						ntbs.Append($" & 0/{tokens[2].AmountOfNotebooks}");
					}
				}

				sb.Append('\t' + ntbs.ToString() + $"\t::::\t{time.Month}/{time.Day}/{time.Year}\t{(tokens.Any(x => x.IsAPR) ? "(APR)" : string.Empty)}"); // notebooks stuff

				Log(path, sb.ToString());

				if (settInstance.Mirror_Check && allowmirror)
				{
					if (flags[0])
						Generate(-s, path, false);
					if (flags[1])
						Generate(s.Mirror(LdStorage.Floor2.Item2), path, false);
					if (flags[2])
						Generate(s.Mirror(LdStorage.Floor3.Item2), path, false);
				}
			}
			catch (SeedCrashException) { } // Ignore seedcrashexceptions, this method is focused for glitched seeds anyways
			catch (Exception e)
			{
				Console.WriteLine();
				Console.WriteLine($"Undentified error found on seed: {s}");
				Console.WriteLine(e);
				Console.WriteLine();
			}
		}

		while (true) // Actual method here
		{
			Console.Clear();
			Console.WriteLine("This is the Hybrid Seed Dumper, all it does is iterate through a huge txt file filled with glitched seeds to search for hybrid ones");
			Console.WriteLine("You can use the seeds available from the official glitched seed guide (that huge text document), it supports that format aswell");
			bool fileExists = File.Exists(settInstance.HybridTextFilePath) && Path.GetExtension(settInstance.HybridTextFilePath) == ".txt";

			if (fileExists)
				Console.WriteLine("Do you want to change the path? (y/n)"); // If exists, it'll ask you this, otherwise it'll skip this and the Console.ReadLine()

			if (!fileExists || Console.ReadLine()?.ToLower() == "y")
			{
				Console.WriteLine("Please type the path to the file that the hybrid check will iterate through (or \'l\' to leave):");
				string? path = Console.ReadLine();
				if (path?.ToLower() == "l") // Leave
					break;

				if (File.Exists(path) && Path.GetExtension(path) == ".txt") // Only .txt files
				{
					settInstance.HybridTextFilePath = path;
					SaveSettings();
				}
				else
				{
					Console.WriteLine("Invalid path or file, please try again...");
					Console.ReadKey();
					continue;
				}
			}

			Console.WriteLine("Current path chosen: " + Path.GetFullPath(settInstance.HybridTextFilePath));
			Console.WriteLine("Should the dumper start? (y/n)");

			if (Console.ReadLine()?.ToLower() != "y")
				break;

			string[] lines = File.ReadAllLines(settInstance.HybridTextFilePath);

			Console.Clear();
			Console.WriteLine("Queueing seeds to be generated...");

			if (lines.Length == 0)
			{
				Console.WriteLine("The txt file appears to be empty, cancelling operation...");
				Console.ReadKey();
				break;
			}

			System.Collections.Concurrent.ConcurrentQueue<int> seeds = new(); // ConcurrentQueue because it is thread-safe
			List<string> ignoredSeeds = [];

			for (int i = 0; i < lines.Length; i++)
			{
				if (int.TryParse(lines[i].Split('\t')[0], out int r))
					seeds.Enqueue(r);

				else
					ignoredSeeds.Add($"Failed to get the seed from line {i}: {lines[i]}");

				Console.Write($"\rProgress: {Math.Ceiling((float)i / lines.Length * 100f)}%\t");
			}



			CreateNewDumpFolder(dirName);

			string[] paths =
				[
					Path.Combine(defaultDumpDirectoryName, dirName, glitchedSeedFileName),
					Path.Combine(defaultDumpDirectoryName, dirName, logsFilename),
				];

			if (ignoredSeeds.Count > 0)
				File.WriteAllLines(paths[1], ignoredSeeds); // Write the ignored seeds here

			Console.WriteLine();

			if (seeds.IsEmpty)
			{
				Console.WriteLine("No seeds were queued, cancelling operation...");
				Console.ReadKey();
				break;
			}

			Console.Clear();
			Console.Write($"\rAmount of seeds left: {seeds.Count} - Updates each {seedLogOffset} seeds");

			for (int i = 0; i < settInstance.AmountOfThreads - 1; i++) // Side threads to read the queue
			{
				Thread t = new(() =>
				{
					while (!seeds.IsEmpty)
					{
						if (seeds.TryDequeue(out int r))
						{
							Generate(r, paths[0]);

						}
					}
				});

				t.Start();
			}

			
			int s = 0;

            while (!seeds.IsEmpty) // main thread doing the job aswell
			{
				if (seeds.TryDequeue(out int r))
				{
					s += settInstance.AmountOfThreads;
					Generate(r, paths[0]);

					if (s >= seedLogOffset)
					{
						s = 0;
						Console.Write($"\rAmount of seeds left: {seeds.Count} - Last Seed: {r} - Updates each {seedLogOffset} seeds");
					}
				}
			}

			Console.WriteLine();

            Console.WriteLine("Iterated all seeds, closing dumper...");
			Console.ReadKey();
			break;


        }


	}

	static void GlitchedSeedFinder()
	{

		var time = DateTime.Now;
		var dirName = $"dumpFolder-{time.Month}-{time.Day}-{time.Year}_{time.Hour}-{time.Minute}-{time.Second}";

		void Generate(int s, (LevelObject, int) obj, string floor, string[] paths, bool? mirrorParameter = null) // Local function
		{

			try
			{
				var token = new Generator(s, obj.Item2, obj.Item1).BeginGeneration(true, floor, mirrorParameter);


				if (token.Type.HasFlag(SeedType.Glitched))
					Log(paths[0], $"{s}\t{floor}\t::::\t0/{token.AmountOfNotebooks}\t{time.Month}/{time.Day}/{time.Year}{(token.IsAPR && floor != "F1" ? "\t(APR)" : string.Empty)}"); // :::: is for name btw...

				// Only runs if glitchedSeeds is null (meaning, no hybrid checks)
				else if (mirrorParameter is null && settInstance.Mirror_Check && obj.Item2 != 0) // Only check the mirror seeds if the current seed is normal
					Generate(s.Mirror(obj.Item2), obj, floor, paths, token.HasMirrorFeatures); // Calls itself again but with the parameters (mirroring the seed)






				StringBuilder sb = new();
				if (token.Data is not null)
				{
					for (int i = 0; i < token.Data.Length; i++)
						sb.Append($"{token.Data[i]}{(i < token.Data.Length - 1 ? " | " : string.Empty)}");

				}

				if (token.Type.HasFlag(SeedType.OOB))
					Log(paths[1], $"{s}\t{floor}\t::::\t{sb}\t{time.Month}/{time.Day}/{time.Year}");

				if (token.Type.HasFlag(SeedType.Uncommon))
					Log(paths[3], $"{s}\t{floor}\t::::\t{time.Month}/{time.Day}/{time.Year}\t{sb}");


			}
			catch (SeedCrashException e)
			{
				Log(paths[2], $"{s}\t{floor}\t::::\t{e.Message}\t{time.Month}/{time.Day}/{time.Year}");
			}
			catch (Exception e)
			{
				Console.WriteLine();
				Console.WriteLine($"Undentified error found on seed: {s}");
				Console.WriteLine(e);
			}
		}
		// ------ Actual method below here ------

		while (true)
		{
			Console.Clear();
			Console.WriteLine("Please, type the Floor you wanna begin on (0 - END, 1 - F1, 2 - F2, 3 - F3) // Type \'l\' to leave");
			Console.WriteLine("Upon running this tool, you'll only be able to close it from the window or if you manage to reach the maximum integer value");
			Console.WriteLine($"Don\'t worry, all seed types are stored on dumps localized on: {Path.Combine(defaultDumpDirectoryName, dirName)}");
			Console.WriteLine("When a seed is found, it\'ll dump the seed into th designed file (the folder will begin empty, but will gradually fill with the files)");
			Console.WriteLine("Enter floor number here: ");

			(LevelObject, int) obj;
			var str = Console.ReadLine();
			string floor;

			if (int.TryParse(str, out int s) && s >= 0 && s <= 3)
			{
				obj = s == 0 ? LdStorage.END : s == 1 ? LdStorage.Floor1 : s == 2 ? LdStorage.Floor2 : LdStorage.Floor3;
				floor = s == 0 ? "END" : $"F{s}";
			}
			else if (str?.ToLower() == "l")
				break; // Literally stops the loop
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


				// Create three files inside them (with using blocks, so they are disposed after being created)

				CreateNewDumpFolder(dirName);

				string[] paths =
					[
						Path.Combine(defaultDumpDirectoryName, dirName, glitchedSeedFileName),
						Path.Combine(defaultDumpDirectoryName, dirName, oobSeedFileName),
						Path.Combine(defaultDumpDirectoryName, dirName, crashSeedFileName),
						Path.Combine(defaultDumpDirectoryName, dirName, uncommonSeedFileName)
					];
				uint numToLog = seedLogOffset;

				for (int i = 0; i < settInstance.AmountOfThreads - 1; i++) // Begin the side-threads
				{
					var t = new Thread((x) => // Creates a new thread
					{
						if (x is not int)
							return;

						int offset = (int)x + 1;

						for (int seed = s + offset; seed < int.MaxValue; seed += settInstance.AmountOfThreads) // += settInstance.AmountOfThreads
							Generate(seed, obj, floor, paths);


					});
					t.Start(i);
				}
				// Main Thread Now
				for (; s < int.MaxValue; s += settInstance.AmountOfThreads) // += settInstance.AmountOfThreads
				{

					Generate(s, obj, floor, paths);


					numToLog += (uint)settInstance.AmountOfThreads;
					if (numToLog >= seedLogOffset)
					{
						Console.Write("\rCurrent seed: {0} (Updates each {1} seeds)", s, seedLogOffset);
						numToLog = 0u;
						//GC.Collect();
					}
				}





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

	static void Setting()
	{
		while (true)
		{
			Console.Clear();

			Console.WriteLine("Enter a number to flip one of the switches available");

			Console.WriteLine($"[1] - Dumper Thread Number: {settInstance.AmountOfThreads}\n[2] - Disallow 1 door to bigroom logging on F3: {settInstance.Disallow_1DoorAtBigroom_InF3}\n" +
				$"[3] - Mirror Check: {settInstance.Mirror_Check}\n[4] - UserName: {settInstance.Username}\n[0] - Exit");

			Console.WriteLine("\nType a number here:");
			if (int.TryParse(Console.ReadLine(), out int num))
			{
				switch (num)
				{
					case 1:
						Console.WriteLine($"Set a new number of threads for the dumper (limit: 1 - {Environment.ProcessorCount})");
						if (int.TryParse(Console.ReadLine(), out num))
							settInstance.AmountOfThreads = num;
						break;

					case 0:
						return; // Leaves

					case 2:
						Console.WriteLine("Should 1 door to bigroom logging be disabled on F3? (y/n)");
						settInstance.Disallow_1DoorAtBigroom_InF3 = Console.ReadLine()?.ToLower() == "y";

						break;

					case 3:
						Console.WriteLine("Toggle Mirror Check? (y/n)");
						settInstance.Mirror_Check = Console.ReadLine()?.ToLower() == "y";
						break;

					case 4:
						Console.WriteLine("What name you wanna put for the dumps generated?");
						string? name = Console.ReadLine();
						settInstance.Username = !string.IsNullOrWhiteSpace(name) ? name : defaultUserName;
						break;

					default:
						break; // Does nothing
				}

				if (num > 0)
					SaveSettings(); // Will happen anyway
			}
		}
	}

	// ****** Below here are methods that aren't accessed normally by the user ******

	static void Log(string path, string log, bool beginWithUserName = true)
	{

		lock (lockObj)
		{
			try
			{
				bool beginWname = beginWithUserName && !File.Exists(path);

				using StreamWriter w = new(path, true);

				if (beginWname)
					w.WriteLine("##name=" + settInstance.Username);

				w.WriteLine(log);
			}
			catch(Exception e)
			{
				Console.WriteLine();
				Console.WriteLine("Failed to log due to: " + e.Message);
				Console.WriteLine();
			}
		}
	}

	static void CreateNewDumpFolder(string dirName)
	{
		if (!Directory.Exists(defaultDumpDirectoryName))
			Directory.CreateDirectory(defaultDumpDirectoryName);

		var dirs = Directory.GetDirectories(defaultDumpDirectoryName);
		while (dirs.Length >= dumpLimit) // Delete every single one.
		{
			Directory.Delete(dirs.OrderBy(Directory.GetCreationTime).First(), true);
			dirs = Directory.GetDirectories(defaultDumpDirectoryName);
		}

		// Create this folder for the current timespan
		Directory.CreateDirectory(Path.Combine(defaultDumpDirectoryName, dirName));
	}

	static void LoadSettings()
	{

		try
		{
			if (!File.Exists(defaultConfigPath))
				File.WriteAllText(defaultConfigPath, JsonConvert.SerializeObject(settInstance));

			var sets = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(defaultConfigPath));
			if (sets is not null)
				settInstance = sets;

			SaveSettings();
		}
		catch
		{
			Console.WriteLine("Failed to load settings probably due to invalid syntax or corruption, using default settings...\n");
			SaveSettings();
			Console.WriteLine("Press any key to initialize the tool...");

			Console.ReadKey();
			Console.Clear();
		}

	}

	static void SaveSettings()
	{
		try
		{
			File.WriteAllText(defaultConfigPath, JsonConvert.SerializeObject(settInstance)); // Nice
		}
		catch (Exception e)
		{
			Console.WriteLine(e.Message);
			Console.WriteLine("Failed to save settings, please try again...\n");
			Console.WriteLine("Press any key to continue...");

			Console.ReadKey();
			Console.Clear();
		}
	}



	// ********* Fields **********

	static readonly string[] descriptions = // Description of the options
	[
		"Visualize Seed - You can visualize a seed through this tool (a basic map drawn over the console)",
		"Seed Dumper - It iterate through all seeds on the game and dumps them in different files/categories",
		"Hybrid Seed Dumper - It iterate through all glitched seeds inside a txt file (provided by you) to find any hybrids",
		"Settings - Seed Dumper Settings"
	];

	static readonly Dictionary<int, Action> options = new() // The options themselves
	{
		{1, Visualizer },
		{2, GlitchedSeedFinder },
		{3, HybridSeedFinder},
		{4, Setting}
	};

	static Settings settInstance = new();

	public static Settings AllSettings => settInstance;

	public sealed class Settings()
	{
		private int _amountOfThreads = 1;
		public int AmountOfThreads { get => _amountOfThreads; set => _amountOfThreads = Math.Clamp(value, 1, Environment.ProcessorCount); }
		public bool Disallow_1DoorAtBigroom_InF3 { get; set; } = false;
		public bool Mirror_Check { get; set; } = false;
		public string HybridTextFilePath { get; set; } = string.Empty;
		public string Username { get; set; } = defaultUserName;
	}

	const string defaultConfigPath = "settings.json", defaultSettingsSectionName = "Settings", defaultDumpDirectoryName = "dumps",
		glitchedSeedFileName = "GlitchedSeeds.txt", oobSeedFileName = "OutOfBoundsSeeds.txt", crashSeedFileName = "CrashSeeds.txt", uncommonSeedFileName = "UncommonSeeds.txt", logsFilename = "Log.txt",
		defaultUserName = "Player";

	const int seedLogOffset = 500, dumpLimit = 5;

	public readonly static object lockObj = new();
}

internal static class IntegerEx // For specific stuff
{
	internal static int Mirror(this int s, int offset) =>
		-(s + (offset * 2));
}