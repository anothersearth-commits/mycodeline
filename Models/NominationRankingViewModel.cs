using System.Collections.Generic;
using EOM.Web.Services;

namespace EOM.Web.Models
{
    public class NominationRankingViewModel
    {
        public int CycleId { get; set; }
        public IList<NominationWithScore> RankedNominations { get; set; } = new List<NominationWithScore>();
    }
} 