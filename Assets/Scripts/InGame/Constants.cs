using System.Collections.Generic;

public abstract class Constants
{
    public abstract class SceneNames
    {
        public const string Menu = "Menu";
        public const string Desk = "Desk_d";
        public const string Kickstart = "Kickstart";
    }
    
    public abstract class Sound
    {
        public abstract class Sfx
        {
            public static readonly Dictionary<Type, string> Paths = new() {
                { Type.ShowingCards, "Sound/SFX/ShowingCards"},
                { Type.Bet, "Sound/SFX/Bet"},
                { Type.ToPot, "Sound/SFX/ToPot"},
                { Type.Check, "Sound/SFX/Check"}
            }; 
            
            public enum Type
            {
                ShowingCards,
                Bet,
                ToPot,
                Check,
            }
        }
    
        public abstract class Music
        {
            public static readonly Dictionary<Type, string> Paths = new() {
                { Type.BgMusic, "Sound/Music/BGMusic"},
                { Type.BgMusic2, "Sound/Music/BGMusic2"},
                { Type.BgMusic3, "Sound/Music/BGMusic3"}
            }; 

            public enum Type
            {
                BgMusic,
                BgMusic2,
                BgMusic3
            }
        }
    }

    public abstract class ResourcesPaths
    {
        public const string Cards = "Sprites/Cards";
        public const string Chips = "Sprites/Chips";
    }
}
