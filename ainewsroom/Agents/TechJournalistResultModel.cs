using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ainewsroom.Agents
{

    public class TechJournalistResultModel : IAgentResult
    {
        public string resultname => "TechResult";
        public string date { get; set; }
        public Column[] columns { get; set; }
        public JournalistCitation[] citations { get; set; }
    }

    public class Column
    {
        public string theme { get; set; }
        public string headline { get; set; }
        public string lede { get; set; }
        public string[] body { get; set; }
        public int[] source_ids { get; set; }
    }

    public class JournalistCitation
    {
        public int id { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string publisher { get; set; }
        public string date { get; set; }
        public string type { get; set; }
    }




}
