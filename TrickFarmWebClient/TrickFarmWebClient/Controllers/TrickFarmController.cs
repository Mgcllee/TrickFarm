using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/TrickFarm")]
public class TrickFarmController : ControllerBase
{
    private readonly TcpClientService _tcpClientService;

    public TrickFarmController(TcpClientService tcpClientService)
    {
        _tcpClientService = tcpClientService;
    }

    [HttpGet("connect")]
    public async Task<IActionResult> Connect()
    {
        bool success = await _tcpClientService.ConnectAsync("127.0.0.1", 5000);
        return success ? Ok("Connected") : StatusCode(500, "Connection failed");
    }

    [HttpGet("send")]
    public async Task<IActionResult> SendMessage([FromQuery] string message)
    {
        string response = await _tcpClientService.SendMessageAsync(message);
        return Ok(response);
    }
}
