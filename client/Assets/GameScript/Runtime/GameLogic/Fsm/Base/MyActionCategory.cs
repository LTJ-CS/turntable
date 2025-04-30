namespace GameScript.Runtime.GameLogic.Fsm.Base
{
    /// <summary>
    /// 定义 FSM Action 的分类
    /// </summary>
    public static class MyActionCategory
    {
        private const string MyActions = nameof(MyActions) + "/";
        public const  string MainUI    = MyActions + nameof(MainUI);
        public const  string Helper    = MyActions + "Helper";
        public const  string LevelTest = MyActions + "LevelTest";
        public const  string Level     = MyActions + "Level";
        public const  string Bgm       = MyActions + "Bgm";
        public const  string Sketch       = MyActions + "Sketch";
    }
}