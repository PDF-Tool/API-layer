using PDF_API.Models.ResponseModels;

namespace PDF_API.Adapters;

public interface IConnectionAdapter
{
    Task SendMessage(Message message);
}