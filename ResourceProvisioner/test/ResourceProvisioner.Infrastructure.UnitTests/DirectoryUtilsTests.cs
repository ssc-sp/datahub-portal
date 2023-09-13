using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Infrastructure.Common;

namespace ResourceProvisioner.Infrastructure.UnitTests;

public class DirectoryUtilsTests
{
    [Test]
    public void ShouldThrowExceptionWhenTemplateNameIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            DirectoryUtils.GetTemplatePath(new ResourceProvisionerConfiguration(), null);
        });
    }

    [Test]
    public void ShouldThrowExceptionWhenAcronymIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            DirectoryUtils.GetProjectPath(new ResourceProvisionerConfiguration(), null);
        });
    }
    
}