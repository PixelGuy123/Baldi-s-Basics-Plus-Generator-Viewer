using BBP_Gen.PlusGenerator;

namespace BBP_Gen.Elements
{
	public class Room : IRoomStructure
	{
		public RoomType Type { get; set; } = RoomType.Room;
		public List<IntVector2> Spots { get; set; } = [];
		public IntVector2 Size { get; set; } = new IntVector2(1, 1);
		public IntVector2 Pos { get; set; } = default;
		public IntVector2 MaxSize { get; set; }
		public List<RoomType> AdjacentRooms { get; } = [];
		public List<Direction> DoorDirs { get; } = [];
	}
}
