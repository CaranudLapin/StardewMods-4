﻿using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;

namespace AudioDescription
{
    internal class ConfigInfo
    {
        internal static void SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            ModEntry.MuteIcon = Game1.content.Load<Texture2D>("LooseSprites/mute_voice_icon");

            //ModEntry._lastTrack = Game1.currentSong.Name;

            ModEntry.AllowedCues?.Clear();

            if (ModEntry.Config.Environment)
            {
                ModEntry.AllowedCues?.AddRange(new List<string>
                {
                    "doorClose",
                    "cricketsAmbient",
                    "boulderCrack",
                    "dropItemInWater",
                    "explosion",
                    "crafting",
                    "stoneCrack",
                    "wind",
                    "SpringBirds",
                    "Ship",
                    "phone",
                    "thunder",
                    "crickets",
                    "cavedrip",
                    "treethud",
                    "treecrack",
                    "leafrustle",
                    "crystal",
                    "potterySmash",
                    "busDriveOff",
                    "Stadium_cheer",
                    "submarine_landing",
                    "cacklingWitch",
                    "thunder_small",
                    "trainWhistle",
                    "distantTrain",
                    "Meteorite",
                    "bubbles",
                    "boulderBreak",
                    "dirtyHit",
                    "newArtifact",
                    "secret1",
                    "jingle1",
                    "waterSlosh",
                    "robotSoundEffects",
                    "robotBLASTOFF",
                    "slosh",
                    "cameraNoise",
                    "mouseClick",
                    "whistle",
                    "barrelBreak"

                });
            }

            if (ModEntry.Config.NPCs)
            {
                ModEntry.AllowedCues?.AddRange(new List<string>
                {
                    "ghost",
                    "cluck",
                    "Duggy",
                    "rabbit",
                    "goat",
                    "cow",
                    "pig",
                    "croak",
                    "batScreech",
                    "seagulls",
                    "shadowDie",
                    "owl",
                    "dogs",
                    "Duck",
                    "sheep",
                    "killAnimal",
                    "junimoMeep1",
                    "dogWhining",
                    "crow",
                    "rooster",
                    "dog_pant",
                    "dog_bark",
                    "cat",
                    "parrot",
                    "fireball",
                    "flameSpellHit",
                    "flameSpell",
                    "monsterdead",
                    "rockGolemSpawn"
                });
            }

            if (ModEntry.Config.FishingCatch)
            {
                ModEntry.AllowedCues?.AddRange(new List<string>
                {
                    "fishBite",
                    "FishHit",
                    "fishEscape",
                    "fishSlap"
                });
            }

            if (ModEntry.Config.ItemSounds)
            {
                ModEntry.AllowedCues?.AddRange(new List<string>
                {
                    "cut",
                    "axe",
                    "wateringCan",
                    "openChest",
                    "parry",
                    "clank",
                    "toyPiano",
                    "trashcan",
                    "trashcanlid",
                    "scissors",
                    "Milking",
                    "breakingGlass",
                    "glug",
                    "doorCreakReverse",
                    "openBox",
                    "axchop",
                    "seeds",
                    "detector",
                    "crit"
                });
            }

            if (ModEntry.Config.PlayerSounds)
            {
                ModEntry.AllowedCues?.AddRange(new List<string>
                {
                    "eat",
                    "gulp",
                    "powerup",
                    "toolCharge",
                    "sipTea",
                    "slingshot",
                    "woodWhack",
                    "stairsdown",
                    "fallDown",
                    "doorCreak",
                    "doorOpen",
                    "pickUpItem",
                    "furnace",
                    "discoverMineral"
                });
            }
        }
    }
}