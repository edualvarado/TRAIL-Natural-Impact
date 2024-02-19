namespace MxM
{

    public interface IMxMExtension
    {
        bool IsEnabled { get; }
        bool DoUpdatePhase1 { get; }
        bool DoUpdatePhase2 { get; }
        bool DoUpdatePost { get; }

        void Initialize();
        void UpdatePhase1();
        void UpdatePhase2();
        void UpdatePost();
        void Terminate();

    }//End of interface: IMxMExtension
}//End of namespace: MxM
