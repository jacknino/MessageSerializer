using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using MessageSerializer;
using GenericLogParser;

namespace MessageSerializerClassFileCreator
{
    public partial class FormMain : Form
    {
        private const string DefaultOutputFilename = "{ClassName}.xml";
        private CommandLineParser _commandLineParser;
        private string _dllNameOnEnter;
        private bool _classesWereSpecifiedThatCouldNotBeLoaded;

        public FormMain()
        {
            InitializeComponent();

            SerializerClassGeneration.WriteCodeAndDebugInfoToDisk = true;
            _commandLineParser = new CommandLineParser("", true);
            SetValuesFromCommandLine(_commandLineParser);
        }

        private void SetValuesFromCommandLine(CommandLineParser commandLineParser)
        {
            if (commandLineParser.ContainsKey("DLL"))
            {
                textBoxDll.Text = commandLineParser["DLL"];
                DisplayClasses();
            }

            if (commandLineParser.ContainsKey("OutputDirectory"))
                textBoxOutputDirectory.Text = commandLineParser["OutputDirectory"];

            if (commandLineParser.ContainsKey("OutputFilename"))
                textBoxOutputFilename.Text = commandLineParser["OutputFilename"];

            _classesWereSpecifiedThatCouldNotBeLoaded = false;
            if (commandLineParser.ContainsKey("Class"))
            {
                List<string> classNames = commandLineParser["Class"].Split('\t').ToList();
                foreach (string className in classNames)
                {
                    bool foundClass = false;
                    foreach (Object item in listBoxClasses.Items)
                    {
                        if (item.ToString() == className)
                        {
                            listBoxClasses.SelectedItems.Add(item);
                            foundClass = true;
                            break;
                        }
                    }

                    if (!foundClass)
                    {
                        AddStatus(string.Format("A class was specified on the command line called {0} but it could not be loaded", className));
                        _classesWereSpecifiedThatCouldNotBeLoaded = true;
                    }
                }
            }
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxOutputFilename.Text))
                textBoxOutputFilename.Text = DefaultOutputFilename;

            if (_commandLineParser.ContainsKey("Process"))
                DoProcessing();
        }

        private OpenFileDialog SetupAndShowOpenFileDialog(
            string fileName,
            string filter,
            bool fileMustExist,
            bool multiSelect)
        {
            if (filter == "")
                filter = "Log Files (*.dll)|*.dll|All Files (*.*)|*.*";

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = fileMustExist;
            openFileDialog.Filter = filter;
            openFileDialog.FileName = fileName;
            openFileDialog.Multiselect = multiSelect;

            if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                return null;

            return openFileDialog;
        }

        private string[] SelectFiles(string fileName, string filter, bool fileMustExist)
        {
            OpenFileDialog openFileDialog = SetupAndShowOpenFileDialog(fileName, filter, fileMustExist, false);
            return (openFileDialog != null ? openFileDialog.FileNames : null);
        }

        private void buttonSelectDll_Click(object sender, EventArgs e)
        {
            string[] fileArray = SelectFiles(textBoxDll.Text, "", true);

            if (fileArray != null && fileArray.Length > 0)
            {
                string dllName = fileArray[0];
                textBoxDll.Text = dllName;

                textBoxOutputDirectory.Text = Path.GetDirectoryName(dllName);
                DisplayClasses();
            }
        }

        private void DisplayClasses()
        {
            try
            {
                listBoxClasses.Items.Clear();
                string dllName = textBoxDll.Text;
                AddStatus(string.Format("Loading classes from {0}", dllName));
                Assembly assembly = Assembly.LoadFrom(dllName);
                foreach (Type type in assembly.GetTypes())
                {
                    listBoxClasses.Items.Add(type);
                }
                AddStatus(string.Format("Loaded {0} classes from {1}", listBoxClasses.Items.Count, dllName));
            }
            catch (Exception ex)
            {
                AddStatus(string.Format("Couldn't add {0}: {1}", textBoxDll.Text, ex));
            }
        }

        private void textBoxDll_Enter(object sender, EventArgs e)
        {
            _dllNameOnEnter = textBoxDll.Text;
        }

        private void textBoxDll_Leave(object sender, EventArgs e)
        {
            // If the DLL name changed we need to reload the list of classes
            if (textBoxDll.Text != _dllNameOnEnter)
                DisplayClasses();
        }

        private void buttonOutputDirectory_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = Path.GetDirectoryName(textBoxDll.Text);
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxOutputDirectory.Text = folderBrowserDialog.SelectedPath;
            }
        }

        // Note: This is run on a separate thread, mostly so the status can be updated while it is being run
        void ProcessClasses(Object stateInfo)
        {
            bool successful = false;

            do
            {
                ClassData classData = (ClassData)stateInfo;

                if (classData.Classes == null || classData.Classes.Count == 0)
                {
                    AddStatus("There were no classes specified to process");
                    break;
                }

                try
                {
                    if (!Directory.Exists(classData.OutputDirectory))
                    {
                        AddStatus(string.Format("Creating output directory {0}", classData.OutputDirectory));
                        Directory.CreateDirectory(classData.OutputDirectory);
                        AddStatus(string.Format("Created output directory {0}", classData.OutputDirectory));
                    }
                }
                catch (Exception ex)
                {
                    AddStatus(string.Format("Failed trying to create output directory {0}: {1}", classData.OutputDirectory, ex));
                    break;
                }

                bool allClassesSuccessful = true;
                AddStatus(string.Format("Processing {0} classes", classData.Classes.Count));
                foreach (Object itemObject in classData.Classes)
                {
                    Type type = (Type) itemObject;
                    string filename = classData.FilenameFormat;
                    filename = filename.Replace("{ClassName}", type.Name);
                    filename = Path.Combine(classData.OutputDirectory, filename);

                    AddStatus(string.Format("Creating class description for {0} in {1}", type.FullName, filename));
                    try
                    {
                        ConfigMessageSerializerClass.WriteDefaultToFile(filename, type);
                        AddStatus(string.Format("Created class description for {0} in {1}", type.FullName, filename));
                    }
                    catch (Exception ex)
                    {
                        AddStatus(string.Format("Failed trying to create file for {0}: {1}", type.FullName, ex));
                        allClassesSuccessful = false;
                    }
                }

                successful = allClassesSuccessful;
            } while (false);

            AddStatus("Done processing classes");
            ProcessingComplete(successful);
        }

        void DoProcessing()
        {
            ClassData classData = new ClassData();
            classData.Classes = listBoxClasses.SelectedItems.Cast<Object>().ToList();
            classData.FilenameFormat = textBoxOutputFilename.Text;
            classData.OutputDirectory = textBoxOutputDirectory.Text;

            ThreadPool.QueueUserWorkItem(ProcessClasses, classData);
        }

        private void buttonProcess_Click(object sender, EventArgs e)
        {
            DoProcessing();
        }

        private delegate void AddStatusCallback(string status);
        private void AddStatus(string status)
        {
            if (richTextBoxStatus.InvokeRequired)
            {
                AddStatusCallback callback = AddStatus;
                richTextBoxStatus.Parent.Invoke(callback, new object[] { status });
            }
            else
            {
                richTextBoxStatus.AppendText(status + "\r\n");
            }
        }

        private delegate void ProcessingCompleteCallback(bool successful);
        private void ProcessingComplete(bool successful)
        {
            successful &= !_classesWereSpecifiedThatCouldNotBeLoaded;
            if (InvokeRequired)
            {
                ProcessingCompleteCallback callback = ProcessingComplete;
                Invoke(callback, new object[] { successful });
            }
            else if (_commandLineParser.ContainsKey("Process")
                && (_commandLineParser.ContainsKey("ExitWhenCompleteAll") || (_commandLineParser.ContainsKey("ExitWhenCompleteSuccess") && successful)))
            {
                Close();
            }
        }
    }
}
