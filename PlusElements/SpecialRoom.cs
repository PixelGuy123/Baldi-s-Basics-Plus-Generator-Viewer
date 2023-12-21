using BBP_Gen.PlusGenerator;

namespace BBP_Gen.Elements
{
    public struct SpecialRoomCreator(MinMax<IntVector2> RandomSizes, string name, bool stickToHalls = true, bool acceptExits = false) : IRoomStructure // Apparently putting as struct works aswell, neat.
	{
		private IntVector2 size = new(1, 1); // actual size
		private MinMax<IntVector2> _rSizes = RandomSizes; // The random sizes available
		private IntVector2 maxSizes = default;
		private IntVector2 pos = default;
		private readonly bool _stickHalls = stickToHalls;
		private Random cRNG = new(); // RNG
		private readonly string name = name;
		private Generator? gen;

		public bool AcceptExits { get; set; } = acceptExits;

		public IntVector2 Size { readonly get => size; set => size = value; } // get from here

		public RoomType Type { readonly get; set; } = RoomType.SpecialRoom;

		public IntVector2 MaxSize { readonly get => maxSizes; set => maxSizes = value; }

		public List<IntVector2> Spots { get; set; } = [];

		public IntVector2 Pos { readonly get => pos; set => pos = value; }

		public readonly string Name => name;

		public readonly bool StickToHalls => _stickHalls;

		public void SetReferences(Generator gen) => this.gen = gen;

		public void SetRandomValues(Random rng) => cRNG = new Random(rng.Next());

		public void Initialize()
		{
			SetSize();
			if (gen is not null) // Nuh uh, it can't be null
			{
				pos = gen.RandomRoomSpawn;
				gen.AddNewArea(this, pos);
			}
		}

		public void SetSize()
		{
			if (maxSizes.x == 0)
			{
				maxSizes.x = cRNG.Next(_rSizes.Min.x, _rSizes.Max.z);
				maxSizes.z = cRNG.Next(_rSizes.Min.z, _rSizes.Max.z);
				if (cRNG.Next(0, 2) == 1)
					(maxSizes.z, maxSizes.x) = (maxSizes.x, maxSizes.z); // Invert vals
				
			}
		}

	}
}
