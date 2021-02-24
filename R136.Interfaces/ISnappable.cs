using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Interfaces
{
	public interface ISnappable<TSnapshot> 
		where TSnapshot : class
	{
		TSnapshot TakeSnapshot(TSnapshot? snapshot = null);

		bool RestoreSnapshot(TSnapshot snapshot);
	}
}
