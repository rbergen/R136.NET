using System.Threading.Tasks;

namespace R136.Interfaces
{
	public interface IEntityReader
	{
		public Task<TEntity> ReadEntity<TEntity>(string? groupLabel, string label);
	}
}
