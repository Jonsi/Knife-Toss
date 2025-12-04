using KnifeToss.Core.Domain;

namespace KnifeToss.Application.Level
{
    /// <summary>
    /// Evaluates level completion and failure conditions.
    /// </summary>
    public sealed class LevelStateEvaluator
    {
        public bool ShouldEvaluate(LevelSession session)
        {
            return session.HasExhaustedKnives() && !session.IsComplete && !session.IsFailed;
        }

        public void Evaluate(LevelSession session)
        {
            if (!ShouldEvaluate(session))
            {
                return;
            }

            if (session.HasMetObjectives)
            {
                session.MarkCompleteIfNeeded();
            }
            else
            {
                session.RecordFailure();
            }
        }
    }
}

