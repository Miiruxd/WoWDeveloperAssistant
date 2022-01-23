using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.IO;

namespace WoWDeveloperAssistant.Phasing
{
    [Serializable]
    public class Entity
    {
        public string newLinkedId;
        public string oldLinkedId;
        public string position_x;
        public string position_y;
        public string position_z;
        public string map;
        public string difficulties;
        public string name;
        public string entry;
    }

        [Serializable]
    class PhasingHandler
    {
        private MainForm mainForm;
        private string phaseMask, phaseId, entry, areaId, linkedIds;
        List<Entity> entities = new List<Entity>();

        public PhasingHandler(MainForm mainForm)
        {
            this.mainForm = mainForm;
            mainForm.comboBox_Phasing_EntryOrLinkedId.SelectedIndex = 0;
            mainForm.comboBox_Phasing_UnitType.SelectedIndex = 0;
        }

        bool GetDataFromForm()
        {
            phaseMask = mainForm.textBox_Phasing_PhaseMask.Text;
            phaseId = mainForm.textBox_Phasing_PhaseID.Text;
            areaId = mainForm.textBox_Phasing_AreaID.Text;

            int retValue = GetDataFromDB();

            if (retValue == 1)
            {
                if (mainForm.comboBox_Phasing_EntryOrLinkedId.SelectedIndex == 0)
                    MessageBox.Show("Specified linked id doesn't exists in your database!");
                else
                    MessageBox.Show("No spawns were detected for the specified entry!");

                return false;
            }
            else if (retValue == 2)
            {
                if (mainForm.comboBox_Phasing_EntryOrLinkedId.SelectedIndex == 0)
                    MessageBox.Show("Specified linked id already has a phaseID set!");
                return false;
            }
            else
                return true;
        }

        int GetDataFromDB()
        {
            if (mainForm.comboBox_Phasing_EntryOrLinkedId.SelectedIndex == 0)
            {
                if (mainForm.comboBox_Phasing_UnitType.SelectedIndex == 2)
                {
                    linkedIds = mainForm.textBox_Phasing_EntryOrLinkedId.Text;

                    string dbQuery;

                    dbQuery = "SELECT creature_template_wdb.Name1, creature.linked_id, creature.id, creature.position_x, creature.position_y, creature.position_z, creature.map, creature.difficulties, creature.phaseID FROM creature_template_wdb INNER JOIN creature ON creature_template_wdb.Entry = creature.id WHERE creature.linked_id IN (" + linkedIds + ")";

                    if (areaId != "")
                        dbQuery += " AND creature.areaId IN (" + areaId + ")";

                    dbQuery += ";";

                    DataSet queryResults = Properties.Settings.Default.UsingDB ? SQLModule.DatabaseSelectQuery(dbQuery) : null;

                    if (queryResults != null && queryResults.Tables["table"].Rows.Count != 0)
                    {
                        for (int i = 0; i < queryResults.Tables["table"].Rows.Count; ++i)
                        {
                            if (queryResults.Tables["table"].Rows[i][7].ToString() != "0")
                                continue;

                            Entity entity = new Entity();
                            entity.name = queryResults.Tables["table"].Rows[i][0].ToString();
                            entity.oldLinkedId = queryResults.Tables["table"].Rows[i][1].ToString();
                            entity.entry = queryResults.Tables["table"].Rows[i][2].ToString();
                            entity.position_x = queryResults.Tables["table"].Rows[i][3].ToString();
                            entity.position_y = queryResults.Tables["table"].Rows[i][4].ToString();
                            entity.position_z = queryResults.Tables["table"].Rows[i][5].ToString();
                            entity.map = queryResults.Tables["table"].Rows[i][6].ToString();
                            entity.difficulties = queryResults.Tables["table"].Rows[i][7].ToString();
                            GenerateNewLinkedId(entity);
                            entities.Add(entity);
                        }
                        return 0;
                    }
                }
                else
                {
                    Entity entity = new Entity();
                    entity.oldLinkedId = mainForm.textBox_Phasing_EntryOrLinkedId.Text;

                    string dbQuery;

                    if (mainForm.comboBox_Phasing_UnitType.SelectedIndex == 0)
                        dbQuery = "SELECT creature_template_wdb.Name1, creature.position_x, creature.position_y, creature.position_z, creature.id, creature.map, creature.difficulties, creature.phaseID FROM creature_template_wdb INNER JOIN creature ON creature_template_wdb.Entry = creature.id WHERE creature.linked_id = '" + entity.oldLinkedId + "';";
                    else
                        dbQuery = "SELECT gameobject_template.name, gameobject.position_x, gameobject.position_y, gameobject.position_z, gameobject.id, gameobject.map, gameobject.difficulties, gameobject.phaseID FROM gameobject_template INNER JOIN gameobject ON gameobject_template.entry = gameobject.id WHERE gameobject.linked_id = '" + entity.oldLinkedId + "';";

                    DataSet queryResults = Properties.Settings.Default.UsingDB ? SQLModule.DatabaseSelectQuery(dbQuery) : null;

                    if (queryResults != null && queryResults.Tables["table"].Rows.Count != 0)
                    {
                        if (queryResults.Tables["table"].Rows[0][7].ToString() != "0")
                            return 2;

                        entity.name = queryResults.Tables["table"].Rows[0][0].ToString();
                        entity.position_x = queryResults.Tables["table"].Rows[0][1].ToString();
                        entity.position_y = queryResults.Tables["table"].Rows[0][2].ToString();
                        entity.position_z = queryResults.Tables["table"].Rows[0][3].ToString();
                        entity.entry = queryResults.Tables["table"].Rows[0][4].ToString();
                        entity.map = queryResults.Tables["table"].Rows[0][5].ToString();
                        entity.difficulties = queryResults.Tables["table"].Rows[0][6].ToString();
                        GenerateNewLinkedId(entity);
                        entities.Add(entity);
                        return 0;
                    }
                }
            }
            else if (mainForm.comboBox_Phasing_EntryOrLinkedId.SelectedIndex == 1)
            {
                entry = mainForm.textBox_Phasing_EntryOrLinkedId.Text;

                string dbQuery;

                if (mainForm.comboBox_Phasing_UnitType.SelectedIndex == 0)
                    dbQuery = "SELECT creature_template_wdb.Name1, creature.linked_id, creature.position_x, creature.position_y, creature.position_z, creature.map, creature.difficulties, creature.phaseID FROM creature_template_wdb INNER JOIN creature ON creature_template_wdb.Entry = creature.id WHERE creature.id = " + entry;
                else
                    dbQuery = "SELECT gameobject_template.name, gameobject.linked_id, gameobject.position_x, gameobject.position_y, gameobject.position_z, gameobject.map, gameobject.difficulties, gameobject.phaseID FROM gameobject_template INNER JOIN gameobject ON gameobject_template.entry = gameobject.id WHERE gameobject.id = " + entry;

                if (areaId != "")
                    dbQuery += " AND creature.areaId IN (" + areaId + ")";

                dbQuery += ";";

                DataSet queryResults = Properties.Settings.Default.UsingDB ? SQLModule.DatabaseSelectQuery(dbQuery) : null;

                if (queryResults != null && queryResults.Tables["table"].Rows.Count != 0)
                {
                    for (int i = 0; i < queryResults.Tables["table"].Rows.Count; ++i)
                    {
                        if (queryResults.Tables["table"].Rows[i][7].ToString() != "0")
                            continue;

                        Entity entity = new Entity();
                        entity.name = queryResults.Tables["table"].Rows[i][0].ToString();
                        entity.oldLinkedId = queryResults.Tables["table"].Rows[i][1].ToString();
                        entity.position_x = queryResults.Tables["table"].Rows[i][2].ToString();
                        entity.position_y = queryResults.Tables["table"].Rows[i][3].ToString();
                        entity.position_z = queryResults.Tables["table"].Rows[i][4].ToString();
                        entity.map = queryResults.Tables["table"].Rows[i][5].ToString();
                        entity.difficulties = queryResults.Tables["table"].Rows[i][6].ToString();
                        entity.entry = entry;
                        GenerateNewLinkedId(entity);
                        entities.Add(entity);
                    }
                    return 0;
                }
            }
            return 1;
        }

        void GenerateNewLinkedId(Entity _entity)
        {
            string linkedIdString = (Math.Round(float.Parse(_entity.position_x) / 0.25f)).ToString() + " " + (Math.Round(float.Parse(_entity.position_y) / 0.25f)).ToString() + " " + (Math.Round(float.Parse(_entity.position_z) / 0.25f)).ToString() + " " + _entity.entry + " " + _entity.map + " " + phaseId + " " + phaseMask + " " + _entity.difficulties;
            _entity.newLinkedId = Hash(linkedIdString);
        }

        static string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        public void GenerateSql()
        {
            entities.Clear();

            if (!GetDataFromForm())
                return;

            string sql, dbQuery;

            for (int i = 0; i < entities.Count; ++i)
            {
                Entity entity = entities[i];

                if (mainForm.comboBox_Phasing_UnitType.SelectedIndex == 0 || mainForm.comboBox_Phasing_UnitType.SelectedIndex == 2)
                {
                    sql = "\r\nDELETE FROM `creature` WHERE `linked_id` = '" + entity.newLinkedId + "';\r\n";
                    sql += "DELETE FROM `creature_addon` WHERE `linked_id` = '" + entity.newLinkedId + "';\r\n";

                    dbQuery = "SELECT * FROM `creature_movement_override` WHERE `LinkedId` = '" + entity.newLinkedId + "';";
                    DataSet queryResults = Properties.Settings.Default.UsingDB ? SQLModule.DatabaseSelectQuery(dbQuery) : null;

                    if (queryResults != null && queryResults.Tables["table"].Rows.Count != 0)
                    {
                        sql += "DELETE FROM `creature_movement_override` WHERE `LinkedId` = '" + entity.newLinkedId + "';\r\n";
                    }

                    sql += "UPDATE `creature` SET `phaseID` = " + phaseId + ", `phaseMask` = " + phaseMask + " WHERE `linked_id` = '" + entity.oldLinkedId + "'; -- " + entity.name + "\r\n";
                    //sql += "UPDATE `creature` SET `phaseID` = " + phaseId + ", `phaseMask` = " + phaseMask + " WHERE `linked_id` = '" + entity.oldLinkedId + "'; -- " + name + ": " + entity.newLinkedId + "\r\n";
                }
                else
                {
                    sql = "\r\nDELETE FROM `gameobject` WHERE `linked_id` = '" + entity.newLinkedId + "';\r\n";
                    sql += "DELETE FROM `gameobject_addon` WHERE `linked_id` = '" + entity.newLinkedId + "';\r\n";
                    sql += "UPDATE `gameobject` SET `phaseID` = " + phaseId + ", `phaseMask` = " + phaseMask + " WHERE `linked_id` = '" + entity.oldLinkedId + "'; -- " + entity.name + "\r\n";
                    //sql += "UPDATE `gameobject` SET `phaseID` = " + phaseId + ", `phaseMask` = " + phaseMask + " WHERE `linked_id` = '" + entity.oldLinkedId + "'; -- " + name + ": " + entity.newLinkedId + "\r\n";
                }

                mainForm.textBox_Phasing_SqlText.Text += sql;
                mainForm.toolStripStatusLabel_FileStatus.Text = "Added " + (i + 1) + " entries.";
            }

            Clipboard.SetText(mainForm.textBox_Phasing_SqlText.Text);

            //string sql = mainForm.textBox_Phasing_SqlText.Text;
            //string output = "";

            //using (StringReader sr = new StringReader(sql))
            //{
            //    string line;
            //    while ((line = sr.ReadLine()) != null)
            //    {
            //        Entity entity = new Entity();

            //        phaseMask = "0";
            //        phaseId = GetEnclosedString(line, "`phaseID` = ", ", ");
            //        entity.oldLinkedId = GetEnclosedString(line, "`linked_id` = '", "';");

            //        // fill db data
            //        string dbQuery;
            //        dbQuery = "SELECT gameobject.position_x, gameobject.position_y, gameobject.position_z, gameobject.id, gameobject.map, gameobject.difficulties FROM gameobject WHERE gameobject.linked_id = '" + entity.oldLinkedId + "';";
            //        DataSet queryResults = Properties.Settings.Default.UsingDB ? SQLModule.DatabaseSelectQuery(dbQuery) : null;

            //        if (queryResults != null && queryResults.Tables["table"].Rows.Count != 0)
            //        {
            //            entity.position_x = queryResults.Tables["table"].Rows[0][0].ToString();
            //            entity.position_y = queryResults.Tables["table"].Rows[0][1].ToString();
            //            entity.position_z = queryResults.Tables["table"].Rows[0][2].ToString();
            //            entry = queryResults.Tables["table"].Rows[0][3].ToString();
            //            entity.map = queryResults.Tables["table"].Rows[0][4].ToString();
            //            entity.difficulties = queryResults.Tables["table"].Rows[0][5].ToString();
            //        }
            //        else
            //        {
            //            output += "-- " + line + "\n";
            //            continue;
            //        }

            //        // generate the new linked id
            //        string newLinkedId = (Math.Round(float.Parse(entity.position_x) / 0.25f)).ToString() + " " + (Math.Round(float.Parse(entity.position_y) / 0.25f)).ToString() + " " + (Math.Round(float.Parse(entity.position_z) / 0.25f)).ToString() + " " + entry + " " + entity.map + " " + phaseId + " " + phaseMask + " " + entity.difficulties;
            //        newLinkedId = Hash(newLinkedId);

            //        output += line + ": " + newLinkedId + "\n";
            //    }
            //}

            //mainForm.textBox_Phasing_SqlText.Text = output;
        }

        //string GetEnclosedString(string input, string beggining, string end)
        //{
        //    int from = input.IndexOf(beggining) + beggining.Length;
        //    int to = input.LastIndexOf(end);

        //    return input.Substring(from, to - from);
        //}
    }
}
