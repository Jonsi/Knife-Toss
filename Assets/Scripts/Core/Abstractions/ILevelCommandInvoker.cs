namespace KnifeToss.Core.Abstractions
{
    public interface ILevelCommandInvoker
    {
        void Retry();
        void Next();
    }
}
