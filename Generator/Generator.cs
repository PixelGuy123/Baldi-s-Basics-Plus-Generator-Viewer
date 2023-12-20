using BBP_Gen.Elements;
using BBP_Gen.Misc;

namespace BBP_Gen.PlusGenerator;

public partial class Generator // Makes it easier to make an instance of it. Main Class here for the generator
{
    public Generator(int seed, int seedOffset, LevelObject levelObj) =>
        (_seed, _seedOffset, ld) = (seed, seedOffset, levelObj);

    public void BeginGeneration(bool allowLogging = false)
    {
        if (!canGenerateAgain)
        {
            Logger.Log("This generator has already generated the seed, in order to use another level, please create another instance with a different LevelObject");
            return;
        }

        if (allowLogging)
          Logger.Log("Initializing with Seed: " + Seed);
        

        _controlledRNG = new(Seed); // for npc stuff

        // --NPC STUFF (later)--



        // --End of npc stuff--



        _controlledRNG = new(Seed + _seedOffset);

        levelSize = new(_controlledRNG.Next(ld.LevelSizes.Min.x, ld.LevelSizes.Max.x + 1) + ld.OuterEdgeBuffer * 2, _controlledRNG.Next(ld.LevelSizes.Min.z, ld.LevelSizes.Max.z + 1) + ld.OuterEdgeBuffer * 2); // Creates level size

        if (_controlledRNG.Next(0, 2) == 1) (levelSize.z, levelSize.x) = (levelSize.x, levelSize.z); // Change Values

		if (allowLogging)
			Logger.Log($"current level size is: {levelSize.x},{levelSize.z}");

        mapTiles = new RoomType[levelSize.x, levelSize.z];
		buffer = new bool[levelSize.x, levelSize.z];
		
		/*for (int i = 0; i < mapTiles.GetLength(0); i++) Default for this should be 0
        {
            for (int j = 0; j < mapTiles.GetLength(1); j++)
            {
                mapTiles[i, j] = -1;
            }
        }
		*/

		int plotCount = _controlledRNG.Next(ld.PlotCount.Min, ld.PlotCount.Max + 1);
        int hallsToRemove = _controlledRNG.Next(ld.HallRemovalCount.Min, ld.HallRemovalCount.Max + 1);
        _controlledRNG.Next(); // Skip one value (SideHallsToRemove... idk what's that even for)
        int hallsToAdd = _controlledRNG.Next(ld.HallsAddCount.Min, ld.HallsAddCount.Max + 1);
        int eventCount = _controlledRNG.Next(ld.EventCount.Min, ld.EventCount.Max);

        // --Event Stuff Here--

        var events = new List<WeightedSelection<RandomEvent>>(ld.RandomEvents);
        var eventsToLaunch = new List<RandomEvent>();



        int h = 0;
        while (h++ < eventCount && events.Count > 0)
        {
            var rEvent = WeightedSelection<RandomEvent>.ControlledRandomSelection(_controlledRNG, [.. events]);
            eventsToLaunch.Add(rEvent);
            events.RemoveAll(x => x.selection == rEvent);
        }

        foreach (var e in eventsToLaunch)
        {
            e.Initialize(_controlledRNG);
            _controlledRNG.NextDouble();
			if (allowLogging)
				Logger.Log($"Event Available: {e.Name}");
        }


        // --Event Stuff Gone--

        Internal_SkipRNGVals(22); // Skip all that material choose thing

        // --Field Trip first operation--



        // --Field trip done--

        int specialRoomCount = _controlledRNG.Next(ld.SpecialRoomCount.Min, ld.SpecialRoomCount.Max + 1);

        // Here comes buffer stuff

        // =========== Note ===========
        // Buffer tiles will have 99 as id by default!!

        int northEdgeBuffer = ld.OuterEdgeBuffer;
        int eastEdgeBuffer = ld.OuterEdgeBuffer;
        int southEdgeBuffer = ld.OuterEdgeBuffer;
        int westEdgeBuffer = ld.OuterEdgeBuffer;

		// --Another field trip operation over buffer tiles--



		// --done--

		List<IntVector2> outerBuffers = [];

        // Special Room Stuff begins here

        for (int i = 0; i < levelSize.x; i++) // This is to create an border, will be useful for special rooms and plots
        {
            for (int j = 0; j < levelSize.z; j++)
            {
				if (i < westEdgeBuffer || levelSize.x - i <= eastEdgeBuffer || j < southEdgeBuffer || levelSize.z - j <= northEdgeBuffer)
				{
					mapTiles[i, j] = (ld.IncludeBuffer ? RoomType.Room : RoomType.Buffer) | RoomType.Border; // Sets as a buffer tile
					outerBuffers.Add(new(i, j));
				}

            }
        }


        List<SpecialRoomCreator> specialRoomsToExpand = []; // Create special rooms

        for (int i = 0; i < specialRoomCount; i++)
        {
            var chosen = WeightedSelection<SpecialRoomCreator>.ControlledRandomSelection(_controlledRNG, ld.SpecialRooms);
            var specialRoom = chosen;
            UpdatePotentialRoomSpawns(true);
            specialRoom.SetRandomValues(_controlledRNG);
            specialRoom.SetReferences(this);
            specialRoom.Initialize();
            if (allowLogging)
                Logger.Log($"Special Room Data: {specialRoom.Name} with size: {specialRoom.MaxSize} on Pos: {specialRoom.Pos}");


            specialRoomsToExpand.Add(specialRoom);
        }

		ExpansionIterator(0, out var specialRooms,[.. specialRoomsToExpand]);
		specialRoomsToExpand = new List<SpecialRoomCreator>(specialRooms.Cast<SpecialRoomCreator>());

       /* Should be unused
        while (specialRoomsToExpand.Count > 0) // Expand them
        {
            for (int i = 0; i < specialRoomsToExpand.Count; i++)
            {
                List<Direction> possibleDirections = GetPossibleDirections(specialRoomsToExpand[i].Pos, specialRoomsToExpand[i].Size, specialRoomsToExpand[i].MaxSize, 0);
                if (possibleDirections.Count > 0)
                {
                    var dir = possibleDirections[_controlledRNG.Next(possibleDirections.Count)];
                    ExpandArea(specialRoomsToExpand[i].Size, specialRoomsToExpand[i].Pos, specialRoomsToExpand[i].Type, dir, out var size, out var pos);
                    var copy = specialRoomsToExpand[i]; // Structs are funny
                    copy.Pos = pos;
                    copy.Size = size;
                    specialRoomsToExpand[i] = copy;
                }
                else
                {
                    specialRoomsToExpand.RemoveAt(i);
                    i--;
                }
            }
        }
	   */

		// --------- Plot Spawning Process ---------

		UpdatePotentialRoomSpawns(false);
		List<Plot> plots = [];

		for (int i = 0; i < plotCount; i++)
		{
            if (potentialRoomSpawns.Count == 0)
				break;


			var pos = RandomRoomSpawn;
			var plot = new Plot(pos);
			plots.Add(plot);
			AddNewArea(plot, pos);

			UpdatePotentialRoomSpawns(false);

			if (allowLogging)
			{
				Logger.Log($"Plot Spawn: {pos}");
			}
		}

		ExpansionIterator(1, [.. plots]);

		for (int i = 0; i < plots.Count; i++) // Removing small plots
		{
			if (plots[i].Size.x < ld.MinPlotSize || plots[i].Size.z < ld.MinPlotSize)
			{
				plots[i].Spots.ForEach(x => mapTiles[x.x, x.z] = RoomType.None);
				plots.RemoveAt(i);
				i--;
			}
		}

		List<List<Direction>> roomsNDirs = [];
		List<Plot> expandablePlots = new (plots);

		for (int num6 = 0; num6 < plots.Count; num6++)
		{
			roomsNDirs.Add(
		[
			Direction.North,
			Direction.East,
			Direction.South,
			Direction.West
		]);
		}
		for (int i = 0; i < hallsToRemove; i++)
		{
			if (expandablePlots.Count == 0)
				break;

			int num = _controlledRNG.Next(0, expandablePlots.Count);
			int num2 = _controlledRNG.Next(0, roomsNDirs[num].Count);
			ExpandArea(expandablePlots[num].Size, expandablePlots[num].Pos, expandablePlots[num].Type, roomsNDirs[num][num2], expandablePlots[num].Spots, out var size, out var pos);

			var copy = expandablePlots[num]; // Structs are funny
			copy.Pos = pos;
			copy.Size = size;
			expandablePlots[num] = copy;

			roomsNDirs[num].RemoveAt(num2);
			if (roomsNDirs[num].Count <= 0)
			{
				roomsNDirs.RemoveAt(num);
				expandablePlots.RemoveAt(num);
			}
			
		}

		/*foreach (var sp in specialRoomsToExpand)
		{
            if (GetPossibleDirections(sp.Pos, sp.Size, IntVector2.MaxValue, 0).Count == 0)
                Console.WriteLine("Out of bounds!!");
        }*/

		// Hall Generation

		for (int i = 0; i < mapTiles.GetLength(0); i++)
		{
			for (int j = 0; j < mapTiles.GetLength(1);  j++)
			{
				if (mapTiles[i, j] == RoomType.None && ProximityCheck(new(i, j), 2, RoomType.Room, RoomType.SpecialRoom))
					AddHall(new(i, j));
			}
		}

		// Done

		for (int i = 0; i < plots.Count; i++) // Removing all plots
		{
				plots[i].Spots.ForEach(x => mapTiles[x.x, x.z] = RoomType.None);
				plots.RemoveAt(i);
				i--;
			
		}

		for (int i = 0; i < outerBuffers.Count; i++) // How could I forget these buffer tiles, crap!!!
		{
			mapTiles[outerBuffers[i].x, outerBuffers[i].z] = RoomType.None;
			outerBuffers.RemoveAt(i);
			i--;
		}

		// Hall Disconnect Connection


		int[,] tilesLabel = new int[mapTiles.GetLength(0),mapTiles.GetLength(1)];
		bool[,] tilesLabeled = new bool[mapTiles.GetLength(0), mapTiles.GetLength(1)];

		int attempts = 0;


		bool tilesConnected = false; // Don't ask me how this works
		while (!tilesConnected)
		{
			int k = -1;
			Queue<IntVector2> tileQueue = new();
			List<List<IntVector2>> tileGroups = [];
			for (int x5 = 0; x5 < levelSize.x; x5++)
			{
				for (int num10 = 0; num10 < levelSize.z; num10++)
				{
					if (!tilesLabeled[x5, num10] && mapTiles[x5, num10] == RoomType.Hall)
					{
						k++;
						tileGroups.Add([new(x5, num10)]);
						tilesLabel[x5, num10] = k;
						tilesLabeled[x5, num10] = true;
						tileQueue.Enqueue(new(x5, num10));
						tileGroups[k].Add(new(x5, num10));
						while (tileQueue.Count > 0)
						{
							List<IntVector2> list4 = new(MatchingAdjacentTiles(tileQueue.Dequeue()));
							for (int num11 = 0; num11 < list4.Count; num11++)
							{
								IntVector2 tileController2 = list4[num11];
								if (!tilesLabeled[tileController2.x, tileController2.z])
								{
									tilesLabel[tileController2.x, tileController2.z] = k;
									tilesLabeled[tileController2.x, tileController2.z] = true;
									tileQueue.Enqueue(tileController2);
									tileGroups[k].Add(tileController2);
								}
							}
							if (++attempts > 10000)
							{
								// Throw exception here in case of crash
								Console.WriteLine("Skipped");
								goto skipHallCrash;
							}
						}
					}
				}
			}

			if (tileGroups.Count > 1)
			{
				for (int x5 = 0; x5 < tileGroups.Count; x5++)
				{
					List<int> list5 = [];
					for (int num12 = 0; num12 < tileGroups.Count; num12++)
					{
						if (num12 != x5)
						{
							list5.Add(num12);
						}
					}
					List<IntVector2> list6 = new(tileGroups[list5[_controlledRNG.Next(0, list5.Count)]]);
					IntVector2 tileController3 = tileGroups[x5][_controlledRNG.Next(0, tileGroups[x5].Count)];
					IntVector2 tileController4 = list6[_controlledRNG.Next(0, list6.Count)];
					int num13 = 0;
					Queue<IntVector2> queue = new();
					List<Direction> list7 = new(DirectionsToDestination(tileController3, tileController4));
					Direction direction = list7[_controlledRNG.Next(0, list7.Count)];
					IntVector2 intVector2 = tileController3;
					while (mapTiles[intVector2.x, intVector2.z] == RoomType.None || tilesLabel[intVector2.x, intVector2.z] == tilesLabel[tileController3.x, tileController3.z])
					{
						if (mapTiles[intVector2.x, intVector2.z] != RoomType.None && tilesLabel[intVector2.x, intVector2.z] == tilesLabel[tileController3.x, tileController3.z])
						{
							queue.Clear();
						}
						else
						{
							queue.Enqueue(intVector2);
						}
						list7 = new List<Direction>(DirectionsToDestination(intVector2, tileController4));
						if (list7.Contains(direction))
						{
							if (list7.Count > 1 && num13 * ld.BridgeTurnChance > _controlledRNG.Next(0, 100) + 1)
							{
								list7.Remove(direction);
								direction = list7[_controlledRNG.Next(0, list7.Count)];
								num13 = 0;
							}
						}
						else
						{
							direction = list7[_controlledRNG.Next(0, list7.Count)];
						}
						intVector2 += direction.ToIntVector2();
						num13++;
					}
					if (queue.Count > 0)
					{
						attempts = 0;
						while (queue.Count > 0)
							AddHall(queue.Dequeue());
						
					}
				}
				for (int num14 = 0; num14 < levelSize.x; num14++)
				{
					for (int num15 = 0; num15 < levelSize.z; num15++)
					{
						if (mapTiles[num14, num15] != RoomType.None)
						{
							tilesLabeled[num14, num15] = false;
							tilesLabel[num14, num15] = 0;
						}
					}
				}
			}
			else
			{
				tilesConnected = true;
			}
		}






	skipHallCrash:



	


		// ---------- Halls to Add Code ----------- CHECK IF THIS IS RIGHT, I'M STILL NOT SURE BECAUSE DEAD END GENERATION NEEDS TO BE FINISHED... or could I just debug the pos values and compare if they are equal :)

		List<IntVector2> potentialStartingPoints = [];
		for (int k = 0; k < hallsToAdd; k++)
		{
			for (int x6 = 0; x6 < levelSize.x; x6++)
			{
				for (int num16 = 0; num16 < levelSize.z; num16++)
				{
					if (mapTiles[x6, num16] == RoomType.None)
					{
						IntVector2 intVector3 = new(x6, num16);
						if ((TileInDirectionCheck(intVector3, Direction.North, ld.RoomSizes.Min.x) || TileInDirectionCheck(intVector3, Direction.South, ld.RoomSizes.Min.x)) && (TileInDirectionCheck(intVector3, Direction.East, ld.RoomSizes.Min.x) || TileInDirectionCheck(intVector3, Direction.West, ld.RoomSizes.Min.x)))
						{
							mapTiles[x6, num16] = RoomType.Buffer;
							buffer[x6, num16] = true;
						}
						
					}
				}
			}
			potentialStartingPoints = new(halls);
			for (int num17 = 0; num17 < potentialStartingPoints.Count; num17++)
			{
				if (GetTileNeighbors(potentialStartingPoints[num17]).Count >= 4)
				{
					potentialStartingPoints.RemoveAt(num17);
					num17--;
				}
			}
			int x5 = _controlledRNG.Next(0, potentialStartingPoints.Count);
			while (potentialStartingPoints.Count > 0 && PotentialPathDirections(potentialStartingPoints[x5]).Count == 0)
			{
				potentialStartingPoints.RemoveAt(x5);
				x5 = _controlledRNG.Next(0, potentialStartingPoints.Count);
			}
			if (potentialStartingPoints.Count > 0)
			{
				bool success = false;
				int x6 = 0;
				Queue<IntVector2> curPath = new();
				while (!success && ++x6 < ld.MaxHallAttempts)
					curPath = GetRandomPath(potentialStartingPoints[x5], ld.AdditionTurnChance, out success);
				

				foreach (IntVector2 intVector4 in curPath)
					AddHall(intVector4);
				
				
				//if (tileToConnect != null && tileToConnect.room != this.halls[0])
				//{
				//	base.AddDoor(tileToConnect, tileToConnect.room.doorPre, connectDir.GetOpposite(), false); This is for adding doors, but well, this won't have any doors anyways
				//}
			}
		}


		

		// Done


		canGenerateAgain = false;
    }


	public void DisplayGrid()
	{
		Logger.Log("Current Grid:");

		var mapTileClone = mapTiles.Reverse2DArray();


		for (int i = mapTileClone.GetLength(0) - 1; i > 0; i--) // Took me a while to figure all of this to display correctly the map inside the console
		{
			for (int j = 0; j < mapTileClone.GetLength(1); j++)
			{
				switch (mapTileClone[i, j])
				{
					case RoomType.Hall: Console.BackgroundColor = ConsoleColor.Yellow; break;
					case RoomType.Room: Console.BackgroundColor = ConsoleColor.Gray; Console.ForegroundColor = ConsoleColor.Black; break;
					case RoomType.SpecialRoom: Console.BackgroundColor = ConsoleColor.White; Console.ForegroundColor = ConsoleColor.Black; break;


					default:
						if (mapTileClone[i, j].HasFlag(RoomType.Border))
							Console.BackgroundColor = ConsoleColor.DarkGray;
						break;
				}

				Console.Write($"{(UseSymmetricalField ? "  " : (buffer[j, i] ? "1" : (int)mapTileClone[i, j]) + ",")}"); // A very specific check lol

				Console.ResetColor();

			}
			Console.WriteLine(); // Skips for one line below
		}
	}

	const bool UseSymmetricalField = false;


	private void ExpansionIterator(int buffer, out List<IRoomStructure> structuresBack,params IRoomStructure[] rooms)
	{
		var rest = new List<IRoomStructure>();
		structuresBack = rest;

		if (rooms.Length == 0) return;

		
		List<IRoomStructure> plotsToExpand = new(rooms);
		while (plotsToExpand.Count > 0)
		{
			for (int i = 0; i < plotsToExpand.Count; i++)
			{
                List<Direction> possibleDirections2 = GetPossibleDirections(plotsToExpand[i].Pos, plotsToExpand[i].Size, plotsToExpand[i].MaxSize, buffer);
				if (possibleDirections2.Count > 0)
				{
					ExpandArea(plotsToExpand[i].Size, plotsToExpand[i].Pos, plotsToExpand[i].Type, possibleDirections2[_controlledRNG.Next(possibleDirections2.Count)], plotsToExpand[i].Spots, out var size, out var pos);
					var copy = plotsToExpand[i]; // Structs are funny
					copy.Pos = pos;
					copy.Size = size;
					plotsToExpand[i] = copy;
				}
				else
				{
					rest.Add(plotsToExpand[i]);
					plotsToExpand.RemoveAt(i);
					i--;
				}
			}
		}
	}

	private void ExpansionIterator(int buffer, params IRoomStructure[] rooms)
	{

		if (rooms.Length == 0) return;


		List<IRoomStructure> plotsToExpand = new(rooms);
		while (plotsToExpand.Count > 0)
		{
			for (int i = 0; i < plotsToExpand.Count; i++)
			{
				List<Direction> possibleDirections2 = GetPossibleDirections(plotsToExpand[i].Pos, plotsToExpand[i].Size, plotsToExpand[i].MaxSize, buffer);
				if (possibleDirections2.Count > 0)
				{
					ExpandArea(plotsToExpand[i].Size, plotsToExpand[i].Pos, plotsToExpand[i].Type, possibleDirections2[_controlledRNG.Next(possibleDirections2.Count)], plotsToExpand[i].Spots, out var size, out var pos);
					var copy = plotsToExpand[i]; // Structs are funny
					copy.Pos = pos;
					copy.Size = size;
					plotsToExpand[i] = copy;
				}
				else
				{
					plotsToExpand.RemoveAt(i);
					i--;
				}
			}
		}
	}

	public void AddNewArea(IRoomStructure room, IntVector2 pos)
	{
		mapTiles[pos.x, pos.z] = room.Type;
		room.Spots.Add(pos);
	}

    private void Internal_SkipRNGVals(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            _controlledRNG.Next();
        }
    }

	// Stuff for initialization

	private readonly ConsoleLogger Logger = new("Generator");

    private Random _controlledRNG = new(0);

    private readonly int _seed = 0;

    private readonly int _seedOffset = 0;

    private readonly LevelObject ld;

    bool canGenerateAgain = true;

    public LevelObject LevelObject { get => ld; }

    public int Seed { get => _seed; }

    private int globalRoomID = 0;

    public int NewRoomID { get => globalRoomID++; } // Every time it is called, it just increments

    // DURING Generation Fields

    RoomType[,] mapTiles = new RoomType[0, 0]; // The strategy is simple, -1 means the tile doesn't exists, above -1 is the tile id, meaning hallways or any other room type

	bool[,] buffer = new bool[0, 0];

	IntVector2 levelSize;

    List<WeightedSelection<IntVector2>> potentialRoomSpawns = [];

	readonly List<IntVector2> halls = [];


    internal IntVector2 RandomRoomSpawn => WeightedSelection<IntVector2>.ControlledRandomSelection(_controlledRNG, [.. potentialRoomSpawns]);




    
}
