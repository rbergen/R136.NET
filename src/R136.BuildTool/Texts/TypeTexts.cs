using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.BuildTool.Texts
{
    public class TypeTexts
    {
        public string Type { get; set; } = string.Empty;
        public Dictionary<int, string[]>? Texts { get; set; }
    }
}
