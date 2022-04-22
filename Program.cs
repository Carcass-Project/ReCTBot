using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.API;
using Discord.Rest;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Carcass;
using Yoakke.Lexer;
using Yoakke.Parser;
using Yoakke.Collections.Values;

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
        public async Task AnnoyingCommand(SocketUser user)
        {


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
                new Thread((async () =>
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
                })).Start();
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
                            fs = fs.Replace("\"", "");
                            fs = new Regex("\\s").Replace(fs, "");
                            links.Add(fs);
                        }
                    }
                    content = atag.Replace(content, "__");
                    content = new Regex("<br>").Replace(content, "\n");
                    content = new Regex("<(.*?)?i>").Replace(content, "*");
                    content = new Regex("<(.*?)?b>").Replace(content, "**");
                    content = new Regex("<(.*?)?u>").Replace(content, "__");
                    content = new Regex("<ul>").Replace(content, "\t* ");
                    content = new Regex("</ul>").Replace(content, "");
                    content = new Regex("&lt;").Replace(content, "<");
                    content = new Regex("&gt;").Replace(content, ">");
                    content = new Regex("&lt").Replace(content, "<");
                    content = new Regex("&gt").Replace(content, ">");
                    content = new Regex("&nbsp;").Replace(content, " ");
                    if (content.Length < 4096)
                    {
                        var embedBuilder = new EmbedBuilder();

                        embedBuilder.Title = name;
                        embedBuilder.Color = Color.Red;
                        embedBuilder.Description = content;

                        await Context.Channel.SendMessageAsync("", false, embedBuilder.Build());
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync(content);
                    }
                    await Task.Delay(1000);
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
            content = new Regex("<ul>").Replace(content, "\t* ");
            content = new Regex("</ul>").Replace(content, "");
            content = new Regex("&lt;").Replace(content, "<");
            content = new Regex("&gt;").Replace(content, ">");
            content = new Regex("&lt").Replace(content, "<");
            content = new Regex("&gt").Replace(content, ">");
            content = new Regex("&nbsp;").Replace(content, " ");

            await ReplyAsync(content);
        }
        [Command("report")]
        public async Task reportCommand(SocketUser user, [Remainder]string reason)
        {
            if(user == null)
            {
                await ReplyAsync("Please select a user to report and try again.");
            }

            var EmbedBuilder = new EmbedBuilder()
                .WithAuthor(Context.User)
                .WithTitle("User Report Card")
                .WithDescription("Reported User: "+user.Id+" :: "+user.Username+"\n **Reason:**\n "+reason)
                .WithTimestamp(DateTimeOffset.UtcNow)
                ;

            await Context.Guild.Owner.SendMessageAsync("Beep, boop! A user report card!:\n", false, EmbedBuilder.Build());
            await Context.User.SendMessageAsync("Beep, boop! A user report card!:\n", false, EmbedBuilder.Build());
            Console.WriteLine("test");
            await ReplyAsync("Report successfully sent to " + Context.Guild.Owner.Username);
        }

        [Command("otfrun")]
        [Alias("compile", "cx", "rct", "rect")]
        public async Task OFTRunCode(string lang, [Remainder]string code)
        {
            
            code = code.Replace("```", "");
            if (lang == "cx")
            {
                Console.WriteLine("cx chosen");

                var Parser = new Carcass.Parser(new Carcass.Lexer(code));
                var Evaluator = new Evaluator();
                Evaluator.callStack.Push(new StackFrame());

                StringWriter sr = new StringWriter();
                Console.SetOut(sr);

                var x = Parser.ParseProgram();

                if(x.IsOk)
                {
                    Console.WriteLine("Parsed Successfully");
                    Evaluator.Evaluate(x.Ok.Value);
                }

                await ReplyAsync("```\n"+sr.ToString()+"```");
            }
        }
        [Command("gamble")]
        public async Task gamble(ulong cash)
        {
            var f = new Random().Next(0, 1);
            if(f == 0)
            {
                await ReplyAsync(":game_die: You lost $" + cash + "!");
            }
            else
            {
                await ReplyAsync(":game_die: You won $" + cash + "!");
            }
        }

        [Command("rolldice")]
        public async Task rolldice()
        {
            await ReplyAsync(":game_die: You rolled " + new Random().Next(1, 6) + ".");
        }
        [Command("rct")]
        public async Task rctWebsite()
        {
            await ReplyAsync("ReCT: <https://rect.ml> | RPS: <https://rps.rect.ml> | Docs: <https://docs.rect.ml> $$");
        }
        [Command("privatedm")]
        public async Task privateDM(SocketUser user, [Remainder]string dm)
        {
            await user.SendMessageAsync("Private DM: "+dm);
        }
        [Command("channel")]
        public async Task newChannel(string name)
        {
            await Context.Guild.CreateTextChannelAsync(name);
        }  

        [Command("spoilergame")]
        public async Task spoilerGame(long size = 24)
        {
            char[] af = new char[size];

            for(int i = 0; i < af.Length; i++)
            {
                af[i] = 'a';
            }

            af[new Random().Next(0, af.Length)] = 'ą';

            string fz = "";

            int ix = 0;
            foreach(var zz in af)
            {
                fz += "||" + zz + "||";
                if (ix % 16 == 0)
                    fz += "\n";
                ix++;
            }

            await ReplyAsync("Find the 'ą'\n" + fz);
        }

        private static readonly string subscriptionKey = "";
        private static readonly string endpoint = "https://api.cognitive.microsofttranslator.com/";

        [Command("translate")]
        public async Task translate(string languageFrom, [Remainder]string text)
        {
            // Input and output languages are defined as parameters.
            string route = "/translate?api-version=3.0&from="+languageFrom+"&to=en";
            string textToTranslate = text;
            object[] body = new object[] { new { Text = textToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                request.Headers.Add("Ocp-Apim-Subscription-Region", "eastus");

                // Send the request and get response.
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                // Read response as a string.
                string result = await response.Content.ReadAsStringAsync();
                
                await ReplyAsync(result);
            }
        }

        bool activeRoulette = false;

        [Command("installusers")]
        public async Task instusrs()
        {
            
            //await Context.Guild.DownloadUsersAsync();
            await ReplyAsync("MEMBERS DOWNLOADED: " + Context.Guild.DownloadedMemberCount);
        }
        enum ChessPiece
        {
            B_QUEEN,
            B_KING,
            B_ROOK,
            B_BISHOP,
            B_PAWN,
            W_QUEEN,
            W_KING,
            W_ROOK,
            W_BISHOP,
            W_PAWN,
            EmptyW,
            EmptyB
        }

        [Command("chess")]
        public async Task chesscmd(SocketGuildUser plr1, SocketGuildUser plr2)
        {
            ChessPiece[,] board = new ChessPiece[8, 8];
            for(int x = 0; x < 8*8; x++)
            {
                if(x % 8 != 0)
                    await Context.Message.Channel.SendMessageAsync(":black_medium_square:");
                else
                    await Context.Message.Channel.SendMessageAsync(":black_medium_square:\n");
            }
        }
        [Command("clear")]
        public async Task clearcmd(int count)
        {
            if(Context.User.Id != 847955498048421909 || (Context.User as SocketGuildUser).GuildPermissions.ManageMessages)
            {
                await Context.Message.Channel.SendMessageAsync("Only botmins and server admins(with manage messages) can use this command.");
            }
            else
            {
                var msgs = await Context.Channel.GetMessagesAsync(count).FlattenAsync();
                foreach(var msg in msgs)
                {
                    await msg.DeleteAsync();
                }
                await ReplyAsync("Done!");
            }
        }
        [Command("rusroulette")]
        public async Task russianRoulette(params SocketGuildUser[] players)
        { 


            Random rnd = new Random();
            bool exploded = false;

            await ReplyAsync("STARTING");
            var xmsg = await ReplyAsync(":gun:");
            while (exploded == false)
            {
                
                int explodeNum = rnd.Next(0, players.Length);

                int i = 0;
                foreach(var f in players)
                {
                    
                    if (i != explodeNum)
                    {
                        await xmsg.ModifyAsync(e => e.Content = f.Username + " :gun:");
                    }
                    else
                    {
                        await xmsg.ModifyAsync(e => e.Content = f.Username + " :boom: :gun:");
                        exploded = true;
                        break;
                    }
                    i++;
                    await Task.Delay(500);
                }
            }
        }

        [Command("slash")]
        public async Task slashCMDS()
        {
          
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
                if (name.Contains(docname) || name.ToUpper().Contains(docname.ToUpper()) || name.ToLower().Contains(docname.ToLower()))
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
                            fs = fs.Replace("<a", "");
                            fs = fs.Replace(">", "");
                            fs = fs.Replace("'", "");
                            fs = fs.Replace("/", "");
                            fs = fs.Replace(">", "");
                            fs = fs.Replace("href=", "");
                            fs = fs.Replace("\"", "");
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

                        await btn.Channel.SendMessageAsync("Found it! ", false, embedBuilder.Build());
                    }
                    else
                    {
                        await btn.Channel.SendMessageAsync(content);
                    }
                    await Task.Delay(1000);
                    await btn.Channel.SendMessageAsync("**Links found**:");
                    Console.WriteLine(links.Count);


                    foreach (var fxs in links)
                    {
                        //Console.WriteLine(fxs);
                        await btn.Channel.SendMessageAsync(fxs);
                        await Task.Delay(1000);
                    }
                }

            }
        }

        public async Task SlashCommandHandler(SocketSlashCommand _cmd)
        {
            await _cmd.RespondAsync("Someone invoked slash command: "+_cmd.Data.Name);
        }
        

        private DiscordSocketClient _client;
        private CommandService _cmdserv;
        private CommandHandler _handler;
        public async Task ClientJoinOrOn()
        {
            
        }

        public async Task ClientReady()
        {
            SlashCommandBuilder _slashBuilder = new SlashCommandBuilder();
            _slashBuilder.WithName("roulette");
            var rouletteOpt = new SlashCommandOptionBuilder();
            //rouletteOpt.AddOption("players", ApplicationCommandOptionType.User, "Choose users to include in ur roulette game.");
            rouletteOpt.WithName("players");
            rouletteOpt.WithType(ApplicationCommandOptionType.User);
            rouletteOpt.AddChoice("player", 578974916771184651);
            _slashBuilder.AddOption(rouletteOpt);
            _slashBuilder.WithDescription("The Russian Roulette game command.");

            await _client.CreateGlobalApplicationCommandAsync(_slashBuilder.Build());
        }

        public async Task MainAsync()
        {
            var _socketConfig = new DiscordSocketConfig();


            _socketConfig.GatewayIntents = GatewayIntents.All;
            _socketConfig.MessageCacheSize = 100;
            
            _client = new DiscordSocketClient(_socketConfig);
            _cmdserv = new CommandService();
            _handler = new CommandHandler(_client, _cmdserv);
            
            _client.Log += Log;
            _client.LoggedIn += ClientJoinOrOn;
            _client.Ready += ClientReady;
            _client.SlashCommandExecuted += SlashCommandHandler;
  
       
            
            var token = "";

            await _client.LoginAsync(Discord.TokenType.Bot, token);
            await _client.StartAsync();

            await _client.SetGameAsync("with $help");
     
            await _handler.InstallCommandsAsync();

            

            _client.ButtonExecuted += ButtonHandler;

            await Task.Delay(-1);
        }
    }
}
