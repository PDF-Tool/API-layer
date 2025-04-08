namespace PDF_API.Adapters;

public interface IConnectionAdapter
{
    Task SendMessage(Message message);
}