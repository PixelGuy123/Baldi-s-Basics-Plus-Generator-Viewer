using BBP_Gen.Elements;
using BBP_Gen.Misc;
using System.Collections;

namespace BBP_Gen.PlusGenerator;

public partial class Generator // Makes it easier to make an instance of it. Main Class here for the generator
{
    public Generator(int seed, int seedOffset, LevelObject levelObj) =>
        (_seed, _seedOffset, ld) = (seed, seedOffset, levelObj);

	public SeedToken BeginGeneration(bool onlyGlitchedMode = false)
    {
		
        

        _controlledRNG = new(Seed); // for npc stuff

		// --NPC STUFF (later)--

		LevelObject[] list = [.. ld.PreviousLevels, ld];
		List<string> npcs = [];

		int k; // Just initialize a k for every iterator

		foreach (LevelObject levelObject in list)
		{
			foreach (string npc in levelObject.ForcedNpcs)
				npcs.Add(npc);
			
			
			List<WeightedSelection<string>> list2 = [.. levelObject.PotentialNPCs];
			foreach (var npc2 in npcs)
			{
				for (k = 0; k < list2.Count; k++)
				{
					if (list2[k].selection == npc2)
					{
						list2.RemoveAt(k);
						k--;
					}
				}
			}
			int num = 0;
			while (num < levelObject.AdditionalNPCs && list2.Count > 0)
			{
				string npc3 = WeightedSelection<string>.ControlledRandomSelection_List(_controlledRNG, list2);
				npcs.Add(npc3);
				for (int l = 0; l < list2.Count; l++)
				{
					if (list2[l].selection == npc3)
					{
						list2.RemoveAt(l);
						l--;
					}
				}
				num++;
			}
		}

		// --End of npc stuff--



		_controlledRNG = new(Seed + _seedOffset);

        levelSize = new(_controlledRNG.Next(ld.LevelSizes.Min.x, ld.LevelSizes.Max.x + 1) + ld.OuterEdgeBuffer * 2, _controlledRNG.Next(ld.LevelSizes.Min.z, ld.LevelSizes.Max.z + 1) + ld.OuterEdgeBuffer * 2); // Creates level size

        if (_controlledRNG.Next(0, 2) == 1) (levelSize.z, levelSize.x) = (levelSize.x, levelSize.z); // Change Values


        mapTiles = new RoomType[levelSize.x, levelSize.z];
		buffer = new bool[levelSize.x, levelSize.z];
		roomTiles = new Room[levelSize.x, levelSize.z];

		int plotCount = _controlledRNG.Next(ld.PlotCount.Min, ld.PlotCount.Max + 1);
        int hallsToRemove = _controlledRNG.Next(ld.HallRemovalCount.Min, ld.HallRemovalCount.Max + 1);
        _controlledRNG.Next(); // Skip one value (SideHallsToRemove... idk what's that even for)
        int hallsToAdd = _controlledRNG.Next(ld.HallsAddCount.Min, ld.HallsAddCount.Max + 1);
        int eventCount = _controlledRNG.Next(ld.EventCount.Min, ld.EventCount.Max);

        // --Event Stuff Here--

        var events = new List<WeightedSelection<RandomEvent>>(ld.RandomEvents);
        var eventsToLaunch = new List<RandomEvent>(); // Note this variable will never lose the events, it'll stay until the end



        int h = 0;
        while (h++ < eventCount && events.Count > 0)
        {
            var rEvent = WeightedSelection<RandomEvent>.ControlledRandomSelection_List(_controlledRNG, events);
            eventsToLaunch.Add(rEvent);
            events.RemoveAll(x => x.selection == rEvent);
        }
		bool hasMystery = false;
        foreach (var e in eventsToLaunch)
        {
			if (e.Name == "Mystery Room")
				hasMystery = true;

            e.Initialize(_controlledRNG);
            _controlledRNG.NextDouble();
        }


        // --Event Stuff Gone--

		// --Field Trip first operation--
		Direction fieldTripDir = Direction.North;
		
		if (ld.FieldTrip)
		{
			fieldTripDir = Directions.ControlledRandomDirection(_controlledRNG);
			Internal_SkipRNGVals(3); // Skip the item selection
		}

		// --Field trip done--

		Internal_SkipRNGVals(22); // Skip all that material choose thing

		int specialRoomCount = _controlledRNG.Next(ld.SpecialRoomCount.Min, ld.SpecialRoomCount.Max + 1);

        // Here comes buffer stuff

        // =========== Note ===========
        // Buffer tiles will have 99 as id by default!!

        int northEdgeBuffer = ld.OuterEdgeBuffer;
        int eastEdgeBuffer = ld.OuterEdgeBuffer;
        int southEdgeBuffer = ld.OuterEdgeBuffer;
        int westEdgeBuffer = ld.OuterEdgeBuffer;

		// --Another field trip operation over buffer tiles--

		int num3 = 5;

		if (ld.FieldTrip)
		{
			switch (fieldTripDir)
			{
				case Direction.North:
					if (northEdgeBuffer < num3)
					{
						northEdgeBuffer = num3;
					}
					break;
				case Direction.East:
					if (eastEdgeBuffer < num3)
					{
						eastEdgeBuffer = num3;
					}
					break;
				case Direction.South:
					if (southEdgeBuffer < num3)
					{
						southEdgeBuffer = num3;
					}
					break;
				case Direction.West:
					if (westEdgeBuffer < num3)
					{
						westEdgeBuffer = num3;
					}
					break;
			}
			_controlledRNG.Next(); // Skip field trip type selection
		}

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
			


            specialRoomsToExpand.Add(specialRoom);
        }

		ExpansionIterator_List(0, out var specialRooms, specialRoomsToExpand);
		specialRoomsToExpand = specialRooms;

		UpdateTileReferences(specialRoomsToExpand.ConvertAll(x => x.AsRoom())); // Update the tiles here because I forgor

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

		}

		ExpansionIterator_List(1, plots);

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

		for (int i = 0; i < plots.Count;) // Removing all plots
		{
				plots[i].Spots.ForEach(x => mapTiles[x.x, x.z] = RoomType.None);
				plots.RemoveAt(i);
		}

		for (int i = 0; i < outerBuffers.Count;) // How could I forget these buffer tiles, crap!!!
		{
			mapTiles[outerBuffers[i].x, outerBuffers[i].z] = RoomType.None;
			outerBuffers.RemoveAt(i);
		}

		// Hall Disconnect Connection


		int[,] tilesLabel = new int[mapTiles.GetLength(0),mapTiles.GetLength(1)];
		bool[,] tilesLabeled = new bool[mapTiles.GetLength(0), mapTiles.GetLength(1)];

		int attempts = 0;


		bool tilesConnected = false; // Don't ask me how this works
		while (!tilesConnected)
		{
			k = -1;
			Queue<IntVector2> tileQueue = new();
			List<List<IntVector2>> tileGroups = [];
			for (int x5 = 0; x5 < levelSize.x; x5++)
			{
				for (int num10 = 0; num10 < levelSize.z; num10++)
				{
					if (!tilesLabeled[x5, num10] && mapTiles[x5, num10] == RoomType.Hall)
					{
						k++;
						tileGroups.Add([]);
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
								throw new SeedCrashException(Seed, SeedCrashType.HallCrash);
							
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



		// ---------- Halls to Add Code -----------

		// Note - Reusing outerBuffers to prevBuffers
		
		List<IntVector2> potentialStartingPoints = [];

		//List<IntVector2> prevBuffers = [];
		
		for (k = 0; k < hallsToAdd; k++)
		{
            for (int i = 0; i < outerBuffers.Count;) // No increment needed if it's gonna clear the list up
			{
				var vec = outerBuffers[i];
				mapTiles[vec.x, vec.z] = RoomType.None;
				buffer[vec.x, vec.z] = false;
				outerBuffers.RemoveAt(i);
			}
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
							outerBuffers.Add(intVector3);
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
				while (!success && x6++ < ld.MaxHallAttempts) // Omg, all the time was just x6++ due to the increment being on the wrong side
					curPath = GetRandomPath(potentialStartingPoints[x5], ld.AdditionTurnChance, out success);


				foreach (IntVector2 intVector4 in curPath)
					AddHall(intVector4);
                
				
				
			}
			
		}

		for (int i = 0; i < outerBuffers.Count;) // No increment needed if it's gonna clear the list up
		{
			var vec = outerBuffers[i]; // Removing buffer again, just here now
			mapTiles[vec.x, vec.z] = RoomType.None;
			buffer[vec.x, vec.z] = false;
			outerBuffers.RemoveAt(i);
		}


		// ------ Dead End Hallway Gen ------


		// Re-using outerbuffers again
		//List<IntVector2> deadEnds = [];
		for (k = 0; k < levelSize.x; k++)
		{
			for (int x9 = 0; x9 < levelSize.z; x9++)
			{
				var pos = new IntVector2(k, x9);
				if (mapTiles[k, x9] != RoomType.None && MatchingAdjacentTiles(pos).Count == 1)
					outerBuffers.Add(pos);
				
			}
		}


		foreach (var tileController24 in outerBuffers)
		{
			bool success = true;
			var list8 = MatchingAdjacentTiles(tileController24);
			List<IntVector2> list9 = [tileController24];
			List<IntVector2> potSpawns = [tileController24];
			for (int num23 = 0; num23 < ld.DeadEndBuffer; num23++)
			{
				if (list8.Count > 2)
				{
					success = false;
					break;
				}
				foreach (var tileController8 in list9)
					list8.RemoveAll(x => x.x == tileController8.x && x.z == tileController8.z);
				
				foreach (var tileController9 in list8)
				{
					list8 = MatchingAdjacentTiles(tileController9);
					potSpawns.Add(tileController9);
					list9.Add(tileController9);
				}
			}


            if (success)
			{
				var pos = potSpawns[_controlledRNG.Next(0, potSpawns.Count)];
                foreach (IntVector2 intVector6 in GetRandomPath(pos, ld.AdditionTurnChance, out _))
					AddHall(intVector6);
                

			}
		}

		var oobspcs = specialRoomsToExpand.Where(s => CheckBigRoomSides(s) == 0); // OOB Check here
		var onedoorspcs = specialRoomsToExpand.Where(s => CheckBigRoomSides(s) == 1); // 1 door to bigroom Check here

		uncommonTags[1] = onedoorspcs.Any();

		// -------------- Elevator Generation --------------
		// Re-using tilesLabeled for it

		//bool[,] acceptExit = new bool[levelSize.x, levelSize.z];
		//tilesLabeled.Initialize(); // Resets to fals < no need anymore lol
		tilesLabel.Initialize(); // Resets to 0
		int label = 0;

		foreach (var specialRoom in specialRoomsToExpand) // apply to special rooms for this to work
		{
			if (specialRoom.AcceptExits)
			{
				label++;
				for (int i = 0; i < specialRoom.Spots.Count; i++)
				{
					var pos = specialRoom.Spots[i];
					if (tilesLabel.InsideBounds(pos))
					{
						tilesLabeled[pos.x, pos.z] = true;
						tilesLabel[pos.x, pos.z] = label;
					}
				}
				
			}
		}

		halls.ForEach(hall => tilesLabeled[hall.x, hall.z] = true); // Halls obviously accept exists and have the label in 1 by default


		List<Direction> potentailExitDirections = Directions.AllList();
		int exitCount = ld.ExitCount;
		for (k = 0; k < exitCount; k++)
		{
			int num24 = _controlledRNG.Next(0, potentailExitDirections.Count);
			Direction direction3 = potentailExitDirections[num24];
			potentailExitDirections.RemoveAt(num24);
			bool flag2 = false;
			List<IntVector2> list10 = [];
			IntVector2 intVector7 = direction3.ToIntVector2();
			int num49 = Math.Max(0, (levelSize.x - 1) * intVector7.x);
			int num25 = Math.Max(0, (levelSize.x - 1) * (intVector7.x + Math.Abs(intVector7.z)));
			int num26 = Math.Max(0, (levelSize.z - 1) * intVector7.z);
			int num27 = Math.Max(0, (levelSize.z - 1) * (intVector7.z + Math.Abs(intVector7.x)));
			for (int num28 = num49; num28 <= num25; num28++)
			{
				for (int num29 = num26; num29 <= num27; num29++)
				{
					IntVector2 intVector8 = direction3.GetOpposite().ToIntVector2();
					IntVector2 vec = default;

					while (IsTileNull(num28 + vec.x, num29 + vec.z))
						vec += intVector8;
					
					
					if (mapTiles.InsideBounds(num28 + vec.x, num29 + vec.z))
					{
						IntVector2 intVector10 = new(num28 + vec.x, num29 + vec.z);
						if (mapTiles[intVector10.x, intVector10.z] != RoomType.None)
						{
							var tileController11 = intVector10;
							if (tilesLabeled[intVector10.x, intVector10.z])
							{
								bool flag3 = true;
								foreach (Direction direction4 in direction3.PerpendicularList())
								{
																	
									
									if (mapTiles[intVector10.x + direction4.ToIntVector2().x, intVector10.z + direction4.ToIntVector2().z] == RoomType.None || tilesLabel[intVector10.x + direction4.ToIntVector2().x, intVector10.z + direction4.ToIntVector2().z] != tilesLabel[tileController11.x, tileController11.z])
									{
										flag3 = false;
										break;
									}
								}
								if (flag3)
								{
									if (ElevatorSpotFits(intVector10 + (direction3.ToIntVector2() * 2), direction3.GetOpposite(), mapTiles.GetItem(intVector10) | RoomType.Elevator))
										list10.Add(intVector10 + direction3.ToIntVector2());
                                    
									
									if (mapTiles[tileController11.x, tileController11.z] != RoomType.Hall)
										flag2 = true;
									
								}
							}
						}
					}
				}
			}
			if (list10.Count > 0)
			{
				if (flag2)
				{
					for (int num30 = 0; num30 < list10.Count; num30++)
					{
						var p = list10[num30] + direction3.GetOpposite().ToIntVector2();
						if (mapTiles[p.x, p.z] == RoomType.Hall)
						{
							list10.RemoveAt(num30);
							num30--;
						}
					}
				}
				if (list10.Count == 0)
					throw new SeedCrashException(Seed, SeedCrashType.ElevatorCrash);

				int num31 = _controlledRNG.Next(0, list10.Count);
				var pos = list10[num31];
				CreateElevator(pos - direction3.GetOpposite().ToIntVector2(), direction3.GetOpposite(), k == 0);
				pos += direction3.GetOpposite().ToIntVector2();
				if (mapTiles[pos.x, pos.z] != RoomType.Hall)
				{
                    int l = tilesLabel[pos.x, pos.z];
					for (int i = 0; i < tilesLabeled.GetLength(0); i++)
					{
						for (int j = 0; j < tilesLabeled.GetLength(1); j++)
						{
							if (tilesLabel[i, j] == l)
								tilesLabeled[i, j] = false; // Basically remove any tile that was accepting exits before
						}
					}
				}
			}
			else
			{
				k--;
				exitCount = Math.Min(exitCount, potentailExitDirections.Count);
			}
		}

		// Field Trip Spawn Here

		if (ld.FieldTrip)
		{
			IntVector2[] array4 = EdgeTiles(fieldTripDir);

			fieldTripDir = fieldTripDir.GetOpposite(); // Opposite it for it to work (whyyyyyyyyyyyy)

			List<IntVector2> list12 = [];
			foreach (var tileController13 in array4)
				if (IsTileNotNull(tileController13) && !IsTileEqual(tileController13, RoomType.Elevator) && FieldTripSuitable(tileController13, fieldTripDir) && tilesLabeled[tileController13.x, tileController13.z])
					list12.Add(tileController13);


			if (list12.Count > 0)
				CreateFieldTrip(list12[_controlledRNG.Next(0, list12.Count)], fieldTripDir);
			else
				uncommonTags[2] = true;
				
			
		}


		// Field Trip Spawn Done




		// --------- Room Generation ---------

		var rooms = new List<Room>();

		UpdatePotentialRoomSpawns(true);

		if (npcs.Contains("Gotta Sweep")) // If sweep, janitor room!!
		{
			var room = new Room
			{
				Type = RoomType.Janitor,
				MaxSize = new IntVector2(2, 2)
			};
			AddNewArea(room, RandomRoomSpawn);
			rooms.Add(room);
			Internal_SkipRNGVals(2); // Setting the sizes for this, which is unnecessary
			List<Direction> possibleDirections4 = GetPossibleDirections(room.Pos, room.Size, room.MaxSize, 0);
			if (possibleDirections4.Count > 0)
			{
				ExpandArea(room.Size, room.Pos, room.Type, possibleDirections4[_controlledRNG.Next(possibleDirections4.Count)], room.Spots, out var size, out var pos);
				room.Size = size;
				room.Pos = pos;
			}
		}

		int ogclassRoomCount = _controlledRNG.Next(ld.ClassRooms.Min, ld.ClassRooms.Max + 1);
		int facultyRoomCount = _controlledRNG.Next(ld.Faculties.Min, ld.Faculties.Max + 1);
		_controlledRNG.Next(); // Extra Rooms Call, idk what are those for
		int officeRoomCount = _controlledRNG.Next(ld.Offices.Min, ld.Offices.Max + 1);
		int roomCount = ogclassRoomCount + facultyRoomCount + officeRoomCount + rooms.Count;


		UpdatePotentialRoomSpawns(true);
		k = rooms.Count;
		do
		{

			var room = new Room();
			rooms.Add(room);
			AddNewArea(room, RandomRoomSpawn);
			room.MaxSize = new(_controlledRNG.Next(ld.RoomSizes.Min.x, ld.RoomSizes.Max.x + 1), _controlledRNG.Next(ld.RoomSizes.Min.z, ld.RoomSizes.Max.z + 1));

			ExpansionIterator(0, room);


			UpdatePotentialRoomSpawns(true);
		} while (++k < roomCount && potentialRoomSpawns.Count > 0);


		// Door operation here, I'll just make it save the adjacent rooms, that's necessary

		UpdateTileReferences(rooms);

		foreach (var room in rooms)
		{

			AddRandomDoor(room, true, false);
            if ((float)_controlledRNG.NextDouble() * 100f < ld.PassThroughChance) // Check if there's any other adjacent room
			{                
                bool flag = !RoomProximityList(room, 0).Any(x => x != RoomType.Hall);
				AddRandomDoor(room, !flag, flag);
			}

        }


		List<WeightedSelection<Room>> potentialClassRooms = [];
		foreach (var room in rooms)
		{
			if (room.AdjacentRooms.Contains(hall) && room.Type == RoomType.Room)
				potentialClassRooms.Add(new WeightedSelection<Room>(room, 1));

           
        }

		eventsToLaunch.ForEach(ev => ev.ClaimARoom(potentialClassRooms, this));

		officeRoomCount = Math.Min(officeRoomCount, potentialClassRooms.Count);
		for (k = 0; k < officeRoomCount; k++)
		{
			int num = _controlledRNG.Next(0, potentialClassRooms.Count);
			potentialClassRooms[num].selection.Type = RoomType.Office;
			UpdateTiles(potentialClassRooms[num].selection); // Update tiles
			potentialClassRooms.RemoveAt(num);

			_controlledRNG.Next(); // For choosing builder
		}
		List<Room> classRooms = [];

		int classRoomCount = Math.Min(ogclassRoomCount, potentialClassRooms.Count);
		for (k = 0; k < classRoomCount; k++)
		{
			potentialClassRooms.ForEach((cs) =>
			{
				cs.weight = 1;
				classRooms.ForEach(c => cs.weight += (int)Math.Round(Math.Pow(Math.Abs((RealRoomMid(c) - RealRoomMid(cs.selection)).Magnitude() / 10f), ld.ClassDistanceWeightExponent)));
			});

			var roomController4 = WeightedSelection<Room>.ControlledRandomSelection_List(_controlledRNG, potentialClassRooms);
			classRooms.Add(roomController4);

			for (int num29 = 0; num29 < potentialClassRooms.Count; num29++)
			{
				if (potentialClassRooms[num29].selection == roomController4)
				{
					potentialClassRooms.RemoveAt(num29);
					break;
				}
			}

			roomController4.Type = RoomType.Classroom;

			_controlledRNG.Next(); // another builder selection
		}

        UpdateTiles(classRooms);

		if (classRooms.Count < ogclassRoomCount)
			type |= SeedType.Glitched;
		
		
		if (oobspcs.Any()) // Any special room with no space available
			type |= SeedType.OOB;
		if (uncommonTags.Any(x => x)) // If any boolean is true
			type |= SeedType.Uncommon;

		List<string> data2 = [];
		if (uncommonTags[1])
			data2.AddRange(onedoorspcs.Select(s => "1 door at " + s.Name));
		if (oobspcs.Any())
			data2.AddRange(oobspcs.Select(s => "OOB " + s.Name));
		if (uncommonTags[0])
			data2.Add("1-way wall");
		if (uncommonTags[2])
			data2.Add("Missing Field Trip"); // Yes, bunch of adds here

		if (onlyGlitchedMode)
			return new SeedToken(type, classRooms.Count, !hasMystery, [.. data2]);

		List<Room> facultyRooms = [];

		foreach (var roomController6 in rooms)
		{
			if (roomController6.Type == RoomType.Room)
			{
				facultyRooms.Add(roomController6);
			}
		}
		facultyRoomCount = Math.Min(facultyRoomCount, facultyRooms.Count);
		for (k = 0; k < facultyRoomCount; k++)
		{
			int num37 = _controlledRNG.Next(0, facultyRooms.Count);
			facultyRooms[num37].Type = RoomType.Faculty;
			UpdateTiles(facultyRooms[num37]);
			facultyRooms.RemoveAt(num37);

			_controlledRNG.Next(); // Builder selection
		}

		// Done
		List<string> data = []; // Can't be really simplified because of the collections
		data.Add("Level initialized with seed: " + Seed);
		data.Add("------- All NPCs on map -------");
		data.Add("Baldi");
		data.AddRange(npcs);
		data.Add("------ Extra Info ------");
		data.Add($"Level Size: {levelSize.x},{levelSize.z}");
		eventsToLaunch.ForEach(e => data.Add($"Event Available: {e.Name}"));
		specialRoomsToExpand.ForEach(specialRoom => data.Add($"Special Room Data: {specialRoom.Name} with size: {specialRoom.MaxSize} on Pos: {specialRoom.Pos}"));
		data.Add($"Elevators: {exitCount}/{ld.ExitCount}");
		data.Add("-------- Room Gen Data --------");
		data.Add($"Notebooks: 0/{classRooms.Count} {(type.HasFlag(SeedType.Glitched) && !hasMystery ? "(APR) " : string.Empty)}{(type.HasFlag(SeedType.Glitched) ? "-- IT IS A GLITCHED SEED!!" : string.Empty)}");
		data.Add($"Faculties: {facultyRoomCount}");
		data.Add("Seed Tags: " + type.ToString());
		
        return new SeedToken(type, classRooms.Count, !hasMystery, [.. data]); // SeedType.Normal is a temporary attribute, I'll change it to be dynamic
    }


	public void DisplayGrid()
	{
        Console.WriteLine("Current Grid:");

		var mapTileClone = mapTiles.Reverse2DArray();

		for (int i = mapTileClone.GetLength(0) - 1; i >= 0; i--) // Took me a while to figure all of this to display correctly the map inside the console
		{
			for (int j = 0; j < mapTileClone.GetLength(1); j++)
			{
				if (spawnSpot.z == i && spawnSpot.x == j)
					Console.BackgroundColor = ConsoleColor.Cyan;
				
				else
				{
					switch (mapTileClone[i, j])
					{
						case RoomType.Hall: Console.BackgroundColor = ConsoleColor.Yellow; break;
						case RoomType.Elevator:
						case RoomType.Border: Console.BackgroundColor = ConsoleColor.Gray; Console.ForegroundColor = ConsoleColor.Black; break;
						case RoomType.Janitor:
						case RoomType.SpecialRoom: Console.BackgroundColor = ConsoleColor.White; Console.ForegroundColor = ConsoleColor.Black; break;
						case RoomType.Room: Console.BackgroundColor = ConsoleColor.Cyan; Console.ForegroundColor = ConsoleColor.Black; break;
						case RoomType.Classroom: Console.BackgroundColor = ConsoleColor.Red; break;
						case RoomType.Faculty: Console.BackgroundColor = ConsoleColor.DarkYellow; break;
						case RoomType.Office: Console.BackgroundColor = ConsoleColor.DarkGray; Console.ForegroundColor = ConsoleColor.Black; break;
						case RoomType.FieldTripRoom: Console.BackgroundColor = ConsoleColor.Blue; break;


						default:
							if (mapTileClone[i, j].HasFlag(RoomType.Border))
								Console.BackgroundColor = ConsoleColor.DarkGray;
							break;
					}
				}

				if (poses.Any(p => p.z == i && p.x == j))
					Console.BackgroundColor = ConsoleColor.Blue;

				Console.Write($"{(UseSymmetricalField ? "  " : (buffer[j, i] ? "1" : (int)mapTileClone[i, j]) + ",")}"); // A very specific check lol

				Console.ResetColor();

			}
			Console.WriteLine(); // Skips for one line below
		}

		Console.WriteLine("Captions: "); // Block of code for caption lol
		Console.BackgroundColor = ConsoleColor.Yellow; Console.ForegroundColor = ConsoleColor.Black;
		Console.Write("  - Hallway");
		Console.ResetColor();
		Console.WriteLine();
		
		Console.BackgroundColor = ConsoleColor.Gray; Console.ForegroundColor = ConsoleColor.Black;
		Console.Write("  - Elevator (Cyan if it is the Player\'s spawn");
		Console.ResetColor();
		Console.WriteLine();
		
		Console.BackgroundColor = ConsoleColor.White; Console.ForegroundColor = ConsoleColor.Black;
		Console.Write("  - Janitor/SpecialRoom/Mystery Room");
		Console.ResetColor();
		Console.WriteLine();
		
		Console.BackgroundColor = ConsoleColor.Cyan; Console.ForegroundColor = ConsoleColor.Black;
		Console.Write("  - Placeholder Room");
		Console.ResetColor();
		Console.WriteLine();
		
		Console.BackgroundColor = ConsoleColor.Red; Console.ForegroundColor = ConsoleColor.Black;
		Console.Write("  - Classroom");
		Console.ResetColor();
		Console.WriteLine();
		
		Console.BackgroundColor = ConsoleColor.DarkGray; Console.ForegroundColor = ConsoleColor.Black;
		Console.Write("  - Office");
		Console.ResetColor();
		Console.WriteLine();
		
		Console.BackgroundColor = ConsoleColor.DarkYellow; Console.ForegroundColor = ConsoleColor.Black;
		Console.Write("  - Faculty");
		Console.ResetColor();
		Console.WriteLine();

		Console.BackgroundColor = ConsoleColor.Blue;
		Console.Write("  - Field Trip");
		Console.ResetColor();
		Console.WriteLine();

		Console.WriteLine("(APR) = All Placeholder Rooms = No Mystery Room in the seed");
		
	}

	readonly IntVector2[] poses = [];

	const bool UseSymmetricalField = true;

	// Stuff for initialization

    private Random _controlledRNG = new(0);

    private readonly int _seed = 0;

    private readonly int _seedOffset = 0;

    private readonly LevelObject ld;

	IntVector2 spawnSpot = default;

	readonly bool[] uncommonTags = new bool[3]; // in order: 1-way wall, 1 door to bigroom, field trip

	readonly Room hall = new()
	{
		Type = RoomType.Hall
	};

	SeedType type = SeedType.Normal;

    public LevelObject LevelObject { get => ld; }

    public int Seed { get => _seed; }

    private int globalRoomID = 0;

    public int NewRoomID { get => globalRoomID++; } // Every time it is called, it just increments

    // DURING Generation Fields

    RoomType[,] mapTiles = new RoomType[0, 0]; // The strategy is simple, -1 means the tile doesn't exists, above -1 is the tile id, meaning hallways or any other room type

	Room[,] roomTiles = new Room[0, 0];

	bool[,] buffer = new bool[0, 0];

	IntVector2 levelSize;

    readonly List<WeightedSelection<IntVector2>> potentialRoomSpawns = [];

	readonly List<IntVector2> halls = [];


    internal IntVector2 RandomRoomSpawn => WeightedSelection<IntVector2>.ControlledRandomSelection_List(_controlledRNG, potentialRoomSpawns);




    
}
