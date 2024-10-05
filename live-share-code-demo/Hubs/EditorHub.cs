using Microsoft.AspNetCore.SignalR;

namespace live_share_code_demo.Hubs
{
    public class EditorHub : Hub
    {
        public async Task UpdateCode(string code)
        {
            await Clients.Others.SendAsync("ReceiveCodeUpdate", code);
        }

        public async Task RunCode(List<string> code)
        {
            await Clients.Others.SendAsync("ReceiveCodeRun", code);
        }
    }
}
