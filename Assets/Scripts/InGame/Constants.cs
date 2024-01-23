using System.Collections.Generic;

public abstract class Constants
{
    public abstract class SceneNames
    {
        public const string Menu = "Menu";
        public const string Desk = "Desk_d";
        public const string Kickstart = "Kickstart";
    }

    public abstract class Sprites
    {
        public const string Music = "MusicIcon";
        public const string Cross = "Cross";
        public const string MenuBackground = "nstu_menu";
        public const string DeskBackground = "nstu_desk_back";
        public const string GameMainIcon = "AppIcon";

        public abstract class Sdt
        {
            public const string Disconnected = "disconnected";
            public const string Loading = "loading";
            public const string Success = "success";
            public const string Fail = "error";
            public const string Abandoned = "questionmark";
        }
    }

    public abstract class Sound
    {
        public static readonly string MixerPath = "Sound/Mixer";
        
        public abstract class Sfx
        {
            public static readonly Dictionary<Type, string> Paths = new()
            {
                { Type.ShowingCards, "Sound/SFX/ShowingCards" },
                { Type.Bet, "Sound/SFX/Bet" },
                { Type.ToPot, "Sound/SFX/ToPot" },
                { Type.Check, "Sound/SFX/Check" },
                { Type.ButtonHover, "Sound/SFX/ButtonHover" },
                { Type.ButtonClick, "Sound/SFX/ButtonClick" }

            };

            public enum Type
            {
                ShowingCards,
                Bet,
                ToPot,
                Check,
                ButtonHover,
                ButtonClick,
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
