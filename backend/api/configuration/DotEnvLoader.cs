namespace api.configuration;

public static class DotEnvLoader
{
    public static void LoadFromNearestEnvironmentFile(params string[] startDirectoryPaths)
    {
        foreach (string startDirectoryPath in startDirectoryPaths)
        {
            DirectoryInfo? currentDirectory = new(startDirectoryPath);

            while (currentDirectory is not null)
            {
                string environmentFilePath = Path.Combine(currentDirectory.FullName, ".env");

                if (File.Exists(environmentFilePath))
                {
                    Load(environmentFilePath);
                    return;
                }

                currentDirectory = currentDirectory.Parent;
            }
        }
    }

    public static void Load(string environmentFilePath)
    {
        if (!File.Exists(environmentFilePath))
        {
            return;
        }

        foreach (string environmentFileLine in File.ReadAllLines(environmentFilePath))
        {
            string trimmedEnvironmentFileLine = environmentFileLine.Trim();

            if (string.IsNullOrWhiteSpace(trimmedEnvironmentFileLine) ||
                trimmedEnvironmentFileLine.StartsWith('#'))
            {
                continue;
            }

            int separatorIndex = trimmedEnvironmentFileLine.IndexOf('=');

            if (separatorIndex <= 0)
            {
                continue;
            }

            string environmentVariableName = trimmedEnvironmentFileLine[..separatorIndex].Trim();
            string environmentVariableValue = trimmedEnvironmentFileLine[(separatorIndex + 1)..].Trim().Trim('"');

            Environment.SetEnvironmentVariable(environmentVariableName, environmentVariableValue);
        }
    }
}
