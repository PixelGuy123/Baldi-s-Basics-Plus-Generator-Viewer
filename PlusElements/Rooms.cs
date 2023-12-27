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
		public List<Room> AdjacentRooms { get; } = [];
		public List<Direction> DoorDirs { get; } = [];
		public bool ConnectedToHall { get => AdjacentRooms.Any(x => x.Type == RoomType.Hall); }
		public bool ConnectedToRoom { get => AdjacentRooms.Any(x => x.Type != RoomType.Hall); }
	}
}
