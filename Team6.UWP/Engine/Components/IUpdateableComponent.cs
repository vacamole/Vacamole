namespace Team6.Engine.Components
{
    public interface IUpdateableComponent
    {
        void Update(float elapsedSeconds, float totalSeconds);
    }
}