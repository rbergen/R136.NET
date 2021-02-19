using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Interfaces
{
	public interface IEntityReader
	{
		public Task<TEntity> ReadEntity<TEntity>(string label);
	}
}
