using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpreadsheetUtilities;
using System.Xml;
//Author: Tyler Centini
//Date: 10/7/2016
namespace SS
{
    /// A spreadsheet contains a cell corresponding to every possible cell name.  (This
    /// means that a spreadsheet contains an infinite number of cells.)  In addition to 
    /// a name, each cell has a contents and a value.  The distinction is important.
    /// 
    /// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
    /// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
    /// of a cell in Excel is what is displayed on the editing line when the cell is selected.)
    /// 
    /// In a new spreadsheet, the contents of every cell is the empty string.
    ///  
    /// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
    /// (By analogy, the value of an Excel cell is what is displayed in that cell's position
    /// in the grid.)
    /// 
    /// If a cell's contents is a string, its value is that string.
    /// 
    /// If a cell's contents is a double, its value is that double.
    /// 
    /// If a cell's contents is a Formula, its value is either a double or a FormulaError,
    /// as reported by the Evaluate method of the Formula class.  The value of a Formula,
    /// of course, can depend on the values of variables.  The value of a variable is the 
    /// value of the spreadsheet cell it names (if that cell's value is a double) or 
    /// is undefined (otherwise).
    /// 
    /// Spreadsheets are never allowed to contain a combination of Formulas that establish
    /// a circular dependency.  A circular dependency exists when a cell depends on itself.
    public class Spreadsheet : AbstractSpreadsheet
    {
        private DependencyGraph graph;
        private Dictionary<string, cell> dictionary;
        private Boolean stateChanged;
        public Spreadsheet() :
            this(s => true, s => s, "default")
        {

        }
        /// <summary>
        /// Spreadsheet contructor that makes a empty spreadsheet takes in user defined variable validator and Normalizer and sets spreadsheet version
        /// </summary>
        /// <param name="isValid"></param>
        /// <param name="normalize"></param>
        /// <param name="version"></param>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
            graph = new DependencyGraph();
            dictionary = new Dictionary<string, cell>();
        }
        /// <summary>
        ///  Spreadsheet contructor that reconstrcts an empty spreadsheet takes in user defined variable validator and Normalizer and sets spreadsheet version
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="isValid"></param>
        /// <param name="normalize"></param>
        /// <param name="version"></param>
        public Spreadsheet(string filepath, Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
            graph = new DependencyGraph();
            dictionary = new Dictionary<string, cell>();
            try
            {
                //creates xml reader and closes it at the end
                using (XmlReader reader = XmlReader.Create(filepath))
                {
                    //gets the version of the spreadsheat
                    if (reader.ReadToFollowing("spreadsheet"))
                    {
                        string versionType = reader.GetAttribute("version");
                        if (versionType != version)
                        {
                            throw new SpreadsheetReadWriteException("Check spreadsheet version");
                        }
                    }

                    string name = null;
                    string content = null;
                    //reads given file and gets all the names of the cells and its contents
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            //gets name
                            if (reader.Name == "name")
                            {
                                reader.Read();
                                name = reader.Value;
                            }
                            //gets contents
                            if (reader.Name == "contents")
                            {
                                reader.Read();
                                content = reader.Value;
                            }
                        }
                        //sets up the cells and adds the names and cells to dictionary
                        if (name != null && content != null)
                        {
                            isValidName(name);
                            name = Normalize(name);
                            double doubleCheck;
                            //calls correct set method for double
                            if (double.TryParse(content, out doubleCheck))
                            {
                                SetCellContents(name, doubleCheck);
                            }
                            char c = char.Parse(content.Substring(0, 1));
                            //calls correct set method for formula
                            if (c == '=')
                            {
                                Formula f = new Formula(content.Substring(1, content.Length - 1), Normalize, IsValid);
                                SetCellContents(name, f);
                            }
                            //calls correct set method for string
                            else
                            {
                                SetCellContents(name, content);
                            }
                            //sets names and contents back to null for new values to be checked
                            name = null;
                            content = null;
                        }
                    }
                }
                //initializes that this is the starting state of spreadsheet.
                Changed = false;
            }
            catch
            {
                throw new SpreadsheetReadWriteException("Cannot read file, check file path, or file format");
            }
        }
        // ADDED FOR PS5
        /// <summary>
        /// True if this spreadsheet has been modified since it was created or saved                  
        /// (whichever happened most recently); false otherwise.
        /// </summary>
        public override bool Changed
        {

            get
            {
                return stateChanged;
            }

            protected set
            {
                stateChanged = value;
            }
        }
        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        public override object GetCellContents(string name)
        {
            //if the name is valid gets the cell and returns its contents
            //this method also throws exceptions

            isValidName(name);
            name = Normalize(name);
            //if named cell is present gets it contents
            if (dictionary.ContainsKey(name))
            {
                cell cell = new cell();
                dictionary.TryGetValue(name, out cell);
                double doubleCheck;
                //for double content
                if (cell.getContent().GetType() == typeof(Double))
                {
                    return cell.getContent();
                }
                //for formula content
                bool isFormula = cell.getContent().GetType() == typeof(Formula);
                if (isFormula)
                {
                    Formula f = (Formula)cell.getContent();
                    return f;
                }
                //for string content
                bool isString = cell.getContent().GetType() == typeof(String);
                if (isString)
                {
                    return (String)cell.getContent();
                }

            }
            //for empty cell
            return "";
        }
        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            LinkedList<string> names = new LinkedList<string>();
            foreach (string name in dictionary.Keys)
            {
                cell check = new cell();
                dictionary.TryGetValue(name, out check);
                //if content is not empty add to the list
                if (check.getContent() != "")
                {
                    names.AddFirst(name);
                }
            }
            return names;
        }
        /// <summary>
        /// If the formula parameter is null, throws an ArgumentNullException.
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException.  (No change is made to the spreadsheet.)
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// Set consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, Formula formula)
        {
            if (formula == null)
            {
                throw new ArgumentNullException();
            }
            //checks if its a valid name
            isValidName(name);
            name = Normalize(name);
            cell cell = new cell();
            ISet<string> set = new HashSet<string>();
            //if the cell isnt already created for that name create it.
            if (!dictionary.ContainsKey(name))
            {
                //create cell and add the formula to it              
                cell.setContent(formula);
                //add the cell to the dictionary the name is the key.
                dictionary.Add(name, cell);
                set.Add(name);

                //sets the variables in the formula as a dependee of the new cell
                foreach (string dependees in formula.GetVariables())
                {

                    graph.AddDependency(dependees, name);
                }
                //checks for circular dependency and recalculates the cells nesseccary
                GetCellsToRecalculate(set);
                //gets all the dependents of the cell and adds to the set
                RecursivlyGetDependents(name, set);
                //checks for circular dependency and recalculates the cells nesseccary
                GetCellsToRecalculate(set);
                return set;
            }
            //if the cell is already created remove it from the dictionary
            else if (dictionary.ContainsKey(name))
            {
                //gets the cell out and removes any dependees from the cell
                dictionary.TryGetValue(name, out cell);
                bool isFormula = cell.getContent().GetType() == typeof(Formula);
                if (isFormula)
                {
                    Formula f = (Formula)cell.getContent();
                    foreach (string dependee in f.GetVariables())
                    {
                        graph.RemoveDependency(dependee, name);
                    }
                }
                set.Add(name);
                //addes new dependees to the cell if any
                foreach (string dependee in formula.GetVariables())
                {
                    graph.AddDependency(dependee, name);
                }
                //gets all the dependents of the cell and adds to the set
                GetCellsToRecalculate(set);
                RecursivlyGetDependents(name, set);
                //checks for circular dependency and recalculates the cells nesseccary
                GetCellsToRecalculate(set);
                dictionary.Remove(name);
                //sets the new formula
                cell.setContent(formula);
                //adds the new cell to the dictionary
                dictionary.Add(name, cell);
                return set;
            }
            return set;
        }
        /// <summary>
        /// gets dependents of given variable, and the dependents of those variables.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="set"></param>
        private void RecursivlyGetDependents(string name, ISet<string> set)
        {
            name = Normalize(name);
            foreach (string dependent in graph.GetDependents(name))
            {
                set.Add(dependent);
                RecursivlyGetDependents(dependent, set);
            }
        }
        /// <summary>
        /// checks if name is a valid variable for a cell.
        /// </summary>
        /// <param name="name"></param>
        private void isValidName(string name)
        {
            if (name == null)
            {
                throw new InvalidNameException();
            }
            char c = char.Parse(name.Substring(0, 1));
            //checks if the name starts with a digit
            if (!char.IsLetter(c))
            {
                throw new InvalidNameException();
            }
            Boolean letterAfterNumber = false;
            char[] varcheck = name.ToCharArray();
            //checks the characters in string for correct variable format
            foreach (char ch in varcheck)
            {
                //makes sure first character is a letter             
                if (char.IsLetter(ch) && letterAfterNumber == false)
                {
                    continue;
                }
                //everything after should be a digit
                else if (char.IsNumber(ch))
                {
                    letterAfterNumber = true;
                    continue;
                }
                //if its not its not a valid name
                else
                {
                    throw new InvalidNameException();
                }
            }
            name = Normalize(name);
            if (!IsValid(name))
            {
                throw new InvalidNameException();
            }
        }
        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException();
            }

            //if its a valid name create a cell add it to a dictionary and adds it name to the set 
            isValidName(name);
            cell cell = new cell();
            ISet<string> set = new HashSet<string>();
            if (!dictionary.ContainsKey(name))
            {
                cell.setContent(text);
                dictionary.Add(name, cell);
                set.Add(name);
                //gets any of the cells that depend on this one and recalculates
                RecursivlyGetDependents(name, set);
                GetCellsToRecalculate(set);
                return set;
            }
            //if cell already exists removes it from the dictionary, and removes its dependencies from the graph
            else if (dictionary.ContainsKey(name))
            {
                dictionary.TryGetValue(name, out cell);
                bool isFormula = cell.getContent().GetType() == typeof(Formula);
                if (isFormula)
                {
                    Formula f = (Formula)cell.getContent();
                    foreach (string dependee in f.GetVariables())
                    {
                        graph.RemoveDependency(dependee, name);
                    }
                }
                set.Add(name);
                //gets any of the cells that depend on this one and recalculates
                RecursivlyGetDependents(name, set);
                GetCellsToRecalculate(set);
                dictionary.Remove(name);
                cell.setContent(text);
                dictionary.Add(name, cell);
                return set;
            }
            return set;
        }
        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// set consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        protected override ISet<string> SetCellContents(string name, double number)
        {
            if (name == null)
            {
                throw new InvalidNameException();
            }
            //if its a valid name create a cell add it to a dictionary add it to the set  
            isValidName(name);
            cell cell = new cell();
            ISet<string> set = new HashSet<string>();
            if (!dictionary.ContainsKey(name))
            {
                cell.setContent(number);
                dictionary.Add(name, cell);
                set.Add(name);
                //gets any of the cells that depend on this one and recalculates
                RecursivlyGetDependents(name, set);
                GetCellsToRecalculate(set);
                return set;
            }
            //if cell already exists removes it from the dictionary, and removes its dependencies from the graph
            else if (dictionary.ContainsKey(name))
            {
                dictionary.TryGetValue(name, out cell);
                bool isFormula = cell.getContent().GetType() == typeof(Formula);
                if (isFormula)
                {
                    Formula f = (Formula)cell.getContent();
                    foreach (string dependee in f.GetVariables())
                    {
                        graph.RemoveDependency(dependee, name);
                    }
                }
                set.Add(name);
                //gets any of the cells that depend on this one and recalculates
                RecursivlyGetDependents(name, set);
                GetCellsToRecalculate(set);
                dictionary.Remove(name);
                cell.setContent(number);
                dictionary.Add(name, cell);
                return set;
            }
            return set;
        }
        /// <summary>
        /// If name is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name isn't a valid cell name, throws an InvalidNameException.
        /// 
        /// Otherwise, returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException();
            }
            isValidName(name);
            name = Normalize(name);
            return graph.GetDependents(name);
        }
        /// <summary>
        /// Returns the version information of the spreadsheet saved in the named file.
        /// If there are any problems opening, reading, or closing the file, the method
        /// should throw a SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override string GetSavedVersion(string filename)
        {
            try
            {
                string versionType = null;
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    //gets version from xml file
                    if (reader.ReadToFollowing("spreadsheet"))
                    {
                        versionType = reader.GetAttribute("version");
                    }
                }
                return versionType;
            }
            catch
            {
                throw new SpreadsheetReadWriteException("Cannot read file, check file path, or file format");
            }
        }
        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using an XML format.
        /// The XML elements should be structured as follows:
        /// 
        /// <spreadsheet version="version information goes here">
        /// 
        /// <cell>
        /// <name>
        /// cell name goes here
        /// </name>
        /// <contents>
        /// cell contents goes here
        /// </contents>    
        /// </cell>
        /// 
        /// </spreadsheet>
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.  
        /// If the cell contains a string, it should be written as the contents.  
        /// If the cell contains a double d, d.ToString() should be written as the contents.  
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        public override void Save(string filename)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            try
            {   //Opens writer and creates a file to write too then closes file
                using (XmlWriter writer = XmlWriter.Create(filename, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("spreadsheet");
                    writer.WriteAttributeString(null, "version", null, Version);
                    //goes through dictionary getting names and cells
                    foreach (string name in dictionary.Keys)
                    {
                        cell cell = new cell();
                        dictionary.TryGetValue(name, out cell);
                        //if empty cell dont write it to xml
                        if (GetCellContents(name) == "")
                        {
                            continue;
                        }
                        writer.WriteStartElement("cell");
                        writer.WriteElementString("name", name);
                        double doubleCheck;
                        //if double sets that to contents element
                        if (double.TryParse(GetCellContents(name).ToString(), out doubleCheck))
                        {
                            writer.WriteElementString("contents", doubleCheck.ToString());
                            writer.WriteEndElement();
                        }
                        //if formula puts that as element content
                        else if (GetCellContents(name).GetType() == typeof(Formula))
                        {
                            Formula f = (Formula)cell.getContent();

                            writer.WriteElementString("contents", "=" + f.ToString());
                            writer.WriteEndElement();
                        }
                        //if string sets that to element content
                        else if (GetCellContents(name).GetType() == typeof(String))
                        {
                            writer.WriteElementString("contents", (String)cell.getContent());
                            writer.WriteEndElement();
                        }
                    }
                    //ends document
                    writer.WriteEndDocument();
                }
            }
            catch
            {
                throw new SpreadsheetReadWriteException("Saving file failed, file path may be incorrect");
            }
        }
        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        public override object GetCellValue(string name)
        {
            //if the name is valid gets the cell and returns its value
            isValidName(name);
            if (dictionary.ContainsKey(name))
            {
                cell cell = new cell();
                dictionary.TryGetValue(name, out cell);
                double doubleCheck;
                //gets double value and returns it
                if (double.TryParse(cell.getContent().ToString(), out doubleCheck))
                {
                    return doubleCheck;
                }
                //gets evaluates formaule and returns is calculated value and returns it
                bool isFormula = cell.getContent().GetType() == typeof(Formula);
                if (isFormula)
                {
                    Formula f = (Formula)cell.getContent();
                    return f.Evaluate(lookup);
                }
                //returns string value of cell
                bool isString = cell.getContent().GetType() == typeof(String);
                if (isString)
                {
                    return (String)cell.getContent();
                }

            }
            //if cell is empty
            return "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private double lookup(string name)
        {
            //if name does not exsists thwos exception
            if (!dictionary.ContainsKey(name))
            {
                throw new ArgumentException();
            }
            if (dictionary.ContainsKey(name))
            {
                cell cell = new cell();
                dictionary.TryGetValue(name, out cell);
                double doubleCheck;
                //if named cell has a double value return it to the formula class 
                if (double.TryParse(cell.getContent().ToString(), out doubleCheck))
                {
                    return doubleCheck;
                }
                object value;
                //if the named cell has its own formula recursivly get its value
                bool isFormula = cell.getContent().GetType() == typeof(Formula);
                if (isFormula)
                {
                    Formula f = (Formula)cell.getContent();
                    value = f.Evaluate(lookup);
                    if (double.TryParse(value.ToString(), out doubleCheck))
                    {
                        return doubleCheck;
                    }
                    //if any of the dependent cells have error it is propagated
                    bool isError = value.GetType() == typeof(FormulaError);
                    if (isError)
                    {
                        throw new ArgumentException();
                    }

                }
                else
                {
                    throw new ArgumentException();
                }

            }
            //should never reach here
            return 0;
        }

        /// <summary>
        /// If content is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if content parses as a double, the contents of the named
        /// cell becomes that double.
        /// 
        /// Otherwise, if content begins with the character '=', an attempt is made
        /// to parse the remainder of content into a Formula f using the Formula
        /// constructor.  There are then three possibilities:
        /// 
        ///   (1) If the remainder of content cannot be parsed into a Formula, a 
        ///       SpreadsheetUtilities.FormulaFormatException is thrown.
        ///       
        ///   (2) Otherwise, if changing the contents of the named cell to be f
        ///       would cause a circular dependency, a CircularException is thrown.
        ///       
        ///   (3) Otherwise, the contents of the named cell becomes f.
        /// 
        /// Otherwise, the contents of the named cell becomes content.
        /// 
        /// If an exception is not thrown, the method returns a set consisting of
        /// name plus the names of all other cells whose value depends, directly
        /// or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// set {A1, B1, C1} is returned.
        /// </summary>
        public override ISet<string> SetContentsOfCell(string name, string content)
        {
            if (content == null)
            {
                throw new ArgumentNullException();
            }
            //checks for valid name and throws exception
            isValidName(name);
            name = Normalize(name);
            ISet<string> cellNames = new HashSet<string>();
            double doubleCheck;
            //if its a double calls correct setcontent for double
            if (double.TryParse(content, out doubleCheck))
            {
                cellNames = SetCellContents(name, doubleCheck);
            }
            //if empty creates empty cell
            else if (content == "")
            {
                cellNames = SetCellContents(name, content);
                return cellNames;
            }
            //if formula sets contents of cell to formula
            else if (char.Parse(content.Substring(0, 1)) == '=')
            {
                Formula f = new Formula(content.Substring(1, content.Length - 1), Normalize, IsValid);
                cellNames = SetCellContents(name, f);
            }
            //else its a string and sets it
            else
            {
                cellNames = SetCellContents(name, content);
            }
            //calculates cells from new changes.
            //GetCellValue(name);
            Changed = true;
            return cellNames;
        }

        /// <summary>
        /// Creates a cell that holds either double, string, or formula
        /// </summary>
        private class cell
        {
            private object content;

            /// <summary>
            /// sets the contents of the cell
            /// </summary>
            /// <param name="content"></param>
            public void setContent(Object content)
            {
                this.content = content;
            }
            /// <summary>
            /// returns the contents of the cell
            /// </summary>
            /// <returns>content</returns>
            public Object getContent()
            {
                return content;
            }

        }
    }
}