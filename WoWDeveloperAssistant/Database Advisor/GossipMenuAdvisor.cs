﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WoWDeveloperAssistant.Database_Advisor
{
    public static class GossipMenuAdvisor
    {
        public static string GetTextForGossipMenu(string menuId)
        {
            string output = "";

            DataSet gossipMenuTextIdDs = SQLModule.DatabaseSelectQuery("SELECT `text_id` FROM `gossip_menu` WHERE `entry` = " + menuId + ";");
            if (gossipMenuTextIdDs == null || gossipMenuTextIdDs.Tables["table"].Rows.Count == 0)
            {
                MessageBox.Show("There is no gossip menu with this Id in your database!");
                return output;
            }

            if (gossipMenuTextIdDs.Tables["table"].Rows.Count > 1)
            {
                foreach (DataRow gossipMenuRow in gossipMenuTextIdDs.Tables["table"].Rows)
                {
                    output += "Text Id: " + gossipMenuRow[0].ToString() + "\r\n";

                    DataSet npcTextBroadcastIdDs = SQLModule.DatabaseSelectQuery("SELECT `BroadcastTextID0`, `BroadcastTextID1`, `BroadcastTextID2`, `BroadcastTextID3`, `BroadcastTextID4`, `BroadcastTextID5`, `BroadcastTextID6`, `BroadcastTextID7` FROM `npc_text` WHERE `ID` = " + gossipMenuRow[0].ToString() + ";");
                    if (npcTextBroadcastIdDs == null || npcTextBroadcastIdDs.Tables["table"].Rows.Count == 0)
                    {
                        output += "There is no npc text with this TextId in your database!";
                        continue;
                    }

                    foreach (var itr in npcTextBroadcastIdDs.Tables["table"].Rows[0].ItemArray.Where(x => x.ToString() != "0"))
                    {
                        DataSet broadcastTextDs = SQLModule.HotfixSelectQuery("SELECT `Text`, `Text1` FROM `broadcasttext` WHERE `ROW_ID` = " + itr.ToString() + ";");
                        if (broadcastTextDs == null || broadcastTextDs.Tables["table"].Rows.Count == 0)
                        {
                            output += "There is no broadcast text with this BroadcastId in your database!";
                            continue;
                        }

                        foreach (string stringRow in broadcastTextDs.Tables["table"].Rows[0].ItemArray)
                        {
                            if (stringRow != "")
                            {
                                output += "- " + stringRow + "\r\n";
                            }
                        }
                    }
                }
            }
            else
            {
                output += "Text Id: " + gossipMenuTextIdDs.Tables["table"].Rows[0][0].ToString() + "\r\n";

                DataSet npcTextBroadcastIdDs = SQLModule.DatabaseSelectQuery("SELECT `BroadcastTextID0`, `BroadcastTextID1`, `BroadcastTextID2`, `BroadcastTextID3`, `BroadcastTextID4`, `BroadcastTextID5`, `BroadcastTextID6`, `BroadcastTextID7` FROM `npc_text` WHERE `ID` = " + gossipMenuTextIdDs.Tables["table"].Rows[0][0].ToString() + ";");
                if (npcTextBroadcastIdDs == null || npcTextBroadcastIdDs.Tables["table"].Rows.Count == 0)
                {
                    MessageBox.Show("There is no npc text with this TextId in your database!");
                    return output;
                }

                foreach (var itr in npcTextBroadcastIdDs.Tables["table"].Rows[0].ItemArray.Where(x => x.ToString() != "0"))
                {
                    DataSet broadcastTextDs = SQLModule.HotfixSelectQuery("SELECT `Text`, `Text1` FROM `broadcasttext` WHERE `ROW_ID` = " + itr.ToString() + ";");
                    if (broadcastTextDs == null || broadcastTextDs.Tables["table"].Rows.Count == 0)
                    {
                        output += "There is no broadcast text with this BroadcastId in your database!";
                        continue;
                    }

                    foreach (string stringRow in broadcastTextDs.Tables["table"].Rows[0].ItemArray)
                    {
                        if (stringRow != "")
                        {
                            output += "- " + stringRow + "\r\n";
                        }
                    }
                }
            }

            return output;
        }
    }
}
