namespace TelegramToTrello;

public interface ITemplateRepository : IRepository<Template>
{
    Task<List<Template>> GetAllTemplatesFromUser(int id);

    Task<Template> GetIncompleteTemplate(int id);
}