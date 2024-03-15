using System.Net.Mail;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Datahub.Core.Data;

/// <summary>
/// Use class To hold info on the MSGraph user list (keep it minimal as this could kill memory)
/// </summary>
public class GraphUser
{
    /// <summary>
    /// Gets or sets user Id
    /// </summary>
    /// <value></value>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets name of user
    /// </summary>
    /// <value></value>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets mail Address
    /// </summary>
    /// <value></value>
    public MailAddress mailAddress { get; set; }

    /// <summary>
    /// Gets the user's email address
    /// </summary>
    /// <value></value>
    public string Mail
    {
        get
        {
            return mailAddress?.Address?.ToLower() ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the user's email address
    /// </summary>
    /// <value></value>
    public string UserName
    {
        get
        {
            return mailAddress?.User?.ToLower() ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the user's email domain
    /// </summary>
    /// <value></value>
    public string Domain
    {
        get
        {
            return mailAddress?.Host?.ToLower() ?? string.Empty;
        }
    }

    public string RootFolder
    {
        get
        {
            return $"{Domain}/{UserName}";
        }
    }

    public string Department { get; set; }

    /// <summary>
    /// Static ctor to create from GraphUser
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static GraphUser Create(User user)
    {
        var email = user.Mail ?? "unknown@unknown.com";
        var instance = new GraphUser()
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            mailAddress = new MailAddress(email),
            Department = user.Department
        };
        return instance;
    }
}