using Microsoft.Extensions.Logging;
using RulesEngine.Models;

namespace Datahub.Achievements;

public class AchievementService
{
    public event EventHandler<AchievementEarnedEventArgs>? AchievementEarned;
    
    private readonly ILogger<AchievementService> _logger;

    private RulesEngine.RulesEngine _rulesEngine;
    
    public AchievementService(ILogger<AchievementService> logger)
    {
        _logger = logger;

        List<Rule> rules = new List<Rule>();

        Rule rule = new Rule();
        rule.RuleName = "Joined A Project";
        rule.SuccessEvent = "User has joined a project.";
        rule.ErrorMessage = "User has not joined a project.";
        rule.Expression = "joinedAProject == true";
        rule.RuleExpressionType = RuleExpressionType.LambdaExpression;
        rules.Add(rule);

        var workflows = new List<Workflow>();

        Workflow exampleWorkflow = new Workflow();
        exampleWorkflow.WorkflowName = "Example Workflow";
        exampleWorkflow.Rules = rules;

        workflows.Add(exampleWorkflow);

        _rulesEngine = new RulesEngine.RulesEngine(workflows.ToArray(), logger);
    }
    
    
    protected virtual void OnAchievementEarned(AchievementEarnedEventArgs args)
    {
        AchievementEarned?.Invoke(this, args);
    }

    public async Task<List<RuleResultTree>> RunRulesEngine(UserAchievementInput input)
    {
        var response = await _rulesEngine.ExecuteAllRulesAsync("Example Workflow", input);
        return response;
    }
}