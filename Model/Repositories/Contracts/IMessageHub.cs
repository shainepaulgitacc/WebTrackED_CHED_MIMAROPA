using WebTrackED_CHED_MIMAROPA.Model.ViewModel.ListViewModel;

namespace WebTrackED_CHED_MIMAROPA.Model.Repositories.Contracts
{
	public interface IMessageHub
	{
		Task CountMessage(string count);
		Task ReceiveMessage(string senderId, string recepientId, string messageContent,string profileFileName,int newMessageId);
		Task RemoveMessage(string senderId,int index);
		Task Recipients(string recipientId);
		Task Sender(int newMessageId);
	}
}
