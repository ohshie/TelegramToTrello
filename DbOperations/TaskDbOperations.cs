using Microsoft.EntityFrameworkCore;
using TelegramToTrello.Repositories;

namespace TelegramToTrello;

public class TaskDbOperations
{
    private readonly IRepository<TTTTask> _tttTaskRepository;

    public TaskDbOperations(IRepository<TTTTask> tttTaskRepository)
    {
        _tttTaskRepository = tttTaskRepository;
    }
    
    public async Task<TTTTask> RetrieveUserTask(int telegramId)
    {
        return await _tttTaskRepository.Get(telegramId);
    }
    
    public async Task RemoveEntry(TTTTask userTask)
    {
        await _tttTaskRepository.Delete(userTask);
    }

    public async Task ToggleEditModeForTask(TTTTask userTask)
    {
        userTask.InEditMode = !userTask.InEditMode;
        await _tttTaskRepository.Update(userTask);
    }

    public async Task ResetParticipants(TTTTask userTask)
    {
        userTask.TaskPartId = null;
        userTask.TaskPartName = null;
        await _tttTaskRepository.Update(userTask); 
    }
}