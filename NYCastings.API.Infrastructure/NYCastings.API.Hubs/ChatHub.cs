using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace NYCastings.API.Hubs;

public class ChatHub : Hub
{
	public async Task JoinConversation(string originalMessageId)
	{
		if (!string.IsNullOrWhiteSpace(originalMessageId))
		{
			await base.Groups.AddToGroupAsync(base.Context.ConnectionId, "conv-" + originalMessageId);
		}
	}

	public async Task LeaveConversation(string originalMessageId)
	{
		if (!string.IsNullOrWhiteSpace(originalMessageId))
		{
			await base.Groups.RemoveFromGroupAsync(base.Context.ConnectionId, "conv-" + originalMessageId);
		}
	}
}
