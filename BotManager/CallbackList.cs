namespace TelegramToTrello.BotManager;

public static class CallbackList
{
    private const string Slash = "/";
    public const string Board =  Slash+"board";
    public const string List = Slash+"list";
    public const string Tag = Slash+"tag";
    public const string Name = Slash+"name";
    public const string Push = Slash+"push";
    public const string EditTaskBoardAndTable = Slash+"edittaskboardandtable";
    public const string TaskEditboard = Slash+"editboard";
    public const string TaskEditlist = Slash+"editlist";
    public const string TaskEditdate = Slash+"editdate";
    public const string TaskEditname = Slash+"editname";
    public const string TaskEditdesc = Slash+"editdesc";
    public const string Drop = Slash+"drop";
    public const string Autodate = Slash+"autodate";
    public const string Edittask = Slash+"edittask";
    public const string Taskcomplete = Slash+"taskComplete";
    public const string TaskMove = Slash+"taskMove";
    public const string AttachmentsDone = "press_this_when_done";
    public const string AddAttachment = Slash+"addattachment";
    
    public const string TemplateBoard = Slash+"templateboard";
    public const string TemplateList = Slash+"templatelist";
    public const string TemplateSave = Slash + "templateSave";
    public const string TemplateEditName = Slash + "templateEditName";
    public const string TemplateEditDesc = Slash + "templateEditDesc";
    public const string TemplateEditBoard = Slash + "templateEditBoard";
    public const string TemplateRemove = Slash + "templateRemove";
}