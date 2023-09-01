namespace TelegramToTrello.Repositories;

public class DialogueStorageDbOperations
{
    private readonly IDialogueStorageRepository _dialogueStorageRepository;

    public DialogueStorageDbOperations(IDialogueStorageRepository dialogueStorageRepository)
    {
        _dialogueStorageRepository = dialogueStorageRepository;
    }

    public async Task<DialogueStorage> Retrieve(int id)
    {
        return await _dialogueStorageRepository.Get(id);
    }

    public async Task SaveUserMessage(int userId, int messageId)
    {
        var ds = await _dialogueStorageRepository.Get(userId);
        if (ds is not null)
        {
            ds.UserMessage = messageId;
        }
        await _dialogueStorageRepository.Update(ds);
    }
    
    public async Task SaveBotMessage(int userId, int messageId)
    {
        var ds = await _dialogueStorageRepository.Get(userId);
        if (ds is not null)
        {
            ds.BotMessage = messageId;
        }
        await _dialogueStorageRepository.Update(ds);
    }

    public async Task CreateDialogue(int userId)
    {
        var ds = await Retrieve(userId);
        if (ds is null)
        {
            ds = new DialogueStorage();
            ds.Id = userId;
            await _dialogueStorageRepository.Add(ds);
        }
    }
}