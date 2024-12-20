﻿using System;
using _4RTools.Model;
using System.Windows.Forms;
using _4RTools.Utils;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Oli.Controls;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace _4RTools.Forms
{
    public partial class ConfigForm : Form, IObserver
    {
        public ConfigForm(Subject subject)
        {
            InitializeComponent();
            this.textReinKey.KeyDown += new System.Windows.Forms.KeyEventHandler(FormUtils.OnKeyDown);
            this.textReinKey.KeyPress += new KeyPressEventHandler(FormUtils.OnKeyPress);
            this.textReinKey.TextChanged += new EventHandler(this.textReinKey_TextChanged);

            this.ammo1textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(FormUtils.OnKeyDown);
            this.ammo1textBox.KeyPress += new KeyPressEventHandler(FormUtils.OnKeyPress);
            this.ammo1textBox.TextChanged += new EventHandler(this.textAmmo1_TextChanged);
            this.ammo2textBox.KeyDown += new System.Windows.Forms.KeyEventHandler(FormUtils.OnKeyDown);
            this.ammo2textBox.KeyPress += new KeyPressEventHandler(FormUtils.OnKeyPress);
            this.ammo2textBox.TextChanged += new EventHandler(this.textAmmo2_TextChanged);

            
            var newListBuff = ProfileSingleton.GetCurrent().UserPreferences.autoBuffOrder;
            this.listBox1.MouseLeave += new System.EventHandler(this.listBox1_MouseLeave);

            toolTip1.SetToolTip(switchAmmoCheckBox, "Intercala entre as munições");
            toolTip2.SetToolTip(textReinKey, "atalho rédea");
            toolTip3.SetToolTip(ammo1textBox, "atalho ammo 1");
            toolTip4.SetToolTip(ammo2textBox, "atalho ammo 2");
            subject.Attach(this);
        }

        public void Update(ISubject subject)
        {
            switch ((subject as Subject).Message.code)
            {
                case MessageCode.PROFILE_CHANGED:
                case MessageCode.ADDED_NEW_AUTOBUFF_SKILL:
                    UpdateUI(subject);
                    break;
            }
        }
        public void UpdateUI(ISubject subject)
        {
            try
            {
                AutoBuffSkill currentBuffs = (AutoBuffSkill)(subject as Subject).Message.data;
                listBox1.Items.Clear();

                if (currentBuffs == null)
                {
                    currentBuffs = ProfileSingleton.GetCurrent().AutobuffSkill;
                }

                var buffsList = currentBuffs.buffMapping.Keys.ToList();
                listBox1.Items.Clear();

                foreach (var buff in buffsList)
                {
                    listBox1.Items.Add(buff.ToDescriptionString());
                }

                this.chkStopBuffsOnCity.Checked = ProfileSingleton.GetCurrent().UserPreferences.stopBuffsCity;
                this.chkStopBuffsOnRein.Checked = ProfileSingleton.GetCurrent().UserPreferences.stopBuffsRein;
                this.chkStopHealOnCity.Checked = ProfileSingleton.GetCurrent().UserPreferences.stopHealCity;
                this.chkStopOnAntiBot.Checked = ProfileSingleton.GetCurrent().UserPreferences.stopSpammersBot;
                this.getOffReinCheckBox.Checked = ProfileSingleton.GetCurrent().UserPreferences.getOffRein;
                this.textReinKey.Text = ProfileSingleton.GetCurrent().UserPreferences.getOffReinKey.ToString();
                this.switchAmmoCheckBox.Checked = ProfileSingleton.GetCurrent().UserPreferences.switchAmmo;
                this.ammo1textBox.Text = ProfileSingleton.GetCurrent().UserPreferences.ammo1Key.ToString();
                this.ammo2textBox.Text = ProfileSingleton.GetCurrent().UserPreferences.ammo2Key.ToString();
            }
            catch { }
        }

        private void listBox1_MouseLeave(object sender, EventArgs e)
        {
            try
            {
                var autoBuffSkill = ProfileSingleton.GetCurrent().AutobuffSkill;
                var newOrderList = new List<EffectStatusIDs>();
                var orderedBuffList = listBox1.Items;
                Dictionary<EffectStatusIDs, Key> currentList = new Dictionary<EffectStatusIDs, Key>(autoBuffSkill.buffMapping);
                Dictionary<EffectStatusIDs, Key> newOrderedBuffList = new Dictionary<EffectStatusIDs, Key>();
                if (currentList.Count > 0)
                {

                    foreach (var buff in orderedBuffList)
                    {
                        var buffId = buff.ToString().ToEffectStatusId();
                        newOrderList.Add(buffId);
                        var findBuff = currentList.FirstOrDefault(t => t.Key == buffId);
                        newOrderedBuffList.Add(findBuff.Key, findBuff.Value);
                    }
                    ProfileSingleton.GetCurrent().UserPreferences.SetAutoBuffOrder(newOrderList);
                    ProfileSingleton.SetConfiguration(ProfileSingleton.GetCurrent().UserPreferences);

                    ProfileSingleton.GetCurrent().AutobuffSkill.ClearKeyMapping();
                    ProfileSingleton.GetCurrent().AutobuffSkill.setBuffMapping(newOrderedBuffList);
                    ProfileSingleton.SetConfiguration(ProfileSingleton.GetCurrent().AutobuffSkill);

                    newOrderedBuffList.Clear();
                }
            }
            catch { }
        }

        private void chkStopBuffsOnRein_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            ProfileSingleton.GetCurrent().UserPreferences.stopBuffsRein = chk.Checked;
            ProfileSingleton.SetConfiguration(ProfileSingleton.GetCurrent().UserPreferences);
        }

        private void chkStopBuffsOnCity_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            ProfileSingleton.GetCurrent().UserPreferences.stopBuffsCity = chk.Checked;
            ProfileSingleton.SetConfiguration(ProfileSingleton.GetCurrent().UserPreferences);
        }
        private void chkStopHealOnCity_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            ProfileSingleton.GetCurrent().UserPreferences.stopHealCity = chk.Checked;
            ProfileSingleton.SetConfiguration(ProfileSingleton.GetCurrent().UserPreferences);
        }

        private void chkStopOnAntiBot_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            ProfileSingleton.GetCurrent().UserPreferences.stopSpammersBot = chk.Checked;
            ProfileSingleton.SetConfiguration(ProfileSingleton.GetCurrent().UserPreferences);
        }

        private void getOffReinCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            ProfileSingleton.GetCurrent().UserPreferences.getOffRein = chk.Checked;
            ProfileSingleton.SetConfiguration(ProfileSingleton.GetCurrent().UserPreferences);
        }

        private void textReinKey_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TextBox txtBox = (TextBox)sender;
                if (txtBox.Text.ToString() != String.Empty)
                {
                    Key key = (Key)Enum.Parse(typeof(Key), txtBox.Text.ToString());
                    ProfileSingleton.GetCurrent().UserPreferences.getOffReinKey = key;
                    ProfileSingleton.SetConfiguration(ProfileSingleton.GetCurrent().UserPreferences);
                }
            }
            catch { }
        }

        private void switchAmmoCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            ProfileSingleton.GetCurrent().UserPreferences.switchAmmo = chk.Checked;
            ProfileSingleton.SetConfiguration(ProfileSingleton.GetCurrent().UserPreferences);
        }

        private void textAmmo1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TextBox txtBox = (TextBox)sender;
                if (txtBox.Text.ToString() != String.Empty)
                {
                    Key key = (Key)Enum.Parse(typeof(Key), txtBox.Text.ToString());
                    ProfileSingleton.GetCurrent().UserPreferences.ammo1Key = key;
                    ProfileSingleton.SetConfiguration(ProfileSingleton.GetCurrent().UserPreferences);
                }
            }
            catch { }
        }

        private void textAmmo2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                TextBox txtBox = (TextBox)sender;
                if (txtBox.Text.ToString() != String.Empty)
                {
                    Key key = (Key)Enum.Parse(typeof(Key), txtBox.Text.ToString());
                    ProfileSingleton.GetCurrent().UserPreferences.ammo2Key = key;
                    ProfileSingleton.SetConfiguration(ProfileSingleton.GetCurrent().UserPreferences);
                }
            }
            catch { }
        }

    }
}
