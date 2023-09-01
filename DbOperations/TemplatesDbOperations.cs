using Telegram.Bot.Types;

namespace TelegramToTrello;

public class TemplatesDbOperations
{
    private readonly ITemplateRepository _templateRepository;
    private readonly IBoardRepository _boardRepository;
    private readonly ITableRepository _tableRepository;

    public TemplatesDbOperations(ITemplateRepository templateRepository, 
        IBoardRepository boardRepository,
        ITableRepository tableRepository)
    {
        _templateRepository = templateRepository;
        _boardRepository = boardRepository;
        _tableRepository = tableRepository;
    }

    public async Task<List<Template>> ListTemplates(int userId)
    {
        var templates = await _templateRepository.GetAllTemplatesFromUser(userId);

        return templates;
    }

    public async Task<List<Template>> GetAllBoardTemplates(int userId, string boardId)
    {
        return await _templateRepository.GetAllTemplatesByUserAndBoard(userId, boardId);
    }

    public async Task<Template> GetIncompleteTemplate(int userId)
    {
        var template = await _templateRepository.GetIncompleteTemplate(userId);

        return template;
    }

    public async Task StartTemplate(RegisteredUser user)
    {
        Template template = new Template
        {
            UserId = user.TelegramId,
            Complete = false,
        };

        await _templateRepository.Add(template);
    }

    public async Task AddBoardToTemplate(Template template, string boardId)
    {
        Board board = await _boardRepository.Get(boardId);

        template.BoardId = board.TrelloBoardId;
        template.BoardName = board.BoardName;

        await _templateRepository.Update(template);
    }

    public async Task AddTableToTemplate(Template template, string tableName)
    {
        Table table = await _tableRepository.GetByNameAndBoardId(tableName, template.BoardId);
        
        template.ListId = table.TableId;

        await _templateRepository.Update(template);
    }

    public async Task AddPlaceholderName(Template template)
    {
        template.TaskName = "%%tempName%%#";

        await _templateRepository.Update(template);
    }

    public async Task AddPlaceholderDesc(Template template)
    {
        template.TaskDesc = "%%tempDesc%%#";
        
        await _templateRepository.Update(template);
    }

    public async Task AddName(Template template, string name)
    {
        template.TaskName = name;
        template.TemplateName = name;
        
        await _templateRepository.Update(template);
    }
    
    public async Task AddDesc(Template template, string desc)
    {
        template.TaskDesc = desc;
        
        await _templateRepository.Update(template);
    }

    public async Task SaveTemplate(Template template)
    {
        template.Complete = true;

        await _templateRepository.Update(template);
    }
}