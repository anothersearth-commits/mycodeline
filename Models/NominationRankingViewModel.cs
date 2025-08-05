using System.Collections.Generic;
using EOM.Web.Services;

namespace EOM.Web.Models
{
    public class NominationRankingViewModel
    {
        public int CycleId { get; set; }
        public AwardType? AwardType { get; set; }
        public IList<NominationWithScore> RankedNominations { get; set; } = new List<NominationWithScore>();
        public bool IsSecondStage { get; set; } = false;
    }
} 