/* 
 * This project was something I did for fun at the end of summer 2016. It's a combination of my C# knowledge and the knowledge/resources provided by the Discord API developers
 * and Discord bot programming community. This bot still has many uses in my group chat, and is certainly one of my favourite projects. Enjoy!!! -Brent
*/
//nuget package for the discord API. contains basic functions and events for the Discord chat, and connects bot to the servers it currently belongs to.
using Discord;
using Discord.Commands;
using Discord.Audio;

//nuget package for converting mp4 videos received in regYoutube() to .wav format and resampling them to output at specified bits per sample through voice chat. 
using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;

//nuget package for web scraping. used in functions regRank(), regPic() to grab image links from html elements on a web page (mainly google images)
using HtmlAgilityPack;

//nuget package for downloading youtube videos from provided links. used in regYoutube() in combination with other packages to play the audio of the video in voice chat.
using YoutubeExtractor;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using BrentBott.Properties;
using System.IO;

namespace BrentBott
{
    class MyBot
    {
        public static DiscordClient discord;

        public static IAudioClient vClient;

        CommandService commands;

        Random rand;

        private static bool playingSong = false;

        string[] coinflip;

        public MyBot()
        {

            rand = new Random();

            coinflip = new string[]{"Heads", "Tails"};

            /*the next three methods allow for the use of chat logs, audio/voice service, and a help function that displays
            what commands the bot has*/
            discord = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
            });

            discord.UsingAudio(x => {
                x.Mode = AudioMode.Outgoing;
            });

            discord.UsingCommands(x =>
            {
                x.PrefixChar = '~';
                x.AllowMentionPrefix = true;
                x.HelpMode = HelpMode.Public;
            });

            var _vClient = discord.GetService<AudioService>();

            commands = discord.GetService<CommandService>();

            /*this is where you declare/register the functions you wish to have as commands in the discord chat.*/

            cflip();

            regPurge();

            regEcho();

            regTtsEcho();

            regJoin();

            regLeave();

            regDie();

            regRank();

            regSay();

            regPic();

            regSlots();

            regRegister();

            regRecall();

            regYoutube();

            regBless();

            //this is what connect the bot to all the servers it is included in. these servers can be seen on the bot's Discord Developers page.
            discord.ExecuteAndWait(async () =>
            {
                await discord.Connect("MjM0NzI2MDE5ODg0NTE1MzMx.CtwVcA.qAFD2GzUsQHic41BlJCpwApsqXE", TokenType.Bot);
            });

        }

        /*this function is a simple 50/50 coinflip using a random ranging 0 to 1. The 0th element in the coinflip string
         array is heads, and the 1st element in the string array is tails.*/
        private void cflip()
        {
            commands.CreateCommand("coinflip")
                .Do(async (e) =>
                {
                    string result = coinflip[rand.Next(2)];
                    await e.Channel.SendMessage(result);
                });
        }

        /*this function deletes messages in the Discord chat equal to the number in the given parameter (can't exceed
         * 100) the function downloads the amount of messages equal to the given parameter, then proceeds to remove those
         * messages from the chat room.*/
        private void regPurge()
        {
            commands.CreateCommand("purge")
                .Parameter("num", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string userrole = Convert.ToString(e.User.Id);
                    var numDel = $"{e.GetArg("num")}";
                    if (userrole == "109060557016965120" && Convert.ToInt32(numDel) <= 100 && Convert.ToInt32(numDel) > 0)
                    {
                        Message[] messagesToDelete;
                        messagesToDelete = await e.Channel.DownloadMessages(Convert.ToInt32(numDel));
                        await e.Channel.DeleteMessages(messagesToDelete);
                    }
                    else if (userrole != "109060557016965120" || Convert.ToInt32(numDel) > 100 )
                    {
                        await e.Channel.SendMessage("cant do that for you my dude");
                    }
                });
        }

        /*this function "echoes" the string in the required string parameter 6 times. The bot simply enters the string parameter
         into the Discord chat in the for loop.*/
        private void regEcho()
        {
            commands.CreateCommand("echo")
            .Parameter("msg", ParameterType.Unparsed)
                .Do(async (e) =>
            {
                var echoThis = $"{ e.GetArg("msg") }";
                for (int i = 0; i < 1200000001; i++)
                {
                    if (i == 200000000 || i == 400000000 || i == 600000000 || i == 800000000 || i == 1000000000 || i == 1200000000)
                    {
                        await e.Channel.SendMessage(echoThis);
                    }
                }
            });
        }

        /*this function gets the string parameter enetered by the user, and makes the bot say it in the Discord chat.*/
        private void regSay()
        {
            commands.CreateCommand("say")
            .Parameter("msg", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    var sayThis = $"{ e.GetArg("msg") }";
                    await e.Channel.SendMessage(sayThis);
                });
        }

        /*this function "echoes" the string in the required string parameter 6 times. This is done by using Discord's text-to-speech function 
         * As a joke, I didn't want my buddy Jeremy to use this function, so i made a condition that only allows everyone except Jeremy
         * (Jeremy#2813) to use this function. */
        private void regTtsEcho()
        {
            commands.CreateCommand("ttsecho")
            .Parameter("msg", ParameterType.Unparsed)
                .Do(async (e) =>
            {
                if (e.User.Name != "Jeremy#2813")
                {
                    var echoThis = $"{ e.GetArg("msg") }";
                    for (int i = 0; i < 1200000001; i++)
                    {
                        if (i == 200000000 || i == 400000000 || i == 600000000 || i == 800000000 || i == 1000000000 || i == 1200000000)
                        {
                            await e.Channel.SendTTSMessage(echoThis);
                        }
                    }
                }
                else
                {
                    await e.Channel.SendMessage("no jeremy");
                }
            });

        }

        /*this function connects the bot to the voice channel of the user. Only the owner (my user ID in the
         if statement condition) can use this function. If the user isnt connected to a voice channel nothing
         happens.*/
        private void regJoin()
        {
            commands.CreateCommand("join")
                .Do(async (e) =>
                {
                    if (Convert.ToString(e.User.Id) == "109060557016965120")
                    {
                        var aClient = await discord.GetService<AudioService>()
                    .Join(e.User.VoiceChannel);
                    }
                });
        }
        
        /*this function is used to disconnect the bot from the user's voice channel. Both the user and the bot must
         be in the same voice channel in order for this function to work.*/
        private void regLeave()
        {
            commands.CreateCommand("leave")
                .Do(async (e) =>
                {
                        await e.User.VoiceChannel.LeaveAudio();
                });
        }

        /*this function is used to disconnect the bot from the voice channel its currently in. The user's ID must match
         the ID of the bot owner (me) in order to disconnect the bot. Other the error message "can't do that for you
         my dude" is displayed. Unlike regLeave() this function can be used while the user isn't in a voice channel.*/
        private void regDie()
        {
            commands.CreateCommand("die")
                .Do(async (e) =>
                {
                    string userrole = Convert.ToString(e.User.Id);
                    if (userrole == "109060557016965120")
                    {
                        await e.Server.LeaveAudio();
                    }
                    else if (userrole != "109060557016965120")
                    {
                        await e.Channel.SendMessage("cant do that for you my dude");
                    }
                });                
        }

        /*this functions takes a string parameter, which is a youtube video link, then sends the link and the voice channel
         the user is in to the DownloadAudio function, in order for the video to be downloaded, converted to .wav format, and played
         in the voice channel of the user.*/
        private void regYoutube()
        {

            commands.CreateCommand("yt")
            .Parameter("vid",ParameterType.Unparsed)
                .Do(async (e) =>
            {
                Channel voiceChan = e.User.VoiceChannel;
                if (voiceChan == null)
                {
                    await e.Channel.SendMessage("join a voice channel my dude");
                    return;
                }
                string link = $"{e.GetArg("vid")}";
                playingSong = true;
                await e.Channel.SendMessage("fetching video...");
                await DownloadAudio(link, voiceChan);
                playingSong = false;
            });
        }

        /*this function uses HtmlAgilityPack to grab the first google image result for the entered string parameter. The function loads the google
         images page for the desired search (searchThis) then looks for the html node of the first result, then grabs the image contained in it. 
         the image is then sent in the Discord chat. If nothing is found, the error message "nothing found my dude" is sent. */
        private void regPic()
        {
            commands.CreateCommand("pic")
                .Parameter("search", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    var searchThis = $"{ e.GetArg("search")}";
                    var web = new HtmlWeb();
                    var site = web.Load("https://www.google.ca/search?q=" + searchThis + "&rlz=1C1CHBF_enCA698CA698&biw=1300&bih=1014&tbm=isch&source=lnms&sa=X&ved=0ahUKEwiKhd-gwIPQAhXk6oMKHWeLByYQ_AUIBygC#tbm=isch&q=" + searchThis);
                    var rank = site.DocumentNode.SelectSingleNode("(//table[@class=\"images_table\"]//img)[1]/parent::a/@href");
                    if (rank != null)
                        if (rank != null)
                        {
                            string say = rank.InnerHtml;
                            string say2 = say.Remove(0, say.IndexOf("https"));
                            string say3 = say2.Substring(0, say2.IndexOf('"'));
                            await e.Channel.SendMessage(say3);
                        }
                        else
                        {
                            await e.Channel.SendMessage("nothing found my dude");
                        }

                });
        }

        /*this function is specifically for checking the rank of a user in an online game called "Osu!". The command requires a string
         paramater, which is the desired Osu! user to be checked. The function then uses HtmlAgilityPack to load a website called
         OsuTack, which has the string parameter added to it (ex. https://ameobea.me/osutrack/user/stringparameter) in order to look
         up the desired user. The functions then searches for the html node containing the string of the user's rank, then searches
         for the html node containing the user's profile picture in Osu!. The rank and profile picture are then displayed in the
         Discord chat, and if either the user, profile picture, or rank are not found, it displays the error message "nothing found
         my dude" in chat.*/
        private void regRank()
        {
            commands.CreateCommand("rank")
                .Parameter("name", ParameterType.Unparsed)
                .Do(async (e) => 
                {
                    var nameGet = $"{e.GetArg("name")}";
                    var web = new HtmlWeb();
                    var site = web.Load("https://ameobea.me/osutrack/user/" + nameGet + "/");
                    HtmlNode rank = site.DocumentNode.SelectSingleNode("//*[@id=\"content\"]/div[1]/div[1]/table/tr[1]/td[1]");
                    if (rank != null)
                        if (rank != null)
                        {
                            string say = rank.InnerText;
                            await e.Channel.SendMessage(say);
                        }
                        else
                        {
                            await e.Channel.SendMessage("nothing found my dude");
                        }

                    HtmlNode pic = site.DocumentNode.SelectSingleNode("//*[@id=\"content\"]/div[1]/div[1]/div/img");
                    if (pic != null)
                        if (pic != null)
                        {
                            var say = pic.Attributes["src"].Value;
                            await e.Channel.SendMessage(say);
                        }
                        else
                        {
                            await e.Channel.SendMessage("nothing found my dude");
                        }
                });
        }

        /*this function simply adds 5000 to a number paired with a username in the property settings string array "users". The if statement
         with the userrole checks if the ID matches the user's ID that entered the command. Since I own this bot, the user ID has to match
         my own! (only I can use this command in the Discord chat.)*/
        private void regBless()
        {
            commands.CreateCommand("bless")
                .Parameter("user", ParameterType.Unparsed)
                .Do(async (e) =>
                {
                    string userrole = Convert.ToString(e.User.Id);
                    if (userrole == "109060557016965120")
                    {
                        string user = $"{e.GetArg("user")}";
                        string[] arr = Properties.Settings.Default.users;
                        var index = Array.FindIndex(arr, row => row.Contains(user));
                        arr[index] = user + " " + "5000";
                        Properties.Settings.Default.Save();
                        await e.Channel.SendMessage(user + " has been blessed xd");
                    }
                    else
                    {
                        await e.Channel.SendMessage("can't do that for you my dude");
                    }
                });
        }

        /*BrentSlots! This function uses the number value paired with the user's name in a property setting labeled "users". The function requires an
         Integer parameter which will be the value of their bet. The function then proceeds to select 3 random images from a pool of 3 images. In
         this case, the images are custom server emoji's. If the three selected images matches any of the if cases (say1 == say2 || etc...) the user's
         either gains or loses money in their bet. The function then proceeds to add the changed bet value to the user's property settings 
         money value and saves it.*/
        private void regSlots()
        {
            commands.CreateCommand("slots")
                .Parameter("money", ParameterType.Unparsed)
                .Do(async (e) => 
                {
                    Random rand = new Random();
                    int bet = 0;
                    int cash = 0;
                    string[] arr = Properties.Settings.Default.users;
                    var index = Array.FindIndex(arr, row => row.Contains(e.User.Name));
                    if (index >= 0)
                    {
                        string finalcut = arr[index].Remove(0, e.User.Name.Length);
                        cash = Convert.ToInt32(finalcut);
                        bet = Convert.ToInt32($"{e.GetArg("money")}");
                    }
                    var pics = new[] { "<:ahh:231188075601068032>", "<:lmao:235625469490364416>", "<:triggered:238851285326102528>" };
                    var say1 = pics[rand.Next(pics.Length)];
                    var say2 = pics[rand.Next(pics.Length)];
                    var say3 = pics[rand.Next(pics.Length)];
                    if (say1 == say2 & say2 == say3)
                    {
                        if (cash >= 0)
                        {
                            cash -= bet;
                            if (cash >= 0)
                            { 
                            bet *= 3;
                            cash += bet;
                            arr[index] = e.User.Name + " " + cash.ToString();
                            Properties.Settings.Default.Save();
                            await e.Channel.SendMessage("------------------" + System.Environment.NewLine + "||" + say1 + "|" + say2 + "|" + say3 + "||" + System.Environment.NewLine + "------------------" + System.Environment.NewLine + "```WINNER! KENYO MY DUDE! " + bet.ToString() + " won!```" + e.User.Mention + " Current cash: " + cash);
                        }
                        }
                        }
                    else if (say1 == say2 || say2 == say3)
                    {
                        if (cash >= 0)
                        {
                            cash -= bet;
                            if (cash >= 0)
                            {
                                bet *= 2;
                                cash += bet;
                                arr[index] = e.User.Name + " " + cash.ToString();
                                Properties.Settings.Default.Save();
                                await e.Channel.SendMessage("------------------" + System.Environment.NewLine + "||" + say1 + "|" + say2 + "|" + say3 + "||" + System.Environment.NewLine + "------------------" + System.Environment.NewLine + "```Pair my dude! " + bet.ToString() + " won!```" + e.User.Mention + " Current cash: " + cash);
                            }
                            }
                        }
                    else if (say1 == say3)
                    {
                        if (cash >= 0)
                        {
                            cash -= bet;
                            if (cash >= bet)
                            {
                                bet *= 1;
                                cash += bet;
                                arr[index] = e.User.Name + " " + cash.ToString();
                                Properties.Settings.Default.Save();
                                await e.Channel.SendMessage("------------------" + System.Environment.NewLine + "||" + say1 + "|" + say2 + "|" + say3 + "||" + System.Environment.NewLine + "------------------" + System.Environment.NewLine + "```Double my dude! Returned bet!```" + e.User.Mention + " Current cash: " + cash);

                            }
                            }
                    }
                    else
                    {
                        if (cash >= 0)
                        {
                            cash -= bet;
                            if (cash >= 0)
                            {
                                bet *= 0;
                                cash += bet;
                                arr[index] = e.User.Name + " " + cash.ToString();
                                Properties.Settings.Default.Save();
                                await e.Channel.SendMessage("------------------" + System.Environment.NewLine + "||" + say1 + "|" + say2 + "|" + say3 + "||" + System.Environment.NewLine + "------------------" + System.Environment.NewLine + "```Nice try my dude. " + bet.ToString() + " won!```" + e.User.Mention + " Current cash: " + cash);
                            }
                            }
                        }
                });
           

        }

        /*this function registers the user that uses the command ~register into the Property Setting "users". it searches for an empty space
         * in the string array "users", and when it finds one, it enters the user's name + the starting amount of money (5000). This money
         * is used for BrentSlots. */
        private void regRegister()
        {
            commands.CreateCommand("register")
                .Do(async (e) => 
                {
                    int x = 0;
                    bool indexOpen = false;
                    while (indexOpen != true)
                    {
                        if (Properties.Settings.Default.users[x] == "0")
                        {
                            Properties.Settings.Default.users[x] = e.User.Name.ToString() + " " + "5000";
                            Properties.Settings.Default.Save();
                            indexOpen = true;
                        }
                        else
                        {
                            x++;
                        }
                    }
                    x = 0;
                    await e.Channel.SendMessage(e.User.Mention + " has been registered my dude.");
                });
        }

        /*this function displays the user's money for slots. it checks whether or not the user is registered in a string called "users"
         * in the Property Settings, then displays the amount of money they have. The user's name is paired with how much money they
         * have so I remove their name in the string "finalcut"; thus showing only how much money the user(who entered the command)
         * has. */
        private void regRecall()
        {
            commands.CreateCommand("profile")
                .Do(async (e) =>
                {

                    string[] arr = Properties.Settings.Default.users;
                    var index = Array.FindIndex(arr, row => row.Contains(e.User.Name));
                    if (index >= 0)
                    {
                        string finalcut = arr[index].Remove(0, e.User.Name.Length);
                        await e.Channel.SendMessage(finalcut);
                    }
                    else
                    {
                        await e.Channel.SendMessage("User not found. Use ~register my dude!");
                    }
                });
        }

        public static async Task DownloadAudio(string link, Channel voice)
        {
            /*
             * Get the available video formats.
             * We'll work with them in the video and audio download examples.
             */
            Channel voiceChan = voice;
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);
            /*
            * Select the first .mp4 video with 360p resolution
            */
            VideoInfo video = videoInfos
                .First(info => info.VideoType == VideoType.Mp4 && info.Resolution == 360);

            /*
             * If the video has a decrypted signature, decipher it
             */
            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            /*
             * Create the video downloader.
             * The first argument is the video to download.
             * The second argument is the path to save the video file.
             */
            string path = @"C:\music\tempvid.mp4";
            var videoDownloader = new VideoDownloader(video, path);

            // Register the ProgressChanged event and print the current progress
            //videoDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage);

            /*
             * Execute the video downloader.
             * For GUI applications note, that this method runs synchronously.
             */
            videoDownloader.Execute();
            await wavConvert(path, voiceChan);
        }

        public static async Task wavConvert(string mp4Conv, Channel voice)
        {
            // convert source audio to AAC
            // create media foundation reader to read the source (can be any supported format, mp3, wav, ...)
            /*using (MediaFoundationReader reader = new MediaFoundationReader(@"d:\source.mp3"))
            {
                MediaFoundationEncoder.EncodeToAac(reader, @"D:\test.mp4");
            }*/
            var channelCount = discord.GetService<AudioService>().Config.Channels;
            // convert "back" to WAV
            // create media foundation reader to read the AAC encoded file
            using (MediaFoundationReader reader = new MediaFoundationReader(mp4Conv))
            // resample the file to PCM with same sample rate, channels and bits per sample
            using (ResamplerDmoStream resampledReader = new ResamplerDmoStream(reader,
                new WaveFormat(48000, 16, channelCount)))
            // create WAVe file
            using (WaveFileWriter waveWriter = new WaveFileWriter(@"C:\music\tempaud.wav", resampledReader.WaveFormat))
            {
                // copy samples
                resampledReader.CopyTo(waveWriter);
            }
            await SendAudio(@"C:\music\tempaud.wav", voice);
        }

        public static async Task SendAudio(string filepath, Channel voiceChannel)
        {

            vClient = await discord.GetService<AudioService>().Join(voiceChannel);

            try
            {
                var channelCount = discord.GetService<AudioService>().Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
                var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
                using (var WaveReader = new WaveFileReader(filepath)) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
                using (var resampler = new MediaFoundationResampler(WaveReader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
                {
                    resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                    int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                    byte[] buffer = new byte[blockSize];
                    int byteCount;

                    while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0 && playingSong) // Read audio into our buffer, and keep a loop open while data is present
                    {
                        if (byteCount < blockSize)
                        {
                            // Incomplete Frame
                            for (int i = byteCount; i < blockSize; i++)
                                buffer[i] = 0;
                        }
                        vClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                    }
                    await vClient.Disconnect();
                }
            }
            catch
            {
                //if something goes wrong.
                System.Console.WriteLine("lol oops");
            }
            await vClient.Disconnect();
        }
            

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
