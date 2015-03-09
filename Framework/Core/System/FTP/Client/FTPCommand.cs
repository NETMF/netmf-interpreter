
namespace System.Net
{
    /// <summary>
    /// Ftp command class
    /// </summary>
    public class FTPCommand
    {
        public string CommandName;
        public string CommandContent;
        public FTPCommand(string name, string content)
        {
            CommandName = name.Trim().ToUpper();
            CommandContent = content;
        }

    }
}
