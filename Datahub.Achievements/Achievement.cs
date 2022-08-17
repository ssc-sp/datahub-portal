using System.ComponentModel.DataAnnotations;
using RulesEngine.Models;

namespace Datahub.Achievements;

public class Achievement
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public List<string>? RuleExpressions { get; init; }

    public List<Rule> Rules
    {
        get
        {
            if (RuleExpressions == null || !RuleExpressions.Any())
            {
                return new List<Rule>();
            }
            
            return RuleExpressions.Select((exp, i) => new Rule()
            {
                RuleName = $"{Code}-{i}",
                SuccessEvent = Code,
                ErrorMessage = $"{Code}-{i}",
                Expression = exp,
                RuleExpressionType = RuleExpressionType.LambdaExpression
            }).ToList();

        }
    }
}