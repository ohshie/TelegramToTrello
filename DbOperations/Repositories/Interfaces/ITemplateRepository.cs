namespace TelegramToTrello;

public interface ITemplateRepository : IRepository<Template>
{
    Task<List<Template>> GetAllTemplatesFromUser(int id);

    Task<List<Template>> GetAllTemplatesByUserAndBoard(int userId, string boardId);

    Task<Template> GetIncompleteTemplate(int id);
}