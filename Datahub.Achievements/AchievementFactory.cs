using System.Text.Json;
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
        var files = Directory.GetFiles(pathName, "*.achievement.json");
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
    
    public Workflow CreateWorkflow()
    {

        var rules = new List<Rule>();
        if (Achievements == null)
            throw new Exception("No achievements found");

        foreach (var (code, achievement) in Achievements)
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
    
    public Achievement FromCode(string code)
    {
        if (Achievements == null)
            throw new Exception("No achievements found");
        if (!Achievements.ContainsKey(code))
            throw new Exception($"Achievement with code {code} not found");
        return Achievements[code];
    }
}