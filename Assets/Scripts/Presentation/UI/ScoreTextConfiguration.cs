using UnityEngine;

namespace KnifeToss.Presentation.UI
{
    [CreateAssetMenu(fileName = "ScoreTextConfiguration", menuName = "Knife Toss/Configurations/UI/Score Text Configuration")]
    public sealed class ScoreTextConfiguration : ScriptableObject
    {
        [SerializeField] private ScoreTextEntry fail = new ScoreTextEntry("FAIL", new Color(0.82f, 0.19f, 0.19f));
        [SerializeField] private ScoreTextEntry oneStar = new ScoreTextEntry("GOOD", new Color(1f, 0.86f, 0.27f));
        [SerializeField] private ScoreTextEntry twoStar = new ScoreTextEntry("GREAT", new Color(1f, 0.65f, 0.3f));
        [SerializeField] private ScoreTextEntry threeStar = new ScoreTextEntry("PERFECT!", new Color(0.345f, 0.435f, 0.318f));

        /// <summary>
        /// Resolves the score text entry for the given star count.
        /// </summary>
        public ScoreTextEntry GetEntry(int stars)
        {
            return stars switch
            {
                <= 0 => fail,
                1 => oneStar,
                2 => twoStar,
                _ => threeStar
            };
        }
    }

    [System.Serializable]
    public struct ScoreTextEntry
    {
        [SerializeField] private string text;
        [SerializeField] private Color color;

        public ScoreTextEntry(string text, Color color)
        {
            this.text = text;
            this.color = color;
        }

        public string Text => text;
        public Color Color => color;
    }
}
