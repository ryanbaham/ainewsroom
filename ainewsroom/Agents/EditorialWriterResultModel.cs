using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ainewsroom.Agents
{
    public class EditorialWriterResultModel
    {

        public string headline { get; set; }
        public string date { get; set; }
        public string lede { get; set; }
        public string body { get; set; }
        public string call_to_action { get; set; }
        public Citation[] citations { get; set; }
        
    }
    public class Citation
    {
        public int id { get; set; }
        public string url { get; set; }
    }
}
