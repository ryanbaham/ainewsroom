using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ainewsroom.Agents
{

    public class ResearchAnalystResultModel : IAgentResult
    {
        public string resultname => "ResearchResult";
        public string cutoff_date { get; set; }
        public Theme[] themes { get; set; }
        public ResearchCitation[] citations { get; set; }
    }

    public class Theme
    {
        public string theme { get; set; }
        public string summary { get; set; }
        public Viewpoint[] viewpoints { get; set; }
        public int impact_score { get; set; }
        public int[] source_ids { get; set; }
    }

    public class Viewpoint
    {
        public string stance { get; set; }
        public string quote { get; set; }
        public int[] source_ids { get; set; }
    }

    public class ResearchCitation
    {
        public int id { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string publisher { get; set; }
        public string date { get; set; }
        public string type { get; set; }
    }

}