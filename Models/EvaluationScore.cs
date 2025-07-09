using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EOM.Web.Models;

public class EvaluationScore
{
    public int EvaluationScoreId { get; set; }
    public int EvaluationId { get; set; }
    [ValidateNever]
    public virtual Evaluation Evaluation { get; set; }
    public int SubCriteriaId { get; set; }
    [ValidateNever]
    public virtual SubCriteria SubCriteria { get; set; }

    public int? Score { get; set; }
    
    public string? Note { get; set; }
}