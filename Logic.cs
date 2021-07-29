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
        private ProgramData ProgramData = new ProgramData();
        private string ProgramDataPath = "";
        private string LatestVersion = "0";
        private bool GithubUnreachable = false;

        public Logic(OutputForm form1)
        {
            Logic.logic = this;
            this.ProgramDataPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                @"\MW5LoadOrderManager";

            try
            {
                GetLatestVersionFromGitHub();
            }
            catch (Exception e)
            {
                form1.listBox1.Items.Add("Github unreachable for update trying to launch from existing executable...");
                Console.WriteLine("Github unreachable for update trying to launch from existing executable...");
                GithubUnreachable = true;
                this.LatestVersion = "0";
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }

            LoadGenProgramData();

            if (!ExeExists())
            {
                if (HandleNoExeFound(form1))
                {
                    return;
                }
            }

            if (!GithubUnreachable)
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
            }

            if (float.Parse(this.LatestVersion) > this.ProgramData.version)
            {
                form1.listBox1.Items.Add("A new version is available, starting update...");
                Console.WriteLine("A new version is available, starting update...");
                if (Update())
                {
                    form1.listBox1.Items.Add("Done, starting MW5 Load Order Manager");
                    Console.WriteLine("Done, starting MW5 Mod Loader Manager");
                }
                else
                {
                    form1.listBox1.Items.Add("Update failed trying to launch present Exe");
                    Console.WriteLine("Update failed trying to launch present Exe");
                }
            }
            else
            {
                form1.listBox1.Items.Add("We are at the latest version, starting MW5 Load Order Manager");
                Console.WriteLine("We are at the latest version, starting MW5 Mod Loader Manager");
            }

            UpdateProgamDataFile();
            form1.timer1.Enabled = true;
            form1.timer1.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="form1"></param>
        /// <returns>bool succes</returns>
        private bool HandleNoExeFound(OutputForm form1)
        {
            form1.listBox1.Items.Add("No executable found! Trying to update...");
            Console.WriteLine("No executable found! Trying to update...");

            if (GithubUnreachable)
            {
                form1.listBox1.Items.Add("Update failed closing program.");
                Console.WriteLine("Update failed closing program.");
                form1.Close();
                Environment.Exit(0);
                return false;
            }

            if (Update())
            {
                form1.listBox1.Items.Add("Done, starting MW5 Load Order Manager");
                Console.WriteLine("Done, starting MW5 Mod Loader Manager");

                UpdateProgamDataFile();
                form1.timer1.Enabled = true;
                form1.timer1.Start();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <returns>bool succes</returns>
        private bool Update()
        {
            if (GithubUnreachable)
            {
                return false;
            }
            try
            {
                using (var webClient = new WebClient())
                {
                    string url = String.Format("https://github.com/rjtwins/MW5-Mod-Manager/releases/download/{0}/MW5.Mod.Manager.exe", this.LatestVersion);
                    webClient.DownloadFile(url, this.ProgramDataPath + @"\MW5 Mod Manager.exe");
                }
            }catch(Exception e)
            {
                return false;
            }

            this.ProgramData.version = float.Parse(this.LatestVersion);
            return true;
        }

        private bool UpdateProgamDataFile()
        {
            try
            {
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
                return false;
            }
            return true;
        }
    

        private void GetLatestVersionFromGitHub()
        {
            var client = new GitHubClient(new ProductHeaderValue("MW5LoadOrderManagerUpdater"));
            this.LatestVersion = client.Repository.Release.GetLatest("rjtwins", "MW5-Mod-Manager").Result.TagName;
        }

        internal static void StartMainProgram()
        {
            //Start the main program
            System.Diagnostics.Process.Start(Logic.logic.ProgramDataPath + @"\MW5 Mod Manager.exe");
        }

        private void HandleNoProgramDataFile()
        {
            //Load install dir from previous session:
            if (File.Exists(ProgramDataPath + @"\ProgramData.json"))
            {
                return;
            }
            CreateEmptyProgramDataFile();
        }

        private void CreateEmptyProgramDataFile()
        {
            System.IO.Directory.CreateDirectory(ProgramDataPath);
            System.IO.File.Create(ProgramDataPath + @"\ProgramData.json").Close();
            this.ProgramData.vendor = "";
            this.ProgramData.installdir = new string[2] { "", "" };
            this.ProgramData.version = 0f;
            Console.WriteLine(ProgramDataPath + @"\ProgramData.json was not found, created one with empty values.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool succes</returns>
        private bool TryReadProgramData()
        {
            try
            {
                ReadProgramData();
            }
            catch (Exception e)
            {
                Console.WriteLine(ProgramDataPath + @"\ProgramData.json unreadable.");
                return false;
            }
            return true;
        }

        private void ReadProgramData()
        {
            string json = File.ReadAllText(ProgramDataPath + @"\ProgramData.json");
            Console.WriteLine(json);
            this.ProgramData = JsonConvert.DeserializeObject<ProgramData>(json);
        }

        public void LoadGenProgramData()
        {
            HandleNoProgramDataFile();

            if (!TryReadProgramData())
            {
                CreateEmptyAndTryReadProgramData();
            }

            if (VersionOutOfRange())
            {
                Console.WriteLine("Version outside of possible range. Setting version to v.0.");
                this.ProgramData.version = 0f;
            }

            bool VersionOutOfRange()
            {
                return this.ProgramData.version <= 0f;
            }
        }

        private void CreateEmptyAndTryReadProgramData()
        {
            CreateEmptyProgramDataFile();
            if (!TryReadProgramData())
            {
                throw new Exception("Unable to read or create program data from " + this.ProgramDataPath + @"\ProgramData.json");
            }
        }

        public bool ExeExists()
        {
            return File.Exists(ProgramDataPath + @"\MW5 Mod Manager.exe");
        }
    }
}