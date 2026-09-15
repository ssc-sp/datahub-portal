using Datahub.Infrastructure.Services.Cost;
using FluentAssertions;

namespace Datahub.Infrastructure.UnitTests.Services.Cost;

[TestFixture]
public class CostManagementClientTypePolicyTests
{
    [TestCase("/subscriptions/test/providers/Microsoft.CostManagement/query")]
    [TestCase("/subscriptions/test/providers/microsoft.costmanagement/query")]
    public void AppliesTo_ShouldReturnTrue_ForCostManagementRequests(string path)
    {
        CostManagementClientTypePolicy.AppliesTo(path).Should().BeTrue();
    }

    [TestCase("/subscriptions/test/providers/Microsoft.Compute/virtualMachines")]
    [TestCase("/subscriptions/test/resourceGroups/test")]
    public void AppliesTo_ShouldReturnFalse_ForOtherArmRequests(string path)
    {
        CostManagementClientTypePolicy.AppliesTo(path).Should().BeFalse();
    }
}
