using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RulesEngine.Models;

namespace Datahub.Achievements.Models;

public record Achievement
{
    [Key]
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public List<string>? RuleExpressions { get; init; }

    public bool CombineExpressionsIntoSingleExpression { get; set; }

    [NotMapped]
    public List<Rule> Rules
    {
        get
        {
            if (RuleExpressions == null || !RuleExpressions.Any())
            {
                return new List<Rule>();
            }
            
            if(CombineExpressionsIntoSingleExpression)
            {
                return new List<Rule>()
                {
                    new()
                    {
                        RuleName = Code,
                        SuccessEvent = Code,
                        ErrorMessage = Code,
                        Expression = string.Join(" AND ", RuleExpressions),
                        RuleExpressionType = RuleExpressionType.LambdaExpression
                    }
                };
            }
            
            return RuleExpressions.Select(exp => new Rule
            {
                RuleName = Code,
                SuccessEvent = Code,
                ErrorMessage = Code,
                Expression = exp,
                RuleExpressionType = RuleExpressionType.LambdaExpression
            }).ToList();

        }
    }
}