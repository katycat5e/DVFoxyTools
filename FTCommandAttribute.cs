using System;
using System.Reflection;
using CommandTerminal;

namespace FoxyTools
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FTCommandAttribute : Attribute
    {
        public string Command;
        public int MaxArgs;
        public int MinArgs;
        public string Help;

        public FTCommandAttribute( 
            string command = null,
            int minArgs = 0, int maxArgs = 0,
            string help = "" )
        {
            Command = command;
            MinArgs = minArgs;
            MaxArgs = maxArgs;
            Help = help;
        }

        public FTCommandAttribute(int minArgs, int maxArgs, string help = "") :
            this(null, minArgs, maxArgs, help)
        {

        }

        public void Register( MethodInfo target )
        {
            if (!(target.CreateDelegate(typeof(Action<CommandArg[]>), null) is Action<CommandArg[]> action))
            {
                FoxyToolsMain.Error($"Failed to register method {target.Name} to command \"{Command}\"");
                return;
            }

            string commandStr;
            if (Command != null)
            {
                if (Command.StartsWith("FT.", StringComparison.InvariantCultureIgnoreCase))
                {
                    commandStr = Command;
                }
                else
                {
                    commandStr = $"FT.{Command}";
                }
            }
            else
            {
                commandStr = $"FT.{target.Name}";
            }

            var command = new CommandInfo
            {
                name = commandStr,
                proc = action,
                min_arg_count = MinArgs,
                max_arg_count = MaxArgs,
                help = Help,
            };

            Terminal.Shell.AddCommand(command);
            Terminal.Autocomplete.Register(command);
        }

        public static void RegisterAll()
        {
            var allTypes = typeof(FTCommandAttribute).Assembly.GetTypes();
            foreach( Type t in allTypes )
            {
                foreach( MethodInfo m in t.GetMethods(BindingFlags.Static | BindingFlags.Public) )
                {
                    var attributes = m.GetCustomAttributes<FTCommandAttribute>();
                    foreach( var command in attributes )
                    {
                        command.Register(m);
                    }
                }
            }
        }
    }
}
