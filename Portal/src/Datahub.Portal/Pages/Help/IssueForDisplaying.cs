using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Newtonsoft.Json.Linq;

namespace Datahub.Portal.Pages.Help
{
    public class IssueForDisplaying
    {
        /// <summary>
        /// Gets or sets the ID of the issue.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title of the issue.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the issue.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the state of the issue.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the message to display to the user based on the state of the issue.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the date the issue was submitted.
        /// </summary>
        public string SubmittedDate { get; set; }

        /// <summary>
        /// Gets or sets the date the issue was last changed.
        /// </summary>
        public string ChangedDate { get; set; }

        /// <summary>
        /// Initializes a new instance of the IssueForDisplaying class.
        /// </summary>
        /// <param name="workItemDetails">The JObject containing the work item details.</param>
        /// <param name="userView">A flag indicating whether the user view is enabled or not.</param>
        public IssueForDisplaying(JObject workItemDetails, bool userView = true)
        {
            Id = (string)workItemDetails["id"];
            Title = (string)workItemDetails["fields"]["System.Title"];
            var description = (string)workItemDetails["fields"]["System.Description"];
            description = description.Split("<strong>Description:</strong> ")[1].Split("<br>")[0];
            Description = TrimDescription(description);
            State = (string)workItemDetails["fields"]["System.State"];
            Message = userView == true ? GetUserHelpStatusMessage(State) : State;
            SubmittedDate = (string)workItemDetails["fields"]["System.CreatedDate"];
            ChangedDate = (string)workItemDetails["fields"]["System.ChangedDate"]; ;
        }

        public IssueForDisplaying(WorkItem workItem, bool userView = true)
        {
            Id = workItem.Id.ToString();
            Title = workItem.Fields["System.Title"].ToString();
            var description = workItem.Fields["System.Description"].ToString();
            description = description.Split("<strong>Description:</strong> ")[1].Split("<br>")[0];
            Description = TrimDescription(description);
            State = workItem.Fields["System.State"].ToString();
            Message = userView == true ? GetUserHelpStatusMessage(State) : State;
            SubmittedDate = workItem.Fields["System.CreatedDate"].ToString();
            ChangedDate = workItem.Fields["System.ChangedDate"].ToString();
        }

        /// <summary>
        /// Gets the user help status message based on the state of the issue.
        /// </summary>
        /// <param name="state">The state of the issue.</param>
        /// <returns>The user help status message.</returns>
        private string GetUserHelpStatusMessage(string state)
        {
            string message;
            switch (state)
            {
                case "New":
                    message = "Your request is submitted and awaiting review.";
                    break;
                case "Active":
                    message = "Your request is currently active and being worked on.";
                    break;
                case "Waiting":
                    message = "We have reached out for more information.";
                    break;
                case "Closed":
                    message = "Your request has been marked as closed.";
                    break;
                default:
                    message = "The state of your request is currently unknown.";
                    break;
            }
            return message;
        }

        /**
         * Trims the description to a specified maximum length and appends ellipsis if necessary.
         *
         * @param description The description to be trimmed.
         * @param maxLength The maximum length of the trimmed description.
         * @return The trimmed description.
         */
        private string TrimDescription(string description, int maxLength = 240)
        {
            if (description.Length > maxLength)
                return description.Substring(0, maxLength) + "...";
            else
                return description;
        }
    }
}
