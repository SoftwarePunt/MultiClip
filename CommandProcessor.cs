namespace MultiClip
{
    public static class CommandProcessor
    {
        public static void Process(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string command = args[i++];
                string arg = null;

                if (command.StartsWith("-"))
                {
                    arg = args[i++];
                    command = command.Substring(1);
                }

                switch (command.ToUpper())
                {
                    case "CN": // Clipboard Number [change action]

                        int clipboardNumber = 0;
                        int.TryParse(arg, out clipboardNumber);
                        clipboardNumber--; // what we get is a one-based index, convert to zero-based

                        if (clipboardNumber >= 0 && clipboardNumber <= 4) // 1 to 5 clipboard buttons
                        {
                            Clipper.SetClipboard(clipboardNumber);
                        }

                        break;
                }
            }
        }
    }
}
