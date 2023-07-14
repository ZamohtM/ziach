using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using WindowsFormsApp1;

namespace dotNetDesktop
{
    public partial class HarmonikaSystem : Form
    {

        private List<Control> buttonList;
        private List<Note> noteList;
        private Dictionary<string, int> arduinoValues;
        private bool recording;
        private string currentSong;
        private Stopwatch swNoteDuration;
        private int index;

        public HarmonikaSystem()
        {
            buttonList = new List<Control>();
            noteList = new List<Note>();
            arduinoValues = new Dictionary<string, int>();
            currentSong = "";
            swNoteDuration = new Stopwatch();

            InitializeComponent();
            fillButtonList();
            fillDictionary();
            serialPort1.Open();

            foreach (var item in arduinoValues)
            {
                Console.WriteLine(item);
            }

        }

        private void fillDictionary()
        {
            int tempCounter = 1;

            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Button && (ctrl.Name.Contains("keyboard") == true))
                {
                    arduinoValues.Add(ctrl.Name + "GreenON", tempCounter);
                    tempCounter++;
                    arduinoValues.Add(ctrl.Name + "BlueON", tempCounter);
                    tempCounter++;
                    arduinoValues.Add(ctrl.Name + "OFF", tempCounter);
                    tempCounter++;
                }
            }

        }

        private void fillButtonList()
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Button && (ctrl.Name.Contains("keyboard") == true))
                {
                    buttonList.Add(ctrl);
                }
            }
        }

        private void btnStartRecording_Click(object sender, EventArgs e)
        {
            noteList.Clear();

            btnSelectSong.Enabled = false;
            btnStopRecording.Enabled = true;

            if (recording == false)
            {
                recording = true;
                btnCurrentlyRecording.BackColor = Color.Green;
            }
        }

        private void btnStopRecording_Click(object sender, EventArgs e)
        {
            btnStopRecording.Enabled = false;
            btnSaveRecording.Enabled = true;

            if (recording == true)
            {
                recording = false;
                btnCurrentlyRecording.BackColor = SystemColors.Control;
            }
        }

        private void btnSaveRecording_Click(object sender, EventArgs e)
        {
            btnSaveRecording.Enabled = false;
            btnSelectSong.Enabled = true;

            string jsonOutput = JsonConvert.SerializeObject(noteList, Formatting.Indented);

            string songNameInput = Interaction.InputBox("Bitte geben Sie den Namen des Stückes ein.", "Save Song As", "z.B.: 'AlleMeineEntchen'", 0, 0);

            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\HarmonikaSystem\\Songs\\" + songNameInput + ".json";

            if (!File.Exists(path))
            {
                File.WriteAllText(path, jsonOutput);
                MessageBox.Show("Stück gespeichert!");
                noteList.Clear();
            }
            else
            {
                MessageBox.Show("Stück existiert bereits!");
                noteList.Clear();
            }
        }

        private void btnSelectSong_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\HarmonikaSystem\\Songs\\",
                Title = "Browse Song Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "json",
                Filter = "json files (*.json)|*.json",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtFile.Text = openFileDialog1.FileName;
                currentSong = File.ReadAllText(txtFile.Text);
                btnPlaySong.Enabled = true;
            }
        }

        private async void btnPlaySong_Click(object sender, EventArgs e)
        {
            btnStopSong.Enabled = true;
            btnStartRecording.Enabled = false;

            foreach (var button in buttonList)
            {
                button.BackColor = Color.White;
                await Task.Delay(50);
                int temp = arduinoValues[button.Name.ToString() + "OFF"];
                serialPort1.Write(temp.ToString());
                button.Enabled = false;
            }

            var jArrayT = JArray.Parse(currentSong);

            foreach (var item in jArrayT)
            {
                switch (item["aufzugOrZuzug"].ToString())
                {
                    case "0":
                        Console.WriteLine("Zuzug");

                        int durationZuzug = (int)item["duration"];
                        string[] keysListZuzug = item["keys"].ToString().Split(',');

                        foreach (var key in keysListZuzug)
                        {
                            foreach (var button in buttonList)
                            {
                                if (button.Name == key)
                                {
                                    button.BackColor = Color.White;
                                    await Task.Delay(20);
                                    button.BackColor = Color.Green;
                                    int temp = arduinoValues[button.Name.ToString()+"GreenON"];
                                    Console.WriteLine(temp);
                                    serialPort1.Write(temp.ToString());
                                } 
                            }
                        }

                        foreach (var key in keysListZuzug)
                        {
                            foreach (var button in buttonList)
                            {
                                if (button.Name == key)
                                {
                                    while (button.Enabled != true)
                                    {
                                        await Task.Delay(1);
                                    }
                                    button.Enabled = false;
                                }
                            }

                            await Task.Delay(durationZuzug);

                        }

                        foreach (var key in keysListZuzug)
                        {
                            foreach (var button in buttonList)
                            {
                                if (button.Name == key)
                                {
                                    await Task.Delay(20);
                                    button.BackColor = Color.White;
                                    int temp = arduinoValues[button.Name.ToString() + "OFF"];
                                    Console.WriteLine(temp);
                                    serialPort1.Write(temp.ToString());
                                    button.Enabled = false;
                                }
                            }
                        }

                        break;

                    case "1":
                        Console.WriteLine("Aufzug");

                        int durationAufzug = (int)item["duration"];
                        string[] keysListAufzug = item["keys"].ToString().Split(',');

                        foreach (var key in keysListAufzug)
                        {
                            foreach (var button in buttonList)
                            {
                                if (button.Name == key)
                                {
                                    button.BackColor = Color.White;
                                    await Task.Delay(20);
                                    button.BackColor = Color.Blue;
                                    int temp = arduinoValues[button.Name.ToString() + "BlueON"];
                                    serialPort1.Write(temp.ToString());
                                }
                            }
                        }

                        foreach (var key in keysListAufzug)
                        {
                            foreach (var button in buttonList)
                            {
                                if (button.Name == key)
                                {
                                    while (button.Enabled != true)
                                    {
                                        await Task.Delay(1);
                                    }
                                    button.Enabled = false;
                                }
                            }

                            await Task.Delay(durationAufzug);
                        }

                        foreach (var key in keysListAufzug)
                        {
                            foreach (var button in buttonList)
                            {
                                if (button.Name == key)
                                {
                                    await Task.Delay(20);
                                    button.BackColor = Color.White;
                                    int temp = arduinoValues[button.Name.ToString() + "OFF"];
                                    Console.WriteLine(temp);
                                    serialPort1.Write(temp.ToString());
                                    button.Enabled = false;
                                }
                            }
                        }

                        break;

                    default:
                        break;
                }
            }
        }

        private void btnStartRecording_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            swNoteDuration.Start();
        }

        private void btnStartRecording_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (recording == true)
            {

                if (Keyboard.IsKeyDown(Key.Space))
                {
                    swNoteDuration.Stop();

                    if (swNoteDuration.ElapsedMilliseconds == 0)
                    {
                        if (e.KeyCode.ToString() != "Space")
                        {
                            noteList[index - 1].keys += "," + "keyboard" + e.KeyCode.ToString();
                        }
                    }
                    else
                    {
                        noteList.Add(new Note { keys = "keyboard" + e.KeyCode.ToString(), duration = swNoteDuration.ElapsedMilliseconds, noteIndex = index, aufzugOrZuzug = 1 });
                        index++;
                    }

                    swNoteDuration.Reset();
                }
                else
                {
                    swNoteDuration.Stop();

                    if (swNoteDuration.ElapsedMilliseconds == 0)
                    {
                        if (e.KeyCode.ToString() != "Space")
                        {
                            noteList[index - 1].keys += "," + "keyboard" + e.KeyCode.ToString();
                        }
                    }
                    else
                    {
                        noteList.Add(new Note { keys = "keyboard" + e.KeyCode.ToString(), duration = swNoteDuration.ElapsedMilliseconds, noteIndex = index, aufzugOrZuzug = 0 });
                        index++;
                    }

                    swNoteDuration.Reset();
                }

            }
            else
            {
                MessageBox.Show("Currently not recording, press start!");
            }
        }

        private void btnPlaySong_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D0:
                    keyboardD0.Enabled = true;
                    break;
                case Keys.D1:
                    keyboardD1.Enabled = true;
                    break;
                case Keys.D2:
                    keyboardD2.Enabled = true;
                    break;
                case Keys.D3:
                    keyboardD3.Enabled = true;
                    break;
                case Keys.D4:
                    keyboardD4.Enabled = true; ;
                    break;
                case Keys.D5:
                    keyboardD5.Enabled = true; ;
                    break;
                case Keys.D6:
                    keyboardD6.Enabled = true;
                    break;
                case Keys.D7:
                    keyboardD7.Enabled = true;
                    break;
                case Keys.D8:
                    keyboardD8.Enabled = true;
                    break;
                case Keys.D9:
                    keyboardD9.Enabled = true;
                    break;
                case Keys.W:
                    keyboardW.Enabled = true;
                    break;
                case Keys.A:
                    keyboardA.Enabled = true;
                    break;
                case Keys.C:
                    keyboardC.Enabled = true;
                    break;
                case Keys.D:
                    keyboardD.Enabled = true;
                    break;
                case Keys.E:
                    keyboardE.Enabled = true;
                    break;
                case Keys.F:
                    keyboardF.Enabled = true;
                    break;
                case Keys.G:
                    keyboardG.Enabled = true;
                    break;
                case Keys.H:
                    keyboardH.Enabled = true;
                    break;
                case Keys.I:
                    keyboardI.Enabled = true;
                    break;
                case Keys.J:
                    keyboardJ.Enabled = true;
                    break;
                case Keys.K:
                    keyboardK.Enabled = true;
                    break;
                case Keys.L:
                    keyboardL.Enabled = true;
                    break;
                case Keys.O:
                    keyboardO.Enabled = true;
                    break;
                case Keys.P:
                    keyboardP.Enabled = true;
                    break;
                case Keys.Q:
                    keyboardQ.Enabled = true;
                    break;
                case Keys.R:
                    keyboardR.Enabled = true;
                    break;
                case Keys.S:
                    keyboardS.Enabled = true;
                    break;
                case Keys.T:
                    keyboardT.Enabled = true;
                    break;
                case Keys.U:
                    keyboardU.Enabled = true;
                    break;
                case Keys.V:
                    keyboardV.Enabled = true;
                    break;
                case Keys.Z:
                    keyboardZ.Enabled = true;
                    break;
                case Keys.Y:
                    keyboardY.Enabled = true;
                    break;
                case Keys.X:
                    keyboardX.Enabled = true;
                    break;
                default:
                    break;
            }
        }

        private void btnPlaySong_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D0:
                    keyboardD0.Enabled = false;
                    break;
                case Keys.D1:
                    keyboardD1.Enabled = false;
                    break;
                case Keys.D2:
                    keyboardD2.Enabled = false;
                    break;
                case Keys.D3:
                    keyboardD3.Enabled = false;
                    break;
                case Keys.D4:
                    keyboardD4.Enabled = false; ;
                    break;
                case Keys.D5:
                    keyboardD5.Enabled = false; ;
                    break;
                case Keys.D6:
                    keyboardD6.Enabled = false;
                    break;
                case Keys.D7:
                    keyboardD7.Enabled = false;
                    break;
                case Keys.D8:
                    keyboardD8.Enabled = false;
                    break;
                case Keys.D9:
                    keyboardD9.Enabled = false;
                    break;
                case Keys.W:
                    keyboardW.Enabled = false;
                    break;
                case Keys.A:
                    keyboardA.Enabled = false;
                    break;
                case Keys.C:
                    keyboardC.Enabled = false;
                    break;
                case Keys.D:
                    keyboardD.Enabled = false;
                    break;
                case Keys.E:
                    keyboardE.Enabled = false;
                    break;
                case Keys.F:
                    keyboardF.Enabled = false;
                    break;
                case Keys.G:
                    keyboardG.Enabled = false;
                    break;
                case Keys.H:
                    keyboardH.Enabled = false;
                    break;
                case Keys.I:
                    keyboardI.Enabled = false;
                    break;
                case Keys.J:
                    keyboardJ.Enabled = false;
                    break;
                case Keys.K:
                    keyboardK.Enabled = false;
                    break;
                case Keys.L:
                    keyboardL.Enabled = false;
                    break;
                case Keys.O:
                    keyboardO.Enabled = false;
                    break;
                case Keys.P:
                    keyboardP.Enabled = false;
                    break;
                case Keys.Q:
                    keyboardQ.Enabled = false;
                    break;
                case Keys.R:
                    keyboardR.Enabled = false;
                    break;
                case Keys.S:
                    keyboardS.Enabled = false;
                    break;
                case Keys.T:
                    keyboardT.Enabled = false;
                    break;
                case Keys.U:
                    keyboardU.Enabled = false;
                    break;
                case Keys.V:
                    keyboardV.Enabled = false;
                    break;
                case Keys.Z:
                    keyboardZ.Enabled = false;
                    break;
                case Keys.Y:
                    keyboardY.Enabled = false;
                    break;
                case Keys.X:
                    keyboardX.Enabled = false;
                    break;
                default:
                    break;
            }
        }

        private void btnStopSong_Click(object sender, EventArgs e)
        {
            foreach (var button in buttonList)
            {
                    button.BackColor = Color.White;
                    int temp = arduinoValues[button.Name.ToString() + "OFF"];
                    serialPort1.Write(temp.ToString());
                    button.Enabled = false;
            }

            btnStopSong.Enabled = true;
            btnStartRecording.Enabled = true;
        }
    }
}
