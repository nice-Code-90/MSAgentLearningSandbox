using JetBrains.Annotations;
using System.Runtime.InteropServices;

namespace ToolCalling.Advanced.Tools;

[PublicAPI]
public class FileSystemTools
{
    public string RootFolder { get; set; }

    public FileSystemTools()
    {

        string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        RootFolder = Path.Combine(homePath, "FunctionCallingExample");

        if (!Directory.Exists(RootFolder))
        {
            Directory.CreateDirectory(RootFolder);
        }
    }

    public string GetRootFolder()
    {
        return RootFolder;
    }


    public void CreateFolder(string folderPath)
    {
        string fullPath = NormalizePath(folderPath);
        Guard(fullPath);
        Directory.CreateDirectory(fullPath);
    }

    public void CreateFile(string filePath, string content)
    {
        string fullPath = NormalizePath(filePath);
        Guard(fullPath);
        File.WriteAllText(fullPath, content);
    }

    public string GetContentOfFile(string filePath)
    {
        string fullPath = NormalizePath(filePath);
        Guard(fullPath);
        return File.ReadAllText(fullPath);
    }

    public void MoveFile(string sourceFilePath, string targetFilePath)
    {
        string source = NormalizePath(sourceFilePath);
        string target = NormalizePath(targetFilePath);

        Guard(source);
        Guard(target);
        File.Move(source, target);
    }

    public void MoveFolder(string sourceFolderPath, string targetFolderPath)
    {
        string source = NormalizePath(sourceFolderPath);
        string target = NormalizePath(targetFolderPath);

        Guard(source);
        Guard(target);
        Directory.Move(source, target);
    }

    public string[] GetFiles(string folderPath)
    {
        string fullPath = NormalizePath(folderPath);
        Guard(fullPath);
        return Directory.GetFiles(fullPath);
    }

    public string[] GetFolders(string folderPath)
    {
        string fullPath = NormalizePath(folderPath);
        Guard(fullPath);
        return Directory.GetDirectories(fullPath);
    }

    public void DeleteFolder(string folderPath)
    {
        string fullPath = NormalizePath(folderPath);

        if (fullPath.TrimEnd('/') == RootFolder.TrimEnd('/'))
        {
            throw new Exception("You are not allowed to delete the Root Folder");
        }

        Guard(fullPath);
        Directory.Delete(fullPath, true);
    }

    public void DeleteFile(string filePath)
    {
        string fullPath = NormalizePath(filePath);
        Guard(fullPath);
        File.Delete(fullPath);
    }

    /// <summary>
    /// Helper method to normalize paths if windows-style backslashes are used.
    /// </summary>
    private string NormalizePath(string path)
    {

        string normalized = path.Replace('\\', '/');


        if (!Path.IsPathRooted(normalized))
        {
            return Path.Combine(RootFolder, normalized);
        }

        return normalized;
    }

    private void Guard(string path)
    {

        if (!path.StartsWith(RootFolder, StringComparison.Ordinal))
        {
            throw new Exception($"Access Denied: You can only work within {RootFolder}. Attempted: {path}");
        }
    }
}