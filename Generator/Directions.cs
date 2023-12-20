using BBP_Gen.Elements;

namespace BBP_Gen.PlusGenerator
{
	public enum Direction
	{
		North,
		East,
		South,
		West,
		Null
	}

	public static class Directions
	{
		public static Direction ControlledRandomDirection(Random rng)
		{
			return (Direction)rng.Next(0, 4);
		}
		public static Direction FromInt(int i)
		{
			return (Direction)i;
		}

		public static IntVector2[] Vectors
		{
			get
			{
				return vectors;
			}
		}
		public static IntVector2 ToIntVector2(this Direction direction)
		{
			return vectors[(int)direction];
		}

		public static Direction GetOpposite(this Direction direction)
		{
			return opposites[(int)direction];
		}

		public static List<Direction> PerpendicularList(this Direction dir)
		{
			List<Direction> list = [];
			if (dir == Direction.North)
			{
				list.Add(Direction.East);
				list.Add(Direction.West);
			}
			else if (dir == Direction.East)
			{
				list.Add(Direction.North);
				list.Add(Direction.South);
			}
			else if (dir == Direction.South)
			{
				list.Add(Direction.East);
				list.Add(Direction.West);
			}
			else if (dir == Direction.West)
			{
				list.Add(Direction.North);
				list.Add(Direction.South);
			}
			return list;
		}

		public static List<Direction> OpenDirectionsFromBin(int bin)
		{
			List<Direction> list = [];
			for (int i = 0; i < 4; i++)
			{
				if ((bin & 1 << i) == 0)
				{
					list.Add((Direction)i);
				}
			}
			return list;
		}

		public static void FillOpenDirectionsFromBin(List<Direction> list, int bin)
		{
			list.Clear();
			for (int i = 0; i < 4; i++)
			{
				if ((bin & 1 << i) == 0)
				{
					list.Add((Direction)i);
				}
			}
		}

		public static List<Direction> ClosedDirectionsFromBin(int bin)
		{
			List<Direction> list = [];
			for (int i = 0; i < 4; i++)
			{
				if ((bin & 1 << i) > 0)
				{
					list.Add((Direction)i);
				}
			}
			return list;
		}

		public static void FillClosedDirectionsFromBin(List<Direction> list, int bin)
		{
			list.Clear();
			for (int i = 0; i < 4; i++)
			{
				if ((bin & 1 << i) > 0)
				{
					list.Add((Direction)i);
				}
			}
		}
		public static bool ContainsDirection(this int val, Direction direction)
		{
			return (val & 1 << direction.Bit()) > 0;
		}

		public static Direction[] All()
		{
			return
			[
				Direction.North,
				Direction.East,
				Direction.South,
				Direction.West
			];
		}

		public static void ReverseList(List<Direction> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = list[i].GetOpposite();
			}
		}

		public static int Bit(this Direction dir)
		{
			return dir switch
			{
				Direction.North => 0,
				Direction.East => 1,
				Direction.South => 2,
				Direction.West => 3,
				Direction.Null => -1,
				_ => 0,
			};
		}

		public const int Count = 4;

		private readonly static IntVector2[] vectors =
		[
			new IntVector2(0, 1),
			new IntVector2(1, 0),
			new IntVector2(0, -1),
			new IntVector2(-1, 0)
		];

		private readonly static Direction[] opposites =
		[
			Direction.South,
			Direction.West,
			Direction.North,
			Direction.East
		];
	}
}
