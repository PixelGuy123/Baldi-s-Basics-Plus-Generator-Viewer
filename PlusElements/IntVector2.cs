using System.Diagnostics.CodeAnalysis;

namespace BBP_Gen.Elements;

public struct IntVector2(int x , int z)
{
	public static IntVector2 operator +(IntVector2 a, IntVector2 b)
	{
		a.x += b.x;
		a.z += b.z;
		return a;
	}

	public static IntVector2 operator -(IntVector2 a, IntVector2 b)
	{
		a.x -= b.x;
		a.z -= b.z;
		return a;
	}

	public static IntVector2 operator *(IntVector2 a, int b)
	{
		a.x *= b;
		a.z *= b;
		return a;
	}

	public static bool operator ==(IntVector2 a, IntVector2 b) => a.x == b.x && a.z == b.z;
	

	public static bool operator !=(IntVector2 a, IntVector2 b) => a.x != b.x || a.z != b.z;
	

	public override readonly bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj is not IntVector2) return false;

		var vector2 = (IntVector2)obj;

		return vector2.x == x & vector2.z == z;
	}

	public override readonly int GetHashCode() => x.GetHashCode() ^ z.GetHashCode();

	public static IntVector2 ControlledRandomPosition(int minX, int maxX, int minZ, int maxZ, Random rng) => new(rng.Next(minX, maxX), rng.Next(minZ, maxZ));
	

	public override readonly string ToString() => $"{x},{z}";

	public int x = x;

	public int z = z;

	public static readonly IntVector2 MaxValue = new(int.MaxValue, int.MaxValue);
}

public static class IntVector2_Extensions
{
	public static IntVector2 CombineLowest(this IntVector2 vectorA, IntVector2 vectorB)
	{
		IntVector2 intVector = default;
		if (vectorA.x > vectorB.x)
		{
			intVector.x = vectorB.x;
		}
		else
		{
			intVector.x = vectorA.x;
		}
		if (vectorA.z > vectorB.z)
		{
			intVector.z = vectorB.z;
		}
		else
		{
			intVector.z = vectorA.z;
		}
		return intVector;
	}

	public static IntVector2 CombineGreatest(this IntVector2 vectorA, IntVector2 vectorB)
	{
		IntVector2 intVector = default;
		if (vectorA.x > vectorB.x)
		{
			intVector.x = vectorA.x;
		}
		else
		{
			intVector.x = vectorB.x;
		}
		if (vectorA.z > vectorB.z)
		{
			intVector.z = vectorA.z;
		}
		else
		{
			intVector.z = vectorB.z;
		}
		return intVector;
	}
}
