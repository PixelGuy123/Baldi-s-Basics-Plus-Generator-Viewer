using BBP_Gen.Main;

namespace BBP_Gen.Elements
{
	public class WeightedSelection<T>
	{
		public static T ControlledRandomSelection(Random rng, params WeightedSelection<T>[] items)
		{
			int num = 0;
			int num2 = 0;
			foreach (WeightedSelection<T> weightedSelection in items)
			{
				num2 += weightedSelection.weight;
			}
			int num3 = rng.Next(0, num2);
			int j;
			for (j = 0; j < items.Length; j++)
			{
				num += items[j].weight;
				if (num > num3)
				{
					break;
				}
			}
			if (j < items.Length)
			{
				return items[j].selection;
			}
			ConsoleLogger.DirectLog("No valid selection found. Returning index 0", ConsoleColor.Red);
			return items[0].selection;
		}

		public WeightedSelection(T selection, int weight) =>
			(this.selection, this.weight) = (selection, weight);

		public T selection;

		public int weight;
	}
}
