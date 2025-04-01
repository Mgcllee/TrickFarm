public interface IChatClientGrain : IGrainWithGuidKey
{
    Task print_recv_message(string message);
    Task leave_client();
}