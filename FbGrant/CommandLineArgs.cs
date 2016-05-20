using CommandLine;

namespace FbGrant
{
    internal abstract class CommonOptions
    {
        [Option('d', "database", Required = true,
            HelpText = "Database")]
        public string Database { get; set; }

        [Option('u', "user", Required = true, 
            HelpText = "db user")]
        public string User { get; set; }

        [Option('r', "role", 
            HelpText = "db role")]
        public string Role { get; set; }

        [Option('x', "execute",
            HelpText = "Execute statements")]
        public bool Execute { get; set; }
    }

    [Verb("grant", HelpText = "Grant permissions")]
    internal class GrantOptions : CommonOptions
    {
        [Option('a', "WithAdminOption",
         HelpText = "grant WITH ADMIN OPTION.")]
        public bool WithAdminOption { get; set; }
    }

    [Verb("revoke", HelpText = "Revoke permissions")]
    internal class RevokeOptions : CommonOptions
    {
    }
}
