using Newtonsoft.Json;
using Octokit;
using System;
using System.IO;
using System.Net;

namespace MW5LOMLauncherV2
{
    public class Logic
    {
        public static Logic logic;
        ProgramData ProgramData = new ProgramData();
        string ProgramDataPath = "";
        string LatestVersion = "";
        public bool ExeExists = false;
        bool GithubUnreachable = false;

        public Logic(OutputForm form1)
        {
            Logic.logic = this;
            this.ProgramDataPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                @"\MW5LoadOrderManager";

            bool update = false;

            try
            {
                var client = new GitHubClient(new ProductHeaderValue("MW5LoadOrderManagerUpdater"));
                this.LatestVersion = client.Repository.Release.GetLatest("rjtwins", "MW5-Mod-Manager").Result.TagName;
            }
            catch (Exception e)
            {
                form1.listBox1.Items.Add("Github unreachable for update trying to launch from existing executable...");
                Console.WriteLine("Github unreachable for update trying to launch from existing executable...");
                GithubUnreachable = true;

                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            //Is this our first time running?
            if (LoadGenProgramData())
            {
                form1.listBox1.Items.Add("First time startup or corrupted files detected, staring update/restore...");
                Console.WriteLine("First time startup or corrupted files detected, staring update/restore...");

                //we are running a "fresh" version with no stored program data or a corrupt program data file.
                update = true;
            }
            else if(!GithubUnreachable)
            {
                form1.listBox1.Items.Add("Checking for updates...");
                form1.listBox1.Items.Add(String.Format("The latest release is tagged at {0} we are running {1}",
                    LatestVersion,
                    this.ProgramData.version));

                Console.WriteLine("Checking for updates...");
                Console.WriteLine(
                    "The latest release is tagged at {0} we are running {1}",
                    LatestVersion,
                    this.ProgramData.version);

                if (this.ProgramData.version < float.Parse(LatestVersion))
                {
                    form1.listBox1.Items.Add("A new version is available, starting update...");
                    Console.WriteLine("A new version is available, starting update...");
                    update = true;
                }
            }

            if (update && !GithubUnreachable)
            {
                try
                {
                    using (var webClient = new WebClient())
                    {
                        string url = String.Format("https://github.com/rjtwins/MW5-Mod-Manager/releases/download/{0}/MW5.Mod.Manager.exe", this.LatestVersion);
                        webClient.DownloadFile(url, this.ProgramDataPath + @"\MW5 Mod Manager.exe");
                    }
                    this.ProgramData.version = float.Parse(this.LatestVersion);

                    //Update program data
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    using (StreamWriter sw = new StreamWriter(ProgramDataPath + @"\ProgramData.json"))
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(writer, this.ProgramData);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }
            }
            form1.listBox1.Items.Add("Done, starting MW5 Load Order Manager");
            Console.WriteLine("Done, starting MW5 Mod Loader Manager");

            form1.timer1.Enabled = true;
            form1.timer1.Start();
        }

        internal static void StartMainProgram()
        {
            //Start the main program
            System.Diagnostics.Process.Start(Logic.logic.ProgramDataPath + @"\MW5 Mod Manager.exe");
        }

        public bool LoadGenProgramData()
        {
            //Load install dir from previous session:
            if (!File.Exists(ProgramDataPath + @"\ProgramData.json"))
            {
                System.IO.Directory.CreateDirectory(ProgramDataPath);
                System.IO.File.Create(ProgramDataPath + @"\ProgramData.json").Close();
                this.ProgramData.vendor = "EPIC";
                this.ProgramData.installdir = new string[2] {"",""};
                this.ProgramData.version = 0f;
                Console.WriteLine(ProgramDataPath + @"\ProgramData.json was not found.");
                return true;
            }
            try
            {
                string json = File.ReadAllText(ProgramDataPath + @"\ProgramData.json");
                this.ProgramData = JsonConvert.DeserializeObject<ProgramData>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(ProgramDataPath + @"\ProgramData.json unreadable.");
                //Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
                return true;
            }
            if (this.ProgramData.version <= 0f)
            {
                Console.WriteLine("Version outside of possible range.");
                return true;
            }


            if (File.Exists(ProgramDataPath + @"\MW5 Mod Manager.exe"))
            {
                ExeExists = true;
            }
            //ProgramData.json is fine but the exe file is missing? Update to get it.
            else
            {
                Console.WriteLine(ProgramDataPath + @"\MW5 Mod Manager.exe not found.");
                return true;
            }
            return false;
        }
    }
}
