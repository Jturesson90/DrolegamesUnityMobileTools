namespace Drolegames.SocialService
{
    using System;
    [Serializable]
    public struct DroleAchievement
    {
        public readonly double steps;
        public readonly double stepsToComplete;
        public readonly string id;
        public readonly bool hasIncrement;
        public DroleAchievement(string id)
        {
            this.id = id;
            steps = 100;
            stepsToComplete = 100;
            hasIncrement = false;
        }
        public DroleAchievement(string id, double steps, double stepsRatio)
        {
            this.id = id;
            this.steps = steps;
            this.stepsToComplete = stepsRatio;
            hasIncrement = true;
        }
    }
}
