using BBP_Gen.PlusGenerator;

namespace BBP_Gen.Elements;

public abstract class RandomEvent(string name)
{
	public virtual void Initialize(Random rng)
	{
		crng = new Random(rng.Next());
	}

	public virtual void ClaimARoom(List<WeightedSelection<Room>> rooms, Generator gen)
	{
	}

	public string Name { get; } = name;

	protected Random crng = new();
}

public class PartyEvent() : RandomEvent("Party Event")
{
	public override void Initialize(Random rng)
	{
		base.Initialize(rng);
		rng.Next(); // Just one actually lol
	}
}

public class GenericEvent(string name) : RandomEvent(name);

public class MysteryRoomEvent() : RandomEvent("Mystery Room")
{
	public override void ClaimARoom(List<WeightedSelection<Room>> rooms, Generator gen)
	{
		List<WeightedSelection<Room>> list = new(rooms);
		Room? room = null;

		while (list.Count > 0 && room == null)
		{
			int num = crng.Next(0, list.Count);
			room = list[num].selection;
			list.RemoveAt(num);
			if (room.AdjacentRooms.Any(x => x != RoomType.Hall))
				room = null;
			
		}

		if (room != null)
		{
            room.Type = RoomType.SpecialRoom; // Yeah, special room to become white, and also because there won't be any difference anyways
			gen.UpdateTiles(room);

			for (int i = 0; i < rooms.Count; i++)
			{
				if (rooms[i].selection == room)
				{
					rooms.RemoveAt(i);
					return;
				}
			}
			return;
		}

		throw new SeedCrashException(gen.Seed, SeedCrashType.MysteryRoomCrash); // If it fails to get a room
	}
}
