namespace SpaceBumper
{
    public interface IInputHandler
    {
        void Start(World world);

        void Update(int iteration, World world, Bumpership bumpership);

        void Dispose();
    }
}