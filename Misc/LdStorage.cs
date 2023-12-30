﻿using BBP_Gen.Elements;
using BBP_Gen.PlusGenerator;

namespace BBP_Gen.Misc;

internal static class LdStorage // Stores Level Objects
{
	public readonly static (LevelObject, int) Floor1 = (new(
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
		[new(new(new(new(10, 10), new(15, 15)), "Cafeteria", acceptExits: true), 100), new(new(new(new(12, 12), new(18, 18)), "Playground"), 50)], // Lots of news lmfao
		4,
		6,
		5,
		2,
		3,
		5,
		6,
		1,
		[],
		[new("Arts and Crafters", 100), new("Bully", 75), new("Chalkles", 75), new("Cloudy Copter", 100), new("Gotta Sweep", 100), new("Beans", 100), new("First Prize", 25)],
		["Principal"],
		3,
		new(4, 4),
		new(4, 5),
		new(1, 1),
		1.5f,
		15
		), 0);

	public readonly static (LevelObject, int) Floor2 = (new(new MinMax<IntVector2>(new(22, 27), new(30, 35)),
		5,
		new MinMax<int>(4, 6),
		new MinMax<int>(1, 2),
		new MinMax<int>(1, 2),
		new MinMax<int>(2, 2),
		true,
		new MinMax<int>(1, 1),
		true,
		3,
		new MinMax<IntVector2>(new(4, 5), new(6, 7)),
		25f,
		4f, // Huge list of events
		[new(new GenericEvent("Broken Ruler"), 50), new(new PartyEvent(), 50), new(new GenericEvent("Flood"), 100), new(new MysteryRoomEvent(), 75), new(new GenericEvent("Gravity Chaos"), 75)],
		[new(new(new(new(10, 10), new(16, 16)), "Library"), 100), new(new(new(new(12, 12), new(18, 18)), "Playground"), 100), new(new(new(new(10, 10), new(16, 16)), "Library"), 100)], // This is messed up lmao
		4,
		6,
		5,
		2,
		3,
		5,
		6,
		2,
		[Floor1.Item1],
		[new("Arts and Crafters", 75), new("Bully", 100), new("Chalkles", 100), new("Cloudy Copter", 75), new("Gotta Sweep", 100), new("Playtime", 100), new("Beans", 100), new("Mrs Pomp", 75), new("First Prize", 75)],
		[],
		1,
		new(7, 7),
		new(5, 6),
		new(1, 1),
		1.5f,
		15), 1);


	public readonly static (LevelObject, int) Floor3 = (new(new MinMax<IntVector2>(new(30, 35), new(40, 45)),
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
		4,
		[Floor1.Item1, Floor2.Item1],
		[new("Arts and Crafters", 75), new("Bully", 100), new("Chalkles", 100), new("Cloudy Copter", 75), new("Gotta Sweep", 100), new("Playtime", 100), new("Beans", 100), new("Mrs Pomp", 150), new("The Test", 100), new("First Prize", 100)],
		[],
		1,
		new(9, 9),
		new(7, 8),
		new(1, 1),
		1.5f,
		15), 2);

	public readonly static (LevelObject, int) END = (new(new MinMax<IntVector2>(new(22, 27), new(30, 35)),
		5,
		new MinMax<int>(4, 6),
		new MinMax<int>(1, 2),
		new MinMax<int>(1, 2),
		new MinMax<int>(4, 4),
		false,
		new MinMax<int>(1, 1),
		true,
		3,
		new MinMax<IntVector2>(new(4, 5), new(6, 7)),
		25f,
		4f, // Huge list of events
		[new(new GenericEvent("Broken Ruler"), 50), new(new PartyEvent(), 50), new(new GenericEvent("Flood"), 100), new(new MysteryRoomEvent(), 75), new(new GenericEvent("Gravity Chaos"), 75), new(new GenericEvent("Fog"), 75)],
		[new(new(new(new(10, 10), new(16, 16)), "Library"), 100), new(new(new(new(12, 12), new(18, 18)), "Playground"), 100), new(new(new(new(10, 10), new(16, 16)), "Library"), 100)],
		4,
		6,
		5,
		2,
		3,
		5,
		6,
		1,
		[], // No previous levels
		[new("Arts and Crafters", 75), new("Bully", 100), new("Chalkles", 100), new("Cloudy Copter", 75), new("Gotta Sweep", 100), new("Playtime", 100), new("Beans", 100), new("Mrs Pomp", 75), new("The Test", 25), new("First Prize", 75)],
		["Principal of the Thing"],
		5,
		new(7, 7),
		new(5, 6),
		new(1, 1),
		1.5f,
		15), 0);
}