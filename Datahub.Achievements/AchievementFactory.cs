using System.Text.Json;
using Datahub.Achievements.Models;
using RulesEngine.Models;

namespace Datahub.Achievements;

public class AchievementFactory
{
    public const string AchievementWorkflowName = "Achievement Workflow";


    public Dictionary<string, Achievement>? Achievements { get; private set; }

    public static async Task<AchievementFactory> CreateFromFilesAsync(string? directoryPath = null)
    {
        var achievementFactory = new AchievementFactory();
        await achievementFactory.InitializeAchievements(directoryPath);
        return achievementFactory;
    }

    private async Task InitializeAchievements(string? directoryPath = null)
    {
        var pathName = directoryPath ?? $"{Directory.GetCurrentDirectory()}/Achievements";
        var files = Directory.GetFiles(pathName, "*.achievement.json", SearchOption.AllDirectories);
        Achievements = new Dictionary<string, Achievement>();
        foreach (var fileData in files)
        {
            var file = await File.ReadAllTextAsync(fileData);
            using var document = JsonDocument.Parse(file);

            var root = document.RootElement;
            var achievement = JsonSerializer.Deserialize<Achievement>(root.GetRawText());
            if (achievement?.Code == null)
            {
                throw new Exception("Achievement Code is null or not found");
            }

            Achievements.Add(achievement.Code, achievement);
        }
    }
    private AchievementFactory() { }
    
    public static Workflow CreateWorkflow(IEnumerable<Achievement> achievements)
    {
        var rules = new List<Rule>();
        if (achievements == null)
            throw new Exception("No achievements found");

        foreach (var achievement in achievements)
        {
            achievement.Rules.ForEach(rules.Add);
        }

        var achievementWorkflow = new Workflow()
        {
            WorkflowName = AchievementWorkflowName,
            Rules = rules
        };

        return achievementWorkflow;
    }
    
    public static Workflow[] CreateWorkflows(IEnumerable<Achievement> achievements)
    {
        if (achievements == null)
            throw new Exception("No achievements found");

        var workflows = new List<Workflow>();

        foreach (var achievement in achievements)
        {
            var rules = new List<Rule>();
            achievement.Rules.ForEach(rules.Add);
            var workflow = new Workflow
            {
                WorkflowName = achievement.Code,
                Rules = rules
            };
            workflows.Add(workflow);
        }

        return workflows.ToArray();
    }
    
    public Achievement FromCode(string code)
    {
        if (Achievements == null)
            throw new Exception("No achievements found");
        if (!Achievements.ContainsKey(code))
            throw new Exception($"Achievement with code {code} not found");
        return Achievements[code];
    }
}