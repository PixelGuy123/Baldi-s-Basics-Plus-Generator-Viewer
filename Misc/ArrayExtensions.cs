using BBP_Gen.Elements;
namespace BBP_Gen.Misc;

public static class ArrayExtensions
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

    public static bool InsideBounds<T>(this T[,] array, int x, int y) => x >= 0 && x < array.GetLength(0) && y >= 0 && y < array.GetLength(1);

    public static bool InsideBounds<T>(this T[,] array, IntVector2 pos) => array.InsideBounds(pos.x, pos.z);

}
