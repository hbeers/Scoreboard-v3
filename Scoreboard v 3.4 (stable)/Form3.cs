using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;


namespace Scoreboard
{

    public partial class Form3 : Form
    {

        // Do not display this form to the users since they can read the PDF file that is being created here on the website
        // Use of Form3 is only necessary for the use of "dataGridView1"

        // variables used in the configuration file
        public string filepath_members, filename_members, filepath_scores, filename_scores, filepath_base, scores_file, members_file, config_file;
        int game_length, blink_msec, warn_at;
        string season;
        string rule_set;
        string warning_sound, point_sound;

        // variables needed for reading the members and scores files
        string[] row;               // reads the lines in the config, members files
        string[,] spelers;          // to hold individual info per player
        string[] scores;            // to hold gameresults per game
        int maxplayers;             // number of maximum players in the playerlist (members.csv)
        int i, j, k;                // just a counter
        string line;                // to read lines of files
        int skipped_records = 0;    // number of invalid records in the scores-file; probably due to inconsistent player names

        // variables needed for calculation of points
        double moyenne1, moyenne2;  // stores temporarily the moyenne of players for calculation of gamepoints
        string[] playerlist;        // holds various counters per player
        int index2;                 // a counter to indicate which player of the playerlist is being processed for gameresults

        // other variables
        int table_row = 0;          // to determine odd or even rows for formatting the rows in the PDF document
        
            public Form3()
        {
            InitializeComponent();

            dataGridView1.AutoGenerateColumns = true;

            // dimension datagrid for game results
            dataGridView1.Rows.Clear();
            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            dataGridView1.Columns.Add("Naam", "Naam");
            dataGridView1.Columns.Add("Moyenne", "Moyenne");
            dataGridView1.Columns.Add("Avatar", "Korte naam");
            dataGridView1.Columns.Add("Wedstrijden", "Wedstrijden");
            dataGridView1.Columns.Add("Gewonnen", "W");
            dataGridView1.Columns.Add("Gelijk", "G");
            dataGridView1.Columns.Add("Verloren", "V");
            dataGridView1.Columns.Add("Punten", "Punten");
            dataGridView1.Columns.Add("Caramboles", "Caramboles");
            dataGridView1.Columns.Add("Poedels", "Totaal Poedels");
            dataGridView1.Columns.Add("P_top", "^");
            dataGridView1.Columns.Add("Hoogste", "Hoogste serie");
            dataGridView1.Columns.Add("Afstoot", "Afstoot");
            dataGridView1.Columns.Add("Nieuw", "Berekend moyenne");

            dataGridView1.Columns[0].Width = 150;   // Naam
            dataGridView1.Columns[1].Width = 80;    // Moyenne
            dataGridView1.Columns[2].Width = 90;    // Korte naam
            dataGridView1.Columns[3].Width = 100;   // Wedstrijden
            dataGridView1.Columns[4].Width = 30;    // Gewonnen
            dataGridView1.Columns[5].Width = 30;    // Gelijk
            dataGridView1.Columns[6].Width = 30;    // Verloren
            dataGridView1.Columns[7].Width = 70;    // Punten
            dataGridView1.Columns[8].Width = 100;   // Caramboles
            dataGridView1.Columns[9].Width = 70;    // Poedels
            dataGridView1.Columns[10].Width = 25;   // Poedels_top
            dataGridView1.Columns[11].Width = 75;   // Hoogste
            dataGridView1.Columns[12].Width = 70;   // Afstoot
            dataGridView1.Columns[13].Width = 90;   // Nieuw Moyenne

            dataGridView1.Visible = false;

            // read shared config file

            // check scoreboard.ini exists in the proper directory : "C:\Program Data\Scoreboard\scoreboard.ini"

            filepath_base = (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

            int index1 = filepath_base.LastIndexOf("\\");
            filepath_base = filepath_base.Substring(0, index1) + "\\Google Drive\\Biljartclub";
            config_file = filepath_base + "\\Programma\\scoreboard.ini";

            // reading the config file

            using (StreamReader game_settings = new StreamReader(config_file))
            {
                int i = 0;
                string line;
                //              game_length, season, rule_set, filepath_members, filepath_scores, filename_members, filename_scores, blink_msec, warn_at, warning_sound, points_sound;

                while ((line = game_settings.ReadLine()) != null)     //as long as a read gives a non-zero result
                {
                    if (i != 0)
                    {

                        row = line.Split('='); // = config identifier; config value

                        if (row[0] == "Game_length") { if (row[1] != "") { game_length = Convert.ToInt32(row[1]); } }
                        if (row[0] == "Season") { season = row[1]; }
                        if (row[0] == "Rule_set") { rule_set = row[1]; }
                        if (row[0] == "Filepath_members") { filepath_members = row[1]; }
                        if (row[0] == "Filepath_scores") { filepath_scores = row[1]; }
                        if (row[0] == "Filename_members") { filename_members = row[1]; }
                        if (row[0] == "Filename_scores") { filename_scores = row[1]; }
                        if (row[0] == "Warn_at") { if (row[1] != "") { warn_at = Convert.ToInt32(row[1]); } }
                        if (row[0] == "Warning_sound") { warning_sound = row[1]; }
                        if (row[0] == "Points_sound") { point_sound = row[1]; }
                        if (row[0] == "Blink_msec") { if (row[1] != "") { blink_msec = Convert.ToInt32(row[1]); } }

                    }
                    i++;
                }
                game_settings.Dispose(); // close the config file

                // check presence of mandatory configuration items
                if (game_length == 0 |
                    season == null |
                    rule_set == null |
                    filepath_members == null |
                    filepath_scores == null |
                    filename_members == null |
                    filename_scores == null |
                    blink_msec == 0)
                {
                    MessageBox.Show("In het configuratiebestand is een fout gevonden\n\nCorrigeer dit eerst ... het programma wordt afgesloten");
                    MessageBox.Show(
                        "Verplicht : \n" +
                        "Game_length = " + game_length + "\n" +
                        "Season = " + season + "\n" +
                        "Rule_set = " + rule_set + "\n" +
                        "Filepath_members = " + filepath_members + "\n" +
                        "Filename_members = " + filename_members + "\n" +
                        "Filepath_scores = " + filepath_scores + "\n" +
                        "Filename_scores = " + filename_scores + "\n" +
                        "Blink_msec = " + blink_msec + "\n\n" +
                        "Optioneel : \n" +
                        "Warn_at = " + warn_at + "\n" +
                        "Warning_sound = " + warning_sound + "\n" +
                        "Point_sound = " + point_sound + "\n",
                    "Inhoud scores.ini"
                    );
                    System.Environment.Exit(30);
                }

                members_file = filepath_base + "\\Uitslagen\\" + season + filepath_members + filename_members;
                scores_file = filepath_base + "\\Uitslagen\\" + season + filepath_scores + filename_scores;

            }

            // start preparing results overview
            // DataTable dt = new DataTable();

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

            string[] regels = File.ReadAllLines(config_file);
            string[] Fields;

            Fields = regels[0].Split(new char[] { ';' });
            int cols = Fields.GetLength(0);

            dataGridView1.Rows.Clear();

            spelers = new string[15, 14];
            scores = new string[11];
            playerlist = new string[15];

            // generate ranking list by number of scored points first
           
            // read members file to fill playerlist[x] and spelers[x,y] and set initial values for the remainder of spelers[x,y]

            using (StreamReader players = new StreamReader(members_file))
            {
                i = 0;
                while ((line = players.ReadLine()) != null)     //as long as a read gives a non-zero result
                {
                    if (i != 0)
                    {
                        row = line.Split(';');
                        spelers[i, 0] = row[0];  // contains the full name of the player
                        spelers[i, 1] = row[1];  // contains the mandatory moyenne of the player
                        spelers[i, 2] = row[2];  // contains the avatar of the player                            
                        spelers[i, 3] = "0";     // contains the total number of games played
                        spelers[i, 4] = "0";     // contains the total number of games won
                        spelers[i, 5] = "0";     // contains the total number of games tied
                        spelers[i, 6] = "0";     // contains the total number of games lost
                        spelers[i, 7] = "0";     // contains the total number of points scored
                        spelers[i, 8] = "0";     // contains the total number of caramboles scored
                        spelers[i, 9] = "0";     // contains the total number of misses (poedel)
                        spelers[i, 10] = "0";    // contains the highest number of misses in one game for this player
                        spelers[i, 11] = "0";    // contains the highest series this season for this player
                        spelers[i, 12] = "0";    // contains the number of succesfull game-starts or game-ends
                        spelers[i, 13] = "0";    // contains the actual moyenne of the player

                        playerlist[i] = row[2];  // fill the indexing array with names

                    }

                    i++;

                }

                maxplayers = i;
                players.Dispose(); // close the members file

            }

            // start reading the results file and calculate values for every player as we go along

            using (StreamReader results = new StreamReader(scores_file))
            {

                i = 0;

                while ((line = results.ReadLine()) != null)     //as long as a read gives a non-zero result
                {
                    if (i != 0)
                    {
                        scores = line.Split(';');

                        // find the indexes of the two players that played this game

                        index1 = Array.IndexOf(playerlist, scores[1]);
                        index2 = Array.IndexOf(playerlist, scores[6]);

                        // check whether a valid playername was used; otherwise skip this; increment # skipped records
                        if (index1 < 0 || index2 < 0)
                        {
                            skipped_records++;
                        }

                        else
                        {

                            // init values for this loop
                            int points1 = 0;
                            int points2 = 0;
                            int old_points = 0;

                            // increment # of games per player
                            spelers[index1, 3] = (Convert.ToInt32(spelers[index1, 3]) + 1).ToString();
                            spelers[index2, 3] = (Convert.ToInt32(spelers[index2, 3]) + 1).ToString();

                            // start calculating points to be awarded based on the result of the game and moyennes of the players
                            float moy1 = Convert.ToSingle(spelers[index1, 1]);
                            float moy2 = Convert.ToSingle(spelers[index2, 1]);
                            float car1 = Convert.ToSingle(scores[2]);
                            float car2 = Convert.ToSingle(scores[7]);

                            //  Calculate scores based on relative results;

                            if (rule_set == "relative" || rule_set == "relative_plus")
                            {
                                float result1 = car1 / moy1;
                                float result2 = car2 / moy2;

                                // calculate game points
                                if (result1 > result2) { points1 = 2; points2 = 0; }
                                if (result1 == result2) { points1 = 1; points2 = 1; }
                                if (result1 < result2) { points1 = 0; points2 = 2; }

                                // check number of games won/tied/lost
                                if (points1 == 2)
                                {
                                    spelers[index1, 4] = (Convert.ToInt32(spelers[index1, 4]) + 1).ToString();
                                    spelers[index2, 6] = (Convert.ToInt32(spelers[index2, 6]) + 1).ToString();
                                }
                                if (points1 == 1)
                                {
                                    spelers[index1, 5] = (Convert.ToInt32(spelers[index1, 5]) + 1).ToString();
                                    spelers[index2, 5] = (Convert.ToInt32(spelers[index2, 5]) + 1).ToString();
                                }

                                if (points1 == 0)
                                {
                                    spelers[index1, 6] = (Convert.ToInt32(spelers[index1, 6]) + 1).ToString();
                                    spelers[index2, 4] = (Convert.ToInt32(spelers[index2, 4]) + 1).ToString();
                                }
                            }

                            //  Calculate scores based on absolute results; 

                            if (rule_set == "absolute" || rule_set == "absolute_plus")
                            {
                                float result1 = car1 - moy1;
                                float result2 = car2 - moy2;

                                // calculate game points
                                if (result1 > result2) { points1 = 2; points2 = 0; }
                                if (result1 == result2) { points1 = 1; points2 = 1; }
                                if (result1 < result2) { points1 = 0; points2 = 2; }

                                // check number of games won/tied/lost
                                if (points1 >= 2)
                                {
                                    spelers[index1, 4] = (Convert.ToInt32(spelers[index1, 4]) + 1).ToString();
                                    spelers[index2, 6] = (Convert.ToInt32(spelers[index2, 6]) + 1).ToString();
                                }

                                if (points1 == 1)
                                {
                                    spelers[index1, 5] = (Convert.ToInt32(spelers[index1, 5]) + 1).ToString();
                                    spelers[index2, 5] = (Convert.ToInt32(spelers[index2, 5]) + 1).ToString();
                                }

                                if (points1 == 0)
                                {
                                    spelers[index1, 6] = (Convert.ToInt32(spelers[index1, 6]) + 1).ToString();
                                    spelers[index2, 4] = (Convert.ToInt32(spelers[index2, 4]) + 1).ToString();
                                }
                            }

                            // Bonuspoints awarded on equal or higher than moyenne

                            if (rule_set == "absolute" || rule_set == "relative")
                            {
                                if (car1 - moy1 >= 0) { points1++; }
                                if (car2 - moy2 >= 0) { points2++; }
                            }

                            // Bonuspoints awarded only on higher than moyenne

                            if (rule_set == "absolute_plus" || rule_set == "relative_plus")
                            {
                                if (car1 - moy1 >= 1) { points1++; }
                                if (car2 - moy2 >= 1) { points2++; }
                            }

                            // add points to player1 record
                            old_points = Convert.ToInt32(spelers[index1, 7]);
                            old_points = old_points + points1;
                            spelers[index1, 7] = old_points.ToString();

                            // add points to player2 record
                            old_points = Convert.ToInt32(spelers[index2, 7]);
                            old_points = old_points + points2;
                            spelers[index2, 7] = old_points.ToString();

                            // increment # caramboles based on this game result
                            spelers[index1, 8] = (Convert.ToInt32(spelers[index1, 8]) + Convert.ToInt32(scores[2])).ToString();
                            spelers[index2, 8] = (Convert.ToInt32(spelers[index2, 8]) + Convert.ToInt32(scores[7])).ToString();

                            // increment number of misses per game (poedel)
                            spelers[index1, 9] = (Convert.ToInt32(spelers[index1, 9]) + Convert.ToInt32(scores[4])).ToString();
                            spelers[index2, 9] = (Convert.ToInt32(spelers[index2, 9]) + Convert.ToInt32(scores[9])).ToString();

                            //check highest number of misses per game per player; change if a higher value has been scored in this game
                            if (Convert.ToInt32(spelers[index1, 10]) < Convert.ToInt32(scores[4]))
                            {
                                if (Convert.ToInt32(scores[4]) < 10)
                                {
                                    spelers[index1, 10] = " " + scores[4];
                                }
                                else
                                {
                                    spelers[index1, 10] = scores[4];
                                }
                            }

                            if (Convert.ToInt32(spelers[index2, 10]) < Convert.ToInt32(scores[9]))
                            {
                                if (Convert.ToInt32(scores[9]) < 10)
                                {
                                    spelers[index2, 10] = " " + scores[9];
                                }
                                else
                                {
                                    spelers[index2, 10] = scores[9];
                                }
                            }

                            //                        if (Convert.ToInt32(spelers[index2, 10]) < Convert.ToInt32(scores[9]))
                            //                        { spelers[index2, 10] = scores[9]; }

                            //check highest score per player; change if a higher value has been scored in this game

                            if (Convert.ToInt32(spelers[index1, 11]) < Convert.ToInt32(scores[3]))
                            {
                                if (Convert.ToInt32(scores[3]) < 10)
                                {
                                    spelers[index1, 11] = " " + scores[3];
                                }
                                else
                                {
                                    spelers[index1, 11] = scores[3];
                                }
                            }

                            if (Convert.ToInt32(spelers[index2, 11]) < Convert.ToInt32(scores[8]))
                            {
                                if (Convert.ToInt32(scores[8]) < 10)
                                {
                                    spelers[index2, 11] = " " + scores[8];
                                }
                                else
                                {
                                    spelers[index2, 11] = scores[8];
                                }
                            }

                            // increment # of succesfull game_starts or game_ends
                            if (Convert.ToInt32(scores[5]) > 0)
                            {
                                spelers[index1, 12] = (Convert.ToInt32(spelers[index1, 12]) + 1).ToString();
                                if (Convert.ToInt32(spelers[index1, 12]) < 10)
                                {
                                    spelers[index1, 12] = " " + spelers[index1, 12];
                                }
                            }
                            if (Convert.ToInt32(scores[10]) > 0)
                            {
                                spelers[index2, 12] = (Convert.ToInt32(spelers[index2, 12]) + 1).ToString();
                                if (Convert.ToInt32(spelers[index2, 12]) < 10)
                                {
                                    spelers[index2, 12] = " " + spelers[index2, 12];
                                }
                            }

                            // actual moyenne; to be used on next year

                            float wed1 = Convert.ToSingle(spelers[index1, 3]);
                            float wed2 = Convert.ToSingle(spelers[index2, 3]);
                            float caram1 = Convert.ToSingle(spelers[index1, 8]);
                            float caram2 = Convert.ToSingle(spelers[index2, 8]);

                            moyenne1 = Math.Round((caram1 / wed1), MidpointRounding.AwayFromZero);
                            if (moyenne1 < 10)
                            {
                                spelers[index1, 13] = " " + moyenne1;
                            }
                            else
                            {
                                spelers[index1, 13] = moyenne1.ToString();
                            }
                            moyenne2 = Math.Round((caram2 / wed2), MidpointRounding.AwayFromZero);
                            if (moyenne2 < 10)
                            {
                                spelers[index2, 13] = " " + moyenne2;
                            }
                            else
                            {
                                spelers[index2, 13] = moyenne2.ToString();
                            }
                        }
                    }
                    i++;

                }
            }

            object[] data_row = new object[14];

            for (j = 1; j < maxplayers; j++)
            {

                for (k = 0; k < 14; k++)
                {
                    if (k < 3)
                    { data_row[k] = spelers[j, k]; }
                    else
                    { data_row[k] = Convert.ToInt16(spelers[j, k]); }
                        
                }
                
                dataGridView1.Rows.Add(data_row);
            }
            this.dataGridView1.RowsDefaultCellStyle.BackColor = Color.Bisque;
            this.dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.Beige;
            this.dataGridView1.DefaultCellStyle.Font = new System.Drawing.Font("Tahoma", 10);

            //calculate size of the datagrid on the screen

            int height = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                height += row.Height;
            }
            height += dataGridView1.ColumnHeadersHeight;

            int width = 0;
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                width += col.Width;
            }
            width += dataGridView1.RowHeadersWidth;

            dataGridView1.ClientSize = new Size(width + 2, height + 2);

            this.dataGridView1.Visible = true;

            // get current date to display

            DateTime today = DateTime.Today;

            // display sorted on points initially

            dataGridKader.Text = "Tussenstand kampioenschap seizoen " + season + "        " + today.ToString("dd MMMMMMMMMM yyyy");
            dataGridView1.Sort(dataGridView1.Columns[7], ListSortDirection.Descending);
            dataGridView1.Columns[7].DefaultCellStyle.ForeColor = Color.Red;
            dataGridView1.Update();
            dataGridView1.ClearSelection();
            dataGridKader.Visible = false;
            dataGridView1.Visible = false;

            // let's save the championship results

            // preparing the headers on the PDF-form
            // header 1
            iTextSharp.text.Font headerfont1 = FontFactory.GetFont("georgia", 14f);
            headerfont1.SetStyle("underline");
            Phrase header1 = new Phrase("Biljartclub \"de Merode\" - Spoordonk                    " + today.ToString("dd MMMMMMMMMM yyyy"), headerfont1);
            Paragraph par1 = new Paragraph();
            par1.Add(header1);
            par1.SpacingBefore = (100f);
            par1.Alignment = (Element.ALIGN_JUSTIFIED_ALL);

            //header 2
            iTextSharp.text.Font headerfont2 = FontFactory.GetFont("georgia", 18f);
            headerfont2.SetStyle("normal");
            Phrase header2 = new Phrase("Kampioenschap seizoen  " + season, headerfont2);
            Paragraph par2 = new Paragraph(header2);
            par2.SpacingBefore = (30f);
            par2.SpacingAfter = (50f);
            par2.Alignment = (Element.ALIGN_CENTER);

            //footer 1
            iTextSharp.text.Font headerfont3 = FontFactory.GetFont("georgia", 8f);
            headerfont2.SetStyle("normal");
            Phrase footer1 = new Phrase("(resultaten gebaseerd op " + rule_set + " regel)", headerfont3);
            Paragraph par3 = new Paragraph(footer1);
            par3.SpacingBefore = (130f);
            par3.Alignment = (Element.ALIGN_RIGHT);

            //Creating iTextSharp Table from the DataTable data
            PdfPTable pdfTable = new PdfPTable(dataGridView1.ColumnCount);

            //set columnwidths - numbers are relative!
            float[] widths = new float[] { 16f, 8f, 8f, 9f, 4f, 4f, 4f, 7f, 10f, 7f, 4f, 7f, 6f, 8f };
            pdfTable.SetWidths(widths);
            pdfTable.DefaultCell.Padding = 3;
            pdfTable.WidthPercentage = 100;
            pdfTable.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfTable.DefaultCell.BorderWidth = 1;

            //Adding Header row
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                PdfPCell cell = new PdfPCell(new Phrase(column.HeaderText));
                cell.BackgroundColor = new iTextSharp.text.BaseColor(Color.Khaki);
                pdfTable.AddCell(cell);
            }

            //Adding DataRows
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                table_row++;
              
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value != null)
                    {
                        PdfPCell content = new PdfPCell(new Phrase(cell.Value.ToString()));
                        content.HorizontalAlignment = Element.ALIGN_RIGHT;
                            if (IsOdd(table_row))
                            {
                                content.BackgroundColor = new iTextSharp.text.BaseColor(Color.Bisque);
                            }
                            else
                            {
                                content.BackgroundColor = new iTextSharp.text.BaseColor(Color.LightYellow);
                            }
                        pdfTable.AddCell(content);
                    }
                }
            }

            //Exporting to PDF

            string folderPath = filepath_base + "\\running competition";

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            using (FileStream stream = new FileStream(folderPath + "\\championship.pdf", FileMode.Create))
            {

                iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(PageSize.A4.Rotate());
                Document pdfDoc = new Document(rec);
                PdfWriter.GetInstance(pdfDoc, stream);

                pdfDoc.Open();
                pdfDoc.Add(par1);
                pdfDoc.Add(par2);
                pdfDoc.Add(pdfTable);
                pdfDoc.Add(par3);
                pdfDoc.Close();

                stream.Close();
            }

            this.Close();

            // end of Form3
        }

        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }
    } 
} 
