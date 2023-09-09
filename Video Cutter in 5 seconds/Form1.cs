using System;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using OpenHardwareMonitor.Hardware;
using System.Threading;

namespace Video_Cutter_in_5_seconds
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing1;
        }

        private void Form1_FormClosing1(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Bit = textBox4.Text;
            Properties.Settings.Default.save = textBox5.Text;
            Properties.Settings.Default.Save();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox4.Text = Properties.Settings.Default.Bit;
            textBox5.Text = Properties.Settings.Default.save;

            Thread monitoringThread = new Thread(MonitorHardware);
            monitoringThread.Start();
        }


        void MonitorHardware()
        {
            Computer computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;

            while (true)
            {
                foreach (var hardware in computer.Hardware)
                {
                    if (hardware.HardwareType == HardwareType.CPU)
                    {
                        hardware.Update();

                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Temperature)
                            {
                                label11.Invoke(new Action(() => label11.Text = $"{sensor.Value}°C"));
                            }
                            if (sensor.SensorType == SensorType.Load)
                            {
                                label12.Invoke(new Action(() => label12.Text = $"{sensor.Value}%"));
                            }
                        }
                    }
                }

                Thread.Sleep(500);
            }
        }


        //public void Monitor()
        //{
        //    Computer computer = new Computer();
        //    computer.Open();

        //    foreach (var hardware in computer.Hardware)
        //    {
        //        Console.WriteLine(hardware.Name);
        //        if (hardware.HardwareType == HardwareType.CPU)
        //        {
        //            hardware.Update();
        //            foreach (var sensor in hardware.Sensors)
        //            {
        //                if (sensor.SensorType == SensorType.Temperature)
        //                {
        //                    Console.WriteLine($"CPU Temperature: {sensor.Value}°C");
        //                }
        //            }
        //        }
        //    }

        //    computer.Close();
        //}

        static string ConvertTime(string timeStr)
        {
            // Разбиваем строку времени на минуты и секунды
            string[] timeParts = timeStr.Split(':');
            int minutes = int.Parse(timeParts[0]);
            int seconds = int.Parse(timeParts[1]);

            // Вычисляем часы и оставшиеся минуты
            int hours = minutes / 60;
            minutes = minutes % 60;

            // Возвращаем строку в формате hh:mm:ss
            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }


        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox3.Text == "")
            {
                label4.Text = "Пусто";
            }
            else
            {
                label4.Text = "Занято";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {


            //Monitor();



            string input_video = textBox3.Text;

            string fileNameWithExtension = Path.GetFileName(input_video);

            string output_video = textBox5.Text;
            if (!output_video.EndsWith("\\"))
            {
                output_video = output_video + "\\" + fileNameWithExtension;
            }
            else
            {
                output_video = output_video + fileNameWithExtension;
            }

            string start_cut = ConvertTime(textBox1.Text);
            string end_cut = ConvertTime(textBox2.Text);


            Process proc = new Process();
            proc.StartInfo.FileName = "ffmpeg-2023-04-26-git-e3143703e9-full_build\\bin\\ffmpeg.exe";
            proc.StartInfo.Arguments = $"-i \"{input_video}\" -ss {start_cut} -to {end_cut} -r 60 -b:v {textBox4.Text}k -c:v libx264 -c:a copy \"{output_video}\"";
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            if (!proc.Start())
            {
                return;
            }

            async void StartAsyncProcessing()
            {
                StreamReader reader = proc.StandardError;
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (line == ": Invalid argument")
                    {
                        MessageBox.Show("Алооо бебрік недорастер", "Ошибка: нужны мозги!!!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if(line == "Press [q] to stop, [?] for help")
                    {
                        label4.Text = "Подготовка до ддоса олега";
                    }
                    else
                    {
                        string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        StringBuilder formattedOutput = new StringBuilder();

                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (i % 2 == 0 && i != 0)
                            {
                                formattedOutput.Append(Environment.NewLine); // Перевод строки между элементами (кроме первого)
                            }
                            formattedOutput.Append(parts[i] + " "); // Вывод элемента с пробелом
                        }

                        label4.Text = formattedOutput.ToString();
                    }
                }

                label4.Clear();
                textBox3.Clear();
                textBox1.Clear();
                textBox2.Clear();
            }

            StartAsyncProcessing();

            proc.Close();
 
        }
    }
}
