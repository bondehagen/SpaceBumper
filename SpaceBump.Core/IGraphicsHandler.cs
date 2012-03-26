namespace SpaceBumper
{
    public interface IGraphicsHandler
    {
        object CreateShip();

        void RenderShip(Bumpership bumpership);

        void RenderMap(Map map);

        bool BeforeRender(float deltaTime);

        void AfterRender();
    }
}