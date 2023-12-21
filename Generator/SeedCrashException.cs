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
