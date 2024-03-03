using System.Collections.Generic;

public abstract class Constants
{
    public abstract class SceneNames
    {
        public const string Menu = "Menu";
        public const string Desk = "Desk";
        public const string Kickstart = "Kickstart";
    }

    public abstract class Prefabs
    {
        public const string MobileSuffix = "_Mobile";
        
        public abstract class UI
        {
            public const string Menu = "MenuUI";
            public const string Desk = "UI";
        }
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

        public abstract class Chips
        {
            public const string ChipsStackTemplate = "ChipsStack_";
            public const uint AssetsCount = 7;
            public const string Pot = "ChipsStack_7";
        }
    }

    public abstract class Sound
    {
        public static readonly string MixerPath = "Sound/Mixer";
        
        public abstract class Sfx
        {
            public const string Hover = "Sound/SFX/ButtonHover";
            public const string Click = "Sound/SFX/ButtonClick";
            
            public static readonly Dictionary<Type, string> Paths = new()
            {
                { Type.ShowingCards, "ShowingCards" },
                { Type.Bet, "Bet" },
                { Type.ToPot, "ToPot" },
                { Type.Check, "Check" },

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
            public static readonly List<string> Paths = new() {
                "BGMusic",
                "BGMusic2",
                "BGMusic3"
            };
        }
    }
    
    public abstract class LoadingUI
    {
        // Used for fake loading on LoadingUI.
        public const float FakeLoadStartValue = 0.3f;
    }
}
