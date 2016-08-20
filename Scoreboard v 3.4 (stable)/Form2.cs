using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Scoreboard
{
    public partial class Display : Form
    {

        bool speler1, speler2;
        bool game_running;
        string scores_file;
        string disp1, disp2;
        string name1, name2;
        string target1, target2;
        string rule;
        int beurten;
        int game_length;

        int carambole1, carambole2;
        int hoogste1, hoogste2;
        int poedel1, poedel2;
        int afstoot1, afstoot2;
        int stand1, stand2;

        public Display(string naam1, string moyenne1, string naam2, string moyenne2, string file_path, int length, int blink_msec, int dead_key, string rule_set)
        {

            InitializeComponent();

            // set display variables

            int height = Screen.PrimaryScreen.WorkingArea.Height;
            int width = Screen.PrimaryScreen.WorkingArea.Width;

            timer1.Interval = blink_msec;
            timer2.Interval = blink_msec;
            //timer3.Interval = warn;
            timer4.Interval = dead_key;

            // copy transient values to values that have been defined in this form for further use

            name1 = naam1;
            name2 = naam2;
            target1 = moyenne1;
            target2 = moyenne2;
            scores_file = file_path;
            game_length = length;
            rule = rule_set;
            disp1 = naam1 + " - " + target1;
            disp2 = naam2 + " - " + target2;

            Resize_screen();

            BeginWedstrijd(disp1, disp2);

        }
        private void PlusPunt()
        {
            if (game_running == true)
            {
                if (timer4.Enabled == false)
                {
                    timer4.Start();

                    if (speler1 == true)
                    {
                        carambole1 = carambole1 + 1;
                        L_score1.Text = carambole1.ToString();
                    }
                    else
                    {
                        carambole2 = carambole2 + 1;
                        L_score2.Text = carambole2.ToString();
                    }
                }
            }
        }
        private void MinPunt()
        {
            if (game_running == true)
            {
                if (timer4.Enabled == false)
                {
                    timer4.Start();

                    if (speler1 == true)
                    {
                        if (carambole1 > stand1)
                        {
                            carambole1 = carambole1 - 1;
                            L_score1.Text = carambole1.ToString();
                        }
                    }
                    if (speler2 == true)
                    {
                        if (carambole2 > stand2)
                        {
                            carambole2 = carambole2 - 1;
                            L_score2.Text = carambole2.ToString();
                        }
                    }
                }
            }
        }
        private void PlusBeurt()
        {
            if (game_running == true)
            {
                if (timer4.Enabled == false)
                {
                    timer4.Start();
                    if (speler1 == true)
                    {
                        speler1 = false;
                        speler2 = true;
                        // stop blink player1
                        timer1.Stop();
                        L_white.Visible = true;
                        // start blink player1
                        timer2.Start();
                        L_yellow.Visible = true;

                        if (carambole1 == stand1)
                        {
                            poedel1 = poedel1 + 1;
                            L_poedels1.Text = poedel1.ToString();
                        }
                        if (carambole1 - stand1 > hoogste1)
                        {
                            hoogste1 = carambole1 - stand1;
                        }
                        if (beurten == 1)
                        {
                            afstoot1 = carambole1 - stand1;
                        }
                        stand1 = carambole1;
                    }
                    else
                    {
                        speler1 = true;
                        speler2 = false;
                        // stop blink player2
                        timer2.Stop();
                        L_yellow.Visible = true;
                        // start blink player1
                        timer1.Start();
                        L_white.Visible = true;

                        if (carambole2 == stand2)
                        {
                            poedel2 = poedel2 + 1;
                            L_poedels2.Text = poedel2.ToString();
                        }

                        if (carambole2 - stand2 > hoogste2)
                        {
                            hoogste2 = carambole2 - stand2;
                        }

                        if (beurten == game_length)
                        {
                            afstoot2 = carambole2 - stand2;

                            EindeWedstrijd();   // parameters toevoegen voor Calculate_points 
                        }

                        else
                        {
                            //Increase beurten
                            beurten = beurten + 1;
                            stand2 = carambole2;
                            L_beurt.Text = beurten.ToString();
                            if (beurten == game_length - 1)
                            {
                                timer3.Start();
                            }
                            else
                            {
                                timer3.Stop();
                                L_beurt.Visible = true;
                            }
                        }
                    }
                }
            }
        }

        private void BeginWedstrijd(string disp1, string disp2)
        {
            //Start a new game

            //Reset speler1
            speler1 = true;
            carambole1 = 0;
            stand1 = 0;
            poedel1 = 0;
            hoogste1 = 0;
            afstoot1 = 0;
            L_speler1.Text = disp1;
            L_score1.Text = carambole1.ToString();
            L_score1.Visible = true;

            //Reset speler2
            speler2 = false;
            carambole2 = 0;
            stand2 = 0;
            poedel2 = 0;
            hoogste2 = 0;
            afstoot2 = 0;
            L_speler2.Text = disp2;
            L_score2.Text = carambole2.ToString();
            L_score2.Visible = true;

            //Reset beurt teller
            beurten = 1;
            game_running = true;
            L_beurt.Text = beurten.ToString();
            L_beurt.Visible = true;

            //Suppress poedels
            L_text1.Visible = false;
            L_poedels1.Visible = false;
            L_text2.Visible = false;
            L_poedels2.Visible = false;

            //Suppress results
            label1.Visible = false;
            L_result1.Visible = false;
            label2.Visible = false;
            L_result2.Visible = false;


            //Enable Break-button
            B_breakgame.Enabled = true;
            B_breakgame.Visible = true;
            B_newgame.Enabled = false;
            B_newgame.Visible = false;

            //Set 1st player blinking and second to be stable

            // stop blink player2
            timer2.Stop();
            L_yellow.Visible = true;
            // start blink player1
            L_white.Visible = true;
            timer1.Start();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

        }

        private void EindeWedstrijd()
        {
            // end the game orderly and display counters and poedels
            // block all keyboard inputs, but enable the new-game button

            int points1 = 0;
            int points2 = 0;
            string punten1, punten2;

            game_running = false; // to disable key-entries (Pluspunt, Minpunt, Poedels etc...)

            timer1.Stop();
            timer2.Stop();
            timer3.Stop();
            L_beurt.Visible = false;

            L_text1.Visible = true;
            L_text2.Visible = true;
            L_poedels1.Visible = true;
            L_poedels2.Visible = true;

            // disable the "break game" button
            B_breakgame.Enabled = false;
            B_breakgame.Visible = false;
            // show the "new game" button
            B_newgame.Enabled = true;
            B_newgame.Visible = true;

            //Modify player labels to display end-results
            L_speler1.Text = name1;
            L_speler2.Text = name2;

            //Show results

            // start calculating points to be awarded based on the result of the game and moyennes of the players

            float moy1 = Convert.ToSingle(target1);
            float moy2 = Convert.ToSingle(target2);
            float car1 = carambole1;
            float car2 = carambole2;

            if (rule == "relative" || rule == "relative_plus")
            {
                float result1 = car1 / moy1;
                float result2 = car2 / moy2;

                // calculate game points
                if (result1 > result2) { points1 = 2; points2 = 0; }
                if (result1 == result2) { points1 = 1; points2 = 1; }
                if (result1 < result2) { points1 = 0; points2 = 2; }

            }

            if (rule == "absolute" || rule == "absolute_plus")
            {
                float result1 = car1 - moy1;
                float result2 = car2 - moy2;

                // calculate game points
                if (result1 > result2) { points1 = 2; points2 = 0; }
                if (result1 == result2) { points1 = 1; points2 = 1; }
                if (result1 < result2) { points1 = 0; points2 = 2; }
            }

            // Bonuspoints awarded on equal or higher than moyenne

            if (rule == "absolute" || rule == "relative")
            {
                if (car1 - moy1 >= 0) { points1++; }
                if (car2 - moy2 >= 0) { points2++; }
            }

            // Bonuspoints awarded only on higher than moyenne

            if (rule == "absolute_plus" || rule == "relative_plus")
            {
                if (car1 - moy1 >= 1) { points1++; }
                if (car2 - moy2 >= 1) { points2++; }
            }

            punten1 = points1.ToString();
            punten2 = points2.ToString();

            L_result1.Text = "+" + punten1;
            L_result2.Text = "+" + punten2;
            label1.Visible = true;
            L_result1.Visible = true;
            label2.Visible = true;
            L_result2.Visible = true;


            // update scores file (.CSV)

            DateTime current = DateTime.Now;
            StreamWriter writegames = new StreamWriter(scores_file, true);
            writegames.WriteLine(current.ToString() + ";" + name1 + ";" + carambole1 + ";" + hoogste1 + ";" + poedel1 + ";" + afstoot1 + ";"
                                                          + name2 + ";" + carambole2 + ";" + hoogste2 + ";" + poedel2 + ";" + afstoot2);
            writegames.Dispose();

            B_newgame.Visible = true;
            B_newgame.Enabled = true;

        }


        private void B_breakgame_Click(object sender, EventArgs e)
        {

            // terminate and release an ongoing game prematurely

            DialogResult result = MessageBox.Show("Wil je de wedstrijd afbreken ?  (uitslag wordt niet opgeslagen)", "Wedstrijd afbreken", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                this.Hide();
                Reload_Matrix_screen();
                B_breakgame.Enabled = false;
            }
            else if (result == DialogResult.No)
            {
                this.Show();
            }
        }

        private void B_newgame_Click(object sender, EventArgs e)
        {
            // used as a waiting moment to view final scores before triggering the selecting of a new game
            B_newgame.Enabled = false;
            this.Hide();
            Reload_Matrix_screen();
        }

        private void Display_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1 || e.KeyCode == Keys.Next) { PlusPunt(); }
            if (e.KeyCode == Keys.F2 || e.KeyCode == Keys.PageUp) { MinPunt(); }
            if (e.KeyCode == Keys.F3 || e.KeyCode == Keys.F5 || e.KeyCode == Keys.Escape || e.KeyCode == Keys.B) { PlusBeurt(); }

            // prevent child controls from handling this event as well
            e.SuppressKeyPress = true;
        }

        private void Reload_Matrix_screen()
        {
            // show the matrix from which new games can be selected

            Scoreboard.Matrix frm = new Scoreboard.Matrix();
            frm.Show();

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // used for blinking playername1

            if (L_white.Visible == true)
                L_white.Visible = false;
            else
                L_white.Visible = true;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            // used for blinking playername2

            if (L_yellow.Visible == true)
                L_yellow.Visible = false;
            else
                L_yellow.Visible = true;
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            // used for blinking beurten at last two attempts

            if (L_beurt.Visible == true)
                L_beurt.Visible = false;
            else
                L_beurt.Visible = true;
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            // timer for the "dead" period of key pressed; to overcome accidental unsollicited scores

            timer4.Stop();
        }

        private void Resize_screen()
        {

            // calculate sizes to fit the form and put them on the right place (reference window = 1920 x 1080)
            // note: references to earlier positions seemed not to work; so 'fixed references' in table below

            int width = Screen.PrimaryScreen.WorkingArea.Width; // get current screen sizes

            L_white.Location = new Point(100 * width / 1920, 70 * width / 1920);
            L_white.Size = new Size(100 * width / 1920, 100 * width / 1920);
            L_yellow.Location = new Point(1070 * width / 1920, 70 * width / 1920);
            L_yellow.Size = new Size(100 * width / 1920, 100 * width / 1920);

            L_dash.Location = new Point(870 * width / 1920, 483 * width / 1920);
            L_dash.Size = new Size(142 * width / 1920, 44 * width / 1920);

            L_speler1.Location = new Point(240 * width / 1920, 68 * width / 1920);
            L_speler1.Size = new Size(700 * width / 1920, 120 * width / 1920);
            L_speler1.Font = new Font("Microsoft Sans Serif", 80 * width / 1920);
            L_speler2.Location = new Point(1200 * width / 1920, 68 * width / 1920);
            L_speler2.Size = new Size(700 * width / 1920, 120 * width / 1920);
            L_speler2.Font = new Font("Microsoft Sans Serif", 80 * width / 1920);

            L_score1.Location = new Point(-92 * width / 1920, 120 * width / 1920);
            L_score1.Size = new Size(1164 * width / 1920, 786 * width / 1920);
            L_score1.Font = new Font("Microsoft Sans Serif", 500 * width / 1920);
            L_score2.Location = new Point(848* width / 1920, 120 * width / 1920);
            L_score2.Size = new Size(1164 * width / 1920, 786 * width / 1920);
            L_score2.Font = new Font("Microsoft Sans Serif", 500 * width / 1920);

            L_text1.Location = new Point(110 * width / 1920, 790 * width / 1920);
            L_text1.Size = new Size(320 * width / 1920, 70 * width / 1920);
            L_text1.Font = new Font("Microsoft Sans Serif", 44 * width / 1920);
            L_text2.Location = new Point(1050 * width / 1920, 790 * width / 1920);
            L_text2.Size = new Size(320 * width / 1920, 70 * width / 1920);
            L_text2.Font = new Font("Microsoft Sans Serif", 44 * width / 1920);

            label1.Location = new Point(506 * width / 1920, 790 * width / 1920);
            label1.Size = new Size(276 * width / 1920, 70 * width / 1920);
            label1.Font = new Font("Microsoft Sans Serif", 44 * width / 1920);
            label2.Location = new Point(1444 * width / 1920, 790 * width / 1920);
            label2.Size = new Size(276 * width / 1920, 70 * width / 1920);
            label2.Font = new Font("Microsoft Sans Serif", 44 * width / 1920);

            L_poedels1.Location = new Point(174 * width / 1920, 852 * width / 1920);
            L_poedels1.Size = new Size(192 * width / 1920, 112 * width / 1920);
            L_poedels1.Font = new Font("Microsoft Sans Serif", 72 * width / 1920);
            L_poedels2.Location = new Point(1114 * width / 1920, 852 * width / 1920);
            L_poedels2.Size = new Size(192 * width / 1920, 112 * width / 1920);
            L_poedels2.Font = new Font("Microsoft Sans Serif", 72 * width / 1920);

            L_result1.Location = new Point(546 * width / 1920, 852 * width / 1920);
            L_result1.Size = new Size(196 * width / 1920, 112 * width / 1920);
            L_result1.Font = new Font("Microsoft Sans Serif", 72 * width / 1920);
            L_result2.Location = new Point(1484 * width / 1920, 852 * width / 1920);
            L_result2.Size = new Size(196 * width / 1920, 112 * width / 1920);
            L_result2.Font = new Font("Microsoft Sans Serif", 72 * width / 1920);

            L_beurt.Location = new Point(805 * width / 1920, 794 * width / 1920);
            L_beurt.Size = new Size(310 * width / 1920, 250 * width / 1920);
            L_beurt.Font = new Font("Microsoft Sans Serif", 125 * width / 1920);

            B_breakgame.Location = new Point(1712 * width / 1920, 954 * width / 1920);
            B_breakgame.Size = new Size(147 * width / 1920, 62 * width / 1920);
            B_breakgame.Font = new Font("Microsoft Sans Serif", 15 * width / 1920);

            B_newgame.Location = new Point(1712 * width / 1920, 954 * width / 1920);
            B_newgame.Size = new Size(147 * width / 1920, 62 * width / 1920);
            B_newgame.Font = new Font("Microsoft Sans Serif", 15 * width / 1920);

        }
    }
}
