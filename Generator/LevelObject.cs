﻿using BBP_Gen.Elements;

namespace BBP_Gen.PlusGenerator;

// Some necessary stuff down here

public record LevelObject( // Level Object, this will be huge
MinMax<IntVector2> LevelSizes,
int OuterEdgeBuffer,
MinMax<int> PlotCount,
MinMax<int> HallRemovalCount,
MinMax<int> HallsAddCount,
MinMax<int> EventCount,
bool FieldTrip,
MinMax<int> SpecialRoomCount,
bool IncludeBuffer,
int EdgeBuffer,
MinMax<IntVector2> RoomSizes,
float CenterWeightMultiplier,
float PerimeterBase,
    WeightedSelection<RandomEvent>[] RandomEvents,
    WeightedSelection<SpecialRoomCreator>[] SpecialRooms);
