using Microsoft.Win32;

namespace PhotoshopApp.Services;

public interface IFileDialogService
{
    string? OpenFileDialog(string filter, string title = "Open File");
    string? SaveFileDialog(string filter, string title = "Save File");
}

public class FileDialogService : IFileDialogService
{
    public string? OpenFileDialog(string filter, string title = "Open File")
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            Title = title,
            CheckFileExists = true,
            CheckPathExists = true
        };

        if (dialog.ShowDialog() == true)
        {
            return dialog.FileName;
        }

        return null;
    }

    public string? SaveFileDialog(string filter, string title = "Save File")
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            Title = title,
            OverwritePrompt = true
        };

        if (dialog.ShowDialog() == true)
        {
            return dialog.FileName;
        }

        return null;
    }
}