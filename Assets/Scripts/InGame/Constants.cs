using System.Collections.Generic;

public abstract class Constants
{
    public abstract class SceneNames
    {
        public const string Menu = "Menu";
        public const string Desk = "Desk_d";
        public const string Kickstart = "Kickstart";
    }

    public abstract class Prefabs
    {
        public const string GameTitleText = "SnaPGameTitleText";
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

        public abstract class Cards
        {
            public const string CardBack = "CardBack2";
        }
    }

    public abstract class Sound
    {
        public static readonly string MixerPath = "Sound/Mixer";
        
        public abstract class Sfx
        {
            public static readonly Dictionary<Type, string> Paths = new()
            {
                { Type.ShowingCards, "ShowingCards" },
                { Type.Bet, "Bet" },
                { Type.ToPot, "ToPot" },
                { Type.Check, "Check" },
                { Type.ButtonHover, "ButtonHover" },
                { Type.ButtonClick, "ButtonClick" }

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
            public static readonly List<string> Paths = new() {
                { "BGMusic"},
                { "BGMusic2"},
                { "BGMusic3"}
            };
        }
    }

    public abstract class ResourcesPaths
    {
        public const string Chips = "Sprites/Chips";
    }
}
