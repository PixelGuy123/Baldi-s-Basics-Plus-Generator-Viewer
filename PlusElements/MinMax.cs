using System;

namespace BBP_Gen.Elements;

public struct MinMax<T> // Not actually from Plus, but just to simplify the min and max values
{
	public MinMax(T min, T max) => (Min, Max) = (min, max);

	public T Min { get; set; }

	public T Max { get; set; }
}
