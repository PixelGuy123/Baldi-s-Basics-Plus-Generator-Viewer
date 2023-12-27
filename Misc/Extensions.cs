using BBP_Gen.Elements;
namespace BBP_Gen.Misc;

public static class Extensions
{
    public static void Do<T>(this IEnumerable<T> array, Action<T> act)
    {
        foreach (var item in array)
        {
            act(item);
        }
    }

    public static T[,] Reverse2DArray<T>(this T[,] array)
    {
        var rArray = new T[array.GetLength(1), array.GetLength(0)]; // Invert grid to correct display

        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                rArray[j, i] = array[i, j];
            }
        }

        return rArray;
    }

	public static void WriteEverythingOnLine<T>(this IEnumerable<T> collection) => collection.Do(x => Console.WriteLine(x));

	public static void WriteEverythingOnLine<T>(this IEnumerable<T> collection, string prefix) => collection.Do(x => Console.WriteLine("{0}{1}", prefix, x));

	public static bool InsideBounds<T>(this T[,] array, int x, int y) => x >= 0 && x < array.GetLength(0) && y >= 0 && y < array.GetLength(1);

    public static bool InsideBounds<T>(this T[,] array, IntVector2 pos) => array.InsideBounds(pos.x, pos.z);
	public static T GetItem<T>(this T[,] array, IntVector2 pos) => array[pos.x, pos.z];
	public static T GetItem<T>(this T[,] array, int x, int y) => array[x, y];

	public static Room AsRoom(this SpecialRoomCreator creator) => new()
	{
		Size = creator.Size,
		Pos = creator.Pos,
		MaxSize = creator.MaxSize,
		Spots = creator.Spots,
		Type = PlusGenerator.RoomType.Room
	};

}
