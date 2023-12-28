using BBP_Gen.Elements;
using BBP_Gen.Misc;
using System.Drawing;
using System.Numerics;

namespace BBP_Gen.PlusGenerator;

public partial class Generator // Partial class, so I can organize better, these are the methods
{
	// Down here are actual methods that are necessary for the generator (spoiler: there's a lot, I think I'll make partial classes just to divide both)
	// For the GB mods of BB, yeah, all of these methods are a Ctrl + C Ctrl+V, the difference is the adaptations I did in order to make this work


	private void UpdatePotentialRoomSpawns(bool stickToHalls)
	{
		potentialRoomSpawns = [];
		for (int i = ld.EdgeBuffer; i < levelSize.x - ld.EdgeBuffer; i++)
		{
			for (int j = ld.EdgeBuffer; j < levelSize.z - ld.EdgeBuffer; j++)
			{
				IntVector2 intVector = new(i, j);
				if (!stickToHalls)
				{
					if (!ProximityCheck(intVector, RoomType.Hall, ld.HallBuffer) && !ProximityCheck(intVector, ld.RoomBuffer, RoomType.Room, RoomType.SpecialRoom, RoomType.FieldTripRoom) && !ProximityCheck(intVector, RoomType.Buffer, ld.EdgeBuffer))
						potentialRoomSpawns.Add(new WeightedSelection<IntVector2>(intVector, 1));
					
				}
				else

				{
					var tileNeighbors = GetTileNeighbors(intVector);
					if (IsTileNull(intVector) && tileNeighbors.Count > 0 && FreeSpaceCheck(intVector, ld.RoomSizes.Min.x))
					{
						bool flag = true;
						
						foreach (var t in tileNeighbors)
						{
							if (mapTiles[t.x, t.z].HasFlag(RoomType.FieldTripRoom) || buffer[t.x, t.z] || mapTiles[t.x, t.z].HasFlag(RoomType.Elevator))
							{
								flag = false;
								break;
							}
						}
						
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
			if (mapTiles[pos.x, pos.z] != RoomType.None)
				list.Add(pos);

		}
		return list;
	}

	private bool FreeSpaceCheck(IntVector2 position, int buffer)
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
						if (mapTiles[intVector2.x, intVector2.z] != RoomType.None)
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
					if (mapTiles[intVector.x, intVector.z] != RoomType.None)
						amount++;

				}
			}
		}
		return amount;
	}


	private int CheckBigRoomSides(SpecialRoomCreator room)
	{
		List<Direction> list = [.. Directions.All()];
		IntVector2 intVector = room.Pos;
		IntVector2 intVector2 = room.Size;

		for (int i = 0; i < list.Count; i++)
		{
			bool flag = false;
			IntVector2 intVector3 = list[i].ToIntVector2();
			for (int j = Math.Max(intVector2.x * intVector3.x + -1 * intVector3.x, 0); j < Math.Max(intVector2.x * (Math.Abs(intVector3.z) + intVector3.x), 1); j++)
			{
				for (int k = Math.Max(intVector2.z * intVector3.z + -1 * intVector3.z, 0); k < Math.Max(intVector2.z * (Math.Abs(intVector3.x) + intVector3.z), 1); k++)
				{
					IntVector2 intVector4 = new(intVector.x + j + intVector3.x, intVector.z + k + intVector3.z);
					if (mapTiles.InsideBounds(intVector4) && mapTiles[intVector4.x, intVector4.z] == RoomType.Hall)
					{
						flag = true;
						break;
					}
				}
				if (flag) break;
			}
			if (!flag)
			{
				list.RemoveAt(i);
				i--;
			}
		}

		return list.Count;
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
					if (!mapTiles.InsideBounds(intVector4) || mapTiles[intVector4.x, intVector4.z] != RoomType.None)
					{
						flag = true;
						break;
					}
				}
				if (flag) break;
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
							if (!mapTiles.InsideBounds(intVector6) || mapTiles[intVector6.x, intVector6.z] != RoomType.None)
							{
								flag2 = true;
								break;
							}
						}
						if (flag2) break;
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

	private void ExpandArea(IntVector2 _size, IntVector2 pos, RoomType roomID, Direction direction, List<IntVector2> spots, out IntVector2 newSize, out IntVector2 newPos)
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
				spots.Add(intVector2);
			}
		}
		newPos = new(pos.x + Math.Min(intVector.x, 0), pos.z + Math.Min(intVector.z, 0));
		newSize = _size + new IntVector2(Math.Abs(intVector.x), Math.Abs(intVector.z));
	}

	private bool ProximityCheck(IntVector2 position, RoomType type, int buffer)
	{
		bool flag2 = false;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < buffer; i++)
		{
			if (flag2)
			{
				num2++;
			}
			else
			{
				num++;
			}
			flag2 = !flag2;
		}
		for (int j = position.x - num; j <= position.x + num2; j++)
		{
			for (int k = position.z - num; k <= position.z + num2; k++)
			{
				if (mapTiles.InsideBounds(j, k) && mapTiles[j, k].HasFlag(type))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool ProximityCheck(IntVector2 position, int buffer, params RoomType[] types) // Just so I don't have to call this method 3 times, so just put in an array
	{
		bool flag2 = false;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < buffer; i++)
		{
			if (flag2)
			{
				num2++;
			}
			else
			{
				num++;
			}
			flag2 = !flag2;
		}
		for (int j = position.x - num; j <= position.x + num2; j++)
		{
			for (int k = position.z - num; k <= position.z + num2; k++)
			{
				if (mapTiles.InsideBounds(j, k) && types.Any(x => mapTiles[j, k].HasFlag(x)))
				{
					return true;
				}
			}
		}
		return false;
	}

	private List<IntVector2> MatchingAdjacentTiles(IntVector2 tile)
	{
		List<IntVector2> list = [];
		for (int i = 0; i < 4; i++)
		{
			IntVector2 intVector = tile + Directions.FromInt(i).ToIntVector2();
			if (mapTiles.InsideBounds(intVector) && mapTiles[intVector.x, intVector.z] != RoomType.None && mapTiles[intVector.x, intVector.z] == mapTiles[tile.x, tile.z])
				list.Add(intVector);
			
		}
		return list;
	}

	private List<Direction> DirectionsToDestination(IntVector2 start, IntVector2 end)
	{
		List<Direction> list = [];
		if (start.x > end.x)
		{
			list.Add(Direction.West);
		}
		else if (start.x < end.x)
		{
			list.Add(Direction.East);
		}
		if (start.z > end.z)
		{
			list.Add(Direction.South);
		}
		else if (start.z < end.z)
		{
			list.Add(Direction.North);
		}
		return list;
	}

	private bool TileInDirectionCheck(IntVector2 pos, Direction dir, int distance)
	{
		for (int i = 1; i <= distance; i++)
		{
			var npos = pos + dir.ToIntVector2() * i;

			if (mapTiles.InsideBounds(npos) && mapTiles[npos.x, npos.z] != RoomType.None)
				return true;
		}
		return false;
	}

	private void AddHall(IntVector2 pos)
	{
		mapTiles[pos.x, pos.z] = RoomType.Hall;
		roomTiles[pos.x, pos.z] = hall;
		hall.Spots.Add(pos);
		halls.Add(pos);
	}

	private List<Direction> PotentialPathDirections(IntVector2 pos)
	{
		List<Direction> list = [];
		for (int i = 0; i < 4; i++)
		{
			IntVector2 intVector = pos + Directions.FromInt(i).ToIntVector2();
			if (mapTiles.InsideBounds(intVector) && mapTiles[intVector.x, intVector.z] == RoomType.None && DirectionSafe(pos, (Direction)i, ((Direction)i).GetOpposite()))
			{
				list.Add(Directions.FromInt(i));
			}
		}
		return list;
	}

	private bool DirectionSafe(IntVector2 initPos, Direction dir, Direction noDir)
	{
		IntVector2 intVector = dir.ToIntVector2();
		IntVector2 intVector2 = initPos + intVector;
		if (mapTiles.InsideBounds(intVector2) && ((mapTiles[intVector2.x, intVector2.z] != RoomType.None && !buffer[intVector2.x, intVector2.z]) || mapTiles[intVector2.x, intVector2.z] == RoomType.None))
		{
			int num = Math.Max(0, intVector2.x * intVector.x);
			int num2 = Math.Max(intVector2.x, (levelSize.x - 1) * (intVector.x + Math.Abs(intVector.z)));
			int num3 = Math.Max(0, intVector2.z * intVector.z);
			int num4 = Math.Max(intVector2.z, (levelSize.z - 1) * (intVector.z + Math.Abs(intVector.x)));
			switch (noDir)
			{
				case Direction.North:
					num4 = intVector2.z;
					break;
				case Direction.East:
					num2 = intVector2.x;
					break;
				case Direction.South:
					num3 = intVector2.z;
					break;
				case Direction.West:
					num = intVector2.x;
					break;
			}
			for (int i = num; i <= num2; i++)
			{
				for (int j = num3; j <= num4; j++)
				{
					if (mapTiles.InsideBounds(i, j) && mapTiles[i, j] != RoomType.None)
						return true;
				}
			}
		}
		return false;
	}


	private Queue<IntVector2> GetRandomPath(IntVector2 start, int turnChance, out bool success)
	{
		Queue<IntVector2> queue = new();
		success = false;
		List<Direction> list = new(PotentialPathDirections(start));
		CheckDirections(start, list, Direction.Null);
		if (list.Count > 0)
		{
			Direction direction = list[_controlledRNG.Next(0, list.Count)];
			Direction opposite = direction.GetOpposite();
			IntVector2 intVector = start + direction.ToIntVector2();
			int num = 0;
			while (mapTiles[intVector.x, intVector.z] == RoomType.None)
			{
				queue.Enqueue(intVector);
				list = [];
				for (int i = 0; i < 4; i++)
				{
					if (Directions.FromInt(i) != opposite && Directions.FromInt(i) != direction.GetOpposite())
					{
						list.Add(Directions.FromInt(i));
					}
				}
				CheckDirections(intVector, list, opposite);
				if (list.Contains(direction))
				{
					if (list.Count > 1 && num * turnChance > _controlledRNG.Next(0, 100) + 1)
					{
						list.Remove(direction);
						direction = list[_controlledRNG.Next(0, list.Count)];
						num = 0;
					}
				}
				else if (list.Count > 0)
				{
					direction = list[_controlledRNG.Next(0, list.Count)];
				}
				if (list.Count <= 0)
				{
					break;
				}
				intVector += direction.ToIntVector2();
				num++;
			}
			if (mapTiles[intVector.x, intVector.z] != RoomType.None && !buffer[intVector.x, intVector.z])
				success = true;
			
		}
		return queue;
	}

	private void CheckDirections(IntVector2 pos, List<Direction> dirs, Direction noDir)
	{
		for (int i = 0; i < dirs.Count; i++)
		{
			if (!DirectionSafe(pos, dirs[i], noDir))
			{
				dirs.RemoveAt(i);
				i--;
			}
		}
	}


	private bool ElevatorSpotFits(IntVector2 pos, Direction dir, RoomType type)
	{
		var frontpos = pos + dir.ToIntVector2();
		var moreFrontPos = pos + (dir.ToIntVector2() * 2);
		return IsTileNull(frontpos) && IsTileNull(frontpos + dir.PerpendicularList()[0].ToIntVector2()) && IsTileNull(frontpos + dir.PerpendicularList()[1].ToIntVector2()) && IsTileNull(pos)
			&& IsTileEqual(moreFrontPos, type) && IsTileEqual(moreFrontPos + dir.PerpendicularList()[0].ToIntVector2(), type) && IsTileEqual(moreFrontPos + dir.PerpendicularList()[1].ToIntVector2(), type);
	}

	private bool IsTileNotNull(IntVector2 pos) => mapTiles.InsideBounds(pos) && mapTiles[pos.x, pos.z] != RoomType.None;

	private bool IsTileNull(IntVector2 pos) => mapTiles.InsideBounds(pos) && mapTiles[pos.x, pos.z] == RoomType.None;

	private bool IsTileEqual(IntVector2 pos, RoomType match) => IsTileNotNull(pos) && match.HasFlag(mapTiles[pos.x, pos.z]);

	private void CreateElevator(IntVector2 pos, Direction dir, bool isSpawn)
	{
		if (isSpawn)
			spawnSpot = pos;

		mapTiles[pos.x, pos.z] = RoomType.Elevator;
		pos += dir.ToIntVector2();
		mapTiles[pos.x, pos.z] = RoomType.Elevator;
		var left = pos + dir.PerpendicularList()[0].ToIntVector2();
		if (IsTileEqual(left + dir.ToIntVector2(), RoomType.Elevator))
			uncommonTags[0] = true;

		mapTiles[left.x, left.z] = RoomType.Elevator;

		var right = pos + dir.PerpendicularList()[1].ToIntVector2();
		if (IsTileEqual(right + dir.ToIntVector2(), RoomType.Elevator))
			uncommonTags[0] = true;

		mapTiles[right.x, right.z] = RoomType.Elevator;
	}

	private RoomType[] RoomProximityList(IRoomStructure room, int buffer)
	{
		HashSet<RoomType> list = [];
		var list2 = Directions.All();
		IntVector2 position = room.Pos;
		IntVector2 size = room.Size;
		for (int i = 0; i < list2.Length; i++)
		{
			IntVector2 intVector = list2[i].ToIntVector2();
			for (int j = Math.Max(size.x * intVector.x + -1 * intVector.x - buffer * Math.Abs(intVector.z), 0 - buffer * Math.Abs(intVector.z)); j < Math.Max(size.x * (Math.Abs(intVector.z) + intVector.x) + buffer * Math.Abs(intVector.z), 1); j++)
			{
				for (int k = Math.Max(size.z * intVector.z + -1 * intVector.z - buffer * Math.Abs(intVector.x), 0 - buffer * Math.Abs(intVector.x)); k < Math.Max(size.z * (Math.Abs(intVector.x) + intVector.z) + buffer * Math.Abs(intVector.x), 1); k++)
				{
					IntVector2 intVector2 = new(position.x + j + intVector.x + buffer * intVector.x, position.z + k + intVector.z + buffer * intVector.z);
					if (IsTileNotNull(intVector2) && !list.Contains(mapTiles[intVector2.x, intVector2.z]))
						list.Add(mapTiles[intVector2.x, intVector2.z]);
					
				}
			}
		}
		return [.. list];
	}

	private IntVector2[] RoomProximityList_Ref(IRoomStructure room, int buffer)
	{
		List<IntVector2> list = [];
		var list2 = Directions.All();
		IntVector2 position = room.Pos;
		IntVector2 size = room.Size;
		for (int i = 0; i < list2.Length; i++)
		{
			IntVector2 intVector = list2[i].ToIntVector2();
			for (int j = Math.Max(size.x * intVector.x + -1 * intVector.x - buffer * Math.Abs(intVector.z), 0 - buffer * Math.Abs(intVector.z)); j < Math.Max(size.x * (Math.Abs(intVector.z) + intVector.x) + buffer * Math.Abs(intVector.z), 1); j++)
			{
				for (int k = Math.Max(size.z * intVector.z + -1 * intVector.z - buffer * Math.Abs(intVector.x), 0 - buffer * Math.Abs(intVector.x)); k < Math.Max(size.z * (Math.Abs(intVector.x) + intVector.z) + buffer * Math.Abs(intVector.x), 1); k++)
				{
					IntVector2 intVector2 = new(position.x + j + intVector.x + buffer * intVector.x, position.z + k + intVector.z + buffer * intVector.z);
					if (roomTiles.InsideBounds(intVector2) && roomTiles.GetItem(intVector2) != null)
						list.Add(intVector2);

				}
			}
		}
		return [.. list];
	}




	static Vector2 RealRoomMin(Room room)
	{
		return new Vector2(room.Pos.x * 10f, room.Pos.z * 10f);
	}
	static Vector2 RealRoomMax(Room room)
	{
		return new Vector2(room.Pos.x * 10f + room.Size.x * 10f, room.Pos.z * 10f + room.Size.z * 10f);
	}
	static Vector2 RealRoomSize(Room room)
	{
		return RealRoomMax(room) - RealRoomMin(room);
	}

	static Vector2 RealRoomMid(Room room)
	{
		return RealRoomSize(room) / 2f + RealRoomMin(room);
	}




	// Only triggers without glitched seed requirement
	private void AddRandomDoor(Room room, bool oneDoorPerRoom, bool oneDirPerRoom) // Don't ask me if this is right, idk...
	{
		List<IntVector2> list = new(RoomProximityList_Ref(room, 0));
		if (list.Count > 0)
		{
            List<IntVector2> list2 = FilterDoorPotents([.. list], room, oneDoorPerRoom, oneDirPerRoom);
            if (list2.Count > 0)
			{
                IntVector2 tileController = list2[_controlledRNG.Next(list2.Count)];
				var tileRoom = roomTiles.GetItem(tileController);

				room.AdjacentRooms.Add(tileRoom);
				tileRoom.AdjacentRooms.Add(room);
				Directions.All().Do((dir) =>
				{
					IntVector2 intVector2 = tileController + dir.ToIntVector2();
					if (roomTiles.InsideBounds(intVector2) && roomTiles.GetItem(intVector2) != null)
					{
						Room tileController2 = roomTiles[intVector2.x, intVector2.z];
						if (tileController2 == room)
						{
							tileRoom.DoorDirs.Add(dir);
							tileController2.DoorDirs.Add(dir.GetOpposite());
						}
					}
				});
			}
		}
	}

	private List<IntVector2> FilterDoorPotents(IntVector2[] list, Room room, bool oneDoorPerRoom, bool oneDirPerRoom)
	{
		List<IntVector2> list2 = [.. list];
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < list2.Count; i++)
		{
			var selRoom = roomTiles.GetItem(list2[i]);
			if ((oneDoorPerRoom && room.AdjacentRooms.Contains(selRoom)) || selRoom.Type == RoomType.Elevator || selRoom.Type == RoomType.FieldTripRoom)
			{
				list2.RemoveAt(i);
				i--;
			}
		}
		for (int j = 0; j < list2.Count; j++)
		{
			if (roomTiles.GetItem(list2[j]).Type == RoomType.Hall)
			{
				flag = true;
			}
			else if (roomTiles.GetItem(list2[j]).ConnectedToHall)
			{
				flag2 = true;
			}
		}
		if (oneDirPerRoom)
		{
			for (int k = 0; k < list2.Count; k++)
			{
				for (int i = 0; i < room.DoorDirs.Count; i++)
				{

					IntVector2 intVector = list2[k] + room.DoorDirs[i].GetOpposite().ToIntVector2();
                    if (IsTileNotNull(intVector) && roomTiles[intVector.x, intVector.z] == room)
					{
						list2.RemoveAt(k);
						k--;
						break;
					}
				}
			}
		}
		if (flag)
		{
			for (int l = 0; l < list2.Count; l++)
			{
				if (roomTiles.GetItem(list2[l]).Type != RoomType.Hall)
				{
					list2.RemoveAt(l);
					l--;
				}
			}
		}
		else if (flag2)
		{
			for (int m = 0; m < list2.Count; m++)
			{
				if (!roomTiles.GetItem(list2[m]).ConnectedToHall && !room.ConnectedToHall)
				{
					list2.RemoveAt(m);
					m--;
				}
			}
		}
		return list2;
	}



}
