using System;
using CommandLine;

namespace FbGrant
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<GrantOptions, RevokeOptions>(args)
           .MapResult(
               (GrantOptions opts) => RunGrantService(opts),
               (RevokeOptions opts) => RunRevokeService(opts),
               errs => 1);
           
#if DEBUG
            Console.ReadLine();
#endif
        }

        static int RunGrantService(GrantOptions options)
        {
            var dbInfo = DatabaseInfo.Parse(options.Database);

            GrantService service = new GrantService();
            service.Command = Commands.Grant;
            service.WithAdminOption = options.WithAdminOption;
            service.ConnectionString = dbInfo.GetConnectionString();
            return RunService(service, options);
        }

        static int RunRevokeService(RevokeOptions options)
        {
            var dbInfo = DatabaseInfo.Parse(options.Database);

            GrantService service = new GrantService();
            service.Command = Commands.Revoke;
            service.ConnectionString = dbInfo.GetConnectionString();
            return RunService(service, options);
        }

        static int RunService(GrantService service, CommonOptions options)
        {
            service.PrepareStatements(options.User, options.Role);
            
            if (options.Execute)
                service.ExecuteStatements();
            else
                PrintStatements(service);

            return 0;
        }

        static void PrintStatements(GrantService service)
        {
            foreach (string statement in service.Statements)
            {
                Console.WriteLine(statement);
            }
        }

    }
}
