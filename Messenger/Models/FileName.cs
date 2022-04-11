namespace Messenger.Models;

public partial class FileName
{
    public FileName()
    {
        ID = default;
        Name = string.Empty;
    }

    public FileName(Guid id, string name)
    {
        ID = id;
        Name = name;
    }

    public Guid ID { get; set; }
    public string Name { get; set; }
}
