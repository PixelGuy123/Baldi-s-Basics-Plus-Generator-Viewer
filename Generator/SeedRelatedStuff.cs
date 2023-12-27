namespace BBP_Gen.PlusGenerator;

public class SeedCrashException(int seed, SeedCrashType type) : Exception($"The seed \'{seed}\' has been skipped due to a crash of type: {type}")
{
}

public enum SeedCrashType : uint
{
	Null = 0,
	HallCrash = 1,
	ElevatorCrash = 2,
	MysteryRoomCrash = 3
}

[Flags]
public enum SeedType : uint
{
	None = 0,
	Normal = 1 << 0,
	Glitched = 1 << 1,
	OOB = 1 << 2,
	Uncommon = 1 << 3
}

public record SeedToken(SeedType Type, int AmountOfNotebooks, bool IsAPR = false, string[]? Data = null);
