namespace Publii.Umbraco.Extensions;

public static class ListExtensions
{
	public static IEnumerable<List<T>> SplitIntoChunks<T>(this List<T> source, int chunkSize)
	{
		for (var i = 0; i < source.Count; i += chunkSize)
		{
			yield return source.Skip(i).Take(chunkSize).ToList();
		}
	}
}