using System;
using System.Collections.Generic;
using System.Linq;
using EOM.Web.Models;

namespace EOM.Web.Services
{
    /// <summary>
    /// Helper methods used to calculate weighted scores for nominations.
    /// Mirrors the front-end calculation logic (see Evaluations/Index page).
    /// </summary>
    public static class ScoringHelper
    {
        /// <summary>
        /// Calculates a weighted score (0-100) for a nomination based on a collection of sub-criteria scores.
        /// </summary>
        /// <param name="nomination">The nomination with award cycle & criteria hierarchy loaded.</param>
        /// <param name="subCriteriaScores">Dictionary mapping SubCriteriaId to Score (nullable int).</param>
        /// <returns>Weighted score between 0 and 100.</returns>
        public static double CalculateWeightedScore(Nomination nomination, IDictionary<int, int?> subCriteriaScores)
        {
            if (nomination?.AwardCycle?.AwardType?.Criteria == null)
            {
                return 0;
            }

            double totalWeightedScore = 0;

            foreach (var criterion in nomination.AwardCycle.AwardType.Criteria)
            {
                double criterionTotal = 0;
                double criterionMax = 0;

                foreach (var sub in criterion.SubCriteria)
                {
                    subCriteriaScores.TryGetValue(sub.SubCriteriaId, out var scoreValue);
                    criterionTotal += scoreValue ?? 0;
                    criterionMax += sub.MaxScore;
                }

                if (criterionMax > 0)
                {
                    var normalized = criterionTotal / criterionMax; // 0-1
                    totalWeightedScore += normalized * (double)criterion.WeightPercent;
                }
            }

            return totalWeightedScore; // 0-100 (assuming weights sum to 100)
        }

        /// <summary>
        /// Builds a dictionary of sub-criteria scores from ManagerScores collection.
        /// </summary>
        public static IDictionary<int, int?> ToScoreDictionary(IEnumerable<ManagerScore> managerScores)
            => managerScores.ToDictionary(ms => ms.SubCriteriaId, ms => ms.Score);

        /// <summary>
        /// Builds a dictionary of sub-criteria scores from EvaluationScores collection.
        /// </summary>
        public static IDictionary<int, int?> ToScoreDictionary(IEnumerable<EvaluationScore> evaluationScores)
            => evaluationScores.ToDictionary(es => es.SubCriteriaId, es => es.Score);
    }
} 