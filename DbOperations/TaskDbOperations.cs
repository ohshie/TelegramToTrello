using Microsoft.EntityFrameworkCore;
using TelegramToTrello.Repositories;

namespace TelegramToTrello;

public class TaskDbOperations
{
    private readonly IRepository<TTTTask> _tttTaskRepository;
    private readonly ITemplateRepository _templateRepository;

    public TaskDbOperations(IRepository<TTTTask> tttTaskRepository, ITemplateRepository templateRepository)
    {
        _tttTaskRepository = tttTaskRepository;
        _templateRepository = templateRepository;
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

    public async Task FillTaskFromTemplate(TTTTask task, int templateId)
    {
        var template = await _templateRepository.Get(templateId);

        if (template is not null)
        {
            task.TaskName = template.TaskName;
            task.TaskDesc = template.TaskDesc;
            task.TrelloBoardId = template.BoardId;
            task.TrelloBoardName = template.BoardName;
            task.ListId = template.ListId;
        }
        
        await _tttTaskRepository.Update(task);
    }
}