using System;

namespace Paradise.DataCenter.Common.Entities {
	public class ApplicationMilestoneView {
		public ApplicationMilestoneView(int milestoneId, int applicationId, DateTime milestoneDate, string description) {
			this.MilestoneId = milestoneId;
			this.ApplicationId = applicationId;
			this.MilestoneDate = milestoneDate;
			this.Description = description;
		}

		public int MilestoneId { get; private set; }

		public int ApplicationId { get; private set; }

		public DateTime MilestoneDate { get; private set; }

		public string Description { get; private set; }
	}
}
