﻿// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Scada.Lang;
using System;
using System.Windows.Forms;

namespace Scada.Forms.Forms
{
    /// <summary>
    /// Represents a universal form for editing module configuration.
    /// <para>Представляет универсальную форму для редактирования конфигурации модуля.</para>
    /// </summary>
    public partial class FrmModuleConfig : Form
    {
        private readonly ModuleConfigProvider configProvider; // provides access to the module configuration
        private bool modified; // indicates that the module configuration is modified


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        private FrmModuleConfig()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public FrmModuleConfig(ModuleConfigProvider configProvider)
            : this()
        {
            this.configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            modified = false;
        }


        /// <summary>
        /// Gets or sets a value indicating whether the module configuration is modified.
        /// </summary>
        private bool Modified
        {
            get
            {
                return modified;
            }
            set
            {
                modified = value;
                btnSave.Enabled = modified;
                btnCancel.Enabled = modified;
            }
        }


        private void FrmModuleConfig_Load(object sender, EventArgs e)
        {
            FormTranslator.Translate(this, GetType().FullName);

            if (!configProvider.LoadConfig(out string errMsg))
                ScadaUiUtils.ShowError(errMsg);

            configProvider.BackupConfig();
            Modified = false;
        }

        private void FrmModuleConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Modified)
            {
                DialogResult result = MessageBox.Show(CommonPhrases.SaveConfigConfirm,
                    CommonPhrases.QuestionCaption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                switch (result)
                {
                    case DialogResult.Yes:
                        if (!configProvider.SaveConfig(out string errMsg))
                        {
                            ScadaUiUtils.ShowError(errMsg);
                            e.Cancel = true;
                        }
                        break;

                    case DialogResult.No:
                        break;

                    default:
                        e.Cancel = true;
                        break;
                }
            }
        }


        private void btnAdd_Click(object sender, EventArgs e)
        {

        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {

        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {

        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            if (configProvider.SaveConfig(out string errMsg))
                Modified = false;
            else
                ScadaUiUtils.ShowError(errMsg);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            configProvider.RestoreConfig();
            Modified = false;
        }
    }
}