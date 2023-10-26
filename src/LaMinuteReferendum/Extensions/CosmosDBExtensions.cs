using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace MonReferendum.Extensions;

public static class CosmosDBExtensions
{
	public static async Task<T?> FindOneAsync<T>(this Container container, Expression<Func<T, bool>>? predicate = null) where T : class
	{
		using var feed = predicate == null ? container.GetItemLinqQueryable<T>().ToFeedIterator() : container.GetItemLinqQueryable<T>().Where(predicate).ToFeedIterator();

		if (feed.HasMoreResults)
		{
			var item = await feed.ReadNextAsync();
			return item.FirstOrDefault();
		}
		else
		{
			return default;
		}
	}
}
