using BBP_Gen.Elements;
using BBP_Gen.PlusGenerator;

namespace BBP_Gen.Misc;

internal static class LdStorage // Stores Level Objects
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
        [new(new(new(new(10, 10), new(15, 15)), "Cafeteria", acceptExits:true), 100), new(new(new(new(12, 12), new(18, 18)), "Playground"), 100)], // Lots of news lmfao
		4,
		6,
		5,
		2,
		3,
		5,
		6,
        1);

	public readonly static LevelObject Floor3 = new(new MinMax<IntVector2>(new(30, 35), new(40, 45)),
		5,
		new MinMax<int>(5, 8),
		new MinMax<int>(3, 6),
		new MinMax<int>(3, 5),
		new MinMax<int>(3, 4),
		false,
		new MinMax<int>(1, 1),
		false,
		3,
		new MinMax<IntVector2>(new(4, 5), new(6, 7)),
		25f,
		4f, // Huge list of events
		[new(new GenericEvent("Broken Ruler"), 100), new(new PartyEvent(), 50), new(new GenericEvent("Fog"), 75), new(new MysteryRoomEvent(), 25), new(new GenericEvent("Test Procedure"), 100), new(new GenericEvent("Gravity Chaos"), 75)],
		[new(new(new(new(10, 10), new(16, 16)), "Library"), 100), new(new(new(new(12,12),new(18,18)), "Playground"), 100), new(new(new(new(10, 10), new(15, 15)), "Cafeteria", acceptExits:true), 100)], // This is messed up lmao
		4,
		6,
		5,
		2,
		3,
		5,
		6,
		4);
}