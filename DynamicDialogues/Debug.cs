﻿using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using lv = StardewModdingAPI.LogLevel;

namespace DynamicDialogues
{
    internal static class Debug
    {
        internal static void Print(string arg1, string[] arg2)
        {
            if (!arg2.Any() || arg2?.Length == 0)
            {
                ModEntry.Mon.Log("Please specify a type. (Possible values: Dialogues, Random, Questions, Notifs", lv.Warn);
                return;
            }

            bool det = arg2.Contains("det");

            if (arg2.Contains("Dialogues"))
            {
                string dials = "Dialogues: ";
                if(ModEntry.Dialogues == null || ModEntry.Dialogues?.Count == 0)
                {
                    dials += "None";
                    ModEntry.Mon.Log(dials, lv.Info);
                }
                else
                {
                    foreach (var pair in ModEntry.Dialogues)
                    {
                        dials += pair.Key + $" ({pair.Value?.Count}), ";
                    }
                    ModEntry.Mon.Log(dials, lv.Info);
                }
            }
            if (arg2.Contains("Notifs"))
            {
                string noti = "Notifications: ";

                if (ModEntry.Notifs?.Count == 0 || ModEntry.Notifs == null)
                {
                    noti += "none";
                    ModEntry.Mon.Log(noti, lv.Info);
                }
                else
                {
                    foreach (var data in ModEntry.Notifs)
                    {
                        noti += "\n";

                        noti += $"Message: {data.Message}, Time: {data.Time}, Location: {data.Location}, IsBox: {data.IsBox} Sound: {data.Sound},";
                    }
                    ModEntry.Mon.Log(noti, lv.Info);
                }

            }
            if (arg2.Contains("Random"))
            {
                string ran = "Random dialogues: ";

                if(ModEntry.RandomPool?.Count == 0 || ModEntry.RandomPool == null)
                {
                    ran += "none";
                    ModEntry.Mon.Log(ran, lv.Info);
                }
                else
                {
                    foreach(var pair in ModEntry.RandomPool)
                    {
                        if (det)
                            ran += "\n";

                        ran += pair.Key + $" {(det ? Listify(pair.Value) : "("+pair.Value?.Count+"), ")}";
                    }
                    ModEntry.Mon.Log(ran, lv.Info);
                }
            }
            if (arg2.Contains("Questions"))
            {
                string Qs = "Questions: ";

                if (ModEntry.Questions == null || ModEntry.Questions?.Count == 0)
                {
                    Qs += "None";
                    ModEntry.Mon.Log(Qs, lv.Info);
                }
                else
                {
                    foreach (var pair in ModEntry.Dialogues)
                    {
                        Qs += pair.Key + $" ({pair.Value?.Count}), ";
                    }
                    ModEntry.Mon.Log(Qs, lv.Info);
                }
            }
            /*
            if(arg2.Contains("Missions") || arg2.Contains("Quests"))
            {
                string mis = "Missions: ";
                if (ModEntry.MissionData == null || ModEntry.MissionData?.Count == 0)
                {
                    mis += "None";
                    ModEntry.Mon.Log(mis, lv.Info);
                }
                else
                {
                    foreach (var pair in ModEntry.MissionData)
                    {
                        mis += pair.Key + $" {(det ? Listify(pair.Value) : "(" + pair.Value?.Count + "), ")}";
                    }
                    ModEntry.Mon.Log(mis, lv.Info);
                }
            }*/
        }
/*
        private static string Listify(List<RawMission> value)
        {
            var result = "\n";
            foreach (var msn in value)
            {
                result += $"ID: {msn.ID}, Text: \"{msn.Dialogue}\", Location: {msn.Location}, Time: {msn.From} - {msn.To}\n";
            }
            return result;
        }*/

        private static string Listify(List<string> value)
        {
            var result = "\n";
            foreach(var text in value)
            {
                result += $"\"{text}\"\n";
            }
            return result;
        }

        public static void SayHiTo(string arg1, string[] arg2)
        {
            try
            {
                if (arg2.Length != 2)
                {
                    ModEntry.Mon.Log("Format: sayHiTo <npc talking> <npc to greet>.\nExample: `sayHiTo Alex Evelyn`",lv.Warn);
                    return;
                }

                var chara = arg2[0];
                var who = Game1.getCharacterFromName(chara);
                var pos = Game1.player.Position;
                pos.X++;
                
                Game1.warpCharacter(who,Game1.player.currentLocation,pos);

                var chara2 = arg2[1];
                var greeted = Game1.getCharacterFromName(chara);

                who.sayHiTo(greeted);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
