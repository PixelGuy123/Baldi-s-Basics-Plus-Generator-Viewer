namespace BBP_Gen.PlusGenerator;

[Flags]
public enum RoomType : uint // This makes a 2-base number, idk how it does it, but at least I don't have to manually type out every base-2 number
{
	None = 0,
	Room = 1 << 0,
	Hall = 1 << 1,
	SpecialRoom = 1 << 2,
	FieldTripRoom = 1 << 3,
	Buffer = 1 << 4,
	Border = 1 << 5
}
