using System;
using System.Windows.Forms;

namespace LoginSystem
{
    partial class MainForm: Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public MainForm(string username, string role, int remainingTimeFromDatabase)
        {
            Username = username;
            Role = role;
            RemainingTimeFromDatabase = remainingTimeFromDatabase;
        }

        public string Username { get; }
        public string Role { get; }
        public int RemainingTimeFromDatabase { get; }
        public Action<object, FormClosedEventArgs> FormClosed { get; internal set; }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>


        internal void Show()
        {
            throw new NotImplementedException();
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>


        #endregion
    }
}