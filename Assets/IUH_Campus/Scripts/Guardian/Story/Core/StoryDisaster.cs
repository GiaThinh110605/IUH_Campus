namespace IUHCampus.Guardian.Story
{
    // Disaster = from the incident (campus turns to night) until daylight returns in the ending,
    // plus every running aftershock. Students and the mascot are only visible in peace time.
    public static class StoryDisaster
    {
        public static bool IsDisasterAtStart(StoryPhase phase)
        {
            return phase == StoryPhase.Incident || phase == StoryPhase.Chapter || phase == StoryPhase.Ending;
        }
    }
}
