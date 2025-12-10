namespace Game.Flow.Stage
{
    public class StageData
    {
        public int StageIndex { get; internal set; }
        public int CurrentLevelIndex { get; internal set; }

        public StageData(int stageIndex)
        {
            StageIndex = stageIndex;
            CurrentLevelIndex = 0;
        }
    }
}
