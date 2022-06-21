namespace PaperPlane2
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string [] arguments)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            string? fileName = null;
            foreach (var arg in arguments) {
                if (arg.ToLower().EndsWith(".pdf")) {
                    fileName = arg;
                }
            }
            Application.Run(new MainForm(fileName));
        }
    }
}