using BBP_Gen.PlusGenerator;

namespace BBP_Gen.Elements
{
	public class Plot(IntVector2 pos) : IRoomStructure
	{
		public IntVector2 Pos { get; set; } = pos;
		public IntVector2 Size { get; set; } = new(1, 1);
		public IntVector2 MaxSize { get; set; } = IntVector2.MaxValue;
		public List<IntVector2> Spots { get; set; } = [];
		public RoomType Type { get; set; } = RoomType.Room;
	}
}
