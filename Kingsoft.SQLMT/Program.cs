using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Kingsoft.Utils.Nuget.SpectreConsole;
using System.Data.SqlClient;
using Kingsoft.Utils.Basic;
using System.IO;
using Newtonsoft.Json;

namespace Kingsoft.SQLMT
{
    internal class Program
    {

        static void Main(string[] args)
        {
            AppConfig config = null;
            var _handler = new ApplicationArgumentsHandler()
                .AddParam("-config", 1);
            var data = _handler.Evaluate(args);
            Dictionary<string, string[]> parameters = data.Item2;
            if (parameters.ContainsKey("-config") && parameters["-config"].Length > 0)
                config = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(parameters["-config"][0]));

            AnsiConsole.Write(new FigletText(FigletFont.Default, "KSSQLMT")
                .Centered().Color(Color.Green));
            AnsiConsole.Write(new Rule().RuleStyle(new Style(Color.Green)));

            ExplorerSelectionPrompt _prompt = new ExplorerSelectionPrompt(frame: "[[{0}]]");
            _prompt.SetPath("/connect", (str, handler) => { handler.Resolve(); });
            _prompt.SetPath("/exit", (str, handler) => { handler.Resolve(); });
            if (AnsiConsole.Prompt(_prompt) == "exit")
                return;
            string conString = "Server={0};Database={1};";
            SqlCredential cred = null;

            if (config != null)
            {
                if (config.ConnectionString != null)
                    conString = config.ConnectionString;
                else
                {
                    conString = string.Format(conString, config.Server, config.Database);
                    unsafe
                    {
                        fixed (char* pw = config.Password)
                        {
                            cred = new SqlCredential(config.Username, new System.Security.SecureString(pw, config.Password.Length));
                        }
                    }
                }
            }
            else
            {
                if (AnsiConsole.Confirm("Is a connection string given?", false))
                    conString = AnsiConsole.Ask<string>("Enter Connection String: ");
                else
                {
                    string server = AnsiConsole.Ask<string>("Enter Server Address: ");
                    string db = AnsiConsole.Ask<string>("Enter Database Name: ");
                    string user = AnsiConsole.Ask<string>("Enter Username: ");
                    string pw = AnsiConsole.Ask<string>("Enter Password: ");
                    conString = string.Format(conString, server, db);
                    unsafe
                    {
                        fixed (char* _pw = pw)
                        {
                            cred = new SqlCredential(config.Username, new System.Security.SecureString(_pw, pw.Length));
                        }
                    }
                }
            }

            SqlConnection con = new SqlConnection(conString);
            if (cred != null)
                con.Credential = cred;
            con.Open();

            while (true)
            {
                bool close = false;
                var prompt = new ExplorerSelectionPrompt("[[...]]", "[[{0}]]")
                    .SetPath("/tables/select", (str, handler) =>
                    {
                        string name = AnsiConsole.Ask<string>("Enter Table Name: ");

                        bool withLimit = AnsiConsole.Confirm("With Limit?");
                        long limit = 0;
                        if (withLimit)
                            limit = AnsiConsole.Ask<long>("Enter Limit: ");

                        SqlCommand command = con.CreateCommand();
                        command.CommandText = string.Format("SELECT * FROM {0}", name + (withLimit ? string.Format(" TOP {0}", limit) : "") + ";");

                    });
                if (close)
                    break;
            }

            while (true) { }
        }
    }
}
