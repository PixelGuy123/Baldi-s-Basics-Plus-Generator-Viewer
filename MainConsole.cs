using BBP_Gen.Misc;
using BBP_Gen.PlusGenerator;
using System.Diagnostics;
using Newtonsoft.Json;

namespace BBP_Gen.Main;

public class MainConsole // Program
{
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



	public static void Main()
	{
		LoadSettings();

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

		static void Log(string path, string log)
		{
			using StreamWriter w = new(path, true);
			w.WriteLine(log);
		}

		var time = DateTime.Now;
		var dirName = $"dumpFolder-{time.Month}-{time.Day}-{time.Year}_{time.Hour}-{time.Minute}-{time.Second}";


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
			{
				break; // Literally stops the loop
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
				/*	bool finished = false; Will leave unused for a while.. if I decide to re-use it again
					while (!finished)
					{
						List<Task> tasks = [];

						for (int i = 0; i < limits; i++) {

							tasks.Add(Task.Run(() =>
							{
								var gen = new Generator(s++, obj.Item2, obj.Item1);
								try
								{

									(bool found, var list) = gen.BeginGeneration(true);

									if (found)
									{
										Console.WriteLine();
										list.WriteEverythingOnLine();
										gen.DisplayGrid();
										finished = true;
										return;
									}
								}
								catch { } // Yup, ignore exceptions
								Console.Write("\rCurrent Seed: {0} \t", s);

							}));
						}

						Task.WaitAll([.. tasks]);

						tasks.Clear();
					}
					*/

				// Create three files inside them (with using blocks, so they are disposed after being created)

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

				string[] paths =
					[
						Path.Combine(defaultDumpDirectoryName, dirName, glitchedSeedFileName),
						Path.Combine(defaultDumpDirectoryName, dirName, oobSeedFileName),
						Path.Combine(defaultDumpDirectoryName, dirName, crashSeedFileName),
						Path.Combine(defaultDumpDirectoryName, dirName, uncommonSeedFileName)
					];
				uint numToLog = seedLogOffset;

				for (;s < int.MaxValue; s+= settInstance.AmountOfThreads)
				{
					Parallel.For(s, s + settInstance.AmountOfThreads, x =>
					{
						var gen = new Generator(x, obj.Item2, obj.Item1);
						try
						{

							var token = gen.BeginGeneration(true, floor);


							if (token.Type.HasFlag(SeedType.Glitched))
								Log(paths[0], $"{x}\t{floor}\t::::\t0/{token.AmountOfNotebooks}\t{time.Month}/{time.Day}/{time.Year}{(token.IsAPR ? "\t(APR)" : string.Empty)}"); // :::: is for name btw...
							
							
							

							System.Text.StringBuilder sb = new();
							if (token.Data is not null)
							{
								for (int i = 0; i < token.Data.Length; i++)
									sb.Append($"{token.Data[i]}{(i < token.Data.Length - 1 ? '/' : string.Empty)}");
								
							}

							if (token.Type.HasFlag(SeedType.OOB))
								Log(paths[1], $"{x}\t{floor}\t::::\t{sb}\t{time.Month}/{time.Day}/{time.Year}");

							if (token.Type.HasFlag(SeedType.Uncommon))
								Log(paths[3], $"{x}\t{floor}\t::::\t{time.Month}/{time.Day}/{time.Year}\t{sb}");
								
						}
						catch (SeedCrashException e)
						{
							Log(paths[2], $"{x}\t{floor}\t::::\t{e.GetType()}\t{time.Month}/{time.Day}/{time.Year}");
						}
						catch (Exception e)
						{
							Console.WriteLine();
							Console.WriteLine($"Undentified error found on seed: {x}");
							Console.WriteLine(e);
                        }
						
					});
					numToLog += (uint)settInstance.AmountOfThreads;
					if (numToLog >= seedLogOffset)
					{
						Console.Write("\rCurrent seed: {0} (Updates each {1} seeds)", s, seedLogOffset);
						numToLog = 0u;
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

			Console.WriteLine($"[1] - Dumper Thread Number: {settInstance.AmountOfThreads}\n[2] - Disallow 1 door to bigroom logging on F3: {settInstance.Disallow_1DoorAtBigroom_InF3}\n[0] - Exit");

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
						Console.WriteLine($"Should 1 door to bigroom logging be disabled on F3? (y/n)");
						settInstance.Disallow_1DoorAtBigroom_InF3 = Console.ReadLine()?.ToLower() == "y";

						break;
                        
                    default:
						break; // Does nothing
				}

				if (num > 0)
					SaveSettings(); // Will happen anyway
			}
		}
	}

	static readonly string[] descriptions =
	[
		"Visualize Seed - You can visualize a seed through this tool (a basic map drawn over the console)",
		"Seed Dumper - It iterate through all seeds on the game and dumps them in different files/categories",
		"Settings - Seed Dumper Settings"
	];

	static readonly Dictionary<int, Action> options = new()
	{
		{1, Visualizer },
		{2, GlitchedSeedFinder },
		{3, Setting}
	};

	static Settings settInstance = new();

	public static Settings AllSettings => settInstance;

	public sealed class Settings()
	{
		private int _amountOfThreads = 1;
		public int AmountOfThreads { get => _amountOfThreads; set => _amountOfThreads = Math.Clamp(value, 1, Environment.ProcessorCount); }

		public bool Disallow_1DoorAtBigroom_InF3 { get; set; } = false;
	}

	const string defaultConfigPath = "settings.json", defaultSettingsSectionName = "Settings", defaultDumpDirectoryName = "dumps",
		glitchedSeedFileName = "GlitchedSeeds.txt", oobSeedFileName = "OutOfBoundsSeeds.txt", crashSeedFileName = "CrashSeeds.txt", uncommonSeedFileName = "UncommonSeeds.txt";

	const int seedLogOffset = 500, dumpLimit = 5;
}