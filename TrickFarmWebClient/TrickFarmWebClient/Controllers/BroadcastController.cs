using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class BroadcastController : ControllerBase
{
    private readonly IHubContext<ChatHub> _hubContext;

    public BroadcastController(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] MessageModel model)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", model.Message);
        return Ok();
    }
}

public class MessageModel
{
    public string Message { get; set; }
}
