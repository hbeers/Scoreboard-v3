using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Deployment.Application;

namespace Scoreboard
{
    public partial class Matrix : Form
    {
        // Global variables

        string members_file, scores_file;
        string[,] spelers;
        Button[] buttonArray = new Button[226];
        Button[] namesArray = new Button[30];
        int game_length;
        int blink_msec;
        int dead_key;
        string rule_set;
       
        public Matrix()
        {
            InitializeComponent();

            this.WindowState = FormWindowState.Maximized;


            //
            // programflow :
            //
            //      - check_prerequisites (input: config file; output: --)
            //      - build_competition matrix (input: scoresfile, membersfile; output: --)
            //      - select_game (input: --; output: player1, moyenne1, player2, moyenne2)
            //      - clear_competitionmatrix (input: --; output: --)
            //      - play_game (input: player1, moyenne1, player2, moyenne2; output: gameresult[])
            //      -   case of break : goto no_save_results 
            //      -   case of end_game : goto save results 
            //      - no_save_results (input: --; output: --)
            //      - save_results (input: gameresult[]; output: --)
            //      - goto build_competitionmatrix
            //

            Check_prerequisites();
            Build_matrix();
   
        }

        public class Form2 : Form
        {

            public Form2()
            {
                // 
            }
        }

        private void Check_prerequisites()
        {
            // check prerequisites for this program

            string filepath_base, season = null, filename_members = null, filename_scores = null, filepath_scores = null, filepath_members = null, line = null, rules = null, plus_key = null, minus_key = null, next_key = null;
            int i;
            string[] row;

            // check correct screensize

            decimal height = Screen.PrimaryScreen.Bounds.Height;
            decimal width = Screen.PrimaryScreen.Bounds.Width;
            decimal ratio = 100 * width/ height;

            if (width < 1360)
            {
                MessageBox.Show("De schermafmetingen dienen minimaal 1366 x 768 pixels te zijn voor een goede weergave van het scoreboard.\r\n\n\nHet progamma wordt gestopt ...", "Verkeerde scherminstellingen !!!");
                System.Environment.Exit(1);
            }

            if (ratio < 160)
            {
                MessageBox.Show("De schermindeling van dit programma is gebaseerd op een schermverhouding van 16 : 9\r\n\nJe huidige schermverdeling is 16 : " + (9 * (ratio / 100)).ToString("#.##") + "\r\n\n\nHet progamma wordt gestopt ...", "Verkeerde scherminstellingen !!!");
                System.Environment.Exit(2);
            }

            // check scoreboard.ini exists in the right directory : "C:\<user>\MyDocuments\Scoreboard\scoreboard.ini"

            filepath_base = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            int index1 = filepath_base.LastIndexOf("\\");
            filepath_base = filepath_base.Substring(0, index1) + "\\Google Drive\\Biljartclub";
            string config_file = filepath_base + "\\Programma\\scoreboard.ini";

            try
            {
                if (!File.Exists(config_file)) // config file
                    throw (new Exception("Het configuratie bestand werd niet gevonden: \n\r\n\r C:\\Program Data\\scoreboard.ini\n\r\r\nMeld dit aan de beheerder\r\n\r\nHet programma wordt gestopt"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                System.Environment.Exit(21);
            }

            // reading the config file

            using (StreamReader game_settings = new StreamReader(config_file))
            {
                i = 0;
                while ((line = game_settings.ReadLine()) != null)     //as long as a read gives a non-zero result
                {
                    if (i != 0)
                    {
                        row = line.Split('='); // = config identifier; config value

                        // define game settings
                        if (row[0] == "Game_length") { game_length = Convert.ToInt16(row[1]); }
                        if (row[0] == "Season") { season = row[1]; }
                        if (row[0] == "Rule_set") { rules = row[1]; }
                        // define file paths
                        if (row[0] == "Filepath_members") { filepath_members = row[1]; }
                        if (row[0] == "Filepath_scores") { filepath_scores = row[1]; }
                        if (row[0] == "Filename_members") { filename_members = row[1]; }
                        if (row[0] == "Filename_scores") { filename_scores = row[1]; }
                        // define keystrokes for scoreboard remote operation
                        if (row[0] == "Plus_key") { plus_key = row[1]; }
                        if(row[0] == "Minus_key") { minus_key = row[1]; }
                        if(row[0] == "Next_key") { next_key = row[1]; }
                        // define scoreboard behaviour
                        if (row[0] == "Blink_msec") { blink_msec = Convert.ToInt32(row[1]); }
                        if (row[0] == "Dead_key") { dead_key = Convert.ToInt32(row[1]); }
                    }
                    i++;
                }
                game_settings.Dispose(); // close the config file

                members_file = filepath_base + "\\Uitslagen\\" + season + filepath_members + filename_members; // definition of membersfile including path
                scores_file = filepath_base + "\\Uitslagen\\" + season + filepath_scores + filename_scores; // definition of scoresfile including path
                rule_set = rules; // definition of the way gameresults are calculated
            }

            // check existence of members file

            try
            {
                if (!File.Exists(members_file)) // members
                    throw (new Exception("Het leden bestand werd niet gevonden\r\n\r\nMeld dit aan de beheerder\r\n\r\nU kunt geen gebruik maken van dit scorebord\r\n\r\nHet programma wordt afgesloten"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                System.Environment.Exit(22);
            }

            try //check existence of score file
            {
                if (!File.Exists(scores_file)) // scores
                {
                    throw (new Exception("Het score bestand werd niet gevonden\n\rEr werd een nieuw bestand aangemaakt"));
                }
             }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                // create a new scores file and open it
                TextWriter new_scores = new StreamWriter(scores_file);

                // write the header line to the file
                new_scores.WriteLine("timestamp;speler1;carambole1;hoogste1;poedels1;afstoot1;speler2;carambole2;hoogste2;poedels2;afstoot");

                // close the stream
                new_scores.Close();
            }

            // save the last known scores file on a backup location (local disk) on this computer as soon as the program is started (separate backups)
            // rename the backupfile with a date/time stamp:  save_scores yyyyMMdd-hhmmss

            string backup_path = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            string scores_backup = backup_path + "\\biljartclub\\backup\\save_scores " + DateTime.Now.ToString("yyyy_MM_dd--hh_mm_ss") + ".csv";

            File.Copy(scores_file, scores_backup, true);
        }

        private void Build_matrix()
        {
            // read memberlist and add them to the crosslist borders (top/left side)

            int i,j;
            string line;
            string[] row;
            int maxplayers;
            string[,] scores;

            spelers = new string[20, 3];
            scores = new string[500, 12];

            int height = Screen.PrimaryScreen.WorkingArea.Height;
            int width = Screen.PrimaryScreen.WorkingArea.Width;

            using (StreamReader players = new StreamReader(members_file))
            {
                i = 0;
                while ((line = players.ReadLine()) != null)     //as long as a read gives a non-zero result
                {
                    if (i != 0)
                    {
                        row = line.Split(';');
                        if (row[0] != "")
                        {
                            spelers[i, 0] = row[0]; // contains the full name of the player
                            spelers[i, 1] = row[1]; // contains the moyenne of the player
                            spelers[i, 2] = row[2]; // contains the avatar of the player
                        }
                    }
                    i++;
                }
                maxplayers = i;
                players.Dispose(); // close the members file
            }

            // set Location of first namesButton
            int x_tile = width / (maxplayers + 4);
            int y_tile = height / (maxplayers + 4);
            int x_size = x_tile * 10 / 11; 
            int y_size = y_tile * 10 / 11; 
            int x_space = x_tile / 11; 
            int y_space = y_tile / 11;
            int horizontal;
            int vertical;
            int fontsize = (15 * height / 1046);

            // put stop_button on the right place and in the right size

            Stop_button.Location = new Point(x_tile * (maxplayers+1), y_tile * (maxplayers+1));
            Stop_button.Size = new Size(x_size,y_size);
            Stop_button.Font = new Font("Arial", fontsize);


//            string app_version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
//            this.Text = "Scoreboard Gasterij de Merode - versie " + app_version;

            // Create an array of button to hold the players' names

            // check max # of rows (must not exceed maxplayers) ; else disable all buttons in the row

            for (i=1; i<15;i++)
            {
                namesArray[i] = new Button();
                this.Controls.Add(namesArray[i]);

                namesArray[i].Font = new Font("Arial",fontsize, FontStyle.Bold);
                namesArray[i].Size = new Size(x_size, y_size);
                namesArray[i].ForeColor = Color.Yellow;
                namesArray[i].BackColor = Color.Blue;

 //               namesArray[i].Enabled = false;

                if (i<maxplayers)
                {
                    // vertical names

                    namesArray[i].Text = spelers[i, 2];
                    namesArray[i].Location = new Point(x_tile * 1, y_tile*(1+i));
                    namesArray[i].Visible = true;
                }
                else
                {
                    namesArray[i].Visible = false; 
                }
            }

            for (j = 16; j < 30; j++)
            {
                namesArray[j] = new Button();
                this.Controls.Add(namesArray[j]);
                namesArray[j].Font = new Font("Arial", fontsize, FontStyle.Bold);
                namesArray[j].Size = new Size(x_size, y_size);
                namesArray[j].BackColor = Color.Blue;
                namesArray[j].ForeColor = Color.Yellow;

                if (j < maxplayers+15)
                {
                    // horizontal names

                    namesArray[j].Text = spelers[j-15, 2];
                    namesArray[j].Location = new Point(x_tile*(j-14), (y_tile));
                    namesArray[j].Visible = true;
                }
                else
                {
                    namesArray[j].Visible = false;
                }
            }

            // Create an array of buttons - for easy access to games to play
            vertical = y_tile * 2;
            horizontal = x_tile * 1;

            for (int k = 1; k < buttonArray.Length; k++)
            {

                buttonArray[k] = new Button();
                buttonArray[k].Font = new Font("Arial", fontsize);
                buttonArray[k].Size = new Size(x_size, y_size);
                this.Controls.Add(buttonArray[k]);
                buttonArray[k].Click += new System.EventHandler(ClickGame);

                // check max # of rows (must not exceed maxplayers) ; else disable all buttons in the row
                if (k / 15 > maxplayers - 2)
                {
                    buttonArray[k].Visible = false;
                    buttonArray[k].Enabled = false;
                }

                // check max # of columns (must not exceed maxplayers) ; else disable remaining buttons in the row 

                else if (k % 15 >= maxplayers)
                {
                    buttonArray[k].Visible = false;
                    buttonArray[k].Enabled = false;
                }
                else if ((k - 1) % 16 == 0)
                {
                    // check if button is on junction with column and row; disable the button and color it yellow
                    buttonArray[k].BackColor = Color.DarkGreen;
                    buttonArray[k].FlatAppearance.BorderColor = Color.Red;
                    buttonArray[k].FlatAppearance.BorderSize = 5;
                    buttonArray[k].FlatStyle = FlatStyle.Flat;
                    buttonArray[k].Enabled = false;
                }
                else
                {
                    // button can be used for games en should be coloured and enabled
                    buttonArray[k].BackColor = Color.LightYellow;
                    buttonArray[k].ForeColor = Color.LightYellow;
                    buttonArray[k].Text = k.ToString();
                }

                // increment vertical and reset horizontal counter (depending on game number) and fetch next button    
                if (k % 15 == 0)
                {
                    //vertical position
                    vertical += (y_tile); 
                    horizontal = x_tile;
                }
                else
                {
                    // horizontal position
                    horizontal += x_tile;
                }
                buttonArray[k].Location = new Point(horizontal, vertical);
            }
        }

        private void Save_scores(object sender, EventArgs e)
        // save the last known scores file on a backup location on this computer as soon as the program is started (incrementally)
        // rename the backupfile with a date/time stamp:  save_scores_yyyy_mm_dd_hh_mm_ss
        {
            DateTime localDate = DateTime.Now;
            String[] cultureNames = { "en-US", "en-GB", "fr-FR",
                                "de-DE", "ru-RU" };

            foreach (var cultureName in cultureNames)
            {
                var culture = new CultureInfo(cultureName);
                Console.WriteLine("{0}: {1}", cultureName,
                                  localDate.ToString(culture));
            }
        }

        private void Fill_matrix()
        {
            // read earlier scores from the scores file and put them into a datatable (dt) as soon as the Matrix-form is (re-)displayed ;

            int i, m, x, y, pointer;

            string[] Lines = File.ReadAllLines(scores_file);
            string[] Fields;

            Fields = Lines[0].Split(new char[] { ';' });
            int Cols = Fields.GetLength(0);

            DataTable dt = new DataTable();

            //1st row in the file must be column names; 
            // 0 : [date/time]
            // 1 : [player1]
            // 2 : [carambole1]
            // 3 : [hoogste1]
            // 4 : [poedels1]
            // 5 : [afstoot1]
            // 6 : [player2]
            // 7 : [carambole2]
            // 8 : [hoogste2]
            // 9 : [poedels2]
            // 10: [afstoot2]

            for (i = 0; i < Cols; i++)
            {
                dt.Columns.Add(Fields[i], typeof(string));
            }
            DataRow Row;

            for (i = 1; i < Lines.GetLength(0); i++)
            {
                Fields = Lines[i].Split(new char[] { ';' });
                Row = dt.NewRow();
                for (int f = 0; f < Cols; f++)
                {
                    Row[f] = Fields[f];
                }

                // fill in and display previous games in crosslist
                // the number of the player must be looked up in the playertable (= reference to column and row in crosslist)

                for (m = 1; m < 15; m++)
                {
                    if (Fields[1] == spelers[m, 2])
                    {
                        x = (m - 1) * 15;
                        for (m = 1; m < 15; m++)
                        {
                            if (Fields[6] == spelers[m, 2])
                            {
                                y = m;
                                pointer = x + y;
                                buttonArray[pointer].BackColor = Color.DarkGreen;
                                buttonArray[pointer].ForeColor = Color.Yellow;
                                buttonArray[pointer].Text = Fields[2].ToString() + " - " + Fields[7].ToString();
                            }
                        }
                    }
                }
                dt.Rows.Add(Row);
            }
        }

        private void ClickGame(Object sender, System.EventArgs e)
        {   
            // Result of (Click Button) event, start new game

            int rij, kolom;
            string naam1, naam2;
            string moyenne1, moyenne2;
            string buttonname;

            Button btn = (Button)sender;

            buttonname = btn.Text;
            if (!buttonname.Contains("-"))
            {
                rij = Convert.ToInt32(btn.Text) / 15 + 1;
                kolom = (Convert.ToInt32(btn.Text)) % 15;

                btn.BackColor = Color.Blue;
                btn.ForeColor = Color.Blue;
                btn.Enabled = false;
                moyenne1 = spelers[rij, 1];
                naam1 = spelers[rij, 2];
                moyenne2 = spelers[kolom, 1];
                naam2 = spelers[kolom, 2];

                // Stop displaying the cross-list
                this.Hide();

                // Start displaying on second form and call its Display routine exchanging parameters
                Display frm = new Display(naam1, moyenne1, naam2, moyenne2, scores_file, game_length, blink_msec, dead_key, rule_set);
                frm.Show();
            }
        }

        private void Stop_button_Click(object sender, EventArgs e)
        {
            // show the championship on the screen and create a printable version for display on the website


            // Stop displaying the cross-list 
            this.Hide();

            // Save the championship results at the end of the session to PDF

            Form3 form3 = new Form3();

            // Stop the program

            Application.Exit();
        }

        private void Matrix_Shown(object sender, EventArgs e)
        {
           Fill_matrix();
        }
    }
}
