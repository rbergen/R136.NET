using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Interfaces
{
	public interface ISnappable<TSnapshot, TId> 
		where TSnapshot : class, ISnapshot<TId>
		where TId : struct
	{
		TSnapshot TakeSnapshot(TSnapshot? snapshot = null);

		bool RestoreSnapshot(TSnapshot snapshot);
	}

	public interface ISnapshot<TId> where TId : struct
	{
		TId ID { get; set; }
	}
}
