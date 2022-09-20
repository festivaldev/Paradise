namespace Paradise.DataCenter.Common.Entities {
	public class LinkedMemberView {
		public LinkedMemberView(int cmid, string name) {
			this.Cmid = cmid;
			this.Name = name;
		}

		public int Cmid { get; private set; }

		public string Name { get; private set; }
	}
}
