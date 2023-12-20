using System;
using BBP_Gen.PlusGenerator;

namespace BBP_Gen.Elements
{
	public interface IRoomStructure // not implemented on the game, just to make the communication with room types easier (like plots, special rooms, etc.)
	{
		IntVector2 Size { get; set; }
		IntVector2 Pos { get; set; }
		IntVector2 MaxSize { get; set; }
		List<IntVector2> Spots { get; set; }
		RoomType Type { get; set; }
	}
}
