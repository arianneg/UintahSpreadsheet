//Arianne Grimes u0830364
//Tyler Centini u0758592

using SS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SS
{
    public partial class Form1 : Form
    {
        private string ContentsInitial;
        private Spreadsheet sheet;
        private Socket server;
        /// <summary>
        /// Initalizes the spreadsheet with A1 cell selelcted, version is "ps6" by default
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            sheet = new Spreadsheet(s => true, Normalize, "ps6");
            spreadsheetPanel1.SelectionChanged += SpreadsheetPanel1_Selection;
            spreadsheetPanel1.SetSelection(0, 0);
            AddressLabel.Text = "A1:";
            this.Text = "Spreadsheet";
            this.serverTextBox.Text = "lab1-18.eng.utah.edu";
        }


        /// <summary>
        /// event that deels with selecting a cell, if cell already has contents it populates the approperiate labels with content information
        /// </summary>
        /// <param name="sender">spreadsheetPanel</param>
        private void SpreadsheetPanel1_Selection(SpreadsheetPanel sender)
        {
            //clears previous content
            ContentsBox.Clear();
            ValueBox.Clear();
            int row;
            int col;
            //used to get letter portion of cell name.
            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            spreadsheetPanel1.GetSelection(out col, out row);
            //adds 1 to row to set name in spreadsheet cell to match panel labels.
            row += 1;
            AddressLabel.Text = "" + alpha[col] + row + ":";
            //sets name for cell using row and col integers
            string name = "" + alpha[col] + row;

            string contents = sheet.GetCellContents(name).ToString();
            string value = sheet.GetCellValue(name).ToString();
            //checks if formula is an error if so sets cell contents to error message and error label.

            if (value == "SpreadsheetUtilities.FormulaError")
            {
                spreadsheetPanel1.SetValue(col, row - 1, "Formula Error");
                ValueBox.Text = "Formula Error";
            }
            //otherwise sets the value and proper content to appropreiate labels and cell.
            else
            {
                ValueBox.Text = value;
                spreadsheetPanel1.SetValue(col, row - 1, value);
            }
            //sets the contents of input box to what is in the current cell.
            ContentsBox.Focus();
            ContentsBox.Text = contents;
        }

        /// <summary>
        /// converst row and col integers into an appropreiate cell name.
        /// </summary>
        /// <param name="col">integer represents col location</param>
        /// <param name="row">integer represents row location</param>
        /// <returns></returns>
        private string ConvertColRowIntoName(int col, int row)
        {

            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            spreadsheetPanel1.GetSelection(out col, out row);
            row += 1;
            //  AddressLabel.Text = "" + alpha[col] + row + ":";
            string name = "" + alpha[col] + row;
            return "" + alpha[col] + row;
        }
        /// <summary>
        /// If set button is clicked, add cell contents to sheet, 
        /// give the GUI the value of the cell to display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            int row;
            int col;
            //gets current location of selected cell
            spreadsheetPanel1.GetSelection(out col, out row);
            //gets name of associated cell
            string name = ConvertColRowIntoName(col, row);
            ContentsBox.Focus();
            //recalculates any cells that are dependent on current cell
            try
            {
                ContentsInitial = ContentsBox.Text;
                HashSet<string> CellsRecalculate = (HashSet<string>)sheet.SetContentsOfCell(name, ContentsBox.Text);
                foreach (string cell in CellsRecalculate)
                {
                    int vertical;
                    int horizontal;
                    ConvertStringToRowAndColumn(cell, out vertical, out horizontal);
                    ContentsBox.Text = sheet.GetCellContents(cell).ToString();

                    string value = sheet.GetCellValue(cell).ToString();
                    //sets error message to box and value lebel if error in formula
                    if (value == "SpreadsheetUtilities.FormulaError")
                    {
                        spreadsheetPanel1.SetValue(vertical, horizontal, "Formula Error");
                        ValueBox.Text = "Formula Error";
                    }
                    else
                    {
                        spreadsheetPanel1.SetValue(vertical, horizontal, value);
                        ValueBox.Text = value;
                    }
                }
            }
            //creates message box with error message
            catch (SpreadsheetUtilities.FormulaFormatException)
            {
                DialogResult dialogResult = MessageBox.Show("Invalid formula syntax.", "Warning", MessageBoxButtons.OK);
            }
            catch (SS.CircularException)
            {
                DialogResult dialogResult = MessageBox.Show("Circular Dependency", "Warning", MessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// Converts cell name to a row and col intagter to be used for panel selection
        /// </summary>
        /// <param name="name"></param>
        /// <param name="col"></param>
        /// <param name="row"></param>
        private void ConvertStringToRowAndColumn(string name, out int col, out int row)
        {
            char colChar = name.ElementAt(0);
            col = Convert.ToInt32(colChar) - 65;
            string rowInt = name.Substring(1);
            Int32.TryParse(rowInt, out row);
            row = row - 1;
        }
        /// <summary>
        /// Creates a new spreadsheet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">event</param>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {

            NewApplicationContext.getAppContext().RunForm(new Form1());
        }


        /// <summary>
        /// handles when enter key is pressed, also clls arrowkey function of arrow keys are pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(this, new EventArgs());
                //supresses enter sound
                e.Handled = true;
                e.SuppressKeyPress = true;
                ContentsBox.Clear();
                ValueBox.Clear();
                int row;
                int col;
                spreadsheetPanel1.GetSelection(out col, out row);
                spreadsheetPanel1.SetSelection(col, row + 1);
                string name = ConvertColRowIntoName(col, row);
                AddressLabel.Text = name + ":";
            }
            //handles arrow keys
            ArrowKeyHandler(e);

        }
        /// <summary>
        /// Moves the spreadsheet selection of cells in directions of arrow keys
        /// </summary>
        /// <param name="e"></param>
        private void ArrowKeyHandler(KeyEventArgs e)
        {
            int col;
            int row;
            string value;
            spreadsheetPanel1.GetSelection(out col, out row);
            spreadsheetPanel1.GetValue(col, row, out value);
            ValueBox.Clear();
            //deals with right arrow key
            if (e.KeyCode == Keys.Right)
            {
                //sets only if no value is in the cell
                if (value == "")
                {
                    button1_Click(this, new EventArgs());
                }

                spreadsheetPanel1.SetSelection(col + 1, row);
                string name = ConvertColRowIntoName(col + 1, row);
                DisplayValue(name);
                

            }
            //deals with left arrow key pressed
            if (e.KeyCode == Keys.Left)
            {
                //sets only if no value is in the cell
                if (value == "")
                {
                    button1_Click(this, new EventArgs());
                }

                spreadsheetPanel1.SetSelection(col - 1, row);
                //sets valuebox to error if formula error
                string name = ConvertColRowIntoName(col + 1, row);
                DisplayValue(name);
               


            }
            //deals with up arrow key
            if (e.KeyCode == Keys.Up)
            {
                //sets only if no value is in the cell
                if (value == "")
                {
                    button1_Click(this, new EventArgs());
                }

                spreadsheetPanel1.SetSelection(col, row - 1);
                //sets valuebox to error if formula error
                string name = ConvertColRowIntoName(col + 1, row);
                DisplayValue(name);
               


            }
            //deals with down arrow key
            if (e.KeyCode == Keys.Down)
            {
                //sets only if no value is in the cell
                if (value == "")
                {
                    button1_Click(this, new EventArgs());
                }

                spreadsheetPanel1.SetSelection(col, row + 1);
                //sets valuebox to error if formula error
                string name = ConvertColRowIntoName(col + 1, row);
                DisplayValue(name);
                


            }

        }
        /// <summary>
        /// Helper method for ArrowKeyHandler. Displayes the value in the 
        /// ValueBox and the contents in the contents box.
        /// </summary>
        /// <param name="name"></param>
        private void DisplayValue(string name)
        {
            string value1 = sheet.GetCellValue(name).ToString();
            if (value1 == "SpreadsheetUtilities.FormulaError")
            {
                ValueBox.Text = "Formula Error";
            }
            else
            {
                ValueBox.Text = value1;
            }
            ContentsBox.Text = sheet.GetCellContents(name).ToString();
            AddressLabel.Text = name + ":";
        }

        /// <summary>
        /// deals withe save event under the file menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Displays a SaveFileDialog so the user can save the Image
            // assigned to file save.
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "sprd files (*.sprd)|*.sprd|All Files (*.*)|*.*";
            saveFileDialog1.Title = "Save a sprd File";
            saveFileDialog1.ShowDialog();
            //file must have a valid name
            if (saveFileDialog1.FileName != "")
            {
                sheet.Save(saveFileDialog1.FileName);
            }
        }
        /// <summary>
        /// delas with the load event under the file menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if current spreadsheet has changed, it will ask to save before loading another.
            if (sheet.Changed)
            {
                DialogResult dialogResult = MessageBox.Show("Do You Want To Save Your Data?", "Save", MessageBoxButtons.YesNo);
                //if user selects yes then it executes save method
                if (dialogResult == DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(this, new EventArgs());
                }
            }
            //opens file dialog box in c:
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "All Files (*.*)|*.*|sprd files (*.sprd)|*.sprd";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.ShowDialog();
            openFileDialog1.RestoreDirectory = true;
            //if no name is valid opens the spreadsheet file.
            if (openFileDialog1.FileName != "")
            {
                sheet = new Spreadsheet(openFileDialog1.FileName, x => true, Normalize, "ps6");
            }
            //adds al the items in the spreadsheet file into the spreadsheet grid
            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            List<char> list = alpha.ToList<char>();
            foreach (string name in sheet.GetNamesOfAllNonemptyCells())
            {
                int col = list.IndexOf(char.Parse(name.Substring(0, 1)));
                int row = int.Parse(name.Substring(1, 1));
                string value = sheet.GetCellValue(name).ToString();
                spreadsheetPanel1.SetValue(col, row - 1, value);
            }
        }
        /// <summary>
        /// deals withe the close item in the file menu dropdown and simply exits
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// closes form and checks if any changes were made to the spreadsheet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sheet.Changed)
            {
                DialogResult dialogResult = MessageBox.Show("Do you want to save your data?", "Save", MessageBoxButtons.YesNo);
                //if user selects yes then it opens dialog box and user can save
                if (dialogResult == DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(this, new EventArgs());
                }
            }
        }
        /// <summary>
        /// normalizer for Spreadsheet 
        /// makes all letters in variable names uppercase
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string Normalize(string s)
        {
            string result = "";
            foreach (char letter in s)
            {
                result = result + Char.ToUpper(letter);
            }
            return result;
        }
        /// <summary>
        /// Help Menu dialog information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Welcome to Tyler and Arianne's Spreadsheet. \nPress 'OK' to continue with the tutorial.", "Help", MessageBoxButtons.OKCancel);
            if (dialogResult == DialogResult.OK)
            {
                DialogResult dialogResult2 = MessageBox.Show("To enter a value into a cell:\nClick on the cell or naviagate to it using the arrow keys and begin typing.", "Help", MessageBoxButtons.OKCancel);
                if (dialogResult2 == DialogResult.OK)
                {
                    DialogResult dialogResult3 = MessageBox.Show("To set the cell's value:\n1. Press 'enter' or \n2. Press any arrow key or\n3. Click on another cell or\n4. Click on the '=' button. ", "Help", MessageBoxButtons.OKCancel);
                    if (dialogResult3 == DialogResult.OK)
                    {
                        DialogResult dialogResult35 = MessageBox.Show("To edit a cell's contents: \n1. Select the cell by clicking on it or navigating to it using the arrow keys.\n2. Edit the cell's contents in the white cell contents box above the grid.", "Help", MessageBoxButtons.OKCancel);
                        if (dialogResult35 == DialogResult.OK)
                        {
                            DialogResult dialogResult4 = MessageBox.Show("If a formula is entered that has incorrect syntax, or if a circular dependency occures, an appropriate warning dialog box will appear.", "Help", MessageBoxButtons.OKCancel);
                            if (dialogResult4 == DialogResult.OK)
                            {
                                DialogResult dialogResult6 = MessageBox.Show("Save, Open, New, and Close features can be accessed under the 'File' tab.", "Help", MessageBoxButtons.OKCancel);
                                if (dialogResult6 == DialogResult.OK)
                                {
                                    DialogResult dialogResult7 = MessageBox.Show("If there are unsaved changes in the spreadsheet and Close is selected, a dialog box asking to Save will appear.", "Help", MessageBoxButtons.OKCancel);
                                    if (dialogResult7 == DialogResult.OK)
                                    {
                                        DialogResult dialogResult8 = MessageBox.Show("End of tutorial.", "Help", MessageBoxButtons.OKCancel);
                                    }
                                }
                                else
                                {
                                    return;
                                }

                            }
                            else
                            {
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }

                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.server = Networking.ConnectToServer(new Action<SocketState>(this.FirstContact), this.serverTextBox.Text);
        }

        private void FirstContact(SocketState ss)
        {
            ss.callMe = new Action<SocketState>(this.ReceiveStartup);
            Networking.Send(this.server, this.nameTextBox.Text + '\n');
        }

        private void ReceiveStartup(SocketState ss)
        {
            MessageBox.Show("Message Received!");
            
        }
    }
}