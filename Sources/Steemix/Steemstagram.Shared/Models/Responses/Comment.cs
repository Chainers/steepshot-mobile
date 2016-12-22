namespace Steemix.Library.Models.Requests
{
	public class Comment
	{
		public string body { get; set; }
		public string title { get; set; }
		public string url { get; set; }
		public string category { get; set; }
		public string author { get; set; }
		public string author_rewards { get; set; }
		public string author_reputation { get; set; }
		public string net_votes { get; set; }
		public string created { get; set; }
		public string curator_payout_value { get; set; }
		public string total_payout_value { get; set; }
		public string pending_payout_value { get; set; }
		public string max_accepted_payout { get; set; }
		public string total_payout_reward { get; set; }
		public string vote { get; set; }
	}
}