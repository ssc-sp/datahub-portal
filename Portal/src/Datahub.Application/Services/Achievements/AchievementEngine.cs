using Datahub.Core.Model.Achievements;
using RulesEngine.Models;

namespace Datahub.Application.Services.Achievements;

public class AchievementEngine
{
    private const string AchievementWorkflow = nameof(AchievementWorkflow);

    private const string MetaAchievementWorkflow = nameof(MetaAchievementWorkflow);

    private readonly List<AchievementRule> rules;

    public AchievementEngine(IEnumerable<Achievement> achievements)
    {
        rules = achievements.Select(CreateAchievementRule).ToList();
    }

    public async IAsyncEnumerable<string> Evaluate(string currentMetric, IEnumerable<string> ownedAchivements)
    {
        // create a set with the owned achievements (SET)
        HashSet<string> earnedSet = new(ownedAchivements);

        // filter the rules (only non achieved achivements)
        var filteredRules = rules.Where(r => !earnedSet.Contains(r.AchivementId)).ToList();

        // create the rule engine
        var engine = CreateRulesEngine(filteredRules);

        // create evaluation function params
        var functParams = new EngineFunctionParms(currentMetric, earnedSet);

        // evaluate "regular" achivement rules
        var response = await engine.ExecuteAllRulesAsync(AchievementWorkflow, functParams);

        // collect achievements
        foreach (var achivement in ExtractAchivements(response))
        {
            earnedSet.Add(achivement);
            yield return achivement;
        }

        // evaluate "meta" achivement rules
        response = await engine.ExecuteAllRulesAsync(MetaAchievementWorkflow, functParams);

        // collect meta achievements
        foreach (var achivement in ExtractAchivements(response))
        {
            yield return achivement;
        }
    }

    private static IEnumerable<string> ExtractAchivements(List<RuleResultTree> response)
    {
        return response.Where(r => r.IsSuccess).Select(r => r.Rule.RuleName);
    }

    private static RulesEngine.RulesEngine CreateRulesEngine(List<AchievementRule> rules)
    {
        var rulesEngineSettings = new ReSettings
        {
            CustomTypes = new[] { typeof(Utils) }
        };
        return new RulesEngine.RulesEngine(CreateWorkflows(rules).ToArray(), rulesEngineSettings);
    }

    private static IEnumerable<Workflow> CreateWorkflows(List<AchievementRule> rules)
    {
        var achievementRules = rules.Where(r => !r.IsMeta).Select(r => r.Rule).ToList();
        if (achievementRules.Any())
        {
            yield return new Workflow() { WorkflowName = AchievementWorkflow, Rules = achievementRules };
        }

        var metaAchievementRules = rules.Where(r => r.IsMeta).Select(r => r.Rule).ToList();
        if (metaAchievementRules.Any())
        {
            yield return new Workflow() { WorkflowName = MetaAchievementWorkflow, Rules = metaAchievementRules };
        }
    }

    private static AchievementRule CreateAchievementRule(Achievement achievement)
    {
        return new(achievement.Id, achievement.IsTrophy(), CreateRule(achievement));
    }

    private static Rule CreateRule(Achievement achievement)
    {
        return new Rule()
        {
            RuleName = achievement.Id,
            SuccessEvent = achievement.Id,
            ErrorMessage = achievement.Id,
            Expression = string.Join(" AND ", achievement.GetRules()),
            RuleExpressionType = RuleExpressionType.LambdaExpression
        };
    }
}

internal record AchievementRule(string AchivementId, bool IsMeta, Rule Rule);
