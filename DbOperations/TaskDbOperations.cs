using Microsoft.EntityFrameworkCore;

namespace TelegramToTrello;

public class TaskDbOperations
{
    public async Task<TTTTask> RetrieveUserTask(int telegramId)
    {
        await using (BotDbContext dbContext = new BotDbContext())
        {
            TTTTask userCreatedTask = await dbContext.CreatingTasks.FindAsync(telegramId);

            if (userCreatedTask != null)
            {
                return userCreatedTask;
            }
        }
        return null;
    }
    
    public async Task RemoveEntry(TTTTask userTask)
    {
        using (BotDbContext dbContext = new BotDbContext())
        {
            dbContext.RemoveRange(userTask);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task ToggleEditModeForTask(TTTTask userTask)
    {
        using (BotDbContext dbContext = new())
        {
            userTask.InEditMode = !userTask.InEditMode;
            dbContext.CreatingTasks.Update(userTask);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task ResetParticipants(TTTTask userTask)
    {
        using (BotDbContext dbContext = new())
        {
            userTask.TaskPartId = null;
            userTask.TaskPartName = null;

            dbContext.CreatingTasks.Update(userTask);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<TaskNotification?> RetrieveAssignedTask(string taskId)
    {
        using (BotDbContext dbContext = new())
        {
            TaskNotification? task = await dbContext.TaskNotifications.FirstOrDefaultAsync(tn => tn.TaskId == taskId);

            if (task != null)
            {
                return task;
            }
        }
        return null;
    }

    public async Task RemoveAssignedTask(TaskNotification task)
    {
        using (BotDbContext dbContext = new())
        {
            dbContext.TaskNotifications.Remove(task);
            await dbContext.SaveChangesAsync();
        }
    }
}