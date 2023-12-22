using BBP_Gen.Elements;

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
WeightedSelection<SpecialRoomCreator>[] SpecialRooms,
int HallBuffer,
int RoomBuffer,
int MinPlotSize,
int BridgeTurnChance,
int MaxHallAttempts,
int AdditionTurnChance,
int DeadEndBuffer,
int ExitCount,
LevelObject[] PreviousLevels,
WeightedSelection<string>[] PotentialNPCs,
string[] ForcedNpcs,
int AdditionalNPCs,
MinMax<int> ClassRooms,
MinMax<int> Faculties,
MinMax<int> Offices,
float ClassDistanceWeightExponent,
int PassThroughChance);
