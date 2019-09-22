namespace InSpaceNoOneSeesYourShadow.Engine.Interfaces
{
    public interface ICloneable<out T>
    {
        T Clone();
    }
}