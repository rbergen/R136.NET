using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.General
{
	public class ContinuationStatus
	{
		public IContinuable Continuable { get; }
		public object Data { get; }

		public ContinuationStatus(IContinuable continuable, object data) => (Continuable, Data) = (continuable, data);
	}


	public interface IContinuable
	{
		public void Continue(object statusData, string input);
	}
}
