﻿namespace BBP_Gen.PlusGenerator;

public class SeedCrashException(string crashType) : Exception(crashType)
{
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

public record SeedToken(bool Generated, SeedType Type, int AmountOfNotebooks, bool IsAPR = false, bool HasMirrorFeatures = false, string[]? Data = null)
{
	public static readonly SeedToken None = new(false, SeedType.None, 0);
}
