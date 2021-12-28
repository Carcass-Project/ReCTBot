using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace NeonBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        public CommandHandler(DiscordSocketClient client, CommandService commands)
        {
            _commands = commands;
            _client = client;
        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: null);
            Console.WriteLine("Commands Installed.");
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            if (!(message.HasCharPrefix('$', ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, message);


            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null);

            
        }
    }
    public class CommandModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        [Alias("callems", "911", "howdoigrowabeard", "burningsensationwhenipee")]
        public async Task HelpCommand()
        {
            Console.WriteLine("Help.");
            await ReplyAsync("**Help:**\n **help: **shows this help message.\n **countdown:** Counts down from seconds x(must be 60 or lower)\n **The prefix is '$'**");
        }

        [Command("beannoying")]
        public async Task AnnoyingCommand()
        {
            var user = Context.User;

            await user.SendMessageAsync("lol I dm'ed you.");
        }

        [Command("nextrctupdate")]
        public async Task RCTUpdateCMD()
        {
            await ReplyAsync("The next ReCT major update is released at: " + DateTimeOffset.MaxValue.ToString());
        }

        [Command("countdown")]
        public async Task CountdownCommand(int countInSeconds)
        {
            if (countInSeconds <= 60)
            {
                var f = await Context.Channel.SendMessageAsync("**Countdown: **" + countInSeconds);

                for (int i = countInSeconds; i > -1; i -= 4)
                {
                    await f.ModifyAsync(msg => msg.Content = "**Countdown: **" + i);
                    await Task.Delay(4000);
                }
                await f.ModifyAsync(msg => msg.Content = "**Countdown finished.**");
                await Task.Delay(2000);
                await f.DeleteAsync();
            }
            else
            {
                await ReplyAsync("Please choose a time lower than 60 seconds.");
            }
        }

        [Command("doc")]
        public async Task GetDoc([Remainder] string docname)
        {
            var wallOfText = new System.Net.WebClient().DownloadString("https://rect.ml/alldocs");

            var f = (JArray)JsonConvert.DeserializeObject(wallOfText);
            var fx = f.AsJEnumerable();
            bool found = false;

            foreach(var x in fx)
            {
                var links = new System.Collections.Generic.List<string>();
                var name = x.Value<string>("Name");
                if(name.Contains(docname) || name.ToUpper().Contains(docname.ToUpper()) || name.ToLower().Contains(docname.ToLower()))
                {
                    found = true;
                    var content = x.Value<string>("Contents");
                    var spans1 = new Regex("<span(.*?)?>");
                    content = spans1.Replace(content, "```js\n");
                    var spans2 = new Regex("</span>");
                    content = spans2.Replace(content, "```");
                    var header1 = new Regex("<h(.*?)?>");
                    content = header1.Replace(content, "**");
                    var header2 = new Regex("</h(.*?)?>");
                    content = header2.Replace(content, "**\n");
                    var para = new Regex("(<p>|</p>)");
                    content = para.Replace(content, "");
                    var atag = new Regex("(<a(.*?)?>|</a>)");
                    var atagg = new Regex("<a(.*?)?>");
                    var mtchs = atagg.Matches(content);
                    if (mtchs.Count > 0)
                    {
                        foreach(Match m in mtchs)
                        {
                            var fs = m.Value;
                            fs = fs.Replace("<a", "");
                            fs = fs.Replace(">", "");
                            fs = fs.Replace("'", "");
                            fs = fs.Replace("/", "");
                            fs = fs.Replace(">", "");
                            fs = fs.Replace("href=", "");
                            fs = new Regex("\\s").Replace(fs, "");
                            links.Add(fs);
                        }
                    }
                    content = atag.Replace(content, "__");
                    content = new Regex("<br>").Replace(content, "\n");
                    content = new Regex("<(.*?)?i>").Replace(content, "*");
                    content = new Regex("<(.*?)?b>").Replace(content, "**");
                    content = new Regex("<(.*?)?u>").Replace(content, "__");
                    content = new Regex("&lt;").Replace(content, "<");
                    content = new Regex("&gt;").Replace(content, ">");
                    content = new Regex("&lt").Replace(content, "<");
                    content = new Regex("&gt").Replace(content, ">");
                    if (content.Length < 4096)
                    {
                        var embedBuilder = new EmbedBuilder();

                        embedBuilder.Title = name;
                        embedBuilder.Color = Color.Red;
                        embedBuilder.Description = content;

                        await Context.Channel.SendMessageAsync("Found it! ", false, embedBuilder.Build());
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync(content);
                    }
                    await Context.Channel.SendMessageAsync("**Links found**:");
                    Console.WriteLine(links.Count);
                    foreach(var fxs in links)
                    {
                        Console.WriteLine(fxs);
                        var buttonBuilder = new ComponentBuilder().WithButton(fxs, fxs);
                        await Context.Channel.SendMessageAsync(null, components: buttonBuilder.Build());
                    }
                }
            }

            if(found == false)
            {
                await ReplyAsync("Couldn't find doc by name ``" + docname + "``");
            }

        }
        [Command("formathtml")]
        public async Task formatHTML([Remainder]string html)
        {
            var content = html;
            var spans1 = new Regex("<span(.*?)?>");
            content = spans1.Replace(content, "```js\n");
            var spans2 = new Regex("</span>");
            content = spans2.Replace(content, "```");
            var header1 = new Regex("<h(.*?)?>");
            content = header1.Replace(content, "**");
            var header2 = new Regex("</h(.*?)?>");
            content = header2.Replace(content, "**\n");
            var para = new Regex("(<p>|</p>)");
            content = para.Replace(content, "");
            var atag = new Regex("(<a(.*?)?>|</a>)");
            content = atag.Replace(content, "__");
           
            content = new Regex("<br>").Replace(content, "\n");
            content = new Regex("<(.*?)?i>").Replace(content, "*");
            content = new Regex("<(.*?)?b>").Replace(content, "**");
            content = new Regex("<(.*?)?u>").Replace(content, "__");
            content = new Regex("&lt;").Replace(content, "<");
            content = new Regex("&gt;").Replace(content, ">");
            content = new Regex("&lt").Replace(content, "<");
            content = new Regex("&gt").Replace(content, ">");

            await ReplyAsync(content);
        }
    }
    public class Program
    {
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();
        public async Task ButtonHandler(SocketMessageComponent btn)
        {
         
            var docname = btn.Data.CustomId;
            var wallOfText = new System.Net.WebClient().DownloadString("https://rect.ml/alldocs");

            var f = (JArray)JsonConvert.DeserializeObject(wallOfText);
            var fx = f.AsJEnumerable();
            bool found = false;

            foreach (var x in fx)
            {
                var links = new System.Collections.Generic.List<string>();
                var name = x.Value<string>("Name");
                if (name.Contains(docname) || name.Contains(docname.ToUpper()) || name.Contains(docname.ToLower()))
                {
                    found = true;
                    var content = x.Value<string>("Contents");
                    var spans1 = new Regex("<span(.*?)?>");
                    content = spans1.Replace(content, "```js\n");
                    var spans2 = new Regex("</span>");
                    content = spans2.Replace(content, "```");
                    var header1 = new Regex("<h(.*?)?>");
                    content = header1.Replace(content, "**");
                    var header2 = new Regex("</h(.*?)?>");
                    content = header2.Replace(content, "**\n");
                    var para = new Regex("(<p>|</p>)");
                    content = para.Replace(content, "");
                    var atag = new Regex("(<a(.*?)?>|</a>)");
                    var atagg = new Regex("<a(.*?)?>");
                    var mtchs = atagg.Matches(content);
                    if (mtchs.Count > 0)
                    {
                        foreach (Match m in mtchs)
                        {
                            var fs = m.Value;
                            atag.Replace(fs, "");
                            links.Add(fs);
                        }
                    }
                    content = atag.Replace(content, "__");
                    content = new Regex("<br>").Replace(content, "\n");
                    content = new Regex("<(.*?)?i>").Replace(content, "*");
                    content = new Regex("<(.*?)?b>").Replace(content, "**");
                    content = new Regex("<(.*?)?u>").Replace(content, "__");
                    content = new Regex("&lt;").Replace(content, "<");
                    content = new Regex("&gt;").Replace(content, ">");
                    content = new Regex("&lt").Replace(content, "<");
                    content = new Regex("&gt").Replace(content, ">");
                    await btn.Channel.SendMessageAsync(content);
                    await btn.Channel.SendMessageAsync("**Links found**:");
                    foreach (var fxs in links)
                    {
                        var buttonBuilder = new ComponentBuilder().WithButton(fxs, fxs, ButtonStyle.Success);
                        await btn.Channel.SendMessageAsync(components: buttonBuilder.Build());
                    }
                }
            }

            if (found == false)
            {
                await btn.Channel.SendMessageAsync("Couldn't find doc by name ``" + docname + "``");
            }

        }

        private DiscordSocketClient _client;
        private CommandService _cmdserv;
        private CommandHandler _handler;
        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _cmdserv = new CommandService();
            _handler = new CommandHandler(_client, _cmdserv);
            
            _client.Log += Log;
       
            
            var token = "OTI1NDQxNjU1NjMxMzg0NjY3.YctKtA.YFnTyfqaTvZzt5G-Nb3nOHtvku8";

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await _client.SetGameAsync("with $help");
            await _handler.InstallCommandsAsync();
            _client.ButtonExecuted += ButtonHandler;

            await Task.Delay(-1);
        }
    }
}
