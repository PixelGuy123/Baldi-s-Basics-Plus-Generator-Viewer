using BBP_Gen.Elements;
using BBP_Gen.Misc;

namespace BBP_Gen.PlusGenerator;

public class Generator // Makes it easier to make an instance of it
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
        {
            Console.WriteLine("Initiating Generator");
            Logger.Log("Initializing with Seed: " + Seed);
        }

        _controlledRNG = new(Seed); // for npc stuff

        // --NPC STUFF (later)--



        // --End of npc stuff--



        _controlledRNG = new(Seed + _seedOffset);

        levelSize = new(_controlledRNG.Next(ld.LevelSizes.Min.x, ld.LevelSizes.Max.x + 1) + ld.OuterEdgeBuffer * 2, _controlledRNG.Next(ld.LevelSizes.Min.z, ld.LevelSizes.Max.z + 1) + ld.OuterEdgeBuffer * 2); // Creates level size

        if (_controlledRNG.Next(0, 2) == 1) (levelSize.z, levelSize.x) = (levelSize.x, levelSize.z); // Change Values

        //Logger.Log($"current level size is: {levelSize.x},{levelSize.z}");

        mapTiles = new int[levelSize.x, levelSize.z];
        for (int i = 0; i < mapTiles.GetLength(0); i++)
        {
            for (int j = 0; j < mapTiles.GetLength(1); j++)
            {
                mapTiles[i, j] = -1;
            }
        }

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
            //Logger.Log($"Event Available: {e.Name}");
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


        // Special Room Stuff begins here

        for (int i = 0; i < levelSize.x; i++) // This is to create an border, will be useful for special rooms and plots
        {
            for (int j = 0; j < levelSize.z; j++)
            {
                if (i < westEdgeBuffer || levelSize.x - i <= eastEdgeBuffer || j < southEdgeBuffer || levelSize.z - j <= northEdgeBuffer)
                    mapTiles[i, j] = 99; // Sets as a buffer tile

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



        while (specialRoomsToExpand.Count > 0) // Expand them
        {
            for (int i = 0; i < specialRoomsToExpand.Count; i++)
            {
                List<Direction> possibleDirections = GetPossibleDirections(specialRoomsToExpand[i].Pos, specialRoomsToExpand[i].Size, specialRoomsToExpand[i].MaxSize, 0);
                if (possibleDirections.Count > 0)
                {
                    var dir = possibleDirections[_controlledRNG.Next(possibleDirections.Count)];
                    ExpandArea(specialRoomsToExpand[i].Size, specialRoomsToExpand[i].Pos, specialRoomsToExpand[i].ID, dir, out var size, out var pos);
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

        Internal_DisplayGrid();








        canGenerateAgain = false;
    }

    public void AddNewArea(int roomId, IntVector2 pos) => mapTiles[pos.x, pos.z] = roomId;

    private void Internal_SkipRNGVals(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            _controlledRNG.Next();
        }
    }

    private void Internal_DisplayGrid()
    {
        Logger.Log("Current Grid:");

        var mapTileClone = mapTiles.Reverse2DArray();


        for (int i = mapTileClone.GetLength(0) - 1; i > 0; i--) // Took me a while to figure all of this to display correctly the map inside the console
        {
            for (int j = 0; j < mapTileClone.GetLength(1); j++)
            {
                if (mapTileClone[i, j] == 0)
                    Console.BackgroundColor = ConsoleColor.Yellow;

                Console.Write($"{(UseSymmetricalField ? mapTileClone[i, j] == 99 ? 0 : 1 : mapTileClone[i, j])},"); // A very specific check lol

                Console.ResetColor();

            }
            Console.WriteLine(); // Skips for one line below
        }
    }

    const bool UseSymmetricalField = true;

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

    public bool DoesRoomExists(int id)
    {
        foreach (int num in mapTiles)
            if (num == id) return true; // Yup

        return false;
    }

    // DURING Generation Fields

    int[,] mapTiles = new int[0, 0]; // The strategy is simple, -1 means the tile doesn't exists, above -1 is the tile id, meaning hallways or any other room type

    IntVector2 levelSize;

    List<WeightedSelection<IntVector2>> potentialRoomSpawns = [];


    internal IntVector2 RandomRoomSpawn => WeightedSelection<IntVector2>.ControlledRandomSelection(_controlledRNG, [.. potentialRoomSpawns]);




    // Down here are actual methods that are necessary for the generator

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stickToHalls"></param>
    private void UpdatePotentialRoomSpawns(bool stickToHalls)
    {
        potentialRoomSpawns = [];
        for (int i = ld.EdgeBuffer; i < levelSize.x - ld.EdgeBuffer; i++)
        {
            for (int j = ld.EdgeBuffer; j < levelSize.z - ld.EdgeBuffer; j++)
            {
                IntVector2 intVector = new(i, j);
                /* This will be uncommented later
				 * 
				if (!stickToHalls)
				{
					if (!this.ProximityCheck(intVector, RoomType.Hall, this.ld.hallBuffer) && !this.ProximityCheck(intVector, RoomType.Room, this.ld.roomBuffer) && !this.ProximityCheck(intVector, RoomType.Null, ld.EdgeBuffer))
					{
						this.potentialRoomSpawns.Add(new WeightedIntVector2());
						this.potentialRoomSpawns[this.potentialRoomSpawns.Count - 1].selection = intVector;
						this.potentialRoomSpawns[this.potentialRoomSpawns.Count - 1].weight = 1;
					}
				}
				else
				*/

                {
                    var tileNeighbors = GetTileNeighbors(intVector);
                    if (tileNeighbors.Count > 0 && FreeSpaceCheck(intVector, ld.RoomSizes.Min.x))
                    {
                        bool flag = true;
                        /*
						foreach (TileController tileController in tileNeighbors)
						{
							if (tileController.containsObject || tileController.room.category == RoomCategory.FieldTrip || tileController.ConstBin == 16)
							{
								flag = false;
								break;
							}
						}
						*/
                        if (flag)
                        {
                            potentialRoomSpawns.Add(new(intVector, WeightFromPos(intVector)));
                        }
                    }
                }
            }
        }
    }

    private List<IntVector2> GetTileNeighbors(IntVector2 position)
    {
        List<IntVector2> list = [];
        for (int i = 0; i < 4; i++)
        {
            var pos = position + ((Direction)i).ToIntVector2();
            if (mapTiles[pos.x, pos.z] >= 0)
                list.Add(pos);

        }
        return list;
    }

    public bool FreeSpaceCheck(IntVector2 position, int buffer)
    {
        bool flag = false;
        bool flag2 = false;
        IntVector2 intVector = default;
        IntVector2 intVector2 = default;
        intVector.x = 0;
        while (intVector.x > -buffer && !flag)
        {
            intVector.z = 0;
            while (intVector.z > -buffer && !flag)
            {
                int num = intVector.x;
                while (num < buffer + intVector.x && !flag2)
                {
                    int num2 = intVector.z;
                    while (num2 < buffer + intVector.z && !flag2)
                    {
                        intVector2.x = position.x + num;
                        intVector2.z = position.z + num2;
                        if (mapTiles[intVector2.x, intVector2.z] >= 0)
                        {
                            flag2 = true;
                        }
                        num2++;
                    }
                    num++;
                }
                if (!flag2)
                {
                    flag = true;
                    break;
                }
                flag2 = false;
                intVector.z--;
            }
            intVector.x--;
        }
        return flag;
    }



    private int WeightFromPos(IntVector2 pos)
    {
        return 1 + (int)Math.Round((0.5f - Math.Abs((pos.x - levelSize.x * 0.5f) / levelSize.x) + (0.5f - Math.Abs((pos.z - levelSize.z * 0.5f) / levelSize.z))) * ld.CenterWeightMultiplier) + (int)Math.Round(Math.Pow(ld.PerimeterBase, TilesFromPerimeter(pos, 1)));
    }


    private int TilesFromPerimeter(IntVector2 center, int size)
    {
        IntVector2 intVector = default;
        int amount = 0;
        for (int i = -size; i < size + 1; i++)
        {
            for (int j = -size; j < size + 1; j++)
            {
                if (Math.Abs(i) == size || Math.Abs(j) == size)
                {
                    intVector.x = i + center.x;
                    intVector.z = j + center.z;
                    if (mapTiles[intVector.x, intVector.z] >= 0)
                        amount++;

                }
            }
        }
        return amount;
    }





    private List<Direction> GetPossibleDirections(IntVector2 pos, IntVector2 size, IntVector2 maxSizes, int buffer)
    {
        List<Direction> list =
        [
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West
        ];
        IntVector2 intVector = pos;
        IntVector2 intVector2 = size;
        for (int i = 0; i < list.Count; i++)
        {
            bool flag = false;
            IntVector2 intVector3 = list[i].ToIntVector2();
            for (int j = Math.Max(intVector2.x * intVector3.x + -1 * intVector3.x - buffer * Math.Abs(intVector3.z), 0 - buffer * Math.Abs(intVector3.z)); j < Math.Max(intVector2.x * (Math.Abs(intVector3.z) + intVector3.x) + buffer * Math.Abs(intVector3.z), 1); j++)
            {
                for (int k = Math.Max(intVector2.z * intVector3.z + -1 * intVector3.z - buffer * Math.Abs(intVector3.x), 0 - buffer * Math.Abs(intVector3.x)); k < Math.Max(intVector2.z * (Math.Abs(intVector3.x) + intVector3.z) + buffer * Math.Abs(intVector3.x), 1); k++)
                {
                    IntVector2 intVector4 = new(intVector.x + j + intVector3.x + buffer * intVector3.x, intVector.z + k + intVector3.z + buffer * intVector3.z);
                    if (!mapTiles.InsideBounds(intVector4) || mapTiles[intVector4.x, intVector4.z] >= 0)
                    {
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                list.RemoveAt(i);
                i--;
            }
        }
        if (size.x >= maxSizes.x)
        {
            list.Remove(Direction.East);
            list.Remove(Direction.West);
        }
        if (size.z >= maxSizes.z)
        {
            list.Remove(Direction.North);
            list.Remove(Direction.South);
        }
        List<Direction> list2 = new(list);
        List<Direction> list3 = [];
        if (list.Count > 1 && (list.Count != 2 || list[0].GetOpposite() != list[1]))
        {
            for (int l = 0; l < list.Count; l++)
            {
                Direction direction = list[l];
                list3 = new List<Direction>(list);
                list3.Remove(direction);
                switch (direction)
                {
                    case Direction.North:
                        intVector = pos;
                        intVector2 = size + direction.ToIntVector2();
                        break;
                    case Direction.East:
                        intVector = pos;
                        intVector2 = size + direction.ToIntVector2();
                        break;
                    case Direction.South:
                        intVector = pos + direction.ToIntVector2();
                        intVector2 = size - direction.ToIntVector2();
                        break;
                    case Direction.West:
                        intVector = pos + direction.ToIntVector2();
                        intVector2 = size - direction.ToIntVector2();
                        break;
                }
                for (int m = 0; m < list3.Count; m++)
                {
                    bool flag2 = false;
                    IntVector2 intVector5 = list3[m].ToIntVector2();
                    for (int n = Math.Max(intVector2.x * intVector5.x + -1 * intVector5.x - buffer * Math.Abs(intVector5.z), 0 - buffer * Math.Abs(intVector5.z)); n < Math.Max(intVector2.x * (Math.Abs(intVector5.z) + intVector5.x) + buffer * Math.Abs(intVector5.z), 1); n++)
                    {
                        for (int num = Math.Max(intVector2.z * intVector5.z + -1 * intVector5.z - buffer * Math.Abs(intVector5.x), 0 - buffer * Math.Abs(intVector5.x)); num < Math.Max(intVector2.z * (Math.Abs(intVector5.x) + intVector5.z) + buffer * Math.Abs(intVector5.x), 1); num++)
                        {
                            IntVector2 intVector6 = new(intVector.x + n + intVector5.x + buffer * intVector5.x, intVector.z + num + intVector5.z + buffer * intVector5.z);
                            if (!mapTiles.InsideBounds(intVector6) || mapTiles[intVector6.x, intVector6.z] >= 0)
                            {
                                flag2 = true;
                            }
                        }
                    }
                    if (flag2)
                    {
                        list2.Remove(direction);
                        break;
                    }
                }
            }
            if (list2.Count > 0)
            {
                list.Clear();
                list.AddRange(list2);
            }
        }
        if (size.x > size.z && (list.Contains(Direction.North) || list.Contains(Direction.South)))
        {
            list.Remove(Direction.East);
            list.Remove(Direction.West);
        }
        else if (size.z > size.x && (list.Contains(Direction.East) || list.Contains(Direction.West)))
        {
            list.Remove(Direction.North);
            list.Remove(Direction.South);
        }
        return list;
    }

    internal void ExpandArea(IntVector2 _size, IntVector2 pos, int roomID, Direction direction, out IntVector2 newSize, out IntVector2 newPos)
    {
        IntVector2 intVector = direction.ToIntVector2();
        IntVector2 size = _size;
        IntVector2 position = pos;
        for (int i = Math.Max(size.x * intVector.x + -1 * intVector.x, 0); i < Math.Max(size.x * (Math.Abs(intVector.z) + intVector.x), 1); i++)
        {
            for (int j = Math.Max(size.z * intVector.z + -1 * intVector.z, 0); j < Math.Max(size.z * (Math.Abs(intVector.x) + intVector.z), 1); j++)
            {
                IntVector2 intVector2 = new(position.x + i + intVector.x, position.z + j + intVector.z);
                mapTiles[intVector2.x, intVector2.z] = roomID;
            }
        }
        newPos = new(pos.x + Math.Min(intVector.x, 0), pos.z + Math.Min(intVector.z, 0));
        newSize = size + new IntVector2(Math.Abs(intVector.x), Math.Abs(intVector.z));
    }
}
