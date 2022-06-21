namespace PaperPlane2
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            string[] args = Environment.GetCommandLineArgs();
            FileAssociations.SetAssociation(".pdf", "PaperPlane.PDF", "Use Paper Plane to split, merge, rearrange PDF files", args[0]);

            string? fileName = null;
            foreach (var arg in args) {
                if (!arg.ToLower().EndsWith(".pdf")) {
                    continue;
                }
                fileName = arg;
                break;
            }


            Application.Run(new MainForm(fileName));
        }
    }
}