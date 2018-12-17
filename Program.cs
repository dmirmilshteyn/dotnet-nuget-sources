using System;
using System.Linq;
using System.Xml.Linq;
using McMaster.Extensions.CommandLineUtils;

namespace dotnet_nuget_sources
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication();

            app.HelpOption();

            app.Command("login", c =>
            {
                var sourceArgument = c.Argument("source", "Name of the package source.", false);
                var usernameOption = c.Option("-u|--username", "Username for the specified package source.", CommandOptionType.SingleValue);
                var passwordOption = c.Option("-p|--password", "Password for the specified package source (will be stored unencrypted).", CommandOptionType.SingleValue);
                var configFileOption = c.Option("-c|--config-file", "Path to NuGet.config", CommandOptionType.SingleOrNoValue);

                c.OnExecute(() =>
                {
                    if (string.IsNullOrEmpty(sourceArgument.Value))
                    {
                        Console.Error.WriteLine("No package soruce specified.");
                        return 1;
                    }
                    if (!usernameOption.HasValue())
                    {
                        Console.Error.WriteLine("No username specified.");
                        return 1;
                    }
                    if (!passwordOption.HasValue())
                    {
                        Console.Error.WriteLine("No password specified.");
                        return 1;
                    }
                    if (!configFileOption.HasValue())
                    {
                        Console.Error.WriteLine("No config file specified.");
                        return 1;
                    }

                    var document = XDocument.Load(configFileOption.Value());

                    var packageSourceCredentialsNode = document.Root.Element("packageSourceCredentials");

                    if (packageSourceCredentialsNode == null)
                    {
                        packageSourceCredentialsNode = new XElement("packageSourceCredentials");
                        document.Root.Add(packageSourceCredentialsNode);
                    }

                    var sourceNode = packageSourceCredentialsNode.Element(sourceArgument.Value);
                    if (sourceNode == null)
                    {
                        sourceNode = new XElement(sourceArgument.Value);
                        packageSourceCredentialsNode.Add(sourceNode);
                    }

                    var usernameNode = sourceNode.Elements("add").Where(x => x.Attribute("key").Value == "Username").FirstOrDefault();
                    if (usernameNode == null) {
                        usernameNode = new XElement("add");
                        usernameNode.SetAttributeValue("key", "Username");

                        sourceNode.Add(usernameNode);
                    }

                    usernameNode.SetAttributeValue("value", usernameOption.Value());

                    var passwordNode = sourceNode.Elements("add").Where(x => x.Attribute("key").Value == "ClearTextPassword").FirstOrDefault();
                    if (passwordNode == null) {
                        passwordNode = new XElement("add");
                        passwordNode.SetAttributeValue("key", "ClearTextPassword");

                        sourceNode.Add(passwordNode);
                    }

                    passwordNode.SetAttributeValue("value", passwordOption.Value());

                    document.Save(configFileOption.Value());

                    return 0;
                });
            });

            return app.Execute(args);
        }
    }
}
