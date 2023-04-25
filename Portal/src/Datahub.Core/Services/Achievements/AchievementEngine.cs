﻿using Datahub.Core.Model.Achievements;
using System.Collections.Generic;
using System.Linq;
using RulesEngine.Models;

namespace Datahub.Core.Services.Achievements;

public class AchievementEngine
{
    private readonly List<AchievementRule> _rules;

    public AchievementEngine(IEnumerable<Achievement> achievements)
    {
        _rules = achievements.Select(CreateAchievementRule).ToList();
    }

    public async IAsyncEnumerable<string> Evaluate(string currentMetric, IEnumerable<string> ownedAchivements)
    {
        // create a set with the owned achievements (SET)
        HashSet<string> ownedSet = new(ownedAchivements);

        // filter the rules (only non achieved achivements)
        var filteredRules = _rules.Where(r => !ownedSet.Contains(r.AchivementId)).ToList();

        // create the rule engine
        var engine = CreateRulesEngine(filteredRules);

        // create evaluation function params
        var functParams = new EngineFunctionParms(currentMetric, ownedSet);

        // evaluate "regular" achivement rules
        var response = await engine.ExecuteAllRulesAsync(AchievementWorkflow, functParams);

        // collect achievements
        foreach (var achivement in ExtractAchivements(response))
        {
            ownedSet.Add(achivement);
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

    static IEnumerable<string> ExtractAchivements(List<RuleResultTree> response)
    {
        return response.Where(r => r.IsSuccess).Select(r => r.Rule.RuleName);
    }

    static RulesEngine.RulesEngine CreateRulesEngine(List<AchievementRule> rules)
    {
        var rulesEngineSettings = new ReSettings 
        { 
            CustomTypes = new[] { typeof(Utils) } 
        };
        return new RulesEngine.RulesEngine(CreateWorkflows(rules).ToArray(), rulesEngineSettings);
    }

    const string AchievementWorkflow = nameof(AchievementWorkflow);
    const string MetaAchievementWorkflow = nameof(MetaAchievementWorkflow);

    static IEnumerable<Workflow> CreateWorkflows(List<AchievementRule> rules)
    {
        var achievementRules = rules.Where(r => !r.IsMeta).Select(r => r.Rule);
        if (achievementRules.Any())
        {
            yield return new Workflow() { WorkflowName = AchievementWorkflow, Rules = achievementRules };
        }

        var metaAchievementRules = rules.Where(r => r.IsMeta).Select(r => r.Rule);
        if (metaAchievementRules.Any())
        {
            yield return new Workflow() { WorkflowName = MetaAchievementWorkflow, Rules = metaAchievementRules };
        }
    }

    static AchievementRule CreateAchievementRule(Achievement achievement)
    {
        return new(achievement.Id, achievement.IsTrophy(), CreateRule(achievement));
    }

    static Rule CreateRule(Achievement achievement)
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

record AchievementRule(string AchivementId, bool IsMeta, Rule Rule);

