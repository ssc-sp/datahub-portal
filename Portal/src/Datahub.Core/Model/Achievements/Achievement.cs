using System;
using System.Collections.Generic;

namespace Datahub.Core.Model.Achievements;

public class Achievement
{
    const char RuleSeparator = '\n';

    private Achievement()
    {
    }

    public Achievement(string id, string name, string description, int points, params string[] rules)
    {
        Id = id;
        Name = name;
        Description = description;
        Points = points;
        ConcatenatedRules = string.Join($"{RuleSeparator}", rules);
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Points { get; set; } = 1;
    public string ConcatenatedRules { get; set; }

    #region Navigation props
    
    public virtual ICollection<UserAchievement> UserAchievements { get; set; }
    
    #endregion

    #region Utility functions
    
    public string[] GetRules() => (ConcatenatedRules ?? "").Split(RuleSeparator);
    public string ImageUrl(string storagePath) => throw new NotImplementedException();
    public string UnlocableUrl(string storagePath) => throw new NotImplementedException();
    public bool IsTrophy() => Id.EndsWith("-000");

    #endregion
}
